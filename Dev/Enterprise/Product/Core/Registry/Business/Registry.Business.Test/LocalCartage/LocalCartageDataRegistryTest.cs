using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LocalCartageDataRegistry))]
	sealed class LocalCartageDataRegistryTest : RegistryItemSetTestCaseWithFactory<LocalCartageDataRegistry>
	{
		#region Telematics

		#region TestGeoFenceRadiusSize

		public void TestGeoFenceRadiusSize()
		{
			TestGenericRegistryItem(ItemSet.GeoFenceRadiusSize, "GeoFenceRadiusSize", "Telematics", "GEO Fence Radius", "GEO Fence Radius size in meters.", RegistryStorageFlags.System, RegistryOptions.NotCached, 500);
		}

		#endregion

		#region TestGetLastProcessedEventTime

		public void TestGetLastProcessedEventTime()
		{
			TestGenericRegistryItem(ItemSet.GetLastProcessedEventTime, "GetLastProcessedEventTime", "Telematics", "Last Processed Time", "Time (in UTC) of last event processed time.", RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsHidden);
		}

		public void TestGetLastProcessedEventTime_Value()
		{
			var now = ZDateTime.Now;
			var timeWithMilisecond = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
			ItemSet.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeWithMilisecond);
			AssertEquals(timeWithMilisecond, ItemSet.GetLastProcessedEventTime.Value);
		}

		#endregion

		public void TestUseGlbDeviceLocationSubscriber()
		{
			TestGenericRegistryItem(ItemSet.UseGlbDeviceLocationSubscriber, "UseGlbDeviceLocationSubscriber", "Telematics", "Use GlbDeviceLocation Subscriber", "Process events based on when new GlbDeviceLocation records have been created", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		#endregion

		#region ConditionallyVisibleRegistryItems

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("EnablePortTransportGPSActivityGenerator");
				return list;
			}
		}

		#endregion
	}
}
