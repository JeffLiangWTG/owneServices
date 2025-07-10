using System.Web.UI;
using System.Web.UI.HtmlControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class SaveLayoutPopupTest : TestCase
	{
		#region TestDisableSaving

		public void TestDisableSaving()
		{
			using (SaveLayoutPopupForTest saveLayoutPopup = new SaveLayoutPopupForTest())
			{
				saveLayoutPopup.DisableSavingForTest();
				Assert(!saveLayoutPopup.ButtonControlForTest.Disabled);
				AssertEquals(SaveLayoutPopup.DefaultButtonToolTip, saveLayoutPopup.ButtonControlForTest.Attributes[nameof(HtmlTextWriterAttribute.Title)]);

				saveLayoutPopup.Enabled = false;
				saveLayoutPopup.DisableSavingForTest();

				Assert(saveLayoutPopup.ButtonControlForTest.Disabled);
				AssertEquals(SaveLayoutPopup.SavingDisabledButtonToolTip, saveLayoutPopup.ButtonControlForTest.Attributes[nameof(HtmlTextWriterAttribute.Title)]);

				saveLayoutPopup.Enabled = true;
				saveLayoutPopup.DisableSavingForTest();

				Assert(!saveLayoutPopup.ButtonControlForTest.Disabled);
				AssertEquals(SaveLayoutPopup.DefaultButtonToolTip, saveLayoutPopup.ButtonControlForTest.Attributes[nameof(HtmlTextWriterAttribute.Title)]);
			}
		}

		#endregion

		#region Implementation

		class SaveLayoutPopupForTest : SaveLayoutPopup
		{
			public HtmlInputButton ButtonControlForTest
			{
				get
				{
					return ButtonControl;
				}
			}

			public string ButtonToolTipForTest
			{
				get
				{
					return ButtonToolTip;
				}
			}

			public void DisableSavingForTest()
			{
				DisableSaving();
			}
		}

		#endregion
	}
}
