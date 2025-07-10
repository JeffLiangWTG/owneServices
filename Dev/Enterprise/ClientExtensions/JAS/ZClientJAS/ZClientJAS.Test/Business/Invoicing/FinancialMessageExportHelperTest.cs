using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Invoicing.Testing
{
	internal class FinancialMessageExportHelperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_NullInvoice()
		{
			new FinancialMessageExportHelper(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_NullInvoicingBase()
		{
			JASInvoicingBaseForTest invoice = Factory.New<JASInvoicingBaseForTest>();
			AssertNull("Sanity check", invoice.InvoicingBase);
			new FinancialMessageExportHelper(invoice);
		}

		public void TestOnSaved_SaveNotSucceeded()
		{
			var adjustmentNote = SetupDetailsForAdjustmentNote();
			var helper = new FinancialMessageExportHelper(adjustmentNote);
			helper.OnSaved(false);
			AssertNoExportFilesInExportDir("Save not successful.");
		}

		public void TestOnSaved_InvoiceHasErrorsAndJXCWarnings()
		{
			ValidationHelper validationHelper = new ValidationHelper();
			var adjustmentNote = Factory.NewWithValidTestData<JASARAdjustmentNote>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			Assert("Sanity check", validationHelper.HasJXCWarnings(adjustmentNote));
			AssertNoExportFilesInExportDir("There are jxc warnings.");
		}

		public void TestOnSaved()
		{
			SetupDetailsForAdjustmentNote();
			Factory.Save();
			string[] files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			AssertEquals("There should be one ACDT message exported", 1, files.Length);
			AssertEquals("00001000_HB101.txt", Path.GetFileName(files[0]));
			using (StreamReader reader = File.OpenText(files[0]))
			{
				object readHeader = reader.ReadLine();
				Assert("should be exported as ACDT message", reader.ReadLine().StartsWith("ACDT"));
			}
		}

		[TestDate(2013, 4, 1)]
		public void TestReloadRequired()
		{
			var creditNote = SetupDetailsForConsolCreditNoteButAsARInvoice();
			var exportHelper = new FinancialMessageExportHelperForTest(creditNote);
			AssertEquals("Precondition: the credit note is instantiated as ARInvoice", creditNote.GetType(), typeof(JASARInvoice));
			creditNote.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			Factory.Save();
			string[] files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			AssertEquals("There should be one ACDT message exported", 1, files.Length);
			AssertEquals("00001000.txt", Path.GetFileName(files[0]));
			using (StreamReader reader = File.OpenText(files[0]))
			{
				object readHeader = reader.ReadLine();
				var invoiceHeaderLine = reader.ReadLine();
				Assert("should be exported as NCDT message", invoiceHeaderLine.StartsWith("NCDT"));
				Assert("should be exported with postiive value as the transaction is a credit note", invoiceHeaderLine.EndsWith(";AUD;400;ITMIL;ITVAL"));
				var invoiceLine = reader.ReadLine();
				Assert("should be exported as INVD message", invoiceLine.StartsWith("INVD"));
				Assert("should be exported with postiive value as the transaction is a credit note", invoiceLine.EndsWith(";400;GST;;;"));
			}
		}

		public void TestOnSaved_AutoJXCMessagingDisabled()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = false;
			SetupDetailsForAdjustmentNote();
			Factory.Save();
			AssertNoExportFilesInExportDir("Auto JXC messaging is disabled");
		}

		public void TestOnSaved_ShouldOnlyExportOnceWhenTheTransactionIsFirstPosted()
		{
			var adjustmentNtoe = SetupDetailsForAdjustmentNote();
			Factory.Save();
			string[] files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			AssertEquals("There should be one ACDT message exported", 1, files.Length);
			AssertEquals("00001000_HB101.txt", Path.GetFileName(files[0]));
			File.Delete(files[0]);
			files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			AssertEquals("Should be empty now", 0, files.Length);
			adjustmentNtoe.HasChanges = true;
			Factory.Save();
			files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			AssertEquals("Should only export the message once, when the adjustment note is first saved", 0, files.Length);
		}

		#region Implementation
		JASARAdjustmentNote SetupDetailsForAdjustmentNote()
		{
			SetupOrgProxy();
			var shipment = SetupShipmentWithConsol();
			var adjustmentNote = Factory.New<JASARAdjustmentNote>();
			adjustmentNote.AH_JH = Factory.NewJobWithValidTestDataForTesting<JobHeader>().PK;
			adjustmentNote.Job.JH_ParentID = shipment.PK;
			adjustmentNote.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			adjustmentNote.AH_GB = GlbBranch.CurrentBranch.PK;
			adjustmentNote.AH_OH = shipment.Consignee.PK;
			adjustmentNote.AH_OSTotal = -230m;
			return adjustmentNote;
		}

		JASARInvoice SetupDetailsForConsolCreditNoteButAsARInvoice()
		{
			SetupOrgProxy();
			var shipment = SetupShipmentWithConsol();
			var result = Factory.New<JASARInvoice>();
			var invoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			invoiceLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.AL_AC = Env.Registry.FreightChargeCode;
			invoiceLine.AL_AH = result.PK;
			invoiceLine.AL_Desc = "Profit Share / Rebate";
			invoiceLine.AL_LineAmount = -400m;
			invoiceLine.AL_OSAmount = -400m;
			invoiceLine.AL_LocalExTaxAmount = -400m;
			invoiceLine.AL_OH = shipment.Consols[0].ReceivingForwarderPK;
			invoiceLine.AL_ExchangeRate = 1;
			invoiceLine.AL_LocalGSTAmount = 0;
			invoiceLine.AL_GB = Env.CurrentBranch.PK;
			result.Lines.Add(invoiceLine);
			result.AH_TransactionNum = "10101010";
			result.AH_ConsolidatedInvoiceRef = "XXXXX";
			result.AH_InvoiceDate = new ZDateTime(2013, 4, 1);
			result.AH_OH = shipment.Consols[0].ReceivingForwarderPK;
			result.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			result.AH_GB = Env.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_OSTotal = -400m;
			result.AH_OutstandingAmount = -400m;
			result.AH_ExchangeRate = 1;
			result.AH_InvoiceAmount = -400m;
			result.AH_FullyPaidDate = ZDateTime.Empty;
			result.AH_GSTAmount = 0;
			return result;
		}

		void AssertNoExportFilesInExportDir(string errorMessage)
		{
			AssertEquals(errorMessage + " There should be no files exported", 0, Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName).Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			string testDir = Path.Combine(Env.TempPath, "FinancialMessageExportHelperTest");
			Directory.CreateDirectory(testDir);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = testDir;
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			base.TearDown();
		}

		void SetupOrgProxy()
		{
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_RL_NKHomePort = "AUSYD";
			JASOrgHeader orgProxy = orgProxy = Factory.NewWithValidTestData<JASOrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch.GB_OH_OrgProxy = orgProxy.PK;
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalNettingCode, "AUCOR", Env.CurrentCompany.Country.Code);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "AUSYD", Env.CurrentCompany.Country.Code);
			Factory.Save();
		}

		JASForwardingShipment SetupShipmentWithConsol()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			JASOrgHeader consignee = Factory.NewWithValidTestData<JASOrgHeader>();
			JASOrgHeader receivingAgent = Factory.NewWithValidTestData<JASOrgHeader>();
			OrgAddress receivingAgentAddr = receivingAgent.Addresses.AddNew(OrgAddressType.Office, true);
			consignee.NettingCode = "ITMIL";
			consignee.OfficeCode = "ITVAL";
			receivingAgent.NettingCode = "ITMIL";
			receivingAgent.OfficeCode = "ITVAL";
			Factory.Save();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HB101";
			shipment.ConsigneePK = consignee.PK;
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ITMIL";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgentAddr.PK;
			consol.JK_MasterBillNum = "MAWB01";
			return shipment;
		}
		#endregion
	}
}
