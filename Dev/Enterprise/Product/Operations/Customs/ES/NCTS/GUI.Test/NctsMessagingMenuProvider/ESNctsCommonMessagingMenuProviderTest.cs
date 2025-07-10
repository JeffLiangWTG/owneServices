using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.ES.NCTS.GUI.Testing.ESNctsDepartureMovementMessagingMenuProviderTest;
using static Enterprise.Customs.Universal.Constants;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class ESNctsCommonMessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestESSendMessageToNctsArrival_Phase4()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			nctsHeader.ArrivalMrnFromUser = CodeToEdit;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;

			nctsHeader.Factory.Save();

			CombineAssertions(() =>
			{
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.ESSendMessageToNcts();
				AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = ZString.Empty;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.ESSendMessageToNcts();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = "INVALID";
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.ESSendMessageToNcts();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.ESSendMessageToNcts();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				nctsHeader.Reload();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.ESSendMessageToNcts();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					commonMenuProvider.ESSendMessageToNcts();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
					AssertEquals("NctsHeader phase status is empty", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				}
			});
		}

		public void TestEditClearanceInfo()
		{
			var newEntryNumber = CreateOrUpdateCusEntryNumber(nctsHeader, NctsDeclarationTypeList.Codes.T1, "233445566", NctsMessageStatusList.Codes.DepartureDeclarationSent, ZDateTime.Today);
			var clearanceInfo = ClearanceInfo.LoadNew(newEntryNumber);
			using (var editClearance = new EditClearanceInfoForm(clearanceInfo))
			{
				var clearanceNumberControl = (ZTextBox)(editClearance.Controls.Find("ClearanceNumber", true).Single());
				var clearanceDateControl = (ZDateEdit)(editClearance.Controls.Find("ClearanceDate", true).Single());
				var arrivalLimitDateControl = (ZDateEdit)(editClearance.Controls.Find("ArrivalLimitDate", true).Single());

				editClearance.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Clearance Number exist", true, clearanceNumberControl.Visible);
					AssertEquals("Clearance Date exist", true, clearanceDateControl.Visible);
					AssertEquals("Arrival Limit Date exist", true, arrivalLimitDateControl.Visible);
				});
			}
		}

		public void TestDownloadTAD_Arrival()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			nctsHeader.ArrivalMrnFromUser = CodeToEdit;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;

			nctsHeader.Factory.Save();

			CombineAssertions(() =>
			{
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = ZString.Empty;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = "INVALID";
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				nctsHeader.Reload();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDownloadTAD_Departure()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.DestinationCustomsOfficeCodeForDeparture = "ES009999";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;

			nctsHeader.Factory.Save();

			CombineAssertions(() =>
			{
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = ZString.Empty;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = "INVALID";
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				nctsHeader.Reload();
				commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
				commonMenuProvider.DownloadTAD();
				AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
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

		[RequiresSTA]
		public void TestESSendMessageToNctsArrival_Phase4_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
				nctsHeader.ArrivalMrnFromUser = CodeToEdit;
				nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
				nctsHeader.DestinationTrader.OrganisationPK = org.PK;

				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
						AssertEquals("NctsHeader phase status is empty", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_Phase);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestESSendMessageToNctsDeparture_Phase4()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = CodeToEdit;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.GetEffectiveGuarantees().AddNew();

				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.NctsDeparture, msg.EM_MessageType);
						AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestESSendMessageToNctsDeparture_Phase4_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = CodeToEdit;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
				nctsHeader.GetEffectiveGuarantees().AddNew();

				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.NctsDeparture, msg.EM_MessageType);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		public void TestESSendMessageToNctsDeparture_Phase5()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = CodeToEdit;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.EffectiveMessageStatus);
						AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
						AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		public void TestESSendMessageToNctsDeparture_Phase5_PreSendValidation()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = CodeToEdit;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertValidationMessage(
						"Cannot send Pre-Declaration if status is Sent",
						("DPD", LogicalStatusList.Codes.Sent, ZString.Empty, ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendPreDeclarationMessage);

					AssertValidationMessage(
						"Cannot send Pre-Declaration if Customs status is not empty",
						("DPD", ZString.Empty, "CO1", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendPreDeclarationMessage);

					AssertValidationMessage(
						"Cannot send Pre-Declaration if phase is TNN",
						("DPD", ZString.Empty, ZString.Empty, "TNN"),
						ESNctsCommonMessagingMenuProvider.CannotSendPreDeclarationMessage);

					AssertValidationMessage("Cannot send Amendment if status is Sent",
						("DPM", LogicalStatusList.Codes.Sent, "PRE", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendAmendmentMessage);

					AssertValidationMessage(
						"Cannot send Amendment if Customs status is not Pre-Lodged",
						("DPM", ZString.Empty, "ACS", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendAmendmentMessage);

					AssertValidationMessage(
						"Cannot send Cancellation if status is Sent",
						("DPC", LogicalStatusList.Codes.Sent, "PRE", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendCancellationMessage);

					AssertValidationMessage(
						"Cannot send Cancellation if status is not Pre-Lodged or Pending Acceptance",
						("DPC", ZString.Empty, "ACS", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendCancellationMessage);

					AssertValidationMessage(
						"Cannot send Annexes if status is Sent",
						("DPA", LogicalStatusList.Codes.Sent, "CO1", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendAnnexesMessage);

					AssertValidationMessage(
						"Cannot send Annexes if status is not Decision to Control",
						("DPA", ZString.Empty, "ACS", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendAnnexesMessage);

					AssertValidationMessage(
						"Cannot send Notification of Goods if status is Sent",
						("DPN", LogicalStatusList.Codes.Sent, "PRE", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendNotificationOfGoodsMessage);

					AssertValidationMessage(
						"Cannot send Notification of Goods if status is not Pre-Lodged",
						("DPN", ZString.Empty, "ACS", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendNotificationOfGoodsMessage);

					SetupPreDeclarationChanges();
					AssertValidationMessage(
						"Cannot send Notification of Goods if there are changes in Pre-Declaration",
						("DPN", ZString.Empty, "PRE", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.ChangesInPreDeclarationMessage(ZString.Empty), partialMessage: true);
				});
			}

			void AssertValidationMessage(string assertMessage, (ZString MessageType, ZString MessageStatus, ZString CustomsStatus, ZString Phase) testCase, string expectedValidationMessage, bool partialMessage = false)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				nctsHeader.CommonMovementHeader.BM_MessageStatus = testCase.MessageStatus;
				nctsHeader.MovementHeader.BM_CustomsStatus = testCase.CustomsStatus;
				nctsHeader.MovementHeader.BM_Phase = testCase.Phase;

				nctsHeader.Factory.Save();
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false, testCase.MessageType);

				commonMenuProvider.ESSendMessageToNcts();

				if (partialMessage)
				{
					Assert(assertMessage, UnitTestUserNotification.Instance.LastMessage.Contains(expectedValidationMessage));
				}
				else
				{
					AssertEquals(assertMessage, expectedValidationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				Assert($"Message not sent: {assertMessage}", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Message sent successfully."));
			}

			void SetupPreDeclarationChanges()
			{
				var sentGuid = ZGuid.NewZGuid();
				var messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:CC013CV1Ent Id=""ES2409301040479008"" xmlns:q1=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC013CV1Ent.xsd"">
  <q1:CC013C>
    <q1:messageSender>ESA78587268</q1:messageSender>
    <q1:messageRecipient>NTA.ES</q1:messageRecipient>
    <q1:preparationDateAndTime>2024-09-30T10:40:47</q1:preparationDateAndTime>
    <q1:messageIdentification>63799</q1:messageIdentification>
    <q1:messageType>CC013C</q1:messageType>
    <q1:TransitOperation>
      <q1:MRN>24ES009999501557J1</q1:MRN>
      <q1:declarationType>T1</q1:declarationType>
      <q1:additionalDeclarationType>D</q1:additionalDeclarationType>
      <q1:security>0</q1:security>
      <q1:reducedDatasetIndicator>0</q1:reducedDatasetIndicator>
      <q1:bindingItinerary>0</q1:bindingItinerary>
      <q1:amendmentTypeFlag>0</q1:amendmentTypeFlag>
    </q1:TransitOperation>
    <q1:Consignment>
      <q1:countryOfDestination>ES</q1:countryOfDestination>
      <q1:containerIndicator>0</q1:containerIndicator>
      <q1:inlandModeOfTransport>3</q1:inlandModeOfTransport>
      <q1:grossMass>2.000</q1:grossMass>
      <q1:referenceNumberUCR>REF</q1:referenceNumberUCR>
    </q1:Consignment>
  </q1:CC013C>
</q1:CC013CV1Ent>";

				var message = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
				message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
				message.EM_SystemCreateTimeUtc = DateTime.Now;
				message.EM_GB = Env.CurrentBranchPK;

				var responseInterchange = Factory.New<EDIInterchange>();
				responseInterchange.EI_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage;
				responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				responseInterchange.EI_SessionGUID = ZGuid.NewZGuid();
				responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
				responseInterchange.EI_To = "CW1";
				responseInterchange.EI_HeaderText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AH</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>10</SentEDIMessageNumber>
</Headers>";
				responseInterchange.ContainedMessages.Add(message);

				nctsHeader.Messages.Add(message);

				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				message.EM_EI = interchange.PK;
				interchange.EI_SessionGUID = sentGuid;

				var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				sentInterchange.EI_SessionGUID = sentGuid;
				sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				var sentMessage = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
				sentMessage.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
				sentMessage.EM_EI = sentInterchange.PK;
				sentMessage.EM_GB = Env.CurrentBranchPK;
				sentMessage.EM_MessageText = messageText;
			}
		}

		public void TestESSendMessageToNctsArrival_Phase5_PreSendValidation()
		{
			const string validMRN = "ESAAAABBBBCCCCDDDD";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.ArrivalGoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = "Text";

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertValidationMessage(
						"Cannot send Unloading Remarks if status is Sent",
						("ADG", LogicalStatusList.Codes.Sent, "UAP", validMRN),
						ESNctsCommonMessagingMenuProvider.CannotSendUnloadingRemarksMessage);

					AssertValidationMessage(
						"Cannot send Unloading Remarks if customs status is not Unload Permission Granted (UAP)",
						("ADG", ZString.Empty, "ACS", validMRN),
						ESNctsCommonMessagingMenuProvider.CannotSendUnloadingRemarksMessage);

					AssertValidationMessage(
						"Cannot send Unloading Remarks if there is no MRN",
						("ADG", ZString.Empty, "UAP", ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendUnloadingRemarksMessage);

					AssertValidationMessage(
						"Cannot send TNN if there is no linked TNN Departure",
						("TNN", ZString.Empty, ZString.Empty, ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendTNNMessage);

					AddLinkedTNNDeparture("MRN");
					AssertValidationMessage(
						"Cannot send TNN if linked TNN Departure customs status is MRN Allocated",
						("TNN", ZString.Empty, ZString.Empty, ZString.Empty),
						ESNctsCommonMessagingMenuProvider.CannotSendTNNMessage);
				});
			}

			void AssertValidationMessage(string assertMessage, (ZString MessageType, ZString MessageStatus, ZString CustomsStatus, ZString MRN) testCase, string expectedValidationMessage)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				nctsHeader.CommonMovementHeader.BM_MessageStatus = testCase.MessageStatus;
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = testCase.CustomsStatus;
				nctsHeader.ArrivalMrnFromUser = testCase.MRN;

				nctsHeader.Factory.Save();
				var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false, testCase.MessageType);

				commonMenuProvider.ESSendMessageToNcts();

				AssertEquals(assertMessage, expectedValidationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert($"Message not sent: {assertMessage}", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Message sent successfully."));
			}

			void AddLinkedTNNDeparture(ZString customsStatus)
			{
				var nctsHeaderTNN = Factory.New<NctsHeader>();
				nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;

				var tnnMovement = nctsHeaderTNN.MovementHeader;
				tnnMovement.BM_CustomsStatus = customsStatus;

				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;
			}
		}

		public void TestESSendMessageToNctsArrival_Phase5()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
				nctsHeader.ArrivalMrnFromUser = CodeToEdit;
				nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
				nctsHeader.DestinationTrader.OrganisationPK = org.PK;
				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.EffectiveMessageStatus);
						AssertEquals("NctsHeader phase status is 007", ESNctsMovementHeaderTransactionStatusList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, msg.EM_MessageType);
						AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestESSendMessageToNctsDeparture_Phase5_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = CodeToEdit;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.EffectiveMessageStatus);
						AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestESSendMessageToNctsArrival_Phase5_EditMessageText()
		{
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
				nctsHeader.ArrivalMrnFromUser = CodeToEdit;
				nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
				nctsHeader.DestinationTrader.OrganisationPK = org.PK;
				nctsHeader.Factory.Save();

				CombineAssertions(() =>
				{
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, true);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						nctsHeader.Reload();
						commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, true);
						TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
						commonMenuProvider.ESSendMessageToNcts();
						TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
						AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
						AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.EffectiveMessageStatus);
						AssertEquals("NctsHeader phase status is 007", ESNctsMovementHeaderTransactionStatusList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
						var msg = nctsHeader.Messages.LastOutgoingMessage;
						AssertNotNull("EDIMessage was created for the ncts header", msg);
						AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, msg.EM_MessageType);
						AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestFillDeclarationGoodsItemNumberOnSending_Departure()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = "Text";

			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertEquals("BY_DeclarationGoodsItemNumber is not set before sending", 0, goodsItem.BY_DeclarationGoodsItemNumber);
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("BY_DeclarationGoodsItemNumber is changed after sending", 1, goodsItem.BY_DeclarationGoodsItemNumber);
				});
			}
		}

		[RequiresSTA]
		public void TestFillDeclarationGoodsItemNumberOnSending_Arrival()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.ArrivalGoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = "Text";

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertEquals("BY_DeclarationGoodsItemNumber is not set before sending", 0, goodsItem.BY_DeclarationGoodsItemNumber);
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("BY_DeclarationGoodsItemNumber is changed after sending", 1, goodsItem.BY_DeclarationGoodsItemNumber);
				});
			}
		}

		public void TestFillDeclarationGoodsItemNumberOnSending_DepartureTNN()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			CreateEntryNumber(nctsHeaderTNN);
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			var consignment = nctsHeader.ArrivalMovementHeader.HeaderTNN.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = "Text";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertEquals("BY_DeclarationGoodsItemNumber is not set before sending", 0, goodsItem.BY_DeclarationGoodsItemNumber);
					var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
					commonMenuProvider.ESSendMessageToNcts();
					AssertEquals("BY_DeclarationGoodsItemNumber is changed after sending", 1, goodsItem.BY_DeclarationGoodsItemNumber);
				});
			}
		}

		public void TestFillSupportingDocumentsSequenceNumberOnSending_Departure()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				var doc1 = goodsItem.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";

				var doc3 = consignment.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = consignment.SupportingDocuments.AddNew();
				doc4.CSI_Code = "C004";

				var doc5 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc5.CSI_Code = "A005";

				var doc6 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc6.CSI_Code = "9006";

				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
						AssertContainsExactElementsInAnyOrder("Prereq: nctsHeader has 2 documents with codes", new ZString[] { "A005", "9006" }, nctsHeader.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: consignment has 2 documents with codes", new ZString[] { "9003", "C004" }, consignment.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: goodsItem has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem.SupportingDocuments.Select(x => x.CSI_Code));

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertEquals("doc3 is deleted because it was in bill", true, doc3.IsDeleted);
						AssertEquals("doc4 is deleted because it was in bill", true, doc4.IsDeleted);
						AssertEquals("doc5 is deleted because it was in header", true, doc5.IsDeleted);
						AssertEquals("doc6 is deleted because it was in header", true, doc6.IsDeleted);

						AssertEquals("When declaration is Departure Phase5 Header has 0 documents", 0, nctsHeader.MovementHeader.SupportingDocuments.Count);
						AssertEquals("When declaration is Departure Phase5 consignment has 0 documents", 0, consignment.SupportingDocuments.Count);
						var goodsItem11SupDocs = goodsItem.SupportingDocuments;
						AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem has 6 documents with codes", new ZString[] { "A005", "C004", "A002", "9006", "9003", "9001" }, goodsItem11SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6 }, goodsItem11SupDocs.Select(x => x.CSI_LineNo));
					});
				}
			}
		}

		public void TestFillSupportingDocumentsSequenceNumberOnSending_Arrival()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.ArrivalGoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();

				var doc1 = goodsItem.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";

				var doc3 = consignment.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = consignment.SupportingDocuments.AddNew();
				doc4.CSI_Code = "C004";

				var doc5 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				doc5.CSI_Code = "A005";

				var doc6 = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
				doc6.CSI_Code = "9006";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 2", 2, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 1", 1, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 2", 2, doc6.CSI_LineNo);
						AssertContainsExactElementsInAnyOrder("Prereq: ArrivalMovementHeader has 2 documents with codes", new ZString[] { "A005", "9006" }, nctsHeader.ArrivalMovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: consignment has 2 documents with codes", new ZString[] { "9003", "C004" }, consignment.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: goodsItem has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem.SupportingDocuments.Select(x => x.CSI_Code));

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertEquals("When declaration is Arrival Phase5 ArrivalMovementHeader has 2 documents", 2, nctsHeader.ArrivalMovementHeader.SupportingDocuments.Count);
						var goodsItem11SupDocs = goodsItem.SupportingDocuments;

						AssertEquals("After: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
						AssertEquals("After: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
						AssertEquals("After: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
						AssertEquals("After: doc4 has CSI_LineNo 2", 2, doc4.CSI_LineNo);
						AssertEquals("After: doc5 has CSI_LineNo 1", 1, doc5.CSI_LineNo);
						AssertEquals("After: doc6 has CSI_LineNo 2", 2, doc6.CSI_LineNo);
						AssertEquals("doc1 is deleted because it was in bill", false, doc1.IsDeleted);
						AssertEquals("doc2 is deleted because it was in bill", false, doc2.IsDeleted);
						AssertEquals("doc3 is deleted because it was in consignment", false, doc3.IsDeleted);
						AssertEquals("doc4 is deleted because it was in consignment", false, doc4.IsDeleted);
						AssertEquals("doc5 is deleted because it was in ArrivalMovementHeader", false, doc5.IsDeleted);
						AssertEquals("doc6 is deleted because it was in ArrivalMovementHeader", false, doc6.IsDeleted);
					});
				}
			}
		}

		public void TestFillSupportingDocumentsSequenceNumberOnSending_DepartureTNN()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				CreateEntryNumber(nctsHeader);
				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				var nctsHeaderTNN = Factory.New<NctsHeader>();
				nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
				CreateEntryNumber(nctsHeaderTNN);
				var tnnMovement = nctsHeaderTNN.MovementHeader;
				tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

				nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				var consignment = nctsHeader.ArrivalMovementHeader.HeaderTNN.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				var doc1 = goodsItem.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";

				var doc3 = consignment.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = consignment.SupportingDocuments.AddNew();
				doc4.CSI_Code = "C004";

				var doc5 = nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.SupportingDocuments.AddNew();
				doc5.CSI_Code = "A005";

				var doc6 = nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.SupportingDocuments.AddNew();
				doc6.CSI_Code = "9006";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
						AssertContainsExactElementsInAnyOrder("Prereq: nctsHeader has 2 documents with codes", new ZString[] { "A005", "9006" }, nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: consignment has 2 documents with codes", new ZString[] { "9003", "C004" }, consignment.SupportingDocuments.Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("Prereq: goodsItem has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem.SupportingDocuments.Select(x => x.CSI_Code));

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertEquals("doc3 is deleted because it was in bill", true, doc3.IsDeleted);
						AssertEquals("doc4 is deleted because it was in bill", true, doc4.IsDeleted);
						AssertEquals("doc5 is deleted because it was in header", true, doc5.IsDeleted);
						AssertEquals("doc6 is deleted because it was in header", true, doc6.IsDeleted);

						AssertEquals("When declaration is Departure TNN Phase5 Header has 0 documents", 0, nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.SupportingDocuments.Count);
						AssertEquals("When declaration is Departure TNN Phase5 consignment has 0 documents", 0, consignment.SupportingDocuments.Count);
						var goodsItem11SupDocs = goodsItem.SupportingDocuments;
						AssertContainsExactElementsInExactOrder("When declaration is Departure TNN Phase5 goodsItem has 6 documents with codes", new ZString[] { "A005", "C004", "A002", "9006", "9003", "9001" }, goodsItem11SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
						AssertContainsExactElementsInAnyOrder("When declaration is Departure TNN Phase5 goodsItem has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6 }, goodsItem11SupDocs.Select(x => x.CSI_LineNo));
					});
				}
			}
		}

		[RequiresSTA]
		public void TestFillAdditionalDocumentsSequenceNumberOnSending_Departure()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				var doc1 = goodsItem.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "INF";

				var doc3 = consignment.AdditionalDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "INF";

				var doc4 = consignment.AdditionalDocuments.AddNew();
				doc4.CSI_Code = "C004";
				doc4.CSI_SubType = "INF";

				var doc5 = nctsHeader.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "A005";
				doc5.CSI_SubType = "INF";

				var doc6 = nctsHeader.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "INF";

				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc5.CSI_LineNo });
						AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in consignment have sequences 3, 4", new ZInt[] { 3, 4 }, new ZInt[] { doc4.CSI_LineNo, doc3.CSI_LineNo });
						AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in goodsItem have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					});
				}
			}
		}

		public void TestFillAdditionalDocumentsSequenceNumberOnSending_Arrival()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				var consignment = nctsHeader.Bills.AddNew();
				var goodsItem = consignment.ArrivalGoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();

				var doc1 = goodsItem.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "INF";

				var doc3 = consignment.AdditionalDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "INF";

				var doc4 = consignment.AdditionalDocuments.AddNew();
				doc4.CSI_Code = "C004";
				doc4.CSI_SubType = "INF";

				var doc5 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "A005";
				doc5.CSI_SubType = "INF";

				var doc6 = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "INF";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 2", 2, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 1", 1, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 2", 2, doc6.CSI_LineNo);

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertEquals("After: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
						AssertEquals("After: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
						AssertEquals("After: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
						AssertEquals("After: doc4 has CSI_LineNo 2", 2, doc4.CSI_LineNo);
						AssertEquals("After: doc5 has CSI_LineNo 1", 1, doc5.CSI_LineNo);
						AssertEquals("After: doc6 has CSI_LineNo 2", 2, doc6.CSI_LineNo);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestFillAdditionalDocumentsSequenceNumberOnSending_DepartureTNN()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				CreateEntryNumber(nctsHeader);
				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				var nctsHeaderTNN = Factory.New<NctsHeader>();
				nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
				CreateEntryNumber(nctsHeaderTNN);
				var tnnMovement = nctsHeaderTNN.MovementHeader;
				tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

				nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				var consignment = nctsHeader.ArrivalMovementHeader.HeaderTNN.Bills.AddNew();
				var goodsItem = consignment.GoodsItems.AddNew();
				goodsItem.BY_Type = "A";
				goodsItem.BY_Description = "Text";

				var doc1 = goodsItem.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "INF";

				var doc3 = consignment.AdditionalDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "INF";

				var doc4 = consignment.AdditionalDocuments.AddNew();
				doc4.CSI_Code = "C004";
				doc4.CSI_SubType = "INF";

				var doc5 = nctsHeader.ArrivalMovementHeader.HeaderTNN.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "A005";
				doc5.CSI_SubType = "INF";

				var doc6 = nctsHeader.ArrivalMovementHeader.HeaderTNN.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "INF";

				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

				nctsHeader.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
						AssertEquals("Prereq: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
						AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
						AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
						AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
						AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);

						var commonMenuProvider = new ESNctsCommonMessagingMenuProviderForEditTest(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, false);
						commonMenuProvider.ESSendMessageToNcts();

						AssertContainsExactElementsInAnyOrder("When declaration is Departure TNN Phase5 documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc5.CSI_LineNo });
						AssertContainsExactElementsInAnyOrder("When declaration is Departure TNN Phase5 documents in consignment have sequences 3, 4", new ZInt[] { 3, 4 }, new ZInt[] { doc4.CSI_LineNo, doc3.CSI_LineNo });
						AssertContainsExactElementsInAnyOrder("When declaration is Departure TNN Phase5 documents in goodsItem have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					});
				}
			}
		}

		void CreateEntryNumber(NctsHeader header)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "ES239928883";
		}

		public void TestLaunchCustomsWebsiteArrival()
		{
			AssertLaunchCustomsWebsite(NctsMovementType.Codes.Arrival);
		}

		public void TestLaunchCustomsWebsite_NctsDepartureAndArrival()
		{
			AssertLaunchCustomsWebsite(NctsMovementType.Codes.DepartureAndArrival);
		}

		public void TestLaunchCustomsWebsite_NctsDeparture()
		{
			AssertLaunchCustomsWebsite(NctsMovementType.Codes.Departure);
		}

		void AssertLaunchCustomsWebsite(ZString nctsMovementType)
		{
			var expectedMRN = "AACCRRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;

			nctsHeader.SetMovementType(nctsMovementType);
			if (nctsHeader.IsArrivalMovement)
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			}

			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);

			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				commonMenuProvider.LaunchCustomsWebsite();
				AssertNullOrEmpty("No url was launched when Header has no MRN", WebUrlLauncher.LastUrlLaunched);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = expectedMRN;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				commonMenuProvider.LaunchCustomsWebsite();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			});
		}

		public void TestCreateEXSDeclarationMessage_Phase4()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var arrivalMovementHeaderGoodsItemNew = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			var lineNew = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			lineNew.BY_LineNo = arrivalMovementHeaderGoodsItemNew.BY_LineNo;
			lineNew.HasDifferences = false;
			lineNew.IsMissing = false;
			lineNew.IsNew = true;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);
			commonMenuProvider.CreateEXSDeclaration();

			AssertEquals("Message when create EXS declaration with arrival", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration with number"));
		}

		public void TestCreateEXSDeclarationMessage_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);
			commonMenuProvider.CreateEXSDeclaration();

			AssertEquals("Message when create EXS declaration with arrival", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration with number"));
		}

		public void TestCreateEXSDeclarationMessageError_Phase4()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var arrivalMovementHeaderGoodsItemNew = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			var lineNew = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			lineNew.BY_LineNo = arrivalMovementHeaderGoodsItemNew.BY_LineNo;
			lineNew.HasDifferences = false;
			lineNew.IsMissing = true;
			lineNew.IsNew = false;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);
			commonMenuProvider.CreateEXSDeclaration();

			AssertEquals("Error message when create EXS declaration", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration could not be created"));
		}

		public void TestCreateEXSDeclarationMessageError_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);
			commonMenuProvider.CreateEXSDeclaration();

			AssertEquals("Error message when create EXS declaration", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration could not be created"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.FillWithValidTestData();

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address";
			org.Contacts.AddNew();

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		}
		NctsHeader nctsHeader;
		OrgHeader org;

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

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = nctsHeader.Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			nctsHeader.Factory.Save();
			return office;
		}

		class ESNctsCommonMessagingMenuProviderForEditTest : ESNctsCommonMessagingMenuProvider
		{
			public ESNctsCommonMessagingMenuProviderForEditTest(NctsMessageFunctionSet messageFunction, NctsHeader header, ZBool shouldEdit, ZString? messageType = null) : base(messageFunction, header)
			{
				this.shouldEdit = shouldEdit;
				this.messageType = messageType;
			}
			readonly ZBool shouldEdit;
			readonly ZString? messageType;

			protected override MessageEditForm GetMessageEditForm()
				 => new MessageEditFormForTest();
			protected override MessageSendingForm GetMessageSendingForm(Business.NctsHeaderMessageSendingObjectParent sendingObjectParent) => new SendFormForTest(sendingObjectParent, messageType);
			protected override bool GetShouldEditMessagePopUpResponse(DialogResult defaultValue) => base.GetShouldEditMessagePopUpResponse(shouldEdit ? DialogResult.Yes : DialogResult.No);
		}

		class MessageEditFormForTest : MessageEditForm
		{
			public MessageEditFormForTest() : base() { }

			public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace(CodeToEdit, "AAAAAAAAAAAAA"), true);
		}

		class SendFormForTest : MessageSendingForm
		{
			public SendFormForTest(Business.NctsHeaderMessageSendingObjectParent sendingObjectParent, ZString? messageType = null)
				: base(sendingObjectParent)
			{
				foreach (Business.NctsHeaderMessageSendingObject sendingObject in sendingObjectParent.SendingObjectsCollection)
				{
					if (messageType.HasValue)
					{
						sendingObject.MessageType = messageType.Value;
					}
					sendingObject.ReasonForCancellation = "Reason for\r\nCancellation";
				}
			}
		}
	}
}
