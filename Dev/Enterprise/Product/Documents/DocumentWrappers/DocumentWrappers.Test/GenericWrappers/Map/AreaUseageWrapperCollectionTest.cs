using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(AreaUseageWrapperCollection))]
	sealed class AreaUseageWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<AreaUseageWrapperCollection>
	{
		public void TestLoadsFromAreaListManagerAndNotDuplicated()
		{
			AreaUseageWrapperCollection collection = new AreaUseageWrapperCollection(Factory);
			Assert("collection.Count should be > 5", collection.Count > 5);
			List<ZString> usages = new List<ZString>();
			List<ZString> explanations = new List<ZString>();
			foreach (AreaUseageWrapper wrapper in collection)
			{
				AssertEquals("wrapper.Useage.IsEmpty", false, wrapper.Useage.IsEmpty);
				AssertEquals("wrapper.Explanation.IsEmpty", false, wrapper.Explanation.IsEmpty);
				AssertCollectionNotContains("wrapper.Useage should not be duplicated.\r\n-->" + wrapper.Useage, wrapper.Useage, usages);
				AssertCollectionNotContains("wrapper.Explanation should not be duplicated.\r\n-->" + wrapper.Explanation, wrapper.Explanation, explanations);
				usages.Add(wrapper.Useage);
				explanations.Add(wrapper.Explanation);
			}
		}

		protected override Base.GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new AreaUseageWrapper(null, Factory);
		}

		protected override AreaUseageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new AreaUseageWrapperCollection(Factory);
		}
	}
}
