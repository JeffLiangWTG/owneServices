using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

public class ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups
{
	public ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups(ImportFromTemporaryStorageRegisterFilterStripBusinessObject filterBizObj)
	{
		this.factory = Argument.NotNull(filterBizObj?.Factory, nameof(ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Factory));
	}

	readonly BusinessObjectFactory factory;

	public CodeDescriptionPairList OwnerReferenceTypeList => factory.GetCachedValue<OwnerReferenceTypeList>();
}
