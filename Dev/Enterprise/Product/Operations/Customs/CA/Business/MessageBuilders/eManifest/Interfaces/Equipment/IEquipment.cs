namespace Enterprise.Customs.CA.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	interface IEquipment : Customs.Business.MessageBuilders.eManifest.IEquipment
	{
		/// <summary>
		/// CA: (C/25), Specify Cargo Control Number if loaded and not exceptional.
		/// This number cannot be re-used for 3 years.
		/// </summary>
		ZString CargoControlNumber { get; }

		/// <summary>
		/// CA: (C/2), Condition: Specify exception code when Tractor is loaded and no CCN is provided at any level (Tractor, Trailer, Container).
		/// </summary>
		ZString CargoExceptionCode { get; }
	}
}
