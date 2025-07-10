using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	public class MenuBuilderGBSSTests : TestCaseWithFactory
	{
		public void TestMenuItem_GBSS_Visibility()
		{
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("GB SS Send Manifest", sendManifestMenuItem);
				var sendAmendmentMenuItem = menu.MenuItems.FindByText("Send Amendment");
				AssertNotNull("GB SS Send Amendment", sendAmendmentMenuItem);
			});
		}

		public void TestSendGBSSSendManifestMessage()
		{
			void TestSendGBSSMessageCore(AsycudaMenuForTest menu)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSUsername"))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSPasssword"))
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");

					var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
					UnitTestUserNotification.Instance.ClearMessages();
					header.AMA_MessageStatus = "";
					Factory.Save();
					sendManifestMenuItem.PerformClick();
					AssertEquals("Pass", "GB S&S Manifest Message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);

					var message = Factory.LoadTop1<IcsSsGreatBritainEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
					AssertNotNull("Created message", message);
					AssertEquals("Manifest type", ICSManifestTypes.Codes.SAS, header.AMA_ManifestType);
					AssertEquals("Message application code", "GIG", message.EM_ApplicationCode);
					AssertContains("Message type created", "CC315A</MesTypMES20>", message.EM_MessageText);
				}
			}

			CreateManifestMenuOnForm(TestSendGBSSMessageCore);
		}

		public void TestSendGBSSSendManifestMessage_MessageStatus()
		{
			CreateManifestMenuOnForm(menu =>
			{
				var menuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("Pre-requisite: find Send Manifest menu item", menuItem);

				const string sentMessage = "GB S&S Manifest Message queued for sending.";
				const string waitingMessage = "Can’t be sent. Is waiting for a prior Customs response.";

				CombineAssertions(() =>
				{
					foreach (var test in new[] {
						(string.Empty, sentMessage),
						("AWA", waitingMessage),
						("CAN", sentMessage),
						("ERR", sentMessage),
						("NOT", sentMessage),
						("UNK", sentMessage),
					})
					{
						header.AMA_MessageStatus = test.Item1;
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessages();
						menuItem.PerformClick();
						AssertEquals($"AMA_MessageStatus: '{test.Item1}'", test.Item2, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			});
		}

		public void TestSendGBSSSendAmendmentMessage()
		{
			void TestSendGBSSMessageCore(AsycudaMenuForTest menu)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSUsername"))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSPasssword"))
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");

					var sendAmendmentMenuItem = menu.MenuItems.FindByText("Send Amendment");
					UnitTestUserNotification.Instance.ClearMessages();
					header.AMA_MessageStatus = "";
					header.RegistrationNumber = "MRN123";
					Factory.Save();
					sendAmendmentMenuItem.PerformClick();
					AssertEquals("Pass", "GB S&S Amendment Message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					var message = Factory.LoadTop1<IcsSsGreatBritainEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));

					AssertNotNull("Created message", message);
					AssertEquals("Manifest type", ICSManifestTypes.Codes.SAS, header.AMA_ManifestType);
					AssertEquals("Message application code", "GIG", message.EM_ApplicationCode);
					AssertContains("Message type created", "CC313A</MesTypMES20>", message.EM_MessageText);
				}
			}

			CreateManifestMenuOnForm(TestSendGBSSMessageCore);
		}

		public void TestSendGBSSSendAmendmentMessage_NoMRN()
		{
			void TestSendGBSSMessageCore(AsycudaMenuForTest menu)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSUsername"))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSPasssword"))
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");

					var sendAmendmentMenuItem = menu.MenuItems.FindByText("Send Amendment");
					header.AMA_MessageStatus = "";
					header.RegistrationNumber = "";
					Factory.Save();
					sendAmendmentMenuItem.PerformClick();
					AssertEquals("Fail", "This manifest has no MRN and so there is nothing to amend. Do not send an amendment message.\r\n\r\nIf you are trying to resolve an initial submission failure, address the failure and then send an original message using the 'Send Manifest' option.  \r\n\r\nIf you are resolving a business rejection reported via message type 316 ('Entry Summary Declaration Rejection'), address the failures and resend a new original message.", UnitTestUserNotification.Instance.LastMessage.Text);
					var message = Factory.LoadTop1<IcsSsGreatBritainEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
					AssertNull("Message not sent", message);
				}
			}

			CreateManifestMenuOnForm(TestSendGBSSMessageCore);
		}

		void CreateManifestMenuOnForm(Action<AsycudaMenuForTest> testFunc)
		{
			using var form = new ZForm(header);
			var menu = new AsycudaMenuForTest(header);
			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			testFunc(menu);
		}

		AsycudaManifestHeaderSS header;

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			Factory.Save();
		}

		public class AsycudaMenuForTest : AsycudaMenu
		{
			public AsycudaMenuForTest(AsycudaManifestHeaderSS header)
				: base(header)
			{ }

			protected override DialogResult ShowSaveDialog(ZSaveFileDialog dialog)
			{
				return DialogResult.OK;
			}

			public new void OnPopup(EventArgs e) => base.OnPopup(e);
		}
	}
}
