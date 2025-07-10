using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT19581_2004
{
	public partial class CN2004DataInterfaceGUI : ZChildForm, INotifications
	{
		public CN2004DataInterfaceGUI(ChinaStandard2004DataInterfaceWrapper businessEntity)
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region BizObj

		ChinaStandard2004DataInterfaceWrapper BizObj
		{
			get
			{
				return (ChinaStandard2004DataInterfaceWrapper)BusinessEntity;
			}
		}

		#endregion

		#region GenerateButton_Click

		protected void GenerateButton_Click(object sender, EventArgs e)
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

		ChinaStandard2004DataInterfaceExporter Exporter
		{
			get { return NewExporter(); }
		}

		protected virtual ChinaStandard2004DataInterfaceExporter NewExporter()
		{
			return new ChinaStandard2004DataInterfaceExporter(BizObj, new NotificationBuffer(this));
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ExportTXTRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (ExportTXTRadioButton.Checked)
			{
				DirectoryTextBox.Visible = false;
				DirectoryPopUpButton.Visible = false;
				DeliveryToTextBox.Visible = true;
			}
			else
			{
				DeliveryToTextBox.Visible = false;
				DirectoryTextBox.Visible = true;
				DirectoryPopUpButton.Visible = true;
			}
		}
	}
}
