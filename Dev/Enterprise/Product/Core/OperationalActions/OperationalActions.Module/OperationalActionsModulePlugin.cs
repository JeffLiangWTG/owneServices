using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.ModulePlugIn;

namespace Enterprise.Services.OperationalActions.Module
{
	internal sealed class OperationalActionsModulePlugin : ZModulePlugin
	{
		public OperationalActionsModulePlugin(ITargetRecordSelection moduleSelection, OperationalActionContext context)
		{
			if (moduleSelection == null)
			{
				throw new ArgumentNullException(nameof(moduleSelection));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			this.moduleSelection = moduleSelection;
			this.context = context;

			menuFactory = new BusinessObjectFactory();
			menuFactory.NameForDebugging = "Operational Actions Menu";
		}

		protected override List<MenuItem> GetActionMenuItemToAddCore()
		{
			var menuItems = new List<MenuItem>();
			menuItems.Add(CreateTopLevelMenuItem(moduleSelection));

			OperationalActionsMenuItemGenerator generator = new OperationalActionsMenuItemGenerator(menuFactory, moduleSelection, context);
			menuItems.AddRange(generator.Generate(true));

			return menuItems;
		}

		protected override MenuItem GetButtonGridMenuItemToAddCore(ZModuleButtonGrid grid)
		{
			return CreateTopLevelMenuItem(new GridSelection(grid));
		}

		protected override bool RequiresMultiSelectCore
		{
			get { return true; }
		}

		MenuItem CreateTopLevelMenuItem(ITargetRecordSelection selection)
		{
			var generator = new OperationalActionsMenuItemGenerator(menuFactory, selection, context);
			return ModuleOperationalActionsHelper.GetOperationalActionTopLevelMenuItem(generator);
		}

		readonly OperationalActionContext context;
		readonly ITargetRecordSelection moduleSelection;
		readonly BusinessObjectFactory menuFactory;
	}
}
