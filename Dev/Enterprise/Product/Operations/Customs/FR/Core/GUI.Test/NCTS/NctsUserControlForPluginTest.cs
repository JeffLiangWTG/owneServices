using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	public class NctsUserControlForPluginTest : TestCaseWithFactory
	{
		public void TestArrivalUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var arrivalUserControl = control.GetNctsArrivalUserControl())
			{
				AssertType<NctsArrivalUserControl>(arrivalUserControl);
			}
		}

		public void TestDeclarationDetailsTabUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var departureUserControl = control.GetDeclarationDetailsTabUserControl())
			{
				AssertType<DeclarationDetailsTabUserControl>(departureUserControl);
			}
		}

		public void TestNctsGoodsItemsUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var nctsGoodsItemsUserControl = control.GetNctsGoodsItemsUserControl())
			{
				AssertType<NctsGoodsItemsUserControl>(nctsGoodsItemsUserControl);
			}
		}

		public void TestUnloadingRemarksUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var nctsGoodsItemsUserControl = control.GetUnloadingRemarksUserControl())
			{
				AssertType<FRUnloadingRemarksUserControl>(nctsGoodsItemsUserControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}

	class NctsUserControlForPluginForTest : NctsUserControlForPlugin
	{
		public NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		public new EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControl() => base.GetDeclarationDetailsTabUserControl();

		public new EU.NCTS.GUI.NctsArrivalUserControl GetNctsArrivalUserControl() => base.GetNctsArrivalUserControl();

		public new EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControl() => base.GetNctsGoodsItemsUserControl();

		public new EU.NCTS.GUI.UnloadingRemarksUserControl GetUnloadingRemarksUserControl() => base.GetUnloadingRemarksUserControl();
	}
}
