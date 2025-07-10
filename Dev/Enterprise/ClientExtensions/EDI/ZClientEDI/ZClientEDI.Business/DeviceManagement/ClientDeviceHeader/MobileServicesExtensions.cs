using System;
using CargoWise.MobileServices.Common.Messages;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public static class MobileServicesExtensions
	{
		public static DeviceKind GetDeviceKindForMobileServices(this ClientDeviceHeader device)
		{
			if (device is null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			return GetDeviceKindForMobileServices(device.CDH_DeviceKind);
		}

		internal static DeviceKind GetDeviceKindForMobileServices(string deviceKind)
		{
			switch (deviceKind)
			{
				case ClientDeviceHeaderLookups.Kinds.Android:
					return DeviceKind.Android;

				case ClientDeviceHeaderLookups.Kinds.AppleMobility:
					return DeviceKind.AppleMobile;

				case ClientDeviceHeaderLookups.Kinds.WindowsMobilityLegacy:
					return DeviceKind.WindowsMobileLegacy;

				case ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded:
					return DeviceKind.WiseTechVehicularPlatform;

				case ClientDeviceHeaderLookups.Kinds.Unknown:
					return DeviceKind.InvalidKind;

				default:
					throw new ArgumentException(FormattableString.Invariant($"Invalid device kind: '{deviceKind}'"));
			}
		}
	}
}
