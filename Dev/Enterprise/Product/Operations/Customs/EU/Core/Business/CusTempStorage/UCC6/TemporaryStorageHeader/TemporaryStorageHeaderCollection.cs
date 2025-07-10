using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[ModuleID(nameof(ModuleId.UCC6TemporaryStorage))]
	public class TemporaryStorageHeaderCollection : ActiveBusinessObjectCollection<TemporaryStorageHeader>
	{
		public TemporaryStorageHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
