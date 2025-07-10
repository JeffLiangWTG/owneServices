using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageDecCollection : CusTempStorageDecCollection<CHGOFFCusTempStorageDec, CusTempStorageJobHeader>
	{
		public CHGOFFCusTempStorageDecCollection(CusTempStorageJobHeader parentStorageHeader) : base(parentStorageHeader)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader);
			return result;
		}
	}
}
