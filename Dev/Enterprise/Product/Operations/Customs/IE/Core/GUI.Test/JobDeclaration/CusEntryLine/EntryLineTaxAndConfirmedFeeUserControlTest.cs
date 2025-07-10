using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI.Testing;

class EntryLineTaxAndConfirmedFeeUserControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using var control = new EntryLineTaxAndConfirmedFeeUserControl();
		var grid = control.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid;
		var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();

		AssertEquals("Should have column NationalFeeTypeCode", true, columnNames.Contains(nameof(CusEntryLineFee.NationalFeeTypeCode)));
		AssertEquals("Should have column NationalFeeTypeDescriptionForDisplay", true, columnNames.Contains(nameof(CusEntryLineFee.NationalFeeTypeDescriptionForDisplay)));
	}
}
