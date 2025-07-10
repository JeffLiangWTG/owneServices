using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AWH
{
	public class AWHARTransExportGUIWrapper : FlatFileXmlExportGUIWrapper
	{
		public AWHARTransExportGUIWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string InitialDirectory
		{
			get { return AWHDataRegistry.Instance.ARTransExportDirectory; }
		}

		protected override AccountingTransactionsDataExporter DataExporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new AWHARInvoiceDataExporter(Factory);
				}
				return fExporter;
			}
		}
		AWHARInvoiceDataExporter fExporter;

		public override string FormCaption
		{
			get { return "Export Transactions To AWH File"; }
		}

		protected override string FileName
		{
			get { return fExporter.GenerateFileName(); }
		}

		protected override string FileExtention
		{
			get { return fExporter.FileExtension; }
		}

		protected override void LoadFormAndExport()
		{
			if (DataExporter.IsTransactionsExistInBatch)
			{
				CreateFileAndExport(null);
			}
			else
			{
				Globals.Message.ShowInformation("No Transactions were exported.", "There are currently no transactions to export");
			}
		}

		protected override void CreateFileAndExport(Stream stream)
		{
			using (var tempFile = TempFile.New())
			using (ProgressForm = new ProgressForm())
			{
				ShowProgressForm();
				DataExporter.Export(File.Open(tempFile.Filename, FileMode.Open, FileAccess.ReadWrite));

				if (fExporter.OrganisationsWithoutLscCodes.Count == 0
					&& fExporter.BranchWithoutCodeMap.Count == 0
					&& fExporter.DeptWithoutCodeMap.Count == 0)
				{
					string finalFileName = FileName;
					string finalPath = Path.Combine(AWHDataRegistry.Instance.ARTransExportDirectory, finalFileName);

					using (var sourceStream = File.OpenRead(tempFile.Filename))
					using (var targetStream = ZSaveFileDialog.OpenFile(finalPath))
					{
						sourceStream.CopyTo(targetStream);
					}

					string message = string.Format("File {0} has been created.", finalFileName);
					Globals.Message.ShowInformation(DataExporter.GetMessageToDisplayWhenExportIsFinished(message), "Financial Transaction Export");
				}
				else
				{
					NotifyUserThatPropertiesAreNotSet();
				}
			}
		}

		protected int LastBatchNumber
		{
			get { return DataExporter.FilterProvider.CurrentBatchNo; }
		}

		protected override DialogResult ShowDialog(IFileDialog dialog)
		{
			return DialogResult.OK;
		}

		void NotifyUserThatPropertiesAreNotSet()
		{
			string message = "The Following Properties must be set and Batch " + LastBatchNumber.ToString() +
				" Needs to be Manually Recreated.\n";

			StringBuilder builder = new StringBuilder(message);

			if (fExporter.BranchWithoutCodeMap.Count > 0)
			{
				builder.Append(GetMessageBody(LastBatchNumber, fExporter.BranchWithoutCodeMap, "Branches", "AWH Branch Code"));
				builder.AppendLine();
			}

			if (fExporter.DeptWithoutCodeMap.Count > 0)
			{
				builder.Append(GetMessageBody(LastBatchNumber, fExporter.DeptWithoutCodeMap, "Departments", "AWH Department Code"));
				builder.AppendLine();
			}

			if (fExporter.OrganisationsWithoutLscCodes.Count > 0)
			{
				builder.Append(GetMessageBody(LastBatchNumber, fExporter.OrganisationsWithoutLscCodes, "Debtors", "Legacy Code"));
				builder.AppendLine();
			}

			Enterprise.ZArchitecture.Environment.Globals.Message.ShowInformation(builder.ToString());
		}

		string GetMessageBody(int batchNumber, StringCollection codeMapList, ZString objName, ZString message)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(AWHConstants.DashLine);
			result.AppendLine("The Following " + objName + " do not have " + message + " set:");
			result.AppendLine(AWHConstants.DashLine);

			foreach (string code in codeMapList)
			{
				result.AppendLine(code);
			}

			return result.ToString();
		}
	}
}
