using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalHeader))]
	class AsycudaArrivalHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.ArrivalHeaders.AddNew();
		}

		public void TestArrivalHeaderCanBeDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			var bill = header.Bills.AddNew();

			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var line = arrivalHeader.ArrivalDetails.AddNew();
			line.ATL_ABL_AsycudaBill = bill.PK;

			AssertEquals("If the Bill has not status, it must be true", true, bill.CanDelete);
			AssertEquals("If the Bill can be deleted, the Arrival Header can be deleted", true, arrivalHeader.CanDelete);

			bill.ABL_BillStatus = "ACP";
			AssertEquals("If the Bill has status ACP, it must be false", false, bill.CanDelete);
			AssertEquals("If the Bill can not be deleted, also the Arrival Header can not be deleted", false, arrivalHeader.CanDelete);
		}
	}
}
