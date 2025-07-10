using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupCodeCollection))]
	public class JournalEntriesClassificationGroupCodeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JournalEntriesClassificationGroupCodeCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override JournalEntriesClassificationGroupCodeCollection GetCollectionToTest() => new JournalEntriesClassificationGroupCodeCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new JournalEntriesClassificationGroupCode();

		#endregion
	}
}
