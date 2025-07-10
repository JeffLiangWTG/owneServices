using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class ForeignOperatorsUserControlTest : TestCaseWithFactory
	{
		public void TestGridComponents()
		{
			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using var control = new ForeignOperatorsUserControl();
				var visibleColumnsCount = control.ForeignOperatorsGrid.ColumnStyles.Count;
				AssertEquals(4, visibleColumnsCount);

				AssertEquals("Should have IsKnow column in ForeignOperatorsGrid", "IsKnow", ((ZCheckBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("Should have CountryCode column in ForeignOperatorsGrid", "CountryCode", ((ZCodeFindBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[1]).ColumnName);
				AssertEquals("Should have AuthorityCode column in ForeignOperatorsGrid", "AuthorityCode", ((ZTextBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[2]).ColumnName);
				AssertEquals("Should have CGI_CustomsStatusDescription column in ForeignOperatorsGrid", "CGI_CustomsStatusDescription", ((ZTextBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[3]).ColumnName);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using var control = new ForeignOperatorsUserControl();
				var visibleColumnsCount = control.ForeignOperatorsGrid.ColumnStyles.Count;
				AssertEquals(6, visibleColumnsCount);

				AssertEquals("Should have IsKnow column in ForeignOperatorsGrid", "IsKnow", ((ZCheckBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("Should have CGI_BFR_ForeignOperator column in ForeignOperatorsGrid", "CGI_BFR_ForeignOperator", ((ZCodeFindBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[1]).ColumnName);
				AssertEquals("Should have ForeignOperatorName column in ForeignOperatorsGrid", "ForeignOperatorName", ((ZTextBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[2]).ColumnName);
				AssertEquals("Should have CountryCode column in ForeignOperatorsGrid", "CountryCode", ((ZCodeFindBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[3]).ColumnName);
				AssertEquals("Should have AuthorityCode column in ForeignOperatorsGrid", "AuthorityCode", ((ZTextBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[4]).ColumnName);
				AssertEquals("Should have CGI_CustomsStatusDescription column in ForeignOperatorsGrid", "CGI_CustomsStatusDescription", ((ZTextBoxColumnStyleInfo)control.ForeignOperatorsGrid.ColumnStyles[5]).ColumnName);
			}
		}

		public void TestDeleteMenuItemEnabled()
		{
			using var control = new ForeignOperatorsUserControl();
			Assert("AllowReadOnlyRowsToBeDeleted", control.ForeignOperatorsGrid.AllowReadOnlyRowsToBeDeleted);
		}
	}
}
