using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescFetchStrategy : NctsCommonCargoDescFetchStrategy
	{
		public NctsDepartureCargoDescFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var goodsItem = (NctsDepartureCargoDesc)BusinessObject;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(NctsDepartureCargoDesc.VatAmount):
					case nameof(NctsDepartureCargoDesc.ExciseAmount):
						Factory.AddFetchHint(CusInBondFeeSchema.BFE_BY, goodsItem.PK);
						break;
				}
			}
		}
	}
}
