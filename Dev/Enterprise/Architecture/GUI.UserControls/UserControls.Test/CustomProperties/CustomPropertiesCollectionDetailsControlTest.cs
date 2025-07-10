using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CustomPropertiesCollectionDetailsControlTest : TestCase
	{
		public void TestSetNothingSetupMessageLabelText()
		{
			using (var control = new CustomPropertiesCollectionDetailsControlForTest())
			{
				control.SetNothingSetupMessageLabelText("Hello, you need to set up something!");
				AssertEquals("Hello, you need to set up something!", control.CustomPropertiesControl_Exposed.NothingSetupMessageLabelText);
			}
		}

		class CustomPropertiesCollectionDetailsControlForTest : CustomPropertiesCollectionDetailsControl
		{
			public CustomPropertiesControl CustomPropertiesControl_Exposed
			{
				get { return base.customPropertiesControl; }
			}
		}
	}
}
