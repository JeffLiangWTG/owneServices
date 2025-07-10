using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Module
{
	public class ProcessControllerModule : ZFilterGridModule
	{
		public ProcessControllerModule()
		{
			if (!Globals.IsTest)
			{
				ShouldPerformSearchAsync = true;
			}
		}

		public override bool AllowNew => true;
		public override bool AllowView => false;
		protected override bool ShouldLoadFilterBusinessObjectDefaults => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override bool AllowAdvancedDataAutomationWizard => false;

		protected override void AddCopyMenuItem(List<MenuItem> menu)
		{
		}

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				var buttons = base.ToolBarButtons;

				var activateButton = Array.Find(buttons, button => button.Text.Replace("&", "") == StartMenuText);
				activateButton.ImageIndex = Icons.GetImageIndex(IconTypes.Tick);

				var deactivateButton = Array.Find(buttons, button => button.Text.Replace("&", "") == StopMenuText);
				deactivateButton.ImageIndex = Icons.GetImageIndex(IconTypes.Cross);

				var removeDecommissionedButton = Array.Find(buttons, button => button.Text.Replace("&", "") == DeleteDecommissionedMenuText);
				removeDecommissionedButton.ImageIndex = Icons.GetImageIndex(IconTypes.MinusButtonActive);

				var newButton = Array.Find(buttons, button => button.ImageIndex == Icons.GetImageIndex(IconTypes.NewButtonRest));
				newButton.Text = InstallLocalMenuText;

				var editButton = Array.Find(buttons, button => button.ImageIndex == Icons.GetImageIndex(IconTypes.EditButtonRest));
				editButton.Text = EditHostConfigurationMenuText;

				var logButton = Array.Find(buttons, (button) => button.Text.Replace("&", "") == LogViewerMenuText);
				logButton.ImageIndex = Icons.GetImageIndex(IconTypes.Settings);

				return Array.FindAll(buttons, button => Array.IndexOf(new[] { "Data Transfer" }, button.Text.Replace("&", "")) < 0);
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var baseMenuItems = base.GetNewStandardMenuItems();

			NewMenuItem.CaptionResourceString = InstallLocalMenuData;
			var editMenuItem = (ZMenuItem)baseMenuItems
				.Single(s => s.Equals(EditMenuItem));
			editMenuItem.CaptionResourceString = EditHostConfigurationMenuData;
			var menuItems = baseMenuItems
				.Where(s => !s.Equals(EditMenuItem));

			var newMenuItems = new MenuItem[]
			{
				editMenuItem,
				new ZMenuItem(StartMenuText, Start_Click),
				new ZMenuItem(StopMenuText, Stop_Click),
				new ZMenuItem(DeleteDecommissionedMenuText, Remove_Click),
				new ZMenuItem(LogViewerMenuText, LogViewerMenu_Click),
			};

			return menuItems.Append(newMenuItems).ToArray();
		}

		#region Filtered Grid

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var hosts = StmServiceHostCollection.Load(factory, query, ObjectFactory.Get<IServiceHostsCache>(), true);
			return PerformSearchResult.Success(factory, query, hosts, permitActiveCollectionUpdates: false);
		}

		#endregion

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.ProcessController;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ProcessController);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var control = new ProcessControllerControl(GridCollection, (ProcessControllerFilterBusinessObject)FilterBusinessObject);
			return control;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return GetHosts();
		}

#if DEBUG
		internal
