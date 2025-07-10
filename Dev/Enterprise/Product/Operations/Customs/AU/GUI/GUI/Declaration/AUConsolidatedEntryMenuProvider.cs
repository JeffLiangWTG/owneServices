using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	internal class AUConsolidatedEntryMenuProvider : ConsolidatedEntryMenuProvider
	{
		public AUConsolidatedEntryMenuProvider(ZForm parentForm)
			: base(parentForm)
		{
		}

		protected override bool MenuItemsVisible
		{
			get
			{
				var result = false;
				if (declaration != null)
				{
					result = declaration.IsImport
						&& declaration.JE_MessageType != JobMessageTypeList.Codes.ExWarehouse
						&& declaration.JE_MessageSubType != JobDeclaration.MessageSubType.SelfAssessedClearance
						&& declaration.JE_MessageSubType != JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines
						&& base.MenuItemsVisible;
				}

				return result;
			}
		}

		protected override void SetExternalDefaults(ZFilterGridModule filterGridModule)
		{
			filterGridModule.FilterBusinessObject.SetExternalDefaults(DefaultConsolidatedDeclarationFilter.GetDefaultConsolidatedDeclarationFilter(declaration));
		}
	}
}
