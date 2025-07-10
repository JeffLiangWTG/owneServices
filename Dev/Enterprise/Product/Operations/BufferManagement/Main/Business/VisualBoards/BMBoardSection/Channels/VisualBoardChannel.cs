using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	internal abstract class VisualBoardChannel : IVisualBoardChannel
	{
		protected internal VisualBoardChannel(ZGuid channelEntityPK,
			ZString channelEntityCode,
			string entityType,
			Func<PropertyCache> getCache,
			Func<BoardFactoryProvider> getFactoryProvider)
		{
			EntityPK = channelEntityPK;
			ChannelEntityCode = channelEntityCode;
			EntityType = entityType;
			GetCache = getCache;
			GetFactoryProvider = getFactoryProvider;
		}

		public ZGuid EntityPK { get; }

		public ZString ChannelEntityCode { get; }

		public string EntityType { get; }

		readonly Func<PropertyCache> GetCache;
		protected PropertyCache Cache => GetCache?.Invoke() ?? new PropertyCache();

		readonly Func<BoardFactoryProvider> GetFactoryProvider;
		protected BoardFactoryProvider FactoryProvider => GetFactoryProvider?.Invoke();

		#region Colors

		public Color BackgroundColor => GetBackgroundColor();

		public Color ForegroundColor => GetForegroundColor();

		protected virtual Color GetBackgroundColor() => Color.Transparent;

		protected virtual Color GetForegroundColor() => SystemColors.ControlText;

		#endregion

		#region Status

		protected const string ChannelStatusCachePropertyName = "ChannelStatus";

		protected class ChannelStatus
		{
			public bool IsHighRisk { get; set; }
			public string Status { get; set; }
			public string ToolTipStatus { get; set; }
			public ZGuid RiskComponentPK { get; set; }
			public ImageWithTooltip StatusImage { get; set; }
			public bool IsOvertime { get; set; }
		}

		ChannelStatus InnerChannelStatus
		{
			get
			{
				return Cache.GetCachedValue(
						EntityPK,
						ChannelStatusCachePropertyName,
						() => GetChannelStatusCore(FactoryProvider.GetNewBackgroundThreadLoaderFactory("ChannelStatus:" + GetType().Name), Cache)); // Factory name for debugging
			}
		}

		protected abstract ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache);

		#region Properties
		public string Status => InnerChannelStatus.Status;

		public string ToolTipStatus => InnerChannelStatus.ToolTipStatus;

		public ZGuid RiskComponentPK => InnerChannelStatus.RiskComponentPK;

		public bool IsOvertime => InnerChannelStatus.IsOvertime;

		public bool IsHighRisk => InnerChannelStatus.IsHighRisk;

		public ImageWithTooltip StatusImage => InnerChannelStatus.StatusImage;

		#endregion

		#endregion

		#region Descriptor

		protected class ChannelDescriptor
		{
			public ChannelNames ChannelNames { get; set; }
			public Image DisplayImage { get; set; }
		}

		const string ChannelDescriptorCachePropertyName = "ChannelDescriptor";
		ChannelDescriptor InnerChannelDescriptor
		{
			get
			{
				if (channelDescriptor == null)
				{
					channelDescriptor = Cache.GetCachedValue(
						EntityPK,
						ChannelDescriptorCachePropertyName,
						() => GetChannelDescriptorCore(FactoryProvider.GetNewBackgroundThreadLoaderFactory("ChannelDescriptor:" + GetType().Name))); // Factory name for debugging
				}

				return channelDescriptor;
			}
		}
		ChannelDescriptor channelDescriptor;

		public void SeedCacheWithChannelDescriptor(BusinessObjectFactory factory)
		{
			var descriptor = GetChannelDescriptorCore(factory);
			Cache.OverwriteCachedValue(EntityPK, ChannelDescriptorCachePropertyName, descriptor);
			channelDescriptor = descriptor;
		}

		protected abstract ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory);

		public ChannelNames Names => InnerChannelDescriptor.ChannelNames;

		public Image DisplayImage => InnerChannelDescriptor.DisplayImage;

		#endregion

		#region Matcher

		protected class ChannelMatcher
		{
			public ChannelMatcher(Func<IProcessTask, bool> isInChannel)
			{
				IsInChannel = isInChannel;
			}

			public Func<IProcessTask, bool> IsInChannel { get; }

			public static ChannelMatcher Empty
			{
				get { return new ChannelMatcher(_ => false); }
			}
		}

		ChannelMatcher InnerChannelMatcher
		{
			get
			{
				if (channelMatcher == null)
				{
					channelMatcher = GetChannelMatcherCore(FactoryProvider.GetNewBackgroundThreadLoaderFactory("ChannelMatcher:" + GetType().Name)); // Factory name for debugging
				}

				return channelMatcher;
			}
		}
		ChannelMatcher channelMatcher;

		public void SeedCacheWithChannelMatcher(BusinessObjectFactory factory)
		{
			channelMatcher = GetChannelMatcherCore(factory);
		}

		protected abstract ChannelMatcher GetChannelMatcherCore(BusinessObjectFactory factory);

		public bool IsInChannel(IProcessTask task, bool showJobWorkflow = false) => InnerChannelMatcher.IsInChannel(task);

		#endregion

		#region ClearCache

		public void ClearCacheAndReload()
		{
			ClearChannelCache();
			Reloaded?.Invoke(this, EventArgs.Empty);
		}

		public void ClearChannelCache()
		{
			ClearChannelCacheCore();
		}

		protected virtual void ClearChannelCacheCore()
		{
			channelDescriptor = null;
			Cache.Remove(EntityPK, ChannelDescriptorCachePropertyName);
			Cache.Remove(EntityPK, ChannelStatusCachePropertyName);
		}

		public event EventHandler Reloaded;

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			if (obj is VisualBoardChannel channel)
			{
				return channel.EntityPK == EntityPK;
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return EntityPK.GetHashCode();
		}

		#endregion
	}
}
