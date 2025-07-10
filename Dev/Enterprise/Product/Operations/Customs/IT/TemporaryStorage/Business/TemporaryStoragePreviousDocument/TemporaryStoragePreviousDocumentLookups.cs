using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

class TemporaryStoragePreviousDocumentLookups : EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentLookups
{
	public TemporaryStoragePreviousDocumentLookups(TemporaryStoragePreviousDocument parent) : base(parent)
	{
	}

	protected new TemporaryStoragePreviousDocument Parent => (TemporaryStoragePreviousDocument)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList => UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory);
}
