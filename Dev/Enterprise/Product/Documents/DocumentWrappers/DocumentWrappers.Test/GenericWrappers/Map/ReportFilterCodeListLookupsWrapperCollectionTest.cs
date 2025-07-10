using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(ReportFilterCodeListLookupsWrapperCollection))]
	sealed class ReportFilterCodeListLookupsWrapperCollectionTest : GenericWrapperCollectionTest<ReportFilterCodeListLookupsWrapperCollection>
	{
		public void TestCollectionCountAndContent()
		{
			var collection = GetNewDocumentWrapperCollection();
			Assert(collection.Count > 0);

			collection.OfType<CodeMultilingualDescriptionWrapper>().ForEach(item =>
			{
				AssertNotNullOrEmpty(item.Useage);
				AssertNotNullOrEmpty(item.Explanation);
			});
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CodeMultilingualDescriptionWrapper("", (NoResString)"", Factory);
		}

		protected override ReportFilterCodeListLookupsWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ReportFilterCodeListLookupsWrapperCollection(Factory);
		}
	}
}
