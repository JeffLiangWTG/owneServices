using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.MobileServices.Common.Messages;
using Enterprise.Client.EDI.DeviceManagement.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.DeviceManagement.ClientDeviceHeader
{
	class MobileServicesExtensionsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAllLookupsMapped()
		{
			foreach (var kind in GetKinds())
			{
				MobileServicesExtensions.GetDeviceKindForMobileServices(kind);
			}
		}

		[ExpectNoExceptions]
		public void TestAllValuesHaveCorrespondingLookup()
		{
			foreach (DeviceKind kind in Enum.GetValues(typeof(DeviceKind)))
			{
				AssertHasCorrespondingLookup(kind);
			}
		}

		static void AssertHasCorrespondingLookup(DeviceKind kind)
		{
			foreach (var lookup in GetKinds())
			{
				if (MobileServicesExtensions.GetDeviceKindForMobileServices(lookup) == kind)
				{
					return;
				}
			}

			Assert("Could not find matching lookup.", false);
		}

		static IEnumerable<string> GetKinds()
			=> typeof(ClientDeviceHeaderLookups.Kinds).GetFields().Select(f => f.GetValue(null)).Cast<string>();
	}
}
