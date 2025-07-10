using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class H7CommonMessageBuilder<TProvider, T> : XMLMessageBuilder<TProvider, T>
		where TProvider : IESEDIMessageCollectionProvider
	{
		public H7CommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		const string MessageRecipientTest = "ES.AEAT.PRUEBAS";
		const string MessageRecipientReal = "ES.AEAT";
		protected const string IsValueTrue = "S";
		protected const string IsValueFalse = "N";

		protected MessageEntTd GetPopulatedMessage()
		{
			return new MessageEntTd()
			{
				MessageIdentification = TransactionId,
				MessageRecipient = provider.IsTest ? MessageRecipientTest : MessageRecipientReal,
				PreparationDate = DateOfCET,
				PreparationTime = TimeOfCET
			};
		}

		protected ContactPersonTd GetPopulatedContactPerson(IPartyContactProvider contactInfo)
		{
			return new ContactPersonTd()
			{
				Name = contactInfo?.Name,
				EMailAddress = contactInfo?.Email,
				PhoneNumber = contactInfo?.PhoneNumber
			};
		}

		protected AddressTd GetPopulatedAddressInfo(IPartyProvider addressInfo)
		{
			return new AddressTd()
			{
				City = addressInfo.City,
				Country = addressInfo.Country,
				Postcode = addressInfo.PostCode,
				StreetAndNumber = addressInfo.Address
			};
		}
	}
}
