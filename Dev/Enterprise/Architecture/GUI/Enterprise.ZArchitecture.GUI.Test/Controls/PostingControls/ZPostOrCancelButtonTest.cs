using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZPostOrCancelButtonTest : TestCase
	{
		public void TestTabStop()
		{
			AssertEquals(true, Button.TabStop);
		}

		#region Implementation

		ZPostOrCancelButton Button
		{
			get { return button ?? (button = new ZPostOrCancelButton()); }
		}
		ZPostOrCancelButton button;

		protected override void TearDown()
		{
			base.TearDown();
			if (button != null)
			{
				button.Dispose();
			}
		}

		#endregion
	}
}
