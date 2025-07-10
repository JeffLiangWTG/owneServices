using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.PlugIn
{
	[ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
	[ZArchitecture.GUI.Testing.TestExcludeZWinFormHasTypedConstructor]
	sealed partial class ZFormForPlugInTest : ZForm
	{
		public ZFormForPlugInTest(IBusiness businessEntity)
				: base(businessEntity)
		{
			PlugIns.Add(DummyControllerIDs.eDocsPlugInForTesting);
		}

		public ZFormForPlugInTest(IBusiness businessEntity, SecurityCheckpoint checkpoint)
			: base(businessEntity)
		{
			PlugIns.Add(DummyControllerIDs.eDocsPlugInForTesting, checkpoint);
		}

		public eDocsPlugInForTesting GetTestPlugInInstance()
		{
			Assertion.Assert("Precondition: Plug in should exist", PlugIns.Instances.Length > 0);
			Assertion.Assert("Precondition: Plug in should should be eDocsPlugInForTesting", PlugIns.Instances[0] is eDocsPlugInForTesting);

			return PlugIns.Instances[0] as eDocsPlugInForTesting;
		}

		public void ExposeAllTabPages()
		{
			ExposeAllTabPages(this);
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline")]
		void ExposeAllTabPages(Control ctrl)
		{
			foreach (Control nextCtrl in ctrl.Controls)
			{
				ZTabControl tabControl = nextCtrl as ZTabControl;
				if (tabControl != null)
				{
					foreach (ZTabPage page in tabControl.TabPages)
					{
						tabControl.SelectedTab = page;
						Application.DoEvents();
					}
				}

				ExposeAllTabPages(nextCtrl);
			}
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		public StorageMain TopLevelParentMain
		{
			get { return ((eDocsPlugInForTesting)PlugIns.Instances[0]).TopLevelParentMain; }
		}

		public override ContinueWithSave FireSaveButton(object sender = null)
		{
			return ForceSaveToFail ? ContinueWithSave.No : base.FireSaveButton(sender);
		}

		public bool ForceSaveToFail { get; set; }
	}
}
