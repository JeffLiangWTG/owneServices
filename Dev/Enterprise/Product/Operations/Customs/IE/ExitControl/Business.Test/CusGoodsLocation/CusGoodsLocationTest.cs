using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.ExitControl.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	[TestedType(typeof(CusGoodsLocation))]
	class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCGL_Type_Caption()
		{
			(var arrivalGoodsLocation, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(arrivalGoodsLocation.CGL_TypeInfo, (string[])null, "Type of Location");
		}

		public void TestCGL_AdditionalIdentifier_Caption()
		{
			(var arrivalGoodsLocation, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, (string[])null, "UNLOCO");
		}

		public void TestIsPresentation()
		{
			(var arrivalGoodsLocation, var report, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.Presentation.ToUpper();
			AssertEquals(true, arrivalGoodsLocation.IsPresentation);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals(false, arrivalGoodsLocation.IsPresentation);
			report.CER_Type = ExitReportTypeList.Codes.Presentation.ToLower();
			AssertEquals(true, arrivalGoodsLocation.IsPresentation);
		}

		public void TestLookups()
		{
			(var arrivalGoodsLocation, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusGoodsLocationLookups>(arrivalGoodsLocation.Lookups);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			(var arrivalGoodsLocation, var report, _) = GetNewBusinessObject(Factory);
			arrivalGoodsLocation.FillWithValidTestData();
			arrivalGoodsLocation.Parent = report;
			return arrivalGoodsLocation;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).arrivalGoodsLocation;

		public static (CusGoodsLocation arrivalGoodsLocation, CusExitReport report, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var report, _, var header) = CusExitReportTest.GetNewBusinessObject(factory);
			return (report.ArrivalGoodsLocation, report, header);
		}
	}
}
