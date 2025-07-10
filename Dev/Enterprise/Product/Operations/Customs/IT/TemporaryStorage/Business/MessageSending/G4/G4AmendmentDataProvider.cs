using CargoWise.Customs.IT.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

[GenerateDataProvider(
	typeof(TemporaneaCustodiaG4AmendmentMessageBuilderMetadata),
	typeof(TemporaryStorageHeader),
	AdditionalNamespaces =
	[
		"Enterprise.Customs.IT.Business",
		"Enterprise.MasterFiles.Business"
	])]
public static partial class TemporaneaCustodiaG4AmendmentDataProvider
{
}
