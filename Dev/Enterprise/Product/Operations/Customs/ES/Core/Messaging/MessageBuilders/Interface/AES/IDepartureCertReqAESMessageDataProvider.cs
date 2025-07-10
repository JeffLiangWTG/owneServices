namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDepartureCertReqAESMessageDataProvider : IAESCommonDataProvider
	{
		IAESCommonExportOperationMRN ExportOperation { get; }
	}
}
