using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GmailOAuth2JsonFileRegistryItem))]
	sealed class GmailOAuth2JsonFileRegistryItemTest : StronglyTypedRegistryItemTestCase<GmailOAuth2JsonFile>
	{
		protected override StronglyTypedRegistryItem<GmailOAuth2JsonFile, GmailOAuth2JsonFile> GetNewRegistryItem()
		{
			return new GmailOAuth2JsonFileRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
		}

		protected override GmailOAuth2JsonFile ValidValue
		{
			get => new GmailOAuth2JsonFile { JsonText = "test json text 1", FileName = "test1.json" };
		}
	}
}
