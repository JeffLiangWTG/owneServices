using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class EnquiryFilterControlTest : TestCaseWithFactory
	{
		public void TestExchangeRateDecimalPlaces()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			AssertDecimalPlacesForExchangeRateColumn();

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			AssertDecimalPlacesForExchangeRateColumn();
		}

		public void TestBinding()
		{
			using (EnquiryFilterControl control = GetTestFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("LVRSalesCalcEdit bound", true, control.LVRSalesCalcEdit.DataBindings.Count > 0);
				control.DisbursementTermsTextBox.Visible = true;
				AssertEquals("DisbursementTermsTextBox bound", true, control.DisbursementTermsTextBox.DataBindings.Count > 0);
				AssertEquals("StandardTermsTextBox bound", true, control.StandardTermsTextBox.DataBindings.Count > 0);
				AssertEquals("YTDPurchaseSalesCalcEdit bound", true, control.YTDPurchaseSalesCalcEdit.DataBindings.Count > 0);
				AssertEquals("CreditBalanceCalcEdit bound", true, control.CreditBalanceCalcEdit.DataBindings.Count > 0);
				AssertEquals("CreditLimitCalcEdit bound", true, control.CreditLimitCalcEdit.DataBindings.Count > 0);
				AssertEquals("MTD_PTDSalesCalcEdit bound", true, control.MTD_PTDSalesCalcEdit.DataBindings.Count > 0);
				AssertEquals("LastPaymentReceiptDateEdit bound", true, control.LastPaymentReceiptDateTextBox.DataBindings.Count > 0);
				AssertEquals("LastPaymentReceiptCalcEdit bound", true, control.LastPaymentReceiptCalcEdit.DataBindings.Count > 0);
				AssertEquals("LastPurchaseSaleCalcEdit bound", true, control.LastPurchaseSaleCalcEdit.DataBindings.Count > 0);
			}
		}

		public void TestComplianceColumnsOnlyShowForSpecificCountries()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should exists for Italy company", ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should exists for Italy company", ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should exists for Peru company", ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should exists for Peru company", ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should exists for Indonesia company", ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should exists for Indonesia company", ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should exists for VietNam company", ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should exists for VietNam company", ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should exists for China company", ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should exists for China company", ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var control = GetTestFilterControl())
			{
				Assert("The Compliance SubType column should be deleted for Australia company", !ColumnExistsInTheGrid(control.Grid, "AH_ComplianceSubType", ResourceStringData.Empty));
				Assert("The Compliance Number column should be deleted for Australia company", !ColumnExistsInTheGrid(control.Grid, "AH_TransactionReference", ResourceStringData.Empty));
			}
		}

		public void TestOutstandingAmountCaption()
		{
			using (var form = new ZForm())
			using (var control = GetTestFilterControl())
			{
				form.Controls.Add(control);
				form.Show();
				System.Windows.Forms.Application.DoEvents();

				var grid = control.FilteredGrid;
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}

		public void TestMatchStatusAndReasonColumns()
		{
			using (var control = GetTestFilterControl())
			{
				Assert(ColumnExistsInTheGrid(control.Grid, "AH_MatchStatus", ResourceStringData.Empty));
				Assert(ColumnExistsInTheGrid(control.Grid, "AH_MatchStatusReasonCode", ResourceStringData.Empty));
			}
		}

		public void TestTaxBranchColumnVisibilityDependsOnPresentationProvider()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			var presentationProviderMock = GetMockEnquiryFilterControlPresentationProvider();

			AssertTaxBranchColumnVisibility(true);
			AssertTaxBranchColumnVisibility(false);

			void AssertTaxBranchColumnVisibility(bool isTaxBranchColumnVisible)
			{
				presentationProviderMock.Setup(x => x.IsTaxBranchColumnAvailable()).Returns(isTaxBranchColumnVisible);
				using (var control = GetTestFilterControl())
				{
					if (isTaxBranchColumnVisible)
					{
						Assert($"{taxBranchColumn} column should be available in the grid", ColumnExistsInTheGrid(control.Grid, taxBranchColumn, ResourceStringData.Empty));
					}
					else
					{
						Assert($"{taxBranchColumn} column should not be visible", !ColumnExistsInTheGrid(control.Grid, taxBranchColumn, ResourceStringData.Empty));
					}
				}
			}
		}

		public void TestTaxBranchColumnIsVisibilityDependsTaxBranchColumnAvailability()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			GetMockEnquiryFilterControlPresentationProvider().Setup(x => x.IsTaxBranchColumnAvailable()).Returns(true);
			using (var control = GetTestFilterControl())
			{
				Assert($"{taxBranchColumn} column should be visible as default", control.Grid.GetColumnStyle(taxBranchColumn).IsVisible);
			}
		}

		ZBool ColumnExistsInTheGrid(ZGrid grid, ZString columnName, ResourceStringData groupName)
		{
			var columnEnum = grid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == columnName && (groupName.IsEmpty() || column.GroupName.Caption == groupName.Caption))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		void AssertDecimalPlacesForExchangeRateColumn()
		{
			bool isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			ZInt expectedDecimalPlaces = 6;

			using (EnquiryFilterControl testControl = GetTestFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(testControl);
				form.Show();

				ZGridColumnInfo info = testControl.FilteredGrid.GetColumnStyle(AccTransactionHeaderSchema.Constants.AH_ExchangeRate);
				AssertNotNull("Should have found exchange rate column", info);
				ZString message = string.Format("Should be {0} decimal places for {1} Company", expectedDecimalPlaces, isReciprocal ? "Reciprocal" : "Non-Reciprocal");
				AssertEquals(message, expectedDecimalPlaces, ((ZCalcEditColumnStyleInfo)info).Decimals);
			}
		}

		Mock<IEnquiryFilterControlPresentationProvider> GetMockEnquiryFilterControlPresentationProvider()
		{
			var presentationProviderMock = new Mock<IEnquiryFilterControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetEnquiryFilterControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);
			return presentationProviderMock;
		}

		protected abstract EnquiryFilterControl GetTestFilterControl();
	}
}
