using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderToPrintCollection))]
	sealed class JobComInvoiceHeaderToPrintCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobComInvoiceHeaderToPrintCollection>
	{
		#region Implementation

		protected override JobComInvoiceHeaderToPrintCollection GetCollectionToTest()
		{
			return new JobComInvoiceHeaderToPrintCollection(Factory.New<JobDeclaration>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobComInvoiceHeaderToPrint(Factory.New<JobComInvoiceHeader>());
		}

		#endregion
	}
}
