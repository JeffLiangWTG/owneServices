using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection))]
	public class CusEntryHeaderBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			return new CusEntryHeaderCollection(declaration, Factory);
		}
	}
}
