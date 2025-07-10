using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	[DoNotPopUpToCheckShortcuts]
	public class TagMenuItemMenuTree : ZMenuItem
	{
		public TagMenuItemMenuTree(ITagMenuTreeViewModel model)
			: base(model.Name)
		{
			this.model = model;

			SetupMenu();
		}

		readonly ITagMenuTreeViewModel model;

		void SetupMenu()
		{
			MenuItems.Add(new ZMenuItem("-"));

			Popup += (e, s) =>
			{
				MenuItems.Clear();
				model.ReloadAllMagnitudes();

				foreach (var definitionMenuItem in model.GetValidDefinitionMenuItems())
				{
					var menuItem = AddTagDefinitionMenuItem(definitionMenuItem);
					if (menuItem.MenuItems.Count > 0)
					{
						MenuItems.Add(menuItem);
					}
				}

				if (model is RemoveTagMenuItemViewModel && this.MenuItems.Count == 0)
				{
					this.MenuItems.Add(new ZMenuItem(Res.GetString("1CB56565-E662-4515-8141-4A5C140370E1", "There are no tags applied")) { Enabled = false });
				}
			};
		}

		ZMenuItem AddTagDefinitionMenuItem(MenuItemDescriptor<TagDefinition> definitionMenuItem)
		{
			var definitionMenu = new ZMenuItem(definitionMenuItem.Text);

			foreach (var magnitudeMenuItem in model.GetValidMagnitudeMenuItems(definitionMenuItem.Payload))
			{
				definitionMenu.MenuItems.Add(new ZMenuItem(magnitudeMenuItem.Text, (s, e) => magnitudeMenuItem.ExecuteAndMaybeShowFormForPayload(s, new MenuItemClickHandlerEventArgs(showPayloadFormModally: true))));
			}

			return definitionMenu;
		}

#if DEBUG
		public void OnPopup()
		{
			base.OnPopup(null);
		}
#endif
	}
}
