using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestTaxAndFeeGrid()
		{
			using (var userControl = new EntryLineAdditionalDataUserControl())
			{
				var grid = userControl.EntryLineDutyAndTaxGrid;
				CombineAssertions(() =>
				{
					AssertColumnStyle<ZDropEditColumnStyleInfo>("Type", grid, "CF_ChargeType");
					AssertColumnStyle<ZTextBoxColumnStyleInfo>("Description", grid, "ChargeTypeDescription");
					AssertColumnStyle<ZCalcEditColumnStyleInfo>("Base Amount", grid, "CF_BaseValue");
					AssertColumnStyle<ZDropEditColumnStyleInfo>("Method of Calculation", grid, "CF_MethodOfCalculation");
					AssertColumnStyle<ZCalcEditColumnStyleInfo>("Tax Rate", grid, "CF_Rate");
					AssertColumnStyle<ZCalcEditColumnStyleInfo>("Total Amount", grid, "CF_ChargeAmount");
					AssertColumnStyle<ZTextBoxColumnStyleInfo>("Method of Payment", grid, "CF_MethodOfPayment");
				});
			}
		}

		void AssertColumnStyle<T>(ZString message, ZGrid grid, ZString columnName) where T : ZGridColumnInfo
		{
			var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName);

			AssertNotNull(message, column);
			AssertType<T>(column);
		}

		public void TestExtendedInformationTabs()
		{
			using (var userControl = new EntryLineAdditionalDataUserControl())
			{
				AssertEquals(2, userControl.ExtendInfoTabControl.TabCount);
			}
		}

		public void TestDataSourceType()
		{
			using (var control = new EntryLineAdditionalDataUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}
	}
}
