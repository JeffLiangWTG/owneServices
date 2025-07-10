using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class DirectDebitFileCreationURLRegistryItem : StronglyTypedRegistryItem<DirectDebitFileCreationURLCollection>
	{
		public DirectDebitFileCreationURLRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DirectDebitFileCreationURLsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.DirectDebitFileCreationURLsRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class DirectDebitFileCreationURLsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DirectDebitFileCreationURLCollection>
	{
		protected override bool ValuesAreEqualCore(DirectDebitFileCreationURLCollection a, DirectDebitFileCreationURLCollection b)
		{
			//Implementation inferred through reading the test
			if (a.Count != b.Count)
			{
				return false;
			}

			for (var i = 0; i < a.Count; i++)
			{
				var itemA = a[i];
				var itemB = b[i];

				if (itemA.BankAccountPK != itemB.BankAccountPK || itemA.BankWebsite != itemB.BankWebsite)
				{
					return false;
				}
			}

			return true;
		}
	}
}
