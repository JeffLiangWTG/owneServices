using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFindBoxTest : ZTextIFramePopupTest
	{
		protected override string ExpectedPopupID
		{
			get { return String.Format("{0}_{1}", base.ExpectedPopupID, FindBox.ModuleID); }
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "ZFilterPage.aspx"; }
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZFindBox/"; }
		}

		protected override Type ExpectedIFrameSourcePageContainerType
		{
			get { return typeof(ZFindBox); }
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			ZDateTime dummyDate = new ZDateTime(2004, 12, 2);
			FindBox.SelectedValue = dummyDate;
			AssertEquals(dummyDate.ToString(), FindBox.SelectedValue);
		}

		public override void TestPopupDimension()
		{
			AssertEquals("Width", Unit.Pixel(700), FindBox.PopupWidthInternal);
			AssertEquals("Height", Unit.Pixel(355), FindBox.PopupHeightInternal);
		}

		public void TestModuleID()
		{
			FindBox.ModuleID = WebModuleIDs.Dummy;
			AssertEquals(WebModuleIDs.Dummy, FindBox.ModuleID);
		}

		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ZFindBox.ModuleIDQuery, FindBox.ModuleID.ToString());
				return result;
			}
		}

		public void TestMaxLength()
		{
			FindBox.MaxLength = 5;
			AssertEquals("MaxLength", 5, FindBox.MaxLength);
			AssertEquals("MaxLength", 5, FindBox.TextBoxControl.MaxLength);
			FindBox.MaxLength = 10;
			AssertEquals("MaxLength", 10, FindBox.MaxLength);
			AssertEquals("MaxLength", 10, FindBox.TextBoxControl.MaxLength);
		}

		public void TestBindToList()
		{
			AssertEquals("PreCondition: BindToList", "", FindBox.BindToList);
			FindBox.BindToList = "TestBindToList";
			AssertEquals("BindToList", "TestBindToList", FindBox.BindToList);
		}

		public void TestBindList()
		{
			DummyBusinessObject dummy1 = DummyBusinessObject.New(Factory);
			dummy1.Z0_Code = "ABC";
			DummyBusinessObject dummy2 = DummyBusinessObject.New(Factory);
			dummy2.Z0_Code = "XYZ";
			TestBizO.Z0_Code = "123";
			AssertNull("PreCondition: List should be null before binding", FindBox.List);
			FindBox.BindToList = "Lookups.DummyList";
			FindBox.BindTo = DummyBusinessObject.Schema.Z0_Code;
			FindBox.Bind(TestBizO);
			AssertNotNull("List", FindBox.List);
			AssertEquals("List should contain Dummy1", dummy1.PK, FindBox.List.PrimaryKeyFromCode("ABC"));
			AssertEquals("List should contain Dummy2", dummy2.PK, FindBox.List.PrimaryKeyFromCode("XYZ"));
			AssertEquals("List should contain TestBizO", TestBizO.PK, FindBox.List.PrimaryKeyFromCode("123"));
		}

		public void TestShouldSerializeModuleID()
		{
			FindBox.ModuleID = WebModuleIDs.NotAssigned;
			AssertEquals(WebModuleIDs.NotAssigned, FindBox.ModuleID);
			Assert("ShouldSerialModuleID should be false for NotAssignedWeb", !FindBox.ShouldSerializeModuleID());
			FindBox.ModuleID = WebModuleIDs.Dummy;
			AssertEquals(WebModuleIDs.Dummy, FindBox.ModuleID);
			Assert("ShouldSerializeModuleID should be true for assigned modules", FindBox.ShouldSerializeModuleID());
		}

		public void TestBeforeBind()
		{
			FindBox.BindToList = "Lookups.DummyList";
			AssertNull("PreCondition: List should be null BeforeBind", FindBox.List);
			FindBox.BindTo = DummyBusinessObject.Schema.Z0_VarCharMax;
			FindBox.Bind(TestBizO);
			AssertNotNull("BeforeBind should BindList", FindBox.List);
			AssertSame("List", TestBizO.Lookups.DummyList, FindBox.List);
		}

		public void TestTextBoxIsUpperCase()
		{
			FindBox.CreateChildControlsInternal();
			AssertEquals(CharacterCasing.Upper, FindBox.TextBoxControl.CharacterCasing);
		}

		public virtual void TestGetSelectedValueWithFindNeares()
		{
			if (FindBox.SelectedValue is ZString)
			{
				FindBox.List = new RefUNLOCOCollection(Factory);
				FindBox.CreateChildControlsInternal();

				FindBox.TextBoxControl.Text = "SYD";
				AssertEquals("Completed by RefUNLOCOCollection", "AUSYD", FindBox.SelectedValue);
			}
			else
			{
				Assert("Nothing to test", true);
			}
		}

		#region Implementation

		protected ZFindBox FindBox
		{
			get { return (ZFindBox)Control; }
		}

		protected override Control GetNewControl()
		{
			ZFindBox result = new ZFindBox();
			result.ModuleID = WebModuleIDs.Organisation;
			return result;
		}

		protected override Type ResourceContainerType
		{
			get { return typeof(ZFindBox); }
		}

		protected override string[] ExpectedResourceNames
		{
			get { return new string[] { "ZFilterPage.aspx" }; }
		}

		#endregion
	}
}
