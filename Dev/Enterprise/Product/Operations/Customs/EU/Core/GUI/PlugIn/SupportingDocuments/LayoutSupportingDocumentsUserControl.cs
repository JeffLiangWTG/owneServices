using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class LayoutSupportingDocumentsUserControl : BaseCustomsEntryUserControl, ISupportingDocumentsUserControl
	{
		public LayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		public LayoutSupportingDocumentsFieldsControl SupportingDocumentsFieldsControl { get; private set; }

		string ISupportingInfoUserControls.GridBindingMember => "FilteredInvoiceLines";

		ZGrid ISupportingInfoUserControls.Grid => SupportingDocumentsGrid;
		ISupportingDocumentsFieldsControl ISupportingDocumentsUserControl.SupportingDocumentsFieldsControl => SupportingDocumentsFieldsControl;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var currentDataItem = CurrentDataItem;
			base.SetDataBinding(dataSource, dataMember);
			var newDataItem = CurrentDataItem;
			if (newDataItem != null && currentDataItem != newDataItem)
			{
				LoadFieldsControl();
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			using (SupportingDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var availableColumnNames = AvailableColumnNames;
				if (availableColumnNames?.Count > 0)
				{
					SupportingDocumentsGrid.SetAllAvailability(false);
					SupportingDocumentsGrid.SetAvailability(true, availableColumnNames);
				}
				var columnNamesInSortingOrder = ColumnNamesInSortingOrder;
				if (columnNamesInSortingOrder?.Count > 0)
				{
					SupportingDocumentsGrid.ReOrderColumns(ColumnNamesInSortingOrder);
				}
			}
		}

		protected bool IsUCC6AndIsExport => JobDeclaration?.IsUCC6AndIsExport ?? false;

		protected virtual IReadOnlyList<string> AvailableColumnNames
		{
			get
			{
				if (JobDeclaration is JobDeclaration declaration && declaration.IsUCC6)
				{
					if (declaration.IsExport)
					{
						return UCC6AndExportAvailableColumnNames;
					}

					if (declaration.IsImport)
					{
						return UCC6AndImportAvailableColumnNames;
					}
				}

				return NonUCC6AndExportAvailableColumnNames;
			}
		}

		protected virtual IReadOnlyList<string> UCC6AndImportAvailableColumnNames => NonUCC6AndExportAvailableColumnNames;

		protected virtual IReadOnlyList<string> UCC6AndExportAvailableColumnNames => new[]
		{
			nameof(SupportingDocument.CSI_Code),
			nameof(SupportingDocument.CSI_ReferenceNumber),
			nameof(SupportingDocument.CSI_AdditionalDescription),
			nameof(SupportingDocument.CSI_Quantity),
			nameof(SupportingDocument.CSI_UnitOfQuantity),
			nameof(SupportingDocument.CSI_Value),
			nameof(SupportingDocument.CSI_RX_NKCurrency),
			nameof(SupportingDocument.CSI_DateOfExpiry),
			nameof(SupportingDocument.CSI_ItemNumber)
		};

		protected virtual IReadOnlyList<string> NonUCC6AndExportAvailableColumnNames => new[]
		{
			nameof(SupportingDocument.CSI_Code),
			nameof(SupportingDocument.CSI_CodeDescription),
			nameof(SupportingDocument.CSI_ReferenceNumber),
			nameof(SupportingDocument.CSI_AdditionalDescription),
			nameof(SupportingDocument.CSI_Status),
			nameof(SupportingDocument.CSI_Quantity),
			nameof(SupportingDocument.CSI_UnitOfQuantity),
			nameof(SupportingDocument.CSI_Quantity2),
			nameof(SupportingDocument.CSI_UnitOfQuantity2),
			nameof(SupportingDocument.CSI_Value),
			nameof(SupportingDocument.CSI_RX_NKCurrency),
			nameof(SupportingDocument.CSI_DateOfIssue),
			nameof(SupportingDocument.CSI_DateOfExpiry),
		};

		protected virtual IReadOnlyList<string> ColumnNamesInSortingOrder => AvailableColumnNames;

		protected virtual LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(JobDeclaration declaration) => new LayoutSupportingDocumentsFieldsControl(declaration);

		void LoadFieldsControl()
		{
			if (SupportingDocumentsFieldsControl == null)
			{
				SupportingDocumentsFieldsControl = GetSupportingDocumentsFieldsControl(JobDeclaration);
				SupportingDocumentsFieldsControl.Dock = DockStyle.Fill;
				SupportingDocumentsFieldsControl.CaptionRenderingEnabled = true;
				BindingSource.SetBindingMember(SupportingDocumentsFieldsControl, GetSupportingDocumentsFieldsControlBindingString());
				BottomPanel.Controls.Add(SupportingDocumentsFieldsControl);
			}
		}

		protected virtual string GetSupportingDocumentsFieldsControlBindingString() => "FilteredInvoiceLines.SupportingDocuments";
	}
}
