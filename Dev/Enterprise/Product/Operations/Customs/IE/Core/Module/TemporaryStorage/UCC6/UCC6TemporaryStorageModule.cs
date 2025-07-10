using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module
{
	public class UCC6TemporaryStorageModule : EU.TemporaryStorage.Module.UCC6TemporaryStorageModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();
	}
}
