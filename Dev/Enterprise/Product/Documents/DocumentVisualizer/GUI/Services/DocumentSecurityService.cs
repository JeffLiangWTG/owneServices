using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed public class DocumentSecurityService : IDocumentSecurityService
	{
		public DocumentSecurityService(IStmMenuItem menuItem, ModuleIdentifier moduleID)
		{
			Argument.NotNull(menuItem, nameof(menuItem));

			this.menuItem = menuItem;
			this.moduleID = moduleID;
		}

		readonly IStmMenuItem menuItem;
		readonly ModuleIdentifier moduleID;

		#region Checkpoints

		ISecurityCheckpoint MenuItemCheckpoint
		{
			get
			{
				if (menuItemCheckpoint == null && moduleID != null)
				{
					using (var module = ZModuleFactory.Instance.Create(moduleID))
					{
						if (module != null && module.SecurityCheckpoint != Env.Security.None)
						{
							var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(moduleID, module.SecurityCheckpoint);
							menuItemCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, moduleID, parentCheckpoint);
						}
					}
				}

				return menuItemCheckpoint;
			}
		}

		ISecurityCheckpoint menuItemCheckpoint;

		ISecurityCheckpoint ModifyCheckpoint
		{
			get
			{
				if (modifyCheckpoint == null && MenuItemCheckpoint != null)
				{
					modifyCheckpoint = Env.Security.FindOrCreateVisualizerFormModifyCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, moduleID, MenuItemCheckpoint);
				}

				return modifyCheckpoint;
			}
		}

		ISecurityCheckpoint modifyCheckpoint;

		ISecurityCheckpoint DeliveryCheckpoint
		{
			get
			{
				if (deliveryCheckpoint == null && MenuItemCheckpoint != null)
				{
					deliveryCheckpoint = Env.Security.FindOrCreateVisualizerFormDeliveryCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, moduleID, MenuItemCheckpoint);
				}

				return deliveryCheckpoint;
			}
		}

		ISecurityCheckpoint deliveryCheckpoint;

		ISecurityCheckpoint SendMessageCheckpoint
		{
			get
			{
				if (sendMessageCheckpoint == null && MenuItemCheckpoint != null)
				{
					sendMessageCheckpoint = Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, moduleID, MenuItemCheckpoint);
				}

				return sendMessageCheckpoint;
			}
		}

		ISecurityCheckpoint sendMessageCheckpoint;

		#endregion

		#region IMenuItemSecurityHelper

		public bool CanView
		{
			get { return MenuItemCheckpoint == null || MenuItemCheckpoint.IsAllowed; }
		}

		public bool CanModify
		{
			get { return ModifyCheckpoint == null || ModifyCheckpoint.IsAllowed; }
		}

		public bool CanDeliver
		{
			get { return DeliveryCheckpoint == null || DeliveryCheckpoint.IsAllowed; }
		}

		public bool CanSendMessage
		{
			get { return SendMessageCheckpoint == null || SendMessageCheckpoint.IsAllowed; }
		}

		public ZString CannotSendMessageError
		{
			get { return SendMessageCheckpoint?.ErrorMessageForNotAllowed; }
		}

		public bool AllowToolsAccess
		{
			get { return Env.Security.AllowToolsAccess.IsAllowed; }
		}

		public void ShowViewError()
		{
			MenuItemCheckpoint.ShowError();
		}

		public void ShowModifyError()
		{
			ModifyCheckpoint.ShowError();
		}

		public void ShowDeliveryError()
		{
			DeliveryCheckpoint.ShowError();
		}

		public void ShowSendMessageError()
		{
			SendMessageCheckpoint.ShowError();
		}

		public void ShowAllowToolsAccessError()
		{
			Env.Security.AllowToolsAccess.ShowError();
		}

		#endregion
	}
}
