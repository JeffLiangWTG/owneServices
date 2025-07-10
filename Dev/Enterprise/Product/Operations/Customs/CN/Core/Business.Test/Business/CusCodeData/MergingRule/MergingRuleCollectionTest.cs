using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(MergingRuleCollection))]
	class MergingRuleCollectionTest : CusCodeDataCollectionTest<MergingRule>
	{
		protected override CusCodeDataCollection<MergingRule> GetCusCodeDataCollection()
		{
			return new MergingRuleCollection(Factory.New<JobDeclaration>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<MergingRule>();
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = Declaration.TablePrefix;
			result.CY_Type = Constants.CusCodeDataTypes.Codes.MergingRule;
			return result;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
