using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GLAccountFormat.Testing
{
	public class GLFormatMatchingFormTest : TestCase
	{
		[ExpectNoExceptions()]
		public void TestConstructor()
		{
			new GLFormatMatchingForm("XX", "XX.X").Dispose();
		}
	}
}
