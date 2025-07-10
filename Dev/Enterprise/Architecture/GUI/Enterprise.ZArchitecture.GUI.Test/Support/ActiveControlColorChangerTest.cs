using System.Drawing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ActiveControlColorChangerTest : TestCase
	{
		readonly Color activeColor = EnterpriseFormLookStrategy.SelectedControlColor;
		readonly Color notificationColor = Color.Green;
		readonly Color validColor = Color.FromArgb(198, 236, 198);
		readonly Color readonlyColor = SystemColors.Control;
		readonly Color colorBackColor = Color.DeepPink;
		readonly Color explicitBackColor = Color.Aqua;

		public void TestWatchesForFocusChangeAndSetActiveColor()
		{
			var control = new ZTextBox { BackColor = colorBackColor };
			var control2 = new ZTextBox();

			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Controls.Add(control2);

				form.Show();

				control.Focus();
				AssertEquals(activeColor, control.BackColor);

				control2.Focus();
				AssertEquals(colorBackColor, control.BackColor);
			}
		}

		public void TestDirectColorChangeHandling()
		{
			var control = new ZTextBox { BackColor = colorBackColor };
			control.EnableValidStateColor = true;

			var changer = new ActiveControlColorChanger(control);
			AssertEquals(colorBackColor, control.BackColor);

			changer.SetActiveBackColor();
			AssertEquals(activeColor, control.BackColor);

			changer.ClearActiveBackColor();
			AssertEquals(colorBackColor, control.BackColor);

			changer.SetReadonlyColor();
			AssertEquals(readonlyColor, control.BackColor);

			changer.ClearReadonlyColor();
			AssertEquals(colorBackColor, control.BackColor);

			changer.SetNotificationBackColor(notificationColor);
			AssertEquals(notificationColor, control.BackColor);

			changer.ClearNotificationBackColor();
			AssertEquals(colorBackColor, control.BackColor);

			changer.SetValidColorIfNeeded();
			AssertEquals(validColor, control.BackColor);

			changer.ClearValidColorIfNeeded();
			AssertEquals(colorBackColor, control.BackColor);

			control.Dispose();
		}

		public void TestSettingColorBackColorItWillNotAffectAnotherColorOfHigherPriority()
		{
			var control = new ZTextBox { BackColor = colorBackColor };
			control.EnableValidStateColor = true;
			var changer = new ActiveControlColorChanger(control);

			var controlBackColor2 = Color.RosyBrown;

			changer.SetReadonlyColor();
			AssertEquals(readonlyColor, control.BackColor);

			control.BackColor = controlBackColor2;
			AssertEquals(readonlyColor, control.BackColor);

			changer.ClearReadonlyColor();
			AssertEquals(controlBackColor2, control.BackColor);

			changer.SetActiveBackColor();
			AssertEquals(activeColor, control.BackColor);

			control.BackColor = colorBackColor;
			AssertEquals(activeColor, control.BackColor);

			changer.ClearActiveBackColor();
			AssertEquals(colorBackColor, control.BackColor);

			changer.SetNotificationBackColor(notificationColor);
			AssertEquals(notificationColor, control.BackColor);

			control.BackColor = controlBackColor2;
			AssertEquals(notificationColor, control.BackColor);

			changer.ClearNotificationBackColor();
			AssertEquals(controlBackColor2, control.BackColor);

			changer.SetValidColorIfNeeded();
			AssertEquals(validColor, control.BackColor);

			changer.ClearValidColorIfNeeded();
			AssertEquals(controlBackColor2, control.BackColor);

			control.Dispose();
		}

		public void TestRespectsColorPriority()
		{
			var control = new ZTextBox { BackColor = colorBackColor };
			var changer = new ActiveControlColorChanger(control);

			changer.SetReadonlyColor();
			AssertEquals(readonlyColor, control.BackColor);

			changer.SetNotificationBackColor(notificationColor);
			AssertEquals(notificationColor, control.BackColor);

			changer.SetActiveBackColor();
			AssertEquals(notificationColor, control.BackColor);

			changer.ClearActiveBackColor();
			AssertEquals(notificationColor, control.BackColor);

			changer.ClearNotificationBackColor();
			AssertEquals(readonlyColor, control.BackColor);

			changer.ClearReadonlyColor();
			AssertEquals(colorBackColor, control.BackColor);

			control.Dispose();
		}

		public void TestForcingBackColor()
		{
			var control = new ZTextBox();
			var changer = new ActiveControlColorChanger(control);

			changer.SetReadonlyColor();
			AssertEquals(readonlyColor, control.BackColor);

			changer.ForceBackColor(explicitBackColor);
			AssertEquals(explicitBackColor, control.BackColor);

			changer.SetActiveBackColor();
			AssertEquals(explicitBackColor, control.BackColor);

			control.Dispose();
		}

		public void TestResetForcedColor()
		{
			using (var control = new FocusableControl { BackColor = colorBackColor })
			{
				var changer = new ActiveControlColorChanger(control);

				control.Focused_Override = true;

				changer.ForceBackColor(explicitBackColor);
				AssertEquals(explicitBackColor, control.BackColor);

				changer.ResetForcedColor();
				AssertEquals(activeColor, control.BackColor);

				control.Focused_Override = false;

				changer.ForceBackColor(explicitBackColor);
				AssertEquals(explicitBackColor, control.BackColor);

				changer.ResetForcedColor();
				AssertEquals(colorBackColor, control.BackColor);
			}
		}

		class FocusableControl : ZTextBox
		{
			public override bool Focused => Focused_Override;

			internal bool Focused_Override { get; set; }
		}
	}
}
