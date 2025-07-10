using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT24589_1
{
	public partial class CNDataInterfaceGUI : ZChildForm, INotifications
	{
		public CNDataInterfaceGUI(ChinaStandard2010DataInterfaceWrapper businessEntity)
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

		#region Controls

		protected ZGroupBox InterfaceFileGroupBox;
#if DEBUG
		public
#endif
		ZButton DirectoryPopUpButton;
		ZTextBox DirectoryTextBox;
		ZPeriodEdit FromPeriodEdit;
#if DEBUG
		public
#endif
 ZButton CloseButton;
#if DEBUG
		public
#endif
 ZButton GenerateButton;
		ZTextBox LogTextBox;
		protected ZGroupBox DateGroupBox;
		protected ZGroupBox zGroupBox1;
		ZCheckBox ExportGeneralLedgerCheckBox;
		ZCheckBox ExportFixedAssetsCheckBox;
		ZCheckBox ExportPayrollsCheckBox;
		ZCheckBox ExportReferenceFilesCheckBox;
		ZCheckBox ExportARAPCheckBox;
		protected ZGuidFindBox BranchGuidFindBox;
		protected ZGroupBox zGroupBox2;
		ZRadioButton OldChartTypeRadioButton;
		ZRadioButton NewChartTypeRadioButton;

		#endregion

		#region BizObj

		ChinaStandard2010DataInterfaceWrapper BizObj
		{
			get
			{
				return (ChinaStandard2010DataInterfaceWrapper)BusinessEntity;
			}
		}

		#endregion

		#region GenerateButton_Click

#if DEBUG
		public
#else
		protected
#endif
 void GenerateButton_Click(object sender, EventArgs e)
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
				GenerateButton.Visible = false;
			}
		}

		#endregion

		#region DisableControls

		void DisableControls()
		{
			InterfaceFileGroupBox.Enabled = false;
			DateGroupBox.Enabled = false;
			GenerateButton.Enabled = false;
		}

		#endregion

#if DEBUG
		public
#else
		protected
#endif
 void DirectoryPopUpButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				if (Directory.Exists(DirectoryTextBox.Text))
				{
					dialog.SelectedPath = DirectoryTextBox.Text;
				}
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					BizObj.ExportDirectory = dialog.UnmappedSelectedPath;
				}
			}
		}

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

		ChinaStandard2010DataInterfaceExporter Exporter
		{
			get { return NewExporter(); }
		}

		protected virtual ChinaStandard2010DataInterfaceExporter NewExporter()
		{
			return new ChinaStandard2010DataInterfaceExporter(BizObj, new NotificationBuffer(this));
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
