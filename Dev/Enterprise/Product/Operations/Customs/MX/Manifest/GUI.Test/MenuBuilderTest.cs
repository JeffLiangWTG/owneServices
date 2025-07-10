using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.MX.Manifest.Business.MXCustomsDataRegistry;

[assembly: UsesConstants(typeof(MXMessagingConstants))]

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		#region Air Manifest

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestSendAirManifest()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();
			CreateAndPopulatePack(header.Bills[0]);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Gross Weight: Weight of packages (4000) is not equal to manifest Gross Weight (20) in (KG)
Manifest UQ: You have not entered a Manifest UQ.
Volume (on Bill): Volume of packages (0) is not equal to manifest Volume (25) in (CC)
Customs Office: The code you have selected is not in the list.
Shipping Agent: You have not entered a Shipping Agent.
Transport Mode: The code you have selected is not in the list.

Do you want to send the message(s) despite these errors?

", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (BillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertEquals("Message sent successfully  HOUSELIGADAMASTER0001" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.SNT, header.Bills[0].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestDoNotSendAirManifestWithoutCredentials()
		{
			PopulateManifestHeader(false, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestCancelAirManifestOneBillSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();
			CreateAndPopulatePack(header.Bills[0]);

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";
			header.Bills[0].ABL_MessageStatus = "ACP";
			header.Bills[1].ABL_MessageStatus = "ACP";

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);

			AssertEquals("ACP", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestCancelAirManifestTwoBillsSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();
			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[1]);

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);
			AssertMessage(header.Bills[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);
			AssertEquals("AWA", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestAmendAirManifestOneBillSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();
			CreateAndPopulatePack(header.Bills[0]);

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";
			header.Bills[0].ABL_MessageStatus = "ACP";
			header.Bills[1].ABL_MessageStatus = "ACP";

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);

			AssertEquals("ACP", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestAmendAirManifestTwoBillsSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Air);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();
			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[1]);

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);
			AssertMessage(header.Bills[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXE);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);
			AssertEquals("AWA", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		#endregion

		#region Sea Manifest

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestSendSeaManifest()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200.3;
			refcontainer.RC_Width = 6000.00;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1051", "", "IMO").First().PK;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Customs Discharge Port: The code entered is expected to match a ""Local Code"" on the Port Of Discharge UNLOCO record.
Customs Load Port: The code you have selected is not in the list.
Gross Weight: Weight of packages (4000) is not equal to manifest Gross Weight (20) in (KG)
Manifest UQ: You have not entered a Manifest UQ.
Volume (on Bill): Volume of packages (0) is not equal to manifest Volume (25) in (CC)
Customs Office: The code you have selected is not in the list.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (BillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertEquals("Message sent successfully  HOUSELIGADAMASTER0001" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.SNT, header.Bills[0].ABL_BillStatus);
		}

		public void TestDoNotSendSeaManifestWithoutCredentials()
		{
			PopulateManifestHeader(false, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestCancelSeaManifestOneBillSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";
			header.Bills[0].ABL_MessageStatus = "ACP";
			header.Bills[1].ABL_MessageStatus = "ACP";

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200;
			refcontainer.RC_Width = 6000;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1051", "", "IMO").First().PK;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "03";
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);

			AssertEquals("ACP", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestCancelSeaManifestTwoBillsSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200;
			refcontainer.RC_Width = 6000;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1051", "", "IMO").First().PK;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "03";
				});

				menu.MenuItems.FindByText("Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);
			AssertMessage(header.Bills[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);
			AssertEquals("AWA", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestAmendSeaManifestOneBillSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";
			header.Bills[0].ABL_MessageStatus = "ACP";
			header.Bills[1].ABL_MessageStatus = "ACP";

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200;
			refcontainer.RC_Width = 6000;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1051", "", "IMO").First().PK;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "03";
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);

			AssertEquals("ACP", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestAmendSeaManifestTwoBillsSelected()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200;
			refcontainer.RC_Width = 6000;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1051", "", "IMO").First().PK;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (BillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "03";
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);
			AssertMessage(header.Bills[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);

			AssertEquals("AWA", header.Bills[0].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[0].ABL_BillStatus);
			AssertEquals("AWA", header.Bills[1].ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.ACP, header.Bills[1].ABL_BillStatus);
		}

		[TestDate(2021, 01, 01, 13, 00, 00)]
		public void TestSendSeaManifestDontIncludeAH1IfDangerousNotEntered()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "";
			refcontainer.RC_Length = 1200.3;
			refcontainer.RC_Width = 6000.00;
			refcontainer.RC_Height = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "LCL";
			container.ACN_RC_ContainerType = refcontainer.PK;

			var pack = CreateAndPopulatePack(header.Bills[0]);
			pack.ContainerPK = container.PK;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (BillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
				}
			}

			AssertMessage(header.Bills[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword, MessageTypes.Codes.MXA);
			AssertNotContains("<AH1>", header.Bills[0].Messages[0].EM_MessageText);
		}

		public void TestDoNotSendManifestWhenCaatWrongLength()
		{
			PopulateManifestHeader(true, Core.Constants.TransportModes.Sea);
			CreateAndPopulateHouseBill();

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "99921";

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("The CAAT of the transmitter must be an alphanumeric value between 2 and 4 characters.You can check the CAAT from Maintain -> User Admin -> Companies. Picking the company, Customs Registration Number field.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "9992";

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertNotContains("The CAAT of the transmitter must be an alphanumeric value between 2 and 4 characters.You can check the CAAT from Maintain -> User Admin -> Companies. Picking the company, Customs Registration Number field.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (BillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
				}
			}

			AssertEquals(1, header.Messages.Count);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			var testItem = new MXWsVucem(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.AirModeWSUsername = "ADMINVUCEM1";
			testItem.AirModeWSPassword = "9974567891";
			testItem.SeaModeWSResponse = "http://54.183.24.126:7001/manifiestoMaritimoRespuestaMock/services/ManifiestoMaritimo355SO?WSDL";
			testItem.SeaModeWSUsername = "ADMINVUCEM13";
			testItem.SeaModeWSPassword = "9974567890";

			Instance.WSVucem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testItem);
			Instance.MXTestingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "9992";

			Factory.Save();
		}

		void PopulateManifestHeader(ZBool setCredentials, ZString transportMode)
		{
			if (setCredentials)
			{
				SetCredencials();
			}

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "address";

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "1245";
			orgCusCode.OK_RN_NKCodeCountry = "MX";

			header.AMA_ManifestType = "MAN";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_LloydsNumber = "VCODE";
			header.AMA_VesselName = "VESSELNUMBER";
			header.AMA_Voyage = "VOYAGE";
			header.AMA_MasterBill = "MANIFESTNUMBER";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_CustomsDischargePort = "35701";
			header.AMA_CustomsLoadPort = "30505";
			header.AMA_RL_NKPortOfLoading = "MXACA";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_E_ARV = new ZDate(2019, 10, 31);
			header.AMA_E_DEP = new ZDate(2019, 11, 12);
			header.LastForeignPort = "31200";
			header.AMA_CustomsOffice = "0801";
			header.AMA_TransportMode = transportMode;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_MasterBillIssueDate = ZDate.Today;
			header.AMA_CarrierCode = "CARRIERCODE";
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "MX";
			org.OH_FullName = "MADERA SA de CV";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN891228BE2");

			orgAddress.OA_Address1 = "AV REMEDIOS";
			orgAddress.OA_Address2 = "ARTEGA Y TINOCO";
			orgAddress.OA_City = "CIUDAD DE MEXICO";
			orgAddress.OA_State = "CMX";
			orgAddress.OA_PostCode = "06700";
			orgAddress.OA_RN_NKCountryCode = "MX";
			orgAddress.OA_Phone = "5542763477";
			orgAddress.OA_Email = "TANIA@GMAIL.COM";

			bill.ABL_OA_Consignee = orgAddress.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "MX";
			org2.OH_FullName = "LALA";
			var orgAddress2 = org2.MainAddress;
			org2.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN8912251");

			orgAddress2.OA_Address1 = "AV PLAZA";
			orgAddress2.OA_Address2 = "ARTEGA Y MEXICO";
			orgAddress2.OA_City = "CIUDAD DE MEXICO";
			orgAddress2.OA_State = "CMX";
			orgAddress2.OA_PostCode = "06700";
			orgAddress2.OA_RN_NKCountryCode = "MX";
			orgAddress2.OA_Phone = "4442763477";
			orgAddress2.OA_Email = "LALA@GMAIL.COM";

			bill.ABL_OA_Shipper = orgAddress2.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_RL_NKClosestPort = "MX";
			org3.OH_FullName = "LORENA";
			var orgAddress3 = org3.MainAddress;
			org3.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN89122");

			orgAddress3.OA_Address1 = "AV ARTEAGA";
			orgAddress3.OA_Address2 = "PLAZA Y TINOCO";
			orgAddress3.OA_City = "CIUDAD DE MEXICO";
			orgAddress3.OA_State = "CMX";
			orgAddress3.OA_PostCode = "06700";
			orgAddress3.OA_RN_NKCountryCode = "MX";
			orgAddress3.OA_Phone = "55427634";
			orgAddress3.OA_Email = "LORENA@GMAIL.COM";

			bill.ABL_OA_NotifyParty = orgAddress3.PK;

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_ManifestQty = 400;
			bill.ABL_GrossWeight = 20;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 25;
			bill.ABL_VolumeUQ = "CC";
			bill.ABL_RL_NKOrigin = "USLAX";
			bill.ABL_GoodsDescription = "Goods";
			bill.ABL_RX_NKFreightValueCurrency = "USD";
			bill.ABL_RX_NKCustomsValueCurrency = "USD";
			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.ABL_FreightValue = 100.33;
			bill.ABL_TransportValue = 300.33;
			bill.ABL_InsuranceValue = 400.33;
			bill.ABL_CustomsValue = 500.33;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
		}

		AsycudaPack CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_WeightUQ = "KG";
			pack.APA_Weight = 4000;
			pack.APA_PackUQ = "BOX";
			pack.APA_PackQty = 400;
			pack.APA_MarksAndNumbers = "MADERA";
			pack.APA_GoodsDescription = "PERCHEROS";
			pack.APA_CommodityCode = "940350";
			pack.LinePrice = 700;
			pack.LinePriceCurrency = "USD";

			return pack;
		}

		void SetCredencials()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "ADMINVUCEM13";
			credential.GP_CurrentPassword = "9974567890";

			GlbCompany.CurrentCompany.Factory.Save();
		}

		void AssertMessage(AsycudaBill bill, GlbCompanyCredential credential, ZString messageType)
		{
			var message = bill.Messages[0];
			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.MXCustoms);
				AssertEquals(message.EM_ApplicationReference, header.Bills[0].ABL_BillNumber);
				AssertEquals(message.EM_IsTestMessage, MXCustomsDataRegistry.Instance.IsMXTestingSystem);
				AssertEquals(message.EM_LinkUniqueID, bill.PK);
				AssertEquals(message.EM_MessageOwner, credential.GP_UserID);
				AssertEquals(message.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				AssertEquals(message.EM_Status, EDIMessage.Status.Queued);
				AssertEquals(message.EM_GP, credential.PK);
				AssertEquals(message.EM_MessageType, messageType);
			});
		}
	}
}
