using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsMovementForm))]
	class NctsMovementFormTest : EU.NCTS.GUI.Testing.NctsMovementFormAbstractTest<NctsHeader>
	{
		[RequiresSTA]
		public void TestDeclarationDetailsUserControlType()
		{
			using (var form = new NctsMovementFormForTesting(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabControlExposed.GetTabPage("MainTabPage"));
				AssertType<DeclarationDetailsTabUserControl>(form.DeclarationDetailsTabDynamicUserControlExposed.HostedControl);
			}
		}

		[RequiresSTA]
		public void TestGoodsItemUserControlType()
		{
			using (var form = new NctsMovementFormForTesting(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabControlExposed.GetTabPage("GoodsItemsTabPage"));
				AssertType<NctsGoodsItemsUserControl>(form.GoodsItemsTabDynamicUserControlExposed.HostedControl);
			}
		}

		public void TestSecurityTabUserControlType()
		{
			header.BH_FTZMove = true;
			header.MovementHeader.BM_TypeOfSecurity = "BTH";
			using (var form = new NctsMovementFormForTesting(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabControlExposed.GetTabPage("SecurityTabPage"));

				var securityTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("SecurityTabDynamicUserControl");
				AssertType<SecurityTabUserControl>(securityTabDynamicUserControl.HostedControl);
			}
		}

		public void TestDocDataPlugInForDepartureMovements()
		{
			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new NctsMovementForm(departureNctsHeader))
			{
				AssertNotNull("DocDataPlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestDocDataPlugInForArrivalMovements()
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new NctsMovementForm(arrivalNctsHeader))
			{
				AssertNull("DocDataPlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}

	class NctsMovementFormForTesting : NctsMovementForm
	{
		public NctsMovementFormForTesting(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZDynamicControlCreationUserControl DeclarationDetailsTabDynamicUserControlExposed => DeclarationDetailsTabDynamicUserControl;
		public ZDynamicControlCreationUserControl GoodsItemsTabDynamicUserControlExposed => GoodsItemsTabDynamicUserControl;
		public ZTemplateTabControl MainTabControlExposed => MainTabControl;
	}
}
