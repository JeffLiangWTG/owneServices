using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(AdditionalProcedureCodeCollection))]
sealed class AdditionalProcedureCodeCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestElementType()
	{
		var additionalProcedureCodeCollection = Factory.New<JobDeclaration>()
			.InvoiceLines.AddNew()
			.AdditionalProcedureCodes;

		AssertType<AdditionalProcedureCode>(additionalProcedureCodeCollection.AddNew());
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Factory.New<JobDeclaration>()
			.Invoices.AddNew()
			.InvoiceLines.AddNew()
			.AdditionalProcedureCodes;
	}
}
