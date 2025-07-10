using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb;

[GenerateDataProvider(
	typeof(ExportSbCACHE01MessageBuilderMetadata),
	typeof(CusEntryHeader),
	AdditionalNamespaces = [
		"Enterprise.Customs.IN.Registry",
		"Enterprise.ZArchitecture.Environment",
		"CargoWise.Types"
	])]
public static partial class ExportSbCACHE01DataProvider
{
}

public partial interface IExportSbCACHE01AdditionalDataProvider
{
}
