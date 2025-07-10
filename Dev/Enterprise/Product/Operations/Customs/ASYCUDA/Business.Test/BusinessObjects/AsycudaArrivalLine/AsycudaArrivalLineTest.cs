using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ASYCUDA.Business.Testing.AsycudaManifestHeaderBaseOnlyTest;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalLine))]
	sealed class AsycudaArrivalLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBillManifestQtyUpdatesArrivalLineExpectedQty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 300;

			var arrHeader = header.ArrivalHeaders.AddNew();
			arrHeader.ATH_VoyageFlightNo = "FLT1";
			arrHeader.ATH_ETAAtDischargePort = ZDateTime.Today.AddDays(-2);

			var arrLine = arrHeader.ArrivalDetails.AddNew();
			AssertEquals(0, arrLine.ATL_ExpectedQty);
			arrLine.ATL_ABL_AsycudaBill = bill.PK;
			arrLine.OnLoaded();
			AssertEquals(300, arrLine.ATL_ExpectedQty);

			bill.ABL_ManifestQty = 400;
			arrLine.OnLoaded();
			AssertEquals(400, arrLine.ATL_ExpectedQty);
		}

		public void TestBillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "001-0492387";
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			bill.ABL_ManifestQty = 300;

			var arrHeader = header.ArrivalHeaders.AddNew();
			arrHeader.ATH_VoyageFlightNo = "FLT1";
			arrHeader.ATH_ETAAtDischargePort = ZDateTime.Today.AddDays(-2);

			var arrLine = arrHeader.ArrivalDetails.AddNew();
			AssertEquals(0, arrLine.ATL_ExpectedQty);
			arrLine.ATL_ABL_AsycudaBill = bill.PK;
			arrLine.OnLoaded();
			AssertEquals("MasterBill should be shown", "001-0492387", arrLine.ATL_BillNumber);

			var hbill = header.Bills.AddNew();
			hbill.ABL_BillNumber = "HB-40298";
			hbill.ABL_BolType = "STD";
			var hbArrLine = arrHeader.ArrivalDetails.AddNew();
			hbArrLine.ATL_ABL_AsycudaBill = hbill.PK;
			hbArrLine.OnLoaded();
			AssertEquals("Housebill should be shown", "HB-40298", hbArrLine.ATL_BillNumber);
		}

		public void TestClusterKey()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var arrivalHeader = header1.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL2";
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_CargoStatus = "STA";
			Factory.Save();
			AssertNotEquals("Cluster key is not 0", 0, header1.AMA_ClusterKey);
			AssertEquals(arrivalHeader.ATH_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(arrivalLine.ATL_ClusterKey, header1.AMA_ClusterKey);
		}

		public void TestPacks()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var arrivalHeader = header1.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL2";
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_CargoStatus = "STA";
			Factory.Save();
			AssertEquals("should be generic type", true, arrivalLine.Lookups.Packs.GetType().IsGenericType);
			var genericParams = arrivalLine.Lookups.Packs.GetType().GetGenericArguments();
			AssertEquals("should be 2 generic arguments", 2, genericParams.Length);
			AssertEquals("first argument should be AsycudaPack", true, typeof(AsycudaPack).IsAssignableFrom(genericParams[0]));
			AssertEquals("first argument should be AsycudaBill", true, typeof(AsycudaBill).IsAssignableFrom(genericParams[1]));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.AMA_JobReference = "X";
			header.AMA_E_ARV = ZDateTime.Today;
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "GBFXT";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";

			var outtturnHeader = factory.NewWithValidTestData<AsycudaArrivalHeader>();
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			outtturnHeader.ATH_ETAAtDischargePort = DateTime.Today;
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			var line = outtturnHeader.ArrivalDetails.AddNew();
			line.ATL_ABL_AsycudaBill = bill.PK;

			return line;
		}
	}
}
