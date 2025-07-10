using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclarationLookups.CAAttachInvoiceCollection))]
	sealed class CAAttachInvoiceCollectionTest : ActiveBusinessObjectCollectionTestCase<JobDeclarationLookups.CAAttachInvoiceCollection>
	{
		protected override JobDeclarationLookups.CAAttachInvoiceCollection GetCollectionToTest() => new JobDeclarationLookupsTest.CAAttachInvoiceCollectionForTesting(Factory.New<JobDeclaration>());
	}
}
