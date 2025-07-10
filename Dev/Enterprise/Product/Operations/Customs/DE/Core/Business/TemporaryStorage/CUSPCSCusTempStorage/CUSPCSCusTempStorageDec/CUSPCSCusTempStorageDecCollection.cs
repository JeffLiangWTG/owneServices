using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSCusTempStorageDecCollection : CusTempStorageDecCollection<CUSPCSCusTempStorageDec, CusTempStorageJobHeader>
	{
		public CUSPCSCusTempStorageDecCollection(CusTempStorageJobHeader jobHeader)
			: base(jobHeader)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(new ZQuery(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo));
		}
	}
}
