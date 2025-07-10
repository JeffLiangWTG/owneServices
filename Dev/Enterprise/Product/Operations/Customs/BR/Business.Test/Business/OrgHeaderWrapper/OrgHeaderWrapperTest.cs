using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	public class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAdditionalIdentification()
		{
			var additionalIdentification1 = orgHeaderWrapper.AdditionalIdentification.AddNew();
			var additionalIdentification2 = orgHeaderWrapper.AdditionalIdentification.AddNew();

			additionalIdentification1.CY_Data = "XXXXX";
			additionalIdentification1.CY_Code = "123";

			additionalIdentification2.CY_Data = "YYYYY";
			additionalIdentification2.CY_Code = "456";

			AssertEquals("AdditionalIdentification", 2, orgHeaderWrapper.AdditionalIdentification.Count);
			AssertEquals("AdditionalIdentification 1 - CY_Data", "XXXXX", orgHeaderWrapper.AdditionalIdentification[0].CY_Data);
			AssertEquals("AdditionalIdentification 1 - CY_Code", "123", orgHeaderWrapper.AdditionalIdentification[0].CY_Code);

			AssertEquals("AdditionalIdentification 2 - CY_Data", "YYYYY", orgHeaderWrapper.AdditionalIdentification[1].CY_Data);
			AssertEquals("AdditionalIdentification 2 - CY_Code", "456", orgHeaderWrapper.AdditionalIdentification[1].CY_Code);
		}

		public void TestAddInfo()
		{
			AssertSame(BROrgImpAddInfo.Get(orgHeader), orgHeaderWrapper.AddInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			return OrgHeaderWrapper.New(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderWrapper = OrgHeaderWrapper.New(orgHeader);
		}

		OrgHeader orgHeader;
		OrgHeaderWrapper orgHeaderWrapper;
	}
}
