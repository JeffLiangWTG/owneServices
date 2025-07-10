using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocumentCollection))]
sealed class PreviousDocumentCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments;
	}
}
