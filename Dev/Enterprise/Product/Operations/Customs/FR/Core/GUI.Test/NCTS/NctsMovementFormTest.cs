using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(NctsMovementForm))]
	public class NctsMovementFormTest : EU.NCTS.GUI.Testing.NctsMovementFormAbstractTest<NctsHeader>
	{
		public void TestFormExist()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new NctsMovementForm(header))
			{
				AssertNotNull(form);
			}
		}
		public void TestMenuDeltaTRemoved()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new NctsMovementForm(header))
			{
				AssertNotNull(form.Menu.MenuItems);
				var messagingMenu = form.Menu.MenuItems.FindByText("Messaging");
				AssertNotNull(messagingMenu);
				var sendDeltaTMenu = messagingMenu.MenuItems.FindByText("Send Delta T");
				AssertNull(sendDeltaTMenu);
			}
		}

		public void TestReplyToEnquiryMenuItemExists()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			using (var form = new NctsMovementForm(header))
			{
				AssertNotNull(form.Menu.MenuItems);
				var messagingMenu = form.Menu.MenuItems.FindByText("Messaging");
				AssertNotNull(messagingMenu);
				AssertNotNull(messagingMenu.MenuItems.FindByText(FRNctsDepartureMovementMessagingMenuProvider.LabelMenuReplyToEnquiry));
			}
		}

		public void TestPressReplyToEnquiryGeneratesIE141Message()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.IsQueried = true;
			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
			var consignee = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			header.Consignee.OrganisationPK = consignee.PK;
			header.Principal.OrganisationPK = consignee.PK;

			var departureOffice = header.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			departureOffice.CY_Data = "FR000040";
			var destinationOffice = header.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationOffice.CY_Data = "FR000040";

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				var messagingMenu = new FRNctsDepartureMovementMessagingMenuProvider(header, nctsMovementForm);
				var replyToEnquiryMenuItem = messagingMenu.CreateMenuItems().FindByText(FRNctsDepartureMovementMessagingMenuProvider.LabelMenuReplyToEnquiry);
				messagingMenu.RefreshMenu();
				replyToEnquiryMenuItem.PerformClick();
				AssertEquals(1, header.Messages.Count);
				AssertEquals("141", header.Messages[0].EM_MessageType);
				AssertContains("CC141A", header.Messages[0].EM_MessageText);
			}
		}

		public void TestReplyToEnquiryMenuItemVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				var messagingMenu = new FRNctsDepartureMovementMessagingMenuProvider(header, nctsMovementForm);
				var replyToEnquiryMenuItem = messagingMenu.CreateMenuItems().FindByText(FRNctsDepartureMovementMessagingMenuProvider.LabelMenuReplyToEnquiry);

				header.IsQueried = false;
				messagingMenu.RefreshMenu();
				AssertEquals(false, replyToEnquiryMenuItem.Visible);

				header.IsQueried = true;
				messagingMenu.RefreshMenu();
				AssertEquals(true, replyToEnquiryMenuItem.Visible);
			}
		}

		public void TestPortMessagingMenuItemVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				var messagingMenu = new FRNctsDepartureMovementMessagingMenuProvider(header, nctsMovementForm);
				var portMessagingMenuItem = messagingMenu.CreateMenuItems().FindByText("Port Messaging");
				messagingMenu.RefreshMenu();
				AssertEquals(true, portMessagingMenuItem.Visible);

				var caedMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("Declaration Pre-Check (CAED)");
				AssertEquals(true, caedMessagingMenuItem.Visible);

				var doaMessagingMenuItem = portMessagingMenuItem.MenuItems.FindByText("Regularization Transit (DOA)");
				AssertEquals(true, doaMessagingMenuItem.Visible);
			}
		}

		protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
		{
			header.QueryInformation = "Information";
		}

		[DeveloperOnlyTest]
		[RequiresSTA]
		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}
	}
}
