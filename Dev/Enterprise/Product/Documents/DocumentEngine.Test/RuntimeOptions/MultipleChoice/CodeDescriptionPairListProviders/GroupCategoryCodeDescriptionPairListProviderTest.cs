using System;
using CargoWise.Integration;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	sealed class GroupCategoryCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest<GroupCategoryCodeDescriptionPairListProvider>
	{
		protected override ICodeDescriptionPairList GetExpectedCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("JAM", "James Bisanette"),
				new CodeDescriptionPair("KEL", "Kelly Moneymaker"),
				new CodeDescriptionPair("DAV", "David Archeologist"),
			};

			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			return list;
		}
	}
}
