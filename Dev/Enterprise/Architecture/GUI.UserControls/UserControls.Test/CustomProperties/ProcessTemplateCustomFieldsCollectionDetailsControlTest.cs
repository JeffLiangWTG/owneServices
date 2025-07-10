using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ProcessTemplateCustomFieldsCollectionDetailsControlTest : TestCase
	{
		public void TestGetCustomPropertiesControl()
		{
			using (var testControl = new ProcessTemplateCustomFieldsCollectionDetailsControlForTest())
			using (CustomPropertiesControl customPropertiesControl = testControl.GetCustomPropertiesControl_Exposed())
			{
				AssertEquals(typeof(ProcessTemplateCustomFieldsControl), customPropertiesControl.GetType());
			}
		}

		public void TestSetNothingSetupMessageLabelText()
		{
			using (var control = new ProcessTemplateCustomFieldsCollectionDetailsControlForTest())
			{
				AssertEquals("Precondition: default text", "To make use of this tab, please setup Transport Booking Instruction custom fields in Workflow Manager.", control.CustomPropertiesControl_Exposed.NothingSetupMessageLabelText);
				control.SetNothingSetupMessageLabelText("Hello, you need to set up something!");
				AssertEquals("Hello, you need to set up something!", control.CustomPropertiesControl_Exposed.NothingSetupMessageLabelText);
			}
		}

		class ProcessTemplateCustomFieldsCollectionDetailsControlForTest : ProcessTemplateCustomFieldsCollectionDetailsControl
		{
			public CustomPropertiesControl CustomPropertiesControl_Exposed
			{
				get { return base.customPropertiesControl; }
			}

			public CustomPropertiesControl GetCustomPropertiesControl_Exposed()
			{
				return base.GetCustomPropertiesControl();
			}
		}
	}
}
