using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalFormData
	{
		ZDateTime FormDate { get; }
		ZString FormAuthority { get; }
		ZString FormAuthorityLanguage { get; }
		ZString FormLocation { get; }
		ZString FormLocationLanguage { get; }
		ZString FormCountry { get; }

		#region Fields For FTX
		ZString FormText { get; }
		ZString FormTextLanguage { get; }
		#endregion
	}
}
