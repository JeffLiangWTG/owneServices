using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Scanning
{
	public class KeyToAsciiTest : TestCase
	{
		#region TestToAscii()

		public void TestToAscii()
		{
			// valid keys
			AssertEquals('a', KeyToAscii.ToAscii((int)Keys.A));
			AssertEquals('g', KeyToAscii.ToAscii((int)Keys.G));
			AssertEquals('z', KeyToAscii.ToAscii((int)Keys.Z));
			AssertEquals('0', KeyToAscii.ToAscii((int)Keys.D0));
			AssertEquals('1', KeyToAscii.ToAscii((int)Keys.D1));
			AssertEquals('0', KeyToAscii.ToAscii((int)Keys.NumPad0));
			AssertEquals('1', KeyToAscii.ToAscii((int)Keys.NumPad1));
			AssertEquals(' ', KeyToAscii.ToAscii((int)Keys.Space));
			AssertEquals('*', KeyToAscii.ToAscii((int)Keys.D8, true, false,false));

			// invalid keys
			AssertEquals('\0', KeyToAscii.ToAscii((int)Keys.Control));
			AssertEquals('\0', KeyToAscii.ToAscii((int)Keys.ControlKey));
			AssertEquals('\0', KeyToAscii.ToAscii(0));
		}

		#endregion
	}
}
