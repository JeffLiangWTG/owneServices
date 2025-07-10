using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	internal class CreditNoteUserControlTestCase : TestCaseWithFactory
	{
		public void TestInvoiceNumberLabel()
		{
			using (var claimChargesUserControl = new ClaimChargesUserControl())
			{
				claimChargesUserControl.SetDataBinding(Factory.NewWithValidTestData<APInvoice>(), null);
				claimChargesUserControl.CreateControl();
				AssertEquals("Credit Note Number", claimChargesUserControl.InvoiceUserControl.TransactionNumTextBoxCaption);
			}
		}

		public void TestAH_ComplianceSubType_Number_Sequence_Visiblity()
		{
			var currCompany = GlbCompany.CurrentCompany;

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			var regEnableComplDocModule = AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule;
			var currCompPK = currCompany.PK.ToGuid();
			foreach (InvoicingBase invoice in new InvoicingBase[] { arInvoice, apInvoice })
			{
				foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
				{
					bool expectVisible = country != CountryCodes.China && country != CountryCodes.Afghanistan && invoice == apInvoice;
					using (currCompany.TemporarilySetCountry(country))
					{
						using (regEnableComplDocModule.SetTemporaryValue(currCompPK, Guid.Empty, Guid.Empty, false))
						using (var claimChargesUserControl = new ClaimChargesUserControl())
						{
							claimChargesUserControl.SetDataBinding(invoice, null);
							claimChargesUserControl.CreateControl();
							claimChargesUserControl.Show();
							var details = claimChargesUserControl.InvoiceUserControlForTest;

							var subType = details.AH_ComplianceSubTypeDropEdit;
							AssertEquals("AH_ComplianceSubTypeDropEdit.Visible", expectVisible, subType.Visible);
							if (expectVisible)
							{
								Assert("NOT AH_ComplianceSubTypeDropEdit.ReadOnly", !subType.ReadOnly);
							}
							AssertEquals("AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() visible", expectVisible, subType.GetExtension<LabelCaptionRenderer>().Visible);

							var number = details.AH_TransactionReferenceTextBox;
							AssertEquals("AH_TransactionReferenceTextBox.Visible", expectVisible, number.Visible);
							if (expectVisible)
							{
								Assert("AH_TransactionReferenceTextBox.ReadOnly", number.ReadOnly);
							}

							var sequence = details.ComplianceSequenceTextBox;
							AssertEquals("ComplianceSequenceTextBox.Visible", expectVisible, sequence.Visible);
							if (expectVisible)
							{
								Assert("ComplianceSequenceTextBox.ReadOnly", sequence.ReadOnly);
							}
							var complSeq = invoice.ComplianceSequence;
							AssertEquals("ComplianceSequenceTextBox value", expectVisible ? (complSeq?.XD_Code + " - " + complSeq?.XD_Description) : "", sequence.Text);
						}

						using (regEnableComplDocModule.SetTemporaryValue(currCompPK, Guid.Empty, Guid.Empty, true))
						using (var claimChargesUserControl = new ClaimChargesUserControl())
						{
							claimChargesUserControl.SetDataBinding(invoice, null);
							claimChargesUserControl.CreateControl();
							claimChargesUserControl.Show();
							var details = claimChargesUserControl.InvoiceUserControlForTest;

							var subType = details.AH_ComplianceSubTypeDropEdit;
							Assert("NOT AH_ComplianceSubTypeDropEdit.Visible", !subType.Visible);
							Assert("NOT AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() visible", !subType.GetExtension<LabelCaptionRenderer>().Visible);

							Assert("NOT AH_TransactionReferenceTextBox.Visible", !details.AH_TransactionReferenceTextBox.Visible);
							Assert("NOT ComplianceSequenceTextBox.Visible", !details.ComplianceSequenceTextBox.Visible);
						}
					}
				}
			}
		}

		public void TestSourceReferenceFieldVisibility()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			AssertResult(arInvoice, false);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				AssertResult(arInvoice, true);
			}

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			AssertResult(apInvoice, false);

			void AssertResult(Invoice invoice, bool expectVisible)
			{
				using (var control = new ClaimChargesUserControl())
				{
					control.SetDataBinding(invoice, null);
					control.CreateControl();
					control.Show();

					AssertEquals("Source Ref field is visible only for AR INV when country is Portugal",
						expectVisible, control.InvoiceUserControlForTest.SourceReferenceTextBox.Visible);
				}
			}
		}

		public void TestCaptions()
		{
			using (var claimChargesUserControl = new ClaimChargesUserControl())
			{
				AssertEquals("Tax Amount", claimChargesUserControl.AH_OSTaxAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Invoice Amount", claimChargesUserControl.AH_OSTotalAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("QST Amount", claimChargesUserControl.AH_OSExtraTaxAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestWHTIsNotShownOnForm()
		{
			using (var form = new ZForm())
			{
				var claimChargesUserControl = new ClaimChargesUserControl();
				form.Controls.Add(claimChargesUserControl);
				claimChargesUserControl.SetDataBinding(Factory.NewWithValidTestData<APInvoice>(), null);
				form.Show();

				var transLinesCols = claimChargesUserControl.InvoiceUserControl.TransactionLinesGrid.ColumnStyles;
				for (int i = transLinesCols.Count - 1; i >= 0; i--)
				{
					ZGridColumnInfo columnStyle = (ZGridColumnInfo)transLinesCols[i];
					if (columnStyle.ColumnName == InvoicingLineBase.Schema.AL_AW || columnStyle.ColumnName == InvoicingLineBase.Schema.AL_OSWHTAmount || columnStyle.ColumnName == InvoicingLineBase.Schema.AL_LocalWHTAmount)
					{
						Assert(columnStyle.IsUnavailable);
					}
				}
			}
		}

		public void TestExtraTaxIsShownOnForm()
		{
			AssertExtraTaxColumnsVisible("AU", false);
			AssertExtraTaxColumnsVisible("IN", true);
			AssertExtraTaxColumnsVisible("CA", true);
			AssertExtraTaxColumnsVisible("MX", true);
		}

		void AssertExtraTaxColumnsVisible(String country, bool shouldBeVisible)
		{
			GlbCompany.CurrentCompany.SetCountry(country);
			using (var form = new ZForm())
			{
				var claimChargesUserControl = new ClaimChargesUserControl();
				form.Controls.Add(claimChargesUserControl);
				claimChargesUserControl.SetDataBinding(Factory.NewWithValidTestData<APInvoice>(), null);
				form.Show();

				AssertEquals(claimChargesUserControl.ExtraTaxGroupBox.Visible, shouldBeVisible);
				var transLinesCols = claimChargesUserControl.InvoiceUserControl.TransactionLinesGrid.ColumnStyles;
				for (int i = transLinesCols.Count - 1; i >= 0; i--)
				{
					ZGridColumnInfo columnStyle = (ZGridColumnInfo)transLinesCols[i];
					if (columnStyle.ColumnName == InvoicingLineBase.Schema.AL_LocalExtraTaxAmount || columnStyle.ColumnName == InvoicingLineBase.Schema.AL_OSExtraTaxAmount)
					{
						AssertEquals(!shouldBeVisible, columnStyle.IsUnavailable);
					}
				}
			}
		}

		public void TestLocalTotalsAreDisplayedBasedOnTransactionCurrency()
		{
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("AU", "USD", true, true, false);
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("AU", "AUD", false, false, false);
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("CA", "AUD", true, true, true);
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("CA", "CAD", false, false, false);
		}

		public void TestReceivablesClaimsAndQueriesClaimChargesLineChgsGrd()
		{
			var checkpoint = Env.Security.ReceivablesClaimsAndQueriesClaimChargesLineChgsGrd;

			var originalValue = checkpoint.IsAllowed;
			try
			{
				var sisterCompany = TestObjectCreator.CreateNewCompany("AAA").PK;
				var currentCompany = GlbCompany.CurrentCompany.PK;

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				AssertLineChargesGridVisibility(checkpoint, true, invoice, currentCompany, true);
				AssertLineChargesGridVisibility(checkpoint, false, invoice, currentCompany, true);

				AssertLineChargesGridVisibility(checkpoint, true, invoice, sisterCompany, true);
				AssertLineChargesGridVisibility(checkpoint, false, invoice, sisterCompany, false);
			}
			finally
			{
				checkpoint.IsAllowed = originalValue;
			}
		}

		public void TestPayablesClaimsAndQueriesClaimChargesLineChargesGrd()
		{
			var checkpoint = Env.Security.PayablesClaimsAndQueriesClaimChargesLineChargesGrd;

			var originalValue = checkpoint.IsAllowed;
			try
			{
				var sisterCompany = TestObjectCreator.CreateNewCompany("AAA").PK;
				var currentCompany = GlbCompany.CurrentCompany.PK;

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();

				AssertLineChargesGridVisibility(checkpoint, true, invoice, currentCompany, true);
				AssertLineChargesGridVisibility(checkpoint, false, invoice, currentCompany, true);

				AssertLineChargesGridVisibility(checkpoint, true, invoice, sisterCompany, true);
				AssertLineChargesGridVisibility(checkpoint, false, invoice, sisterCompany, false);
			}
			finally
			{
				checkpoint.IsAllowed = originalValue;
			}
		}

		void AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency(string country, string currency, bool tax, bool total, bool extraTax)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			using (var claimChargesUserControl = new ClaimChargesUserControl())
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_GC = GlbCompany.CurrentCompany.PK;
				invoice.AH_RX_NKTransactionCurrency = currency;
				claimChargesUserControl.SetDataBinding(invoice, null);
				AssertEquals(tax, claimChargesUserControl.AH_LocalTaxAmountCalcEdit.Visible);
				AssertEquals(total, claimChargesUserControl.AH_LocalTotalAmountCalcEdit.Visible);
				AssertEquals(extraTax, claimChargesUserControl.AH_LocalExtraTaxAmountCalcEdit.Visible);
			}
		}

		void AssertLineChargesGridVisibility(SecurityCheckpoint securityCheckpoint, bool isAllowed, InvoicingBase invoice, ZGuid invoiceCompanyPK, bool lineChargesGridvisibilityExpected)
		{
			securityCheckpoint.IsAllowed = isAllowed;
			invoice.AH_GC = invoiceCompanyPK;

			using (var form = new ZForm())
			using (var claimChargesUserControl = new ClaimChargesUserControl())
			{
				claimChargesUserControl.SetDataBinding(invoice, null);

				form.Controls.Add(claimChargesUserControl);
				form.Show();
				Application.DoEvents();

				var lineChargesTabPage = claimChargesUserControl.FindSingleOrDefault<ZTabPage>("LineChargesTabPage");
				var restrictedLineChargesLabel = claimChargesUserControl.FindSingleOrDefault<ZLabel>("RestrictedLineChargesLabel");
				AssertNotNull("Pre-condition:", lineChargesTabPage);
				AssertNotNull("Pre-condition:", restrictedLineChargesLabel);

				Assert("LineChargesGrid.Visible should be : " + lineChargesGridvisibilityExpected.ToString(), claimChargesUserControl.LineChargesGrid.Visible == lineChargesGridvisibilityExpected);
				Assert("RestrictedLineChargesLabel.Visible should be : " + (!lineChargesGridvisibilityExpected).ToString(), restrictedLineChargesLabel.Visible == !lineChargesGridvisibilityExpected);

				if (!lineChargesGridvisibilityExpected)
				{
					Assert("RestrictedLineChargesLabel.Text should be : " + securityCheckpoint.ErrorMessageForNotAllowed, restrictedLineChargesLabel.Text == securityCheckpoint.ErrorMessageForNotAllowed);
				}
			}
		}

		#region implementation

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
