using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.NCTS.Business;

class LockForEditTestHelper : IDisposable
{
	internal LockForEditTestHelper(BusinessObjectFactory factory,  string declarationType, string tabPage)
	{
		var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		var lockConfig = new DeclarationLockConfig(fallbackLevel, null) { DeclarationType = declarationType };
		lockConfig.TabInfos.RemoveAndDeleteAll();
		lockConfig.TabInfos.AddNew().TabPage = tabPage;
		registryDisposable = CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { lockConfig });
		factory.InvalidateCachedProperties();
	}

	readonly IDisposable registryDisposable;

	public void Dispose()
	{
		registryDisposable.Dispose();
	}
}
