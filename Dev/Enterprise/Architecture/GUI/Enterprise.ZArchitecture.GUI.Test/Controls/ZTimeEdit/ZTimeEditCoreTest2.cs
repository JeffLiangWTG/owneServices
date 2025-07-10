using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTimeEditCoreTest2 : ZTimeEditCoreTest
	{
		public override void TestGetTimeFromText()
		{
			base.TestGetTimeFromText();

			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(999).AddMinutes(45), "99945");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(999).AddMinutes(45), "999:45");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(999).AddMinutes(14), "999:14");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(-999).AddMinutes(-45), "-99945");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(-999).AddMinutes(-14), "-999:14");
		}

		[ExpectNoExceptions]
		public void TestBigNum()
		{
			Control.MaxLength = 999;
			new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(365 * 24).AddMinutes(-14);
		}

		public void TestSizeControlsMax()
		{
			Control.MaxLength = 8;
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(9999).AddMinutes(45), "9999:45");
		}

		protected override ZTimeEditCore NewCore()
		{
			return new ZTimeEditExCore(Control);
		}

		new ZTimeEditEx Control
		{
			get { return (ZTimeEditEx)base.Control; }
		}

		protected override ZTimeEdit NewEditor()
		{
			return new ZTimeEditEx();
		}
	}
}
