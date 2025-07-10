using System.Collections.Specialized;
using System.Web.UI;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCheckBoxTest : WebControlTest
	{
		public void TestOneTimeLoadPostData()
		{
			var postBackDataHandler = CheckBox as IPostBackDataHandler;
			AssertNotNull(postBackDataHandler);

			CheckBox.Checked = false;

			var collectionWhereCheckboxIsChecked = new NameValueCollection();
			collectionWhereCheckboxIsChecked.Add(CheckBox.UniqueID, "on");

			postBackDataHandler.LoadPostData(CheckBox.UniqueID, collectionWhereCheckboxIsChecked);
			Assert("Checkbox should have changes", CheckBox.HasChanges);
			Assert("Checkbox should be checked", CheckBox.Checked);

			postBackDataHandler.LoadPostData(CheckBox.UniqueID, collectionWhereCheckboxIsChecked);
			Assert("Repeated loading of post data should not affect Checkbox state", CheckBox.HasChanges);
		}

		public void TestBind()
		{
			Assert("Pre-condition", !CheckBox.Checked);
			AssertNull("Pre-condition", CheckBox.Info);

			TestBizO.Z0_Bool = true;
			CheckBox.Bind(TestBizO);
			Assert("Should be bound to TestBizO.Z0_Bool", CheckBox.Checked);
			AssertEquals("Should be assigned when binding", TestBizO, CheckBox.BusinessEntity);
			AssertEquals("Should be assigned when binding", TestBizO.Z0_BoolInfo, CheckBox.Info);

			CheckBox.Checked = false;
			CheckBox.HasChanges = true;
			CheckBox.Bind(TestBizO);
			Assert("Should be bound to TestBizO.Z0_Bool", !CheckBox.Checked);
			Assert("Should be bound to TestBizO.Z0_Bool", !TestBizO.Z0_Bool);
			Assert("HasChanges should be set to false", !CheckBox.HasChanges);
		}

		public void TestUnbind()
		{
			CheckBox.BusinessEntity = TestBizO;
			CheckBox.UnBind();
			AssertNull("Should be set to null", CheckBox.BusinessEntity);
			AssertEquals("Should be set to empty string", "", CheckBox.BindTo);
		}

		public void TestRebindOnRaisePostDataChangedEvent()
		{
			TestBizO.Z0_Bool = true;
			CheckBox.Bind(TestBizO);
			Assert("Should be bound to TestBizO.Z0_Bool", CheckBox.Checked);

			TestBizO.Z0_Bool = false;
			CheckBox.RaisePostDataChangedEventForTesting();
			Assert("Should be rebound on RaisePostDataChangedEvent", !CheckBox.Checked);
		}

		[ExpectNoExceptions]
		public void TestRaisePostDataChangedEvent_NullBusinessEntity()
		{
			CheckBox.RaisePostDataChangedEventForTesting();
		}

		protected override Control GetNewControl()
		{
			var result = new ZCheckBoxForTest();
			result.BindTo = DummyBizoSchema.Constants.Z0_Bool;
			return result;
		}

		ZCheckBoxForTest CheckBox
		{
			get { return (ZCheckBoxForTest)Control; }
		}

		class ZCheckBoxForTest : ZCheckBox
		{
			#region Test Properties

			public void RaisePostDataChangedEventForTesting() => RaisePostDataChangedEvent();

			#endregion
		}
	}
}
