using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(MessageVersionRegistryCollection))]
class MessageVersionRegistryNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageVersionRegistryCollection>
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
		var collection = new MessageVersionRegistryCollection();
		var defaults = collection.DefaultCollection;

		CombineAssertions(() =>
		{
			AssertEquals("Target System Name for NCTS", "NCTS.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.NCTSP5DomainCode).TargetSystemName);
			AssertEquals("Target System Name for AES", "AES.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.AESDomainCode).TargetSystemName);
			AssertEquals("Target System Name for IDMS", "IDMS.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.IDMSDomainCode).TargetSystemName);
			AssertEquals("Target System Name for PN/TS", "PN-TS.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.PNTSDomainCode).TargetSystemName);
			AssertEquals("Target System Name for TSD", "TSD.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.TSDDomainCode).TargetSystemName);
			AssertEquals("Target System Name for REN", "REN.BE", defaults.Cast<MessageVersionRegistry>().First(x => x.DomainCode == MessageVersionRegistry.RENDomainCode).TargetSystemName);
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
