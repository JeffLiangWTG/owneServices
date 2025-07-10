using System;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS
{
	[Serializable]
	public class JASDataErrorNotificationType : ErrorType
	{
		internal JASDataErrorNotificationType(string name, string message)
			: base(name, message)
		{
		}

		public static readonly JASDataErrorNotificationType CannotCreateNewShipmentJob = new JASDataErrorNotificationType("CannotCreateNewShipmentJob",
			"Shipment Job cannot be created at this time");
		public static readonly JASDataErrorNotificationType ShipmentJobAlreadyExistWithCharges = new JASDataErrorNotificationType("ShipmentJobAlreadyExistWithCharges",
			"Shipment Job and Charges already exist");
			}
}
