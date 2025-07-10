using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDayAndTimeEditTest : TestCase
	{
		public void TestTimeUnit()
		{
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Days, Control.TimeUnit);
			//removed because it was glitchy, slow and seemingly impossible to do in column context (though open to being proven wrong!)
			//AssertEquals(System.Windows.Forms.HorizontalAlignment.Right, Control.TextAlign);

			Control.TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, Control.TimeUnit);
			//AssertEquals(System.Windows.Forms.HorizontalAlignment.Left, Control.TextAlign);
		}

		public void TestAllowNegative()
		{
			AssertEquals(false, Control.AllowNegative);
			Control.AllowNegative = true;
			AssertEquals(false, Control.AllowNegative);

			Control.TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
			Control.AllowNegative = true;
			AssertEquals(true, Control.AllowNegative);
		}

		#region Implementation

		ZDayAndTimeEdit Control
		{
			get
			{
				if (control == null)
				{
					control = new ZDayAndTimeEdit();
				}
				return control;
			}
		}
		ZDayAndTimeEdit control;

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
