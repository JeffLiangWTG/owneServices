using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZRadioButtonTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			ZRadioButton result = new ZRadioButton();
			result.BindTo = DummyBizoSchema.Constants.Z0_Bool;
			return result;
		}

		public void TestHasChanges()
		{
			ZRadioButton radio1 = GetNewControl() as ZRadioButton;
			ZRadioButton radio2 = GetNewControl() as ZRadioButton;
			ZRadioButton radio3 = GetNewControl() as ZRadioButton;

			ContentPlaceHolder holder = new ContentPlaceHolder();
			radio1.ID = "MrRadio";
			radio2.ID = "MrsRadio";
			radio3.ID = "MsRadio";

			radio1.GroupName = radio2.GroupName = radio3.GroupName = "Salutation";

			radio1.Checked = true;

			holder.Controls.Add(radio1);
			holder.Controls.Add(radio2);
			holder.Controls.Add(radio3);

			Page.FormControl.Controls.Add(holder);

			Page.PrepareForRendering();
			Page.OnPreRenderForTesting();

			AssertEquals("Radio1 HasChanges should be false", false, radio1.HasChanges);
			AssertEquals("Radio2 HasChanges should be false", false, radio2.HasChanges);
			AssertEquals("Radio3 HasChanges should be false", false, radio3.HasChanges);

			AssertEquals("Radio1 should be checked", true, radio1.Checked);
			AssertEquals("Radio2 should be unchecked", false, radio2.Checked);
			AssertEquals("Radio3 should be unchecked", false, radio3.Checked);

			AssertEquals("radio1.ClientID", "ctl02_MrRadio", radio1.ClientID);
			AssertEquals("radio2.ClientID", "ctl02_MrsRadio", radio2.ClientID);
			AssertEquals("radio3.ClientID", "ctl02_MsRadio", radio3.ClientID);

			AssertEquals("radio1.UniqueGroupName", "ctl02$Salutation", radio1.UniqueGroupName);
			AssertEquals("radio2.UniqueGroupName", "ctl02$Salutation", radio2.UniqueGroupName);
			AssertEquals("radio3.UniqueGroupName", "ctl02$Salutation", radio3.UniqueGroupName);

			NameValueCollection postData = new NameValueCollection();
			postData.Add("ctl02$Salutation", "MrsRadio");

			radio1.ProcessPostData(radio1.ClientID, postData);
			radio2.ProcessPostData(radio1.ClientID, postData);
			radio3.ProcessPostData(radio1.ClientID, postData);

			AssertEquals("Radio1 should be unchecked", false, radio1.Checked);
			AssertEquals("Radio2 should be checked", true, radio2.Checked);
			AssertEquals("Radio3 should be unchecked", false, radio3.Checked);

			AssertEquals("Radio1 HasChanges should be false", true, radio1.HasChanges);
			AssertEquals("Radio2 HasChanges should be false", true, radio2.HasChanges);
			AssertEquals("Radio3 HasChanges should be false", false, radio3.HasChanges);

			radio1.HasChanges = false;
			radio2.HasChanges = false;
			radio3.HasChanges = false;

			postData["ctl02$Salutation"] = "MsRadio";

			radio1.ProcessPostData(radio1.ClientID, postData);
			radio2.ProcessPostData(radio1.ClientID, postData);
			radio3.ProcessPostData(radio1.ClientID, postData);

			AssertEquals("Radio1 should be unchecked", false, radio1.Checked);
			AssertEquals("Radio2 should be unchecked", false, radio2.Checked);
			AssertEquals("Radio3 should be checked", true, radio3.Checked);

			AssertEquals("Radio1 HasChanges should be false", false, radio1.HasChanges);
			AssertEquals("Radio2 HasChanges should be false", true, radio2.HasChanges);
			AssertEquals("Radio3 HasChanges should be false", true, radio3.HasChanges);
		}

		public void TestBind()
		{
			Assert("Pre-condition", !RadioButton.Checked);
			AssertNull(RadioButton.Info);

			TestBizO.Z0_Bool = true;
			RadioButton.Bind(TestBizO);
			Assert("Should be bound to TestBizO.Z0_Bool", RadioButton.Checked);
			AssertEquals("Should be assigned when binding", TestBizO, RadioButton.BusinessEntity);
			AssertEquals("Should be assigned when binding", TestBizO.Z0_BoolInfo, RadioButton.Info);

			RadioButton.Checked = false;
			RadioButton.HasChanges = true;
			RadioButton.Bind(TestBizO);
			Assert("Should be bound to TestBizO.Z0_Bool", !RadioButton.Checked);
			Assert("Should be bound to TestBizO.Z0_Bool", !TestBizO.Z0_Bool);
			Assert("HasChanges should be set to false", !RadioButton.HasChanges);
		}

		public void TestUnbind()
		{
			RadioButton.BusinessEntity = TestBizO;
			RadioButton.UnBind();
			AssertNull("Should be set to null", RadioButton.BusinessEntity);
			AssertEquals("Should be set to empty string", "", RadioButton.BindTo);
		}

		public void TestRebindPostDataChanged()
		{
			DummyBusinessObject testBizO1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject testBizO2 = Factory.New<DummyBusinessObject>();
			ZRadioButton radio1 = new ZRadioButton();
			radio1.ID = "R1";
			ZRadioButton radio2 = new ZRadioButton();
			radio2.ID = "R2";
			radio1.GroupName = radio2.GroupName = "MEH";
			radio1.BindTo = radio2.BindTo = DummyBizoSchema.Constants.Z0_Bool;

			testBizO1.Z0_Bool = true;
			testBizO2.Z0_Bool = false;
			radio1.Bind(testBizO1);
			radio2.Bind(testBizO2);
			Assert("Should be bound", radio1.Checked);
			Assert("Should be bound", !radio2.Checked);

			NameValueCollection postData = new NameValueCollection();
			postData[radio2.UniqueGroupName] = "R2";
			radio1.ProcessPostData(radio1.ClientID, postData);
			radio2.ProcessPostData(radio2.ClientID, postData);
			Assert("Should be rebound", !radio1.Checked);
			Assert("Should be rebound", radio2.Checked);
			Assert("Should be rebound", !testBizO1.Z0_Bool);
			Assert("Should be rebound", testBizO2.Z0_Bool);
		}

		ZRadioButton RadioButton
		{
			get { return (ZRadioButton)Control; }
		}
	}
}
