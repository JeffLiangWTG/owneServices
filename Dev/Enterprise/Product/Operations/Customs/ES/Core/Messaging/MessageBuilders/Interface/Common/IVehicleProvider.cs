using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IVehiclePackagesInfoCommon
	{
		#region Fields For PCI

		IReadOnlyCollection<IVehicleCommon> Packages { get; }

		#endregion
	}

	public interface IVehicleCommon
	{
		ZString Chassis { get; }
		ZString Brand { get; }
		ZString Model { get; }
	}
}
