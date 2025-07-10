using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class NFeExportForm : ZChildForm
	{
		public NFeExportForm(NFeExportObject nFeExportObject) : base(nFeExportObject)
		{
			InitializeComponent();
			RemoveColumns();
		}

		public override string FormVerb => string.Empty;

		NFeExportObject NFeExportObject => DataSource as NFeExportObject;

		#region Events On Click

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ExportDataButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog.FileName = $"{NFeExportObject.Declaration.JE_DeclarationReference} - {ZDateTime.Now:yyyyMMddHHmmss}"; // filename should be in English only

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(SaveFileDialog) == DialogResult.OK)
			{
				var notifications = new ExcelExporterGuiNotifications(FindForm());
				var dataExcelExporter = new NFeDataExcelExporter(NFeExportObject, LinesGrid, notifications);
				using (var outputStream = SaveFileDialog.OpenFile())
				{
					dataExcelExporter.ExportDataToExcel(outputStream);
				}
			}
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
				if (SaveFileDialog != null)
				{
					SaveFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void RemoveColumns()
		{
			if (NFeExportObject.Declaration.IsImportSiscomex)
			{
				LinesGrid.RemoveFromAvailableColumns(NFeInvoiceLineExportObject.Schema.Permits);
			}

			if (NFeExportObject.Declaration.IsImportOnly)
			{
				LinesGrid.RemoveFromAvailableColumns(NFeInvoiceLineExportObject.Schema.Nve, NFeInvoiceLineExportObject.Schema.ImportLicenseFineAmount);
			}

			if (!NFeExportObject.Declaration.IsImportOnly)
			{
				LinesGrid.RemoveFromAvailableColumns(NFeInvoiceLineExportObject.Schema.Complement);
			}
		}
	}
}
