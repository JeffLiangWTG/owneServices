using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid : ZUserControl, ISupportingInfoUserControls
	{
		public UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid()
		{
			InitializeComponent();
			PreviousDocumentsGrid.AfterBind += PreviousDocumentsGrid_AfterBind;
			DetailsLayoutControl.AllowOutsideOfParent();
		}

		void PreviousDocumentsGrid_AfterBind(object sender, EventArgs e)
		{
			SetPreviousDocumentsGridLayout();
			AdjustControlProperties();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
				Header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
				TemporaryStorageHeader_AMA_MessageTypeChanged(this, EventArgs.Empty);
				UpdatePreviousDocumentsGridColumnLayout();
			}
		}

		void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs e)
		{
			if (Header != null)
			{
				PreviousDocumentsGrid.SetAvailability(!Header.IsTransfer && !Header.IsDeconsolidation, nameof(TemporaryStoragePreviousDocument.CSI_LineNo));
			}
		}

		void UpdatePreviousDocumentsGridColumnLayout()
		{
			var gridColumnLayoutProvider = LayoutProvider
				?.GetTemporaryStorageGridColumnLayoutProviderFactory()
				?.CreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetails() ?? new UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayout();

			PreviousDocumentsGrid.ApplyGridColumnLayout(gridColumnLayoutProvider);
		}

		TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;

		ITemporaryStorageLayoutProvider fLayoutProvider;
		ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ?? (fLayoutProvider = TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header));

		protected override void Dispose(bool disposing)
		{
			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
			}
			base.Dispose(disposing);
		}

		void SetPreviousDocumentsGridLayout()
		{
			DetailsLayoutControl.SetLayout(CreateNewUCC6TemporaryStoragePreviousDocumentsDetailsLayout());
		}

		protected virtual void AdjustControlProperties()
		{
			PreviousDocumentsGroupBox.CaptionResourceString = Res.GetData("7A46A367-A68B-4821-9E13-D515EC356693", "Previous Documents");
			this.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		IPanelLayoutProvider CreateNewUCC6TemporaryStoragePreviousDocumentsDetailsLayout() => LayoutProvider?.GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid();

		#region ISupportingInfoUserControls

		string ISupportingInfoUserControls.GridBindingMember => nameof(TemporaryStorageHeader.Bills);

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;

		#endregion

		public const string UCC6TemporaryStorageBillPreviousDocumentsBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.PreviousDocuments);
		public const string UCC6TemporaryStorageHeaderPreviousDocumentsBingdingMemberName = nameof(TemporaryStorageHeader.PreviousDocuments);
	}
}
