using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Accounting.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DP2
{
	public class DP2ARTransactionModuleOverride : ARTransactionModuleStrip
	{
		public DP2ARTransactionModuleOverride()
		{
		}

		protected override void AddExtraImportExportMenuItems()
		{
			if (DP2DataRegistry.Instance.EnableARTransactionExportItem.Value)
			{
				AddExportDataMenuItem("To AWH File", new EventHandler(ARInvoiceExport));
			}
		}

		void ARInvoiceExport(object sender, EventArgs e)
		{
			ZString errorMesg = ValidateEnvironmentAndReturnError;
			if (errorMesg.IsEmpty)
			{
				DP2ARTransExportGUIWrapper exportWrapper = new DP2ARTransExportGUIWrapper(new BusinessObjectFactory());
				ZFormModaliser.ShowDialogAndDispose(new FlatFileXmlExportForm(exportWrapper));
			}
			else
			{
				ZString message = "----------------------------------------------------------\nPlease fix the following error(s):\n-----------------------------------------------------------\n";
				Globals.Message.ShowInformation(message + errorMesg);
			}
		}

		ZString ValidateEnvironmentAndReturnError
		{
			get
			{
				StringBuilder builder = new StringBuilder();

				if (DP2DataRegistry.Instance.ARTransExportDirectory.IsEmpty)
				{
					builder.AppendLine("- The Export Directory is not set.");
				}

				if (!FolderExists)
				{
					builder.AppendLine("- The Export Directory doesn't exist.");
				}

				if (DP2DataRegistry.Instance.ExportFilePrefix.IsEmpty)
				{
					builder.AppendLine("- The Output File Prefix is not set.");
				}

				if (BranchCodeNotSet)
				{
					builder.AppendLine("- The Branch Code Mapping is not set.");
				}

				if (DP2DataRegistry.Instance.DepartmentList.Count == 0)
				{
					builder.AppendLine("- The Department Code Mapping is not set.");
				}

				return builder.ToString();
			}
		}

		bool BranchCodeNotSet
		{
			get
			{
				bool result = false;

				foreach (ICodeDescription pair in DP2DataRegistry.Instance.BranchList)
				{
					if (string.IsNullOrEmpty(pair.Description))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		bool FolderExists
		{
			get
			{
				ZString exportDirectory = DP2DataRegistry.Instance.ARTransExportDirectory;
				return Directory.Exists(ClientSharedComponents.SharedUtil.GetFinalPath(exportDirectory));
			}
		}
	}
}
