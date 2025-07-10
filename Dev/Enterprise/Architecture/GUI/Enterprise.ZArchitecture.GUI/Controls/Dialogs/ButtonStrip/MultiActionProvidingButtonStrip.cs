using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class MultiActionProvidingButtonStrip<T> : KTableLayoutPanel where T : struct, IConvertible
	{
		public MultiActionProvidingButtonStrip(params ButtonStripAction<T>[] actions)
			: this(null, actions)
		{
		}

		public MultiActionProvidingButtonStrip(Action afterPress, params ButtonStripAction<T>[] actions)
		{
			ColumnCount = actions.Length;
			RowCount = 1;
			Dock = DockStyle.Fill;

			for (var i = 0; i < actions.Length; i++)
			{
				var button = GetButton(afterPress, actions[i]);
				ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / actions.Length));
				Controls.Add(button, i, 1);
			}

			ControlDpiScalingHelper.SetWidth(this, actions.Length * BASE_BUTTON_WIDTH, true);
		}

		const int BASE_BUTTON_WIDTH = 100;

		ZButton GetButton(Action afterPress, ButtonStripAction<T> viewModel)
		{
			var text = viewModel.Image != null ? System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + viewModel.Text : viewModel.Text;
			var button = new ZButton
			{
				Dock = DockStyle.Fill,
				Text = text,
				Image = viewModel.Image,
				Tag = viewModel.Response,
				Name = viewModel.Name ?? viewModel.Text,
				ToolTipCaption = viewModel.ToolTip,
			};

			button.Click += (s, e) =>
			{
				var form = this.FindForm();
				if (form != null)
				{
					form.Tag = viewModel.Response;
				}

				if (viewModel.FireAction != null)
				{
					if (viewModel.HideFormBeforeFireAction)
					{
						form.Visible = false;
					}

					viewModel.FireAction(s, e);
				}

				if (afterPress != null)
				{
					afterPress();
				}
			};

			return button;
		}
	}
}
