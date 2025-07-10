using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI
{
	public partial class MonthlyClosingForm : ZTemplateForm
	{
		public MonthlyClosingForm(CusReconDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			AddMessagingMenu();
			SetupActionsMenu();
		}

		public override string FormCaption => Declaration.HumanReadableName;

		CusReconDeclaration Declaration => (CusReconDeclaration)BusinessEntity;

		void AddMessagingMenu()
		{
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), new MonthlyClosingMessagingMenu(Declaration));
		}

		void SetupActionsMenu()
		{
			var linkSimplifiedDeclarationsMenuItem = new ZMenuItem(ResString.GetMultilingualString("7140A230-3BC2-46C5-AA89-E0F010CF918F", "Link Simplified Declarations"), LinkSimplifiedDeclarationsMenuItem_Click);
			linkSimplifiedDeclarationsMenuItem.Enabled = !Declaration.IsFinalized;
			ActionsMenuItem.MenuItems.Add(linkSimplifiedDeclarationsMenuItem);
		}

		void LinkSimplifiedDeclarationsMenuItem_Click(object sender, EventArgs e)
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.SimplifiedDeclaration) as ZFilterGridModule)
			{
				module.SetFormsModalTo(this);
				var moduleGridCollection = (ActiveBusinessObjectCollection<CusReconEntry>)module.GridCollection;
				LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(moduleGridCollection, Declaration);

				using (var simplifiedDeclarationPopup = module.ShowPopup() as ZForm)
				{
					ZFormModaliser.ShowDialogWithoutDispose(simplifiedDeclarationPopup, this);
				}
			}
		}
	}
}
