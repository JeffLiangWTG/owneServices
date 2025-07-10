using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CW1CusEntryLineFeeCollection : CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CW1CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			var subQuery = new ZQuery(CusEntryLineFeeSchema.CF_Source, CusEntryLineFeeSourceCodeList.Codes.CW1);
			subQuery.AddToFilter(JoinCondition.Or, CusEntryLineFeeSchema.CF_Source, null);
			query.AddToFilter(subQuery);
			return query;
		}

		protected override BusinessObject AddNewCore()
		{
			var cusEntryLineFee = base.AddNewCore() as CusEntryLineFee;
			cusEntryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			return cusEntryLineFee;
		}
	}
}
