using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	internal class UnBufferedGridBuilder : ComponentGridBuilderBase
	{
		internal UnBufferedGridBuilder(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section)
			: base(grid, primaryChannels, secondaryChannels, section)
		{
		}

		#region Cell Type

		protected override CellContentType GetCellType(int primaryIndex, int secondaryIndex)
		{
			if (remainingPrimaryAxisChannels.Count == 0 && IsChanneled)
			{
				return CellContentType.None;
			}
			if (secondaryIndex == 0 && IsChanneled)
			{
				return GetHeaderCellType(primaryIndex);
			}
			else
			{
				return GetNonHeaderCellType(primaryIndex);
			}
		}

		CellContentType GetHeaderCellType(int primaryIndex)
		{
			if (primaryIndex == 0 && HasSecondaryChannelsOrderedByTimeCode)
			{
				return CellContentType.Label;
			}
			else
			{
				return CellContentType.ChannelHeading;
			}
		}

		CellContentType GetNonHeaderCellType(int primaryIndex)
		{
			if (primaryIndex == 0 && (HasSecondaryChannelsOrderedByTimeCode || sectionConfiguration.IsWrapped))
			{
				if (sectionConfiguration.ChannelSecondaryBy != BMConstants.ChannelByTimeCode || sectionConfiguration.IsReleaseScheduler)
				{
					return CellContentType.ChannelHeading;
				}
				else
				{
					return CellContentType.AgeHeading;
				}
			}
			else if (IsChanneled || primaryIndex == 0 || ((sectionConfiguration.CellsPerSubsection != 1 || sectionConfiguration.IsWrapped) && primaryIndex == 1))
			{
				return CellContentType.Cards;
			}
			else
			{
				return CellContentType.None;
			}
		}

		#endregion

		#region Card Sort Type

		protected override void SetCardSortType(CellContent cell)
		{
			if (cell.SecondaryChannel is ReleaseSchedulerReleasedChannel)
			{
				cell.CardSortType = CardSortType.LastTransferTime;
			}
		}

		#endregion
	}
}
