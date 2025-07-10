using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(AsycudaBill))]
	public class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);

			bill.ABL_BolType = "BOL";
			AssertType<AsycudaBillValidationForMasterChild>(bill.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}
	}
}
