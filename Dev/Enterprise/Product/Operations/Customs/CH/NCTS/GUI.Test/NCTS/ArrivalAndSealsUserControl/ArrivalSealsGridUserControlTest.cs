using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSeal.Schema;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class ArrivalSealsGridUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(CusSealCollection), userControl.BindingSource.DataSourceType);
	}

	public void TestAvailableColumns()
	{
		AssertSequencesEqual("Columns", new[] { BK_SequenceNumber, BK_UnloadingState, BK_SealNumber, "UnloadingRemarksText" },
			sealsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new ArrivalSealsGridUserControl();
		sealsGrid = userControl.SealsGrid;
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	ArrivalSealsGridUserControl userControl;
	ZGrid sealsGrid;
}
