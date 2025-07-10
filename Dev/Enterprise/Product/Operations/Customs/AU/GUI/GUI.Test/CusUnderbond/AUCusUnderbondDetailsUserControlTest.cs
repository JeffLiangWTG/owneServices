using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCusUnderbondDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGetCusOutturnUserControl()
		{
			using (var control = new AUCusUnderbondDetailsUserControlForTest())
			{
				AssertType(typeof(AUCusOutturnUserControl), control.OutturnUserControlTest);
			}
		}

		sealed class AUCusUnderbondDetailsUserControlForTest : AUCusUnderbondDetailsUserControl
		{
			public CusOutturnUserControl OutturnUserControlTest => OutturnUserControl;
		}
	}
}
