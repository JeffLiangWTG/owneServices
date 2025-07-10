using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class MessagesUserControl : AdditionalTabPageUserControlBase, IAdditionalTabPage
	{
		public MessagesUserControl()
		{
			InitializeComponent();
			CustomizeLayout();
			AddMessageGridContextMenuItem();
		}

		void CustomizeLayout() => CustomizeLayoutCore();

		protected virtual void CustomizeLayoutCore() { }

		void AddMessageGridContextMenuItem()
		{
			var queryInterchangeCreator = new QueryInterchangeCreator(messagesGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();
			messagesGrid.ContextMenu.Popup += (s, e) =>
			{
				var message = CurrentSelectedMessage;
				SetMessagesGridExtraContextMenuItemsVisibility(message);
			};
		}

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			if (provider != null)
			{
				SetMessagesGridExtraColumnInfosVisibility(provider);
				SetupMessagesGridExtraContextMenuItems(provider);
			}
		}

		protected override void HookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			base.HookManifestHeaderEventsCore(header);
			header.OnNatureOrCountryChanged += ManifestHeader_OnNatureOrCountryChanged;
		}

		protected override void UnHookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			base.UnHookManifestHeaderEventsCore(header);
			header.OnNatureOrCountryChanged -= ManifestHeader_OnNatureOrCountryChanged;
		}

		void ManifestHeader_OnNatureOrCountryChanged(object sender, EventArgs e)
		{
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
			SetUserFullNameVisibility(provider);
		}

		EDIMessage CurrentSelectedMessage => messagesGrid.GetCurrent() as EDIMessage;

		void SetUserFullNameVisibility(ApplicationGUIProvider provider)
		{
			messagesGrid.GetColumnStyle("EM_CreateUserFullName").IsVisible = provider.IsMessageGridUserFullNameVisible();
		}

		void SetupMessagesGridExtraContextMenuItems(ApplicationGUIProvider provider)
		{
			var contextMenu = messagesGrid.ContextMenu;

			if (contextMenu != null)
			{
				if (messagesGridExtraMenuItems != null)
				{
					var messagesGridMenuItems = contextMenu.MenuItems;
					foreach (var menuItem in messagesGridExtraMenuItems)
					{
						messagesGridMenuItems.Remove(menuItem);
					}
				}
				messagesGridExtraMenuItems = provider?.GetMessagesGridExtraMenuItems(messagesGrid, Header).ToArray();
				if (messagesGridExtraMenuItems != null && messagesGridExtraMenuItems.Length > 0)
				{
					contextMenu.MenuItems.AddRange(messagesGridExtraMenuItems);
				}
			}
		}
		ZMenuItem[] messagesGridExtraMenuItems;

		void SetMessagesGridExtraContextMenuItemsVisibility(EDIMessage message)
		{
			if (Header != null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
				provider?.SetMessagesGridExtraMenuItemsVisibility(messagesGridExtraMenuItems, message);
			}
		}

		void SetMessagesGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			using (messagesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (messagesGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in messagesGridExtraColumnInfos)
					{
						messagesGrid.ColumnStyles.Remove(columnInfo);
					}
				}
				messagesGridExtraColumnInfos = provider?.GetMessagesGridExtraColumnInfos().ToArray();
				if (messagesGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in messagesGridExtraColumnInfos)
					{
						messagesGrid.ColumnStyles.Add(columnInfo);
					}
				}

				var messagesGridColumnAvailability = provider?.GetMessagesGridColumnAvailability(Header);
				if (messagesGridColumnAvailability != null)
				{
					foreach (var messagesGridColumnAvailabilityData in messagesGridColumnAvailability)
					{
						messagesGrid.SetAvailability(messagesGridColumnAvailabilityData.Key, messagesGridColumnAvailabilityData.Value);
					}
				}

				var messagesGridColumnVisible = provider?.GetMessagesGridColumnVisible(Header);
				if (messagesGridColumnVisible != null)
				{
					foreach (var mmessagesGridColumnVisibleData in messagesGridColumnVisible)
					{
						messagesGrid.SetColumnVisible(mmessagesGridColumnVisibleData.Key, mmessagesGridColumnVisibleData.Value);
					}
				}

				var messagesGridColumnsWidth = provider?.GetMessagesGridColumnsWidth(Header);
				if (messagesGridColumnsWidth != null)
				{
					foreach (var messagesGridColumnsWidthData in messagesGridColumnsWidth)
					{
						var columnStyle = messagesGrid.GetColumnStyle(messagesGridColumnsWidthData.Key);
						columnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(messagesGridColumnsWidthData.Value);
					}
				}

				var messagesGridOrder = provider?.GetMessagesGridOrder().ToArray();
				if (messagesGridOrder != null)
				{
					messagesGrid.ReOrderColumns(messagesGridOrder);
				}
			}
		}

		ZGridColumnInfo[] messagesGridExtraColumnInfos;

		protected virtual ResourceStringData SetAdditionalTabPageCaption() => Enterprise.Customs.ASYCUDA.Gui.Res.GetData("74f4fc01-f4c1-4c6e-bb83-1b07ed8d8c20", "Messages");

		#region IAdditionalTabPage

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => SetAdditionalTabPageCaption();

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 6;

		#endregion
	}
}
