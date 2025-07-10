using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CustomsOfficer))]
	sealed class CustomsOfficerTest : CusCodeDataTest<CustomsOfficer>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.CustomsOfficer, customsOfficer.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<CustomsOfficerValidation>(customsOfficer.Validation);
		}

		public void TestCY_Data()
		{
			customsOfficer.CY_Data = "COF123김승옥";
			AssertEquals("COF123김승옥", customsOfficer.CY_Data);
			AssertNoErrors(customsOfficer.CY_DataInfo);
			AssertEquals(42, customsOfficer.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return customsOfficer;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CustomsOfficer>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			customsOfficer = entryHeader.CustomsOfficers.AddNew();
			Factory.Save();
		}
		CustomsOfficer customsOfficer;
	}
}
