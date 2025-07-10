using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(DeclarationLevelPackingGroupCollection))]
	public class DeclarationLevelPackingGroupCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeclarationLevelPackingGroupCollection((JobDeclaration)Declaration);
		}
	}
}
