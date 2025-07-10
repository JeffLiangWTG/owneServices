using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsUserControlForPluginTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestMessagesUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var messagesUserControl = control.GetMessagesUserControlExposed())
		{
			AssertType<MessagesTabUserControl>(messagesUserControl);
		}
	}

	public void TestGetDeclarationDetailsTabUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var declarationDetailsTabUserControl = control.GetDeclarationDetailsTabUserControlExposed())
		{
			AssertType<DeclarationDetailsTabUserControl>(declarationDetailsTabUserControl);
		}
	}

	public void TestGetNctsGoodsItemsUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var nctsGoodsItemsUserControl = control.GetNctsGoodsItemsUserControlExposed())
		{
			AssertType<NctsGoodsItemsUserControl>(nctsGoodsItemsUserControl);
		}
	}

	public void TestGetSecurityUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var securityUserControl = control.GetSecurityUserControlExposed())
		{
			AssertType<SecurityTabUserControl>(securityUserControl);
		}
	}

	public void TestGetMiscOptionsUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var miscOptionsUserControl = control.GetMiscOptionsUserControlExposed())
		{
			AssertType<NctsMiscOptionsUserControl>(miscOptionsUserControl);
		}
	}

	public void TestGetDeclarationStatusUserControl()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		using (var declarationStatusUserControl = control.GetDeclarationStatusUserControlExposed())
		{
			AssertType<DeclarationStatusTabUserControl>(declarationStatusUserControl);
		}
	}

	public void TestSupportsMiscOptionsTabPage()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		{
			AssertEquals("SupportsMiscOptionsTabPage", true, control.SupportsMiscOptionsTabPageExposed);
		}
	}

	public void TestSupportsStatusTabPage()
	{
		using (var control = new NctsUserControlForPluginForTest(nctsHeader))
		{
			AssertEquals("SupportsStatusTabPage", true, control.SupportsStatusTabPageExposed);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}
	NctsHeader nctsHeader;
}

class NctsUserControlForPluginForTest : NctsUserControlForPlugin
{
	public NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
	{
	}

	public EU.NCTS.GUI.MessagesTabUserControl GetMessagesUserControlExposed() => GetMessagesUserControl();

	public EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControlExposed() => GetDeclarationDetailsTabUserControl();

	public EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControlExposed() => GetNctsGoodsItemsUserControl();

	public EU.NCTS.GUI.SecurityTabUserControl GetSecurityUserControlExposed() => GetSecurityUserControl();

	public EU.NCTS.GUI.MiscOptionsUserControl GetMiscOptionsUserControlExposed() => GetMiscOptionsUserControl();

	public EU.NCTS.GUI.DeclarationStatusTabUserControl GetDeclarationStatusUserControlExposed() => GetDeclarationStatusUserControl();

	public ZBool SupportsMiscOptionsTabPageExposed => SupportsMiscOptionsTabPage;

	public ZBool SupportsStatusTabPageExposed => SupportsStatusTabPage;
}
