using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(NeoUpgradeLicenceCollection))]
	internal sealed class NeoUpgradeLicenceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<NeoUpgradeLicenceCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NeoUpgradeLicenceCollection GetCollectionToTest() => new NeoUpgradeLicenceCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NeoUpgradeLicence(NewFallbackLevel(), Factory);

		#endregion
	}
}
