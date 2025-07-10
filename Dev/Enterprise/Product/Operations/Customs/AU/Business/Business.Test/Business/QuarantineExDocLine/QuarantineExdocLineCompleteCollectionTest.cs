using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExdocLineCompleteCollection))]
	sealed class QuarantineExdocLineCompleteCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleExportDeclaration();
			var declaration = helper.Declaration;
			return new QuarantineExdocLineCompleteCollection(declaration);
		}
	}
}
