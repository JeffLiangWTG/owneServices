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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class ESNctsDepartureMovementMessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestNewNctsDepartureMovementMessagingMenuProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				AssertType(typeof(ESNctsDepartureMovementMessagingMenuProvider), new ESNctsDepartureMovementMessagingMenuProvider(header, nctsMovementForm));
			}
		}

		public void TestSendMessageToNctsCore()
		{
			SetRefData();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Description = GoodsDescription;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Estonia;
				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsHeader.Consignee.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					nctsMovementForm.Show();
					var menu = new ESNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm, true, false);
					var menuItems = menu.CreateMenuItems().ToArray();
					var depMenuItem = menuItems.FindByText("Send Departure Message");

					var declarationTypeDropEdit = nctsMovementForm.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

					CombineAssertions(() =>
					{
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						nctsHeader.BH_CustomsProfile = ZString.Empty;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = "INVALID";
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("When the declaration has not been sent, MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);

						menu = new ESNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm, false, false);
						menuItems = menu.CreateMenuItems().ToArray();
						depMenuItem = menuItems.FindByText("Send Departure Message");
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						depMenuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\n\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

						AssertEquals("When the declaration has been sent, MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestEditClearanceInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var newEntryNumber = CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, "AAAAA", NctsMessageStatusList.Codes.DepartureDeclarationSent, ZDateTime.Today);
				CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
				var clearanceInfo = ClearanceInfo.LoadNew(newEntryNumber);

				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				clearanceInfo.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					nctsMovementForm.Show();
					var menu = new ESNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm, false, false);
					var menuItems = menu.CreateMenuItems().ToArray();
					var editMenuItem = menuItems.FindByText("Update Clearance info");

					var declarationTypeDropEdit = nctsMovementForm.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

					CombineAssertions(() =>
					{
						menu.clearanceNumber = ZString.Empty;
						menu.clearanceDate = ZDateTime.Today;
						menu.arrivalLimitDate = ZDateTime.Today.AddDays(4);
						menu.RefreshMenu();
						editMenuItem.PerformClick();
						AssertEquals("Message when Clearance Number is empty", "The Clearance Number must not be empty", UnitTestUserNotification.Instance.LastMessage.Text);

						menu.clearanceNumber = "AAaa11222-";
						menu.clearanceDate = ZDateTime.Today;
						menu.arrivalLimitDate = ZDateTime.Today.AddDays(4);
						menu.RefreshMenu();
						editMenuItem.PerformClick();
						AssertEquals("Message when Clearance Number Format is incorrect", "AAaa11222- format is incorrect. Please fill with a correct Clearance Number", UnitTestUserNotification.Instance.LastMessage.Text);

						menu.clearanceNumber = ClearanceNumber;
						menu.clearanceDate = ZDateTime.Today;
						menu.arrivalLimitDate = ZDateTime.Empty;
						menu.RefreshMenu();
						editMenuItem.PerformClick();
						AssertEquals("Message when Clearance Number is empty", "The arrival limit date must not be empty", UnitTestUserNotification.Instance.LastMessage.Text);

						menu.clearanceNumber = ClearanceNumber;
						menu.clearanceDate = ZDateTime.Empty;
						menu.arrivalLimitDate = ZDateTime.Today.AddDays(4);
						menu.RefreshMenu();
						editMenuItem.PerformClick();
						AssertEquals("Message when Clearance Number is empty", "The clearance date date must not be empty", UnitTestUserNotification.Instance.LastMessage.Text);

						menu.clearanceNumber = ClearanceNumber;
						menu.clearanceDate = ZDateTime.Today;
						menu.arrivalLimitDate = ZDateTime.Today.AddDays(-1);
						menu.RefreshMenu();
						editMenuItem.PerformClick();
						AssertEquals("Message when Clearance Number is empty", "Arrival limit date must be later than Clearance date", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						menu.clearanceNumber = ClearanceNumber;
						menu.clearanceDate = ZDateTime.Today.AddDays(1);
						menu.arrivalLimitDate = ZDateTime.Today.AddDays(4);
						menu.RefreshMenu();
						TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", nctsHeader.Factory);
						editMenuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After document capture", nctsHeader.Factory);

						nctsHeader.Reload();

						var cusEntryNumberSelected = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
						AssertEquals("Clearance number has been changed when the pop up was accepted", ClearanceNumber, cusEntryNumberSelected.CE_EntryNum);
						AssertEquals("Clearance date has been changed when the pop up was accepted", ZDateTime.Today.AddDays(1), cusEntryNumberSelected.CE_IssueDate);
						AssertEquals("Arrival limit date has been changed when the pop up was accepted", ZDateTime.Today.AddDays(4), cusEntryNumberSelected.CE_ExpiryDate);

						AssertEquals("Clearance number New event in logs", "|NEW=" + ClearanceNumber + "|OLD=AAAAA|RES=Manually Modify Clearance Number to Entry NCT00000001|TYP=CLR", nctsHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=" + ClearanceNumber)).SL_Reference);
						AssertEquals("Clearance date New event in logs", "|NEW=" + ZDateTime.Today.AddDays(1) + "|OLD=" + ZDateTime.Today + "|RES=Manually Modify Clearance Date to Entry NCT00000001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=" + ZDateTime.Today.AddDays(1).ToString())).SL_Reference);
						AssertEquals("Arrival limit date New event in logs", "|NEW=" + ZDateTime.Today.AddDays(4) + "|OLD=" + ZDateTime.Today.AddDays(5) + "|RES=Manually Modify Arrival Limit Date to Entry NCT00000001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=" + ZDateTime.Today.AddDays(4).ToString())).SL_Reference);

						AssertContains("Updating Clearance number triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestSendMessageToNctsCore_EditMessageText()
		{
			SetRefData();

			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Description = GoodsDescription;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Estonia;
				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsHeader.Consignee.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					nctsMovementForm.Show();
					var menu = new ESNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm, true, true);
					var menuItems = menu.CreateMenuItems().ToArray();
					var depMenuItem = menuItems.FindByText("Send Departure Message");

					var declarationTypeDropEdit = nctsMovementForm.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

					CombineAssertions(() =>
					{
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						nctsHeader.BH_CustomsProfile = ZString.Empty;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = "INVALID";
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						nctsHeader.Factory.Save();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						menu.RefreshMenu();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("When the declaration has not been sent, MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);

						menu = new ESNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm, false, true);
						menuItems = menu.CreateMenuItems().ToArray();
						depMenuItem = menuItems.FindByText("Send Departure Message");
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						depMenuItem.PerformClick();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\n\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

						AssertEquals("When the declaration has been sent, MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestSendMessageToNctsCore_ParentFormShipment()
		{
			SetRefData();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Description = GoodsDescription;

				var packages = goodsItem.Packages.AddNew();
				packages.B5_UnitType = packageType;
				packages.B5_UnitCount = 1;
				packages.B5_MarksAndNumbers = "Mark and Numbers";

				goodsItem.BY_GrossWeight = 100;
				goodsItem.BY_NetWeight = 100;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Estonia;
				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "Code";
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
				var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				guarantee.PW_BondType = "8";
				nctsHeader.Consignee.E2_OA_Address = org.MainAddress.PK;

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
					var depMenuItem = menuItems.FindByText("Send Departure Message");

					var declarationTypeDropEdit = frm.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

					CombineAssertions(() =>
					{
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
						nctsHeader.BH_CustomsProfile = ZString.Empty;
						nctsHeader.Factory.Save();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = "INVALID";
						nctsHeader.Factory.Save();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
						nctsHeader.Factory.Save();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						depMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
						{
							AssertEquals("When the declaration has not been sent, MainTabPage should be unlocked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);

							TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
							depMenuItem.PerformClick();
							TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
							AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\n\nDo you want to edit the EDI Messages generated by this operation? Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\n\nDo you want to edit the EDI Messages generated by this operation?"));
							AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
							var msg = nctsHeader.Messages.LastOutgoingMessage;
							AssertNotNull("EDIMessage was created for the ncts header", msg);
							AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

							AssertEquals("When the declaration has been sent, MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
						}
					});
				}
			}
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

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType,
				"Desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "1");
			Factory.Save();
		}

		const string packageType = "CT";

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

		const string GoodsDescription = "Description";

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

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = nctsHeader.Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			nctsHeader.Factory.Save();
			return office;
		}

		public void TestViewOnCustomsWebsite_NctsDeparture()
		{
			var expectedMRN = "AACCRRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = nctsHeader;
				var menuItem = menu.MenuItems.FindByText("View on Customs Website");

				ZFormModaliser.ShowDialogsInTest = false;
				WebUrlLauncher.ClearLastUrlLaunched();

				CombineAssertions(() =>
				{
					menuItem.PerformClick();
					AssertNullOrEmpty("No url was launched when Departure has no MRN", WebUrlLauncher.LastUrlLaunched);

					CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, expectedMRN, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
					menuItem.PerformClick();
					AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
					WebUrlLauncher.ClearLastUrlLaunched();
				});
			}
		}

		public void TestMenu_DepartureVisibility_ViewOnCustomsWebsite()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				var menuES = new ESNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = menuES.CreateMenuItems().ToArray();
				var viewOnWebsite = menuItems.FindByText("View on Customs Website");

				CombineAssertions(() =>
				{
					menuES.RefreshMenu();
					AssertEquals("View On Website MenuItem is not visible when Departure has no MRN", false, viewOnWebsite.Visible);

					CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "AACCRRRRRRNNNNNNNN", ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
					menuES.RefreshMenu();
					AssertEquals("View On Website MenuItem is visible when Deaprture has MRN", true, viewOnWebsite.Visible);
				});
			}
		}

		protected CusEntryNumber CreateOrUpdateCusEntryNumber(NctsHeader header, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_IssueDate = issueDate;
			newEntryNumber.CE_ExpiryDate = issueDate.AddDays(5);
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			return newEntryNumber;
		}

		#region Document Requests

		#region NCTSDeparture

		public void TestMenuItemVisibility_UpdateClearanceInfo()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var menu = new ESNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = menu.CreateMenuItems().ToArray();
				var downloadClearanceInfoMenuItem = menuItems.FindByText("Update Clearance info");

				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					AssertEquals("Update Clearance info, MenuItem is not visible when header has no MRN (Departure)", false, downloadClearanceInfoMenuItem.Visible);

					CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
					menu.RefreshMenu();
					AssertEquals("Update Clearance info, MenuItem is visible when header has MRN (Departure)", true, downloadClearanceInfoMenuItem.Visible);
				});
			}
		}

		public void TestMenuItemVisibility_CanDownloadTADMenuItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var menu = new ESNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
				var menuItems = menu.CreateMenuItems().ToArray();
				var downloadTADMenuItem = menuItems.FindByText("Download TAD (Transit Accompanying Document)");

				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					AssertEquals("Download TAD (Transit Accompanying Document) MenuItem is not visible when header has no MRN", false, downloadTADMenuItem.Visible);

					CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
					menu.RefreshMenu();
					AssertEquals("Download TAD (Transit Accompanying Document) MenuItem is visible when header has MRN", true, downloadTADMenuItem.Visible);
				});
			}
		}

		public void TestDownloadTAD_NoDocumentNeeded()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "21ES00999912345678", "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
				CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, "ABCDEFGHIJKLMNOP", ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					var menu = new ESNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
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

		[RequiresSTA]
		public void TestDownloadTAD_DocumentNeeded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
				{
					var menu = new ESNctsDepartureMovementMessagingMenuProvider(nctsHeader, nctsMovementForm);
					var menuItems = menu.CreateMenuItems().ToArray();
					var downloadTADMenuItem = menuItems.FindByText("Download TAD (Transit Accompanying Document)");

					CombineAssertions(() =>
					{
						menu.RefreshMenu();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
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

		protected void CreateOrUpdateCusEntryNumber(NctsHeader header, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate, ZDateTime expiryDate)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_IssueDate = issueDate;
			newEntryNumber.CE_ExpiryDate = expiryDate;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
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

		const string ClearanceNumber = "AAaa11222";

		#endregion
		#endregion

		class ESNctsDepartureMovementMessagingMenuProviderForTest : ESNctsDepartureMovementMessagingMenuProvider
		{
			public ESNctsDepartureMovementMessagingMenuProviderForTest(NctsHeader header, NctsMovementForm nctsMovementForm, ZBool hasInvalidCertificate, ZBool shouldEdit) : base(header, nctsMovementForm)
			{
				this.shouldEdit = shouldEdit;
				this.hasInvalidCertificate = hasInvalidCertificate;
			}
			public ESNctsDepartureMovementMessagingMenuProviderForTest(NctsHeader header, EU.NCTS.DataTransfer.NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper, ZBool hasInvalidCertificate, ZBool shouldEdit) : base(header, ntcsHeaderUniversalMessagingHelper)
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

			public ZString clearanceNumber = ZString.Empty;
			public ZDateTime clearanceDate = ZDateTime.Empty;
			public ZDateTime arrivalLimitDate = ZDateTime.Empty;

			protected override EditClearanceInfoForm GetEditClearanceInfoForm(ClearanceInfo clearanceInfo)
				=> new EditClearanceInfoFormForTest(clearanceInfo, clearanceNumber, clearanceDate, arrivalLimitDate);
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

			public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace(GoodsDescription, "AAAAAAAAAAAAA"), true);
		}

		class EditClearanceInfoFormForTest : EditClearanceInfoForm
		{
			public EditClearanceInfoFormForTest(ClearanceInfo clearanceInfo, ZString clearanceNumber, ZDateTime clearanceDate, ZDateTime arrivalLimitDate)
				: base(clearanceInfo)
			{
				ClearanceNumber.Text = clearanceNumber;
				ClearanceDate.DateTimeValue = clearanceDate;
				ArrivalLimitDate.DateTimeValue = arrivalLimitDate;
				clearanceInfo.ClearanceNumber = clearanceNumber;
				clearanceInfo.ClearanceDate = clearanceDate;
				clearanceInfo.ArrivalLimitDate = arrivalLimitDate;
			}
		}
	}
}
