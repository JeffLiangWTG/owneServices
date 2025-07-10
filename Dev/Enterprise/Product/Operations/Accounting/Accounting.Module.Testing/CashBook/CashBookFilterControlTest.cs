using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class CashBookFilterControlTest : TestCaseWithFactory
	{
		public void TestExchangeRateDecimalPlaces()
		{
			CashBookFilterBusinessObject filterBizO = new CashBookFilterBusinessObject();
			CashbookTransactionCollection transactions = new CashbookTransactionCollection(Factory);

			ZBool originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				using (CashBookFilterControl testControl = new CashBookFilterControl(transactions, filterBizO))
				{
					testControl.Show();
					ZCalcEditColumnStyleInfo exRateColumnStyle = (ZCalcEditColumnStyleInfo)testControl.FilteredGrid.GetColumnStyle("AH_ExchangeRate");
					AssertEquals("Exchange rate should have 6 decimal places", 6, exRateColumnStyle.Decimals);
				}

				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				using (CashBookFilterControl testControl = new CashBookFilterControl(transactions, filterBizO))
				{
					testControl.Show();
					ZCalcEditColumnStyleInfo exRateColumnStyle = (ZCalcEditColumnStyleInfo)testControl.FilteredGrid.GetColumnStyle("AH_ExchangeRate");
					AssertEquals("Exchange rate should have 6 decimal places", 6, exRateColumnStyle.Decimals);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		public void TestColumnsAddedCorrectly()
		{
			AssertColumnsAddedCorrectly("AH_GS_NKCashier");
			AssertColumnsAddedCorrectly("AH_GS_NKAuditedBy");

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertColumnsAddedCorrectly("AH_PlaceOfSupply", true, false);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertColumnsAddedCorrectly("AH_PlaceOfSupply", false);
			}
		}

		public void TestTaxBranchColumnAddedCorrectly()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertColumnsAddedCorrectly(AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch, false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertColumnsAddedCorrectly(AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch, false);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertColumnsAddedCorrectly(AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch, false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertColumnsAddedCorrectly(AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch, true);
		}

		public void TestTransactionCategoryColumn()
		{
			var transactions = new CashbookTransactionCollection(Factory);

			using (var form = new DummyForm(transactions))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var transactionCategoryColumn = columns.FirstOrDefault(x => x.ColumnName == "AH_TransactionCategory");

				AssertNotNull("Assert transaction category column exists", transactionCategoryColumn);
				AssertEquals("Assert transaction category column is available", false, transactionCategoryColumn.IsUnavailable);
				AssertEquals("Assert transaction category column is visible", true, transactionCategoryColumn.IsVisible);
				AssertEquals("Category", transactionCategoryColumn.CaptionResourceString.ShortCaption);
				AssertEquals("Transaction Category", transactionCategoryColumn.CaptionResourceString.Caption);
			}
		}

		void AssertColumnsAddedCorrectly(ZString columnName, bool isColumnsAvailable = true, bool isVisible = true)
		{
			var transactions = new CashbookTransactionCollection(Factory);

			using (var form = new DummyForm(transactions))
			{
				form.Show();
				var filterControl = form.FilterControl;
				var expectedColumn = columnName;
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var isAvailable = !(columns.FirstOrDefault(x => x.ColumnName == expectedColumn)?.IsUnavailable ?? true);
				AssertEquals("New column should be added.", isColumnsAvailable, isAvailable);
				if (isColumnsAvailable)
				{
					AssertEquals("Assert column is Visible", isVisible, columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
					AssertEquals("New column should not be read only", false, columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
				}
			}
		}

		class DummyForm : ZForm
		{
			public DummyForm(IBusinessObjectCollection gridCollection) : base(gridCollection)
			{
				FilterControl = new CashBookFilterControl(gridCollection, new CashBookFilterBusinessObject());
				Controls.Add(FilterControl);
			}

			public CashBookFilterControl FilterControl { get; }
		}
	}
}
