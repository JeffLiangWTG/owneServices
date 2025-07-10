using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class DefaultChannelsProvider
	{
		public static IEnumerable<string> SupportedChannelTypes
		{
			get { return new[] { ChannelTypeList.Codes.Resource, ChannelTypeList.Codes.NotChanneled, ChannelTypeList.Codes.ReleaseSchedulerChannels, }; }
		}

		static bool IsWebOrWebServices => Globals.IsWeb || Globals.IsWebService;

		public static void RefreshChannels(BMComponentSectionConfiguration sectionConfiguration, BusinessObjectFactory factory)
		{
			AddChannelsFetchHints(sectionConfiguration, factory);

			PurgeInvalidChannels(sectionConfiguration, factory);
			AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.SecondaryAxisChannels, sectionConfiguration.ChannelSecondaryBy, sectionConfiguration.OverrideSecondaryChannels);
			AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.PrimaryAxisChannels, sectionConfiguration.ChannelBy, sectionConfiguration.OverrideChannels);
		}

		static void AddChannelsFetchHints(BMComponentSectionConfiguration sectionConfiguration, BusinessObjectFactory factory)
		{
			AddResourceChannelsFetchHints(sectionConfiguration, factory);
			AddOtherChannelsFetchHints(sectionConfiguration, factory);
		}

		static void AddResourceChannelsFetchHints(BMComponentSectionConfiguration sectionConfiguration, BusinessObjectFactory factory)
		{
			if ((sectionConfiguration.ChannelBy == ChannelTypeList.Codes.Resource && !sectionConfiguration.OverrideChannels) ||
				sectionConfiguration.ChannelSecondaryBy == ChannelTypeList.Codes.Resource && !sectionConfiguration.OverrideSecondaryChannels)
			{
				AddReleaseGroupChannelsFetchHints(sectionConfiguration.ApplicableReleaseGroupPK, factory);
			}
			else
			{
				var staffPKs = sectionConfiguration.Channels
					.Where(channel => channel.MSC_ChannelType == ChannelTypeList.Codes.Resource)
					.Select(channel => channel.EntityPK)
					.ToArray();

				var query = new ZQuery(GlbStaffSchema.PK, staffPKs);

				if (!IsWebOrWebServices)
				{
					query.IncludeBlob(GlbStaffSchema.GS_ProfilePhoto);
				}

				factory.AddFetchHint(GlbStaffSchema.Instance, query);
			}
		}

		static void AddOtherChannelsFetchHints(BMComponentSectionConfiguration sectionConfiguration, BusinessObjectFactory factory)
		{
			var otherChannels = sectionConfiguration.Channels
					.Where(channel => channel.MSC_ChannelType != ChannelTypeList.Codes.Resource)
					.Select(channel => new
					{
						businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(channel.MSC_ParentTableCode),
						PK = channel.MSC_ParentID
					})
					.ToArray();

			if (otherChannels.Any())
			{
				foreach (var channel in otherChannels)
				{
					factory.AddFetchHint(channel.businessObjectType, channel.PK);
				}
			}
		}

		static void AddReleaseGroupChannelsFetchHints(ZGuid releaseGroupPK, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			var groupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);

			groupLinkSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, releaseGroupPK);
			query.AddSubQuery(groupLinkSubQuery, JoinCondition.And);

			if (!IsWebOrWebServices)
			{
				query.IncludeBlob(GlbStaffSchema.GS_ProfilePhoto);
			}

			factory.AddFetchHint(GlbStaffSchema.Instance, query);
		}

		static void PurgeInvalidChannels(BMComponentSectionConfiguration sectionConfiguration, BusinessObjectFactory factory)
		{
			PurgeInvalidChannels(sectionConfiguration.PrimaryAxisChannels, sectionConfiguration.ChannelBy, sectionConfiguration.OverrideChannels, factory);
			PurgeInvalidChannels(sectionConfiguration.SecondaryAxisChannels, sectionConfiguration.ChannelSecondaryBy, sectionConfiguration.OverrideSecondaryChannels, factory);
		}

		static void PurgeInvalidChannels(BMBoardSectionChannelCollection channels, string channelBy, bool overrideChannels, BusinessObjectFactory factory)
		{
			var invalidChannels = channels
				.Cast<BMBoardSectionChannel>()
				.Where(channel => IsInvalidChannel(channel, channelBy, overrideChannels, factory))
				.ToArray();

			foreach (var channel in invalidChannels)
			{
				channel.IsPurged = true;
				channel.Delete();
			}
		}

		public static void AddMissingDefaultChannels(BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionChannelCollection channels, string channelBy, bool overrideChannels)
		{
			if (!overrideChannels && channelBy == ChannelTypeList.Codes.Resource)
			{
				channels.IsMissingChannelAdded = true;

				foreach (var resource in GetResources(sectionConfiguration, channels))
				{
					var channel = channels.AddNew();
					channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
					channel.MSC_ParentID = resource.PK;
				}

				channels.IsMissingChannelAdded = false;
			}
		}

		static bool IsInvalidChannel(BMBoardSectionChannel channel, string channelBy, bool overrideChannels, BusinessObjectFactory factory)
		{
			if (channel.IsUnChanneled && channel.Section.SectionConfiguration.ShowUnchanneled)
			{
				return false;
			}

			var channelBusinessObject = channel.GetChannelBusinessObject(factory);

			return channelBusinessObject == null &&
				(channel.MSC_ChannelType == channelBy || overrideChannels) &&
				!string.IsNullOrEmpty(channel.MSC_ChannelType) &&
				channel.MSC_ParentID.IsValid ||
				!channel.ShouldParentBeShown(overrideChannels, factory);
		}

		#region Staff 

		static IEnumerable<GlbStaff> GetResources(BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionChannelCollection channels)
		{
			var releaseGroup = sectionConfiguration.Factory.Load<GlbGroup>(sectionConfiguration.ApplicableReleaseGroupPK);

			if (releaseGroup != null)
			{
				return releaseGroup.Staff.Cast<GlbStaff>().Where(s => !channels.Any(c => c.MSC_ParentID == s.PK) && s.GS_IsActive);
			}

			return Enumerable.Empty<GlbStaff>();
		}

		static bool ShouldParentBeShown(this BMBoardSectionChannel channel, bool overrideChannels, BusinessObjectFactory factory)
		{
			if (channel.MSC_ChannelType != ChannelTypeList.Codes.Resource)
			{
				return true;
			}

			if (overrideChannels)
			{
				return channel.IsResourceActiveOrHasIncompleteTasksAssigned(factory);
			}
			else
			{
				return channel.IsResourceInSectionReleaseGroup(factory);
			}
		}

		static bool IsResourceActiveOrHasIncompleteTasksAssigned(this BMBoardSectionChannel channel, BusinessObjectFactory factory)
		{
			var staff = channel.GetChannelBusinessObject(factory) as GlbStaff;

			if (staff == null)
			{
				return false;
			}

			if (staff.GS_IsActive)
			{
				return true;
			}

			return StaffHasOpenTasks(staff);
		}

		static bool StaffHasOpenTasks(GlbStaff staff)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code)
			{
				MaximumRows = 1
			};

			var assignedStatusQuery = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Assigned);
			assignedStatusQuery.AddToFilter(new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working), JoinCondition.Or);
			assignedStatusQuery.AddToFilter(new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Suspended), JoinCondition.Or);

			query.AddToFilter(assignedStatusQuery);

			var openTasksWithStaffAssigned = staff.Factory.Load<ProcessTask>(query);

			return openTasksWithStaffAssigned.Any();
		}

		static bool IsResourceInSectionReleaseGroup(this BMBoardSectionChannel channel, BusinessObjectFactory factory)
		{
			var biz = channel.GetChannelBusinessObject(factory);
			var staff = biz as GlbStaff;
			var group = factory.Load<GlbGroup>(channel.Section.SectionConfiguration.ApplicableReleaseGroupPK);
			return staff != null && group != null && group.Staff.Contains(staff.PK);
		}

		#endregion
	}
}
