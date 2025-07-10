using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.GUI
{
	public partial class StmServiceTaskForm : ZTemplateForm
	{
		StmServiceTaskForm()
		{
			InitializeComponent();
		}

		public StmServiceTaskForm(StmServiceTask serviceTask)
			: base(serviceTask)
		{
			TaskAttributes = serviceTask.StaticServiceAttributes;

			InitializeComponent();

			if (!string.IsNullOrEmpty(TaskAttributes.ConfigControlTypeAssemblyName) && !string.IsNullOrEmpty(TaskAttributes.ConfigControlTypeName))
			{
				Control configControl;
				try
				{
					configControl = (Control)Activator.CreateInstance(Type.GetType($"{TaskAttributes.ConfigControlTypeName}, {TaskAttributes.ConfigControlTypeAssemblyName}", throwOnError: true));
				}
				catch (MissingMethodException)//if we try to create instance of interface
				{
					var parts = TaskAttributes.ConfigControlTypeName.Split('.');
					configControl = (Control)ObjectFactory.Get(parts[parts.Length - 1]);
				}
				catch (TypeLoadException ex)
				{
					configControl = HandleConfigControlException(ex);
				}
				catch (System.IO.FileNotFoundException ex)
				{
					configControl = HandleConfigControlException(ex);
				}

				configControl.Dock = DockStyle.Fill;
				ConfigTabPage.Controls.Add(configControl);
			}
			else
			{
				ConfigTabPage.TabVisible = false;
			}

			ExtendedConfigTabPage.TabVisible = serviceTask.StaticServiceAttributes.AllowsMultipleInstances;

			if (serviceTask.IsNudgeable)
			{
				// Recurrence control will be hidden so we can shift Default schedule into the gap and reduce the window size
				DefaultScheduleControl.Location = RecurrenceControl.Location;
				DefaultScheduleControl.BringToFront();
				RecurrenceControl.Visible = false;
				MinimumSize = ControlDpiScalingHelper.NewScaledSize(
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(MinimumSize.Width),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(MinimumSize.Height) - ControlDpiScalingHelper.UnscaleFromCurrentDpiY(RecurrenceControl.Height)
				);
			}
			else if (serviceTask.IsScheduleReadOnly)
			{
				RecurrenceControl.Enabled = false;
			}

			BranchFindBox.Visible = !serviceTask.StaticServiceAttributes.CanRunInAnyBranch;

			MainTabControl.Controls.Remove(LogsTabPage);
			LogsTabPage.Dispose();
		}

		static Control HandleConfigControlException(Exception ex)
		{
			Control result = new ZLabel() { Name = "ConfigControlError", AutoSize = true, TextAlign = System.Drawing.ContentAlignment.TopLeft, ForeColor = System.Drawing.Color.Red };
			result.Text = ex.Message + "\r\n\r\n" + Res.GetString("E936D48D-9318-4684-812D-B417E3A60FE7", "Starting this Service Task should clear this error.");
			return result;
		}

		ResourceStringData GetFormCaption()
		{
			return Res.GetData("a63d1bd9-1f06-400b-acee-ee66828c3ba4", "Service Schedule Task {0}")
				.Format(((StmServiceTask)BusinessEntity)?.SST_ServiceTaskCode);
		}

		protected readonly IHostedServiceAttribute TaskAttributes;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;
	}
}
