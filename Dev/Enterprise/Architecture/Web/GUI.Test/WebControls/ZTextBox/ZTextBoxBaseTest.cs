using System;
using System.IO;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZTextBoxBaseTest : WebControlTest
	{
		#region test setup

		protected ZTextBoxBase TestTextBoxBase;

		protected abstract ZPropertyInfo BindToProperty { get; }

		protected abstract bool BindToProperty_ReadOnly { get; set; }

		protected abstract IZType TestValue { get; }
		protected abstract IZType TestValue2 { get; }
		protected abstract IZType TestValueTooLong { get; }

		protected override void SetUp()
		{
			base.SetUp();
			TestTextBoxBase = GetNewControl() as ZTextBoxBase;
			Page.Controls.Add(TestTextBoxBase);
		}

		#endregion

		public void TestPostDataChanged()
		{
			PostDataChangedHasBeenCalled = false;
			TestTextBoxBase.PostDataChanged += new EventHandler(OnPostDataChangedTest);
			Assert(!PostDataChangedHasBeenCalled);

			TestTextBoxBase.RaisePostDataChangedEventInternal();
			Assert(PostDataChangedHasBeenCalled);
		}

		bool PostDataChangedHasBeenCalled;

		void OnPostDataChangedTest(object sender, EventArgs e)
		{
			PostDataChangedHasBeenCalled = true;
		}

		public void TestSelectedValue()
		{
			BindToProperty.Value = TestValue;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			TestTextBoxBase.SelectedValueInternal = TestValue;
			AssertEquals(TestValue, TestTextBoxBase.SelectedValueInternal);
			AssertEquals("text is set to value", TestValue.ToString(), TestTextBoxBase.Text);

			TestTextBoxBase.SelectedValueInternal = null;
			AssertEquals("text is blank", "", TestTextBoxBase.Text);
		}

		public void TestHasChangesAndBind()
		{
			BindToProperty.Value = TestValue;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals(TestValue.ToString(), TestTextBoxBase.Text);
			AssertEquals(TestValue, BindToProperty.Value);

			TestTextBoxBase.SelectedValueInternal = TestValue2;
			TestTextBoxBase.HasChanges = true;
			TestTextBoxBase.Bind(TestBizO);
			AssertEquals(TestValue2.ToString(), TestTextBoxBase.Text);
			AssertEquals(TestValue2, TestTextBoxBase.SelectedValueInternal);
			AssertEquals(TestValue2, BindToProperty.Value);
		}

		public void TestReadOnlyDisplayStyle()
		{
			AssertEquals("Text box should not be read only by default", false, TestTextBoxBase.ReadOnly);

			BindToProperty.Value = TestValue;
			BindToProperty_ReadOnly = true;

			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals("Precondition: ReadOnly", true, TestTextBoxBase.ReadOnly);
			AssertEquals("Text box background", "transparent", TestTextBoxBase.Style["background-color"]);
			AssertEquals("Text box border", "none", TestTextBoxBase.Style["border"]);

			BindToProperty_ReadOnly = false;
			TestTextBoxBase.Bind(TestBizO);
			AssertEquals("Precondition: ReadOnly", false, TestTextBoxBase.ReadOnly);
			AssertNull("Text box background", TestTextBoxBase.Style["background-color"]);
			AssertNull("Text box border", TestTextBoxBase.Style["border"]);

			TestTextBoxBase.BindTo = ZString.Empty;
		}

		public void TestReadOnly_IsReadOnlyAffectingEnabledFlagAndStylesSetToFalse()
		{
			TestTextBoxBase.IsReadOnlyAffectingEnabledFlagAndStyles = false;
			AssertEquals("Text box should not be read only by default", false, TestTextBoxBase.ReadOnly);

			BindToProperty.Value = TestValue;
			BindToProperty_ReadOnly = true;

			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals("Precondition: ReadOnly", true, TestTextBoxBase.ReadOnly);
			AssertNull("Text box background - unaffected", TestTextBoxBase.Style["background-color"]);
			AssertNull("Text box border - unaffected", TestTextBoxBase.Style["border"]);

			BindToProperty_ReadOnly = false;
			TestTextBoxBase.Bind(TestBizO);
			AssertEquals("Precondition: ReadOnly", false, TestTextBoxBase.ReadOnly);
			AssertNull("Text box background - unaffected", TestTextBoxBase.Style["background-color"]);
			AssertNull("Text box border - unaffected", TestTextBoxBase.Style["border"]);

			TestTextBoxBase.BindTo = ZString.Empty;
		}

		public void TestBindSetsReadOnly()
		{
			AssertEquals("ReadOnly is false", false, TestTextBoxBase.ReadOnly);
			AssertEquals("MaxLength is 0", 0, TestTextBoxBase.MaxLength);

			BindToProperty.Value = TestValue;
			BindToProperty_ReadOnly = true;

			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals(TestValue.ToString(), TestTextBoxBase.Text);
			AssertEquals(TestValue, BindToProperty.Value);

			AssertEquals("ReadOnly is true", true, TestTextBoxBase.ReadOnly);

			BindToProperty_ReadOnly = false;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals("ReadOnly is false", false, TestTextBoxBase.ReadOnly);
		}

		public void TestCanBeEnabledByClient()
		{
			TestTextBoxBase.CanBeEnabledByClient = true;
			AssertEquals(true, TestTextBoxBase.CanBeEnabledByClient);

			TestTextBoxBase.CanBeEnabledByClient = false;
			AssertEquals(false, TestTextBoxBase.CanBeEnabledByClient);
		}

		public void TestRenderWhenReadOnlyAndCanBeEnabledByClient()
		{
			BindToProperty_ReadOnly = true;
			TestTextBoxBase.CanBeEnabledByClient = false;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			var markupWriter = new StringWriter();
			TestTextBoxBase.RenderControl(new HtmlTextWriter(markupWriter));
			AssertEquals(true, markupWriter.ToString().StartsWith("<span"));

			markupWriter = new StringWriter();
			TestTextBoxBase.CanBeEnabledByClient = true;
			TestTextBoxBase.RenderControl(new HtmlTextWriter(markupWriter));
			AssertEquals(true, markupWriter.ToString().StartsWith("<input"));
		}

		public void TestRenderWhenNoAllowEdit()
		{
			BindToProperty_ReadOnly = false;
			TestTextBoxBase.CanBeEnabledByClient = false;
			TestTextBoxBase.AllowEdit = false;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);
			TestTextBoxBase.ID = "MyControlID";

			var markupWriter = new StringWriter();
			TestTextBoxBase.RenderControl(new HtmlTextWriter(markupWriter));
			AssertEquals(true, markupWriter.ToString().StartsWith("<span"));
			AssertEquals(true, markupWriter.ToString().Contains("id=\"MyControlID\""));
		}

		public void TestBindTruncatesByMaxLength()
		{
			if (TestValueTooLong != null)
			{
				TestTextBoxBase.SelectedValueInternal = TestValueTooLong;
				TestTextBoxBase.HasChanges = true;

				TestTextBoxBase.BindTo = BindToProperty.Name;
				TestTextBoxBase.Bind(TestBizO);

				var expectedValue = TestValueTooLong.ToString().Substring(0, TestTextBoxBase.MaxLength);
				AssertEquals(expectedValue, TestTextBoxBase.SelectedValueInternal);
			}

			Assert(true);
		}

		public void TestIsBindable()
		{
			TestTextBoxBase.BindTo = "";
			AssertEquals(false, TestTextBoxBase.IsBindable(TestBizO));

			TestTextBoxBase.BindTo = "Some property";
			AssertEquals(true, TestTextBoxBase.IsBindable(TestBizO));

			AssertEquals("null DataSource, should not be bindable", false, TestTextBoxBase.IsBindable(null));
		}

		public void TestUnBind()
		{
			BindToProperty.Value = TestValue;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals(TestValue, TestTextBoxBase.SelectedValueInternal);
			AssertNotNull(TestTextBoxBase.BusinessEntity);

			TestTextBoxBase.UnBind();

			AssertEquals("SelectedValue should be set to null", "", TestTextBoxBase.Text);
			AssertEquals("BindTo should be set to null", null, TestTextBoxBase.BindTo);
			AssertEquals("property of BizO remains unchanged", TestValue, BindToProperty.Value);
			AssertNull(TestTextBoxBase.BusinessEntity);
		}

		public void TestRebindOnRaisePostDataChangedEvent()
		{
			BindToProperty.Value = TestValue;
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);
			AssertEquals(TestValue, TestTextBoxBase.SelectedValueInternal);

			BindToProperty.Value = TestValue2;
			TestTextBoxBase.RaisePostDataChangedEventInternal();
			AssertEquals("Should be rebound on RaisePostDataChangedEvent", TestValue2, TestTextBoxBase.SelectedValueInternal);
		}

		[ExpectNoExceptions]
		public void TestRaisePostDataChangedEvent_NullBusinessEntity()
		{
			TestTextBoxBase.RaisePostDataChangedEventInternal();
		}

		public void TestNotifications()
		{
			AssertEquals(0, TestTextBoxBase.Notifications.GetErrors().Count());
			AssertEquals(0, TestTextBoxBase.Notifications.GetMessageErrors().Count());
			AssertEquals(0, TestTextBoxBase.Notifications.GetWarnings().Count());

			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals(0, TestTextBoxBase.Notifications.GetErrors().Count());
			AssertEquals(0, TestTextBoxBase.Notifications.GetMessageErrors().Count());
			AssertEquals(0, TestTextBoxBase.Notifications.GetWarnings().Count());

			using (TestBizO.SuspendValidationTesting())
			{
				BindToProperty.AddError("error");

				BindToProperty.AddWarning("warning1");
				BindToProperty.AddWarning("warning2");

				BindToProperty.AddMessageError("message error1");
				BindToProperty.AddMessageError("message error2");
				BindToProperty.AddMessageError("message error3");
			}
			TestTextBoxBase.BindTo = BindToProperty.Name;
			TestTextBoxBase.Bind(TestBizO);

			AssertEquals(1, TestTextBoxBase.Notifications.GetErrors().Count());
			AssertEquals(2, TestTextBoxBase.Notifications.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(3, TestTextBoxBase.Notifications.GetMessageErrors().Count());
		}
	}
}
