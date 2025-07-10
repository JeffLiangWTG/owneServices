using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			var bizObj = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackValidation>(bizObj.Validation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObjectWithParent(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectWithParent(Factory);
		}

		AsycudaPack GetNewBusinessObjectWithParent(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
