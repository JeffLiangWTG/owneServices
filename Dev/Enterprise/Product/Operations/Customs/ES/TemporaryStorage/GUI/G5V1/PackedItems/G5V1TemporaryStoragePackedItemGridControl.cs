using System;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class G5V1TemporaryStoragePackedItemGridControl : UCC6TemporaryStoragePackedItemGridControl
	{
		public G5V1TemporaryStoragePackedItemGridControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayout()
		{
			base.InitializeGridLayout();
			_ = PackedItemGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
			{
				ColumnName = TemporaryStoragePackedItem.Schema.IsMissing,
				TextAlign = System.Windows.Forms.HorizontalAlignment.Center,
				IsUnavailable = true
			});
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
			var currentPackedItem = (TemporaryStorageHeader)CurrentDataItem;
			if (currentPackedItem != null)
			{
				ModifyColumnVisibility(currentPackedItem);
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var currentPackedItem = (TemporaryStorageHeader)CurrentDataItem;
			if (currentPackedItem != null)
			{
				currentPackedItem.AMA_MessageTypeInfo.ValueChanged += AMA_MessageTypeInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var currentPackedItem = (TemporaryStorageHeader)CurrentDataItem;
			if (currentPackedItem != null)
			{
				currentPackedItem.AMA_MessageTypeInfo.ValueChanged -= AMA_MessageTypeInfo_ValueChanged;
			}
		}

		#endregion

		void AMA_MessageTypeInfo_ValueChanged(object sender, EventArgs e) => ModifyColumnVisibility((TemporaryStorageHeader)CurrentDataItem);

		void ModifyColumnVisibility(TemporaryStorageHeader packedItem)
		{
			var isReception = packedItem?.IsMessageTypeG5V1Reception ?? false;
			PackedItemGrid.SetAvailability(isReception, TemporaryStoragePackedItem.Schema.IsMissing);
		}
	}
}
