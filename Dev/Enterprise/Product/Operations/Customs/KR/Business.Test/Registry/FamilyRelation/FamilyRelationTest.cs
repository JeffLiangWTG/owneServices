using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FamilyRelation))]
	public sealed class FamilyRelationTest : RegistryBusinessObjectTemplateTestCase<FamilyRelation>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override FamilyRelation GetBusinessObjectToClone() => new FamilyRelation();

		protected override FamilyRelation GetBusinessObjectToSerialise() => new FamilyRelation();
	}
}
