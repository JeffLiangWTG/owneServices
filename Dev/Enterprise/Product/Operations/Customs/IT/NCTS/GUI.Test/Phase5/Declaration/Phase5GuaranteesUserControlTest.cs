using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AutoCusBondDetail.Schema;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing.Declaration;

sealed class Phase5GuaranteesUserControlTest : TestCaseWithFactory
{
	public void TestAvailableColumns()
	{
		using (var control = new Phase5GuaranteesUserControl())
		{
			var guaranteesGrid = control.FindSingle<ZGrid>("GuaranteesGrid");
			AssertNotNull(guaranteesGrid);

			var columnNames = guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName);
			var expectedColumnNames = new[]
			{
				PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_RX_NKCurrency, PW_BondNumber2, PW_SuretyCode, PW_Override, PW_BondFiledPort, "OfficeDescription",
			};

			AssertSequencesEqual("Columns", expectedColumnNames, columnNames);
		}
	}
}
