using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegPremisesProvider : CollectionProvider, Integration.Customs.EU.ICusTempStorageRegPremisesProvider
{
	public CusTempStorageRegPremisesProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
	{
	}

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.TempStoragePremises;

	protected override IBusinessObjectCollection CreateCollection()
	{
		return new CusTempStorageRegPremisesCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(CusTempStorageRegPremises)));
	}

	protected override IBusinessObjectCollection GetCollectionForFindbox()
	{
		return new CusTempStorageRegPremisesCollection(BusinessObjectFactory);
	}
}
