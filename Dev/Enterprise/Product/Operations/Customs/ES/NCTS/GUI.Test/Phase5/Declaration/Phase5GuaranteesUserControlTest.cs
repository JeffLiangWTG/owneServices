using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AutoCusBondDetail.Schema;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class Phase5GuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestPW_BondNumber2Visibility()
		{
			var bondNumber2Info = guaranteesGrid.GetColumnStyle(PW_BondNumber2);
			AssertEquals("IsVisible", false, bondNumber2Info.IsVisible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GuaranteesUserControl();
			guaranteesGrid = userControl.FindSingle<ZGrid>("GuaranteesGrid");
		}
		ZGrid guaranteesGrid;
		Phase5GuaranteesUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
