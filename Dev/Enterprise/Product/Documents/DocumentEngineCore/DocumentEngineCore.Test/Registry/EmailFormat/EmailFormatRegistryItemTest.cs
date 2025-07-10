using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailFormatRegistryItem))]
	sealed class EmailFormatRegistryItemTest : StronglyTypedRegistryItemTestCase<EmailFormat>
	{
		protected override StronglyTypedRegistryItem<EmailFormat, EmailFormat> GetNewRegistryItem()
		{
			return new EmailFormatRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
