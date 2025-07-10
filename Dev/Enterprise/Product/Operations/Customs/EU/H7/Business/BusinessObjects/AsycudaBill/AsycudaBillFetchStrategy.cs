using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business
{
	class AsycudaBillFetchStrategy : ASYCUDA.Business.AsycudaBillFetchStrategy
	{
		public AsycudaBillFetchStrategy(ASYCUDA.Business.AsycudaBill bill)
			: base(bill)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaBill.Schema.LocalReferenceNumber:
						Factory.AddFetchHint(typeof(CusEntryNumber), GetCusEntryNumberQuery(CusEntryNumberTypes.EU.LocalReferenceNumber));
						break;
					case AsycudaBill.Schema.MovementReferenceNumber:
						Factory.AddFetchHint(typeof(CusEntryNumber), GetCusEntryNumberQuery(CusEntryNumberTypes.Standard.MovementReferenceNumber));
						break;
				}
			}
		}

		ZQuery GetCusEntryNumberQuery(string entryType)
		{
			var country = CountryCodes.GetCustomsCountryOfJurisdiction(BusinessObject.Header.AMA_RN_NKCountry);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, BusinessObject.PK);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, country);
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;

			return query;
		}
	}
}
