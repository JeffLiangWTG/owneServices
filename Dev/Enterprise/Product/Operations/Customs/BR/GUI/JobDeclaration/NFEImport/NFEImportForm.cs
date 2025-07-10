using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class NFEImportForm : BaseImportFileForm
	{
		public NFEImportForm()
		{
		}

		public NFEImportForm(NFEImportObjectParent nFEImportObjectParent)
			: base(nFEImportObjectParent)
		{
			Master.InvoiceImported = SetProgressAndLog;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			NFEGrid.SetAvailability(Master?.Declaration?.IsPersistent ?? false, NFEImportObject.Schema.EntryInstructionPK);
		}

		public NFEImportObjectParent Master => base.BusinessEntity as NFEImportObjectParent;

		#region Events On Click

		protected override string MessageForStartLoadingFile => Res.GetString("a44aaf73-e225-43f7-92d4-5593dd2f1167", "Start Importing...");

		void SetControlsEnabled(bool enable)
		{
			BrowseButton.Enabled = enable;
			ImportButton.Enabled = enable;
			NFEGrid.Enabled = enable;
		}

		protected override void OnClickBrowseButton()
		{
			SetControlsEnabled(false);

			int completedSteps = 0;
			var totalSteps = OpenFileDialog.SelectedFiles.Length;
			var nfeImportObjects = new List<NFEImportObject>();

			try
			{
				foreach (var fileInfo in OpenFileDialog.SelectedFiles)
				{
					string message;
					var filename = Path.GetFileName(fileInfo.UnmappedFileName);

					using (var fileStream = fileInfo.OpenFile())
					{
						try
						{
							var nfeImportObject = Master.ProcessStreamAndAddNew(fileStream);
							if (nfeImportObject == null)
							{
								message = Res.GetString("1a9e0eee-9e58-42a6-a396-23fea2823477", "Skip {0}, same NF-e Key already exists.", filename);
							}
							else
							{
								nfeImportObjects.Add(nfeImportObject);
								message = Res.GetString("add701ad-474b-4002-9094-f459d22bdc1d", "Loaded: {0}", filename);
							}
						}
						catch (InvalidOperationException)
						{
							message = Res.GetString("f3ef119e-ae52-4727-9295-3f6046bc63c3", "Unable to read {0}", filename);
						}
					}

					SetProgressAndLog(++completedSteps, totalSteps, message);
				}
			}
			finally
			{
				SetControlsEnabled(true);
			}
		}

		protected override void OnClickImportButton()
		{
			if (Master.NFEImportObjectCollection.Any())
			{
				ImportInvoices();
			}
		}

		protected void ImportInvoices()
		{
			SetControlsEnabled(false);

			try
			{
				Master.ImportInvoices();
			}
			finally
			{
				SetControlsEnabled(true);
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Master.InvoiceImported = null;
			}
			base.Dispose(disposing);
		}
	}
}
