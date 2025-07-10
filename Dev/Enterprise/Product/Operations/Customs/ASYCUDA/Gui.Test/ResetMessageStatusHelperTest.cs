using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class ResetMessageStatusHelperTest : TestCaseWithFactory
	{
		public void TestResetMessageStatusManifestLevel()
		{
			AssertResetMessageStatusManifestLevel(ResetMessageStatusHelper.MessageLevel.Manifest);
		}

		public void TestResetMessageStatusBillLevel()
		{
			AssertResetMessageStatusManifestLevel(ResetMessageStatusHelper.MessageLevel.Bill);
		}

		public void TestResetMessageStatusPackLevel()
		{
			AssertResetMessageStatusManifestLevel(ResetMessageStatusHelper.MessageLevel.Pack);
		}

		public void AssertResetMessageStatusManifestLevel(ResetMessageStatusHelper.MessageLevel level)
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			if (level != ResetMessageStatusHelper.MessageLevel.Pack)
			{
				var globalManifestApplicationBusinessProvider = new List<ApplicationBusinessProvider>
				{
					ObjectFactory.Get<ApplicationBusinessProvider>("ZAManifest.ApplicationBusinessProvider_ForTesting", Array.Empty<object>())
				};
				ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", globalManifestApplicationBusinessProvider);
			}

			var manifestType = "RFM";
			if (level == ResetMessageStatusHelper.MessageLevel.Bill)
			{
				manifestType = "COH";
			}
			if (level == ResetMessageStatusHelper.MessageLevel.Pack)
			{
				countryCode = Core.Constants.CountryCodes.Singapore;
				manifestType = "MGI";
			}

			Factory.Save();

			Env.Security.GlobalManifestResetMessageStatus.IsAllowed = true;

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_OverrideFreightDefaults = true;
				header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
				header.AMA_ParentId = Factory.New<ForwardingConsol>().PK;
				header.AMA_ManifestType = manifestType;

				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BOL1234";

				var pack = bill.Packs.AddNew();
				pack.APA_GoodsDescription = "FRUIT";
				pack.APA_PackQty = 1;
				var packedItem = pack.PackedItemForTesting();

				Factory.Save();

				using (var form = new ManifestForm(header))
				{
					form.Show();
					Application.DoEvents();

					var manifestUserControl = form.FindSingle<AsycudaManifestUserControl>();
					var countryTextBox = manifestUserControl.FindSingle<ZTextBox>(control => control.Name == "MessageStatusTextBox");
					var manifestResetMessageStatus = countryTextBox.ContextMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(x => x.Text == ResetMessageStatusHelper.ResetMessageStatusText);

					var tabControl = (ZTabControl)manifestUserControl.Controls.Find("mainTabControl", true).Single();
					tabControl.SelectTab("billsAndPacksTabPage");

					var billsAndPacksTabControl = form.FindSingle<ZTabControl>(control => control.Name == "billsAndPacksTabControl");
					var packsTabPage = form.FindSingle<ZTabPage>(control => control.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					billsAndPacksTabControl.SelectedTab = packsTabPage;

					var billUserControl = form.FindSingle<AsycudaBillUserControl>(control => control.Name == "asycudaBillUserControl");
					var billTextBox = billUserControl.FindSingle<ZTextBox>(nameof(CommonBillUserControl.MessageStatusTextBox));
					var billResetMessageStatus = billTextBox.ContextMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(x => x.Text == ResetMessageStatusHelper.ResetMessageStatusText);

					var asycudaPackUserControl = form.FindSingle<AsycudaPackUserControl>(control => control.Name == "AsycudaPackUserControl");
					var packCountrySplitContainer = asycudaPackUserControl.FindSingle<CargoWise.Windows.UI.KSplitContainer>(control => control.Name == "PackCountrySplitContainer");
					var packUserControlResetMessageStatusSupporter = packCountrySplitContainer.Panel2.Controls.OfType<IResetMessageStatusSupporter>().FirstOrDefault();
					var packResetMessageStatus = packUserControlResetMessageStatusSupporter == null ? null : ((Control)packUserControlResetMessageStatusSupporter).FindSingle<ZTextBox>(nameof(CommonPackedItemDetailsUserControl.MessageStatusTextBox)).ContextMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(x => x.Text == ResetMessageStatusHelper.ResetMessageStatusText);

					AssertNotNull("Should have a manifest level Reset Message Status option", manifestResetMessageStatus);
					AssertNotNull("Should have a bill level Reset Message Status option", billResetMessageStatus);
					if (level >= ResetMessageStatusHelper.MessageLevel.Pack)
					{
						AssertNotNull("Should have a pack level Reset Message Status option", packResetMessageStatus);
					}
					else
					{
						AssertNull("Should not have a pack level Reset Message Status option", packResetMessageStatus);
					}

					IResetMessageStatusSupporter manifestUserControlResetMessageStatusSupporter = manifestUserControl;
					manifestUserControlResetMessageStatusSupporter.ResetMessageStatus_Popup();
					IResetMessageStatusSupporter billUserControlResetMessageStatusSupporter = billUserControl;
					billUserControlResetMessageStatusSupporter.ResetMessageStatus_Popup();
					packUserControlResetMessageStatusSupporter?.ResetMessageStatus_Popup();

					Assert("Should not enable the manifest level Reset Message Status option, as we have not sent ORG yet.", !manifestResetMessageStatus.Enabled);
					Assert("Should not enable the bill level Reset Message Status option, as we have not sent ORG yet.", !billResetMessageStatus.Enabled);
					if (packUserControlResetMessageStatusSupporter != null)
					{
						Assert("Should not enable the pack level Reset Message Status option, as we have not sent ORG yet.", !packResetMessageStatus.Enabled);
					}

					SetMessageStatusCanBeReset(level, header, bill, packedItem);

					manifestUserControlResetMessageStatusSupporter.ResetMessageStatus_Popup();
					billUserControlResetMessageStatusSupporter.ResetMessageStatus_Popup();
					packUserControlResetMessageStatusSupporter?.ResetMessageStatus_Popup();

					Assert("Should enable the manifest level Reset Message Status option, ORG has been sent and no response yet.", manifestResetMessageStatus.Enabled);
					AssertEquals("Should enable the bill level Reset Message Status option, ORG has been sent and no response yet.", level >= ResetMessageStatusHelper.MessageLevel.Bill, billResetMessageStatus.Enabled);
					if (packUserControlResetMessageStatusSupporter != null)
					{
						AssertEquals("Should enable the pack level Reset Message Status option, ORG has been sent and no response yet.", level >= ResetMessageStatusHelper.MessageLevel.Pack, packResetMessageStatus.Enabled);
					}

					AssertMessageStatusCanBeResetForLevel(manifestUserControlResetMessageStatusSupporter.ResetMessageStatus_Click, ResetMessageStatusHelper.MessageLevel.Manifest, header, bill, packedItem);
					if (level >= ResetMessageStatusHelper.MessageLevel.Bill)
					{
						SetMessageStatusCanBeReset(level, header, bill, packedItem);
						AssertMessageStatusCanBeResetForLevel(billUserControlResetMessageStatusSupporter.ResetMessageStatus_Click, ResetMessageStatusHelper.MessageLevel.Bill, header, bill, packedItem);
					}
					if (packUserControlResetMessageStatusSupporter != null)
					{
						SetMessageStatusCanBeReset(level, header, bill, packedItem);
						AssertMessageStatusCanBeResetForLevel(packUserControlResetMessageStatusSupporter.ResetMessageStatus_Click, ResetMessageStatusHelper.MessageLevel.Pack, header, bill, packedItem);
					}
				}
			}
		}

		static void AssertMessageStatusCanBeResetForLevel(Action resetMessageStatus_Click, ResetMessageStatusHelper.MessageLevel level, AsycudaManifestHeader header, AsycudaBill billCountry, AsycudaPackedItem packedItem)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			resetMessageStatus_Click();
			AssertContains("Are you sure you want to reset", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertMessageStatusHasBeenReset(header, billCountry, packedItem);
		}

		static void AssertMessageStatusHasBeenReset(AsycudaManifestHeader header, AsycudaBill billCountry, AsycudaPackedItem packedItem)
		{
			CombineAssertions(() =>
			{
				AssertEquals("", header.AMA_MessageStatus);
				AssertEquals("", billCountry?.ABL_MessageStatus ?? "");
				if (packedItem != null && !packedItem.IsDeleted)
				{
					AssertEquals("", packedItem.API_MessageStatus);
				}
			});
		}

		static void SetMessageStatusCanBeReset(ResetMessageStatusHelper.MessageLevel level, AsycudaManifestHeader header, AsycudaBill bill, AsycudaPackedItem packedItem)
		{
			switch (level)
			{
				case ResetMessageStatusHelper.MessageLevel.Manifest:
					header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
					break;
				case ResetMessageStatusHelper.MessageLevel.Bill:
					bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;
					break;
				case ResetMessageStatusHelper.MessageLevel.Pack:
					packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
					break;
			}
		}
	}
}
