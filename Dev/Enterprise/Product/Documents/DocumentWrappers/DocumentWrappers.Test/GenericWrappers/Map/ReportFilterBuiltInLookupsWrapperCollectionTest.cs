using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(ReportFilterBuiltInLookupsWrapperCollection))]
	sealed class ReportFilterBuiltInLookupsWrapperCollectionTest : GenericWrapperCollectionTest<ReportFilterBuiltInLookupsWrapperCollection>
	{
		public void TestLoadsFromAreaListManagerAndNotDuplicated()
		{
			var collection = GetNewDocumentWrapperCollection();
			Assert("collection.Count should be > 5", collection.Count > 5);
			List<ZString> usages = new List<ZString>();
			List<ZString> explanations = new List<ZString>();
			foreach (CodeMultilingualDescriptionWrapper wrapper in collection)
			{
				Assert("wrapper.Useage is not empty", !wrapper.Useage.IsEmpty);
				Assert("wrapper.Explanation is not empty", !wrapper.Explanation.IsEmpty);
				AssertCollectionNotContains("wrapper.Useage should not be duplicated.\r\n-->" + wrapper.Useage, wrapper.Useage, usages);
				AssertCollectionNotContains("wrapper.Explanation should not be duplicated.\r\n-->" + wrapper.Explanation, wrapper.Explanation, explanations);
				usages.Add(wrapper.Useage);
				if (!DuplicateExplanations.Contains(wrapper.Explanation))
				{
					explanations.Add(wrapper.Explanation);
				}
			}
		}

		List<string> duplicateExplanations;
		List<string> DuplicateExplanations
		{
			get
			{
				if (duplicateExplanations == null)
				{
					duplicateExplanations = new List<string>()
					{
						"Get all active organizations.",
						"Get all active organizations which are forwarders.",
						"Get all active consignors.",
						"Get all active branches under current company.",
						"Get all active departments."
					};
				}
				return duplicateExplanations;
			}
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CodeMultilingualDescriptionWrapper("", (NoResString)"", Factory);
		}

		protected override ReportFilterBuiltInLookupsWrapperCollection GetNewDocumentWrapperCollection()
		{
			var collection = new ReportFilterBuiltInLookupsWrapperCollection(Factory);
			return collection;
		}
	}
}
