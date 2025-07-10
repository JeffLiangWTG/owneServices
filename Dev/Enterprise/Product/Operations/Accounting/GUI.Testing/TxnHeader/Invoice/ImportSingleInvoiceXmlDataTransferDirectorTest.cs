using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Invoices.Testing
{
	public class ImportSingleInvoiceXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestFileBeingUsedbyAnotherProcess()
		{
			string fileName = Env.TempPath + "test.xml";

			try
			{
				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("-------------------");
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (Stream fs = File.OpenWrite(fileName))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					Director.PromptUserAndImport(BillingInterfaceName.Test);
				}
				AssertEquals("The process cannot access the file '" + fileName + "' because it is being used by another process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPromptUserAndImport_Context()
		{
			var filePath = BaseSourcePath
				+ @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ValidInvoice.xml";

			ZFormModaliser.FileNameToSelectInShowCommonDialog = filePath;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Director.PromptUserAndImport(BillingInterfaceName.Test);
			Assert("Should have context \"AllowReopenJobWhenImporting\"", Director.ImportedInvoice.Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidInvoiceXML()
		{
			using (StreamReader stream = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvalidInvoice.xml"))
			{
				ImportSingleInvoiceXmlDataTransferDirectorTestClass director = new ImportSingleInvoiceXmlDataTransferDirectorTestClass(false);
				director.Import(stream.BaseStream, Factory, Notifications);
				AssertEquals("Error: Unexpected end of file has occurred. The following elements are not closed: OrganisationDetails, DebtorOrCreditor, FinancialInvoice. Line 6, position 52." + System.Environment.NewLine, Notifications.AsString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ValidTxnHeaderType()
		{
			using (StreamReader stream = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ValidInvoice.xml"))
			{
				Director.Import(stream.BaseStream, Factory, Notifications);
				InvoicingBase invoice = Director.ImportedInvoice;

				AssertNotNull("Invoice Should not be null", invoice);
				AssertEquals("InvocingBase Concrete Type", nameof(ARInvoice), invoice.GetType().Name);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportOfInvalidTxnHeaderType()
		{
			using (StreamReader stream = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvalidInvoiceType.xml"))
			{
				Director.Import(stream.BaseStream, Factory, Notifications);
				InvoicingBase invoice = Director.ImportedInvoice;
				AssertNull("Invoice Should be null", invoice);
				AssertEquals("Error: This transaction cannot be imported as the Ledger or Transaction Type is invalid. Ledger: AR, Transaction Type: TRF. Valid Ledgers are [AP, AR] and Valid Transaction Types are [ADJ, CRD, INV]." + System.Environment.NewLine, Notifications.AsString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvoiceWithConsolCostInDifferentCurrency()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol("NLAMS", "AUSYD", "C00001");
			var shipment = objectCreator.CreateShipment("S00001", consol);
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, 6588m, null, "SHP");
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 1.0834;

			OrgHeader aBIGAS = objectCreator.ABIGAS;
			aBIGAS.OH_IsCreditor = true;

			var branch = objectCreator.CreateBranch("AMS", GlbCompany.CurrentCompany);
			Factory.Save();

			using (StreamReader stream = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\LegacyXmlImportWithConsolCostInDifferentCurrency.xml"))
			{
				Director.Import(stream.BaseStream, Factory, Notifications);
				InvoicingBase invoice = Director.ImportedInvoice;
				AssertNotNull("Invoice should not be null", invoice);
				AssertEquals("Invoice should have 1 consol cost", 1, invoice.ConsolCosting.ConsolCosts.Count);
				AssertNotEquals("The import cost should not have existing consol's currency.", "USD", invoice.ConsolCosting.ConsolCosts[0].E6_RX_NKCurrency);
				AssertEquals("The import cost should have original currency.", invoice.ConsolCosting.ConsolCosts[0].LocalCurrency, invoice.ConsolCosting.ConsolCosts[0].E6_RX_NKCurrency);

				AssertEquals("The currency of invoice line should be local currency", GlbCompany.CurrentCompany.LocalCurrency.Code, invoice.Lines[0].AL_RX_NKTransactionCurrency);
				AssertNotEquals("The invoice line should not have existing consol's currency.", "USD", invoice.Lines[0].AL_RX_NKTransactionCurrency);
			}
		}

		public void TestExtraValidation()
		{
			Assert(!((FinancialInvoiceDataAdapter)Director.Adapter_Exposed).RunExtraValidation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvoiceWithInvalidOperationException()
		{
			using (StreamReader stream = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvalidXmlForInvoice.xml"))
			{
				Director.Import(stream.BaseStream, Factory, Notifications);
				AssertEquals("Error: Token EndElement in state Start would result in an invalid XML document." + System.Environment.NewLine, Notifications.AsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Director = new ImportSingleInvoiceXmlDataTransferDirectorTestClass(true);
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		class ImportSingleInvoiceXmlDataTransferDirectorTestClass : ImportSingleInvoiceXmlDataTransferDirector
		{
			public ImportSingleInvoiceXmlDataTransferDirectorTestClass(bool hasLicence)
				: base(hasLicence, ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
			}

			public new void Import(Stream xmlFileStream, BusinessObjectFactory factory, INotifications notifications)
			{
				base.Import(xmlFileStream, factory, notifications);
			}

			public IValueObjectDataAdapter Adapter_Exposed
			{
				get
				{
					return base.Adapter;
				}
			}
		}

		ImportSingleInvoiceXmlDataTransferDirectorTestClass Director;
	}
}
