using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Wizards.EIDR;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.Chief.Declaration;
using Enterprise.Customs.GB.GUI.Wizards;
using Enterprise.Customs.GB.MCP.MessageSenders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class EDIMenu : EU.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected override void JobDeclarationChangedCore(BaseJobDeclaration oldValue, BaseJobDeclaration newValue)
		{
			base.JobDeclarationChangedCore(oldValue, newValue);
			chiefMenu.Declaration = Declaration;
			cdsMenu.Declaration = Declaration;
			pentantMenu.Declaration = Declaration;
		}

		ChiefEDIMenu chiefMenu;
		CDSEDIMenu cdsMenu;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Add("-");

			chiefMenu = new ChiefEDIMenu(true);
			MenuItem cfspMenu = new ZMenuItem(CfspWizardCaption);
			var wizardsMenuItem = new ZMenuItem(WizardsCaption);
			MenuItems.Add(chiefMenu);
			cdsMenu = new CDSEDIMenu();
			MenuItems.Add(cdsMenu);

			pentantMenu = new GBViaPentantMenu();
			MenuItems.Add(pentantMenu);

			cfspMenu.MenuItems.Add(new ZMenuItem(CDSFsdWizardCaption, CDSFsdWizardEntry_Click));

			jobDeclarationWizardMenuItem = new ZMenuItem(JobDeclarationWizardCaption, JobDecWizard_Click);

			if (SADHDataEntryFormMenuItem != null && SingleLineEntryMenuItem != null)
			{
				// Move the two EU ones from root into \Wizards\ and add the two CFSP ones there too
				MenuItems.Remove(SADHDataEntryFormMenuItem);
				MenuItems.Remove(SingleLineEntryMenuItem);
				wizardsMenuItem.MenuItems.Add(SADHDataEntryFormMenuItem);
				wizardsMenuItem.MenuItems.Add(SingleLineEntryMenuItem);
				wizardsMenuItem.MenuItems.Add(cfspMenu);
				wizardsMenuItem.MenuItems.Add(new ZMenuItem(EidrWizardCaption, EidrWizard_Click));
				wizardsMenuItem.MenuItems.Add(jobDeclarationWizardMenuItem);
				MenuItems.Add(wizardsMenuItem);
			}
			else
			{
				MenuItems.Add(cfspMenu);
			}

			claimUCNMenuItem = new ZMenuItem(ClaimUCNCaption, ClaimUCN_Click);
			MenuItems.Add(claimUCNMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = Declaration;
			var isDeclarationIntegrated = declaration?.IsDeclarationIntegrated ?? false;
			var chiefMenuVisible = declaration?.ApplicationExtender is ChiefApplicationExtender;
			chiefMenu.Visible = !isDeclarationIntegrated && chiefMenuVisible;
			cdsMenu.Visible = !isDeclarationIntegrated && !chiefMenuVisible;
			jobDeclarationWizardMenuItem.Visible = cdsMenu.Visible;
			pentantMenu.Visible = !isDeclarationIntegrated && (declaration?.IsPentant ?? false);
			chiefMenu.RefreshMenu();
			cdsMenu.RefreshMenu();

			var containersCount = declaration?.CusContainers?.Count ?? 0;
			claimUCNMenuItem.Visible = !isDeclarationIntegrated && (declaration?.IsMCP ?? false) && containersCount >= 1;
			claimUCNMenuItem.Text = containersCount <= 1 ? ClaimUCNCaption : ClaimUCNAmalgamateCaption;
		}

		void ClaimUCN_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				if (FormInternal.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes)
				{
					var messageSender = new UCNMessageSender(Declaration);
					var result = messageSender.SendMessage();
					Declaration.Factory.Save();
					Globals.Message.Show(result);
				}
			}
		}

		void CDSFsdWizardEntry_Click(object sender, EventArgs e)
		{
			var cdsFsdWizardManager = new CDSFinalSupplementaryDeclarationHelperManager(Declaration);
			ZFormModaliser.ShowDialogAndDispose(new CdsFsdWizardForm(cdsFsdWizardManager));
		}

		void EidrWizard_Click(object sender, EventArgs e)
		{
			var eidrWizardManager = new EidrWizardManager(Declaration);
			ZFormModaliser.ShowDialogAndDispose(new EidrWizardForm(eidrWizardManager));
		}

		void JobDecWizard_Click(object sender, EventArgs e)
		{
			DeclarationWizardGuiHelper.WizardClicked(Declaration);
		}

		public const string CDSFsdWizardCaption = "Create CDS FSD type 'Q' declaration";
		public const string CfspWizardCaption = "CFSP";
		public const string IcsGroupCaption = "(Dev only) ICS (entry summary declarations)";
		public const string IcsSendEnsAmendment = "Send amended ENS (IE313)";
		public const string IcsSendENS = "Send ENS (IE315)";
		public const string IcsSendDiversionMrn = "Divert by MRN (IE323)";
		public const string IcsSendDiversionVessel = "Divert entire vessel (IE323)";
		public const string IcsSendArrival = "Arrive entire vessel (IE347)";
		public const string WizardsCaption = "Wizards";
		public const string JobDeclarationWizardCaption = "Help me make a CDS declaration";
		public const string EidrWizardCaption = "Create EIDR (Entry In Declarant's Records)";
		public const string ClaimUCNCaption = "Claim UCN via MCP Destin8";
		public const string ClaimUCNAmalgamateCaption = "Claim UCN via MCP Destin8 (Amalgamate)";

		protected virtual ZForm FormInternal => Form;

		protected ZMenuItem suppDecWizardMenu;
		protected MenuItem jobDeclarationWizardMenuItem;
		GBViaPentantMenu pentantMenu;
		ZMenuItem claimUCNMenuItem;
	}
}
