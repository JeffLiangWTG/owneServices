using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FamilyRelationCollection))]
	public sealed class FamilyRelationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FamilyRelationCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override FamilyRelationCollection GetCollectionToTest() => new FamilyRelationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new FamilyRelation();
	}
}
