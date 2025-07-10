using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class LegendForm : ZChildForm
	{
		public LegendForm()
		{
			InitializeComponent();

			SetupIndicators();

			MainStatusBar.Visible = false;
			ControlDpiScalingHelper.SetHeight(this, this.Height - MainStatusBar.Height, false);

			CloseLegendButton.AllowOutsideOfParent();
		}

		void SetupIndicators()
		{
			var statuses = new[] { ProcessTaskStatusCodeList.Codes.Working, ProcessTaskStatusCodeList.Codes.Suspended, CurrentTaskIdentifier, NextAvailableResourceIdentifier,
				CcrPreConstraintResourceIdentifier, CcrPostConstraintResourceIdentifier, CcrPreConstraintAtRiskResourceIdentifier, CcrPostConstraintAtRiskResourceIdentifier };
			AddControls(statuses, GetStatusControl, TaskStatusPanel);
		}

		static void AddControls<T>(IEnumerable<T> items, Func<T, int, Control> controlGenerator, ScrollableControl panelToAddControls)
		{
			foreach (var item in items)
			{
				var control = controlGenerator(item, panelToAddControls.Width);
				if (control != null)
				{
					panelToAddControls.Controls.Add(control);

					if (control.Bottom > panelToAddControls.Height)
					{
						panelToAddControls.AutoScroll = true;
						panelToAddControls.VerticalScroll.Visible = true;
					}
				}
			}
		}

		Control GetStatusControl(string status, int maxWidth)
		{
			Image image;
			string message;

			switch (status)
			{
				case ProcessTaskStatusCodeList.Codes.Working:
					image = StatusIndicatorControl.WorkingStatusImage;
					message = Res.GetString("b67343e0-a1f8-4972-940f-91dbfb891afd", "Task is Working");
					break;

				case ProcessTaskStatusCodeList.Codes.Suspended:
					image = StatusIndicatorControl.SuspendedStatusImage;
					message = Res.GetString("a36d0e79-4ab3-4048-bf0d-84c46df6f4f7", "Task is Suspended");
					break;

				case CurrentTaskIdentifier:
					image = StatusIndicatorControl.CurrentTaskImage;
					message = Res.GetString("4785922f-7818-4566-b472-b5075d959b11", "For task cards, task is startable (task which can be worked on). For workflow cards, a workflow that has a startable task.");
					break;

				case NextAvailableResourceIdentifier:
					image = StatusIndicatorControl.NextAvailableResourceImage;
					message = Res.GetString("c41df76c-1a17-454d-88d5-bd9d5260412b", "Task has not been assigned to a resource");
					break;

				case CcrPreConstraintResourceIdentifier:
					image = StatusIndicatorControl.CCRPreconstraintTaskImage;
					message = Res.GetString("41ac7829-2ed9-4c58-9db8-df0cf870e184", "This task precedes another task on the same workflow that is assigned to a CCR.");
					break;

				case CcrPostConstraintResourceIdentifier:
					image = StatusIndicatorControl.CCRPostconstraintTaskImage;
					message = Res.GetString("2fb45408-d82d-41b8-9ca4-aa31537abdbb", "This task follows another task on the same workflow that is assigned to a CCR.");
					break;

				case CcrPreConstraintAtRiskResourceIdentifier:
					image = StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage;
					message = Res.GetString("37531e1a-afef-496e-b351-cc2d42305367", "This task precedes another task on the same workflow that is assigned to a CCR and is in Zone 1/0 of the relevant buffer.");
					break;

				case CcrPostConstraintAtRiskResourceIdentifier:
					image = StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage;
					message = Res.GetString("77e4108b-4c62-4c46-afdc-d3cad0a50dd6", "This task follows another task on the same workflow that is assigned to a CCR and is in Zone 1/0 of the relevant buffer.");
					break;

				default:
					throw new NotSupportedException("No control defined for status " + status);
			}

			maxWidth -= image.Width;

			return GetImagePanel(image, message, maxWidth);
		}

		const string CurrentTaskIdentifier = "CUR";
		const string NextAvailableResourceIdentifier = "NAR";
		const string CcrPreConstraintResourceIdentifier = "CPR";
		const string CcrPostConstraintResourceIdentifier = "CPS";
		const string CcrPreConstraintAtRiskResourceIdentifier = "CRR";
		const string CcrPostConstraintAtRiskResourceIdentifier = "CRS";

		Panel GetImagePanel(Image image, string description, int maxWidth)
		{
			return GetImagePanel(image, description, maxWidth, ControlDpiScalingHelper.NewScaledSize(ImageSize, ImageSize));
		}

		Panel GetImagePanel(Image image, string description, int maxWidth, Size imageSize)
		{
			image = new Bitmap(image, imageSize.Width, imageSize.Height);
			var pictureBox = new OptimisticPictureBox(shouldDisposeImageOnControlDispose: true)
			{
				Image = image,
				MaximumSize = ControlDpiScalingHelper.NewScaledSize(maxWidth, 0, false),
				Size = ControlDpiScalingHelper.NewScaledSize(imageSize.Width, imageSize.Height, false),
			};
			return GetPanelWithGraphicAndDescription(pictureBox, description, maxWidth);
		}

		static Panel GetPanelWithGraphicAndDescription(Control indicator, string description, int maxWidth)
		{
			var panel = new ZPanel
			{
				AutoSize = true,
				BackColor = Color.Transparent,
			};
			ControlDpiScalingHelper.SetHeight(panel, ImageSize, true);
			var label = new ZLabel
			{
				AutoSize = true,
				MaximumSize = ControlDpiScalingHelper.NewScaledSize(maxWidth - indicator.Width, 0, false),
				Text = description,
			};
			ControlDpiScalingHelper.SetLeft(label, indicator.Width, false);
			panel.Controls.Add(indicator);
			panel.Controls.Add(label);
			ControlDpiScalingHelper.SetWidth(ref panel, label.Left + label.Width, false);

			return panel;
		}

		const int ImageSize = 12;

		void CloseLegendButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
