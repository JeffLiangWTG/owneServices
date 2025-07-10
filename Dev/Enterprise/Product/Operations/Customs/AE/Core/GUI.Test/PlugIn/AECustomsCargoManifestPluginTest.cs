using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.GUI.PlugIn.Testing;

class AECustomsCargoManifestPluginTest : ZPlugInGenericTest
{
	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
	{
		return new AECustomsCargoManifestPlugin(Factory.New<ForwardingConsol>());
	}

	public void TestDeclarareManifest()
	{
		Env.Registry.AECustoms.CourierID = "";
		var consol = Factory.New<ForwardingConsol>();
		using (var plugin = new AECustomsCargoManifestPluginTestClass(consol))
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			plugin.Menu.MenuItems[0].PerformClick();
			AssertEquals("Please set a value in Config > System > Registry > Customs > UAE > Clearing Agent Code", UnitTestUserNotification.Instance.LastMessage.Text);
			Env.Registry.AECustoms.CourierID = "12345";
			plugin.Menu.MenuItems[0].PerformClick();
			AssertEquals("Unable to send due to the following errors:\n\nCan't allocate the Agent's Reference Number from ", UnitTestUserNotification.Instance.LastMessage.Text);
			consol.JK_UniqueConsignRef = "C000001";
			plugin.Menu.MenuItems[0].PerformClick();
			AssertEquals("You must save the current record before generating a message", UnitTestUserNotification.Instance.LastMessage.Text);
			Factory.Save();
			plugin.Menu.MenuItems[0].PerformClick();
			if (File.Exists(plugin.TestFileName))
			{
				Assert(true);
				File.Delete(plugin.TestFileName);
			}
			else
			{
				Fail("File is not Created.");
			}
		}
	}

	public void TestLoadZPlugIn()
	{
		using (AECustomsCargoManifestPlugin plugin = new AECustomsCargoManifestPlugin(Factory.New<ForwardingConsol>()))
		{
			AssertNull(plugin.UserControl);
			AssertNotNull(plugin.TopLevelMenu);
			AssertEquals("Customs Manifest", plugin.TopLevelMenu.Text);
			AssertEquals("Customs Manifest", plugin.Name);
		}
	}

	public void TestChangeTheVisibility()
	{
		GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedArabEmirates);
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
		using (var plugIn = new AECustomsCargoManifestPluginTestClass(consol))
		{
			consol.JK_RL_NKLoadPort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.Ukraine);
			consol.JK_RL_NKDischargePort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.UnitedArabEmirates);
			AssertEquals("Consol is Import to AE so should be visible", true, plugIn.Enabled);
			consol.JK_RL_NKLoadPort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.UnitedArabEmirates);
			consol.JK_RL_NKDischargePort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.Australia);
			AssertEquals("Shouldn't be visible as going from AE", false, plugIn.Enabled);
			consol.JK_RL_NKLoadPort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.Ukraine);
			consol.JK_RL_NKDischargePort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.Australia);
			AssertEquals("TransShipment for AE so should be visible", true, plugIn.Enabled);
			consol.JK_RL_NKLoadPort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.UnitedArabEmirates);
			consol.JK_RL_NKDischargePort = GetAnyUNLOCOCode(Core.Constants.CountryCodes.UnitedArabEmirates);
			AssertEquals("Domestic consol from AE port to AE port so should be visible", true, plugIn.Enabled);
		}
	}

	ZString GetAnyUNLOCOCode(ZString countryCode)
	{
		RefUNLOCO unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode));
		return unloco == null ? ZString.Empty : unloco.Code;
	}
}

class AECustomsCargoManifestPluginTestClass : AECustomsCargoManifestPlugin
{
	public AECustomsCargoManifestPluginTestClass(ForwardingConsol consol) : base(consol) { }

	protected override MenuItem GetNewTopLevelMenu()
	{
		Menu = base.GetNewTopLevelMenu();
		return Menu;
	}
	public MenuItem Menu;

	protected override DialogResult ShowSaveDialog(ZSaveFileDialog dialog)
	{
		dialog.FileName = TestFileName;
		return DialogResult.OK;
	}

	public string TestFileName = Path.Combine(EnvProxy.Instance.TempPath, "test.man");
}
