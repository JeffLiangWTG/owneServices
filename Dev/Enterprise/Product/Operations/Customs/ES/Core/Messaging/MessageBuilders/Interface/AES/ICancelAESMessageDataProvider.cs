using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICancelAESMessageDataProvider : IAESCommonDataProvider
{
	ICancelAESExportOperation ExportOperation { get; }
	ZString CustomsOfficeOfExport { get; }
	IPartyIdProvider Exporter { get; }
	IPartyIdProvider Declarant { get; }
	ICommonRepresentative Representative { get; }
}

public interface ICancelAESExportOperation : IAESCommonExportOperationMRN
{
	ZString InvalidationReason { get; }
}
