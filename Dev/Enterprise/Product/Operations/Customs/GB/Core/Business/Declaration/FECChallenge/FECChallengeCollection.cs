using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class FECChallengeCollection : BusinessObjectCollection<FECChallenge>
	{
		public FECChallengeCollection(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader)
			: base(factory)
		{
			this.cusEntryHeader = cusEntryHeader;
		}
		readonly CusEntryHeader cusEntryHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			var firstQuery = new ZQuery(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.FEC);
			var secondQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, cusEntryHeader.PK);
			secondQuery.AddToFilter(new ZQuery(CusCodeDataSchema.CY_ParentID, cusEntryHeader.AllEntryLines.Select(x => x.PK).ToList()), JoinCondition.Or);

			var result = base.CreateRelationshipFilter().AddToFilter(firstQuery).AddToFilter(secondQuery);
			result.OrderBy = string.Join(", ", CusCodeDataSchema.Constants.CY_Order, CusCodeDataSchema.Constants.CY_Code);
			return result;
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
