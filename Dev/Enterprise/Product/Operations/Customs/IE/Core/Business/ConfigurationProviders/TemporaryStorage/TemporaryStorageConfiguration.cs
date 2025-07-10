namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageConfiguration : EU.Business.CusTempStorage.TemporaryStorageConfiguration
	{
		protected override EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration GetNewPreviousDocumentConfiguration() => new TemporaryStoragePreviousDocumentConfiguration();

		protected override EU.Business.CusTempStorage.TemporaryStorageSupportingDocumentConfiguration GetNewSupportingDocumentConfiguration() => new TemporaryStorageSupportingDocumentConfiguration();

		protected override EU.Business.CusTempStorage.TemporaryStoragePackedItemConfiguration GetNewPackedItemConfiguration() => new TemporaryStoragePackedItemConfiguration();
	}
}
