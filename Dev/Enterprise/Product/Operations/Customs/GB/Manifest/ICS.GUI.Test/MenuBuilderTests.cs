using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	public class MenuBuilderTests : TestCaseWithFactory
	{
		public void TestMenuItem_GBICS_Visibility()
		{
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("GB ICS Send Manifest", sendManifestMenuItem);
				var sendAmendmentMenuItem = menu.MenuItems.FindByText("Send Amendment");
				AssertNull("GB ICS Send Amendment", sendAmendmentMenuItem);
			});
		}

		public void TestSendICSSendMessage()
		{
			void TestSendICSSendMessageCore(AsycudaMenuForTest menu)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSUsername"))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSPasssword"))
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");

					var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
					header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting;
					Factory.Save();
					sendManifestMenuItem.PerformClick();
					AssertEquals("Awaiting", "Can’t be sent. Is waiting for a prior Customs response.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("No message should be created.", Factory.LoadTop1<IcsNorthernIrelandEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK)));

					UnitTestUserNotification.Instance.ClearMessages();
					header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Cancel;
					Factory.Save();
					sendManifestMenuItem.PerformClick();
					AssertEquals("Send from CAN", "ICS Manifest Message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					var messages = Factory.Load<IcsNorthernIrelandEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
					AssertEquals("Message created from CAN", 1, messages.Length);

					UnitTestUserNotification.Instance.ClearMessages();
					header.AMA_MessageStatus = "";
					Factory.Save();
					sendManifestMenuItem.PerformClick();
					AssertEquals("Send from blank status", "ICS Manifest Message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					messages = Factory.Load<IcsNorthernIrelandEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
					AssertEquals("Message created from blank status", 2, messages.Length);

					var message = messages[1];
					AssertEquals("Manifest type", EU.Manifest.Business.EUManifestTypes.Codes.ICS, header.AMA_ManifestType);
					AssertEquals("Message application code", "GIN", message.EM_ApplicationCode);
					AssertContains("Message type created", "<ie:MesTypMES20>CC315A</ie:MesTypMES20>", message.EM_MessageText);
				}
			}

			CreateManifestMenuOnForm(TestSendICSSendMessageCore);
		}

		#region Implementation

		void CreateManifestMenuOnForm(Action<AsycudaMenuForTest> testFunc)
		{
			using (var form = new ZForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				testFunc(menu);
			}
		}

		AsycudaManifestHeader header;

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = EU.Manifest.Business.EUManifestTypes.Codes.ICS;
			Factory.Save();
		}

		public class AsycudaMenuForTest : AsycudaMenu
		{
			public AsycudaMenuForTest(AsycudaManifestHeader header)
				: base(header)
			{ }

			protected override DialogResult ShowSaveDialog(ZSaveFileDialog dialog)
			{
				return DialogResult.OK;
			}

			public new void OnPopup(EventArgs e) => base.OnPopup(e);
		}
		#endregion

	}
}
