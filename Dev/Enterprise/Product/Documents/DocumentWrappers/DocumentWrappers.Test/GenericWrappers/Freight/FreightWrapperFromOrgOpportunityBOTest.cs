using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromOrgOpportunityBO))]
	sealed class FreightWrapperFromOrgOpportunityBOTest : FreightWrapperTest
	{
		#region Implementation

		OrgOpportunity opportunity;
		FreightWrapperFromOrgOpportunityBO orgGenericFreightWrapper;

		protected override void SetUp()
		{
			opportunity = Factory.New<OrgOpportunity>();
			orgGenericFreightWrapper = new FreightWrapperFromOrgOpportunityBO(opportunity, Factory);
			base.SetUp();
		}

		#endregion

		public void TestGetDocOrgOpportunity()
		{
			opportunity.P8_OpportunityID = "TESTID";
			var opportunityDoc = orgGenericFreightWrapper.OrgOpportunityDocument;

			AssertType(typeof(DocOrgOpportunity), opportunityDoc);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<OrgOpportunity>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromOrgOpportunityBO(opportunity, Factory);
		}
	}
}
