using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module
{
	public class DCGStatementLineSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var statementLineSubQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
			statementLineSubQuery.AddToFilter(JoinCondition.And, CusStatementLineSchema.B3_EntryType, StatementEntryTypeList.Codes.DCG);
			statementLineSubQuery.AddToFilter(filter);

			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));
			result.AddSubQuery(statementLineSubQuery, JoinCondition.And);
			return result;
		}
	}
}
