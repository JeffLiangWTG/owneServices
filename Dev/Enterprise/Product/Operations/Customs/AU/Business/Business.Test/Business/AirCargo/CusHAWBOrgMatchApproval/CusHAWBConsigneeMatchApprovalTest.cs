using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBConsigneeMatchApproval))]
	sealed class CusHAWBConsigneeMatchApprovalTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganisationType()
		{
			AssertEquals("Consignee", matchApproval.OrganisationType);
		}

		public void TestSetOrganisationDetails()
		{
			var orgPatternMatchAddress = Factory.New<OrgPatternMatchAddress>();

			var newOrganisation = Factory.New<OrgHeader>();
			matchApproval.P2_ParentID = orgPatternMatchAddress.PK;
			matchApproval.AddressToBeMatched.P3_Code = "OwnerCode";
			matchApproval.AddressToBeMatched.P3_CompanyName = "FullName";
			matchApproval.AddressToBeMatched.P3_Address1 = "Street";
			matchApproval.AddressToBeMatched.P3_Address2 = "Street2";
			matchApproval.AddressToBeMatched.P3_City = "City";
			matchApproval.AddressToBeMatched.P3_State = "State";
			matchApproval.AddressToBeMatched.P3_PostCode = "PostCode";
			matchApproval.AddressToBeMatched.P3_Phone = "Phone";

			matchApproval.CopyDetailsToOrganisation(newOrganisation);

			var currentCountry = Factory.Load<RefCountry>(Env.CurrentCompany.Country.PK);

			AssertEquals("FullName set correctly", "FullName", newOrganisation.OH_FullName);
			AssertEquals("Street set correctly", "Street", newOrganisation.MainAddress.OA_Address1);
			AssertEquals("Street2 set correctly", "Street2", newOrganisation.MainAddress.OA_Address2);
			AssertEquals("City set correctly", "City", newOrganisation.MainAddress.OA_City);
			AssertEquals("State set correctly", "State", newOrganisation.MainAddress.OA_State);
			AssertEquals("PostCode set correctly", "PostCode", newOrganisation.MainAddress.OA_PostCode);
			AssertEquals("Phone set correctly", "Phone", newOrganisation.MainAddress.OA_Phone);
			AssertEquals("Organisation type set correctly", true, newOrganisation.OH_IsConsignee);
		}

		public void TestMatchApproval()
		{
			var organisation = Factory.New<OrgHeader>();
			matchApproval.ApproveMatchBySupervisor(organisation);
			AssertEquals("Match approved, the consignee fk should be populated on the air cargo record", organisation.MainAddress.PK, airCargo.CS_OA_ConsigneeAddress);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var airCargo = Factory.New<CusHAWB>();
			var matchApproval = (CusHAWBConsigneeMatchApproval)loader.LoadOrCreate(airCargo.PK, OrgMatchApprovalType.AirCargoConsignee);
			return matchApproval;
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new OrgMatchApproval.Loader(Factory);
			matchApproval = (CusHAWBConsigneeMatchApproval)GetNewBusinessObject();
			airCargo = matchApproval.Parent;
		}

		OrgMatchApproval.Loader loader;
		CusHAWB airCargo;
		CusHAWBConsigneeMatchApproval matchApproval;
	}
}
