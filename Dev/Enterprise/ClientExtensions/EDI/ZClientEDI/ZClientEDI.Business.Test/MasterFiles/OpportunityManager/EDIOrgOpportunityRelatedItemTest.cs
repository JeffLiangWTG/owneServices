using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunity))]
	public class EDIOrgOpportunityRelatedItemTest : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => "TYP";

		protected override string ExpectedSelectionCriterion2 => "AAA";

		protected override string ExpectedSelectionCriterion3 => "PCK";

		protected override string ExpectedSelectionCriterion4 => "SRC";

		protected override string ExpectedSelectionCriterion5 => "10%";

		protected override Type ExpectedPivotCollectionType => typeof(OrgOpportunityGenPivotCollection);

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AAA";

			var ediOrgOpportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ediOrgOpportunity.P8_OpportunityType = "TYP";
			ediOrgOpportunity.P8_PackageType = "PCK";
			ediOrgOpportunity.P8_Source = "SRC";
			ediOrgOpportunity.P8_CloseCertainty = 10;
			ediOrgOpportunity.P8_GC = company.PK;

			return ediOrgOpportunity;
		}
	}
}
