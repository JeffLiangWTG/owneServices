using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public enum DisplayNameType
	{
		None = 0,
		FullName = 1,
		FriendlyName = 2,
		ChannelCode = 3,
	}

	public interface IVisualBoardChannel : IChannel
	{
		string EntityType { get; }
		string Status { get; }
		string ToolTipStatus { get; }
		ZGuid RiskComponentPK { get; }
		bool IsOvertime { get; }
		bool IsHighRisk { get; }
		ImageWithTooltip StatusImage { get; }
		Color BackgroundColor { get; }
		Color ForegroundColor { get; }
		ChannelNames Names { get; }
		Image DisplayImage { get; }

		bool IsInChannel(IProcessTask task, bool showJobWorkflow = false);

		ZString ChannelEntityCode { get; }

		void ClearChannelCache();
		void ClearCacheAndReload();

		event EventHandler Reloaded;
	}

	public static class IVisualBoardChannel_Extensions
	{
		public static string GetChannelName(this IVisualBoardChannel channel, DisplayNameType requestedNameType)
		{
			return channel.Names?.GetSpecificName(requestedNameType) ?? string.Empty;
		}

		public static bool TryGetCapabilityPKs(this IVisualBoardChannel channel, BusinessObjectFactory factory, out ZGuid[] capabilityPKs)
		{
			var staff = factory.Load<GlbStaff>(channel.EntityPK);

			if (staff != null)
			{
				capabilityPKs = staff.CapabilityPivots.Select(c => c.G5_G4_Capability).ToArray();
				return true;
			}
			else
			{
				capabilityPKs = null;
				return false;
			}
		}

#if DEBUG
		public static void SeedCacheWithChannelMatcherTest(this IVisualBoardChannel channel, BusinessObjectFactory factory)
		{
			(channel as VisualBoardChannel)?.SeedCacheWithChannelMatcher(factory);
		}

		public static string GetStatusInCacheForTest(this IVisualBoardChannel channel)
		{
			return (channel as ResourceChannel)?.GetStatusInCacheForTest();
		}

		public static void SeedCacheWithChannelDescriptorForTest(this IVisualBoardChannel channel, BusinessObjectFactory factory)
		{
			(channel as VisualBoardChannel)?.SeedCacheWithChannelDescriptor(factory);
		}
#endif
	}
}
