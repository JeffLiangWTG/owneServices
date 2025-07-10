using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.GDM.Testing
{
	sealed class SummaryControlTest : TestCase
	{
		public void TestGDMBasicLayoutType()
		{
			using (var control = new SummaryControl())
			{
				AssertType<GDMBasicLayout>(control.GetGDMBasicLayout());
			}
		}
	}
}
