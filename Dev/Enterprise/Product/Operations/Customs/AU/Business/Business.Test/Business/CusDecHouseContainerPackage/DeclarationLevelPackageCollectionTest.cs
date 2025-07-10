using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeclarationLevelPackageCollection))]
	public class DeclarationLevelPackageCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackageCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeclarationLevelPackageCollection((JobDeclaration)Declaration);
		}
	}
}
