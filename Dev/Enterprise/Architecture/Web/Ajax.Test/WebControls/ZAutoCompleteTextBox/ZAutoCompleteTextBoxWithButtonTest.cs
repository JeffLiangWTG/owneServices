using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class ZAutoCompleteTextBoxWithButtonForTesting : ZAutoCompleteTextBoxWithButton
	{
		internal ZAutoCompleteTextBox TextBoxForTesting => TextBox;

		internal ZFindBox ButtonForTesting => Button;
		internal void OnInitForTesting(EventArgs e) => OnInit(e);
		internal void EnsureChildControlsForTesting() => EnsureChildControls();
	}

	sealed class ZAutoCompleteTextBoxWithButtonTest : WebControlTest
	{
		#region TestProperties

		public void TestProperties()
		{
			AutoCompleteTextBoxWithButton.ModuleID = WebModuleIDs.RefCountry;
			AutoCompleteTextBoxWithButton.OnInitForTesting(new EventArgs());
			AutoCompleteTextBoxWithButton.EnsureChildControlsForTesting();

			AssertEquals("BindTextTo", AutoCompleteTextBoxWithButton.TextBoxForTesting.BindTextTo, AutoCompleteTextBoxWithButton.BindTextTo);

			AssertEquals("ModuleID", AutoCompleteTextBoxWithButton.ButtonForTesting.ModuleID, AutoCompleteTextBoxWithButton.ModuleID);

			AutoCompleteTextBoxWithButton.AutoPostBack = true;
			AssertEquals("AutoPostBack (TextBox)", AutoCompleteTextBoxWithButton.TextBoxForTesting.AutoPostBack, AutoCompleteTextBoxWithButton.AutoPostBack);
			AssertEquals("AutoPostBack (Button)", AutoCompleteTextBoxWithButton.ButtonForTesting.AutoPostBack, AutoCompleteTextBoxWithButton.AutoPostBack);

			AutoCompleteTextBoxWithButton.Helper = new CountryAutoCompleteHelper(Factory);
			AssertEquals("Helper (TextBox)", AutoCompleteTextBoxWithButton.TextBoxForTesting.Helper, AutoCompleteTextBoxWithButton.Helper);

			AutoCompleteTextBoxWithButton.Width = Unit.Pixel(100);
			AssertEquals("Whole width", Unit.Pixel(100 + 25), AutoCompleteTextBoxWithButton.Width);
			AssertEquals("TextBox width", Unit.Pixel(100), AutoCompleteTextBoxWithButton.TextBoxForTesting.Width);

			AutoCompleteTextBoxWithButton.PopupEnabled = false;
			AssertEquals("PopupEnabled", AutoCompleteTextBoxWithButton.ButtonForTesting.Enabled, AutoCompleteTextBoxWithButton.PopupEnabled);

			AutoCompleteTextBoxWithButton.TextBoxForTesting.HasChanges = true;
			AssertEquals("HasChanges", AutoCompleteTextBoxWithButton.TextBoxForTesting.HasChanges, AutoCompleteTextBoxWithButton.HasChanges);

			AutoCompleteTextBoxWithButton.BindTo = TestBizO.Z0_CodeInfo.Name;
			AssertEquals("BindTo (TextBox)", AutoCompleteTextBoxWithButton.TextBoxForTesting.BindTo, AutoCompleteTextBoxWithButton.BindTo);
			AssertEquals("BindTo (Button)", AutoCompleteTextBoxWithButton.ButtonForTesting.BindTo, AutoCompleteTextBoxWithButton.BindTo);
		}

		#endregion

		#region TestBinding

		public void TestBinding()
		{
			AutoCompleteTextBoxWithButton.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBoxWithButton.BindTo = TestBizO.Z0_CodeInfo.Name;
			AutoCompleteTextBoxWithButton.OnInitForTesting(new EventArgs());
			TestBizO.Z0_Code = "UA";
			AutoCompleteTextBoxWithButton.Bind(TestBizO);

			AssertEquals("Bounded value (TextBox)", "Ukraine", AutoCompleteTextBoxWithButton.TextBoxForTesting.Text);
		}

		#endregion

		#region TestIPostBackDataHandler

		public void TestIPostBackDataHandler()
		{
			IPostBackDataHandler postBackDataHandler = AutoCompleteTextBoxWithButton;
			AssertNotNull(postBackDataHandler);
			AutoCompleteTextBoxWithButton.ModuleID = WebModuleIDs.RefCountry;
			NameValueCollection postData = new NameValueCollection();
			AutoCompleteTextBoxWithButton.OnInitForTesting(new EventArgs());
			postData.Add(AutoCompleteTextBoxWithButton.UniqueID, "xxxxx");
			bool result = AutoCompleteTextBoxWithButton.LoadPostData(AutoCompleteTextBoxWithButton.UniqueID, postData);
			Assert(result);
			AssertEquals("Empty becasue bindtotext is empty - so we cant hold info anywhere", ZString.Empty, AutoCompleteTextBoxWithButton.TextBoxForTesting.Text);
			AutoCompleteTextBoxWithButton.BindTextTo = "someproperty";
			result = AutoCompleteTextBoxWithButton.LoadPostData(AutoCompleteTextBoxWithButton.UniqueID, postData);
			Assert(result);
			AssertEquals("xxxxx", AutoCompleteTextBoxWithButton.TextBoxForTesting.Text);

			result = AutoCompleteTextBoxWithButton.LoadPostData(AutoCompleteTextBoxWithButton.UniqueID, postData);
			Assert(!result);
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZAutoCompleteTextBoxWithButtonForTesting();
		}

		ZAutoCompleteTextBoxWithButtonForTesting AutoCompleteTextBoxWithButton
		{
			get
			{
				return Control as ZAutoCompleteTextBoxWithButtonForTesting;
			}
		}

		#endregion
	}
}
