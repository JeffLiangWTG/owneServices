using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(ExportStatusRequestRecipientRegistryCollection))]
	class ExportStatusRequestRecipientNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportStatusRequestRecipientRegistryCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestDefaultValues()
		{
			var defaults = collection.DefaultCollection.Cast<ExportStatusRequestRecipientRegistry>().ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Atlas default", "DE001348", defaults.Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AtlasSystemCode).MessageRecipient);
				AssertEquals("Aes default", "DE001342", defaults.Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AESSystemCode).MessageRecipient);
			});
		}

		protected override ExportStatusRequestRecipientRegistryCollection GetCollectionToTest() => new ExportStatusRequestRecipientRegistryCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExportStatusRequestRecipientRegistry();

		protected override void SetUp()
		{
			base.SetUp();
			collection = new ExportStatusRequestRecipientRegistryCollection();
		}
		ExportStatusRequestRecipientRegistryCollection collection;
	}

	[TestedType(typeof(ExportStatusRequestRecipientRegistryCollection))]
	class ExportStatusRequestRecipientCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ExportStatusRequestRecipientRegistryCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => false;
		protected override ExportStatusRequestRecipientRegistryCollection GetCollectionToTest() => new ExportStatusRequestRecipientRegistryCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExportStatusRequestRecipientRegistry();
	}
}
