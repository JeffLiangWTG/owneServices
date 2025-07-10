using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new InvoiceLineCompleteCollection((JobDeclaration)Declaration);
	}
}
