namespace Enterprise.Customs.ES.Business.CusTempStorage;

sealed class TemporaryStorageConfiguration : EU.Business.CusTempStorage.TemporaryStorageConfiguration
{
	protected override EU.Business.CusTempStorage.TemporaryStorageBillConfiguration GetNewBillConfiguration() => new TemporaryStorageBillConfiguration();

	protected override EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration GetNewPreviousDocumentConfiguration() => new TemporaryStoragePreviousDocumentConfiguration();

	protected override string TemporaryStorageHeaderDocumentWrapperClassCore => ESConstants.DocumentWrapperConstants.TemporaryStorageHeaderDocumentWrapperType;
}
