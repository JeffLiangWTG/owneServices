using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBImporterMatchApproval))]
	sealed class CusHAWBImporterMatchApprovalTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMatchType()
		{
			CusHAWBImporterMatchApproval matchApproval = (CusHAWBImporterMatchApproval)GetNewBusinessObject();
			AssertEquals("Match type should be correct", OrgMatchApprovalType.AirCargoImporter.Code, matchApproval.MatchType.Code);
		}

		public void TestOrganisationType()
		{
			CusHAWBImporterMatchApproval matchApproval = (CusHAWBImporterMatchApproval)GetNewBusinessObject();
			AssertEquals("Organisation type correct", "Importer", matchApproval.OrganisationType);
		}

		public void TestMappingOverrides()
		{
			CusHAWBImporterMatchApproval createdMatchApproval = (CusHAWBImporterMatchApproval)GetNewBusinessObject();
			var matchApproval = Factory.Load<TestCusHAWBImporterMatchApproval>(createdMatchApproval.PK);
			var importer = Factory.Load<OrgPatternMatchAddress>(createdMatchApproval.P2_ParentID);

			importer.P3_Code = "AccountID";
			importer.P3_CompanyName = "CompanyName";
			importer.P3_Address1 = "Street";
			importer.P3_Address2 = "Street2";
			importer.P3_City = "City";
			matchApproval.Parent.CS_RN_NKConsigneeCountry = "NZ";
			importer.P3_State = "State";
			importer.P3_PostCode = "Postcode";
			importer.P3_Phone = "Phone";
			importer.P3_Fax = "Fax";

			AssertEquals("OwnerCode", "AccountID", matchApproval.ParentOwnerCode);
			AssertEquals("CompanyName", "CompanyName", matchApproval.ParentCompanyName);
			AssertEquals("Street", "Street", matchApproval.ParentStreet);
			AssertEquals("Street2", "Street2", matchApproval.ParentStreet2);
			AssertEquals("City", "City", matchApproval.ParentCity);
			AssertEquals("UNLOCO", "NZ", matchApproval.ParentUNLOCO);
			AssertEquals("State", "State", matchApproval.ParentState);
			AssertEquals("Postcode", "Postcode", matchApproval.ParentPostCode);
			AssertEquals("Phone", "Phone", matchApproval.ParentPhone);
			AssertEquals("Fax", "Fax", matchApproval.ParentFax);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var airCargo = Factory.New<CusHAWB>();
			var matchApproval = (CusHAWBImporterMatchApproval)loader.LoadOrCreate(airCargo.PK, OrgMatchApprovalType.AirCargoImporter);
			return matchApproval;
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new OrgMatchApproval.Loader(Factory);
		}

		OrgMatchApproval.Loader loader;

		sealed class TestCusHAWBImporterMatchApproval : CusHAWBImporterMatchApproval
		{
			public TestCusHAWBImporterMatchApproval(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString ParentOwnerCode => base.ParentOwnerCode;

			public new ZString ParentCompanyName => base.ParentCompanyName;

			public new ZString ParentStreet => base.ParentStreet;

			public new ZString ParentStreet2 => base.ParentStreet2;

			public new ZString ParentCity => base.ParentCity;

			public new ZString ParentUNLOCO => base.ParentUNLOCO;

			public new ZString ParentState => base.ParentState;

			public new ZString ParentPostCode => base.ParentPostCode;

			public new ZString ParentPhone => base.ParentPhone;

			public new ZString ParentFax => base.ParentFax;
		}
	}
}
