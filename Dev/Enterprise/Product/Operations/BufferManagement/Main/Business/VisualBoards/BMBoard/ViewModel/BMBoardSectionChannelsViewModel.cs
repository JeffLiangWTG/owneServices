using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public enum ChannelAxis
	{
		Primary,
		Secondary,
	}

	public class ChannelsChangedEventArgs : EventArgs
	{
		public ChannelsChangedEventArgs(ChannelAxis axis)
		{
			Axis = axis;
		}

		public ChannelAxis Axis { get; private set; }
	}

	public class BMBoardSectionChannelsViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BMBoardSectionChannelsViewModel(BMComponentSectionConfiguration sectionConfiguration, ChannelAxis axis)
		{
			this.sectionConfiguration = sectionConfiguration;
			this.axis = axis;
		}

		readonly BMComponentSectionConfiguration sectionConfiguration;
		readonly ChannelAxis axis;

		public ChannelAxis Axis
		{
			get { return axis; }
		}

		#region Channels

		public BMBoardSectionChannelCollection PrimaryAxisChannels
		{
			get { return sectionConfiguration.PrimaryAxisChannels; }
		}

		public BMBoardSectionChannelCollection SecondaryAxisChannels
		{
			get { return sectionConfiguration.SecondaryAxisChannels; }
		}

		#endregion

		#region Actions

		public event EventHandler<ChannelsChangedEventArgs> ChannelsChanged;

		public void OnChannelsChanged(ChannelAxis channelAxis)
		{
			if (!sectionConfiguration.IsValidationSuspended)
			{
				sectionConfiguration.Validation.ValidateChannelBy();
				sectionConfiguration.Validation.ValidateChannelSecondaryBy();
			}
			if (ChannelsChanged != null)
			{
				ChannelsChanged(this, new ChannelsChangedEventArgs(channelAxis));
			}
		}

		public void CreateNewChannel(ChannelAxis channelAxis)
		{
			switch (channelAxis)
			{
				case ChannelAxis.Primary:
					PrimaryAxisChannels.AddNew();
					break;
				case ChannelAxis.Secondary:
					SecondaryAxisChannels.AddNew();
					break;
				default:
					throw new InvalidOperationException("unexpected axis");
			}

			OnChannelsChanged(channelAxis);
		}

		public void DeleteChannel(BMBoardSectionChannel channel, ChannelAxis channelAxis)
		{
			channel.Delete();
			OnChannelsChanged(channelAxis);
		}

		public void RemoveIrrelevantChannels()
		{
			if (RemoveIrrelevantChannels(PrimaryAxisChannels, sectionConfiguration.OverrideChannels, sectionConfiguration.ChannelBy))
			{
				OnChannelsChanged(ChannelAxis.Primary);
			}
			if (RemoveIrrelevantChannels(SecondaryAxisChannels, sectionConfiguration.OverrideSecondaryChannels, sectionConfiguration.ChannelSecondaryBy))
			{
				OnChannelsChanged(ChannelAxis.Secondary);
			}
		}

		static bool RemoveIrrelevantChannels(BMBoardSectionChannelCollection channels, bool overrideChannels, string channelBy)
		{
			if (!overrideChannels && !string.IsNullOrEmpty(channelBy))
			{
				var channelsToRemove = channels.Cast<BMBoardSectionChannel>().Where(c => c.MSC_ChannelType != channelBy || c.IsUnChanneled).ToArray();
				foreach (var channel in channelsToRemove)
				{
					channel.Delete();
				}

				return true;
			}

			return false;
		}

		#endregion

		#region Properties

		#region ChannelBy

		[List("ChannelByList")]
		[ResourceStringData("BMBoardSectionChannelsViewModel.ChannelBy", Caption = "Channel By", FullDescription = "Select which type of channels to display on this axis of this board section.")]
		public ZString ChannelBy
		{
			get { return axis == ChannelAxis.Primary ? sectionConfiguration.ChannelBy : sectionConfiguration.ChannelSecondaryBy; }
			set
			{
				if (axis == ChannelAxis.Primary)
				{
					sectionConfiguration.ChannelBy = value;
				}
				else
				{
					sectionConfiguration.ChannelSecondaryBy = value;
				}
			}
		}

		public ZPropertyInfo ChannelByInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ChannelBy), _ => sectionConfiguration == null ? null : axis == ChannelAxis.Primary ? sectionConfiguration.ChannelByInfo : sectionConfiguration.ChannelSecondaryByInfo); }
		}

		public CodeDescriptionPairList ChannelByList
		{
			get { return axis == ChannelAxis.Primary ? sectionConfiguration.Lookups.ChannelByList : sectionConfiguration.Lookups.ChannelSecondaryByList; }
		}

		#endregion

		#region OverrideChannels

		[ResourceStringData("BMBoardSectionChannelsViewModel.OverrideChannels", Caption = "Override Channels", FullDescription = "Use this option to override the default channels and add or remove any channels of any type.")]
		public ZBool OverrideChannels
		{
			get { return axis == ChannelAxis.Primary ? sectionConfiguration.OverrideChannels : sectionConfiguration.OverrideSecondaryChannels; }
			set
			{
				if (axis == ChannelAxis.Primary)
				{
					sectionConfiguration.OverrideChannels = value;
				}
				else
				{
					sectionConfiguration.OverrideSecondaryChannels = value;
				}

				ChannelByInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OverrideChannelsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OverrideChannels), _ => sectionConfiguration == null ? null : axis == ChannelAxis.Primary ? sectionConfiguration.OverrideChannelsInfo : sectionConfiguration.OverrideSecondaryChannelsInfo); }
		}

		#endregion

		#region ShowUnchanneled

		[ResourceStringData("BMBoardSectionChannelsViewModel.ShowUnchanneled", Caption = "Show Un-channeled", FullDescription = "Shows a channel for all valid tasks which do not belong in any channel.")]
		public ZBool ShowUnchanneled
		{
			get { return axis == ChannelAxis.Primary ? sectionConfiguration.ShowUnchanneled : sectionConfiguration.ShowSecondaryUnchanneled; }
			set
			{
				if (axis == ChannelAxis.Primary)
				{
					sectionConfiguration.ShowUnchanneled = value;
				}
				else
				{
					sectionConfiguration.ShowSecondaryUnchanneled = value;
				}
			}
		}

		public ZPropertyInfo ShowUnchanneledInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ShowUnchanneled), _ => sectionConfiguration == null ? null : axis == ChannelAxis.Primary ? sectionConfiguration.ShowUnchanneledInfo : sectionConfiguration.ShowSecondaryUnchanneledInfo); }
		}

		#endregion

		#region SortAlphabetically

		[ResourceStringData("BMBoardSectionChannelsViewModel.SortChannels", Caption = "Display Channels Alphabetically", FullDescription = "Display these channels on a Visual Board in alphabetical order according to their description.")]
		public ZBool SortChannels
		{
			get { return axis == ChannelAxis.Primary ? sectionConfiguration.SortPrimaryChannels : sectionConfiguration.SortSecondaryChannels; }
			set
			{
				if (axis == ChannelAxis.Primary)
				{
					sectionConfiguration.SortPrimaryChannels = value;
				}
				else
				{
					sectionConfiguration.SortSecondaryChannels = value;
				}
			}
		}

		public ZPropertyInfo SortChannelsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SortChannels), _ => sectionConfiguration == null ? null : axis == ChannelAxis.Primary ? sectionConfiguration.SortPrimaryChannelsInfo : sectionConfiguration.SortSecondaryChannelsInfo); }
		}

		#endregion

		#endregion
	}
}
