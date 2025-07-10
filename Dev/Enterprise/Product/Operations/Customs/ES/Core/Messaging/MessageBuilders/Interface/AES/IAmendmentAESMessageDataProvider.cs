using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IAmendmentAESMessageDataProvider : IDeclarationAESCommonMessageDataProvider
	{
		IAmendmentAESExportOperation ExportOperation { get; }
	}

	public interface IAmendmentAESExportOperation : IDeclarationAESExportOperation
	{
		ZString MRN { get; }
	}
}
