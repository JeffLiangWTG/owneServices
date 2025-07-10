using CargoWise.Types;

namespace Enterprise.VisualBoards.Business.Test
{
	class PropertyCacheTest : VisualBoardsTestCase
	{
		public void FAT_TestPropertyCacheSpeed()
		{
			var cache = new PropertyCache();

			var propertyName = "name";
			for (int i = 0; i < 1000000; i++)
			{
				cache.GetCachedValue(ZGuid.NewZGuid(), propertyName, () => 1);
			}

			Assert(true);
		}
	}
}
