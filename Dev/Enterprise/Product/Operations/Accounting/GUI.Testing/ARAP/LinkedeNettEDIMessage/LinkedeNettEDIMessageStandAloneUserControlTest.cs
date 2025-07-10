using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public class LinkedeNettEDIMessageStandAloneUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			using (LinkedeNettEDIMessageStandAloneUserControl control = new LinkedeNettEDIMessageStandAloneUserControl())
			{
				Assert(control.CaptionRenderingEnabled.Value);
			}
		}
	}
}
