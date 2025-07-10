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

namespace Enterprise.Client.AWH
{
	public class AWHARTransactionModuleOverride : ARTransactionModuleStrip
	{
		public AWHARTransactionModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem("To AWH File", new EventHandler(ARInvoiceExport));
		}

		void ARInvoiceExport(object sender, EventArgs e)
		{
			ZString errorMesg = ValidateEnvironmentAndReturnError;
			if (errorMesg.IsEmpty)
			{
				AWHARTransExportGUIWrapper exportWrapper = new AWHARTransExportGUIWrapper(new BusinessObjectFactory());
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

				if (AWHDataRegistry.Instance.ARTransExportDirectory.IsEmpty)
				{
					builder.AppendLine("- The Export Directory is not set.");
				}

				if (!FolderExists)
				{
					builder.AppendLine("- The Export Directory doesn't exist.");
				}

				if (AWHDataRegistry.Instance.ExportFilePrefix.IsEmpty)
				{
					builder.AppendLine("- The Output File Prefix is not set.");
				}

				if (BranchCodeNotSet)
				{
					builder.AppendLine("- The Branch Code Mapping is not set.");
				}

				if (AWHDataRegistry.Instance.DepartmentList.Count == 0)
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

				foreach (ICodeDescription pair in AWHDataRegistry.Instance.BranchList)
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
				ZString exportDirectory = AWHDataRegistry.Instance.ARTransExportDirectory;
				return Directory.Exists(ClientSharedComponents.SharedUtil.GetFinalPath(exportDirectory));
			}
		}
	}
}
