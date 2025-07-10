using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSFinalSupplementaryDeclarationHelperLateChildCollection))]
	public class CDSFinalSupplementaryDeclarationHelperLateChildCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CDSFinalSupplementaryDeclarationHelperLateChildCollection>
	{
		protected override CDSFinalSupplementaryDeclarationHelperLateChildCollection GetCollectionToTest() => new CDSFinalSupplementaryDeclarationHelperLateChildCollection(new CDSFinalSupplementaryDeclarationHelper(Factory.New<JobDeclaration>()));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
	}
}
