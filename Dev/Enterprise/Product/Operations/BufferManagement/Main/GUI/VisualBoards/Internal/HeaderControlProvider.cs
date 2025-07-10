using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public static class HeaderControlProvider
	{
		public static Control GetHeaderControl(CellContent cell, BMComponentControl componentControl, BMBoardSectionViewModel viewModel, Func<Control> controlToWrapGetter)
		{
			return new HeaderWrapperControl(controlToWrapGetter(), cell, componentControl, viewModel);
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		public class HeaderWrapperControl : Control
		{
			public HeaderWrapperControl(Control controlToWrap, CellContent cell, BMComponentControl componentControl, BMBoardSectionViewModel viewModel)
			{
				this.Controls.Add(controlToWrap);
				controlToWrap.Dock = DockStyle.Fill;
				this.Size = controlToWrap.Size;

				this.channel = cell.Channel ?? cell.SecondaryChannel;
				this.cell = cell;
				this.componentControl = componentControl;
				this.viewModel = viewModel;

				if (!viewModel.IsPreview)
				{
					if (controlToWrap.Controls.Count == 0)
					{
						HookupControl(controlToWrap);
					}
					else
					{
						foreach (Control control in controlToWrap.Controls)
						{
							HookupControl(control);
						}
					}
				}

				cell.BackgroundColorChanged += Cell_BackgroundColorChanged;

				if (controlToWrap.ContextMenuStrip is LazyContextMenuStrip lazyMenu)
				{
					lazyMenu.ItemsAdding += AddCapacityMenuItem;
				}
			}

			void AddCapacityMenuItem(object sender, EventArgs e)
			{
				var menu = (KContextMenuStrip)sender;

				var menuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("5951bc8e-2762-4bfd-aade-51106d213383", "Show Capacity Details"), MenuItemToShowDetailsOnClick);
				menu.Items.Add(menuItem);
			}

			void MenuItemToShowDetailsOnClick(object sender, EventArgs eventArgs)
			{
				ShowChannelCapacity();
			}

			readonly IVisualBoardChannel channel;
			protected readonly CellContent cell;
			readonly BMComponentControl componentControl;
			readonly BMBoardSectionViewModel viewModel;

			internal BMComponentControl ComponentControl => componentControl;

			void HookupControl(Control control)
			{
				control.MouseClick += ChildControl_Click;

				foreach (Control child in control.Controls)
				{
					HookupControl(child);
				}
			}

			void ChildControl_Click(object sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left && IsHeaderExpansionEnabled(sender as Control))
				{
					componentControl.HeaderClicked(cell);
				}
			}

			bool IsHeaderExpansionEnabled(Control control)
			{
				// Hack: Sometimes these controls override mouseclick.
				var channelStatusPictureBox = control as ChannelStatusPictureBox;
				var statusLabel = control as ChannelStatusLabel;

				if (channelStatusPictureBox == null && statusLabel == null)
				{
					return true;
				}

				return channelStatusPictureBox != null && !channelStatusPictureBox.IsClickable ||
					statusLabel != null && !statusLabel.IsClickable;
			}

			void Cell_BackgroundColorChanged(object sender, EventArgs e)
			{
				this.BeginInvokeSafe(() =>
				{
					BackColor = string.IsNullOrEmpty(cell.BackColor?.Name) ? BMConstants.BackgroundDefaultColor : cell.BackColor.Value;
					SetAgeHeadingLabelBold();
				});
			}

			void SetAgeHeadingLabelBold()
			{
				if (cell.ContentType == CellContentType.AgeHeading && Controls.Count > 0)
				{
					var label = Controls[0] as Label;

					if (label != null)
					{
						var isBold = label.Font.Bold;
						if (cell.IsBoldLabel && !isBold)
						{
							label.Font = new Font(label.Font, FontStyle.Bold);
						}
						else if (!cell.IsBoldLabel && isBold)
						{
							label.Font = new Font(label.Font, FontStyle.Regular);
						}
					}
				}
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if (disposing)
				{
					cell.BackgroundColorChanged -= Cell_BackgroundColorChanged;
				}
			}

			#region Capacity

			public void ShowChannelCapacity()
			{
				if (viewModel.ComponentGrid.IsReadyToCalculateChannelCapacity)
				{
					ShowChannelCapacityCore();
				}
			}

			void ShowChannelCapacityCore()
			{
				var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": ShowChannelCapacity", RefreshEnabled = false };
				var section = factory.Load<BMBoardSection>(viewModel.SectionPK);

				if (section != null)
				{
					var capacityMessage = GetCapacityMessage(channel, cell, section, viewModel);

					if (!IsDisposed)
					{
						Globals.Message.Show(capacityMessage.Item1, capacityMessage.Item2, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}

			static Tuple<string, string> GetCapacityMessage(IVisualBoardChannel channel, CellContent cell, BMBoardSection section, BMBoardSectionViewModel viewModel)
			{
				var capacity = new ChannelCapacity(channel, cell, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
				return Tuple.Create(capacity.Message, capacity.Caption);
			}

			#endregion

			#region Test Only

#if DEBUG
			public void MouseClick_ForTest()
			{
				ChildControl_Click(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
			}

			public CellContentType ContentType
			{
				get { return cell.ContentType; }
			}
#endif

			#endregion
		}
	}
}
