using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class BaseZCodeFindBoxTest : BaseFindBoxTest
	{
		public void TestBindToForDescription()
		{
			CreateDummies();
			Dummy.SS_Dummy = ZString.Empty;
			Dummy.SS_Name = ZString.Empty;

			BindToForDescription = "SS_Name";
			CreateControls(Form, "SS_Dummy");
			Form.Show();
			AssertEquals("ZCodeFindBox.CodeBox.Text", "", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "", ZCodeFindBox.DescriptionBox.Text);

			SetValueBoundToToGetAABCDTestValues();
			AssertEquals("ZCodeFindBox.CodeBox.Text", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "", ZCodeFindBox.DescriptionBox.Text);

			Dummy.SS_Name = new ZString("We Have a Description");
			AssertEquals("ZCodeFindBox.CodeBox.Text", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "We Have a Description", ZCodeFindBox.DescriptionBox.Text);

			ClearValueBoundTo();
			AssertEquals("ZCodeFindBox.CodeBox.Text", "", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "We Have a Description", ZCodeFindBox.DescriptionBox.Text);

			Dummy.SS_Name = ZString.Empty;
			AssertEquals("ZCodeFindBox.CodeBox.Text", "", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "", ZCodeFindBox.DescriptionBox.Text);
		}

		#region Implementation

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		protected virtual void SetValueBoundToToGetAABCDTestValues()
		{
			Dummy.SS_Dummy = "AABCD";
		}

		protected virtual void ClearValueBoundTo()
		{
			Dummy.SS_Dummy = ZString.Empty;
		}

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZCodeFindBox(); }
		}

		protected ZCodeFindBox ZCodeFindBox
		{
			get { return (ZCodeFindBox)FindBox; }
		}

		protected override void SetBindTo(ZFindBoxUserControl findBox)
		{
			base.SetBindTo(findBox);
			if (!string.IsNullOrEmpty(BindToForDescription))
			{
				((ZCodeFindBox)findBox).BindToForDescription = BindToForDescription;
			}
		}

		protected string BindToForDescription = "";

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
