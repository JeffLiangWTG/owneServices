using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(MessageVersionRegistryCollection))]
sealed class MessageVersionRegistryNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageVersionRegistryCollection>
{
	public void TestPreventNewAndRemove()
	{
		var collection = new MessageVersionRegistryCollection();
		CombineAssertions(() =>
		{
			AssertEquals("AllowNew", false, collection.AllowNew);
			AssertEquals("AllowRemove", false, collection.AllowRemove);
		});
	}

	public void TestDefaultValues()
	{
		var defaults = MessageVersionRegistryCollection.DefaultCollection;

		CombineAssertions(() =>
		{
			AssertEquals("Target System Name for NCTS", "NCTS.NL", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.NCTSP5DomainCode).TargetSystemName);
			AssertEquals("Target System Name for DMS", "DMS.NL", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.DMSDomainCode).TargetSystemName);
		});
	}

	protected override MessageVersionRegistryCollection GetCollectionToTest() => new MessageVersionRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageVersionRegistry();
}

[TestedType(typeof(MessageVersionRegistryCollection))]
class MessageVersionRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageVersionRegistryCollection>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => false;

	protected override MessageVersionRegistryCollection GetCollectionToTest() => new MessageVersionRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageVersionRegistry();
}
