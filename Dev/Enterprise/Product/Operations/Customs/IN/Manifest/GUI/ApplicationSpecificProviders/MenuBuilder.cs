using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

public sealed class MenuBuilder : ASYCUDA.GUI.MenuBuilder
{
	public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	public override ResourceString MenuCaption => ResString.GetMultilingualString("88CBE1BD-61BD-45BB-B077-2EA13C240160", "IN Manifest");

	public override ZMenuItem[] BuildMenu()
	{
		var menuItems = new List<ZMenuItem>();
		if (IsValidForMessage())
		{
			var sendMessasgeCaption = ResString.GetMultilingualString("6C9FD524-EE09-4C18-A9C4-344873534259", "ICEGATE - Send Electronically");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, sendMessasgeCaption, Header, SendManifestMessage, false);
		}
		else
		{
			var menuItem = GetInvalidMessageMenuItem();
			menuItems.Add(menuItem);
		}

		return menuItems.ToArray();
	}

	void SendManifestMessage()
	{
		if (SaveDataFirst.Confirm(Header, mainForm))
		{
			var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
			SendMessageHelper.GenerateMessage(sendingObjectParent, () => new SendMessageForm(sendingObjectParent));
		}
	}
}
