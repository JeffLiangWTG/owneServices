using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AllowBackPostingSubLedgerTransactionRegistryItem))]
	class AllowBackPostingSubLedgerTransactionRegistryItemTest : BooleanRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new AllowBackPostingSubLedgerTransactionRegistryItem("", null, null, null, RegistryStorageFlags.System, false);
		}

		public void TestDataType()
		{
			AllowBackPostingSubLedgerTransactionRegistryItem testRegistryItem = (AllowBackPostingSubLedgerTransactionRegistryItem)GetNewRegistryItem();
			Assert("DataType must be correct.", testRegistryItem.DataType is AllowBackPostingSubLedgerTransactionRegistryDataType);
		}
	}
}
