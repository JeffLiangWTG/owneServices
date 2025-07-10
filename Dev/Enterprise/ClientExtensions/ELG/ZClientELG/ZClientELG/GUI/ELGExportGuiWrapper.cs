using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ELG
{
	public class ELGExportGUIWrapper : FlatFileXmlExportGUIWrapper
	{
		public ELGExportGUIWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string FormCaption
		{
			get { return SagFormCaption; }
		}

		protected override AccountingTransactionsDataExporter DataExporter
		{
			get { return ExportDirector.Exporter; }
		}

		protected override string InitialDirectory
		{
			get { return ELGDataRegistry.Instance.SagExportDirectory; }
		}

		protected override string FileName
		{
			get { return ExportDirector.Exporter.FileName; }
		}

		protected override bool IsOKToExport()
		{
			ZStringBuilder message = ELGDataRegistry.GetIsExportEnvironmentValid();
			if (!message.IsEmpty)
			{
				Globals.Message.ShowError(message.ToStringWithNewLineBetweenAppends(), "Accounting Export Error");
			}
			return message.IsEmpty && base.IsOKToExport();
		}

		protected override DialogResult ShowDialog(IFileDialog dialog)
		{
			return DialogResult.OK;
		}

		protected override void LoadFormAndExport()
		{
			using (ProgressForm = new ProgressForm())
			{
				ShowProgressForm();
				try
				{
					ExportDirector.Exporter.ProcessingProgressed += new EventHandler(UpdateStatusAndPercentageComplete);
					ProgressForm.Text = "Exporting Transactions";
					ExportDirector.Execute();
				}
				finally
				{
					ExportDirector.Exporter.ProcessingProgressed -= new EventHandler(UpdateStatusAndPercentageComplete);
				}
			}
		}

		SagGuiWrapperExportDirector ExportDirector
		{
			get
			{
				if (exportDirector == null)
				{
					exportDirector = new SagGuiWrapperExportDirector(Factory, new NotificationBuffer());
					exportDirector.EnableManualMode = true;
				}
				return exportDirector;
			}
		}
		SagGuiWrapperExportDirector exportDirector;

		const string SagFormCaption = "Export Transactions in Sage Format";
	}
}
