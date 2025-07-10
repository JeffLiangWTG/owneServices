using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using FreightConsolWrapper = Enterprise.Customs.AU.Declaration.Business.FreightConsolWrapper;

namespace Enterprise.Customs.AU.Declaration.GUI.PlugIn
{
	public class AUCustomsExit2PluginToConsol : CustomsCargoManifestPlugin
	{
		public AUCustomsExit2PluginToConsol(ForwardingConsol consol)
			: base(consol)
		{
		}

		public new ForwardingConsol ManifestProvider => (ForwardingConsol)base.ManifestProvider;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.ExportManifest;

		protected virtual bool ShouldBeSentViaCMR
		{
			get
			{
				FreightConsolWrapper wrapper = new FreightConsolWrapper(ManifestProvider);
				return wrapper.ShouldBeSentViaCMR;
			}
		}

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			if (ShouldBeSentViaCMR)
			{
				return new ESMManifestStatus((ForwardingConsol)manifestProvider);
			}
			else
			{
				return new Exit2ManifestStatus((ForwardingConsol)manifestProvider);
			}
		}

		protected override Control GetNewUserControl() => new ZManifestMessageHistoryUserControl();

		protected override string NameCore => "Customs Export Manifest";

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItem resetManifest = new PluginMenuItem("&Reset Manifest");
			resetManifest.Click += new EventHandler(ResetManifestMenuItem_Click);
			mainMenuItem.MenuItems.Add(resetManifest);

			mainMenuItem.MenuItems.Add("-");
			string transportMode = ManifestProvider != null && ManifestProvider.IsSea ? "Sea" : "Air";
			MenuItem cRNContingencyMenuItem = new PluginMenuItem(transportMode + " Freight &CRN Contingency");
			cRNContingencyMenuItem.Click += new EventHandler(CRNContingencyMenuItem_Click);
			mainMenuItem.MenuItems.Add(cRNContingencyMenuItem);

			MenuItem eDNContingencyMenuItem = new PluginMenuItem(transportMode + " Freight &EDN Contingency");
			eDNContingencyMenuItem.Click += new EventHandler(EDNContingencyMenuItem_Click);
			mainMenuItem.MenuItems.Add(eDNContingencyMenuItem);
		}

		protected void ResetManifestMenuItem_Click(object sender, EventArgs e)
		{
			if (CustomsManifestStatus != null)
			{
				CustomsManifestStatus.ResetToOriginal(new SendsMessagesToCustomsGUI());
			}
		}

		ZForm MainForm => HasUserControl ? (ZForm)((ZUserControl)UserControl).ParentForm : null;

		void EDNContingencyMenuItem_Click(object sender, EventArgs e) => new CMRExportForm(MainForm, new ConsolEDNExporter(ManifestProvider)).Export();

		void CRNContingencyMenuItem_Click(object sender, EventArgs e) => new CMRExportForm(MainForm, new ConsolCRNExporter(ManifestProvider)).Export();

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = ManifestProvider != null
				&& ManifestProvider.JK_RL_NKLoadPort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		[SuppressFormsLocalizedTest]
		class PluginMenuItem : MenuItem
		{
			public PluginMenuItem(string text)
				: base(text)
			{ }
		}
	}
}
