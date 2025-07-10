using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.GUI
{
	class ServiceTaskGridMenuItemProvider : IFilterGridMenuItemProvider
	{
		public ServiceTaskGridMenuItemProvider()
			: this(new ServiceTaskCsvExporter(new HostedServiceAttributeProvider()), CreateSaveDialog)
		{
		}

		internal ServiceTaskGridMenuItemProvider(IServiceTaskCsvExporter serviceTaskCsvExporter,  Func<IFileDialog> fileDialogFactory)
		{
			this.serviceTaskCsvExporter = serviceTaskCsvExporter;
			this.fileDialogFactory = fileDialogFactory;
		}

		public IEnumerable<MenuItem> GetMenuItems(ZFilterGridModule module)
		{
			if (IsServiceTaskModule(module.ID) && IsCurrentUserSupport())
			{
				yield return new ZMenuItem(ResString.GetMultilingualString("AD217B56-0AB0-400D-9DFD-F14BCCF01F5F", "Export All Service Tasks"), ExportServiceTasksClick);
			}
			
			static bool IsServiceTaskModule(ModuleIdentifier moduleId)
			{
				return moduleId == ModuleIDs.StmServiceTask;
			}

			static ZBool IsCurrentUserSupport()
			{
				return GlbStaff.CurrentUser.IsSupportUser;
			}
		}
		
		void ExportServiceTasksClick(object sender, EventArgs e)
		{
			using var dialog = fileDialogFactory();

			if (dialog.ShowDialog(null) != DialogResult.OK)
			{
				return;
			}

			using (var fileStream = dialog.OpenFile())
			{
				serviceTaskCsvExporter.WriteTo(fileStream);
			}

			Globals.Message.Show(Res.GetString("D1F871AF-5C60-4CB8-AC5E-B0A7D0AC479E", "The list of service tasks was saved as {0}", dialog.UnmappedFileName));
		}

		internal static IFileDialog CreateSaveDialog()
		{
			return new ZSaveFileDialog
			{
				FileName = "ServiceTasks",
				DefaultExt = "csv",
				Filter = "*.csv|*.csv",
				AddExtension = true,
				OverwritePrompt = true,
			};
		}

		readonly IServiceTaskCsvExporter serviceTaskCsvExporter;
		readonly Func<IFileDialog> fileDialogFactory;
	}
}
