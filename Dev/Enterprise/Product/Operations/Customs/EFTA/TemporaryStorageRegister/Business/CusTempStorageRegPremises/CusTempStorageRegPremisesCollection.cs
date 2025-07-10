using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[ModuleID(ModuleId.TempStoragePremises)]
public class CusTempStorageRegPremisesCollection : ActiveBusinessObjectCollection<CusTempStorageRegPremises>, Integration.Customs.TemporaryStorage.ICusTempStorageRegPremisesCollection
{
	public CusTempStorageRegPremisesCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public CusTempStorageRegPremisesCollection(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
	{
	}
}
