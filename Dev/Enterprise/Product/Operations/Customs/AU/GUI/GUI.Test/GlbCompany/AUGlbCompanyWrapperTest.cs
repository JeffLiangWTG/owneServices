using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AUGlbCompanyWrapper))]
	sealed class AUGlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<AUGlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			Assert(Wrapper.IsValidWrapper);
		}
	}
}
