using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

public class SumARegisterFilterBusinessObjectLookups(SumARegisterFilterBusinessObject filterBizObj)
{
	protected BusinessObjectFactory Factory => filterBizObj.Factory;

	public virtual CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public virtual  CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<CodeDescriptionPairList>();
}
