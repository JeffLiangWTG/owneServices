using System;
using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderBase))]
	public abstract class AsycudaManifestHeaderBaseTest : EU.Manifest.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestIsLloydsNumberMandatory()
		{
			var header = GetManifestHeader();
			header.AMA_TransportMode = "";
			AssertEquals(false, header.IsLloydsNumberMandatory);
			header.AMA_TransportMode = "SEA";
			AssertEquals(true, header.IsLloydsNumberMandatory);
			header.AMA_TransportMode = "AIR";
			AssertEquals(false, header.IsLloydsNumberMandatory);
			header.AMA_TransportMode = "IWT";
			AssertEquals(true, header.IsLloydsNumberMandatory);
		}

		public void TestItinerary()
		{
			var header = GetManifestHeader();
			AssertType<RouteEntryCollection>(header.Itinerary);
		}

		public void TestBills()
		{
			var header = GetManifestHeader();
			AssertEquals(ExpectedBillsType, header.Bills.GetType());
		}

		protected virtual Type ExpectedBillsType => typeof(ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeaderBase>);

		public void TestSupportedCusCodeDataTypes()
		{
			ICusCodeDataTypeSupporter header = GetManifestHeader();
			var supportedCusCodeDataTypes = header.GetCusCodeDataTypes();
			AssertCollectionContains(CusCodeDataTypeList.Codes.IcsRouteEntry, supportedCusCodeDataTypes.Keys);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = GetManifestHeader();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		protected abstract AsycudaManifestHeaderBase GetManifestHeader();
	}
}

