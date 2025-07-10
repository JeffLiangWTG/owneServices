using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCTOForm))]
	sealed class AirCTOFormTest : ZFormBasherTest
	{
		public void TestMenu()
		{
			using (AirCTOForm form = (AirCTOForm)GetFormToBashCore())
			{
				bool messagingMenuPresent = false;
				foreach (MenuItem item in form.Menu.MenuItems)
				{
					if (item.GetType() == typeof(AirCTOMenu))
					{
						messagingMenuPresent = true;
					}
				}

				AssertEquals("MessagingMenuPresent ", true, messagingMenuPresent);
			}
		}

		public void TestGetManager()
		{
			using (var form = (AirCTOForm)GetFormToBashCore())
			{
				AssertType<CTOCusMAWBMessageManager>(form.Manager);
			}
		}

		public void TestGetMessagingMenu()
		{
			using (var form = (AirCTOForm)GetFormToBashCore())
			{
				AssertType<AirCTOMenu>(form.Menu.MenuItems.FindByText("Messaging"));
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			AirCTOForm form = new AirCTOForm(mAWB);
			return form;
		}
		#endregion
	}
}
