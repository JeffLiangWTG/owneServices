using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityDocManagerInfo))]
	sealed class CrmOpportunityDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CrmOpportunity>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<CrmOpportunity>();
		}
	}
}
