using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ExitControlMenuItem : ZMenuItem
{
	public ExitControlMenuItem()
	{
		CaptionResourceString = Res.GetData("23243606-ECC5-4C04-A6CC-1A9E34A76C57", "E&xit Control");
	}

	public CusExitHeader ExitHeader
	{
		get { return exitHeader; }
		set
		{
			if (exitHeader != value)
			{
				exitHeader = value;
				SetupMenuItems();
			}
		}
	}
	CusExitHeader exitHeader;

	public const string SendToCustomsMenuItemSeparatorMenuItemName = "SendToCustomsMenuItemSeparatorMenuItem";
	public static ZMenuItem CreateSendToCustomsMenuItemSeparatorMenuItem() => new ZMenuItem("-") { Name = SendToCustomsMenuItemSeparatorMenuItemName };

	internal void InsertBeforeSendToCustomsMenuItem(ZMenuItem[] menuItems)
	{
		if (menuItems == null || menuItems.Length == 0 || ExitHeader == null)
		{
			return;
		}

		var exitControlMenuItems = MenuItems;
		var notMatchedMenuItems = new List<System.Windows.Forms.MenuItem>();
		menuItems.ForEach(menuItem =>
		{
			var index = exitControlMenuItems.IndexOfKey(menuItem.Name);
			if (index == -1)
			{
				notMatchedMenuItems.Add(menuItem);
			}
			else
			{
				exitControlMenuItems.InsertRange(index + 1, new[] { menuItem });
				exitControlMenuItems.RemoveAt(index);
			}
		});
		var separatorIndex = exitControlMenuItems.IndexOfKey(SendToCustomsMenuItemSeparatorMenuItemName);
		if (separatorIndex == -1)
		{
			var sendToCustomsMenuIteIndex = exitControlMenuItems.IndexOfKey(ExitControlSendToCustomsMenuCreator.SendToCustomsMenuItemName);
			if (sendToCustomsMenuIteIndex > -1)
			{
				exitControlMenuItems.InsertRange(sendToCustomsMenuIteIndex, new[] { CreateSendToCustomsMenuItemSeparatorMenuItem() });
				separatorIndex = sendToCustomsMenuIteIndex;
			}
		}
		if (separatorIndex > -1)
		{
			exitControlMenuItems.InsertRange(separatorIndex, notMatchedMenuItems);
		}
		else
		{
			exitControlMenuItems.AddRange(notMatchedMenuItems.ToArray());
		}
	}

	void SetupMenuItems()
	{
		if (ExitHeader is CusExitHeader header)
		{
			if (CreateMenuItems(header) is ZMenuItem[] menuItems)
			{
				MenuItems.AddRange(menuItems);
			}
		}
	}

	IReadOnlyList<ZMenuItem> CreateMenuItems(CusExitHeader header)
	{
		IReadOnlyList<ZMenuItem> menuItems;
		if (ExitControlMenuProviderManager.GetMenuProvider(header.CountryCode) is IExitControlMenuProvider menuProvider)
		{
			menuItems = menuProvider.GetExitControlMainMenuProvider(header).AdditionalMainMenuItems;
		}
		else
		{
			menuItems = new[] { new ExitControlSendToCustomsMenuCreator(header).Create() };
		}
		return menuItems;
	}
}
