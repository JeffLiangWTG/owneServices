using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class VisualBoardChannelFetchHintProvider
	{
		public static void FetchChannelEntities(BusinessObjectFactory factory, IEnumerable<IVisualBoardChannel> channels)
		{
			var groups = channels.GroupBy(c => c.EntityType);

			foreach (var group in groups)
			{
				switch (group.Key)
				{
					case ChannelTypeList.Codes.Capability:
						FetchChannelEntity<GlbCapability>(factory, group);
						break;
					case ChannelTypeList.Codes.Group:
						FetchChannelEntity<GlbGroup>(factory, group);
						break;

					case ChannelTypeList.Codes.Resource:
						FetchResources(factory, group);
						break;
					case ChannelTypeList.Codes.Tag:
						FetchChannelEntity<TagMagnitude>(factory, group);
						break;

					case ChannelTypeList.Codes.NotChanneled:
					case ChannelTypeList.Codes.ReleaseSchedulerChannels:
						// Do nothing. No fetching is applicable.
						break;

					default:
						throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "All channel groups must have fetch hints. Unrecognised group [{0}].", group.Key));
				}
			}
		}

		static void FetchResources(BusinessObjectFactory factory, IEnumerable<IVisualBoardChannel> channels)
		{
			var resourcesPks = channels.OfType<VisualBoardChannel>()
				.Where(c => c.EntityPK.IsValid)
				.Select(c => c.EntityPK);

			factory.AddFetchHint(GlbResourceCapabilityPivotSchema.Instance, new ZQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, resourcesPks));

			var resources = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, resourcesPks));
			var allCapabilityPKs = resources.SelectMany(c => c.CapabilityPivots.Select(cc => cc.G5_G4_Capability)).Distinct();

			factory.AddFetchHint(GlbCapabilitySchema.Instance, new ZQuery(GlbCapabilitySchema.PK, allCapabilityPKs));
		}

		static void FetchChannelEntity<T>(BusinessObjectFactory factory, IEnumerable<IVisualBoardChannel> channels)
			where T : BusinessObject
		{
			foreach (var channel in channels.Where(c => c.EntityPK.IsValid))
			{
				factory.AddFetchHint(typeof(T), channel.EntityPK);
			}
		}
	}
}
