using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	public class InvoiceLineViewCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		protected override InvoiceLineViewCollection GetCollectionToTest()
		{
			return new InvoiceLineViewCollection((JobDeclaration)JobDeclaration);
		}
	}
}
