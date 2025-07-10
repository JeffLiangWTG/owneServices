using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(MessageVersionRegistryCollection))]
	class MessageVersionRegistryNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageVersionRegistryCollection>
	{
		public void TestPreventNewAndRemove()
		{
			var collection = new MessageVersionRegistryCollection();
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestDefaultValues()
		{
			var collection = new MessageVersionRegistryCollection();
			var defaults = collection.DefaultCollection;

			AssertEquals("10.1", defaults.Cast<MessageVersionRegistry>().First(x => x.SystemCode == MessageVersionRegistry.AtlasSystemCode).VersionNumber);
			AssertEquals("3.0", defaults.Cast<MessageVersionRegistry>().First(x => x.SystemCode == MessageVersionRegistry.AESSystemCode).VersionNumber);
			AssertEquals("2.4", defaults.Cast<MessageVersionRegistry>().First(x => x.SystemCode == MessageVersionRegistry.EmcsSystemCode).VersionNumber);
		}

		protected override MessageVersionRegistryCollection GetCollectionToTest() => new MessageVersionRegistryCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageVersionRegistry();
	}

	[TestedType(typeof(MessageVersionRegistryCollection))]
	class MessageVerionRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageVersionRegistryCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => false;
		protected override MessageVersionRegistryCollection GetCollectionToTest() => new MessageVersionRegistryCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageVersionRegistry();
	}
}
