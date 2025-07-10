using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSDeclarationForm : ZForm, ITabVisibilityDeciderPersistence
	{
		public EMCSDeclarationForm()
		{
		}

		public EMCSDeclarationForm(EMCSJobDeclaration declaration)
			: base(declaration)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl);
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), GetNewTopLevelMenu(declaration));
			AddPlugins();
			AddScreeningLogsMenuAndTabPage();
		}

		public override string FormCaption
		{
			get
			{
				string caption = Res.GetString("814F17A7-DAAA-4546-A825-9B1F472F8B1E", "EMCS Declaration");

				if (!this.IsDesignMode() && !Declaration.JE_DeclarationReference.IsEmpty)
				{
					caption += " - " + Declaration.JE_DeclarationReference;
				}
				return caption;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, TopLevelTabControl).Enabled = true;
			}
		}

		ZMenuItem GetNewTopLevelMenu(EMCSJobDeclaration declaration)
		{
			if (declaration != null && declaration.JE_IsCancelled)
			{
				var result = (ZMenuItem)new EDIMenuStub();
				result.CaptionResourceString = EMCSMenu.TopLevelMenuCaptionResourceStringData;
				return result;
			}
			else
			{
				var provider = EMCSTopMenuProvider.GetTopMenuProvider(declaration.GetDefaultDataGroupingCode());
				return provider.TopLevelMenu(declaration);
			}
		}

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(Declaration.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		protected override ZTabControl TopLevelTabControl => EMCSCustomsBrokerageUserControl.MainTabControl;

		EMCSJobDeclaration Declaration => (EMCSJobDeclaration)BusinessEntity;

		void ITabVisibilityDeciderPersistence.StoreTabVisible(ZTabPage page)
		{
			this.StoreTabVisible(page, Declaration, JobDeclarationSchema.JE_InvisibleTabsXML);
		}

		bool ITabVisibilityDeciderPersistence.RetrieveTabPageVisible(ZTabPage page)
		{
			return this.RetrieveTabPageVisible(page, Declaration, JobDeclarationSchema.JE_InvisibleTabsXML);
		}

		bool ITabVisibilityDeciderPersistence.HasTabVisiblePersisted => !Declaration?.JE_InvisibleTabsXML.IsEmpty ?? false;

		void AddScreeningLogsMenuAndTabPage()
		{
			new DeniedPartyScreeningPresentationManager().CreateMenusForJob(this);
			if (!EMCSCustomsBrokerageUserControl.EventTabPage.IsDisposed)
			{
				var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				screenStatusControl.SetBindingMember("RelatedOrgPartyScreeningStatusCollection");
				EMCSCustomsBrokerageUserControl.EventTabPage.AddAdditionalTab(Res.GetString("E5D3D6AC-38B7-4549-B2FE-2502C6D36DF4", "Denied Party Screening Logs"), screenStatusControl);
			}
		}

		protected virtual EMCSMenu TopLevelMenu => new EMCSMenu(Declaration);
	}
}
