using CargoWise.Customs.ES.MessageDefinitions.Version1.ENS.Outgoing;
using CargoWise.Customs.ES.MessageDefinitions.Version1.SummaryDeclarations.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public abstract class SummaryDeclarationsCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
	where TProvider : ISummaryDeclarationsCommonMessageDataProvider
{
	protected SummaryDeclarationsCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected const string SummaryDeclarationsReceiver = "NICA.ES";

	protected void PopulateSummaryDeclarationMessageData(ISummaryDeclarationsCommon declaration)
	{
		declaration.Id = TransactionId;

		declaration.MessageSenderId = provider.SenderId;
		declaration.Receiver = SummaryDeclarationsReceiver;
		declaration.DeclarationDate = CET.ToShortCustomsFormatDateString();
		declaration.DeclarationTime = CET.ToCustomsFormatTimeString();
		declaration.MessageId = TransactionIdLength14;
	}

	protected void PopulateENSGenericMessageData(IENSGenericMessage declaration, IENSGenericMessageDataProvider commonProvider)
	{
		PopulateSummaryDeclarationMessageData(declaration);

		declaration.SenderId = provider.SenderId;
		declaration.SenderName = commonProvider.SenderName;

		declaration.Priority = commonProvider.Priority;
	}

	protected T GetPopulatedItineraryCountry<T>(ZString country)
		where T : ISummaryDeclarationsItineraryCountryField, new()
	{
		return new T
		{
			ItineraryCountry = country
		};
	}

	protected T GetPopulatedAddressInformationENS<T>(IENSAddressInformation provider) where T : IENSOrgInfo, new()
	{
		var declaration = GetPopulatedAddressInformationCommon<T>(provider);
		if (declaration != null)
		{
			declaration.Language = provider.Language;
		}
		return declaration;
	}
}
