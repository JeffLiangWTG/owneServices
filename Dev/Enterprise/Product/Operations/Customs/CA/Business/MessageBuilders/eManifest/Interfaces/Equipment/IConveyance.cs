namespace Enterprise.Customs.CA.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	interface IConveyance : IEquipment, Customs.Business.MessageBuilders.eManifest.IConveyance
	{
		/// <summary>
		/// Loaded/Empty is provided only at the tractor level.
		/// If loaded, either CCN or valid cargo exception code must be provided at least one of tractor, trailer or container level.
		/// CA (M/1).
		/// </summary>
		ZBool EmptyIndicator { get; }
	}
}
