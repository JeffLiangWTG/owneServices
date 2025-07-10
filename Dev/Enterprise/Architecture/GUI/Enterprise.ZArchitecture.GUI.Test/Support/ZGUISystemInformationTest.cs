using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGUISystemInformationTest : TestCase
	{
		public void TestVerticalScrollBarWidth()
		{
			AssertEquals("VerticalScrollBarWidth", SystemInformation.VerticalScrollBarWidth, ZGUISystemInformation.VerticalScrollBarWidth);
		}
	}
}
