using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPESimilarOrgMatchForApproval))]
	public class UPESimilarOrgMatchForApprovalTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestDelegatedConstruction()
		{
			OrgMatchApproval orgMatchApproval = (OrgMatchApproval)Factory.New(typeof(UPECusHAWBConsigneeMatchApproval));
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			AssertEquals(typeof(UPESimilarOrgMatchForApproval), SimilarOrgMatchForApproval.New(orgMatchApproval, orgPatternMatch).GetType());
		}

		public void TestOwnerCodeType()
		{
			OrgMatchApproval orgMatchApproval = (OrgMatchApproval)Factory.New(typeof(UPECusHAWBConsigneeMatchApproval));
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			UPESimilarOrgMatchForApproval.RegisterThisSubTypeOverride();
			UPESimilarOrgMatchForApproval orgMatchForApproval = (UPESimilarOrgMatchForApproval)SimilarOrgMatchForApproval.New(orgMatchApproval, orgPatternMatch);
			AssertEquals(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, orgMatchForApproval.GetOwnerCodeTypeForTest);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgMatchApproval orgMatchApproval = (OrgMatchApproval)Factory.New(typeof(UPECusHAWBConsigneeMatchApproval));
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			orgPatternMatch.OS_OH = Factory.New(typeof(OrgHeader)).PK;
			UPESimilarOrgMatchForApproval.RegisterThisSubTypeOverride();
			return (UPESimilarOrgMatchForApproval)SimilarOrgMatchForApproval.New(orgMatchApproval, orgPatternMatch);
		}
	}
}
