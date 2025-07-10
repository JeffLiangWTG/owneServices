using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleCollection : BusinessObjectCollection<ComplianceRule>, IComplianceRuleCollection
	{
		public ComplianceRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZString CountryCode { get; private set; }

		public void LoadComplianceRules(ZString countryCode)
		{
			CountryCode = countryCode;

			var query = new ZQuery();
			if (CountryCode.IsEmpty)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.DefaultJoinCondition = JoinCondition.Or;
				query.AddToFilter(ComplianceRuleSchema.CRU_Origin, CountryCode);
				query.AddToFilter(ComplianceRuleSchema.CRU_Destination, CountryCode);
			}

			RemoveAllButLeaveRelationshipsIntact();
			AddRange(Factory.Load<ComplianceRule>(query));
			LastLoadedAdditionalFilter = query;

			foreach (var rule in this.Cast<ComplianceRule>())
			{
				rule.CurrentCountryCode = CountryCode;
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			var result = base.CreateBusinessObjectFromRow(row);
			((ComplianceRule)result).CurrentCountryCode = CountryCode;
			return result;
		}
	}
}
