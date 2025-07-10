using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid : ZUserControl, ISupportingInfoUserControls
	{
		public UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid()
		{
			InitializeComponent();
			SupportingDocumentsGrid.AfterBind += SupportingDocumentsGrid_AfterBind;
			DetailsLayoutControl.AllowOutsideOfParent();
		}

		void SupportingDocumentsGrid_AfterBind(object sender, EventArgs e)
		{
			ChangeGridColumnsVisibility();
			SetSupportingDocumentsLayout();
			AdjustControlProperties();
		}

		protected virtual string[] AvailableColumnNames => new[]
		{
			nameof(TemporaryStorageSupportingDocument.CSI_Code),
			nameof(TemporaryStorageSupportingDocument.CSI_ReferenceNumber),
		};

		string[] ColumnNamesInSortingOrder => AvailableColumnNames;

		protected void ChangeGridColumnsVisibility()
		{
			using (SupportingDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var availableColumnNames = AvailableColumnNames;
				if (availableColumnNames?.Length > 0)
				{
					SupportingDocumentsGrid.SetAllAvailability(false);
					SupportingDocumentsGrid.SetAvailability(true, availableColumnNames);
				}
				var columnNamesInSortingOrder = ColumnNamesInSortingOrder;
				if (columnNamesInSortingOrder?.Length > 0)
				{
					SupportingDocumentsGrid.ReOrderColumns(ColumnNamesInSortingOrder);
				}
			}
		}

		void SetSupportingDocumentsLayout()
		{
			DetailsLayoutControl.SetLayout(CreateNewUCC6TemporaryStorageSupportingDocumentsDetailsLayout());
		}

		protected virtual void AdjustControlProperties()
		{
			SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("68C51D9C-3226-47D1-804B-760C6A165BC1", "Supporting Documents");
			SupportingDocumentsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 50, true);
			SupportingDocumentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 60, true);
			this.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		public string GridBindingMember => nameof(TemporaryStorageHeader.Bills);

		public ZGrid Grid => SupportingDocumentsGrid;

		protected virtual IPanelLayoutProvider CreateNewUCC6TemporaryStorageSupportingDocumentsDetailsLayout() => new UCC6TemporaryStorageSupportingDocumentsDetailsLayoutWithGrid();

		protected bool IsBoundToUCC6TemporaryStorageBill => SupportingDocumentsGrid.DataMember == UCC6TemporaryStorageBillSupportingDocumentsBingdingMemberName;
		protected bool IsBoundToUCC6TemporaryStorageBillPackedItem => SupportingDocumentsGrid.DataMember == UCC6TemporaryStorageBillPackedItemSupportingDocumentsBingdingMemberName;

		public const string UCC6TemporaryStorageBillSupportingDocumentsBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.SupportingDocuments);
		public const string UCC6TemporaryStorageBillPackedItemSupportingDocumentsBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.PackedItems) + "." + nameof(TemporaryStoragePackedItem.SupportingDocuments);
	}
}
