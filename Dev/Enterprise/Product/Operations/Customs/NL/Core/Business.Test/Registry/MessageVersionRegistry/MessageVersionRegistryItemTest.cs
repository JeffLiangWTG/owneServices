using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(MessageVersionRegistryItem))]
sealed class MessageVersionRegistryItemTest : StronglyTypedRegistryItemTestCase<MessageVersionRegistryCollection>
{
	public void TestGetTargetSystemName()
	{
		var item = (MessageVersionRegistryItem)Item;
		CombineAssertions(() =>
		{
			AssertEquals("Empty", ZString.Empty, item.GetTargetSystemName(string.Empty));
			AssertEquals("Invalid", ZString.Empty, item.GetTargetSystemName("XX"));
			AssertEquals("Valid NCTS", MessageVersionRegistry.NCTSP5DefaultTarget, item.GetTargetSystemName(MessageVersionRegistry.NCTSP5DomainCode));
			AssertEquals("Valid DMS", MessageVersionRegistry.DMSDefaultTarget, item.GetTargetSystemName(MessageVersionRegistry.DMSDomainCode));
		});
	}

	protected override StronglyTypedRegistryItem<MessageVersionRegistryCollection, MessageVersionRegistryCollection> GetNewRegistryItem() => new MessageVersionRegistryItem("", null, null, null, RegistryStorageFlags.All, MessageVersionRegistryCollection.DefaultCollection);
}
