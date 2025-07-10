using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
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

		string ISupportingInfoUserControls.GridBindingMember => nameof(JobDeclaration.CustomsEntryInstructions);

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;

		#endregion

		void SetPreviousDocumentsLayout()
		{
			if (IsBoundToEntryInstructions)
			{
				DetailsLayoutControl.SetLayout(new PreviousDocumentFieldsLayout());
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

		protected IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new PreviousDocumentsGridColumnLayout();

		protected new JobDeclaration DataSource => (JobDeclaration)base.DataSource;

		protected bool IsBoundToEntryInstructions => PreviousDocumentsGrid.DataMember == CusEntryInstructionInfoBindingMemberName;
		const string CusEntryInstructionInfoBindingMemberName = nameof(JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.PreviousDocuments);

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => IsBoundToEntryInstructions ? new[] { Constants.CusEntryInstruction.PreviousDocumentCaption } : System.Array.Empty<string>();

		void UpdateGridColumnLayout()
		{
			PreviousDocumentsGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
		}
	}
}
