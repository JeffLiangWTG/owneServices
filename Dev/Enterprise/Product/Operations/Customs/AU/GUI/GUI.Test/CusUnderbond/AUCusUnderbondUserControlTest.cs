using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCusUnderbondUserControlTest : TestCaseWithFactory
	{
		public void GetCusUnderbondDetailsUserControl()
		{
			using (var control = new AUCusUnderbondUserControlForTest())
			{
				AssertType(typeof(AUCusUnderbondDetailsUserControl), control.GetCusUnderbondDetailsUserControlForTest());
			}
		}

		sealed class AUCusUnderbondUserControlForTest : AUCusUnderbondUserControl
		{
			public CusUnderbondDetailsUserControl GetCusUnderbondDetailsUserControlForTest() => GetCusUnderbondDetailsUserControl();
		}
	}
}
