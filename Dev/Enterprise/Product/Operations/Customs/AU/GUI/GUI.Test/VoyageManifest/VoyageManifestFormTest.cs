using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(VoyageManifestForm))]
	sealed class VoyageManifestFormTest : ZFormBasherTest
	{
		public void TestMessagePopsUpWhenDeletingOceanBillIsDisallowed()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			using (VoyageManifestForm form = new VoyageManifestForm(tranHead))
			{
				CusSeaManOBLHeader oceanBillToRemove = tranHead.OceanBills.AddNew();
				oceanBillToRemove.BO_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tranHead.OceanBillsView.RemoveAndDelete(oceanBillToRemove);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				CusSeaManOBLHeader oceanBillDisallowedToRemove = tranHead.OceanBills.AddNew();
				oceanBillDisallowedToRemove.BO_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tranHead.OceanBillsView.RemoveAndDelete(oceanBillDisallowedToRemove);
				AssertEquals("You should withdraw the OceanBill prior to delete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportMenu()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			using (VoyageManifestForm form = new VoyageManifestForm(tranHead))
			{
				bool voyageManifestImportMenuPresent = false;
				foreach (MenuItem item in form.Menu.MenuItems)
				{
					if (item.GetType() == typeof(VoyageManifestImportMenu))
					{
						VoyageManifestImportMenu menu = (VoyageManifestImportMenu)item;
						AssertEquals("Has our transport header", tranHead, menu.TranHead);
						voyageManifestImportMenuPresent = true;
						break;
					}
				}

				AssertEquals("VoyageManifestImportMenuPresent", true, voyageManifestImportMenuPresent);
			}
		}

		public void TestMenu()
		{
			using (VoyageManifestForm form = new VoyageManifestForm(Factory.New<CusSeaManTranHead>()))
			{
				bool voyageManifestMessagingMenuPresent = false;
				foreach (MenuItem item in form.Menu.MenuItems)
				{
					if (item.GetType() == typeof(VoyageManifestMessagingMenu))
					{
						voyageManifestMessagingMenuPresent = true;
					}
				}

				AssertEquals("VoyageManifestMessagingMenuPresent", true, voyageManifestMessagingMenuPresent);
			}
		}

		public void TestGetMessagingMenu()
		{
			using (var form = new VoyageManifestFormForTest(Factory.New<CusSeaManTranHead>()))
			using (MenuItem menu = form.GetMessagingMenuInternal())
			{
				AssertEquals("MenuType", typeof(VoyageManifestMessagingMenu), menu.GetType());
			}
		}

		public void TestGetManager()
		{
			using (VoyageManifestForm form = new VoyageManifestForm(Factory.New<CusSeaManTranHead>()))
			{
				AssertEquals("MenuType", typeof(CusSeaManTranHeadMessageManager), form.Manager.GetType());
			}
		}

		public void TestFormCaption()
		{
			using (VoyageManifestForm form = new VoyageManifestForm(Factory.New<CusSeaManTranHead>()))
			{
				AssertEquals("Customs Import Manifest", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore() => new VoyageManifestForm(Factory.New<CusSeaManTranHead>());

		sealed class VoyageManifestFormForTest : VoyageManifestForm
		{
			public VoyageManifestFormForTest(CusSeaManTranHead tranHead)
				: base(tranHead)
			{
			}

			internal MenuItem GetMessagingMenuInternal() => GetMessagingMenu();
		}
	}
}
