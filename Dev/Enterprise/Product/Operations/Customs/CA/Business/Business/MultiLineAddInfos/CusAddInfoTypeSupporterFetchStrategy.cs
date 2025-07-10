using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CA.Business
{
	class CusAddInfoTypeSupporterFetchStrategy : Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy
	{
		public CusAddInfoTypeSupporterFetchStrategy(ICusAddInfoTypeSupporter supporter, bool hasCusCALPCO) : base(supporter)
		{
			this.hasCusCALPCO = hasCusCALPCO;
		}

		readonly bool hasCusCALPCO;

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();

			if (hasCusCALPCO)
			{
				Factory.AddFetchHint(CusCALPCOSchema.CLP_ParentID, supporter.PK);
			}
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			if (hasCusCALPCO)
			{
				Factory.AddFetchHint(CusCALPCOSchema.CLP_ParentID, supporter.PK);
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (hasCusCALPCO)
			{
				Factory.AddFetchHint(CusCALPCOSchema.CLP_ParentID, supporter.PK);
			}
		}
	}
}
