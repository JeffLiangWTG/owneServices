using System.Collections.Specialized;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZGuidFindBoxTest : ZFindBoxTest
	{
		public override void TestAssignSelectedValueToInvalidZType()
		{
			DummyBusinessObject dummy1 = GetNewDummy("ABC");
			DummyBusinessObject dummy2 = GetNewDummy("XYZ");
			FindBox.BindToList = "Lookups.DummyList";
			FindBox.BindTo = "Z0_Guid";
			FindBox.Bind(TestBizO);
			ZDateTime badIZType = ZDateTime.Now;
			FindBox.SelectedValue = badIZType;
			AssertEquals("Selected Value", ZGuid.Invalid, FindBox.SelectedValue);
			AssertEquals("Text", badIZType.ToString(), FindBox.TextBoxControl.Text);
		}

		public void TestGetSelectedValue()
		{
			DummyBusinessObject dummy1 = GetNewDummy("ABC");
			DummyBusinessObject dummy2 = GetNewDummy("XYZ");
			FindBox.BindToList = "Lookups.DummyList";
			FindBox.BindTo = "Z0_Guid";
			NameValueCollection postData = new NameValueCollection();
			postData.Add("TextControl.Text", "ABC");
			FindBox.LoadPostData("TextControl.Text", postData);
			FindBox.Bind(TestBizO);
			AssertEquals("SelectedValue", dummy1.PK, FindBox.GetSelectedValueInternal());
			AssertEquals("SelectedValue", dummy1.PK, FindBox.SelectedValue);
			AssertEquals("GuidContainerValue", ZGuid.Empty.ToString(), FindBox.GuidContainerValueInternal);

			postData = new NameValueCollection();
			postData.Add("TextControl.Text", "");
			FindBox.GuidForTest = dummy2.PK;
			FindBox.LoadPostData("TextControl.Text", postData);
			FindBox.Bind(TestBizO);
			AssertEquals("TextBoxControl", "", FindBox.TextBoxControl.Text);
			AssertEquals("SelectedValue", ZGuid.Empty, FindBox.GetSelectedValueInternal());
			AssertEquals("SelectedValue", ZGuid.Empty, FindBox.SelectedValue);
		}

		public void TestGetTextFromValue()
		{
			DummyBusinessObject dummy1 = GetNewDummy("ABC");
			DummyBusinessObject dummy2 = GetNewDummy("XYZ");
			FindBox.BindToList = "Lookups.DummyList";
			FindBox.BindTo = "Z0_Guid";
			FindBox.Bind(TestBizO);
			FindBox.SelectedValue = dummy1.PK;
			AssertEquals("TextFromValue", "ABC", FindBox.GetTextFromValueInternal(dummy1.PK));
			AssertEquals("TextBoxControl", "ABC", FindBox.TextBoxControl.Text);
			FindBox.SelectedValue = dummy2.PK;
			AssertEquals("TextFromValue", "XYZ", FindBox.GetTextFromValueInternal(dummy2.PK));
			AssertEquals("TextBoxControl", "XYZ", FindBox.TextBoxControl.Text);
		}

		public void TestInvalidGuidUpdatesTextBox()
		{
			DummyBusinessObject dummy1 = GetNewDummy("ABC");
			DummyBusinessObject dummy2 = GetNewDummy("XYZ");
			FindBox.BindToList = "Lookups.DummyList";
			FindBox.BindTo = "Z0_Guid";
			NameValueCollection postData = new NameValueCollection();
			postData.Add("TextControl.Text", "ABC");
			FindBox.LoadPostData("TextControl.Text", postData);
			FindBox.Bind(TestBizO);
			AssertEquals("SelectedValue", dummy1.PK, FindBox.GetSelectedValueInternal());
			AssertEquals("SelectedValue", dummy1.PK, FindBox.SelectedValue);

			FindBox.SelectedValue = ZGuid.Invalid;
			AssertEquals("TextBoxControl", "Invalid", FindBox.TextBoxControl.Text);
			AssertEquals("SelectedValue", ZGuid.Invalid, FindBox.SelectedValue);

			FindBox.SelectedValue = ZGuid.Empty;
			AssertEquals("TextBoxControl", "", FindBox.TextBoxControl.Text);
			AssertEquals("SelectedValue", ZGuid.Empty, FindBox.SelectedValue);
		}

		public void TestGuidContainerID()
		{
			FindBox.ID = "Test";
			AssertEquals("ClentID", "Test", FindBox.ClientID);
			AssertEquals("GuidContainerID", "GuidContainerTest", FindBox.GuidContainerID);
		}

		public void TestGuidContainerValue()
		{
			TestGuidContainerID();
			FindBox.GuidForTest = ZGuid.NewZGuid();
			AssertEquals("GuidContainerValue", FindBox.GuidForTest.ToString(), FindBox.GuidContainerValueInternal);
		}

		[HttpContextEnabledTest]
		public override void TestClickHandlerAssignment()
		{
			AssertEquals(string.Format("ZTextPopup_ShowGuidPopup('ctl01_TextBox', 'ctl01_ctl00', '{0}', 'GuidContainerctl01', '{1}');", ExpectedPopupID, ExpectedIFrameSourceString) + FindBox.AdditionalButtonClickHandlerInternal, FindBox.ButtonClickHandlerInternal);
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZFindBox/"; }
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "ZFilterPage.aspx"; }
		}

		protected override string ExpectedOKFunctionName
		{
			get { return "ZTextPopup_SetValueAndGuidThenHidePopup"; }
		}

		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ZGuidFindBox.GuidContainerIDQuery, FindBox.GuidContainerID);
				return result;
			}
		}

		protected new ZGuidFindBoxForTest FindBox
		{
			get
			{
				return Control as ZGuidFindBoxForTest;
			}
		}

		protected override Control GetNewControl()
		{
			return new ZGuidFindBoxForTest();
		}

		DummyBusinessObject GetNewDummy(string code)
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = code;
			return dummy;
		}

		protected class ZGuidFindBoxForTest : ZGuidFindBox
		{
			protected override string GuidContainerValue
			{
				get { return GuidForTest.ToString(); }
			}

			public ZGuid GuidForTest;
		}
	}
}
