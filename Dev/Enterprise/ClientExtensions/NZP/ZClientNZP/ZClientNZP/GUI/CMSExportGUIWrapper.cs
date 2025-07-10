using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Client.NZP.CMS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.NZP.GUI
{
	public class CMSExportGUIWrapper : FlatFileXmlExportGUIWrapper
	{
		public CMSExportGUIWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string FormCaption
		{
			get { return Constants.CMSMenuItem; }
		}

		[BusinessObjectTestExclude()]
		public override ZDateTime DateTo
		{
			get { return base.DateTo; }
			set
			{
				base.DateTo = (value > ZDateTime.Today ? ZDateTime.Today : value).AddDays(1).AddMilliseconds(-1);
				DateToInfo.RefreshBinding();
			}
		}

		#region DataExporter

		protected override AccountingTransactionsDataExporter DataExporter
		{
			get
			{
				if (CashSalesExporter == null)
				{
					CashSalesExporter = new CMSCashSalesExporter(0, Factory);
					SetFromAndToDates();
				}
				return CashSalesExporter;
			}
		}

		CMSCashSalesExporter CashSalesExporter;

		#endregion

		#region Implementation

		protected override string FileName
		{
			get { return ZDateTime.Now.ToString(Constants.FileNameFormat) + "_DIEL_"; }
		}

		ZString ExportDirectory
		{
			get { return NZPDataRegistry.Instance.CMSExportDirectory; }
		}

		bool FolderExists
		{
			get
			{
				return Directory.Exists(ClientSharedComponents.SharedUtil.GetFinalPath(ExportDirectory));
			}
		}

		protected override string FileExtention
		{
			get { return "_I.lst"; }
		}

		protected override bool IsOKToExport()
		{
			bool result = true;
			if (FolderExists)
			{
				result = base.IsOKToExport();
			}
			else
			{
				string message = string.Format("CMS Export Directory: {0} is not specified or does not exists.{1}Please specify a valid Export Directory in Registry -> NZ Post Client Extensions", ExportDirectory, System.Environment.NewLine);
				Globals.Message.ShowError(message, "Export Directory Error");
				result = false;
			}
			return result;
		}

		protected override DialogResult ShowDialog(IFileDialog dialog)
		{
			return DialogResult.OK;
		}

		protected override void LoadFormAndExport()
		{
			string cashSalesFile = Path.Combine(ExportDirectory, FileName + "T" + FileExtention);
			string creditSalesFile = Path.Combine(ExportDirectory, FileName + "S" + FileExtention);
			string creditNotesFile = Path.Combine(ExportDirectory, FileName + "X" + FileExtention);
			ZInt originalBatchNumber = ExistingBatchNumberToExport;

			using (ProgressForm = new ProgressForm())
			{
				ShowProgressForm();
				ExportCMSFiles((CMSDataExporter)DataExporter, cashSalesFile, "Cash Sales");
				ExportCMSFiles(new CMSCreditSalesExporter(DataExporter.FilterProvider.CurrentBatchNo, Factory), creditSalesFile, "Credit Sales");
				ExportCMSFiles(new CMSCreditNotesExporter(DataExporter.FilterProvider.CurrentBatchNo, Factory), creditNotesFile, "Credit Notes");
			}

			if (originalBatchNumber == 0)
			{
				NZPDataRegistry.Instance.CMSLastDateExported = DateTo.Date.ToDateTime();
				SetFromAndToDates();
			}

			Globals.Message.ShowInformation(cashSalesFile + ",\n" + creditSalesFile + ", and\n" + creditNotesFile + "\nwere Exported.", "CMS Audit Information");
		}

		protected void SetFromAndToDates()
		{
			DateFrom = NZPDataRegistry.Instance.CMSLastDateExported.IsValid ? NZPDataRegistry.Instance.CMSLastDateExported : ZDateTime.Today.AddDays(-6);
			DateTo = NZPDataRegistry.Instance.CMSLastDateExported.IsValid ? NZPDataRegistry.Instance.CMSLastDateExported.AddDays(6) : ZDateTime.Today;
		}

		void ExportCMSFiles(CMSDataExporter exporter, string fileName, string transactionType)
		{
			try
			{
				exporter.ProcessingProgressed += new EventHandler(UpdateStatusAndPercentageComplete);
				using (Stream stream = ZSaveFileDialog.OpenFile(fileName))
				{
					ProgressForm.Text = "Exporting " + transactionType + " Transactions";
					exporter.Export(stream);
				}
			}
			finally
			{
				exporter.ProcessingProgressed -= new EventHandler(UpdateStatusAndPercentageComplete);
			}
		}
		#endregion
	}
}
