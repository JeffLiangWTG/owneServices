using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(MessageVersionRegistry))]
	sealed class MessageVersionRegistryTest : RegistryBusinessObjectTemplateTestCase<MessageVersionRegistry>
	{
		public void TestValidAtlasVersions()
		{
			AssertCorrectListReturned(MessageVersionRegistry.AtlasSystemCode, new ZString[] { "10.1" });
		}

		public void TestValidAESVersion()
		{
			AssertCorrectListReturned(MessageVersionRegistry.AESSystemCode, new ZString[] { "3.0" });
		}

		public void TestValidEmcsVersion()
		{
			AssertCorrectListReturned(MessageVersionRegistry.EmcsSystemCode, new ZString[] { "2.4", "2.5" });
		}

		public void TestSystemCodeReadOnly()
		{
			Assert(new MessageVersionRegistry().SystemCodeInfo.ReadOnly);
		}

		public void TestAtlasVersionNumber()
		{
			AssertEquals(MessageVersionRegistry.AtlasDefaultVersionNumber, MessageVersionRegistry.CurrentAtlasVersion);
		}

		public void TestAESVersionNumber()
		{
			AssertEquals(MessageVersionRegistry.AESDefaultVersionNumber, MessageVersionRegistry.CurrentAESVersion);
		}

		public void TestEMCSVersionNumber()
		{
			AssertEquals(MessageVersionRegistry.EmcsDefaultVersionNumber, MessageVersionRegistry.CurrentEMCSVersion);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override MessageVersionRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override MessageVersionRegistry GetBusinessObjectToSerialise()
		{
			BizObj.SystemCode = MessageVersionRegistry.AtlasSystemCode;
			BizObj.VersionNumber = MessageVersionRegistry.AtlasDefaultVersionNumber;
			return BizObj;
		}

		void AssertCorrectListReturned(ZString systemCode, ZString[] expectedVersionList)
		{
			var messageVersionRegistry = new MessageVersionRegistry { SystemCode = systemCode };
			AssertContainsExactElementsInAnyOrder(expectedVersionList, messageVersionRegistry.VersionNumbers.GetAllCodesZString());
		}
	}

	[TestedType(typeof(MessageVersionRegistryItem))]
	class MessageVersionRegistryItemTest : StronglyTypedRegistryItemTestCase<MessageVersionRegistryCollection>
	{
		protected override StronglyTypedRegistryItem<MessageVersionRegistryCollection, MessageVersionRegistryCollection> GetNewRegistryItem() => new MessageVersionRegistryItem("", null, null, null, RegistryStorageFlags.All, new MessageVersionRegistryCollection().DefaultCollection);
	}
}
