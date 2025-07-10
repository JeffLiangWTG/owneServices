using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class CsvExportGLTransactionForm : ZChildForm, INotifications
	{
		#region Controls

#if DEBUG
		public
#endif
 ZGroupBox PrimaryFiltersGroupBox;
		ZDateEdit ToDateDateEdit;
		ZDateEdit FromDateDateEdit;
		protected ZGuidFindBox EndGLAccountGuidFindBox;
		protected ZGuidFindBox StartGLAccountGuidFindBox;
		protected ZGuidFindBox DepartmentPKGuidFindBox;
		protected ZGuidFindBox BranchPKGuidFindBox;
		protected ZDropEdit DescriptionDisplayDropEdit;
		protected ZGroupBox SortByGroupBox;
		protected ZPeriodEdit ToPeriodPeriodEdit;
		protected ZPeriodEdit FromPeriodPeriodEdit;
		ZRadioButton SourceRadioButton;
		ZRadioButton PeriodRadioButton;
#if DEBUG
		public
#endif
 new ZButton CancelButton;
#if DEBUG
		public
#endif
 ZButton ExportButton;
		ZTextBox LogTextBox;
		protected ZGroupBox DirectoryGroupBox;
		protected ZTextBox ExportDirectoryTextBox;
		protected ZButton BrowseButton;
		protected ZGroupBox BatchGroupBox;
		ZCheckBox ExportExistingBatchCheckBox;
		ZCheckBox CreateAndExportBatchCheckBox;
		ZCalcEdit BatchNumberCalcEdit;

		#endregion

		public CsvExportGLTransactionForm(GLTransactionBusinessObject businessEntity)
			: base(businessEntity)
		{
			BizObj.Exporter = Exporter;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region BizObj

		GLTransactionBusinessObject BizObj
		{
			get
			{
				return (GLTransactionBusinessObject)BusinessEntity;
			}
		}

		#endregion

		#region BrowseButton_Click

#if DEBUG
		public
#else
		protected
#endif
 void BrowseButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				if (Directory.Exists(ExportDirectoryTextBox.Text))
				{
					dialog.SelectedPath = ExportDirectoryTextBox.Text;
				}
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					BizObj.ExportDirectory = dialog.UnmappedSelectedPath;
				}
			}
		}

		#endregion

		#region ExportButton_Click

#if DEBUG
		public
#endif
 void ExportButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave continueExport = ContinueWithSave.No;

			try
			{
				continueExport = ValidateAndSave();
			}
			catch (ZCannotSaveException)
			{
			}

			if (continueExport == ContinueWithSave.Yes)
			{
				DisableControls();
				ExportButton.Visible = false;
				CancelButton.Text = Res.GetString("CsvExportGLTransactionForm|7A611CA9-2FE6-432a-9944-BCEF202F5FCC", "Close");
			}
		}

		#endregion

		#region DisableControls

		void DisableControls()
		{
			PrimaryFiltersGroupBox.Enabled = false;
			SortByGroupBox.Enabled = false;
			BatchGroupBox.Enabled = false;
			DirectoryGroupBox.Enabled = false;
			ExportButton.Enabled = false;
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		public void Notify(INotification notification)
		{
			BizObj.AddToLog(notification.Message + System.Environment.NewLine);

			LogTextBox.SelectionStart = BizObj.Log.Length;
			LogTextBox.ScrollToCaret();
		}

		#endregion

		GLTransactionExporter Exporter
		{
			get { return NewExporter(); }
		}

		protected virtual GLTransactionExporter NewExporter()
		{
			return new GLTransactionExporter(BizObj, new NotificationBuffer(this));
		}
	}
}

