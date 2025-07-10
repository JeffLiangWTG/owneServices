using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	public static class ClientDeviceMenuItemGenerator
	{
		public static IEnumerable<IClickableNestedMenuItem> GetNewMenuWithModelTemplates<TMenuItemType>(IEnumerable<IClickableNestedMenuItem> menuItems, BusinessObjectFactory factory, Action<ClientDeviceHeader> postClickAction = null) where TMenuItemType : IClickableNestedMenuItem
		{
			var myNewMenuItem = (IClickableNestedMenuItem)Activator.CreateInstance<TMenuItemType>();
			myNewMenuItem.Text = "New";
			myNewMenuItem.Image = Icons.GetImage(IconTypes.NewButtonRest);

			var deviceTemplates = new ClientDeviceHeaderTemplateCollection(factory);
			foreach (var model in deviceTemplates.OrderBy(x => x.CDH_ModelID))
			{
				var childMenuItem = (IClickableNestedMenuItem)Activator.CreateInstance<TMenuItemType>();
				childMenuItem.Text = model.CDH_ModelID + " - " + model.CDH_Description;
				childMenuItem.Click += delegate
				{
					var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.ClientDevice);
					if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
					{
						LastControllerForTesting = controller;
					}

					var form = (ZForm)controller.ShowNewForm();
					var newDevice = (ClientDeviceHeader)form.BusinessEntity;
					model.PopulateDeviceFromModel(newDevice);
					postClickAction?.Invoke(newDevice);
				};
				myNewMenuItem.AddChildItem(childMenuItem);
			}

			var result = menuItems.ToList();
			result.Insert(0, myNewMenuItem);
			return result;
		}

		internal static ZController LastControllerForTesting;
	}
}
