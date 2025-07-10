using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageConfiguration : EU.Business.CusTempStorage.TemporaryStorageConfiguration
{
	protected override ITemporaryStorageHeaderValidationDecider GetValidationDeciderCore() => new TemporaryStorageHeaderValidationDecider();

	protected override bool SupportLRNGenerationCore => false;

	protected override bool SupportAgentDefaultingCore => false;

	protected override EU.Business.CusTempStorage.TemporaryStorageBillConfiguration GetNewBillConfiguration() => new TemporaryStorageBillConfiguration();

	protected override EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration GetNewPreviousDocumentConfiguration() => new TemporaryStoragePreviousDocumentConfiguration();

	protected override string TemporaryStorageHeaderDocumentWrapperClassCore => $"{typeof(TemporaryStorageHeaderWrapper).FullName}, {typeof(TemporaryStorageHeaderWrapper).Assembly.GetName().Name}";

	protected override EU.Business.CusTempStorage.TemporaryStoragePackedItemConfiguration GetNewPackedItemConfiguration() => new TemporaryStoragePackedItemConfiguration();
}
