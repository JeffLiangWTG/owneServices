using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAsycudaPackedItemCollectionMaster()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var supportingDocumentCollection = new SupportingDocumentCollection(jobDeclaration);
			AssertType<JobDeclaration>(supportingDocumentCollection.Master);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(jobDeclaration);
		}
	}
}
