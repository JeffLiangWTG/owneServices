using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class ZAutoCompleteTextBoxForTesting : ZAutoCompleteTextBox
	{
		internal void OnPreRenderForTesting(EventArgs e) => OnPreRender(e);
		internal string GetStartupScriptForTesting() => GetStartupScript();
		internal ZWebResource AutoCompleterScriptForTesting => AutoCompleterScript;
		internal ZWebResource AutoCompleterRequestScriptForTesting => AutoCompleterRequestScript;
		internal ZWebResource ObserverScriptForTesting => ObserverScript;
		internal ZWebResource AutoCompleterCSSForTesting => AutoCompleterCSS;
		internal ZWebResource AutoCompleterImageForTesting => AutoCompleterImage;
	}

	sealed class ZAutoCompleteTextBoxTest : WebControlTest
	{
		#region TestCheckAutoCompleteTextBoxMark

		public void TestCheckAutoCompleteTextBoxMark()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBox.OnPreRenderForTesting(EventArgs.Empty);

			AssertEquals("Expected background-image", AutoCompleteTextBox.Style["background-image"], string.Format("url({0})", AutoCompleteTextBox.AutoCompleterImageForTesting.FileName));
			AssertEquals("Expected background-position", AutoCompleteTextBox.Style["background-position"], "right center");
			AssertEquals("Expected background-repeat", AutoCompleteTextBox.Style["background-repeat"], "no-repeat");
		}

		#endregion

		#region Register Scripts

		public void TestRegisterScriptAndCssFiles()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBox.OnPreRenderForTesting(EventArgs.Empty);

			Assert("AutoCompleter CSS file should be registered on the page", Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZAutoCompleteTextBox), "AutoCompleterCSS"));
			Assert("Observer script file should be registered on the page", Page.ZClientScript.IsClientScriptIncludeRegistered("ObserverScript"));
			Assert("AutoCompleter script file should be registered on the page", Page.ZClientScript.IsClientScriptIncludeRegistered("AutoCompleterScript"));
			Assert("AutoCompleter-Request script file be should registered on the page", Page.ZClientScript.IsClientScriptIncludeRegistered("AutoCompleterRequestScript"));
		}

		public void TestStartupScript()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBox.Helper.MaxOptionsCount = 5;

			string expectedScript = "new Autocompleter.Request.HTML($('ctl01'), '~/AutoCompleteTextBoxRequestHandler.ashx', {'postData': {helper:'QWO8px4dgIV-CDdJPQ3mCte-neeJHHxgwerKKdjL8soBLSm5ulbVdhiXv4YFYRysUcQ1pB7FAjbro7N-8h4lYL|YmccbveuyBvn9l2zcDXCNvWqUvps7j0zDbWgTblS1aJN6lH9ZzqCvUsECwHGECEHBgoaa92Azu-bUaXPEnEFg3d0Z1Raeh2TtZfDaqFiKUDO5kHWoTvYKopheQmeFOUia22sehKHPdG-jmIuCbtq3BXK1ns42HaIWsz7Fagbv2iiU3cUx1Kbr71U-muDM4lJL|mFoVr9SZORng|gEsEAsREE9CpO|vzShj820esCYmBzPNUsxnCyWJ|9A89keMHpo7zZBIYR9PTvhYgab4TLhEUTCkU8vpltU6kzrVBTboQh7mn4MdUCDUJ1Tb6ZNu4CyrmLjKppJqhaivEctjBWlrWawre9oX5fALBkXxDS-vD1j-j8fwq3pVYsOF2L7TA!!',params:'aE3cFTDBzaQmq2UR5mqjjA!!',count:'cGrWSIsOJZx2Bxoi42mcDg!!'},'maxChoices': 5});";
			AssertEquals(expectedScript, AutoCompleteTextBox.GetStartupScriptForTesting());

			AutoCompleteTextBox.OnPreRenderForTesting(EventArgs.Empty);

			Assert("Startup script should be registered on the page", Page.ZClientScript.IsStartupScriptRegistered(typeof(ZAutoCompleteTextBox), AutoCompleteTextBox.ClientID));
		}

		#endregion

		#region Resources

		public void TestResources()
		{
			bool observerScriptFound = false;
			bool autoCompleterScriptFound = false;
			bool autoCompleterRequestScriptFound = false;
			bool autoCompleterCSSFound = false;
			bool autoCompleterImageFound = false;

			foreach (ZWebResource resource in ((IContainResources)AutoCompleteTextBox).Resources)
			{
				if (resource == AutoCompleteTextBox.ObserverScriptForTesting)
				{
					observerScriptFound = true;
				}

				if (resource == AutoCompleteTextBox.AutoCompleterScriptForTesting)
				{
					autoCompleterScriptFound = true;
				}

				if (resource == AutoCompleteTextBox.AutoCompleterRequestScriptForTesting)
				{
					autoCompleterRequestScriptFound = true;
				}

				if (resource == AutoCompleteTextBox.AutoCompleterCSSForTesting)
				{
					autoCompleterCSSFound = true;
				}

				if (resource == AutoCompleteTextBox.AutoCompleterImageForTesting)
				{
					autoCompleterImageFound = true;
				}
			}
			Assert("Observer script file should be included in the Resources collection", observerScriptFound);
			Assert("AutoCompleter script file should be included in the Resources collection", autoCompleterScriptFound);
			Assert("AutoCompleter-Request script file should be included in the Resources collection", autoCompleterRequestScriptFound);
			Assert("AutoCompleter CSS file should be included in the Resources collection", autoCompleterCSSFound);
			Assert("AutoCompleter image file file should be included into Resources collection", autoCompleterImageFound);
		}

		#endregion

		#region TestBindTo
		public void TestBindToZString()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AssertBindTo(TestBizO.Z0_CodeInfo, (ZString)"UA", (ZString)"XYZ", (ZString)"Ukraine");
		}
		public void TestBindToZGuid()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Bon Jovi";

			AutoCompleteTextBox.Helper = new OrgContactAutoCompleteHelper(Factory) { ParentPK = org.PK };
			AssertBindTo(TestBizO.Z0_GuidInfo, contact.PK, ZGuid.NewZGuid(), contact.OC_ContactName);
		}

		void AssertBindTo(ZPropertyInfo propertyInfo, IZType key, IZType wrongKey, ZString text)
		{
			AutoCompleteTextBox.BindTo = propertyInfo.Name;

			ZPropertyAccessor.Set(TestBizO, propertyInfo.Name, key);
			AutoCompleteTextBox.Bind(TestBizO);
			AssertEquals("Control should find text by key", text, AutoCompleteTextBox.Text);

			ZPropertyAccessor.Set(TestBizO, propertyInfo.Name, wrongKey);
			AutoCompleteTextBox.Bind(TestBizO);
			AssertEquals("Expected empty text", "", AutoCompleteTextBox.Text);

			AutoCompleteTextBox.Text = text;
			AutoCompleteTextBox.HasChanges = true;
			AutoCompleteTextBox.Bind(TestBizO);
			AssertEquals("Control should update business object", key, ZPropertyAccessor.Get(TestBizO, propertyInfo.Name));
		}

		#endregion

		#region TestBindTextTo
		public void TestBindTextTo()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBox.BindTo = TestBizO.Z0_CodeInfo.Name;
			AutoCompleteTextBox.BindTextTo = TestBizO.Z0_VarCharMaxInfo.Name;

			TestBizO.Z0_VarCharMax = "Russia";
			AutoCompleteTextBox.Bind(TestBizO);
			AssertEquals("Control should show property value which is defined by BindTextTo", "Russia", AutoCompleteTextBox.Text);

			AutoCompleteTextBox.Text = "Australia";
			AutoCompleteTextBox.HasChanges = true;
			AutoCompleteTextBox.Bind(TestBizO);
			AssertEquals("Control should update business object", "AU", TestBizO.Z0_Code);
			AssertEquals("Control should update business object", "Australia", TestBizO.Z0_VarCharMax);
		}

		#endregion

		#region TestReadOnly
		public void TestReadOnlyAppearance()
		{
			AutoCompleteTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			AutoCompleteTextBox.BindTo = TestBizO.Z0_CodeInfo.Name;
			TestBizO.Z0_Code_ReadOnly = true;
			AutoCompleteTextBox.Bind(TestBizO);

			AssertEquals("Expected background-color", "transparent", AutoCompleteTextBox.Style["background-color"]);
			AssertEquals("Expected border", "none", AutoCompleteTextBox.Style["border"]);
		}

		#endregion

		#region TestMaxLength
		public void TestMaxLength()
		{
			AutoCompleteTextBox.Helper = new OrgHeaderAutoCompleteHelper(Factory);

			AutoCompleteTextBox.BindTo = TestBizO.Z0_GuidInfo.Name;
			AutoCompleteTextBox.BindTextTo = TestBizO.Z0_CodeInfo.Name;
			AutoCompleteTextBox.Bind(TestBizO);

			AssertEquals("Expected max length", 5, AutoCompleteTextBox.MaxLength);
		}

		#endregion

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZAutoCompleteTextBoxForTesting();
		}

		ZAutoCompleteTextBoxForTesting AutoCompleteTextBox
		{
			get
			{
				return Control as ZAutoCompleteTextBoxForTesting;
			}
		}

		#endregion
	}
}
