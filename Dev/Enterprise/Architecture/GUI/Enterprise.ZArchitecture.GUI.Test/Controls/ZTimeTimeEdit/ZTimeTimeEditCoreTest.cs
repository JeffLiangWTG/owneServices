using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZTimeTimeEditCoreTest : TestCase
	{
		public virtual void TestGetTimeFromText()
		{
			AssertGetTimeFromText(new ZTime(10, 30), "10:30");
			AssertGetTimeFromText(new ZTime(10, 14), "10:14");
			AssertGetTimeFromText(new ZTime(1, 3), "01:03");
			AssertGetTimeFromText(new ZTime(0, 16), "00:16");
			AssertGetTimeFromText(new ZTime(0, 1), "00:01");
			AssertGetTimeFromText(new ZTime(0, 0), "00:00");
			AssertGetTimeFromText(new ZTime(23, 45), "23:45");
			AssertGetTimeFromText(new ZTime(0, 0).AddMinutes(45), ":45");
			AssertGetTimeFromText(new ZTime(0, 0).AddHours(5), "5:");
		}

		public void TestGetTimeFromText_NegativeNumberInvalidWhenNotAllowed()
		{
			Core.AllowNegative = false;
			AssertGetTimeFromText(ZTime.Invalid, "-10:30");
		}

		public ZTimeTimeEditCore Core
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
		ZTimeTimeEditCore core;

		protected virtual ZTimeTimeEditCore NewCore()
		{
			return new ZTimeTimeEditCore(Control);
		}

		public void AssertGetTimeFromText(ZTime expectedTime, string text)
		{
			AssertEquals(expectedTime, Core.GetTimeFromText(text));
		}

		public ZTimeTimeEdit Control
		{
			get { return control ?? (control = NewEditor()); }
		}
		ZTimeTimeEdit control;

		protected virtual ZTimeTimeEdit NewEditor()
		{
			return new ZTimeTimeEdit();
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
