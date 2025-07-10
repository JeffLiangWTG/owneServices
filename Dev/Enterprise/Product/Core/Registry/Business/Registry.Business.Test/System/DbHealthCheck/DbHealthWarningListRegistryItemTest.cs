using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DbHealthWarningListRegistryItem))]
	sealed class DbHealthWarningListRegistryItemTest : StronglyTypedRegistryItemTestCase<DbHealthWarningRegistryCollection>
	{
		public void TestMustOverrideDefaultValue()
		{
			var dbHealthWarningRegistryItem = GetNewRegistryItem();
			AssertEquals(
				"Users cannot clear the warning list by setting DbHealthWarningListRegistryItem back to its default value. MustOverrideDefaultValue option on?",
				true, (dbHealthWarningRegistryItem.Options & RegistryOptions.MustOverrideDefaultValue) == RegistryOptions.MustOverrideDefaultValue);
		}

		protected override StronglyTypedRegistryItem<DbHealthWarningRegistryCollection, DbHealthWarningRegistryCollection> GetNewRegistryItem()
		{
			return new DbHealthWarningListRegistryItem("", null, null, null, new DbHealthWarningRegistryCollection());
		}
	}
}
