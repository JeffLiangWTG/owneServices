using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonCargoDescFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public NctsCommonCargoDescFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var goodsItem = (NctsCommonCargoDesc)BusinessObject;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case NctsCommonCargoDesc.Schema.BY_Supplements:
						Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, goodsItem.PK);
						break;
					case nameof(NctsCommonCargoDesc.DutyAmount):
					case nameof(NctsCommonCargoDesc.AntiDumpingDutyAmount):
					case nameof(NctsCommonCargoDesc.CountervailingDutyAmount):
					case nameof(NctsCommonCargoDesc.LiabilityAmount):
						Factory.AddFetchHint(CusInBondFeeSchema.BFE_BY, goodsItem.PK);
						break;
				}
			}
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusInvPackSchema.B5_ParentID, BusinessObject.PK);
		}
	}
}
