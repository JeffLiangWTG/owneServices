using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageDecCollection : CusTempStorageDecCollection<CHGTSTCusTempStorageDec, CusTempStorageJobHeader>
	{
		public CHGTSTCusTempStorageDecCollection(CusTempStorageJobHeader jobHeader) : base(jobHeader)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation);
			return result;
		}
	}
}
