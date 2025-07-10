using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage;
public class CusTempStorageRegPremisesCollection : ActiveBusinessObjectCollection<CusTempStorageRegPremises>, Integration.Customs.TemporaryStorage.ICusTempStorageRegPremisesCollection
{
	public CusTempStorageRegPremisesCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public CusTempStorageRegPremisesCollection(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
	{
	}

	public CusTempStorageRegPremisesCollection(BusinessObjectFactory factory, ZString typeFilter) : base(factory)
	{
		DefaultModuleFilterFields(typeFilter);
	}

	public void DefaultModuleFilterFields(string typeFilter)
	{
		if (Relationship != null)
		{
			base.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Type", "Property", (ZString)(NoResString)typeFilter,false));
		}
	}
}

