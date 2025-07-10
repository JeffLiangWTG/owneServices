using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class NctsUserControlForPluginTest : TestCaseWithFactory
	{
		public void TestArrivalUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var arrivalUserControl = control.GetNctsArrivalUserControlExposed())
			{
				AssertType<NctsArrivalUserControl>(arrivalUserControl);
			}
		}

		public void TestSecurityTabUserControlType()
		{
			nctsHeader.BH_FTZMove = true;
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var securityUserControl = control.GetSecurityUserControlExposed())
			{
				AssertType<SecurityTabUserControl>(securityUserControl);
			}
		}

		public void TestDeclarationDetailsTabUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var declarationDetailsTabUserControl = control.GetDeclarationDetailsTabUserControlExposed())
			{
				AssertType<DeclarationDetailsTabUserControl>(declarationDetailsTabUserControl);
			}
		}

		public void TestNctsGoodsItemsUserControlType()
		{
			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			using (var nctsGoodsItemsUserControl = control.GetNctsGoodsItemsUserControlExposed())
			{
				AssertType<NctsGoodsItemsUserControl>(nctsGoodsItemsUserControl);
			}
		}

		[RequiresSTA]
		public void TestShouldCreateUnloadingRemarksUserControlArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			Factory.Save();

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");
				AssertEquals("It should be true for ES Arrival", true, control.ShouldCreateUnloadingRemarksUserControlExposed);
			}
		}

		[RequiresSTA]
		public void TestShouldCreateUnloadingRemarksUserControlDeparture()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var control = new NctsUserControlForPluginForTest(nctsHeader))
			{
				control.SetDataBinding(nctsHeader, "");
				AssertEquals("It should be false for ES Departure", false, control.ShouldCreateUnloadingRemarksUserControlExposed);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}

	class NctsUserControlForPluginForTest : NctsUserControlForPlugin
	{
		public NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		public bool ShouldCreateUnloadingRemarksUserControlExposed => ShouldCreateUnloadingRemarksUserControl;

		public EU.NCTS.GUI.NctsArrivalUserControl GetNctsArrivalUserControlExposed() => GetNctsArrivalUserControl();

		public EU.NCTS.GUI.SecurityTabUserControl GetSecurityUserControlExposed() => GetSecurityUserControl();

		public EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControlExposed() => GetDeclarationDetailsTabUserControl();

		public EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControlExposed() => GetNctsGoodsItemsUserControl();
	}
}
