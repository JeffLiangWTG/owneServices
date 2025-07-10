using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentForm))]
	public class IntercompanyCostsApportionmentFormTest : AccountingZFormBasherTest
	{
		public AccountingPeriodTestHelper PeriodManagementTestHelper { get; private set; }

		protected override Form GetFormToBashCore()
		{
			return new IntercompanyCostsApportionmentForm(new IntercompanyCostsApportionmentInvoice(Factory));
		}

		IntercompanyCostsApportionmentInvoice testCostsApportionmentInvoice;
		GlbBranch intercompanyBranch;
		AccGLHeader headerCurrentCompany;
		AccGLHeader headerIntercompany;

		void setupPeriodManagement(int year)
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, Enterprise.MasterFiles.Business.AccountingPeriodTestHelper.CalendarType.CalendarYear);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, intercompanyBranch.GB_GC, Enterprise.MasterFiles.Business.AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void setupExchangeRate()
		{
			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = intercompanyBranch.GB_GC;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(2009, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2009, 12, 31);
			exRate.RE_SellRate = 0.7;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestShowGLAccountsForImportAction()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("TRR");
			Factory.Save();

			var alternateAccount = creator.CreateAccAlternateGlAccount(chart.PK, "111", Core.Constants.AccountType.BalanceSheetAccount);
			var glHeader = creator.CreateGLHeader("3333.33.33");
			var glHeader2 = creator.CreateGLHeader("4444.33.33");
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader.PK);
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader2.PK);
			Factory.Save();

			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			using (var form = new IntercompanyCostsApportionmentForm(invoice))
			{
				form.Show();
				AssertNotNull(invoice.Lines.ShowGLAccountsForImportAction);
				var lineSummaryGrid = (ZGrid)form.Controls.Find("LineSummaryGrid", true)[0];
				lineSummaryGrid.SetDataBinding(invoice, "Lines");
				Application.DoEvents();
				var style = lineSummaryGrid.Columns["GenericCharge"].ColumnStyle;
				lineSummaryGrid.BeginEdit(style, 0);
				var codeBox = ((ZGridGuidFindBox)lineSummaryGrid.LastFocusedColumn.EditControl).CodeBox;
				codeBox.Text = "111";
				form.Controls.Find("DescriptionTextBox", true)[0].Focus();
				Application.DoEvents();
				var selectionForm = (GLAccountSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(selectionForm);
			}
		}

		[TestDate(2017, 08, 03)]
		public void TestHandleComplianceSequenceFailedToAssign()
		{
			testCostsApportionmentInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			intercompanyBranch = TestObjectCreator.LoadIntercompanyBranch();
			headerCurrentCompany = TestObjectCreator.CreateCurrentCompanyIntercompanyClearingGLHeader();
			headerIntercompany = TestObjectCreator.CreateIntercompanyIntercompanyClearingGLHeader();
			Factory.Save();

			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate();
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testCostsApportionmentInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testCostsApportionmentInvoice.PostedDate = testCostsApportionmentInvoice.InvoiceDate;
			testCostsApportionmentInvoice.DueDate = testCostsApportionmentInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "IT");
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			testCostsApportionmentInvoice.Creditor = orgHeader.PK;

			testCostsApportionmentInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testCostsApportionmentInvoice.InvoiceNumber = "2222";
			testCostsApportionmentInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testCostsApportionmentInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			line.AL_GovtChargeCode = "GOVT1.2";
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert(AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value);
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

				using (IntercompanyCostsApportionmentForm form = new IntercompanyCostsApportionmentForm(testCostsApportionmentInvoice))
				{
					form.Show();
					form.FireSaveButton();
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
						"Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."));
				}
			}
		}

		public void TestBranchNameAndDepartmentDescriptionColumnsIsVisibleAndIsReadOnly()
		{
			testCostsApportionmentInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (var form = new IntercompanyCostsApportionmentForm(testCostsApportionmentInvoice))
			{
				form.Show();
				Application.DoEvents();

				var costsControl = form.GetControl<CostsControl>("costsControl");
				var linesGrid = costsControl.LineSummaryGrid;

				AssertEquals("BranchName column should be able.", true, linesGrid.Columns.Contains(IntercompanyCostsApportionmentInvoiceLine.Schema.BranchName));
				AssertEquals("DepartmentDescription column should be able.", true, linesGrid.Columns.Contains(IntercompanyCostsApportionmentInvoiceLine.Schema.DepartmentDescription));
				AssertEquals("BranchName should be hidden by default.", false, linesGrid.GetColumnStyle(IntercompanyCostsApportionmentInvoiceLine.Schema.BranchName).IsVisible);
				AssertEquals("DepartmentDescription column should be hidden by default.", false, linesGrid.GetColumnStyle(IntercompanyCostsApportionmentInvoiceLine.Schema.DepartmentDescription).IsVisible);
				AssertEquals("BranchName column should be readonly by default.", true, linesGrid.GetColumnStyle(IntercompanyCostsApportionmentInvoiceLine.Schema.BranchName).IsReadOnly);
				AssertEquals("DepartmentDescription column should be readonly by default.", true, linesGrid.GetColumnStyle(IntercompanyCostsApportionmentInvoiceLine.Schema.DepartmentDescription).IsReadOnly);
			}
		}
	}
}
