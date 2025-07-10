using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.GUI;

namespace Enterprise.Services.OperationalActions.Module
{
	internal sealed class OperationalActionsCustomisationMenuMaker : CustomisationMenusMaker<MenuItem>
	{
		public OperationalActionsCustomisationMenuMaker(OperationalActionContext context)
			: base(null, context.Supporter.CustomizationSecurityCheckpoint, new ZDocumentsMenuItemMenuHelper())
		{
			this.context = context;
		}

		protected override IMenuCustomisationForm GetCustomisationForm()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OperationalActionManager manager = new OperationalActionManager(factory, context);
			return new OperationalActionCustomizationForm(manager);
		}

		readonly OperationalActionContext context;
	}
}
