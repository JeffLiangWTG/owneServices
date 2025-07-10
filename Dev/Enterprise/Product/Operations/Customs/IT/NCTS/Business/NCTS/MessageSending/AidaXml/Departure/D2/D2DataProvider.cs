using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

[GenerateDataProvider(
	typeof(DeclarationD2MessageBuilderMetadata),
	typeof(NctsHeaderMessageSendingObject))]
public static partial class DeclarationD2DataProvider
{
}

public partial interface IDeclarationD2AdditionalDataProvider
{
}