#endif
		IBusinessObjectCollection GetHosts()
		{
			var collection = new StmServiceHostCollection(Factory);
			return collection;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProcessControllerFilterBusinessObject();
		}

		public override bool AllowUniversalCopy => false;

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ProcessController;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Install

		void Start_Click(object sender, EventArgs e)
		{
			StartOrStop(SelectedBusinessObjects.Cast<StmServiceHost>().ToArray(), true);
		}

		protected override IZForm ShowNewForm()
		{
			var hostname = ServiceManagerHelper.GetHostName();
			var existingHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostname));
			if (existingHost != null)
			{
				Globals.Message.Show(
					Res.GetString("55261DB1-F556-4629-BAC1-6FF8E005722E", "There is already a record of a Process Controller installed on the local machine {0}. Please uninstall first.", hostname));

				return base.ShowNewForm();
			}

			var install = Globals.Message.Show(
				Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.Message", "Do you wish to install Process Controller Host Address:\r\n\t{0}", hostname),
				Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.Title", "Install New Process Controller"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;

			if (install)
			{
				InstallHost(hostname);
			}
			return base.ShowNewForm();
		}

		void InstallHost(string hostname)
		{
			var installSuccessful = StmServiceHostMenuHelper.UpdateHost(Factory, StmServiceHostMenuHelper.InstallServiceText, hostname, Db.ServerName, out var errors);
			if (installSuccessful)
			{
				if (Globals.Message.Show(
						Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.Activate.Message",
							"Process Controller successfully installed.\r\nWould you like to start the Process Controller?"),
						Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.Activate.Title",
							"Activate New Process Controller"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					var hosts = Factory.Load<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostname));
					StartOrStop(hosts, true);
				}
			}
			else
			{
				Globals.Message.ShowError(
					Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.FailureToInstall",
						"The installation of {0} encountered an error.\r\nPlease ensure there is adequate system authority to install services and the services are not already installed.\r\nAn uninstall attempt may be required to clean the broken installation.\r\n{1}",
						hostname,
						errors));
				var host = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostname));
				UninstallHost(host, out errors);
				if (Globals.Message.Show(
						Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.ShowNewForm.RetryInstallation",
							"Would you like to retry the installation?",
							hostname),
						Res.GetString("Error", "Error"), MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
				{
					InstallHost(hostname);
				}
			}
		}

		void UninstallHost(StmServiceHost host, out string errors)
		{
			if (host != null)
			{
				StmServiceHostMenuHelper.UpdateHost(Factory, StmServiceHostMenuHelper.UninstallServiceText, host.SH_HostName,
					host.SH_ProxyHost, out errors);
				host.Delete();
				Factory.Save();
			}

			errors = string.Empty;
		}

		#endregion

		#region Edit

		protected override void HandleEditClickCore(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Length == 1)
			{
				var stmHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, ((StmServiceHost)SelectedBusinessObjects[0]).SH_HostName));
				if (stmHost != null)
				{
					var controller = new ServiceTaskProxyConfigurationController();
					controller.ShowEditForm(stmHost);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("99B23834-ED33-483e-A157-31648BBA44F4", "Cannot find service host. Please install and start service tasks before using this configuration."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("0E9A72C9-4FD5-4933-B0F6-CD66CD0900B5", "Select an entry to edit, only one configuration can be changed at time."));
			}
		}

		#endregion

		#region Stop

		protected override ResourceStringData GetDeleteMenuItemText() => Res.GetData("Enterprise.ServiceManager.Module.ProcessControllerModule.GetDeleteMenuItemText", "Uninstall Local");

		void Stop_Click(object sender, EventArgs e)
		{
			StartOrStop(SelectedBusinessObjects.Cast<StmServiceHost>().ToArray(), false);
		}

		void StartOrStop(StmServiceHost[] hosts, bool start)
		{
			if (!hosts.Any())
			{
				Globals.Message.Show(Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.Deactivate.NoHosts", "There is no Process Controller selected."));
			}
			else
			{
				var inactiveHosts = hosts.Where(host => !host.SH_IsActive).ToArray();
				if (inactiveHosts.Any())
				{
					Globals.Message.Show(
						Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.SelectionContainsUninstalledHosts",
							"The Process controller(s) [{0}] have not been installed. Please deselect them and try again.", string.Join(", ", inactiveHosts.Select(i => i.SH_HostName))));
					return;
				}

				var results = new List<string>();
				var methodToUse = start ? StmServiceHostMenuHelper.StartServiceText : StmServiceHostMenuHelper.StopServiceText;
				foreach (var host in hosts)
				{
					var success = StmServiceHostMenuHelper.UpdateHost(Factory, methodToUse, host.SH_HostName, host.SH_ProxyHost, out var errors);
					if (!string.IsNullOrEmpty(errors))
					{
						results.Add(FormattableString.Invariant($"{host.SH_HostName}: {errors}"));
					}
					else
					{
						results.Add(Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.Deactivate.Successful", "{0}: {1} {2}", host.SH_HostName, methodToUse, success ? "Successful" : "Unsuccessful"));
					}
				}
				Globals.Message.Show(string.Join(System.Environment.NewLine, results));
			}
		}

		#endregion

		#region Uninstall

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			var localHostName = ServiceManagerHelper.GetHostName();
			if (Globals.Message.Show(
					Res.GetString("35958295-55AC-46A9-A7AE-FD8CCAF2115B",
						"You are about to uninstall the process controller from local host: {0}. Would you like to proceed?", localHostName),
					Res.GetString("B91F63DE-600E-4E5C-B97B-8553389EB582",
						"Uninstall from local host"), MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
			{
				return;
			}

			var result = StmServiceHostMenuHelper.UpdateHost(Factory, StmServiceHostMenuHelper.UninstallServiceText, localHostName, Db.ServerName, out var errors);
			Globals.Message.Show(
				result
					? Res.GetString("828DEAE7-3F27-4BD4-A56E-C1257ED58F85", "Un-installation successful.")
					: Res.GetString("4D691476-7F2D-4336-8A60-D39A89598566", "Process controller did not uninstall, if this problem persists manually stop the process controller and retry or contact your administrator."));

			var localHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, localHostName));
			if (localHost != null && Globals.Message.Show(
					Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.HandleDeleteClick.Delete.Message",
						"Would you like to delete the Process Controller from the database?"),
					Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.HandleDeleteClick.Delete.Title",
						"Delete Process Controller"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				localHost.Delete();
			}
			Factory.Save();
		}

		#endregion

		#region Delete

		void Remove_Click(object sender, EventArgs e)
		{
			var hosts = SelectedBusinessObjects
				.Cast<StmServiceHost>()
				.Select(h => h.SH_HostName)
				.ToArray();

			if (hosts.Length == 0)
			{
				Globals.Message.Show(Res.GetString("Enterprise.ServiceManager.Module.ProcessControllerModule.Deactivate.NoHosts", "There is no Process Controller selected."));
			}
			else
			{
				var allHosts = string.Join(", ", hosts);
				if (
					Globals.Message.Show(Res.GetString("3D6EF3CA-0728-4CF2-B742-61DBE2D2A0DD", "Do you wish to remove host records {0}?", allHosts), Res.GetString("26B81957-447A-4647-AFA7-255327699A36", "Remove Service Host"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
				{
					StmServiceHostMenuHelper.ModifyAndSaveWithConcurrencyRetry(StmServiceHost.Schema.TableName, (factory) =>
					{
						factory.Load<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hosts))
							.ForEach(h => h.Delete());
					});
				}
			}
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		#endregion

		#region Logs

		void LogViewerMenu_Click(object sender, EventArgs e)
		{
			ShowLogForm();
		}

		IZForm ShowLogForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.ServiceTaskLogViewer);
			var logViewer = new ServiceTaskLogViewer()
			{
				SearchBasedLogViewer = { ServiceTaskCode = "HOST" },
			};
			return controller.ShowEditForm(logViewer);
		}

		#endregion

		#region Menu Text Strings
		public static ResourceStringData InstallLocalMenuData => Res.GetData("6F098937-8C13-4B7C-A83F-A0520F27B08C", "Install Local");
		static string InstallLocalMenuText => InstallLocalMenuData.Caption;
		static MultilingualString StartMenuText => ResString.GetMultilingualString("EB4550D8-3FAF-4813-991C-238814D5C77D", "Start");
		static MultilingualString StopMenuText => ResString.GetMultilingualString("4089C13B-7231-42F2-B493-BCAFFFD538C4", "Stop");
		static ResourceStringData EditHostConfigurationMenuData => Res.GetData("CCD370F7-070C-4DDB-A7D3-72CA15D41795", "Edit Host Configuration");
		public static string EditHostConfigurationMenuText => EditHostConfigurationMenuData.Caption;
		static MultilingualString DeleteDecommissionedMenuText => ResString.GetMultilingualString("FCFAABEA-56DF-4DE2-A3E4-53500EF95764", "Remove Decommissioned Host");
		static MultilingualString LogViewerMenuText => ResString.GetMultilingualString("C5C6949F-9D46-4EC5-BD22-090A995FA1A", "Logs");

		#endregion
	}
}
