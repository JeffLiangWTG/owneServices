using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class LocalCartageDataRegistry : RegistryItemSet
	{
		#region Instance

		public static LocalCartageDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new LocalCartageDataRegistry()); }
		}

		[ThreadStatic]
		static LocalCartageDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Telematics { get { return ResString.GetMultilingualString("B64711FA-8807-4EF7-96A9-27B1B2EADC77", "Telematics"); } }
		}

		#endregion

		#region Telematics

		#region GeoFenceRadiusSize

		public IntRegistryItem GeoFenceRadiusSize
		{
			get
			{
				return GetItem<IntRegistryItem>("GeoFenceRadiusSize", delegate
				{
					return new IntRegistryItem(
						"GeoFenceRadiusSize",
						Categories.Telematics,
						ResString.GetMultilingualString("f61248bd-2da5-4395-a9e6-c86bd776b2d7", "GEO Fence Radius"),
						ResString.GetMultilingualString("5ADCCDAB-B117-422A-9D35-62B3A6DF4C6B", "GEO Fence Radius size in meters."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						500);
				});
			}
		}

		#endregion

		#region GetLastProcessedEventTime

		public DateTimeRegistryItem GetLastProcessedEventTime
		{
			get
			{
				return GetItem("GetLastProcessedEventTime", delegate
				{
					return new DateTimeRegistryItem(
						"GetLastProcessedEventTime",
						Categories.Telematics,
						(NoResString)"Last Processed Time",
						(NoResString)"Time (in UTC) of last event processed time.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		public BooleanRegistryItem UseGlbDeviceLocationSubscriber
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseGlbDeviceLocationSubscriber", delegate
				{
					return new BooleanRegistryItem(
						"UseGlbDeviceLocationSubscriber",
						Categories.Telematics,
						(NoResString)"Use GlbDeviceLocation Subscriber",
						(NoResString)"Process events based on when new GlbDeviceLocation records have been created",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

	}
}
