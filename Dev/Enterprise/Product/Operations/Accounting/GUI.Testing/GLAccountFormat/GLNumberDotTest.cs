using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GLAccountFormat.Testing
{
	public class GLNumberDotTest : TestCase
	{
		public void TestConstructor()
		{
			using (GLNumberDot dot = new GLNumberDot())
			{
				AssertEquals(".", dot.Text);
				Assert(dot.AutoSize);
			}
		}
	}
}
