using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageDecCollection : CusTempStorageDecCollection<PRLCONCusTempStorageDec, CusTempStorageJobHeader>
	{
		public PRLCONCusTempStorageDecCollection(CusTempStorageJobHeader jobHeader)
			: base(jobHeader)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation);
			return result;
		}
	}
}
