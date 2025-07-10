using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZTimeEditCoreTest : TestCase
	{
		public virtual void TestGetTimeFromText()
		{
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 10, 30, 0), "10:30");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 10, 14, 0), "10:14");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 1, 3, 0), "01:03");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 0, 16, 0), "00:16");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 0, 1, 0), "00:01");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 0, 0, 0), "00:00");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1, 23, 45, 0), "23:45");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddMinutes(-1), "-00:01");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddMinutes(-15), "-00:15");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(-23).AddMinutes(-45), "-23:45");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddMinutes(45), ":45");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(5), "5:");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddMinutes(-45), "-:45");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(-5), "-5:");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(-5).AddMinutes(-9), "-5:9");
		}

		public void TestGetTimeFromText_NegativeNumberInvalidWhenNotAllowed()
		{
			Core.AllowNegative = false;
			AssertGetTimeFromText(ZDateTime.Invalid, "-10:30");
		}

		public ZTimeEditCore Core
		{
			get
			{
				if (core == null)
				{
					core = NewCore();
					core.AllowNegative = true;
				}
				return core;
			}
		}
		ZTimeEditCore core;

		protected virtual ZTimeEditCore NewCore()
		{
			return new ZTimeEditCore(Control);
		}

		public void AssertGetTimeFromText(ZDateTime expectedTime, string text)
		{
			AssertEquals(expectedTime, Core.GetTimeFromText(text));
		}

		public ZTimeEdit Control
		{
			get { return control ?? (control = NewEditor()); }
		}
		ZTimeEdit control;

		protected virtual ZTimeEdit NewEditor()
		{
			return new ZTimeEdit();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}
	}
}
