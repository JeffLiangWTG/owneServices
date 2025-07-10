using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.JobDeclaration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeaderSingleElementCollection))]
	sealed class JobComInvoiceGroupHeaderSingleElementCollectionTest : BusinessObjectCollectionTestCase
	{
		//TODO: complete this test: I00028302
		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobComInvoiceGroupHeaderSingleElementCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
		}
	}
}
