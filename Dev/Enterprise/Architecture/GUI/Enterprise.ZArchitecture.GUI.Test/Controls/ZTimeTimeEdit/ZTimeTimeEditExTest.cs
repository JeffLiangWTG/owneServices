using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTimeTimeEditExTest : TestCase
	{
		public void TestMaxLength()
		{
			AssertEquals("Should not have bound MaxLength.", "-999.99".Length, Control.MaxLength);

			var width = control.Width;
			Control.MaxLength = 10;

			AssertEquals(10, Control.MaxLength);
			AssertNotEquals(width, Control.Width);
		}

		#region Implementation

		ZTimeTimeEditEx Control
		{
			get
			{
				if (control == null)
				{
					control = new ZTimeTimeEditEx();
				}
				return control;
			}
		}
		ZTimeTimeEditEx control;

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		#endregion
	}
}
