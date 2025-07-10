using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb;

[GenerateDataProvider(
	typeof(ExportSbGoodsRegistrationCACHE05MessageBuilderMetadata),
	typeof(DeclarationMessageSendingObject),
	AdditionalNamespaces = [
		"Enterprise.Customs.IN.Registry",
		"Enterprise.ZArchitecture.Environment",
		"CargoWise.Types"
	])]
public static partial class ExportSbGoodsRegistrationCACHE05DataProvider
{
}
