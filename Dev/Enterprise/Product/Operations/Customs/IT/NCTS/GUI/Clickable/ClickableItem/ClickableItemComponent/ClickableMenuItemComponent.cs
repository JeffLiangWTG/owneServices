using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class ClickableMenuItemComponent : ClickableItem
{
	public ClickableMenuItemComponent(IClickableContext clickableContext, Action<ZMenuItem, IClickableItem> onCreateMenuItem = null) : base(clickableContext)
	{
		OnCreateMenuItem = onCreateMenuItem ?? ((i, c) => { /* Do nothing */ });
	}

	#region ClickableItem

	protected override ZForm ParentForm => MenuItem.ParentControl?.FindForm() as ZForm;

	#endregion

	#region Implementation

	ZMenuItem GetMenuItem()
	{
		var menuItem = new ZMenuItem(ClickableContext.Caption);
		OnCreateMenuItem(menuItem, this);
		return menuItem;
	}

	public ZMenuItem MenuItem => menuItem ??= GetMenuItem();
	ZMenuItem menuItem;

	Action<ZMenuItem, IClickableItem> OnCreateMenuItem { get; }

	#endregion
}
