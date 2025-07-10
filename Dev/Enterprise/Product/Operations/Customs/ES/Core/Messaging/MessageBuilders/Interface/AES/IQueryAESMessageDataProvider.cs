namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IQueryAESMessageDataProvider : IAESCommonDataProvider
	{
		IAESCommonExportOperationMRN ExportOperation { get; }
	}
}
