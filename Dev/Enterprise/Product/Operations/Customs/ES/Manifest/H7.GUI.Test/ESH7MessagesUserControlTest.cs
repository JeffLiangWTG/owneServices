using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	sealed class ESH7MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessageTabCaption()
		{
			using (var control = new ESH7MessagesUserControl())
			{
				AssertEquals("G3 Messages", ((IAdditionalTabPage)control).AdditionalTabPageCaption.Caption);
			}
		}
	}
}
