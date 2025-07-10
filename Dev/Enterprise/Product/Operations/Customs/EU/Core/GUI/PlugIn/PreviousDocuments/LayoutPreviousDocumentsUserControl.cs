using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class LayoutPreviousDocumentsUserControl : BaseCustomsEntryUserControl, ISupportingInfoUserControls, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
	{
		public LayoutPreviousDocumentsUserControl()
		{
			InitializeComponent();

			DetailsLayoutControl.AllowOutsideOfParent();

			UpdateGridColumnLayout();
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var currentDataItem = CurrentDataItem;
			base.SetDataBinding(dataSource, dataMember);
			var newDataItem = CurrentDataItem;
			if (newDataItem != null && currentDataItem != newDataItem)
			{
				SetPreviousDocumentsLayout();
			}
		}

		#region ISupportingInfoUserControls

		string ISupportingInfoUserControls.GridBindingMember => nameof(Business.Declaration.JobDeclaration.FilteredInvoiceLines);

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;

		#endregion

		void SetPreviousDocumentsLayout()
		{
			if (IsBoundToInvoiceHeaders)
			{
				DetailsLayoutControl.SetLayout(LayoutProvider.GetInvoiceHeaderPreviousDocumentsDetailsLayout(DataSource));
			}
			else if (IsBoundToInvoiceLines)
			{
				DetailsLayoutControl.SetLayout(LayoutProvider.GetInvoiceLinePreviousDocumentsDetailsLayout(DataSource));
			}
			else if (IsBoundToEntryInstructions)
			{
				DetailsLayoutControl.SetLayout(LayoutProvider.GetEntryInstructionPreviousDocumentsDetailsLayout(DataSource));
			}
			else if (IsBoundToDeclaration)
			{
				DetailsLayoutControl.SetLayout(LayoutProvider.GetDeclarationPreviousDocumentsDetailsLayout(DataSource));
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			using (PreviousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				PreviousDocumentsGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
				var columnNames = PreviousDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				PreviousDocumentsGrid.AddToAvailableColumns(columnNames);
				PreviousDocumentsGrid.ReOrderColumns(columnNames);
			}
		}

		protected bool IsUCC6ImportOrExport => (JobDeclaration?.IsUCC6AndIsImport ?? false) || (JobDeclaration?.IsUCC6AndIsExport ?? false);

		protected virtual IGridColumnLayoutProvider GetGridColumnLayoutProvider()
			=> IsUCC6ImportOrExport ? new UCC6PreviousDocumentGridColumnLayout() : new NonUCC6PreviousDocumentGridColumnLayout();

		protected new JobDeclaration DataSource => (JobDeclaration)base.DataSource;

		IPlugInLayoutProvider LayoutProvider => layoutProviderCached ??= PlugInLayoutProvider.GetLayoutProvider(DataSource?.GetDefaultDataGroupingCode());
		IPlugInLayoutProvider layoutProviderCached;

		protected bool IsBoundToInvoiceLines => PreviousDocumentsGrid.DataMember == InvoiceLinesAdditionalInfoBindingMemberName;
		protected bool IsBoundToInvoiceHeaders => PreviousDocumentsGrid.DataMember == InvoiceHeadersAdditionalInfoBindingMemberName;
		protected bool IsBoundToEntryInstructions => PreviousDocumentsGrid.DataMember == CusEntryInstructionInfoBindingMemberName;
		protected bool IsBoundToDeclaration => PreviousDocumentsGrid.DataMember == DeclarationAdditionalInfoBindingMemberName;

		const string InvoiceLinesAdditionalInfoBindingMemberName = nameof(Business.Declaration.JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.PreviousDocuments);
		const string InvoiceHeadersAdditionalInfoBindingMemberName = nameof(Business.Declaration.JobDeclaration.Invoices) + "." + nameof(JobComInvoiceLine.PreviousDocuments);
		const string CusEntryInstructionInfoBindingMemberName = nameof(Business.Declaration.JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.PreviousDocuments);
		const string DeclarationAdditionalInfoBindingMemberName = "." + nameof(JobComInvoiceLine.PreviousDocuments);

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine };

		void UpdateGridColumnLayout()
		{
			PreviousDocumentsGrid.ApplyGridColumnLayout(new PreviousDocumentsGridColumnLayout());
		}
	}
}
