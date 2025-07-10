using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public abstract class CusTempStorageLineCollection : DependentBusinessObjectCollection<CusTempStorageLine, CusTempStorageDec>
{
	protected CusTempStorageLineCollection(CusTempStorageDec parentStorageDec)
		: base(parentStorageDec)
	{
	}

	protected override SchemaGuidColumn FKSchemaColumnInDependent => CusTempStorageLineSchema.TSL_STH;
}
