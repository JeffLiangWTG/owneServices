using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public abstract class CusTempStorageDecCollection : DependentBusinessObjectCollection<CusTempStorageDec, CusTempStorageJobHeader>
{
	protected CusTempStorageDecCollection(CusTempStorageJobHeader parentStorageHeader)
		: base(parentStorageHeader, parentStorageHeader.Factory)
	{
	}

	protected override SchemaGuidColumn FKSchemaColumnInDependent => CusTempStorageDecSchema.STH_SJH;
}
