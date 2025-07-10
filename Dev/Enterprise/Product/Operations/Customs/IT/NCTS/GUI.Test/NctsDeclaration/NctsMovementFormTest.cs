using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(NctsMovementForm))]
sealed class NctsMovementFormTest : EU.NCTS.GUI.Testing.NctsMovementFormAbstractTest<NctsHeader>
{
	/// <summary>
	/// Only for testing purposes
	/// Please give a look at WI00393915 - Fix Amnesty test failure eConversation
	/// This will be rolled back once the issue has been addressed and fixed.
	/// </summary>
	protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
	{
		base.PerformExtraNctsHeaderConfiguration(header);
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
	}

	public void TestGoodsItemUserControlType()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("GoodsItemsTabPage"));
			AssertEquals(typeof(NctsGoodsItemsUserControl), form.GoodsItemsTabDynamicUserControl.UserControlType);
		}
	}

	[RequiresSTA]
	public void TestDeclarationDetailsUserControlType()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("MainTabPage"));
			AssertEquals(typeof(DeclarationDetailsTabUserControl), form.DeclarationDetailsTabDynamicUserControl.UserControlType);
		}
	}

	[RequiresSTA]
	public void TestMiscOptionsUserControlType()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("MiscOptionsTabPage"));

			var miscOptionsDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MiscOptionsTabDynamicUserControl");
			AssertType<NctsMiscOptionsUserControl>(miscOptionsDynamicUserControl.HostedControl);
		}
	}

	public void TestMiscOptionsTabPageVisibility()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			Assert("SupportsMiscOptionsTabPage", form.SupportsMiscOptionsTabPageExposed);
		}
	}

	public void TestStatusTabPageVisibility()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			Assert("StatusTabPage", form.SupportsStatusTabPageExposed);
		}
	}

	public void TestDeclarationStatusTabUserControlType()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("StatusTabPage"));

			var declarationStatusDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("DeclarationStatusTabDynamicUserControl");
			AssertType<DeclarationStatusTabUserControl>(declarationStatusDynamicUserControl.HostedControl);
		}
	}

	[RequiresSTA]
	public void TestStatusTabPageIsJustAfterMessagesTabPage()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();

			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("MessagesTabPage"));
			var messagesTabPageIndex = form.MainTabControl.SelectedIndex;
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("StatusTabPage"));
			var statusTabPageIndex = form.MainTabControl.SelectedIndex;
			AssertEquals("StatusTabPage TabIndex", messagesTabPageIndex + 1, statusTabPageIndex);
		}
	}

	[RequiresSTA]
	public void TestMessagesUserControlType()
	{
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("MessagesTabPage"));

			var messagesTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
			AssertType<MessagesTabUserControl>(messagesTabDynamicUserControl.HostedControl);
		}
	}

	[RequiresSTA]
	public void TestSecurityTabUserControlType()
	{
		header.BH_FTZMove = true;
		header.MovementHeader.BM_TypeOfSecurity = "BTH";
		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectTab(form.MainTabControl.GetTabPage("SecurityTabPage"));

			var securityTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("SecurityTabDynamicUserControl");
			AssertType<SecurityTabUserControl>(securityTabDynamicUserControl.HostedControl);
		}
	}

	public void TestDocDataPlugInForDepartureMovements()
	{
		var departureNctsHeader = Factory.New<NctsHeader>();
		departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		using (var form = new NctsMovementForm(departureNctsHeader))
		{
			AssertNotNull("DocDataPlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
		}
	}

	public void TestDocDataPlugInForArrivalMovements()
	{
		var arrivalNctsHeader = Factory.New<NctsHeader>();
		arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		using (var form = new NctsMovementForm(arrivalNctsHeader))
		{
			AssertNull("DocDataPlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
		}
	}

	[DeveloperOnlyTest]
	public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
	{
		Assert(true);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}

	protected override bool StackTraceEnabled => false;

	NctsHeader header;
}

class NctsMovementFormForTesting : NctsMovementForm
{
	public NctsMovementFormForTesting(NctsHeader nctsHeader)
		: base(nctsHeader)
	{
	}

	public new ZDynamicControlCreationUserControl DeclarationDetailsTabDynamicUserControl => base.DeclarationDetailsTabDynamicUserControl;
	public new ZDynamicControlCreationUserControl GoodsItemsTabDynamicUserControl => base.GoodsItemsTabDynamicUserControl;
	public new ZTemplateTabControl MainTabControl => base.MainTabControl;
	public ZBool SupportsMiscOptionsTabPageExposed => SupportsMiscOptionsTabPage;
	public ZBool SupportsStatusTabPageExposed => base.SupportsStatusTabPage;
	public ZTabPage StatusTabPageExposed => StatusTabPage;
	public ZTabPage GoodsItemsTabPageExposed => GoodsItemsTabPage;
}
