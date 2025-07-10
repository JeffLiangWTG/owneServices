using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(DocumentUDFFieldBuilderWrapperCollection))]
	sealed class DocumentUDFFieldBuilderWrapperCollectionTest : GenericWrapperCollectionTest<DocumentUDFFieldBuilderWrapperCollection>
	{
		public void TestLoadsFromAreaListManagerAndNotDuplicated()
		{
			var collection = GetNewDocumentWrapperCollection();
			Assert("collection.Count should be > 5", collection.Count > 5);
			List<ZString> usages = new List<ZString>();
			List<ZString> explanations = new List<ZString>();
			foreach (ReportFilterBuilderWrapper wrapper in collection)
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
			return new ReportFilterBuilderWrapper(new FilterBuilderDocumenter("", "", new List<string>()), Factory);
		}

		protected override DocumentUDFFieldBuilderWrapperCollection GetNewDocumentWrapperCollection()
		{
			var collection = new DocumentUDFFieldBuilderWrapperCollection(Factory);
			return collection;
		}
	}
}
