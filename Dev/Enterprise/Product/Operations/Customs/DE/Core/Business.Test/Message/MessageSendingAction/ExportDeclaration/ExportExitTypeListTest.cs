using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ExportExitTypeListTest : TestCase
	{
		public void TestIs2()
		{
			Assert(ExportExitTypeList.Is2(ExportExitTypeList.Codes._2));
			Assert(!ExportExitTypeList.Is2(ExportExitTypeList.Codes._4));
		}

		public void TestIs2_4()
		{
			Assert(ExportExitTypeList.Is2_4(ExportExitTypeList.Codes._2));
			Assert(ExportExitTypeList.Is2_4(ExportExitTypeList.Codes._4));
		}

		public void TestIs4()
		{
			Assert(!ExportExitTypeList.Is4(ExportExitTypeList.Codes._2));
			Assert(ExportExitTypeList.Is4(ExportExitTypeList.Codes._4));
		}
	}
}
