
using System;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZTimeZoneFindBoxTest : FindBoxTestFramework
	{
		public void TestTextInDropDownSuccessfullyParsedFromOffsetValue()
		{
			CreateControls(Form, "");
			Form.Show();

			ZTimeZoneFindBox.CodeBox.Text = "AUSYD";

			ZTimeZoneFindBox.OffsetValue = new TimeSpan(10, 0, 0);
			AssertEquals("The drop down text should be 'GMT +10:00'", "GMT +10:00", ZTimeZoneFindBox.TimeZoneComboBox.Text);

			ZTimeZoneFindBox.OffsetValue = TimeSpan.Zero;
			AssertEquals("The drop down text should be 'GMT +00:00'", "GMT +00:00", ZTimeZoneFindBox.TimeZoneComboBox.Text);
		}

		public void TestCodeBoxTextSuccessfullyChangesOffsetValueSelectedAndDropDownText()
		{
			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();

				ZTimeZoneFindBox.CodeBox.Text = "BRFEN";
				AssertEquals("Because the text of the CodeBox is BRFEN with an offset of -2:00 the expected OffsetValue should also be this.", new TimeSpan(-2, 0, 0), ZTimeZoneFindBox.OffsetValue);
				AssertEquals("Accordingly, the text of the drop down should change to 'GMT -02:00'.", "GMT -02:00", ZTimeZoneFindBox.TimeZoneComboBox.Text);

				ZTimeZoneFindBox.CodeBox.Text = "AQMAW";
				AssertEquals("Because the text of the CodeBox is AQMAW with an offset of +06:00 the expected OffsetValue should also be this.", new TimeSpan(6, 0, 0), ZTimeZoneFindBox.OffsetValue);
				AssertEquals("Accordingly, the text of the drop down should change to 'GMT +06:00'.", "GMT +06:00", ZTimeZoneFindBox.TimeZoneComboBox.Text);
			}
		}
			
		#region Implementation

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		protected ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZTimeZoneFindBox(); }
		}

		protected ZTimeZoneFindBox ZTimeZoneFindBox
		{
			get { return (ZTimeZoneFindBox)FindBox; }
		}

		protected void SetBindTo(ZFindBoxUserControl findBox)
		{
			findBox.BindTo = "SS_Dummy";
			findBox.BindToList = "Dummies";

			if (!string.IsNullOrEmpty(BindToForDropEdit))
			{
				((ZTimeZoneFindBox)findBox).BindToForDropEdit = BindToForDropEdit;
			}
		}

		protected string BindToForDropEdit = "";

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "SS_Dummy");

			FindBox = NewFindBoxTester;
			testForm.Controls.Add(FindBox);

			SetBindTo(FindBox);
			testForm.SetDataBinding(Dummy, "");
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (FindBox != null)
			{
				FindBox.Dispose();
			}

			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
