using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZFilterStripDropHelperTest : TestCase
	{
		public void TestIsCategoryItem()
		{
			AssertEquals(true, Helper.IsCategoryItem(new CategoryCodeDescriptionPair("", "")));
			AssertEquals(false, Helper.IsCategoryItem(new CodeDescriptionPair("", "")));
		}

		public void TestIsCommonItem()
		{
			Filter.IsCommon = true;
			AssertEquals(true, Helper.IsCommonItem(Filter));

			Filter.IsCommon = false;
			AssertEquals(false, Helper.IsCommonItem(Filter));
		}

		public void TestIsExclusiveItem()
		{
			Filter.IsExclusiveHelper = true;
			AssertEquals(true, Helper.IsExclusiveItem(Filter));

			Filter.IsExclusiveHelper = false;
			AssertEquals(false, Helper.IsExclusiveItem(Filter));
		}

		public void TestIsFilterAlreadySelected()
		{
			Filter.IsActive = true;
			AssertEquals(true, Helper.IsFilterAlreadySelected(Filter));

			Filter.IsActive = false;
			AssertEquals(false, Helper.IsFilterAlreadySelected(Filter));
		}

		public void TestIsSelectableItem()
		{
			Filter.IsActive = true;
			AssertEquals(true, Helper.IsSelectableItem(Filter)); // due to duplicate 'OR', you can now select the same filter twice
			AssertEquals(false, Helper.IsSelectableItem(new CategoryCodeDescriptionPair("", "category item")));
			AssertEquals(false, Helper.IsSelectableItem(new CodeDescriptionPair("", "code is empty")));

			Filter.IsActive = false;
			AssertEquals(true, Helper.IsSelectableItem(Filter));
			AssertEquals(true, Helper.IsSelectableItem(new CodeDescriptionPair("code", "description")));
			AssertEquals(false, Helper.IsSelectableItem(null));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new ZFilterStripDropHelper();
			Filter = new DummyModuleFilter("Description", DummyBizoSchema.Z0_Code);
		}

		ZFilterStripDropHelper Helper;
		DummyModuleFilter Filter;

		#endregion
	}
}
