using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DeclarationLevelPackingGroupCollection))]
class DeclarationLevelPackingGroupCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
{
	protected override BusinessObjectCollection GetCollectionToTest() => new DeclarationLevelPackingGroupCollection((JobDeclaration)Declaration);
}
