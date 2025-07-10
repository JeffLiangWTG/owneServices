using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OrganisationCustomsMessagingPlugInMenuTest : TestCaseWithFactory
	{
		public void TestMenuItems()
		{
			var menu = GetMenuForTest();
			AssertEquals(2, menu.MenuItems.Count);
		}

		public void TestTradeChainPartnerUpdate_Click()
		{
			var org = wrapper.organisation;
			org.OH_Code = "FENIMP";
			org.OH_FullName = "FENIX IMPORTS INC";

			var orgTCP1 = Factory.New<OrgHeader>();
			orgTCP1.OH_Code = "FENVEN";
			orgTCP1.OH_FullName = "DOLE FRESH VEGETABLES";

			var tcp1Addr = orgTCP1.Addresses.AddNew();
			tcp1Addr.Address1 = "500 S ALTA ST";
			tcp1Addr.Address2 = "";
			tcp1Addr.OA_RN_NKCountryCode = "US";
			tcp1Addr.City = "GONZALES";
			tcp1Addr.Postcode = "93926";
			tcp1Addr.State = "CA";

			var orgImpAddInfo = OrgImpAddInfo.Get(org);

			var tcp1 = orgImpAddInfo.TradeChainPartners.AddNew();
			tcp1.CA_Org = org.PK;
			tcp1.CA_Address = tcp1Addr.PK;
			tcp1.CA_CSAIDType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			tcp1.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			tcp1.CA_CSAID = "645321789";
			Factory.Save();

			var menu = GetMenuForTest();
			var item = FindMenuItem(menu.MenuItems, "Send CSA TCP Updates");
			AssertNotNull(item);
			item.PerformClick();
			var form = ZFormModaliser.LastFormShownDialogForTest as TradeChainPartnerSendingMessageForm;
			AssertNotNull(form);
			AssertEquals("Update Trade Chain Partner", form.FormHeading);
			form.Close();

			wrapper.organisation.OH_Code = "TESTORG";
			item.PerformClick();
			AssertEquals("The organization has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 03, 17)]
		public void TestExportTCPDataMenu_Click()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var menu = GetMenuForTest(tempDirectory);
				var item = FindMenuItem(menu.MenuItems, "Export TCP Data");
				AssertNotNull(item);

				var org = wrapper.organisation;
				org.OH_Code = "FENIMP";
				org.OH_FullName = "FENIX IMPORTS INC";

				var orgTCP1 = Factory.New<OrgHeader>();
				orgTCP1.OH_Code = "FENVEN";
				orgTCP1.OH_FullName = "DOLE FRESH VEGETABLES";

				var tcp1Addr = orgTCP1.Addresses.AddNew();
				tcp1Addr.Address1 = "500 S ALTA ST";
				tcp1Addr.Address2 = "";
				tcp1Addr.OA_RN_NKCountryCode = "US";
				tcp1Addr.City = "GONZALES";
				tcp1Addr.Postcode = "93926";
				tcp1Addr.State = "CA";

				var tcp1BRM = orgTCP1.CustomsCodes.AddNew();
				tcp1BRM.OK_CustomsRegNo = "111000201RM9000";
				tcp1BRM.OK_RN_NKCodeCountry = "CA";
				tcp1BRM.OK_CodeType = "BRM";
				tcp1BRM.OK_OA_PremisesAddress = tcp1Addr.PK;

				var orgTCP2 = Factory.New<OrgHeader>();
				orgTCP2.OH_Code = "FENCONSIGNEE";
				orgTCP2.OH_FullName = "1005 ZEHRS";

				var tcp2Addr = orgTCP2.Addresses.AddNew();
				tcp2Addr.Address1 = "1005 OTTAWA ST N";
				tcp2Addr.Address2 = "";
				tcp2Addr.OA_RN_NKCountryCode = "CA";
				tcp2Addr.City = "KITCHENER";
				tcp2Addr.Postcode = "N2A 1H2";
				tcp2Addr.State = "ON";

				var tcp2BRM = orgTCP2.CustomsCodes.AddNew();
				tcp2BRM.OK_CustomsRegNo = "111000222RM9000";
				tcp2BRM.OK_RN_NKCodeCountry = "CA";
				tcp2BRM.OK_CodeType = "BRM";
				tcp2BRM.OK_OA_PremisesAddress = tcp2Addr.PK;

				Factory.Save();

				item.PerformClick();
				AssertContains("Nothing to export.", UnitTestUserNotification.Instance.LastMessage.Text);

				var orgImpAddInfo = OrgImpAddInfo.Get(org);
				var tcp1 = orgImpAddInfo.TradeChainPartners.AddNew();
				tcp1.CA_Org = orgTCP1.PK;
				tcp1.CA_Address = tcp1Addr.PK;
				tcp1.CA_Type = "V";
				tcp1.CA_CSAIDType = "ORG";
				tcp1.CA_CSAID = "FENVEN";
				tcp1.CA_CSAStatus = "Added";

				var tcp2 = orgImpAddInfo.TradeChainPartners.AddNew();
				tcp2.CA_Org = orgTCP2.PK;
				tcp2.CA_Address = tcp2Addr.PK;
				tcp2.CA_Type = "C";
				tcp2.CA_CSAIDType = "ORG";
				tcp2.CA_CSAID = "FENCONSIGNEE";
				tcp2.CA_CSAStatus = "Added";

				item.PerformClick();
				AssertEquals("The organization has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				item.PerformClick();
				AssertContains("CSA Importer FENIX IMPORTS INC is missing mandatory field BRM in Organization. Unable to generate the required file. Please input a valid BRM and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

				var brm = org.CustomsCodes.AddNew();
				brm.OK_CustomsRegNo = "100035922RM0001";
				brm.OK_RN_NKCodeCountry = "CA";
				brm.OK_CodeType = "BRM";

				Factory.Save();
				item.PerformClick();
				AssertContains("TCP Export saved to file:", UnitTestUserNotification.Instance.LastMessage.Text);
				var fileContents = File.ReadAllText(menu.GetFileName());

				AssertASCIIFilesHasSameData(filePath + @"Sample_TCPLoad.txt", menu.GetFileName());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			wrapper = OrgHeaderTCPMessageWrapper.New(organisation);
		}

		OrgHeader organisation;
		OrgHeaderTCPMessageWrapper wrapper;

		readonly string filePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\";

		OrganisationCustomsMessagingPlugInMenuForTesting GetMenuForTest(TempDirectory tempDir = null)
		{
			return new OrganisationCustomsMessagingPlugInMenuForTesting(wrapper
				, () => Env.Licence.ImportBroker.Login(new OrganisationCustomsMessagingPlugIn(new CustomsMessagingPlugInSupportOrgHeaderWrapper(organisation)))
				, tempDir);
		}

		ZMenuItem FindMenuItem(System.Windows.Forms.Menu.MenuItemCollection menuItems, string caption)
		{
			foreach (ZMenuItem menu in menuItems)
			{
				if (menu.Caption == caption)
				{
					return menu;
				}
			}

			return null;
		}

		sealed class OrganisationCustomsMessagingPlugInMenuForTesting : OrganisationCustomsMessagingPlugInMenu
		{
			public OrganisationCustomsMessagingPlugInMenuForTesting(OrgHeaderTCPMessageWrapper organisation, Action licenceLogIn, TempDirectory tempDir = null) : base(organisation)
			{
				this.tempDir = tempDir;
			}
			readonly TempDirectory tempDir;

			internal ZString GetFileName() => Path.Combine(GetOutptutPathForExport(), GetFileNameForExport());

			protected override ZString GetOutptutPathForExport() => tempDir.DirectoryName;
		}
	}
}
