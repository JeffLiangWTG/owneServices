using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class ESNctsArrivalMovementMessagingMenuProviderTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNewNctsArrivalMovementMessagingMenuProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				AssertType(typeof(ESNctsArrivalMovementMessagingMenuProvider), new ESNctsArrivalMovementMessagingMenuProvider(header, nctsMovementForm));
			}
		}

		public void TestMenuItemVisibility_Create_EXS_declaration()
		{
			CombineAssertions(() =>
			{
				var nctsHeaderArrival = Factory.New<NctsHeader>();
				nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsWrittenOff;

				using (var nctsMovementFormArrival = new NctsMovementForm(nctsHeaderArrival))
				{
					var menu = new ESNctsArrivalMovementMessagingMenuProvider(nctsHeaderArrival, nctsMovementFormArrival);
					var menuItems = menu.CreateMenuItems().ToArray();
					var createEXSDeclarationMenuItem = menuItems.FindByText("Create EXS declaration");

					menu.RefreshMenu();
					AssertEquals("In arrival, 'Create EXS declaration' MenuItem is visible when arrival status is AWO", true, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
					menu.RefreshMenu();
					AssertEquals("In arrival, 'Create EXS declaration' MenuItem is not visible when arrival status is INI", false, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.UnloadingPermissionGranted;
					menu.RefreshMenu();
					AssertEquals("In arrival, 'Create EXS declaration' MenuItem is visible when arrival status is AUP", true, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
					menu.RefreshMenu();
					AssertEquals("In arrival, 'Create EXS declaration' MenuItem is visible when arrival status is DNR", false, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
					menu.RefreshMenu();
					AssertEquals("In arrival, 'Create EXS declaration' MenuItem is visible when arrival status is DCC", true, createEXSDeclarationMenuItem.Visible);
				}

				var nctsHeaderArrivalAndDeparture = Factory.New<NctsHeader>();
				nctsHeaderArrivalAndDeparture.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				nctsHeaderArrivalAndDeparture.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsWrittenOff;
				using (var nctsMovementFormArrival = new NctsMovementForm(nctsHeaderArrivalAndDeparture))
				{
					var menu = new ESNctsArrivalMovementMessagingMenuProvider(nctsHeaderArrivalAndDeparture, nctsMovementFormArrival);
					var menuItems = menu.CreateMenuItems().ToArray();
					var createEXSDeclarationMenuItem = menuItems.FindByText("Create EXS declaration");

					menu.RefreshMenu();
					AssertEquals("In arrival and departure, 'Create EXS declaration MenuItem' is visible when arrival status is AWO", true, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrivalAndDeparture.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
					menu.RefreshMenu();
					AssertEquals("In arrival and departure, 'Create EXS declaration' MenuItem is not visible when arrival status is INI", false, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrivalAndDeparture.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.UnloadingPermissionGranted;
					menu.RefreshMenu();
					AssertEquals("In arrival and departure, 'Create EXS declaration MenuItem' is visible when arrival status is AUP", true, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrivalAndDeparture.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
					menu.RefreshMenu();
					AssertEquals("In arrival and departure, 'Create EXS declaration MenuItem' is visible when arrival status is CNR", false, createEXSDeclarationMenuItem.Visible);

					nctsHeaderArrivalAndDeparture.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
					menu.RefreshMenu();
					AssertEquals("In arrival and departure, 'Create EXS declaration MenuItem' is visible when arrival status is DCC", true, createEXSDeclarationMenuItem.Visible);
				}
			});
		}

		[RequiresSTA]
		public void TestMenu_CreateEXSDeclarationMessageFromArrival()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
			arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var arrivalMovementeHeaderGoodsItemNew = arrivalHeader.ArrivalMovementHeader.GoodsItems.AddNew();

			var lineNew = arrivalHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			lineNew.BY_LineNo = arrivalMovementeHeaderGoodsItemNew.BY_LineNo;
			lineNew.HasDifferences = false;
			lineNew.IsMissing = false;
			lineNew.IsNew = true;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var nctsMovementFormArrival = new NctsMovementForm(arrivalHeader))
			{
				nctsMovementFormArrival.Show();
				var menu = new ESNctsArrivalMovementMessagingMenuProvider(arrivalHeader, nctsMovementFormArrival);
				var menuItems = menu.CreateMenuItems().ToArray();
				var menuItem = menuItems.FindByText("Create EXS declaration");

				menuItem.PerformClick();
				AssertEquals("Message when create EXS declaration with arrival", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration with number"));
			}
		}

		[RequiresSTA]
		public void TestMenu_CreateEXSDeclarationMessageFromArrivalAndDeparture()
		{
			var arrivalAndDepartureHeader = Factory.New<NctsHeader>();
			arrivalAndDepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalAndDepartureHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalAndDepartureHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
			arrivalAndDepartureHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
			arrivalAndDepartureHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;

			var arrivalMovementeHeaderGoodsItem = arrivalAndDepartureHeader.ArrivalMovementHeader.GoodsItems.AddNew();

			var lineNew = arrivalAndDepartureHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			lineNew.BY_LineNo = arrivalMovementeHeaderGoodsItem.BY_LineNo;
			lineNew.HasDifferences = false;
			lineNew.IsMissing = false;
			lineNew.IsNew = true;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var nctsMovementFormArrival = new NctsMovementForm(arrivalAndDepartureHeader))
			{
				nctsMovementFormArrival.Show();
				var menu = new ESNctsArrivalMovementMessagingMenuProvider(arrivalAndDepartureHeader, nctsMovementFormArrival);
				var menuItems = menu.CreateMenuItems().ToArray();
				var menuItem = menuItems.FindByText("Create EXS declaration");

				menuItem.PerformClick();
				AssertEquals("Message when create EXS declaration with arrival and departure", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration with number"));
			}
		}

		public void TestMenu_CreateEXSDeclarationMessageErrorFoundFromArrival()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
			arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var arrivalMovementeHeaderGoodsItemNew = arrivalHeader.ArrivalMovementHeader.GoodsItems.AddNew();

			var lineNew = arrivalHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			lineNew.BY_LineNo = arrivalMovementeHeaderGoodsItemNew.BY_LineNo;
			lineNew.HasDifferences = false;
			lineNew.IsMissing = true;
			lineNew.IsNew = false;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var nctsMovementFormArrival = new NctsMovementForm(arrivalHeader))
			{
				nctsMovementFormArrival.Show();
				var menu = new ESNctsArrivalMovementMessagingMenuProvider(arrivalHeader, nctsMovementFormArrival);
				var menuItems = menu.CreateMenuItems().ToArray();
				var menuItem = menuItems.FindByText("Create EXS declaration");

				menuItem.PerformClick();
				AssertEquals("Error message when create EXS declaration", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration could not be created"));
			}
		}

		public void TestMenuItemVisibility_CanDownloadTADMenuItem_ControlArrival()
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;

			using (var nctsMovementFormArrival = new NctsMovementForm(nctsHeaderArrival))
			{
				var menu = new ESNctsArrivalMovementMessagingMenuProvider(nctsHeaderArrival, nctsMovementFormArrival);
				var menuItems = menu.CreateMenuItems().ToArray();
				var downloadTADMenuItem = menuItems.FindByText("Download TAD (Transit Accompanying Document)");

				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					AssertEquals("Download TAD (Transit Accompanying Document) MenuItem is visible when header no has MRN", false, downloadTADMenuItem.Visible);

					CreateOrUpdateCusEntryNumber(nctsHeaderArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
					menu.RefreshMenu();
					AssertEquals("Download TAD (Transit Accompanying Document) MenuItem is visible when header has MRN and the Arrival Declaration is created", true, downloadTADMenuItem.Visible);
				});
			}
		}

		[RequiresSTA]
		public void TestMenu_SendArrivalAndDepartureMessage_ES()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BBBBB";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			header.DestinationCustomsOfficeCodeForArrival = "ES009999";
			header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "ES009999");
			header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "ES009999");
			header.Guarantees.AddNew().PW_BondNumber2 = "22";
			header.ArrivalMrnFromUser = "19DE12345678900000";
			header.DestinationTrader.OrganisationPK = org.PK;
			header.Declarant.E2_OA_Address = org.MainAddress.PK;
			header.MovementHeader.BM_TransportAtDeparture = "ID";
			header.MovementHeader.BM_RN_NKTransportAtDepartureCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_Description = CodeToEdit;
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			header.LocalReferenceNumber = "NCT00000012";
			header.BH_GB = GlbBranch.CurrentBranch.PK;

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				nctsMovementForm.Show();
				var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(header, nctsMovementForm, true, false);
				var menuItems = menu.CreateMenuItems().ToArray();
				var menuItem = menuItems.FindByText("Send Combined Arrival and Departure");

				var arrivalMRNTextBox = nctsMovementForm.FindSingle<ZTextBox>("zTextBox1");

				CombineAssertions(() =>
				{
					menuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					header.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					header.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					header.BH_CustomsProfile = ZString.Empty;
					header.Factory.Save();
					menuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					header.BH_CustomsProfile = "INVALID";
					header.Factory.Save();
					menuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					header.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					header.Factory.Save();
					menuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					header.Reload();
					menu.RefreshMenu();
					menuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("When the declaration has not been sent, ArrivalTabPage should be unlocked, zTextBox1.ReadOnly", false, arrivalMRNTextBox.ReadOnly);

					menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(header, nctsMovementForm, false, false);
					menuItems = menu.CreateMenuItems().ToArray();
					menuItem = menuItems.FindByText("Send Combined Arrival and Departure");
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, header.EffectiveMessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("EDIMessage Type is TNA", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("When the declaration has been sent, ArrivalTabPage should be locked, zTextBox1.ReadOnly", true, arrivalMRNTextBox.ReadOnly);
				});
			}
		}

		public void TestMenu_SendArrivalAndDepartureMessage_ES_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "BBBBB";
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				header.DestinationCustomsOfficeCodeForArrival = "ES009999";
				header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "ES009999");
				header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "ES009999");
				header.Guarantees.AddNew().PW_BondNumber2 = "22";
				header.ArrivalMrnFromUser = "19DE12345678900000";
				header.DestinationTrader.OrganisationPK = org.PK;
				header.Declarant.E2_OA_Address = org.MainAddress.PK;
				header.MovementHeader.BM_TransportAtDeparture = "ID";
				header.MovementHeader.BM_RN_NKTransportAtDepartureCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				var goodsItem = header.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Description = CodeToEdit;
				header.Principal.E2_OA_Address = org.MainAddress.PK;
				header.LocalReferenceNumber = "NCT00000012";
				header.BH_GB = GlbBranch.CurrentBranch.PK;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(header))
				{
					nctsMovementForm.Show();
					var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(header, nctsMovementForm, true, true);
					var menuItems = menu.CreateMenuItems().ToArray();
					var menuItem = menuItems.FindByText("Send Combined Arrival and Departure");

					var arrivalMRNTextBox = nctsMovementForm.FindSingle<ZTextBox>("zTextBox1");

					CombineAssertions(() =>
					{
						menuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						header.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						header.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						header.BH_CustomsProfile = ZString.Empty;
						header.Factory.Save();
						menuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						header.BH_CustomsProfile = "INVALID";
						header.Factory.Save();
						menuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						header.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						header.Factory.Save();
						menuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						header.Reload();
						menu.RefreshMenu();
						menuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("When the declaration has not been sent, ArrivalTabPage should be unlocked, zTextBox1.ReadOnly", false, arrivalMRNTextBox.ReadOnly);

						menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(header, nctsMovementForm, false, true);
						menuItems = menu.CreateMenuItems().ToArray();
						menuItem = menuItems.FindByText("Send Combined Arrival and Departure");
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
						menuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, header.EffectiveMessageStatus);
						var msg = header.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("EDIMessage Type is TNA", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi, msg.EM_MessageType);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

						AssertEquals("When the declaration has been sent, ArrivalTabPage should be locked, zTextBox1.ReadOnly", true, arrivalMRNTextBox.ReadOnly);
					});
				}
			}
		}

		public void TestMenu_SendArrivalAndDepartureMessageFoundMatchingMRN_ES()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BBBBB";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

			var depHeader = Factory.New<NctsHeader>();
			depHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			depHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "19DE12345678900000";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.CE_ParentTable = "CusInBondHeader";
			entryNum.Parent = depHeader;

			depHeader.ArrivalMrnFromUser = entryNum.CE_EntryNum;

			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.DestinationCustomsOfficeCodeForArrival = "ES009999";
			header.ArrivalMrnFromUser = entryNum.CE_EntryNum;
			header.DestinationTrader.OrganisationPK = org.PK;
			header.BH_GB = GlbBranch.CurrentBranch.PK;

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = header;
				var menuItem = menu.MenuItems.FindByText("Send Combined Arrival and Departure");
				menuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Use this departure’s data for TNN?"));
			}
		}

		public void TestMenu_SendArrivalAndDepartureMessageNotFoundMatchingMRN_ES()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.DestinationCustomsOfficeCodeForArrival = "ES009999";
			header.ArrivalMrnFromUser = "19DE12345678900000";
			header.DestinationTrader.OrganisationPK = org.PK;
			header.BH_GB = GlbBranch.CurrentBranch.PK;

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = header;
				var menuItem = menu.MenuItems.FindByText("Send Combined Arrival and Departure");
				menuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Generate new departure for TNN?"));
			}
		}

		public void TestSendMessageToNctsCore()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.FillWithValidTestData();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			nctsHeader.ArrivalMrnFromUser = CodeToEdit;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address";
			org.Contacts.AddNew();
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, true, false);
				var menuItems = menu.CreateMenuItems().ToArray();
				var arrMenuItem = menuItems.FindByText("Send Arrival Message");

				var cnrTextBox = nctsMovementForm.FindSingle<ZTextBox>("zTextBoxCRN");

				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					menu.RefreshMenu();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					menu.RefreshMenu();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					menu.RefreshMenu();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					menu.RefreshMenu();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("When the declaration has not been sent, ArrivalTabPage should be unlocked, zTextBoxCRN.ReadOnly", false, cnrTextBox.ReadOnly);

					menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, false, false);
					menuItems = menu.CreateMenuItems().ToArray();
					arrMenuItem = menuItems.FindByText("Send Arrival Message");
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					arrMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("EDIMessage Type is AVI", DeclarationMessageTypeList.Codes.NctsArrivalNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("When the declaration has been sent, ArrivalTabPage should be locked, zTextBoxCRN.ReadOnly", true, cnrTextBox.ReadOnly);
				});
			}
		}

		public void TestSendMessageToNctsCore_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.FillWithValidTestData();
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
				nctsHeader.ArrivalMrnFromUser = CodeToEdit;
				nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
				var address = org.Addresses.AddNew();
				address.OA_Address1 = "Address";
				org.Contacts.AddNew();
				nctsHeader.DestinationTrader.OrganisationPK = org.PK;

				nctsHeader.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					nctsMovementForm.Show();
					var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, true, true);
					var menuItems = menu.CreateMenuItems().ToArray();
					var arrMenuItem = menuItems.FindByText("Send Arrival Message");

					var cnrTextBox = nctsMovementForm.FindSingle<ZTextBox>("zTextBoxCRN");

					CombineAssertions(() =>
					{
						menu.RefreshMenu();
						arrMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						nctsHeader.BH_CustomsProfile = ZString.Empty;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						arrMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = "INVALID";
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						arrMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						arrMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						menu.RefreshMenu();
						arrMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("When the declaration has not been sent, ArrivalTabPage should be unlocked, zTextBoxCRN.ReadOnly", false, cnrTextBox.ReadOnly);

						menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, false, true);
						menuItems = menu.CreateMenuItems().ToArray();
						arrMenuItem = menuItems.FindByText("Send Arrival Message");
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						arrMenuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("EDIMessage Type is AVI", DeclarationMessageTypeList.Codes.NctsArrivalNotification, msg.EM_MessageType);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

						AssertEquals("When the declaration has been sent, ArrivalTabPage should be locked, zTextBoxCRN.ReadOnly", true, cnrTextBox.ReadOnly);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestSendMessageToNctsCore_ParentFormShipment()
		{
			SetRefData();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.FillWithValidTestData();
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			nctsHeader.ArrivalMovementHeader.BM_InBondEntryType = NctsArrivalDocTypeList.Codes.Dua;
			nctsHeader.ArrivalMovementHeader.BM_TransportAtDeparture = "transport";
			nctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "ES";
			nctsHeader.ArrivalMovementHeader.BM_ArrivalDate = ZDateTime.Now.AddDays(1);
			nctsHeader.ArrivalMrnFromUser = CodeToEdit;

			CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "ES009999", new ZDateTime(2012, 10, 13, 7, 7, 0));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address";
			org.Contacts.AddNew();
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.DeclarantAddressPK = org.MainAddress.PK;
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var frm = new ShipmentForm(shipment))
			using (var menu = new NctsMessagingMenu(frm))
			{
				menu.NctsHeader = nctsHeader;
				var control = (NctsUserControlForPlugin)frm.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.NctsMovementController).UserControl;
				control.SetDataBinding(nctsHeader, "");
				frm.Controls.Add(control);
				frm.Show();
				var menuItems = menu.MenuItems;
				var arrMenuItem = menuItems.FindByText("Send Arrival Message");

				var cnrTextBox = frm.FindSingle<ZTextBox>("zTextBoxCRN");

				CombineAssertions(() =>
				{
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					arrMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						AssertEquals("When the declaration has not been sent, ArrivalTabPage should be unlocked, zTextBoxCRN.ReadOnly", false, cnrTextBox.ReadOnly);

						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						arrMenuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("EDIMessage Type is AVI", DeclarationMessageTypeList.Codes.NctsArrivalNotification, msg.EM_MessageType);
						AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

						AssertEquals("When the declaration has been sent, ArrivalTabPage should be locked, zTextBoxCRN.ReadOnly", true, cnrTextBox.ReadOnly);
					}
				});
			}
		}

		public void TestDownloadTAD_NoDocumentNeeded()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
				CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, "ABCDEFGHIJKLMNOP", ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, true, true);
					var menuItems = menu.CreateMenuItems().ToArray();
					var downloadTADMenuItem = menuItems.FindByText("Download TAD (Transit Accompanying Document)");

					menu.RefreshMenu();
					downloadTADMenuItem.PerformClick();
					AssertContains("The header is not saved so nothing is done", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

					var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
					docManagerInfo.AddFileOrDocument(new byte[1], "21ES00999912345678_NCTS_AEAT_TAD.pdf", "CAU");
					docManagerInfo.Save();
					nctsHeader.Factory.Save();

					menu.RefreshMenu();
					downloadTADMenuItem.PerformClick();
					AssertContains("All Documents for AEAT already exist for the NCTS Departure so nothing will be sent", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDownloadTAD_DocumentNeeded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					var menu = new ESNctsArrivalMovementMessagingMenuProviderForEditTest(nctsHeader, nctsMovementForm, true, true);
					var menuItems = menu.CreateMenuItems().ToArray();
					var downloadTADMenuItem = menuItems.FindByText("Download TAD (Transit Accompanying Document)");

					CombineAssertions(() =>
					{
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						nctsHeader.BH_CustomsProfile = ZString.Empty;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = "INVALID";
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
						{
							nctsHeader.Reload();
							nctsHeader.Principal.OrganisationPK = Principal.PK;
							nctsHeader.Factory.Save();

							menu.RefreshMenu();
							downloadTADMenuItem.PerformClick();
							AssertContains("NCTS Departure Document Capture Request can not be sent when there is no MRN so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

							CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
							nctsHeader.Factory.Save();
							menu.RefreshMenu();
							downloadTADMenuItem.PerformClick();
							AssertContains("NCTS Departure Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

							CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, "ABCDEFGHIJKLMNOP", ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
							nctsHeader.Factory.Save();
							menu.RefreshMenu();
							downloadTADMenuItem.PerformClick();
							AssertContains("1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					});
				}
			}
		}

		public OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = CreateOrgHeader(PrincipalData.Id, PrincipalData.IdType, PrincipalData.Code, PrincipalData.Name, PrincipalData.Address, PrincipalData.City, PrincipalData.PostCode, PrincipalData.Country);
				}
				return principal;
			}
		}
		OrgHeader principal;

		OrgHeader CreateOrgHeader(string id, string type, string code, string name, string address, string city, string postCode, string country)
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(type, id);
			org.OH_Code = code;
			org.OH_FullName = name;

			org.MainAddress.OA_OH = org.PK;
			org.MainAddress.OA_Address1 = address;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_PostCode = postCode;
			org.MainAddress.OA_RN_NKCountryCode = country;

			return org;
		}

		public struct PrincipalData
		{
			public const string Id = "DEC33333333";
			public const string IdType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			public const string Code = "DECTEST";
			public const string Name = "Declarant Test Org";
			public const string Address = "Declarant Test Street";
			public const string City = "Valencia";
			public const string PostCode = "45612";
			public const string Country = "ES";

			public const string Email = "mail.mail@mail.com";
		}

		void SetRefData()
		{
			var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, declarationTypeCode, NctsDeclarationTypeList.Codes.T1, "Goods moving under external community transit procedure", startDate, endDate);

			Factory.Save();
		}

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.GetStaffAccount();
				}

				return staff;
			}
		}
		GlbStaff staff;

		const string CodeToEdit = "Description";

		public static EuOfficeCode CreateCustomsOfficeForTest(NctsHeader nctsHeader, string officePurpose, string officeCode, ZDateTime arrivalTime, bool clearOffices = false)
		{
			NctsEuOfficeCode office = null;
			if (clearOffices)
			{
				nctsHeader.CustomsOffices.RemoveAndDeleteAll();
			}
			else
			{
				if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				}
				else if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				}
			}

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = nctsHeader.Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			nctsHeader.Factory.Save();

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}
			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;
			return office;
		}

		public void TestMenu_ArrivalVisibility_CanSendArrivalMessage()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				var menuES = new ESNctsArrivalMovementMessagingMenuProvider(header, nctsMovementForm);
				var menuItems = menuES.CreateMenuItems().ToArray();
				var sendArrival = menuItems.FindByText("Send Arrival Message");

				CombineAssertions(() =>
				{
					header.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationRejected;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is not visible when status is DRJ", false, sendArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is visible when status is DRJ", true, sendArrival.Visible);
					header.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.Unknown;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is not visible when status is empty", false, sendArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is visible when status is empty", true, sendArrival.Visible);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is visible when status is MAS", true, sendArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is not visible when status is MAS", false, sendArrival.Visible);
				});
			}
		}

		public void TestMenu_ArrivalVisibility_CanSendCombinedArrivalAndDeparture()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				var menuES = new ESNctsArrivalMovementMessagingMenuProvider(header, nctsMovementForm);
				var menuItems = menuES.CreateMenuItems().ToArray();
				var sendDepartureAndArrival = menuItems.FindByText("Send Combined Arrival and Departure");

				CombineAssertions(() =>
				{
					header.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationRejected;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is not visible when status is DRJ", false, sendDepartureAndArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is visible when status is DRJ", true, sendDepartureAndArrival.Visible);
					header.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.Unknown;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is not visible when status is empty", false, sendDepartureAndArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is visible when status is empty", true, sendDepartureAndArrival.Visible);
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
					menuES.RefreshMenu();
					AssertNotEquals("Send Arrival Message MenuItem is visible when status is MAS", true, sendDepartureAndArrival.Visible);
					AssertEquals("Send Arrival Message MenuItem is not visible when status is MAS", false, sendDepartureAndArrival.Visible);
				});
			}
		}

		public void TestViewOnCustomsWebsite_NctsArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl;

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = nctsHeader;
				var menuItem = menu.MenuItems.FindByText("View on Customs Website");

				ZFormModaliser.ShowDialogsInTest = false;

				AssertViewOnCustomsWebsite(nctsHeader, menuItem);
			}
		}

		public void TestViewOnCustomsWebsite_NctsDepartureAndArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = nctsHeader;
				var menuItem = menu.MenuItems.FindByText("View on Customs Website");

				ZFormModaliser.ShowDialogsInTest = false;

				AssertViewOnCustomsWebsite(nctsHeader, menuItem);
			}
		}

		public void TestMenu_ArrivalVisibility_ViewOnCustomsWebsite()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.GoodsWrittenOff;

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				var menuES = new ESNctsArrivalMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = menuES.CreateMenuItems().ToArray();
				var viewOnWebsite = menuItems.FindByText("View on Customs Website");

				AssertViewOnCustomsWebsiteVisibility(nctsHeader, menuES, viewOnWebsite);
			}
		}

		[RequiresSTA]
		public void TestMenu_DepartureAndArrivalVisibility_ViewOnCustomsWebsite()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				var menuES = new ESNctsArrivalMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = menuES.CreateMenuItems().ToArray();
				var viewOnWebsite = menuItems.FindByText("View on Customs Website");

				AssertViewOnCustomsWebsiteVisibility(nctsHeader, menuES, viewOnWebsite);
			}
		}

		void AssertViewOnCustomsWebsite(NctsHeader nctsHeader, MenuItem menuItem)
		{
			var expectedMRN = "AACCRRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				menuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when NctsHeader has no MRN", WebUrlLauncher.LastUrlLaunched);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = expectedMRN;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				menuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			});
		}

		void AssertViewOnCustomsWebsiteVisibility(NctsHeader nctsHeader, ESNctsArrivalMovementMessagingMenuProvider menuES, MenuItem viewOnWebsite)
		{
			CombineAssertions(() =>
			{
				menuES.RefreshMenu();
				AssertEquals("View On Website MenuItem is not visible when NctsHeader has no MRN", false, viewOnWebsite.Visible);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = "AACCRRRRRRNNNNNNNN";
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				menuES.RefreshMenu();
				AssertEquals("View On Website MenuItem is visible when NctsHeader has MRN", true, viewOnWebsite.Visible);
			});
		}

		protected void CreateOrUpdateCusEntryNumber(NctsHeader header, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate, ZDateTime expiryDate)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_IssueDate = issueDate;
			newEntryNumber.CE_ExpiryDate = expiryDate;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}

		class ESNctsArrivalMovementMessagingMenuProviderForEditTest : ESNctsArrivalMovementMessagingMenuProvider
		{
			public ESNctsArrivalMovementMessagingMenuProviderForEditTest(NctsHeader header, NctsMovementForm nctsMovementForm, ZBool hasInvalidCertificate, ZBool shouldEdit) : base(header, nctsMovementForm)
			{
				this.shouldEdit = shouldEdit;
				this.hasInvalidCertificate = hasInvalidCertificate;
			}
			readonly ZBool shouldEdit;
			readonly ZBool hasInvalidCertificate;

			protected override void SendMessageToNctsCore(NctsMessageFunctionSet messageFunction)
			{
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(messageFunction, (NctsHeader)Header, hasInvalidCertificate, shouldEdit, (NctsMovementForm)ParentForm);
				commonMenuProvider.ESSendMessageToNcts();
			}
		}

		class ESNctsCommonMessagingMenuProviderForEditTest : ESNctsCommonMessagingMenuProvider
		{
			public ESNctsCommonMessagingMenuProviderForEditTest(NctsMessageFunctionSet messageFunction, NctsHeader header, ZBool hasInvalidCertificate, ZBool shouldEdit, NctsMovementForm nctsMovementForm) : base(messageFunction, header, nctsMovementForm)
			{
				this.shouldEdit = shouldEdit;
				this.hasInvalidCertificate = hasInvalidCertificate;
			}
			readonly ZBool shouldEdit;
			readonly ZBool hasInvalidCertificate;

			protected override MessageEditForm GetMessageEditForm()
				 => new MessageEditFormForTest();
			protected override bool GetShouldEditMessagePopUpResponse(DialogResult defaultValue) => base.GetShouldEditMessagePopUpResponse(shouldEdit ? DialogResult.Yes : DialogResult.No);
			protected override bool HasInvalidCertificate() => hasInvalidCertificate && base.HasInvalidCertificate();
		}

		class MessageEditFormForTest : MessageEditForm
		{
			public MessageEditFormForTest()
				: base()
			{
			}

			public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace(CodeToEdit, "AAAAAAAAAAAAA"), true);
		}
	}
}
