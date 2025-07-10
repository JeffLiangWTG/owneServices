using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsIEOfficeCode))]
	sealed class NctsIEOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsIEOfficeCode>
	{
		public void TestCY_Date_Readonly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;
			var officeCode = (NctsIEOfficeCode)movementHeader.CustomsOfficesForDeparture.AddNew();

			officeCode.CY_Code = "DEP";
			AssertEquals("CY_Date readonly when DEP.", true, officeCode.CY_DateInfo.ReadOnly);

			officeCode.CY_Code = "DES";
			AssertEquals("CY_Date readonly when DES.", true, officeCode.CY_DateInfo.ReadOnly);

			officeCode.CY_Code = "TXT";
			AssertEquals("CY_Date readonly when TXT.", true, officeCode.CY_DateInfo.ReadOnly);

			officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			AssertEquals("CY_Date writable when TRA.", false, officeCode.CY_DateInfo.ReadOnly);
		}

		protected override IEnumerable<NctsIEOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Data = "D";
			yield return (NctsIEOfficeCode)customsOffice;

			var arrivalHeader = factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
			var customsOfficeArrival = arrivalHeader.ArrivalMovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Data = "A";
			yield return (NctsIEOfficeCode)customsOfficeArrival;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			return movementHeader.CustomsOffices.AddNew();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterNctsIEOfficeCode(bizObjToTest);
	}

	class LightValidationTesterNctsIEOfficeCode : LightValidationTester
	{
		public LightValidationTesterNctsIEOfficeCode(BusinessObject bo) : base(bo) { }

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			var objType = info.BizObj.GetType();
			return objType != typeof(JobDocAddress) && objType != typeof(NctsIEOfficeCode) && base.ShouldTestProperty(info);
		}
	}
}
