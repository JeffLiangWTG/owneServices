using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class AnalysisServerControl : ZUserControl
	{
		#region Load
		public AnalysisServerControl()
		{
			this.SkipSettingChildControlReadOnly = true;
			InitializeComponent();

			ssasCubesGrid.IsWholeRowSelectedOnClick = true;
			if (IsInternalOrSupport)
			{
				var menuItemCollection = new[]
					{
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("e3e6e75e-8000-4d74-b30b-61f959e1eda7", "Activate"), activateModelsMenu),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("7d903d8c-ab19-462c-8de7-2e5c03ab04f1", "Deactivate"), deactivateModelsMenu),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("5e1544e7-8c67-4c0b-9071-ef8f82864024", "Reprocess"), reprocessModelsMenu),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("060D0D1E-3EDA-4AAE-BD81-7CE9DE0E5D4F", "Redeploy"), redeployModelsMenu),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("A6F15984-747D-477A-B242-6CE81B808F37", "Cancel Redeploy"), cancelRedeployModelsMenu),
						new ZMenuItem("-"),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("C4C6E953-E96B-40EC-A8BF-2D452D33ED2E", "Enable ETL"), enableEtlMenu),
						new ZMenuItem(CargoWise.Bi.Product.Manager.GUI.Res.GetData("09BA4729-2650-4E3E-8DA9-D36E6D81E2F1", "Disable ETL"), disableEtlMenu),
						new ZMenuItem("-")
					};

				ssasCubesGrid.ContextMenu.MenuItems.InsertRange(0, menuItemCollection);
			}
		}

		public virtual bool IsInternalOrSupport
		{
			get
			{
				if (isInternalOrSupport == null)
				{
					var registration = ObjectFactory.Get<IProductRegistration>();
					var isInternal = registration.LocalVerify() == ProductRegistrationVerifyResult.OK && registration.IsWiseTechGlobalInternalSystem();
					var isSupport = !GlbStaff.CurrentUser.IsNull && (GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper);

					isInternalOrSupport = isInternal || isSupport;
				}
				return isInternalOrSupport.Value;
			}
		}
		bool? isInternalOrSupport;

		void ssasCubesGrid_MouseClick(object sender, MouseEventArgs e)
		{
			SsasCubesGridMenuCore();
		}

		public void SsasCubesGridMenuCore()
		{
			if (IsInternalOrSupport)
			{
				var selectedModels = ssasCubesGrid.SelectedElements.Select(r => ((SsasCube)r).ModelFileName);
				if (selectedModels.Any())
				{
					SetSupportContextMenuItemVisibility(true);
				}
				else
				{
					SetSupportContextMenuItemVisibility(false);
				}
			}
		}

		void SetSupportContextMenuItemVisibility(bool value)
		{
			var menuItems = ssasCubesGrid.ContextMenu.MenuItems;

			menuItems[0].Visible = value;
			menuItems[1].Visible = value;
			menuItems[2].Visible = value;
			menuItems[3].Visible = value;
			menuItems[4].Visible = value;
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			ThreadRunner.RunInAnotherThread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					RefreshInfo();
				}
			});
		}

		public void RefreshInfo()
		{
			var tokenSource = new CancellationTokenSource();
			var parentForm = (this.ParentForm as BiManagerForm);
			parentForm?.ShowLoadingTextAsync(tokenSource.Token);

			var bo = (BindingSource.DataSource as BiMonitorBusinessObject);
			if (bo != null)
			{
#if !WINZOR
				bo.RefreshAnalysisCubeInformation();
#endif
			}
			if (!parentForm.IsDisposed && !parentForm.IsDisposing)
			{
				Invoke(new Action(() =>
				{
					ssasCubesGrid.SetDataBinding(bo.AnalysisServerInfo, "SsasCubes");
					RecreateHandle();
				}));
			}

			tokenSource.Cancel();
		}

#endregion //Load

		#region Ssas Cube Options

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void activateModelsMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Activate", modelManager.ActivateModels);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void deactivateModelsMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Deactivate", modelManager.DeactivateModels);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void reprocessModelsMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Reprocess", modelManager.ReprocessModels);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void redeployModelsMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Redeploy", modelManager.RedeployModels);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void disableEtlMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Disable ETL", modelManager.DisableEtlOnModel);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void enableEtlMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Enable ETL", modelManager.EnableEtlOnModel);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu option")]
		void cancelRedeployModelsMenu(object sender, EventArgs e)
		{
			using (var modelManager = new SsasModelManager())
			{
				processRequestInAnotherThread("Cancel Redeploy", modelManager.cancelRedeployModels);
			}
		}

		void processRequestInAnotherThread(string request, Action<IEnumerable<ZString>> action)
		{
			var selectedModels = ssasCubesGrid.SelectedElements.Select(r => ((SsasCube)r).ModelFileName);
			processRequestInAnotherThread(request, action, selectedModels);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message box")]
		void processRequestInAnotherThread(string request, Action<IEnumerable<ZString>> action, IEnumerable<ZString> selectedModels)
		{
			string message;
			if (selectedModels.Any())
			{
#if DEBUG
				if (Globals.IsTest)
				{
					action(selectedModels);
					RefreshInfo();
				}
				else
#endif
				{
					if (selectedModels.Count() > 20)
					{
						message = string.Format(CultureInfo.InvariantCulture, "This will {0} {1} tabular model(s) during the next BI Deployment task execution. Are you sure?", request.ToUpperInvariant(), selectedModels.Count());
					}
					else
					{
						message = string.Format(CultureInfo.InvariantCulture, "This will {0} the following tabular models during the next BI Deployment task execution:\r\n\t{1}\r\n\r\nAre you sure?", request.ToUpperInvariant(), string.Join("\r\n\t", selectedModels));
					}
					using (var msgBox = new ZMessageBox(message, $"{request} Tabular Models", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
					{
						if (msgBox.ShowDialog() == DialogResult.Yes)
						{
							action(selectedModels);
							RefreshInfo();
						}
					}
				}
			}
		}
		#endregion // SsasCube Options
	}
}
