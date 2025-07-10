
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AllEntryLineCPDecQuestion : BusinessObjectCollection<CMRCusEntryCPDec>
	{
		public AllEntryLineCPDecQuestion(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			EntryHeader = entryHeader;
		}

		public readonly CusEntryHeader EntryHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			var query = new ZQuery(CusEntryLineSchema.CL_CH, EntryHeader.PK);
			query.AddToFilter(CusEntryLineSchema.CL_CustomsPostedStatus, Customs.Business.EntryLineStatusList.Codes.Active);

			CusEntryLine[] entryLines = Factory.Load<CusEntryLine>(query);
			if (entryLines.Length > 0)
			{
				ZGuid[] guids = new ZGuid[entryLines.Length];
				for (int i = 0; i < entryLines.Length; i++)
				{
					guids[i] = entryLines[i].PK;
				}
				result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_CL, SQLComparisonOperator.Equal, guids);
				result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_JE, SQLComparisonOperator.Equal, null);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}
	}
}
