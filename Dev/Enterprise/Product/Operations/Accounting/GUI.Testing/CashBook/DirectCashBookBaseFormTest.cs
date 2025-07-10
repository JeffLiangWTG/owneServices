using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(DirectCashBookBaseForm))]
	public class DirectCashBookBaseFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DirectTransactionHeaderBase testTransaction = (DirectTransactionHeaderBase)Factory.New(typeof(DirectPayment));
			return new DirectCashBookBaseForm(testTransaction);
		}

		public void TestColumns()
		{
			using (DirectCashBookBaseForm form = (DirectCashBookBaseForm)GetFormToBashCore())
			{
				ZGrid linesGrid = form.CashBookLineBoundGrid_ForTestOnly;

				form.Show();

				AssertEquals("GL Account column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_AG));
				AssertEquals("Branch column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_GB));
				AssertEquals("Department column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_Desc));
				AssertEquals("AL_OSTaxAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_AG));
				AssertEquals("AL_OSExTaxAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSExTaxAmount));
				AssertEquals("AL_LocalExTaxAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalExTaxAmount));
				AssertEquals("AL_OSTaxAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount));
				AssertEquals("AL_LocalTaxAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount));
				AssertEquals("AL_OSTotalAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OverseasTotal));
				AssertEquals("AL_LocalTotalAmount column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTotalAmount));
				AssertEquals("AL_TaxID column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_AT));
				AssertEquals("AL_TaxDate column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_TaxDate));
				AssertEquals("AL_TaxMessage column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_A9_VATClass));
				AssertEquals("Tax Branch column should not be able by default.", false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_GB_TaxBranch));
				AssertEquals("Tax Branch column should not be able by default.", false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.TaxBranchName));
			}
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

			var testTransaction = (DirectTransactionHeaderBase)Factory.New(typeof(DirectPayment));
			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			using (var form = new DirectCashBookBaseForm(testTransaction))
			{
				form.Show();
				AssertNotNull(testTransaction.Lines.ShowGLAccountsForImportAction);
				var grid = (ZGrid)form.Controls.Find("CashBookLineBoundGrid", true)[0];
				grid.SetDataBinding(testTransaction, "Lines");
				Application.DoEvents();
				var style = grid.Columns["AL_AG"].ColumnStyle;
				grid.BeginEdit(style, 0);
				var codeBox = ((ZGridGuidFindBox)grid.LastFocusedColumn.EditControl).CodeBox;
				codeBox.Text = "111";
				form.Controls.Find("BankAccountsFindBox", true)[0].Focus();
				Application.DoEvents();
				var selectionForm = (GLAccountSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(selectionForm);
			}
		}

		public void TestColumns_EnableTaxBranchReporting()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, false);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.TaxBranchName, false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, false);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.TaxBranchName, false);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, false);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.TaxBranchName, false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, true, true, true);
			AssertColumnsAddedCorrectly(DirectTransactionLineBase.Schema.TaxBranchName, true, true, true);
		}

		void AssertColumnsAddedCorrectly(ZString columnName, bool isColumnsAvailable,bool isVisible = true, bool isReadOnly = true)
		{
			using (DirectCashBookBaseForm form = (DirectCashBookBaseForm)GetFormToBashCore())
			{
				form.Show();
				ZGrid linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
				var columns = linesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var isAvailable = !(columns.FirstOrDefault(x => x.ColumnName == columnName)?.IsUnavailable ?? true);
				AssertEquals("New column should be added.", isColumnsAvailable, isAvailable);
				if (isColumnsAvailable)
				{
					AssertEquals("Visible", isVisible, columns.FirstOrDefault(x => x.ColumnName == columnName).IsVisible);
					AssertEquals("ReadOnly", isReadOnly, !columns.FirstOrDefault(x => x.ColumnName == columnName).IsReadOnly);
				}
			}
		}

		public void TestAH_GB_TaxBranchGuidFindBox()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertVisible("AH_GB_TaxBranchGuidFindBox should be hidden when registry is not enabled.", false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertVisible("AH_GB_TaxBranchGuidFindBox should be hidden when registry is not enabled.", false);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertVisible("AH_GB_TaxBranchGuidFindBox should be hidden when registry is not enabled.", false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertVisible("AH_GB_TaxBranchGuidFindBox should display when registry is enabled.", true);

			void AssertVisible(string comment,bool isVisible = true)
			{
				using (DirectCashBookBaseForm form = (DirectCashBookBaseForm)GetFormToBashCore())
				{
					form.Show();
					AssertEquals(comment, isVisible, form.AH_GB_TaxBranchGuidFindBox_ForTestOnly.Visible);
				}
			}
		}

		public void TestExtraTaxColumnsAreShownInAllExtraTaxApplicableCountries()
		{
			AssertExtraTaxColumnsAreAvailable("AU", null, null, false);
			AssertExtraTaxColumnsAreAvailable("CA", "GST", "QST", true);
			AssertExtraTaxColumnsAreAvailable("IN", "GST", "SGST", true);
			AssertExtraTaxColumnsAreAvailable("MX", "IVA", "RET", true);
		}

		void AssertExtraTaxColumnsAreAvailable(string countryCode, string gSTCaption, string exTaxCaption, bool shouldBeVisible)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			using (DirectCashBookBaseForm form = (DirectCashBookBaseForm)GetFormToBashCore())
			{
				ZGrid linesGrid = form.CashBookLineBoundGrid_ForTestOnly;

				form.Direct_ForTestOnly.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				form.Show();

				if (shouldBeVisible)
				{
					AssertEquals("AL_OSExtraTaxAmount column should not be hidden.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount));
					AssertEquals("AL_OSExtraTaxAmount caption should show local tax name", exTaxCaption + " Amount", linesGrid.GetColumnCaption(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount));
					AssertEquals("AL_OSExtraTaxAmount column should not be hidden.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSGSTAmount));
					AssertEquals("AL_OSExtraTaxAmount caption should show local tax name", gSTCaption + " Amount", linesGrid.GetColumnCaption(DirectTransactionLineBase.Schema.AL_OSGSTAmount));
					form.Direct_ForTestOnly.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Andorra;
					AssertEquals("AL_LocalExtraTaxAmount column should not be hidden.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount));
					AssertEquals("AL_OSExtraTaxAmount caption should show local tax name", exTaxCaption + " Local", linesGrid.GetColumnCaption(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount));
					AssertEquals("AL_LocalExtraTaxAmount column should not be hidden.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalGSTAmount));
					AssertEquals("AL_OSExtraTaxAmount caption should show local tax name", gSTCaption + " Local", linesGrid.GetColumnCaption(DirectTransactionLineBase.Schema.AL_LocalGSTAmount));
				}
				else
				{
					AssertEquals("AL_OSExtraTaxAmount column should not be available.", false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount));
					AssertEquals("AL_LocalExtraTaxAmount column should not be available.", false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount));
				}
			}
		}

		public void TestLocalTotalsAreDisplayedBasedOnTransactionCurrency()
		{
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("AU", "USD", true);
			AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency("AU", "AUD", false);
		}

		void AssertLocalTotalsAreDisplayedBasedOnTransactionCurrency(string country, string currency, bool shouldBeVisible)
		{
			GlbCompany.CurrentCompany.SetCountry(country);
			using (DirectCashBookBaseForm form = (DirectCashBookBaseForm)GetFormToBashCore())
			{
				form.Direct_ForTestOnly.AH_GC = GlbCompany.CurrentCompany.PK;
				form.Direct_ForTestOnly.AH_RX_NKTransactionCurrency = currency;
				form.Show();

				AssertEquals(shouldBeVisible, form.AH_LocalExTaxAmountCalcFindBox_ForTestOnly.Visible);
			}
		}

		public void TestBranchNameAndDepartmentDescriptionColumnsIsVisibleAndIsReadOnly()
		{
			var dirPay = Factory.NewWithValidTestData<DirectPayment>();
			var dirRec = Factory.NewWithValidTestData<DirectReceipt>();
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			AssertBranchNameAndDepartmentDesc(dirPay);
			AssertBranchNameAndDepartmentDesc(dirRec);
		}

		void AssertBranchNameAndDepartmentDesc(DirectTransactionHeaderBase invoice)
		{
			using (var form = new DirectCashBookBaseForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				ZGrid linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("BranchName column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.BranchName));
				AssertEquals("DepartmentDescription column should be able.", true, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.DepartmentDescription));
				AssertEquals("BranchName should be hidden by default.", false, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.BranchName).IsVisible);
				AssertEquals("DepartmentDescription column should be hidden by default.", false, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.DepartmentDescription).IsVisible);
				AssertEquals("BranchName column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.BranchName).IsReadOnly);
				AssertEquals("DepartmentDescription column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.DepartmentDescription).IsReadOnly);
			}
		}

		public void TestExtraTaxColumnsIsReadOnly()
		{
			var dirPay = Factory.NewWithValidTestData<DirectPayment>();
			var dirRec = Factory.NewWithValidTestData<DirectReceipt>();
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			AssertExtraTaxColumns(dirPay);
			AssertExtraTaxColumns(dirRec);
		}

		public void TestAL_TaxDateColumn()
		{
			var directPayment = Factory.NewWithValidTestData<DirectPayment>();
			using (var form = new DirectCashBookBaseForm(directPayment))
			{
				form.Show();
				var taxDateColumn = form.CashBookLineBoundGrid_ForTestOnly.GetColumnStyle(DirectTransactionLineBase.Schema.AL_TaxDate);
				AssertNotNull(taxDateColumn);
				AssertEquals(false, taxDateColumn.IsVisible);
			}
		}

		void AssertExtraTaxColumns(DirectTransactionHeaderBase invoice)
		{
			using (var form = new DirectCashBookBaseForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("OverseasTotal column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OverseasTotal).IsReadOnly);
				AssertEquals("OSGSTAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSGSTAmount).IsReadOnly);
				AssertEquals("OSExtraTaxAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount).IsReadOnly);
				AssertEquals("LocalGSTAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalGSTAmount).IsReadOnly);
				AssertEquals("LocalExtraTaxAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount).IsReadOnly);
				AssertEquals("LocalTaxAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalTaxAmount).IsReadOnly);
				AssertEquals("LocalTotalAmount column should be readonly by default.", true, linesGrid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalTotalAmount).IsReadOnly);
			}
		}

		DirectTransactionHeaderBase CreateDirectTransactionForTest(DirectTransactionHeaderBase invoice, bool isTaxIncluded)
		{
			var line1 = TestObjectCreator.CreateDirectTransactionLine(invoice, TestObjectCreator.GLHeader1, 100m, 10m, "test line");
			if (isTaxIncluded)
			{
				var gST1 = TestObjectCreator.CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1);
				line1.AL_AT = gST1.PK;
			}
			else
			{
				line1.AL_AT = ZGuid.Empty;
			}
			return invoice;
		}

		void AssertTaxRelatedFieldsandColumnsDisplay(DirectTransactionHeaderBase invoice, bool hasTaxLines)
		{
			using (var form = new DirectCashBookBaseForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;

				if (invoice.AH_TransactionType == TransactionTypes.DirectPayment)
				{
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount_Recoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount_NotRecoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount_Recoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount_NotRecoverable));
				}
				else
				{
					AssertEquals(false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount_Recoverable));
					AssertEquals(false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount_NotRecoverable));
					AssertEquals(false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount_Recoverable));
					AssertEquals(false, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount_NotRecoverable));
				}

				AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_AT));
				AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_TaxDate));
				AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_A9_VATClass));
				AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_OSTaxAmount));
				AssertEquals(hasTaxLines, linesGrid.Columns.Contains(DirectTransactionLineBase.Schema.AL_LocalTaxAmount));
			}
		}

		public void TestTaxRelatedColumnsDisplayForPostedDirectTransactionNotHavingTaxLines()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var dirPay = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectPayment>(), false);
			var dirRec = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectReceipt>(), false);

			Factory.Save();

			AssertTaxRelatedFieldsandColumnsDisplay(dirPay, false);
			AssertTaxRelatedFieldsandColumnsDisplay(dirRec, false);
		}

		public void TestTaxRelatedColumnsDisplayForPostedDirectTransactionHavingTaxLines()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var dirPay = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectPayment>(), true);
			var dirRec = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectReceipt>(), true);

			Factory.Save();

			AssertTaxRelatedFieldsandColumnsDisplay(dirPay, true);
			AssertTaxRelatedFieldsandColumnsDisplay(dirRec, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			AssertTaxRelatedFieldsandColumnsDisplay(dirPay, true);
			AssertTaxRelatedFieldsandColumnsDisplay(dirRec, true);
		}

		public void TestPlaceOfSupplyDropEditVisiblity_DirectPayment()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var dirPay = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectPayment>(), true);
					using (var form = new DirectCashBookBaseForm(dirPay))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply applicable, so visible", regValue, form.ZDropEditPlaceOfSupply_ForTestOnly.Visible);

						var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
						AssertEquals("Place of Supply", true, linesGrid.Columns.Contains("AL_PlaceOfSupply"));
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var dirPay = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectPayment>(), true);
					using (var form = new DirectCashBookBaseForm(dirPay))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply not applicable, so not visible", false, form.ZDropEditPlaceOfSupply_ForTestOnly.Visible);

						var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
						AssertEquals("Place of Supply", false, linesGrid.Columns.Contains("AL_PlaceOfSupply"));
					}
				}
			}
		}

		public void TestPlaceOfSupplyDropEditVisiblity_DirectReceipt()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var dirRec = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectReceipt>(), true);
					using (var form = new DirectCashBookBaseForm(dirRec))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply applicable, so visible", regValue, form.ZDropEditPlaceOfSupply_ForTestOnly.Visible);

						var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
						AssertEquals("Place of Supply", true, linesGrid.Columns.Contains("AL_PlaceOfSupply"));
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var dirRec = CreateDirectTransactionForTest(Factory.NewWithValidTestData<DirectReceipt>(), true);
					using (var form = new DirectCashBookBaseForm(dirRec))
					{
						form.Show();
						Application.DoEvents();
						AssertEquals("Place of supply not applicable, so not visible", false, form.ZDropEditPlaceOfSupply_ForTestOnly.Visible);

						var linesGrid = form.CashBookLineBoundGrid_ForTestOnly;
						AssertEquals("Place of Supply", false, linesGrid.Columns.Contains("AL_PlaceOfSupply"));
					}
				}
			}
		}

		public void TestSupplyTypeColumnVisibilityForDirectPayment()
		{
			var directPayment = TestObjectCreator.CreateDirectPayment(TestObjectCreator.Today, 1000, 100, 2000, 200);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new DirectCashBookBaseForm(directPayment))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("Supply Type column not present", false, grid.Columns.Contains("AL_SupplyType"));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new DirectCashBookBaseForm(directPayment))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("Supply Type column present", true, grid.Columns.Contains("AL_SupplyType"));
			}
		}

		public void TestSupplyTypeColumnVisibilityForDirectReceipt()
		{
			var directReceipt = TestObjectCreator.CreateDirectReceipt(TestObjectCreator.Today, 1000, 100, 2000, 200);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new DirectCashBookBaseForm(directReceipt))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("Supply Type column not present", false, grid.Columns.Contains("AL_SupplyType"));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new DirectCashBookBaseForm(directReceipt))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.CashBookLineBoundGrid_ForTestOnly;
				AssertEquals("Supply Type column present", true, grid.Columns.Contains("AL_SupplyType"));
			}
		}

		[TestDate(2020, 3, 12)]
		public void TestSubAccountsIsAutoGenenatedWithFormAction()
		{
			var directPayment = TestObjectCreator.CreateDirectPayment(ZDateTime.Today, 100m, 10m, 100m, 0m);
			directPayment.Lines.RemoveAndDeleteAll();
			AssertSubAccountsIsAutoGenenatedWithDirectTransactionHeaderBase(directPayment, typeof(DirectPayment));

			var directReceipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Today, 100M, 10m, 100M, 0M);
			directReceipt.Lines.RemoveAndDeleteAll();
			AssertSubAccountsIsAutoGenenatedWithDirectTransactionHeaderBase(directReceipt, typeof(DirectReceipt));

			void AssertSubAccountsIsAutoGenenatedWithDirectTransactionHeaderBase(DirectTransactionHeaderBase header, Type type)
			{
				var header1 = header;
				var glHeader1 = TestObjectCreator.CreateGLHeader();
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);
				var line1 = header1.Lines.AddNew();
				line1.AL_AG = glHeader1.PK;
				line1.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				AssertSubAccountsIsAutoGenenatedWithFormAction(header1, true);
				AssertSubAccountsIsAutoGenenatedWithFormAction(header1, false);

				TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, false);
				Factory.Save();

				var header2 = (DirectTransactionHeaderBase)Factory.CreateNewFactory().Load(type, header1.PK);
				AssertSubAccountsIsAutoGenenatedWithFormAction(header2, true);
				AssertSubAccountsIsAutoGenenatedWithFormAction(header2, false);

				void AssertSubAccountsIsAutoGenenatedWithFormAction(DirectTransactionHeaderBase directTransactionHeaderBase, bool isViewForm)
				{
					var controller = ZControllerFactory.Create(type == typeof(DirectPayment) ? ControllerIDs.DirectPayment : ControllerIDs.DirectReceipt);
					using (var form = (isViewForm ? controller.ShowViewForm(directTransactionHeaderBase) : controller.ShowEditForm(directTransactionHeaderBase)))
					{
						Application.DoEvents();
						AssertType(type == typeof(DirectPayment) ? typeof(DirectPaymentForm) : typeof(DirectReceiptForm), form);
						var directCashBookBase = (DirectTransactionHeaderBase)((DirectCashBookBaseForm)form).BusinessEntity;

						AssertEquals("HasChanges", false, directCashBookBase.HasChanges);
						AssertEquals("IsInDatabaseIncludingChildren", true, directCashBookBase.IsInDatabaseIncludingChildren);
						AssertEquals("line count", 1, directCashBookBase.Lines.Count);
						AssertEquals("Sub Account Count", 1, directCashBookBase.Lines[0].SubAccounts.Count);
					}
				}
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
			var directPayment = TestObjectCreator.CreateDirectPayment(TestObjectCreator.Today, 1000, 100, 2000, 200);
			using (var form = new DirectCashBookBaseForm(directPayment))
			{
				form.Show();
				var alternateGLAccountNumber = form.CashBookLineBoundGrid_ForTestOnly.GetColumnStyle("AlternateGLAccountNumber");
				AssertNotNull(alternateGLAccountNumber);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountNumber.IsUnavailable);

				var alternateGLAccountDescription = form.CashBookLineBoundGrid_ForTestOnly.GetColumnStyle("AlternateGLAccountDescription");
				AssertNotNull(alternateGLAccountDescription);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountDescription.IsUnavailable);
			}
		}

		#region Implementation

		protected override bool ShouldHaveAuditPlugIn => true;

		#endregion
	}
}
