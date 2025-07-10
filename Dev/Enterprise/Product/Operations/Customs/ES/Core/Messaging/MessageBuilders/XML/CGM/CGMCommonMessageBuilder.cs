using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CGM.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class CGMCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : ICGMCommonDataProvider
{
	protected CGMCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected T GetPopulatedCommonMessage<T>()
		where T : ICGMMessageCommon, new()
	{
		return new T()
		{
			MessageIdentification = TransactionId,
			PreparationDateAndTime = CET.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss),
			SendEmailL = provider.SendEmailL,
		};
	}

	protected T GetPopulatedCGMPartyProviderWithAddressAndContactPerson<T, A, C>(ICGMPartyProviderWithAddressAndContactPerson cgmPartyProvider)
		where T : ICGMOrgAddressInfoWithAddressAndContactPerson, new()
		where A : IOrgAddressCommon, new()
		where C : ICommonContactPerson, new()
	{
		var party = default(T);
		if (cgmPartyProvider != null)
		{
			party = new T()
			{
				Id = cgmPartyProvider.Id,
				Name = cgmPartyProvider.Name,
				Address = GetPopulatedAddressCommon<A>(cgmPartyProvider.Address),
				ContactPerson = GetPopulatedContactPerson<C>(cgmPartyProvider.ContactPerson),
			};
		}
		return party;
	}
}
