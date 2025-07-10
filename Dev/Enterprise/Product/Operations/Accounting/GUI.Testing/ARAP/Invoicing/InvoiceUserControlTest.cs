using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.TaxFramework.GUI;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public class InvoiceUserControlTest : TestCaseWithFactory
	{
		public void TestControlsAllowedToRemainEditableAfterOtherTaxesCalculated()
		{
			using (var control = new InvoiceUserControl())
			{
				var ahDescTextBox = control.GetControl<ZTextBox>("AH_DescTextbox");
				var otherTaxesSummaryTabPage = control.GetControl<ZTabPage>("TaxTransactionSummaryTabPage");
				var validInvoiceTotalCalcEdit = control.GetControl<ZCalcEdit>("ValidInvoiceTotalCalcEdit");
				AssertContainsExactElementsInAnyOrder(new[] { ahDescTextBox.Name, otherTaxesSummaryTabPage.Name, validInvoiceTotalCalcEdit.Name }, control.ControlsAllowedToRemainEditableAfterOtherTaxesCalculated);
			}
		}

		public void TestRelatedJobNumberColumnIsAvailableOnlyForAPInvoiceAndAPCreditNote()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			var unapprovedInvoice = Factory.NewWithValidTestData<UAInvoice>();
			var unapprovedCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			Factory.Save();
			var incompleteAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			incompleteAPInvoice.SaveAsIncomplete();
			var incompleteAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			incompleteAPCreditNote.SaveAsIncomplete();

			AssertRelatedjobNumberColumnAvailability(apInvoice);
			AssertRelatedjobNumberColumnAvailability(aPCreditNote);
			AssertRelatedjobNumberColumnAvailability(unapprovedInvoice);
			AssertRelatedjobNumberColumnAvailability(unapprovedCreditNote);
			AssertRelatedjobNumberColumnAvailability(arInvoice);
			AssertRelatedjobNumberColumnAvailability(arCreditNote);
			AssertRelatedjobNumberColumnAvailability(incompleteAPInvoice);
			AssertRelatedjobNumberColumnAvailability(incompleteAPCreditNote);

			void AssertRelatedjobNumberColumnAvailability(InvoicingBase transaction)
			{
				using (var form = new BaseInvoicingForm(transaction))
				{
					form.Show();
					Application.DoEvents();
					AssertEquals(!transaction.IsAPInvoiceOrCreditNote, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("AL_Calc_RelatedJobNumber").IsUnavailable);
				}
			}
		}

		public void TestRelatedJobNumberColumn()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = creator.CreateShipment("S00011", gatewayConsol);
			Factory.Save();

			var gatewayJob = creator.CreateJob(gatewayConsol, createWithMutex: false);
			var gatewayCharge = creator.CreateCharge(gatewayJob, creator.CC1, 10m, 10m);
			gatewayCharge.JR_Calc_RelatedJobNumber = shipment.JS_UniqueConsignRef;
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.OriginalJobCharge = gatewayCharge;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				var relatedJobNumberColumn = form.InvoiceDetails.TransactionLinesGrid.Columns.FirstOrDefault(c => c.ColumnName == "AL_Calc_RelatedJobNumber");
				AssertNotNull("Related Job Number column should exist.", relatedJobNumberColumn);
				Assert("Related Job Number is not available by default.", !relatedJobNumberColumn.IsVisible);
				Assert("Related Job Number is read only.", relatedJobNumberColumn.ColumnStyle.ReadOnly);

				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				AssertEquals(shipment.JS_UniqueConsignRef, ((InvoicingLineBase)form.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()).AL_Calc_RelatedJobNumber);
			}
		}

		#region multi period apportionment columns visibility

		public void TestPeriodApportionmentMethodColumnsVisibility()
		{
			AssertMultiPeriodColumnsVisibility("PeriodApportionmentMethod");
		}

		public void TestPeriodClearingGLAccountPKColumnsVisibility()
		{
			AssertMultiPeriodColumnsVisibility("PeriodClearingGLAccountPK");
		}

		public void TestPeriodStartDateColumnsVisibility()
		{
			AssertMultiPeriodColumnsVisibility("PeriodStartDate");
		}

		public void TestPeriodEndDateColumnsVisibility()
		{
			AssertMultiPeriodColumnsVisibility("PeriodEndDate");
		}

		void AssertMultiPeriodColumnsVisibility(string columnName)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, 100);
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				Assert(linesGrid.Columns.Contains(columnName));
			}
			Factory.Save();

			var invoiceReloaded = Factory.Load<APInvoice>(invoice.PK);
			using (var form = new InvoiceForm(invoiceReloaded))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				Assert(!linesGrid.Columns.Contains(columnName));
			}
		}

		#endregion

		#region XML Columns Visibility

		public void TestJobConsolXMLDataColumnsVisibility()
		{
			AssertXMLColumnsVisibility("JobConsolXMLData");
		}

		public void TestImportedChargeCodeColumnsVisibility()
		{
			AssertXMLColumnsVisibility("ImportedChargeCode");
		}

		public void TestImportedChargeCodeXmlCodeColumnsVisibility()
		{
			AssertXMLColumnsVisibility("ImportedChargeCodeXmlCode");
		}

		void AssertXMLColumnsVisibility(string columnName)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100);
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(false, linesGrid.Columns.Contains(columnName));
			}

			line.IndexOfImportedUniversalTransactionLine = 0;
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(true, linesGrid.Columns.Contains(columnName));
				AssertEquals("Not visible by default", false, linesGrid.GetColumnStyle(columnName).IsVisible);
			}

			invoice.SaveAsIncomplete();
			invoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			invoice.RestoreSavedData();
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(true, linesGrid.Columns.Contains(columnName));
				AssertEquals("Not visible by default", false, linesGrid.GetColumnStyle(columnName).IsVisible);
			}

			invoice.MoveFromIncompleteToPayableLedger();
			invoice.Factory.Save();

			invoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(false, linesGrid.Columns.Contains(columnName));
			}
		}

		#endregion

		public void TestAmountDecimalPlaces()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)aPInv.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();

				RefCurrency currency0DP = Factory.NewWithValidTestData<RefCurrency>();
				currency0DP.RX_SubUnitRatio = 0;
				RefCurrency currency2DP = Factory.NewWithValidTestData<RefCurrency>();
				currency2DP.RX_SubUnitRatio = 100;
				Factory.Save();

				ZCalcEditColumnStyleInfo oSAmountColumn = (ZCalcEditColumnStyleInfo)form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(TransactionLine.Schema.AL_OSExTaxAmount);
				aPInv.AH_RX_NKTransactionCurrency = currency2DP.RX_Code;
				AssertEquals("Decimals should be 2", 2, line[oSAmountColumn.BindToDecimalPlaces]);
				aPInv.AH_RX_NKTransactionCurrency = currency0DP.RX_Code;
				AssertEquals("Decimals should be 0", 0, line[oSAmountColumn.BindToDecimalPlaces]);

				IDataGridLayoutIdentifierRoot idRoot = form;
				AssertEquals(form.Name + aPInv.GetType().Name, idRoot.ID);
			}
		}

		public void TestGovtChargeCodeColumnsAreInAvailableColumnList()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
					APInvoiceLine line = (APInvoiceLine)aPInv.Lines.AddNew();
					using (InvoiceForm form = new InvoiceForm(aPInv))
					{
						form.Show();
						AssertEquals(!enableGovtChargeCode, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_GovtChargeCode).IsUnavailable);
					}
				}
			}
		}

		public void TestIsFinalChargeSet()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = TestObjectCreator.ABIGAS.PK;
			aPInv.AH_TransactionNum = "I001";
			APInvoiceLine line = (APInvoiceLine)aPInv.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.AttachJobToAPLine(line);
			line.AL_OSAmount = 100M;
			line.AL_LineAmount = 100M;
			line.AL_IsFinalCharge = true;
			TestObjectCreator.AttachChargeToAPLine(line);
			Factory.Save();
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				Assert(line.AL_IsFinalCharge);
			}
		}

		public void TestOriginalReferenceDatesEditIsOnlyVisibleForARCreditNoteInPortugal()
		{
			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();

			using (var form = new BaseInvoicingForm(aRCreditNote))
			{
				form.Show();
				Application.DoEvents();
				Assert("Should not be visible", !form.InvoiceDetails.AH_OriginalReferenceEndDateEdit.Visible);
				Assert("Should not be visible", !form.InvoiceDetails.AH_OriginalReferenceStartDateEdit.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				using (var form = new BaseInvoicingForm(aRCreditNote))
				{
					form.Show();
					Application.DoEvents();
					Assert("Should be visible", form.InvoiceDetails.AH_OriginalReferenceEndDateEdit.Visible);
					Assert("Should be visible", form.InvoiceDetails.AH_OriginalReferenceStartDateEdit.Visible);
				}

				using (var form = new BaseInvoicingForm(aPCreditNote))
				{
					form.Show();
					Application.DoEvents();
					Assert("Should not be visible", !form.InvoiceDetails.AH_OriginalReferenceEndDateEdit.Visible);
					Assert("Should not be visible", !form.InvoiceDetails.AH_OriginalReferenceStartDateEdit.Visible);
				}
			}
		}

		public void TestOriginalInvoiceReferenceFieldsVisibility()
		{
			foreach (bool areOriginalInvoiceReferenceFieldsVisible in new bool[] { true, false })
			{
				var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceFields(It.IsAny<string>(), It.IsAny<string>())).Returns(areOriginalInvoiceReferenceFieldsVisible);
				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
				countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					var transaction = Factory.NewWithValidTestData<ARCreditNote>();
					using (var form = new BaseInvoicingForm(transaction))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(areOriginalInvoiceReferenceFieldsVisible, form.InvoiceDetails.AH_OriginalInvoiceDateEdit.Visible);
						AssertEquals(areOriginalInvoiceReferenceFieldsVisible, form.InvoiceDetails.AH_OriginalTransactionNumTextBox.Visible);
						AssertEquals(areOriginalInvoiceReferenceFieldsVisible, form.InvoiceDetails.TransactionGuidFindBox.Visible);
					}
				}
			}
		}

		public void TestReasonFieldsVisibility()
		{
			foreach (bool areReasonFieldsVisible in new bool[] { true, false })
			{
				var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceFields(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceReasonFields(It.IsAny<string>(), It.IsAny<string>())).Returns(areReasonFieldsVisible);
				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
				countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					var transaction = Factory.NewWithValidTestData<ARCreditNote>();
					using (var form = new BaseInvoicingForm(transaction))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(areReasonFieldsVisible, form.InvoiceDetails.ReasonCodeDropEdit.Visible);
						AssertEquals(areReasonFieldsVisible, form.InvoiceDetails.ReasonDescriptionTextBox.Visible);
					}
				}
			}
		}

		public void TestOriginalInvoiceReferenceDatesFieldsVisibility()
		{
			foreach (bool areOriginalInvoiceReferenceDatesFieldsVisible in new bool[] { true, false })
			{
				var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceFields(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceDatesFields(It.IsAny<string>(), It.IsAny<string>())).Returns(areOriginalInvoiceReferenceDatesFieldsVisible);
				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
				countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					var transaction = Factory.NewWithValidTestData<ARCreditNote>();
					using (var form = new BaseInvoicingForm(transaction))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(areOriginalInvoiceReferenceDatesFieldsVisible, form.InvoiceDetails.AH_OriginalReferenceStartDateEdit.Visible);
						AssertEquals(areOriginalInvoiceReferenceDatesFieldsVisible, form.InvoiceDetails.AH_OriginalReferenceEndDateEdit.Visible);
					}
				}
			}
		}

		public void TestOriginalInvoiceReferenceFieldsReadOnly()
		{
			foreach (bool areOriginalInvoiceReferenceFieldsReadOnly in new bool[] { true, false })
			{
				var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceFields(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceDatesFields(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
				originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceReasonFields(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
				originalInvoiceReferenceMock.Setup(x => x.GetAreAllOriginalInvoiceReferenceFieldsEnabled(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(!areOriginalInvoiceReferenceFieldsReadOnly);
				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
				countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					var transaction = Factory.NewWithValidTestData<ARCreditNote>();
					using (var form = new BaseInvoicingForm(transaction))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(areOriginalInvoiceReferenceFieldsReadOnly, form.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
						AssertEquals(areOriginalInvoiceReferenceFieldsReadOnly, form.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
						AssertEquals(areOriginalInvoiceReferenceFieldsReadOnly, form.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);
						AssertEquals(areOriginalInvoiceReferenceFieldsReadOnly, form.InvoiceDetails.AH_OriginalReferenceStartDateEdit.ReadOnly);
						AssertEquals(areOriginalInvoiceReferenceFieldsReadOnly, form.InvoiceDetails.AH_OriginalReferenceEndDateEdit.ReadOnly);
						//Reason code and Description ReadOnly is already tested in TestReasonFields_ReadOnly()
					}
				}
			}
		}

		public void TestAH_ConsolidatedInvoiceRefTextBoxIsVisibleOnlyForAP()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("Ledger Type", LedgerTypes.AccountsPayable, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				Assert("Should be visible", form.InvoiceDetails.AH_ConsolidatedInvoiceRefTextBox.Visible);
			}

			aPInv.SaveAsIncomplete();
			AssertEquals("Ledger Type", LedgerTypes.IncompleteTransactions, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				Assert("Should be visible", form.InvoiceDetails.AH_ConsolidatedInvoiceRefTextBox.Visible);
			}

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			AssertEquals("Ledger TYpoe", LedgerTypes.AccountsReceivable, aRInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aRInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be visible", false, form.InvoiceDetails.AH_ConsolidatedInvoiceRefTextBox.Visible);
			}
		}

		public void TestAH_ChequeOrReferenceTextBoxIsVisibleOnlyForAP()
		{
			var expectedShortCaption = "Sup. Cost Ref.";
			var expectedCaption = "Supplier Cost Reference";

			var aPInv = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("Ledger Type", LedgerTypes.AccountsPayable, aPInv.AH_Ledger);
			AssertChequeOrReferenceTextBox(aPInv, expectedShortCaption, expectedCaption);

			aPInv.SaveAsIncomplete();
			AssertEquals("Ledger Type", LedgerTypes.IncompleteTransactions, aPInv.AH_Ledger);
			AssertChequeOrReferenceTextBox(aPInv, expectedShortCaption, expectedCaption);

			var uAInv = Factory.NewWithValidTestData<UAInvoice>();
			Factory.Save();
			AssertEquals("Ledger Type", LedgerTypes.UnapprovedPayableTransactions, uAInv.AH_Ledger);
			AssertChequeOrReferenceTextBox(uAInv, expectedShortCaption, expectedCaption);
		}

		public void TestAH_ChequeOrReferenceTextBoxForAR()
		{
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var aRAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			Factory.Save();

			var expectedShortCaption = "Sell Ref.";
			var expectedCaption = "Sell Reference";

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, aRInv.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.Invoice, aRInv.AH_TransactionType);
			AssertChequeOrReferenceTextBox(aRInv, expectedShortCaption, expectedCaption);

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, aRCreditNote.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.CreditNote, aRCreditNote.AH_TransactionType);
			AssertChequeOrReferenceTextBox(aRCreditNote, expectedShortCaption, expectedCaption);

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, aRAdjustmentNote.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.AdjustmentNote, aRAdjustmentNote.AH_TransactionType);
			AssertChequeOrReferenceTextBox(aRAdjustmentNote, expectedShortCaption, expectedCaption);
		}

		void AssertChequeOrReferenceTextBox(InvoicingBase invoice, string shortCaption, string caption)
		{
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert("Should be visible", form.InvoiceDetails.AH_ChequeOrReferenceTextBox.Visible);
				AssertEquals(shortCaption, form.InvoiceDetails.AH_ChequeOrReferenceTextBox.CaptionResourceString.ShortCaption);
				AssertEquals(caption, form.InvoiceDetails.AH_ChequeOrReferenceTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestComplianceDocumentRelatedColumnsVisible()
		{
			var arInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new InvoiceForm(arInv))
			{
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentNumber));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSubType));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentOrganization));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentDate));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentSupportingReason));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentType));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(arInv))
			{
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentNumber));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSubType));
			}

			var apInv = Factory.NewWithValidTestData<APInvoice>();
			apInv.AH_TransactionType = TransactionTypes.CreditNote;
			Factory.Save();
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentOrganization));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentDate));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentSupportingReason));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentType));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting));
			}

			var line = (APInvoiceLine)apInv.Lines.AddNew(typeof(APInvoiceLine));
			apInv.AH_TransactionType = TransactionTypes.Invoice;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentOrganization));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentDate));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentSupportingReason));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentType));
				AssertEquals(false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting));
			}

			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				apInv.AH_OH = TestObjectCreator.AALSHI.PK;
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentOrganization));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentDate));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentSupportingReason));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentType));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentNumber));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSubType));
			}

			apInv.AH_Ledger = LedgerTypes.IncompleteTransactions;
			apInv.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentOrganization));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentDate));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentSupportingReason));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSupportingDocumentType));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceDocumentNumber));
				AssertEquals(true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.ComplianceSubType));
			}
		}

		public void TestOnHasPCDSettingInfoChangedWithOutException()
		{
			var apInv = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();

			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			TestObjectCreator.ABIGAS.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				AssertNoExceptionThrown("With PCD setting", () => apInv.AH_OH = TestObjectCreator.AALSHI.PK);

				AssertNoExceptionThrown("Without PCD setting", () => apInv.AH_OH = TestObjectCreator.ABIGAS.PK);
			}
		}

		public void TestComplianceDocumentRelatedColumnsEnable()
		{
			var arInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(arInv))
			{
				arInv.AH_Ledger = LedgerTypes.AccountsReceivable;
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(form.DisplayMode, ODisplayMode.Edit);
				Assert(linesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly);
				Assert(linesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly);
			}

			var apInv = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				apInv.AH_TransactionType = TransactionTypes.CreditNote;
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals(form.DisplayMode, ODisplayMode.Edit);
				Assert(linesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly);
				Assert(linesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly);
			}

			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				apInv.AH_TransactionType = TransactionTypes.Invoice;
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				apInv.AH_OH = TestObjectCreator.AALSHI.PK;
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly);
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly);
			}

			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				apInv.AH_Ledger = LedgerTypes.IncompleteTransactions;
				apInv.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				apInv.AH_OH = TestObjectCreator.AALSHI.PK;
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly);
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly);
			}

			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(apInv))
			{
				apInv.AH_Ledger = LedgerTypes.IncompleteTransactions;
				apInv.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				apInv.AH_OH = TestObjectCreator.AALSHI.PK;
				Application.DoEvents();
				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly);
				Assert(!linesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly);
			}
		}

		public void TestAHDocReceivedDateEditVisible()
		{
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, aRInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aRInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be visible", false, form.InvoiceDetails.AH_DocReceivedDateEdit.Visible);
			}

			var aPInv = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("Ledger Type", LedgerTypes.AccountsPayable, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should be visible", true, form.InvoiceDetails.AH_DocReceivedDateEdit.Visible);
			}

			aPInv.SaveAsIncomplete();
			AssertEquals("Ledger Type", LedgerTypes.IncompleteTransactions, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should be visible", true, form.InvoiceDetails.AH_DocReceivedDateEdit.Visible);
			}
		}

		public void TestInvoiceRemittanceReferenceTextBoxVisible()
		{
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, aRInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aRInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should be visible", true, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAR.Visible);
				AssertEquals("Should not be visible", false, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAP.Visible);
			}

			var aPInv = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("Ledger Type", LedgerTypes.AccountsPayable, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be visible", false, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAR.Visible);
				AssertEquals("Should be visible", true, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAP.Visible);
			}

			aPInv.SaveAsIncomplete();
			AssertEquals("Ledger Type", LedgerTypes.IncompleteTransactions, aPInv.AH_Ledger);
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be visible", false, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAR.Visible);
				AssertEquals("Should be visible", true, form.InvoiceDetails.InvoiceRemittanceReferenceTextBoxForAP.Visible);
			}
		}

		public void TestGSTAmountColumnsIsVisible()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			AssertEquals("Ledger Type", LedgerTypes.AccountsPayable, aPInv.AH_Ledger);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("AL_OSGSTAmount column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSGSTAmount));
				AssertEquals("AL_LocalGSTAmount column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalGSTAmount));
				AssertEquals("TaxReportingBasisHumanReadableName column should be able.", true, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				AssertEquals("Tax Basis column should be hidden by default.", false, linesGrid.GetColumnStyle("TaxReportingBasisHumanReadableName").IsVisible);
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage).IsVisible);
				AssertEquals("AL_OSTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_OSTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable).IsVisible);
			}

			using (InvoiceForm form = new InvoiceForm(aRInv))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("AL_OSGSTAmount column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSGSTAmount));
				AssertEquals("AL_LocalGSTAmount column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalGSTAmount));
				AssertEquals("TaxReportingBasisHumanReadableName column should be able.", true, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				AssertEquals("Tax Basis column should be hidden by default.", false, linesGrid.GetColumnStyle("TaxReportingBasisHumanReadableName").IsVisible);
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage));
				AssertEquals("AL_OSTaxAmount_Recoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable));
				AssertEquals("AL_OSTaxAmount_NotRecoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable));
				AssertEquals("AL_LocalTaxAmount_Recoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable));
				AssertEquals("AL_LocalTaxAmount_NotRecoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable));
			}

			aPInv.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("TaxReportingBasisHumanReadableName column should be hidden.", false, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage).IsVisible);
				AssertEquals("AL_OSTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_OSTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable).IsVisible);
			}

			aPInv.AH_Ledger = LedgerTypes.IncompleteTransactions;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("TaxReportingBasisHumanReadableName column should be able.", true, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage).IsVisible);
				AssertEquals("AL_OSTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_OSTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_Recoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable).IsVisible);
				AssertEquals("AL_LocalTaxAmount_NotRecoverable column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable).IsVisible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("AL_OSGSTAmount column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSGSTAmount));
				AssertEquals("AL_LocalGSTAmount column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalGSTAmount));
				AssertEquals("TaxReportingBasisHumanReadableName column should be hidden.", false, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage));
				AssertEquals("AL_OSTaxAmount_Recoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable));
				AssertEquals("AL_OSTaxAmount_NotRecoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable));
				AssertEquals("AL_LocalTaxAmount_Recoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable));
				AssertEquals("AL_LocalTaxAmount_NotRecoverable column should be hidden.", false, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable));
			}
		}

		public void TestJobOperatorColumnVisible()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			TestObjectCreator.CreateGenExportBatchSequenceHeader(200, invoicePendingAllocation.PK, 1);
			invoicePendingAllocation.AH_OSTaxAmount = 10m;
			invoicePendingAllocation.AH_Desc = "Test Description";
			invoicePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			Assert("Precondition", invoice is APInvoice);
			AssertEquals("Precondition", LedgerTypes.TransactionsPendingAllocation, invoice.AH_LedgerInfo.OriginalValue);

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("JobOperator column should be available.", true, linesGrid.Columns.Contains("Job+JH_GS_NKRepOps"));
				AssertEquals("JobOperator column should not be visible by default.", false, linesGrid.Columns["Job+JH_GS_NKRepOps"].IsVisible);
				AssertEquals("JobOperator column should be readonly.", true, linesGrid.Columns["Job+JH_GS_NKRepOps"].ColumnStyle.ReadOnly);
			}

			invoice = Factory.New<APInvoice>();
			Assert("Precondition", invoice is APInvoice);
			AssertNotEquals("Precondition", LedgerTypes.TransactionsPendingAllocation, invoice.AH_LedgerInfo.OriginalValue);

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("JobOperator column should not be available.", false, linesGrid.Columns.Contains("Job+JH_GS_NKRepOps"));
			}
		}

		public void TestSetInvoiceTypeCaptions()
		{
			using (InvoiceUserControl iuc = new InvoiceUserControl())
			{
				iuc.SetInvoiceTypeCaptions("Hello", "World");
				AssertEquals("Hello should be used for header group box", "Hello Summary", iuc.HeaderGroupBox.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("World should be used for header invoice date edit", "World Date", iuc.AH_InvoiceDateEdit.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("World should be used for header transaction number text box", "World Number", iuc.AH_TransactionNumTextBox.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestSetOrgInformationGroupBoxCaption()
		{
			using (InvoiceUserControl iuc = new InvoiceUserControl())
			{
				iuc.SetOrgInformationGroupBoxCaption("Blah");
				AssertEquals("Blah should be used for OrgInformationGroupBox caption", "Blah", iuc.AddressWithContactControl.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestAH_NumberOfSupportingDocumentsCalcEditVisibility()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Italy);
			using (var form = new ZForm())
			{
				var invoiceControl = new InvoiceUserControl();
				form.Controls.Add(invoiceControl);
				form.Show();
				AssertEquals("Not china, so not visible", false, invoiceControl.AH_NumberOfSupportingDocumentsCalcEdit.Visible);
			}
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using (var form = new ZForm())
			{
				var invoiceControl = new InvoiceUserControl();
				form.Controls.Add(invoiceControl);
				form.Show();
				AssertEquals("China, so visible", true, invoiceControl.AH_NumberOfSupportingDocumentsCalcEdit.Visible);
			}
		}

		public void TestPlaceOfSupplyDropEditVisibility()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var apInv = Factory.NewWithValidTestData<APInvoice>();
					using (var form = new InvoiceForm(apInv))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply applicable, so visible", regValue, form.InvoiceDetails.zDropEditPlaceOfSupply.Visible);

						var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
						AssertEquals("Place of Supply", true, linesGrid.Columns.Contains("AL_PlaceOfSupply"));
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var apInv = Factory.NewWithValidTestData<APInvoice>();
					using (var form = new InvoiceForm(apInv))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply not applicable, so not visible", false, form.InvoiceDetails.zDropEditPlaceOfSupply.Visible);

						var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
						AssertEquals("Place of Supply", false, linesGrid.Columns.Contains("zDropEditPlaceOfSupply"));
					}
				}
			}
		}

		public void TestSupplyTypeColumnVisibility_OldTestWrittenBeforeUsingViewModel()
		{
			// This test has been retained to ensure that original functionality is not affected after view model is used to make column visibility decision.
			// Do not re-use this test's method to write any test in the future.

			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("Supply Type column shouldn't be available.", false, linesGrid.Columns.Contains("AL_SupplyType"));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("Supply Type column should be available.", true, linesGrid.Columns.Contains("AL_SupplyType"));
			}
		}

		public void TestSupplyColumnVisibility()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);

			var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();

			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);

			AssertSupplyTypeColumnVisibility("When IsSupplyTypeColumnVisible() returns false", false);
			AssertSupplyTypeColumnVisibility("When IsSupplyTypeColumnVisible() returns true", true);

			void AssertSupplyTypeColumnVisibility(string message, bool isColumnVisible)
			{
				accountingPresentationProviderFactoryMock.Setup(x => x.GetInvoiceFormPresentationProvider()).Returns(invoiceFormPresentationProviderMock.Object);
				invoiceFormPresentationProviderMock.Setup(x => x.IsSupplyTypeColumnVisible()).Returns(isColumnVisible);

				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;

					invoiceFormPresentationProviderMock.Verify(x => x.IsSupplyTypeColumnVisible(), Times.Once);
					AssertEquals(message, !isColumnVisible, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_SupplyType).IsUnavailable);
					invoiceFormPresentationProviderMock.Invocations.Clear();
				}
			}
		}

		public void TestOverrideTransactionDescriptionMenuItem()
		{
			var alternateBranch = TestObjectCreator.CreateBranch("TMP", TestObjectCreator.CreateNewCompany("TMP"));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToString());
			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100);
			job.JH_JobLocalReference = "123";
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where(m => m.Text == "Override Transaction Line Description").ToList();
				AssertEquals("Invoice not in database, so hide menu", 0, otdItems.Count);
				form.Close();
			}

			Factory.Save();

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("Invoice in database, so show menu", 1, otdItems.Count);

				otdItems[0].PerformClick();
				AssertEquals("Please select one or more invoice lines", UnitTestUserNotification.Instance.LastMessage.Text);
				linesGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				otdItems[0].PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				var overridePopupForm = Application.OpenForms.Cast<Form>().Single(f => f is OverrideInvoiceLineDescriptionForm);
				AssertNotNull("An OverrideInvoiceLineDescriptionForm was opened", overridePopupForm);
				overridePopupForm.Close();

				form.Close();
			}

			Env.Security.OverrideTransactionLineDescription.IsAllowed = false;
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("Invoice in database, so show menu", 1, otdItems.Count);

				otdItems[0].PerformClick();
				AssertEquals(@"You do not have sufficient rights to override transaction line descriptions. You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Override Transaction Line Description", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, alternateBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("Sister company looking at invoice, so don't show menu", 0, otdItems.Count);
			}

			Env.Security.OverrideTransactionLineDescription.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, String.Empty);

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Description")).ToList();
				AssertEquals("No charge codes set up in registry so menu item hidden", 0, otdItems.Count);
			}
		}

		public void TestOverrideTransactionSequenceMenuItem()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Now.AddDays(-30)))
			{
				var chargeCode = TestObjectCreator.RevenueChargeCode;
				var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
				var job2 = TestObjectCreator.CreateJob("Job2", TestObjectCreator.LocalClient, 2, TestObjectCreator.Agent, 2);
				var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", -100);
				line1.AL_Sequence = 1;
				line1.AL_JH = job.PK;
				line1.AL_AC = TestObjectCreator.CC1.PK;
				line1.GenericCharge = chargeCode.PK;
				line1.AL_AT = TestObjectCreator.GST1.PK;
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", 200);
				line2.AL_Sequence = 2;
				line2.AL_JH = job2.PK;
				line2.AL_AC = TestObjectCreator.CC1.PK;
				job.JH_JobLocalReference = "123";
				line2.GenericCharge = chargeCode.PK;
				line2.AL_AT = TestObjectCreator.GST2.PK;
				TestObjectCreator.CreateJobCharge(line1, job, chargeCode);
				TestObjectCreator.CreateJobCharge(line2, job2, chargeCode);

				Factory.Save();

				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
					var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Sequence")).ToList();
					AssertEquals("Should show menu 'Override Transaction Line Sequence'.", 1, otdItems.Count);

					otdItems[0].PerformClick();
					AssertEquals("Please select one or more invoice lines", UnitTestUserNotification.Instance.LastMessage.Text);
					linesGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					otdItems[0].PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

					var overridePopupForm = Application.OpenForms.Cast<Form>().Single(f => f is OverrideInvoiceLineSequenceForm);
					AssertNotNull("An OverrideInvoiceLineSequenceForm was opened", overridePopupForm);
					overridePopupForm.Close();

					form.Close();
				}

				Env.Security.OverrideTransactionLineSequence.IsAllowed = false;
				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
					var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Sequence")).ToList();
					AssertEquals("Should show menu 'Override Transaction Line Sequence'.", 1, otdItems.Count);

					otdItems[0].PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Override Transaction Line Sequence", UnitTestUserNotification.Instance.LastMessage.Text);
					form.Close();
				}

				line1.AL_OSExTaxAmount = 100;

				var overrideTransactionLineSequenceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(invoice.Company.GC_RN_NKCountryCode) as IOverrideTransactionLineSequenceProvider;

				AssertEquals(false, overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice));

				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
					var otdItems = linesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Where((m => m.Text == "Override Transaction Line Sequence")).ToList();
					AssertEquals("CanOverrideTransactionLineSequence is false, so don't show menu", 0, otdItems.Count);
				}
			}
		}

		public void TestBranchNameAndDepartmentDescriptionColumnsIsVisibleAndIsReadOnly()
		{
			var aPInv = Factory.NewWithValidTestData<APInvoice>();
			var aPCNote = Factory.NewWithValidTestData<APCreditNote>();
			var aPAdjust = Factory.NewWithValidTestData<APAdjustmentNote>();
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			var aRCNote = Factory.NewWithValidTestData<ARCreditNote>();
			var aRAdjust = Factory.NewWithValidTestData<ARAdjustmentNote>();
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			AssertBranchNameAndDepartmentDesc(aPInv);
			AssertBranchNameAndDepartmentDesc(aPCNote);
			AssertBranchNameAndDepartmentDesc(aPAdjust);
			AssertBranchNameAndDepartmentDesc(aRInv);
			AssertBranchNameAndDepartmentDesc(aRCNote);
			AssertBranchNameAndDepartmentDesc(aRAdjust);
		}

		public void TestTaxSummaryTabPageDisplayForPostedInvoice()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			var gST1 = TestObjectCreator.CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1);
			line1.AL_AT = gST1.PK;
			Factory.Save();

			AssertEquals(true, invoice.IsTaxed);
			Assert("Should be in database!", invoice.IsInDatabase);
			AssertTaxSummaryTabPageDisplay(true, invoice, ODisplayMode.Browse);

			Factory.ClearCachedValue<ZBool>("IsTaxed:" + invoice.PK.ToStringKey());

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				line.AL_AT = ZGuid.Empty;
			}
			AssertEquals(false, invoice.IsTaxed);
			AssertTaxSummaryTabPageDisplay(false, invoice, ODisplayMode.Browse);
		}

		public void TestTaxSummaryTabPageDisplayForAH_Ledger()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			AssertTaxSummaryTabPageDisplay(true, invoice, ODisplayMode.Edit);
			invoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			AssertTaxSummaryTabPageDisplay(true, invoice, ODisplayMode.Edit);
			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			AssertTaxSummaryTabPageDisplay(true, invoice, ODisplayMode.Edit);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			AssertTaxSummaryTabPageDisplay(false, invoice, ODisplayMode.Edit);
			invoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			AssertTaxSummaryTabPageDisplay(false, invoice, ODisplayMode.Edit);
			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			AssertTaxSummaryTabPageDisplay(false, invoice, ODisplayMode.Edit);
		}

		public void TestTaxSummaryTabPageDisplayForNewInvoice()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			AssertTaxSummaryTabPageDisplay(true, invoice, ODisplayMode.New);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			AssertTaxSummaryTabPageDisplay(false, invoice, ODisplayMode.New);
		}

		public void TestTaxSummaryTabPageDisplaysTaxNameSpecificToCountry()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				var tabPage = form.InvoiceDetails.TabControl_ForTest.TabPages["TaxSummaryTabPage"] as ZTabPage;
				AssertEquals("GST Tax Summary", tabPage.CaptionResourceString.Caption);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				var tabPage = form.InvoiceDetails.TabControl_ForTest.TabPages["TaxSummaryTabPage"] as ZTabPage;
				AssertEquals("VAT Tax Summary", tabPage.CaptionResourceString.Caption);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				var tabPage = form.InvoiceDetails.TabControl_ForTest.TabPages["TaxSummaryTabPage"] as ZTabPage;
				AssertEquals("TVA Tax Summary", tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTaxTransactionSummaryTabPageVisibility_IsApplicableForTaxTransactions()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			AssertEquals("Invoice applicable for Tax Transactions", false, taxRecordParent.IsApplicableForTaxTransactions);
			AssertTaxTransactionSummaryTabPageVisibility(invoice, false);

			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			AssertEquals("Invoice applicable for Tax Transactions", true, taxRecordParent.IsApplicableForTaxTransactions);
			AssertTaxTransactionSummaryTabPageVisibility(invoice, true);
		}

		public void TestTaxTransactionSummaryTabPageVisibility_WhenNotPostedAndTaxTransactionsNotExists()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			Assert("Precondition: IsPosted ", !invoice.IsPosted);

			var taxTransactions = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice).TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection;
			Assert(!taxTransactions.Any());

			AssertTaxTransactionSummaryTabPageVisibility(invoice, false);
		}

		public void TestTaxTransactionSummaryTabPageVisibility_WhenNotPostedAndTaxTransactionsExists()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			Assert("Precondition: IsPosted ", !invoice.IsPosted);

			AssertTaxTransactionSummaryTabPageVisibility_WhenTaxTransactionExists(invoice, false);
		}

		public void TestTaxTransactionSummaryTabPageVisibility_WhenPostedAndTaxTransactionsNotExists()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			Factory.Save();

			Assert("Precondition: IsPosted ", invoice.IsPosted);

			var taxTransactions = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice).TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection;
			Assert(!taxTransactions.Any());

			AssertTaxTransactionSummaryTabPageVisibility(invoice, false);
		}

		public void TestTaxTransactionSummaryTabPageVisibility_WhenPostedAndTaxTransactionsExists()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			Factory.Save();

			Assert("Precondition: IsPosted ", invoice.IsPosted);
			AssertTaxTransactionSummaryTabPageVisibility_WhenTaxTransactionExists(invoice, true);
		}

		void AssertTaxTransactionSummaryTabPageVisibility(InvoicingBase invoice, bool isVisible)
		{
			using (var form = new ZForm(invoice))
			using (var control = new InvoiceUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var tabPage = control.TabControl_ForTest.TabPages["TaxTransactionSummaryTabPage"] as ZTabPage;
				var tabPageVisibility = tabPage != null && tabPage.TabVisible;
				AssertEquals("TaxTransactionSummaryTabPage visibility", isVisible, tabPageVisibility);
				if (isVisible)
				{
					Assert(!control.IsTaxTransactionSummaryTabActivated_ForTest);
					control.ActivateOtherTaxesTab();
					Assert(control.IsTaxTransactionSummaryTabActivated_ForTest);
				}
				else
				{
					Assert(!control.IsTaxTransactionSummaryTabActivated_ForTest);
					control.ActivateOtherTaxesTab();
					Assert("TaxTransactionSummaryTabPage can not be selected due to it is invisible", !control.IsTaxTransactionSummaryTabActivated_ForTest);
				}
			}
		}

		void AssertTaxTransactionSummaryTabPageVisibility_WhenTaxTransactionExists(InvoicingBase invoice, bool istaxTransactionSummaryTabPageVisible)
		{ 
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");

				var taxTransaction = Factory.New<AccTaxTransaction>();
				taxTransaction.ATT_AH = invoice.PK;

				var taxTransactions = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice).TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection;
				Assert(taxTransactions.Any());
				AssertTaxTransactionSummaryTabPageVisibility(invoice, istaxTransactionSummaryTabPageVisible);
			}
		}

		public void TestInvoiceOtherTaxesControlPresentationProviderIsInitialized()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);

			using (var form = new ZForm(invoice))
			using (var control = new InvoiceUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNoExceptionThrown("OtherTaxesTab is activated without exceptions meaning that presentation providers are initialized", () => control.ActivateOtherTaxesTab());
				AssertEquals("Postcondition: OtherTaxesTab tab is active now", control.IsTaxTransactionSummaryTabActivated_ForTest, true);
			}
		}

		public void TestAPInvoiceTabPagesVisibilityOnGSTDisabledAndTaxFrameworkDisabled()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD) as APInvoice;
			apInvoice.AH_OH = creditor.PK;
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.New, true, false);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.Browse, false, false);
		}

		public void TestAPCreditNoteTabPagesVisibilityOnGSTDisabledAndTaxFrameworkDisabled()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD001", TestObjectCreator.AUD) as APCreditNote;
			apCreditNote.AH_OH = creditor.PK;
			var apCreditNoteLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.New, true, false);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.Browse, false, false);
		}

		public void TestAPInvoiceTabPagesVisibilityOnGSTDisabledAndTaxFrameworkEnabled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD) as APInvoice;
			apInvoice.AH_OH = creditor.PK;
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.New, true, true);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.Browse, false, true);
		}

		public void TestAPCreditNoteTabPagesVisibilityOnGSTDisabledAndTaxFrameworkEnabled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD001", TestObjectCreator.AUD) as APCreditNote;
			apCreditNote.AH_OH = creditor.PK;
			var apCreditNoteLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.New, true, true);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.Browse, false, true);
		}

		public void TestAPInvoiceTabPagesVisibilityOnGSTEnabledAndTaxFrameworkEnabled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD) as APInvoice;
			apInvoice.AH_OH = creditor.PK;
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			apInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.New, true, true);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.Browse, true, true);
		}

		public void TestAPCreditNoteTabPagesVisibilityOnGSTEnabledAndTaxFrameworkEnabled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD001", TestObjectCreator.AUD) as APCreditNote;
			apCreditNote.AH_OH = creditor.PK;
			var apCreditNoteLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			apCreditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.New, true, true);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.Browse, true, true);
		}

		public void TestAPInvoiceTabPagesVisibilityOnGSTEnabledAndTaxFrameworkDisabled()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD) as APInvoice;
			apInvoice.AH_OH = creditor.PK;
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			apInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.New, true, false);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apInvoice, ODisplayMode.Browse, true, false);
		}

		public void TestAPCreditNoteTabPagesVisibilityOnGSTEnabledAndTaxFrameworkDisabled()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("Creditor1", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD001", TestObjectCreator.AUD) as APCreditNote;
			apCreditNote.AH_OH = creditor.PK;
			var apCreditNoteLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
			apCreditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.New, true, false);
			Factory.Save();
			AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(apCreditNote, ODisplayMode.Browse, true, false);
		}

		void AssertGSTAndTaxTransactionsSummaryTabPagesVisibility(InvoicingBase invoice, ODisplayMode mode, bool expectedTaxSummaryTabIsVisible, bool expectedOtherTaxesSummaryTabIsVisible)
		{
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			AssertEquals("Invoice applicable for Tax Transactions", expectedOtherTaxesSummaryTabIsVisible, taxRecordParent.IsApplicableForTaxTransactions);
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.DisplayMode = mode;
				form.Show();
				Application.DoEvents();
				AssertEquals("TaxSummaryTabPage_ForTestOnly visibility", expectedTaxSummaryTabIsVisible, form.InvoiceDetails.TaxSummaryTabPage_ForTestOnly.TabVisible);
				AssertEquals("Tax TransactionSummaryTabPage_ForTestOnly visibility", expectedOtherTaxesSummaryTabIsVisible, form.InvoiceDetails.TaxTransactionSummaryTabPage_ForTestOnly.TabVisible);
			}
		}

		public void TestRemoveTaxTransactionsButtonEnabledWhenOtherTaxesCalculated()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			AssertRemoveTaxTransactionsButtonDisabledWhenOtherTaxesCalculated(false);

			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);
			AssertRemoveTaxTransactionsButtonDisabledWhenOtherTaxesCalculated(true);

			void AssertRemoveTaxTransactionsButtonDisabledWhenOtherTaxesCalculated(bool isVisible)
			{
				using (var form = new ZForm(invoice))
				using (var control = new InvoiceUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();

					if (isVisible)
					{
						control.ActivateOtherTaxesTab();
						var button = form.GetControl<ZButton>("RemoveTaxTransactionsButton", true);
						Assert("Button is disabled", !button.Enabled);
						taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
						Assert("Button is enabled when Tax Transactions are calculated on the invoice", button.Enabled);
						taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
						Assert("Button is disabled when Tax Transactions are deleted on the invoice", !button.Enabled);
						taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
						Assert("Button is enabled when Tax Transactions are calculated on the invoice", button.Enabled);
						Factory.Save();
						Assert(invoice.IsInDatabase);
						Assert("Button is disabled after invoice is posted", !button.Enabled);
					}
					else
					{
						AssertNull("Tax Transaction Summary Tab Page is not visible", control.Controls["TaxTransactionSummaryTabPage"]);
						AssertNull("Button does not exist if Tax Transaction Summary Tab Page is not visible", control.RemoveTaxTransactionsButton_ForTestOnly);
					}
				}
			}
		}

		public void TestRemoveOtherTaxesButton_Click()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var viewModelMock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();
			TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);

			using (invoice.GetValidationSuspender())
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				var control = form.GetControl<InvoiceUserControl>("InvoiceDetails", true);
				AssertNotNull(control);
				Application.DoEvents();

				var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
				Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				var button = form.GetControl<ZButton>("CalculateTaxTransactionsButton", true);
				button.PerformClick();
				Assert(taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				button = form.GetControl<ZButton>("RemoveTaxTransactionsButton", true);
				viewModelMock.Verify(v => v.SuspendTrackingHasChanges, Times.Never);
				button.PerformClick();
				viewModelMock.Verify(v => v.SuspendTrackingHasChanges, Times.Once);
				Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
			}
		}

		public void TestOnOtherTaxesCalculated_Changed_UnhookedOnDispose()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			using (invoice.GetValidationSuspender())
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				var control = form.GetControl<InvoiceUserControl>("InvoiceDetails", true);
				AssertNotNull(control);
				Application.DoEvents();

				Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				var button = form.GetControl<ZButton>("CalculateTaxTransactionsButton", true);
				button.PerformClick();
				Assert(taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
				AssertEquals("The event handlers are hooked because of InvoiceUserControl, BaseInvoicingForm and TransactionLinesForTaxTransactionsDisplay", 3, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);

				button = form.GetControl<ZButton>("RemoveTaxTransactionsButton", true);
				button.PerformClick();
				Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				AssertEquals("The event handlers are hooked because of InvoiceUserControl, BaseInvoicingForm and TransactionLinesForTaxTransactionsDisplay", 3, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			}

			AssertEquals("The event handlers due to BaseInvoicingForm and InvoiceUserControl are unhooked when the form is disposed", 1, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);

			var displayOtherTaxesObj = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice);
			displayOtherTaxesObj.TaxRecordTransactionLinePivotForDisplay.UnhookEventHandler_OnOtherTaxesCalculatedBeforePosting_Changed_ForTestOnly();
			AssertEquals("The event handler that had remained after the disposed form was due to TransactionLinesForTaxTransactionsDisplay and is now unhooked", 0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
		}

		public void TestProcessWhenTaxTransactionsAddedOrRemovedCalled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			using (var form = new ZForm(invoice))
			using (var control = new InvoiceUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var controls = form.Controls.Find("InvoiceTaxTransactionsControl", true);
				AssertEquals(0, controls.Length);
				AssertNoExceptionThrown(() => taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true);
				AssertNoExceptionThrown(() => taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false);

				control.ActivateOtherTaxesTab();
				var invoiceOtherTaxesControl = form.GetControl<InvoiceOtherTaxesControl>("InvoiceTaxTransactionsControl", true);
				AssertNotNull(invoiceOtherTaxesControl);
				var pointingToTransactionLines = invoiceOtherTaxesControl.GetField("PointingToTransactionLines") as ZToolStripButton;
				var pointingToOtherTaxes = invoiceOtherTaxesControl.GetField("PointingToOtherTaxes") as ZToolStripButton;

				AssertProcessWhenOtherTaxesAddedOrRemovedCalled(pointingToTransactionLines);
				AssertProcessWhenOtherTaxesAddedOrRemovedCalled(pointingToOtherTaxes);

				void AssertProcessWhenOtherTaxesAddedOrRemovedCalled(ZToolStripButton button)
				{
					button.PerformClick();
					Assert(button.Checked);
					taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
					Assert(!button.Checked);

					button.PerformClick();
					Assert(button.Checked);
					taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
					Assert(!button.Checked);
				}
			}
		}

		void AssertBranchNameAndDepartmentDesc(InvoicingBase invoice)
		{
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
				AssertEquals("BranchName column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.BranchName));
				AssertEquals("DepartmentDescription column should be able.", true, linesGrid.Columns.Contains(InvoicingLineBase.Schema.DepartmentDescription));
				AssertEquals("BranchName should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.BranchName).IsVisible);
				AssertEquals("DepartmentDescription column should be hidden by default.", false, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.DepartmentDescription).IsVisible);
				AssertEquals("BranchName column should be readonly by default.", true, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.BranchName).IsReadOnly);
				AssertEquals("DepartmentDescription column should be readonly by default.", true, linesGrid.GetColumnStyle(InvoicingLineBase.Schema.DepartmentDescription).IsReadOnly);
			}
		}

		void AssertTaxSummaryTabPageDisplay(bool isContained, InvoicingBase invoice, ODisplayMode mode)
		{
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.DisplayMode = mode;
				form.Show();
				Application.DoEvents();

				Assert(isContained ? form.InvoiceDetails.TabControl_ForTest.TabPages.ContainsKey("TaxSummaryTabPage") : !form.InvoiceDetails.TabControl_ForTest.TabPages.ContainsKey("TaxSummaryTabPage"));
			}
		}

		public void TestShowSubAccounts()
		{
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote));
			var arCreditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote));
			var apAdjustmentNote = TestObjectCreator.CreateInvoice(typeof(APAdjustmentNote));
			var arAdjustmentNote = TestObjectCreator.CreateInvoice(typeof(ARAdjustmentNote));
			var uaInvoice = TestObjectCreator.CreateInvoice(typeof(UAInvoice));
			var uaCreditNote = TestObjectCreator.CreateInvoice(typeof(UACreditNote));

			AssertSubAccountForInvoice(apInvoice);
			AssertSubAccountForInvoice(arInvoice);
			AssertSubAccountForInvoice(apCreditNote);
			AssertSubAccountForInvoice(arCreditNote);
			AssertSubAccountForInvoice(apAdjustmentNote);
			AssertSubAccountForInvoice(arAdjustmentNote);
			AssertSubAccountForInvoice(uaInvoice);
			AssertSubAccountForCliam(uaCreditNote);

			void AssertSubAccountForInvoice(InvoicingBase invoice)
			{
				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					Application.DoEvents();
					AssertSubAccount(form, form.InvoiceDetails.TransactionLinesGrid, invoice);
				}
			}

			void AssertSubAccountForCliam(InvoicingBase invoice)
			{
				using (var form = new ZForm())
				using (var claimChargesUserControl = new ClaimChargesUserControl())
				{
					form.Controls.Add(claimChargesUserControl);
					claimChargesUserControl.SetDataBinding(invoice, null);
					form.Show();
					Application.DoEvents();
					AssertSubAccount(form, claimChargesUserControl.InvoiceUserControl.TransactionLinesGrid, invoice);
				}
			}

			void AssertSubAccount(ZForm form, ZGrid transactionLinesGrid, InvoicingBase transaction)
			{
				var subAccountsTabPage = form.FindSingleOrDefault<ZTabPage>("SubAccountsTabPage");
				AssertEquals("AL_Calc_FirstSubClassParent caption", "Sub Account 1 Type", transactionLinesGrid.GetColumnCaption("AL_Calc_FirstSubClassParent"));
				AssertEquals("AL_Calc_FirstSubClassParentId caption", "Sub Account 1", transactionLinesGrid.GetColumnCaption("AL_Calc_FirstSubClassParentId"));
				AssertEquals("AL_Calc_SecondSubClassParent caption", "Sub Account 2 Type", transactionLinesGrid.GetColumnCaption("AL_Calc_SecondSubClassParent"));
				AssertEquals("AL_Calc_SecondSubClassParentId caption", "Sub Account 2", transactionLinesGrid.GetColumnCaption("AL_Calc_SecondSubClassParentId"));

				AssertNotNull("Pre-condition: subAccountsTabPage", subAccountsTabPage);
				AssertEquals("SubAccountsTabPage should be visible", true, subAccountsTabPage.TabVisible);
				var parentTabPage = subAccountsTabPage.Parent as ZTabControl;
				AssertNotNull("Pre-condition: parentTabPage", parentTabPage);
				var lineChargesTabPage = form.FindSingleOrDefault<ZTabPage>("LineChargesTabPage");
				AssertNotNull("Pre-condition: slineChargesTabPage", lineChargesTabPage);

				var glHeaderWithoutSubAccount = TestObjectCreator.CreateGLHeader();
				var glHeaderWithSubAccount = TestObjectCreator.CreateGLHeaderWithSubAccount(Enterprise.ZArchitecture.Schema.OrgHeaderSchema.Constants.Prefix, false);
				var line1 = (InvoicingLineBase)transaction.Lines.AddNew();
				var line2 = (InvoicingLineBase)transaction.Lines.AddNew();

				line1.AL_AG = glHeaderWithoutSubAccount.PK;
				line2.AL_AG = glHeaderWithoutSubAccount.PK;

				transactionLinesGrid.ListManager.Position = 0;
				AssertEquals("current line should be line1", line1, transactionLinesGrid.GetCurrent());
				AssertEquals("LineChargesTabPage shoule be selected when line’s GL Account has no Sub Accounts", lineChargesTabPage, parentTabPage.SelectedTab);
				line1.AL_AG = glHeaderWithSubAccount.PK;
				AssertEquals("LineChargesTabPage shoule be selected when line’s GL Account has Sub Accounts and didn't select sub account cells", lineChargesTabPage, parentTabPage.SelectedTab);

				transactionLinesGrid.ListManager.Position = 1;
				AssertEquals("current line should be line2", line2, transactionLinesGrid.GetCurrent());
				AssertEquals("LineChargesTabPage shoule be selected when line’s GL Account has no Sub Accounts", lineChargesTabPage, parentTabPage.SelectedTab);

				transactionLinesGrid.ListManager.Position = 0;
				AssertEquals("current line should be line1", line1, transactionLinesGrid.GetCurrent());
				AssertEquals("LineChargesTabPage shoule be selected when line’s GL Account has Sub Accounts and didn't select sub account cells", lineChargesTabPage, parentTabPage.SelectedTab);
			}
		}

		public void TestSubAccountsIsAutoGenenatedWithFormActionForPayables()
		{
			AssertSubAccountForInvoice(typeof(APInvoice), ControllerIDs.APInvoice, typeof(InvoiceForm));
			AssertSubAccountForInvoice(typeof(APCreditNote), ControllerIDs.APCreditNote, typeof(CreditNoteForm));
			AssertSubAccountForInvoice(typeof(APAdjustmentNote), ControllerIDs.APAdjustmentNote, typeof(AdjustmentNoteForm));
			AssertSubAccountForInvoice(typeof(UAInvoice), ControllerIDs.UAInvoice, typeof(UAInvoiceForm));
		}

		public void TestSubAccountsIsAutoGenenatedWithFormActionForReceivables()
		{
			AssertSubAccountForInvoice(typeof(ARInvoice), ControllerIDs.ARInvoice, typeof(InvoiceForm));
			AssertSubAccountForInvoice(typeof(ARCreditNote), ControllerIDs.ARCreditNote, typeof(CreditNoteForm));
			AssertSubAccountForInvoice(typeof(ARAdjustmentNote), ControllerIDs.ARAdjustmentNote, typeof(AdjustmentNoteForm));
		}

		public void TestSubAccountsIsAutoGenenatedWithFormActionForIncompletePayables()
		{
			AssertSubAccountForInvoice(typeof(APInvoice), ControllerIDs.APIncompleteInvoice, typeof(InvoiceForm));
			AssertSubAccountForInvoice(typeof(APCreditNote), ControllerIDs.APIncompleteCreditNote, typeof(CreditNoteForm));
			AssertSubAccountForInvoice(typeof(APAdjustmentNote), ControllerIDs.APIncompleteAdjustmentNote, typeof(AdjustmentNoteForm));
		}

		public void TestSubAccountsIsAutoGenenatedWithFormActionForClaims()
		{
			AssertSubAccountForCliam(typeof(Business.AccQueryClaims.APAccQueryClaim), ControllerIDs.APAccQueryClaim, typeof(UACreditNote));
			AssertSubAccountForCliam(typeof(Business.AccQueryClaims.ARAccQueryClaim), ControllerIDs.ARAccQueryClaim, typeof(UACreditNote));

			void AssertSubAccountForCliam(Type claimType, ControllerID controllerID, Type invoiceType)
			{
				var queryClaim1 = (AccQueryClaim)Factory.NewWithValidTestData(claimType);
				var invoice1 = TestObjectCreator.CreateInvoice(invoiceType);
				queryClaim1.AY_AH = invoice1.PK;

				var glHeader1 = TestObjectCreator.CreateGLHeader();
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, ZArchitecture.Schema.OrgHeaderSchema.Constants.Prefix, false);
				var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
				line1.AL_AG = glHeader1.PK;
				line1.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				AssertSubAccountsIsAutoGenenatedWithFormAction(queryClaim1, false);
				AssertSubAccountsIsAutoGenenatedWithFormAction(queryClaim1, true);

				var queryClaim2 = (AccQueryClaim)Factory.CreateNewFactory().Load(claimType, queryClaim1.PK);
				AssertSubAccountsIsAutoGenenatedWithFormAction(queryClaim2, false);
				AssertSubAccountsIsAutoGenenatedWithFormAction(queryClaim2, true);

				void AssertSubAccountsIsAutoGenenatedWithFormAction(AccQueryClaim queryClaim, bool isViewForm)
				{
					var controller = ZControllerFactory.Create(controllerID);
					using (var form = (isViewForm ? controller.ShowViewForm(queryClaim) : controller.ShowEditForm(queryClaim)))
					{
						Application.DoEvents();
						AssertType(typeof(AccQueryClaimForm), form);
						var formUACreditNote = Factory.Load<UACreditNote>(((AccQueryClaim)((AccQueryClaimForm)form).BusinessEntity).AY_AH);

						AssertEquals("HasChanges", false, formUACreditNote.HasChanges);
						AssertEquals("IsInDatabaseIncludingChildren", true, formUACreditNote.IsInDatabaseIncludingChildren);
						AssertEquals("line count", 1, formUACreditNote.Lines.Count);
						AssertEquals("Sub Account Count", 1, formUACreditNote.Lines[0].SubAccounts.Count);
					}
				}
			}
		}

		void AssertSubAccountForInvoice(Type invoiceType, ControllerID controllerID, Type expectedFormType)
		{
			var isIncompleteInvoice = (controllerID == ControllerIDs.APIncompleteInvoice || controllerID == ControllerIDs.APIncompleteCreditNote || controllerID == ControllerIDs.APIncompleteAdjustmentNote);
			var glHeader1 = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, ZArchitecture.Schema.OrgHeaderSchema.Constants.Prefix, false);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var invoice1 = new TestObjectCreator(newFactory).CreateInvoice(invoiceType);
			var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_AG = glHeader1.PK;
			line1.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.AALSHI.PK;

			if (isIncompleteInvoice)
			{
				invoice1.SaveAsIncomplete();
			}
			else
			{
				newFactory.Save();
			}

			AssertSubAccountsIsAutoGenenatedWithFormAction(invoice1, true, 1);
			AssertSubAccountsIsAutoGenenatedWithFormAction(invoice1, false, 1);

			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, ZArchitecture.Schema.GlbStaffSchema.Constants.Prefix, false);
			Factory.Save();

			var invoice2 = (InvoicingBase)Factory.CreateNewFactory().Load(invoiceType, invoice1.PK);
			AssertSubAccountsIsAutoGenenatedWithFormAction(invoice2, true, isIncompleteInvoice ? 2 : 1);
			AssertSubAccountsIsAutoGenenatedWithFormAction(invoice2, false, isIncompleteInvoice ? 2 : 1);

			void AssertSubAccountsIsAutoGenenatedWithFormAction(InvoicingBase invoicingBase, bool isViewForm, int expectedSubAccount)
			{
				var controller = ZControllerFactory.Create(controllerID);
				using (var form = (isViewForm ? controller.ShowViewForm(invoicingBase) : controller.ShowEditForm(invoicingBase)))
				{
					Application.DoEvents();
					AssertType(expectedFormType, form);
					var formInvoicingBase = (InvoicingBase)((BaseInvoicingForm)form).BusinessEntity;

					AssertEquals("HasChanges", false, formInvoicingBase.HasChanges);
					AssertEquals("IsInDatabaseIncludingChildren", !isIncompleteInvoice, formInvoicingBase.IsInDatabaseIncludingChildren);
					AssertEquals("line count", 1, formInvoicingBase.Lines.Count);
					AssertEquals("Sub Account Count", expectedSubAccount, formInvoicingBase.Lines[0].SubAccounts.Count);
				}
			}
		}

		public void TestAL_TaxDateColumn()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				var taxDateColumn = form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_TaxDate);
				AssertNotNull(taxDateColumn);
				AssertEquals(true, taxDateColumn.IsVisible);
			}
		}

		public void TestInitExtendField()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				var extendDropEdit = form.InvoiceDetails.ExtendDropEdit_ForTestOnly;
				CombineAssertions("Hide as default", () =>
				{
					AssertEquals("Visible", false, extendDropEdit.Visible);
					AssertEquals("Enabled", false, extendDropEdit.Enabled);
				});

				form.InvoiceDetails.InitExtendField(x =>
				{
					x.Visible = true;
					AssertEquals("setting Visible as true", true, extendDropEdit.Visible);

					x.Enabled = true;
					AssertEquals("setting Visible as true", true, extendDropEdit.Enabled);
				});
			}
		}

		public void TestReversalStatusCode_Properties()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				AssertEquals(true, form.InvoiceDetails.ReversalDropEdit.Enabled);
				AssertEquals(false, form.InvoiceDetails.ReversalDropEdit.Visible);
				AssertEquals(false, form.InvoiceDetails.ReversalDropEdit.EditableInViewMode);
				AssertEquals(false, form.InvoiceDetails.ReversalDropEdit.ShouldResizeByMaxLength);
			}
		}

		public void TestChangeInvoiceHeaderSize()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();
				var headerGroupBoxOuter = form.InvoiceDetails.HeaderGroupBoxOuterPanel;
				var originalHeight = headerGroupBoxOuter.Height;
				var menuItem = headerGroupBoxOuter.ContextMenu.MenuItems.FindByText("Minimize Invoice Header");
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				AssertEquals("When MinizeInvoiceHeader menu item is clicked the height of HeaderGroupBoxOuterPanel reduce to its Minimum Height", headerGroupBoxOuter.MaximumSize.Height, 60);
				AssertEquals("And menuItem caption change to Maximize Invoice Header", ((ZMenuItem)menuItem).Caption, "Maximize Invoice Header");

				menuItem = headerGroupBoxOuter.ContextMenu.MenuItems.FindByText("Maximize Invoice Header");
				menuItem.PerformClick();
				AssertEquals("When MaximizeInvoiceHeader menu item is clicked the height of HeaderGroupBoxOuterPanel roll back to its original height", headerGroupBoxOuter.MaximumSize.Height, originalHeight);
				AssertEquals("And menuItem caption change to Minimize Invoice Header", ((ZMenuItem)menuItem).Caption, "Minimize Invoice Header");
			}
		}

		public void TestShowAlternateGLAccountNumberAndDescription_HasGLAccountSelectionAndEntry()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			AssertShowAlternateGLAccountNumberAndDescription(true);
		}

		public void TestShowAlternateGLAccountNumberAndDescription_NoGLAccountSelectionAndEntry()
		{
			AssertShowAlternateGLAccountNumberAndDescription(false);
		}

		void AssertShowAlternateGLAccountNumberAndDescription(bool hasGLAccountSelectionAndEntry)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				var alternateGLAccountNumber = form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("AlternateGLAccountNumber");
				AssertNotNull(alternateGLAccountNumber);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountNumber.IsUnavailable);

				var alternateGLAccountDescription = form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("AlternateGLAccountDescription");
				AssertNotNull(alternateGLAccountDescription);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountDescription.IsUnavailable);
			}
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}

		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		#endregion
	}
}
