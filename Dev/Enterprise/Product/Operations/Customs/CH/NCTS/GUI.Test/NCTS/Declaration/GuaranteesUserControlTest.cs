using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AutoCusBondDetail.Schema;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GuaranteesUserControl))]
sealed class GuaranteesUserControlTest : TestCaseWithFactory
{
	public void TestAvailableColumns()
	{
		var guaranteesGrid = UserControl.FindSingle<ZGrid>("GuaranteesGrid");
		AssertSequencesEqual("Columns", new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_RX_NKCurrency, PW_BondNumber2, PW_Override }, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	GuaranteesUserControl UserControl => userControl ??= new GuaranteesUserControl();
	GuaranteesUserControl userControl;

	protected override void TearDown()
	{
		base.TearDown();
		userControl?.Dispose();
	}
}
