using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AsycudaBill))]
sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
{
	public void TestHeader()
	{
		var bill = GetNewBusinessObject() as AsycudaBill;
		AssertType<AsycudaManifestHeader>(bill.Header);
	}

	public void TestGetLRN()
	{
		var bill = GetNewBusinessObject() as AsycudaBill;

		AssertNullOrEmpty("Pre-condition: LRN should be empty", bill.LocalReferenceNumber);

		var lrn = bill.GetAndSetLRNIfNeeded();

		AssertNotNullOrEmpty("LRN is generated", lrn);
		AssertEquals("The LRN is saved to bill", lrn, bill.LocalReferenceNumber);
		AssertSame("Should return the same LRN on all subsequent calls", lrn, bill.GetAndSetLRNIfNeeded());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObjectForDeleteTest(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		return header.Bills.AddNew();
	}
}
