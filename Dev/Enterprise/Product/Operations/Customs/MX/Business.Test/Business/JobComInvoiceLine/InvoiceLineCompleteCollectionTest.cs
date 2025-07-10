using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration as JobDeclaration);

		protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
