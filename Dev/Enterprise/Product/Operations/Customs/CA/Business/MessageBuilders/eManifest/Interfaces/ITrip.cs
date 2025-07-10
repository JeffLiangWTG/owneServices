namespace Enterprise.Customs.CA.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MessageBuilders.eManifest;

	interface ITrip : Customs.Business.MessageBuilders.eManifest.ITrip
	{
		/// <summary>
		/// Service Option Id (976 = Inward Report). (M/3)
		/// </summary>
		ZString ServiceOptionId { get; }

		/// <summary>
		/// Used to indicate CBSA sub-location code. (C/4)
		/// Condition: Transmit if available.
		/// </summary>
		ZString SubLocationCode { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// The CBSA identifier of the party that is to receive an electronic notification from the CBSA regarding the shipment. (M/15)
		/// </summary>
		ZString SecondNotifyPartyId { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// </summary>
		IEnumerable<ICrew> CrewMembers { get; }

		IConveyance Conveyance { get; }
		IEnumerable<IEquipment> Equipment { get; }
	}
}
