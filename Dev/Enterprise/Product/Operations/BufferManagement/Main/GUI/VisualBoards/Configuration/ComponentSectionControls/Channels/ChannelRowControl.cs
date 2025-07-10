using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ChannelRowControl : ZUserControl
	{
		public ChannelRowControl()
		{
			InitializeComponent();
		}

		public ChannelRowControl(BMBoardSectionChannelsViewModel viewModel, BMBoardSectionChannel channel, ChannelAxis axis)
		{
			this.viewModel = viewModel;
			this.channel = channel;
			this.axis = axis;
			channel.MSC_ChannelTypeInfo.ValueChanged += ChannelTypeCodeInfo_ValueChanged;

			InitializeComponent();
			ControlDpiScalingHelper.SetHeight(this, 30, true);

			this.SetDoubleBuffered(true);
			ChannelPickerPanel.SetDoubleBuffered(true);

			SetDataBinding(channel, string.Empty);
			ShowChannelSelectionControl();

#if !WINZOR
			var handler = DragDropHelper.AddDragDropSupport(this, new DragDropDescriptor { AllowDragOutsideParentBounds = false, AllowHorizontalDrag = false });
			handler.DragFinished += DragDropHandler_DragFinished;
#endif
		}

		readonly BMBoardSectionChannelsViewModel viewModel;
		readonly BMBoardSectionChannel channel;
		readonly ChannelAxis axis;

#if DEBUG
		public BMBoardSectionChannel Channel_ForTesting
		{
			get { return channel; }
		}
#endif

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (channel?.MSC_ChannelTypeInfo is not null)
				{
					channel.MSC_ChannelTypeInfo.ValueChanged -= ChannelTypeCodeInfo_ValueChanged;
				}
				DeleteButton.Click -= DeleteButton_Click;
			}
		}

		#region Event handlers

		void DeleteButton_Click(object sender, EventArgs e)
		{
			if (channel.UseDefaultChannels)
			{
				Globals.Message.ShowWarning(Res.GetString("d2db3034-cb53-4ef7-a06a-bf6ea9226e3f", "Cannot delete this channel since this section uses default channels."));
			}
			else
			{
				viewModel.DeleteChannel(channel, axis);
			}
		}

		void DragDropHandler_DragFinished(object sender, MouseEventArgs e)
		{
			OnFinishDragging();
		}

		#endregion

		#region Drag/Drop

#if DEBUG
		public
#endif
 void OnFinishDragging()
		{
			var channelControls = Parent.Controls.OfType<ChannelRowControl>().OrderBy(r => r.Top).ThenByDescending(r => r == this).ToArray();

			var sequence = 1;
			foreach (var control in channelControls)
			{
				control.channel.MSC_Sequence = sequence++;
			}

			viewModel.SortChannels = false;
			viewModel.OnChannelsChanged(axis);
		}

		#endregion

		#region ShowChannelSelectionControl

		void ChannelTypeCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowChannelSelectionControl();
		}

		void ShowChannelSelectionControl()
		{
			foreach (var control in ChannelPickerPanel.Controls.Cast<Control>().ToArray())
			{
				ChannelPickerPanel.Controls.Remove(control);
				control.Dock = DockStyle.None;
				control.Visible = false;
				this.Controls.Add(control);
			}

			var selectionControl = channel.IsUnChanneled || channel.IsCurrentUser ? null : ChannelEntityFindBox;
			if (selectionControl != null)
			{
				this.Controls.Remove(selectionControl);
				selectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
				ControlDpiScalingHelper.SetWidth(ref selectionControl, ChannelPickerPanel.Width, false);
				ChannelPickerPanel.Controls.Add(selectionControl);
				selectionControl.Visible = true;
			}
		}

		#endregion
	}
}
