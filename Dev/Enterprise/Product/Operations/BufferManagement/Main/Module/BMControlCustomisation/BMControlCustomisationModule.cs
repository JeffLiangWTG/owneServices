using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMControlCustomisationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BMControlCustomisation; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BMControlCustomisation);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BMControlCustomisationFilterControl(GridCollection, (BMControlCustomisationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BMControlCustomisationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMControlCustomisationFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BMControlCustomisation; }
		}

		#region Actions menu

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = base.GetNewActionMenuItems().ToList();
			var cloneMenuItem = new ZMenuItem { Caption = ResString.GetMultilingualString("4cf393d8-68bf-4a00-b9e6-a8e0c7ad63ff", "Clone System Default") };
			menuItems.Add(cloneMenuItem);

			foreach (CodeDescriptionPair controlType in new CustomisedControlTypeList())
			{
				var menuItem = new ZMenuItem { Caption = controlType.MultilingualDescription };
				menuItem.Click += (s, e) => ShowForm(factory => BMControlCustomisation.GetNewDefaultCardLayout(factory, controlType.Code));
				cloneMenuItem.MenuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		static void ShowForm(Func<BusinessObjectFactory, BMControlCustomisation> customisationGetter)
		{
			if (!Env.Security.BMControlCustomisationNew.IsAllowed)
			{
				Env.Security.BMControlCustomisationNew.ShowError();
				return;
			}

			var factory = new BusinessObjectFactory { NameForDebugging = "BMControlCustomisationModule.CloneDetailedCard" };
			var customisation = customisationGetter(factory);
			customisation.AppendNameForClone();

			ZControllerFactory.Create(ControllerIDs.BMControlCustomisation).ShowFormForNewEntity(customisation);
		}

		#endregion
	}
}
