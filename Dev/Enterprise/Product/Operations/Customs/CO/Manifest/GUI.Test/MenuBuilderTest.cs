using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	class MenuBuilderTest : TestCaseWithFactory
	{
		protected AsycudaManifestHeader header;

		public void TestSaveRequestManifest()
		{
			LoadDocumentIds();
			PopulateManifestHeader();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Issue Date: You have not entered an Issue Date.
ARV: You have not entered an Estimated Time of Arrival at Border.
Travel Document Type: You have not entered a Travel Document Type.
Cargo Disposition: You have not entered a Cargo Disposition.
Container Mode: You have not entered a Container Mode.
Goods' Description (on Bill): You have not entered a Goods' Description (on Bill).
Gross Weight: You have not entered a Gross Weight.
Gross Weight Unit: You have not entered a Gross Weight Unit.
Quantity (on Bill): You have not entered a Quantity (on Bill).
Manifest UQ: You have not entered a Manifest UQ.
Consignee: You have not entered a Consignee.
Goods Location: A Warehouse is required
Shipper: You have not entered a Shipper.
Volume (on Bill): You have not entered a Volume (on Bill).
Volume Unit (on Bill): You have not entered a Volume Unit (on Bill).
Customs Office: The code you have selected is not in the list.
Carrier: You have not entered a Carrier.
Shipping Agent: You have not entered a Shipping Agent.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							var fileName = tempFile.Filename;

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							AssertContains("Request file Dmuisca_010116607" + DateTime.Now.Year.ToString() + "00000096 saved.", UnitTestUserNotification.Instance.LastMessage.Text);

							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}

							AssertEquals("The Message Status should be Sent", "SNT", header.AMA_MessageStatus);

							var storageMain = header.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(header, Core.Constants.RefDocTypes.MiscellaneousDocument);

							AssertEquals(1, storageMain.Files.Count);
							AssertEquals("Dmuisca_010116607" + DateTime.Now.Year.ToString() + "00000096.xml", storageMain.Files[0].FileName);
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		public void TestSaveAmendManifest()
		{
			LoadDocumentIds();
			PopulateManifestHeader(true, true, true);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
							menu.MenuItems.FindByText("Save Amend Manifest").PerformClick();

							AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Issue Date: You have not entered an Issue Date.
ARV: You have not entered an Estimated Time of Arrival at Border.
Travel Document Type: You have not entered a Travel Document Type.
Cargo Disposition: You have not entered a Cargo Disposition.
Container Mode: You have not entered a Container Mode.
Goods' Description (on Bill): You have not entered a Goods' Description (on Bill).
Gross Weight: You have not entered a Gross Weight.
Gross Weight Unit: You have not entered a Gross Weight Unit.
Quantity (on Bill): You have not entered a Quantity (on Bill).
Manifest UQ: You have not entered a Manifest UQ.
Consignee: You have not entered a Consignee.
Goods Location: A Warehouse is required
Shipper: You have not entered a Shipper.
Volume (on Bill): You have not entered a Volume (on Bill).
Volume Unit (on Bill): You have not entered a Volume Unit (on Bill).
Customs Office: The code you have selected is not in the list.
Carrier: You have not entered a Carrier.
Shipping Agent: You have not entered a Shipping Agent.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							var fileName = tempFile.Filename;

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

							menu.MenuItems.FindByText("Save Amend Manifest").PerformClick();

							AssertContains("Amend file Dmuisca_020116607" + DateTime.Now.Year.ToString() + "00000096 saved.", UnitTestUserNotification.Instance.LastMessage.Text);

							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}

							AssertEquals("The Message Status should be Sent", "SNT", header.AMA_MessageStatus);

							var storageMain = header.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(header, Core.Constants.RefDocTypes.MiscellaneousDocument);
							AssertEquals(1, storageMain.Files.Count);
							AssertEquals("Dmuisca_020116607" + DateTime.Now.Year.ToString() + "00000096.xml", storageMain.Files[0].FileName);
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		public void TestRemainingDocumentIds()
		{
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, ZBool.False, "11667803932049", GlbCompany.CurrentCompany.PK);
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, ZBool.True, "11667803932078", GlbCompany.CurrentCompany.PK);
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, ZBool.True, "11667803932082", GlbCompany.CurrentCompany.PK);
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, ZBool.False, "11667803932090", GlbCompany.GetDemoCompany(Factory).PK);
			CreateDocumentId(CusTransactionNumberTypeList.Codes.IECustoms, ZBool.False, "11667803932063", GlbCompany.CurrentCompany.PK);

			PopulateManifestHeader(true);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // File save 

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							AssertContains("There is not enough documents IDs to be used in the current file.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertContains("You should ask for a new set of IDs and load it on Maintain->Customs(CO)->Document IDs to the system before saving again.", UnitTestUserNotification.Instance.LastMessage.Text);

							var fileName = tempFile.Filename;
							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		public void TestDoNotSendManifestWithoutNITRegNoFilled()
		{
			GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;

			PopulateManifestHeader();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Save Request Manifest").PerformClick();
					AssertEquals("You have not entered a Company Tax ID. You can enter it from Maintain -> User Admin -> Companies. Picking the company, NIT Reg No field.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDoNotSendManifestWithoutBillsAdded()
		{
			PopulateManifestHeader(false);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Save Request Manifest").PerformClick();
					AssertEquals("No Bills have been added. Please add their information so that the Manifest can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		public void TestDoNotSendManifestWithoutPacksAdded()
		{
			PopulateManifestHeader(true, false);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Save Request Manifest").PerformClick();
					AssertEquals("No Packs have been added. Please add their information so that the Manifest can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDoNotSendManifestWithoutEnoughDocumentIds()
		{
			PopulateManifestHeader();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // File save 

					menu.MenuItems.FindByText("Save Request Manifest").PerformClick();
					AssertEquals("There is not enough documents IDs to be used in the current file." + System.Environment.NewLine + "You should ask for a new set of IDs and load it on Maintain->Customs(CO)->Document IDs to the system before saving again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSaveRequestManifestFromConsol()
		{
			var consol = CreateAndPopulateForwardingConsol();
			LoadDocumentIds();
			PopulateManifestHeader();

			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				using (var tempFile = TempFile.New())
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Manifest").OnPopup(EventArgs.Empty);

					var manifestMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Manifest")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "CO Manifest")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Save Request Manifest");

					AssertNotNull("Save Request Manifest menu item exists", manifestMenuItem);

					try
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						manifestMenuItem.PerformClick();

						AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Issue Date: You have not entered an Issue Date.
ARV: You have not entered an Estimated Time of Arrival at Border.
Travel Document Type: You have not entered a Travel Document Type.
Cargo Disposition: You have not entered a Cargo Disposition.
Container Mode: You have not entered a Container Mode.
Goods' Description (on Bill): You have not entered a Goods' Description (on Bill).
Gross Weight: You have not entered a Gross Weight.
Gross Weight Unit: You have not entered a Gross Weight Unit.
Quantity (on Bill): You have not entered a Quantity (on Bill).
Manifest UQ: You have not entered a Manifest UQ.
Consignee: You have not entered a Consignee.
Goods Location: A Warehouse is required
Shipper: You have not entered a Shipper.
Volume (on Bill): You have not entered a Volume (on Bill).
Volume Unit (on Bill): You have not entered a Volume Unit (on Bill).
Customs Office: The code you have selected is not in the list.
Carrier: You have not entered a Carrier.
Shipping Agent: You have not entered a Shipping Agent.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

						var fileName = tempFile.Filename;

						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

						manifestMenuItem.PerformClick();

						AssertContains("Request file Dmuisca_010116607" + DateTime.Now.Year.ToString() + "00000096 saved.", UnitTestUserNotification.Instance.LastMessage.Text);

						if (File.Exists(fileName))
						{
							Assert(true);
							File.Delete(fileName);
						}
						else
						{
							Fail("File is not Created.");
						}

						AssertEquals("The Message Status should be Sent", "SNT", header.AMA_MessageStatus);

						var storageMain = header.Consol.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(header.Consol, Core.Constants.RefDocTypes.MiscellaneousDocument);

						AssertEquals(1, storageMain.Files.Count);
						AssertEquals("Dmuisca_010116607" + DateTime.Now.Year.ToString() + "00000096.xml", storageMain.Files[0].FileName);
					}
					finally
					{
						DeleteIfExists(tempFile.Filename);
					}
				}
			}
		}

		ForwardingConsol CreateAndPopulateForwardingConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "DEABC";
			consol.JK_RL_NKLoadPort = "CO8SG";

			return consol;
		}

		void PopulateManifestHeader(bool includeBills = true, bool includePacks = true, bool isMessageStatusAccepted = false)
		{
			header.AMA_ManifestNumber = "96";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Voyage = "UC1103";
			header.AMA_CustomsOffice = "25";
			header.AMA_RL_NKPortOfLoading = "CO8SG";
			header.AMA_RL_NKPortOfDischarge = "DEABC";
			header.DeliveryMode = CODeliveryModeList.Codes._1;

			if (isMessageStatusAccepted)
			{
				header.AMA_MessageStatus = "ACP";
			}

			if (includeBills)
			{
				CreateAndPopulateHouseBill(includePacks);
				CreateAndPopulateMasterBill();
			}

			Factory.Save();
		}

		void CreateAndPopulateHouseBill(ZBool includePacks)
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";

			if (includePacks)
			{
				CreateAndPopulatePack(bill);
			}
		}

		void CreateAndPopulateMasterBill()
		{
			var bill = header.MasterBill;

			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_CustomsDischargePort = "COBOG";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			var pack = bill.Packs.AddNew();
			pack.IsHazardous = true;

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Code = "12345";
			substance.DG_Class = "1";

			var undg = pack.UNDGs.AddNew();
			undg.SubstancePK = substance.PK;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "DGContact";
			undg.DGContact.OC_Phone = "092702171";
			undg.DGContact.OC_OA_OrgAddress = orgAddress.PK;
			undg.DI_IMOClass = "1";
		}

		void LoadDocumentIds()
		{
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, false, "11667803932049", GlbCompany.CurrentCompany.PK);
			CreateDocumentId(CusTransactionNumberTypeList.Codes.ColombiaManifest, false, "11667803932056", GlbCompany.CurrentCompany.PK);
		}

		void CreateDocumentId(ZString type, ZBool isUsed, ZString docId, ZGuid companyPK)
		{
			var transactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();

			transactionNumber.TN_Type = type;
			transactionNumber.TN_IsUsed = isUsed;
			transactionNumber.TN_TransactionReference = docId;
			transactionNumber.TN_GC_Company = companyPK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}
	}
}
