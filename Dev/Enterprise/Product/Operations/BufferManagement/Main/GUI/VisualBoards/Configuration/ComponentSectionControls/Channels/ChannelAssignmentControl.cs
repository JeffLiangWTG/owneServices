using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ChannelAssignmentControl : ZUserControl
	{
		public ChannelAssignmentControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				this.SetDoubleBuffered(true);
				ChannelsPanel.SetDoubleBuffered(true);
			}

			AddChannelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var parentForm = FindForm() as ZForm;
			var factory = parentForm?.BusinessEntity?.Factory;

			if (factory != null)
			{
				factory.Saving += Factory_Saving;
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			UpdateChannels();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var parentForm = FindForm() as ZForm;
				var factory = parentForm?.BusinessEntity?.Factory;

				if (factory != null)
				{
					factory.Saving -= Factory_Saving;
				}
			}

			base.Dispose(disposing);

			if (disposing)
			{
				ViewModel = null;
			}
		}

		[DefaultValue(null)]
		public BMBoardSectionChannelsViewModel ViewModel
		{
			get { return viewModel; }
			set
			{
				if (value != null)
				{
					value.ChannelsChanged += ViewModel_ChannelsChanged;
					value.ChannelByInfo.ValueChanged += ChannelByInfo_ValueChanged;
					value.OverrideChannelsInfo.ValueChanged += UpdateChannelIsRequired;
					value.ShowUnchanneledInfo.ValueChanged += UpdateChannelIsRequired;
					value.SortChannelsInfo.ValueChanged += SortChannelsInfo_ValueChanged;
				}
				if (viewModel != null)
				{
					viewModel.ChannelsChanged -= ViewModel_ChannelsChanged;
					viewModel.ChannelByInfo.ValueChanged -= ChannelByInfo_ValueChanged;
					viewModel.OverrideChannelsInfo.ValueChanged -= UpdateChannelIsRequired;
					viewModel.ShowUnchanneledInfo.ValueChanged -= UpdateChannelIsRequired;
					viewModel.SortChannelsInfo.ValueChanged -= SortChannelsInfo_ValueChanged;
				}

				viewModel = value;
				UpdateChannels();
			}
		}

		BMBoardSectionChannelsViewModel viewModel;

		BMBoardSectionChannelCollection Channels
		{
			get { return ViewModel == null ? null : ViewModel.Axis == ChannelAxis.Primary ? ViewModel.PrimaryAxisChannels : ViewModel.SecondaryAxisChannels; }
		}

		internal event EventHandler ChannelsChanged;

		void ViewModel_ChannelsChanged(object sender, ChannelsChangedEventArgs e)
		{
			if (e.Axis == ViewModel.Axis)
			{
				UpdateChannels();

				if (ChannelsChanged != null)
				{
					ChannelsChanged(this, EventArgs.Empty);
				}
			}
		}

		void UpdateChannels()
		{
			var sortChannels = ViewModel?.SortChannels ?? false;
			var channels = Channels;

			if (channels != null)
			{
				ChannelsPanel.SuspendDrawing();
				try
				{
					var scrollPosition = ChannelsPanel.VerticalScroll.Value;
					ChannelsPanel.Controls.RemoveAndDisposeAll();

					Control channelControl = null;
					BMBoardSectionChannel[] orderedChannels = null;

					if (sortChannels)
					{
						orderedChannels = channels.Cast<BMBoardSectionChannel>().OrderBy(c => c.BizoDescription).ToArray();

						var sequence = 1;
						foreach (var channel in orderedChannels)
						{
							channel.MSC_Sequence = sequence++;
						}
					}
					else
					{
						orderedChannels = channels.Cast<BMBoardSectionChannel>().OrderBy(c => c.MSC_Sequence).ToArray();
					}

					for (int i = 0; i < orderedChannels.Length; i++)
					{
						var channel = orderedChannels[i];
						var top = channelControl != null ? channelControl.Height * i : 0;
						channelControl = GetAndAddChannelControl(channel, top);
					}

					// Setting the scrollbar position once has no effect. Possible a bug in WinForms. See: http://stackoverflow.com/questions/262534/how-to-scroll-a-panel-manually#comment15236167_263590
					ChannelsPanel.VerticalScroll.Value = ChannelsPanel.VerticalScroll.Value = scrollPosition;
				}
				finally
				{
					ChannelsPanel.ResumeDrawing();
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		ChannelRowControl GetAndAddChannelControl(BMBoardSectionChannel channel, int top)
		{
			var channelControl = new ChannelRowControl(ViewModel, channel, ViewModel.Axis);

			try
			{
				channelControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, top, false);
				ChannelsPanel.Controls.Add(channelControl);

				return channelControl;
			}
			catch
			{
				try
				{
					channelControl.Dispose();
				}
				catch { }
				throw;
			}
		}

		void ChannelByInfo_ValueChanged(object sender, EventArgs e)
		{
			ViewModel.RemoveIrrelevantChannels();
		}

		void UpdateChannelIsRequired(object sender, EventArgs e)
		{
			UpdateChannels();
		}

		void AddChannelButton_Click(object sender, EventArgs e)
		{
			if (ViewModel == null)
			{
				Globals.Message.ShowWarning(Res.GetString("c56bb91c-c23f-4150-8e6f-f04a73bf800a", "Cannot add a new channel when no section is selected."));
			}
			else if (!ViewModel.OverrideChannels)
			{
				Globals.Message.ShowWarning(Res.GetString("1EA43CA1-7E2B-4A5A-B43E-27B711BA1A2D", "Cannot add a new channel unless 'Override Channels' is ticked."));
			}
			else
			{
				ViewModel.CreateNewChannel(ViewModel.Axis);
			}
		}

		void SortChannelsInfo_ValueChanged(object sender, EventArgs e)
		{
			SortCheckBox.Checked = ViewModel.SortChannels;
			UpdateChannels();
		}
	}
}
