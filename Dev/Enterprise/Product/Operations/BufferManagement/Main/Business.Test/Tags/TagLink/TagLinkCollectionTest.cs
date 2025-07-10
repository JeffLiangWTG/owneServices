using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagLinkCollection))]
	class TagLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<TagLinkCollection>
	{
		public void TestNewTagsHaveParentTableCode()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var collection = new TagLinkCollection(workflow);

			var tag = collection.AddNew();

			AssertEquals(workflow.TablePrefix, tag.TGL_ParentTableCode);
			AssertEquals(false, string.IsNullOrEmpty(tag.TGL_ParentTableCode));
		}

		#region Implementation

		protected override TagLinkCollection GetCollectionToTest()
		{
			return new TagLinkCollection(Factory.NewWithValidTestData<ProcessHeader>());
		}

		#endregion
	}
}
