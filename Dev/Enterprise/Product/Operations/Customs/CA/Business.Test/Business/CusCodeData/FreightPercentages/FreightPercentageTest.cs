using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(FreightPercentage))]
	sealed class FreightPercentageTest : Customs.Business.Testing.CusCodeDataTest<FreightPercentage>
	{
		public void TestValidation()
		{
			var number = Factory.New<FreightPercentage>();
			AssertEquals("Validation", typeof(FreightPercentageValidation), number.Validation.GetType());
		}

		public void TestLookups()
		{
			FreightPercentage regNo = Factory.New<FreightPercentage>();
			AssertEquals("Validation", typeof(FreightPercentageLookups), regNo.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<FreightPercentage>();
			AssertEquals(CusCodeDataTypeList.Codes.FreightPercentage, number.CY_Type);
		}

		public void TestParents()
		{
			var freightPercentage = Factory.New<FreightPercentage>();
			freightPercentage.CY_ParentID = OrgHeader.PK;
			freightPercentage.CY_ParentTableCode = OrgHeader.TablePrefix;
			AssertEquals(OrgHeader, freightPercentage.Parent);
		}

		public void TestIFreightPercentageIsCorrectlySetup()
		{
			AssertEquals(typeof(FreightPercentage), ObjectFactory.GetType<Integration.Customs.CA.IFreightPercentage>());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return OrgImpAddInfo.Get(factory.NewWithValidTestData<OrgHeader>()).FreightPercentages.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgImpAddInfo.FreightPercentages.AddNew();
		}

		OrgHeader OrgHeader
		{
			get { return orgHeader ?? (orgHeader = Factory.New<OrgHeader>()); }
		}
		OrgHeader orgHeader;

		OrgImpAddInfo OrgImpAddInfo
		{
			get { return orgImpAddInfo ?? (orgImpAddInfo = OrgImpAddInfo.Get(OrgHeader)); }
		}
		OrgImpAddInfo orgImpAddInfo;
	}
}
