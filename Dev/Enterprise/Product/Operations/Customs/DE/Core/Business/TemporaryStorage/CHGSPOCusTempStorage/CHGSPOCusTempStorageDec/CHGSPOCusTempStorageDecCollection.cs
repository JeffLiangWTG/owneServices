using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGSPOCusTempStorageDecCollection : CusTempStorageDecCollection<CHGSPOCusTempStorageDec, CusTempStorageJobHeader>
	{
		public CHGSPOCusTempStorageDecCollection(CusTempStorageJobHeader jobHeader)
			: base(jobHeader)
		{
		}

		protected override ZQuery CreateRelationshipFilter() => base.CreateRelationshipFilter().AddToFilter(new ZQuery(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm));
	}
}
