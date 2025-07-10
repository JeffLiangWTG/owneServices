namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class CustomsEntryStatusTypeListTest : NUnit.Framework.TestCase
	{
		public void TestDeclinedByCustoms()
		{
			Assert(!CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.ACL));
			Assert(CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.DMS));
			Assert(!CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.EDC));
			Assert(CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.CCL));
			Assert(!CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.ICG));
			Assert(CustomsEntryStatusTypeList.IsDeclinedByCustoms(CustomsEntryStatusTypeList.Codes.RJC));
		}
	}
}
