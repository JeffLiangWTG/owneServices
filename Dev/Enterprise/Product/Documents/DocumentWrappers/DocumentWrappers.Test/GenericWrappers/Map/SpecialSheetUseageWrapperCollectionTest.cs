using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(SpecialSheetUseageWrapperCollection))]
	sealed class SpecialSheetUseageWrapperCollectionTest : GenericWrapperCollectionTest<SpecialSheetUseageWrapperCollection>
	{
		public void TestLoadsFromAreaListManagerAndNotDuplicated()
		{
			var collection = new SpecialSheetUseageWrapperCollection(Factory, new FilterUseageCodeDescriptionList());
			Assert("collection.Count should be > 2", collection.Count > 2);
			List<ZString> usages = new List<ZString>();
			List<ZString> explanations = new List<ZString>();
			foreach (CodeMultilingualDescriptionWrapper wrapper in collection)
			{
				Assert("wrapper.Useage is not empty", !wrapper.Useage.IsEmpty);
				Assert("wrapper.Explanation is not empty", !wrapper.Explanation.IsEmpty);
				AssertCollectionNotContains("wrapper.Useage should not be duplicated.\r\n-->" + wrapper.Useage, wrapper.Useage, usages);
				AssertCollectionNotContains("wrapper.Explanation should not be duplicated.\r\n-->" + wrapper.Explanation, wrapper.Explanation, explanations);
				usages.Add(wrapper.Useage);
				explanations.Add(wrapper.Explanation);
			}
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CodeMultilingualDescriptionWrapper("", (NoResString)"", Factory);
		}

		protected override SpecialSheetUseageWrapperCollection GetNewDocumentWrapperCollection()
		{
			var collection = new SpecialSheetUseageWrapperCollection(Factory, new FilterUseageCodeDescriptionList());
			return collection;
		}
	}
}
