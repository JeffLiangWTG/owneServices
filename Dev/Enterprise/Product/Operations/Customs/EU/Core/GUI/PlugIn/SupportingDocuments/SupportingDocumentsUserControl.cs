using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class SupportingDocumentsUserControl : BaseCustomsEntryUserControl, ISupportingDocumentsUserControl
	{
		public SupportingDocumentsUserControl()
		{
			InitializeComponent();
			LoadFieldsControl();
		}

		public BaseCustomsEntryUserControl SupportingDocumentsFieldsControl;

		string ISupportingInfoUserControls.GridBindingMember => "FilteredInvoiceLines";

		ZGrid ISupportingInfoUserControls.Grid => SupportingDocumentsGrid;

		ISupportingDocumentsFieldsControl ISupportingDocumentsUserControl.SupportingDocumentsFieldsControl => SupportingDocumentsFieldsControl as ISupportingDocumentsFieldsControl;

		protected virtual BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new SupportingDocumentsFieldsControl();

		protected void RemoveColumnsExcept(IEnumerable<string> columnNamesNotToRemove)
		{
			var notNeededColumns = SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !columnNamesNotToRemove.Contains(x.ColumnName)).ToArray();
			notNeededColumns.ForEach(SupportingDocumentsGrid.ColumnStyles.Remove);
		}

		void LoadFieldsControl()
		{
			SupportingDocumentsFieldsControl = GetSupportingDocumentsFieldsControl();
			SupportingDocumentsFieldsControl.JobDeclaration = JobDeclaration;
			SupportingDocumentsFieldsControl.Dock = DockStyle.Fill;
			SupportingDocumentsFieldsControl.CaptionRenderingEnabled = true;
			SupportingDocumentsFieldsControl.SetDataBinding(JobDeclaration, "");
			BottomPanel.Controls.Add(SupportingDocumentsFieldsControl);
		}
	}
}
