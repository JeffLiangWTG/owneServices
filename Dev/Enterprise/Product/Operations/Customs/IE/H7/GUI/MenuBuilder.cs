using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public sealed class MenuBuilder : EU.H7.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm)
			: base(header, mainForm)
		{
		}

		protected override List<ZMenuItem> BuildMenuCore()
		{
			var menuItems = base.BuildMenuCore();
			AddSendRefundApplicationMenuItem(menuItems);

			return menuItems;
		}

		void AddSendRefundApplicationMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("8e936c7d-4c12-4873-8dfa-1b65196ff6d8", "Send Refund Application");
			var menuItemAction = () =>
			{
				if (Header is AsycudaManifestHeader header && CheckBeforeSending())
				{
					var messageSendingParent = header.ApplicationBusinessProvider.GetNewRF415MessageSendingObjectParent((AsycudaManifestHeader)Header);
					using var form = new RF415MessageSendingForm(messageSendingParent);
					ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
				}
			};

			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, menuItemAction, validateManifest: false);
		}

		protected override EU.H7.GUI.MessageSendingForm GetMessageSendingForm(Customs.Business.BaseMessageSendingObjectParent messageSendingParent)
		{
			return new MessageSendingForm(messageSendingParent);
		}

		protected override bool CheckBeforeSending()
		{
			return base.CheckBeforeSending()
				&& (Header.Branch.Company?.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired() ?? false);
		}
	}
}
