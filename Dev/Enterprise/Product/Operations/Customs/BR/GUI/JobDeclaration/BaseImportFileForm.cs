using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class BaseImportFileForm : ZChildForm
	{
		public BaseImportFileForm()
		{
		}

		public BaseImportFileForm(IBusiness businessObject)
			: base(businessObject)
		{
			ImportButton.GetExtension<ILabelCaptionRenderer>().Caption = ImportButtonCaption;
			CloseButton.GetExtension<ILabelCaptionRenderer>().Caption = CloseButtonCaption;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		SaveProgressMediator progressMediator;

		public override string FormVerb => string.Empty;

		protected virtual string CloseButtonCaption => Res.GetString("5a9765aa-ec7f-42d3-a713-a8947a257c7d", "Close");

		protected virtual string ImportButtonCaption => Res.GetString("659e7a80-f295-46cc-9a68-6f7ab02efdf1", "Import");

		#region Events On Click

		protected virtual string MessageForStartLoadingFile => Res.GetString("0e4cf322-ff8e-482a-b16d-9fd4549e1347", "Start Loading...");

		protected virtual string MessageForStartImporting => Res.GetString("0677b3fc-91df-416b-8d53-80bc3a9005c2", "Importing...");

		protected virtual void OnClickBrowseButton() { }

		protected virtual void OnClickImportButton() { }

		protected virtual bool ShowSaveProgressOnImporting => true;

		protected virtual bool ShowSaveProgressOnBrowsing => true;

		void BrowseButton_Click(object sender, EventArgs e)
		{
			if (ZFormModaliser.ShowCommonDialogWithoutDispose(OpenFileDialog) == DialogResult.OK)
			{
				FileNameTextBox.Text = string.Join(", ", OpenFileDialog.SelectedFiles.Select(o => o.UnmappedFileName));

				using (progressMediator = ShowSaveProgressOnBrowsing ? new SaveProgressMediator() : null)
				{
					progressMediator?.ShowModalProgressForm(Bounds, MessageForStartLoadingFile, 0);
					OnClickBrowseButton();
					progressMediator?.HideForm();
					progressMediator = null;
				}
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.RunPreSaveValidation();
				if (BusinessEntity.Notifications.HasErrors())
				{
					ShowErrorsDialog();
				}
				else
				{
					using (progressMediator = ShowSaveProgressOnImporting ? new SaveProgressMediator() : null)
					{
						progressMediator?.ShowModalProgressForm(Bounds, MessageForStartImporting, 0);
						OnClickImportButton();
						progressMediator?.HideForm();
						progressMediator = null;
					}
				}
			}
		}

		protected void HideProgressMediator()
		{
			progressMediator?.HideForm();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Progress & Log

		protected void SetProgressAndLog(int completedCount, int totalCount, string message)
		{
			if (message != null)
			{
				LogDetailsListBox.Items.Insert(0, message);
			}

			var percentage = totalCount > 0 ? (completedCount * 100 / totalCount) : 100;
			progressMediator?.UpdateStatus(message ?? progressMediator.Status, percentage);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
