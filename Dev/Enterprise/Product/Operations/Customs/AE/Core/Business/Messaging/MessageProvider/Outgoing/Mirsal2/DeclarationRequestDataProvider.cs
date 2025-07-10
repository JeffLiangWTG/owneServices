using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.AE.Business;

[GenerateDataProvider(
	typeof(DeclarationRequestMessageBuilderMetadata),
	typeof(CusEntryHeader),
	AdditionalNamespaces = [
		"System"
	])]
public static partial class DeclarationRequestDataProvider
{
}

public partial interface IDeclarationRequestAdditionalDataProvider;
