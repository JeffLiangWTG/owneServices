using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public class AFRMainMenuItem : ZMenuItem
	{
		public AFRMainMenuItem(JPAFRHeader header)
			: base(ResString.GetMultilingualString("JPAFRMainMenuItem|MainMenuItem", "A&FR"))
		{
			this.Header = header;

			if (Header != null && !Header.JPH_IsActive)
			{
				AddDeactivatesMenuItem();
			}
			else
			{
				AddRegisterManifestMenuItem();
				if (header != null && header.JPH_IsShippingLineEntry)
				{
					AddRegisterATDMenuItem();
				}
				else
				{
					AddBillRegistrationCompletionMenuItem();
				}
				AddAmendmentManifestMenuItem();
				if (Header != null && Header.IsAnyBillAlreadyRegistered)
				{
					if (Business.ValidationUtils.GetCurrentJPDate >= JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.Value)
					{
						AddSendBlanketVesselChangeMenuItem();
					}
					if (JPAFRRegistry.Instance.AFRShowBLLFunctions.Value)
					{
						AddBLLFunctionMenuItems();
					}
				}
			}
		}

		void AddBLLFunctionMenuItems()
		{
			bllFunctionMenuHelper = new BLLFuntionMenuHelper(() => Header, () => null);

			var registrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|BLL Function", "BLL Function"));
			registrationMenuItem.MenuItems.Add(0, bllFunctionMenuHelper.RegisterSplitMenuItem);
			registrationMenuItem.MenuItems.Add(1, bllFunctionMenuHelper.RegisterSwitchMenuItem);
			registrationMenuItem.MenuItems.Add(2, bllFunctionMenuHelper.RegisterMergeMenuItem);
			registrationMenuItem.MenuItems.Add(3, bllFunctionMenuHelper.CancelSplitMenuItem);
			registrationMenuItem.MenuItems.Add(4, bllFunctionMenuHelper.CancelSwitchMenuItem);
			registrationMenuItem.MenuItems.Add(5, bllFunctionMenuHelper.CancelMergeMenuItem);

			registrationMenuItem.Select += RegistrationMenuItem_Select;

			this.MenuItems.Add(registrationMenuItem);
		}

		void RegistrationMenuItem_Select(object sender, EventArgs e)
		{
			var bills = Header.Bills;
			bllFunctionMenuHelper.CancelSplitMenuItem.Visible = bills.Any(x =>
				x.IsBillAlreadyRegistered
				&& (x.BLLFunctionInfo?.JP_FunctionCode ?? -1) == (int)BLLFunctionCode.RegisterSplit);

			bllFunctionMenuHelper.CancelSwitchMenuItem.Visible = bills.Any(x =>
				x.IsBillAlreadyRegistered
				&& (x.BLLFunctionInfo?.JP_FunctionCode ?? -1) == (int)BLLFunctionCode.RegisterSwitch);

			bllFunctionMenuHelper.CancelMergeMenuItem.Visible = bills.Any(x =>
				x.IsBillAlreadyRegistered
				&& (x.BLLFunctionInfo?.JP_FunctionCode ?? -1) == (int)BLLFunctionCode.RegisterMerge);

			var registrationMenuVisibility = bills.Any(x =>
				x.IsBillAlreadyRegistered
				&& x.BLLFunctionInfo == null);

			bllFunctionMenuHelper.RegisterSplitMenuItem.Visible = registrationMenuVisibility;
			bllFunctionMenuHelper.RegisterSwitchMenuItem.Visible = registrationMenuVisibility;
			bllFunctionMenuHelper.RegisterMergeMenuItem.Visible = registrationMenuVisibility;
		}

		BLLFuntionMenuHelper bllFunctionMenuHelper;

		public JPAFRHeader Header
		{
			get;
			internal set;
		}

		void AddDeactivatesMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|HeaderDeactivated", "AFR Header deactivated, to reactivate go to Actions->Make Active"));
			this.MenuItems.Add(item);
		}

		#region MenuItem Section

		void AddRegisterManifestMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|RegisterManifest", "&Register Manifest"));
			item.Click += delegate
			{ ValidateAndDeclareAFR(ActionCode.Registering); };
			this.MenuItems.Add(item);
		}

		void AddBillRegistrationCompletionMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|BillRegistrationCompletion", "&Register Manifest Completion"));
			item.Click += delegate
			{ ValidateAndDeclareAFR(ActionCode.RegisterCompletionByRegistration); };
			this.MenuItems.Add(item);
		}

		void AddRegisterATDMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|RegisterDepartureTime", "&Register Departure Time"));
			item.Click += delegate
			{ ValidateAndDeclareAFR(ActionCode.RegisterDepartureTime); };
			this.MenuItems.Add(item);
		}

		void AddAmendmentManifestMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|AmendmentManifest", "&Amendment Manifest"));
			item.Click += delegate
			{ ValidateAndDeclareAFR(ActionCode.AmendingAdd); };
			this.MenuItems.Add(item);
		}

		void AddSendBlanketVesselChangeMenuItem()
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|SendBlanketVesselChange", "&Send Blanket Vessel Change"));
			item.Click += delegate
			{
				if (Header != null)
				{
					if (SaveData(Header))
					{
						if (Header.JPH_IsActive)
						{
							SendBlanketVesselChange();
						}
						else
						{
							Globals.Message.ShowError(ResString.GetMultilingualString("JPAFRMainMenuItem|HeaderDeactivatedForSending",
								"Messages can not be sent when AFR Header is deactivated, to reactivate go to Actions->Make Active"));
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Constants.Message.NoHeaderNotification, Constants.Caption.NoHeaderCaption);
				}
			};
			this.MenuItems.Add(item);
		}

		#endregion

		void ValidateAndDeclareAFR(ActionCode actionCode)
		{
			if (Header != null)
			{
				if (SaveData(Header))
				{
					if (Header.JPH_IsActive)
					{
						DeclareAFR(actionCode);
					}
					else
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("JPAFRMainMenuItem|HeaderDeactivatedForSending", "Messages can not be sent when AFR Header is deactivated, to reactivate go to Actions->Make Active"));
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Constants.Message.NoHeaderNotification, Constants.Caption.NoHeaderCaption);
			}
		}

		void DeclareAFR(ActionCode actionCode)
		{
			switch (actionCode)
			{
				case ActionCode.Registering:
					DeclareAFRRegistration();
					break;
				case ActionCode.RegisterCompletionByRegistration:
					DeclareAFRRegistrationCompletion();
					break;
				case ActionCode.RegisterDepartureTime:
					DeclareATDRegistration();
					break;
				default:
					DeclareAFRAmendment(actionCode);
					break;
			}
		}

		#region Variation of Declare AFR

		#region Helper

		static bool CheckIsHeaderFinalized(bool isShippingLineEntry, bool isBillRegistrationCompleted, bool isDepartureTimeRegistered)
		{
			var result = false;
			if ((!isShippingLineEntry && isBillRegistrationCompleted) || (isShippingLineEntry && isDepartureTimeRegistered))
			{
				var message = isShippingLineEntry ? Constants.Message.DepartureTimeRegistrationCompletionNotification : Constants.Message.BillRegistrationCompletionNotification;
				var caption = isShippingLineEntry ? Constants.Caption.DepartureTimeRegistrationNotificationCaption : Constants.Caption.BillRegistrationCompletionNotificationCaption;
				Globals.Message.ShowError(message, caption);
				result = true;
			}
			return result;
		}

		static bool CheckAreAllBillsRegistered(MessageSendingAction messageSendingAction)
		{
			var result = false;
			if (messageSendingAction.MessageSendingObjects.Count == 0)
			{
				Globals.Message.ShowError(Constants.Message.NoUnRegisteredBillsNotification, Constants.Caption.NoBillsCaption);
				result = true;
			}
			return result;
		}

		#endregion

		void SendBlanketVesselChange()
		{
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(new BlanketVesselChange(Header)))
			{
				var blanketVesselChange = form.BusinessEntity;
				if (!blanketVesselChange.JPM_BlanketChange && blanketVesselChange.BlanketVesselChangeBills.Count == 0)
				{
					Globals.Message.ShowError(Constants.Message.NoBillsNotification, Constants.Caption.NoBillsCaption);
				}
				else if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var billsToSend = blanketVesselChange.BillsToSend;
					var manifestSuccessfulMessage = ManifestSuccessfulMessage;
					var messageGenerator = new AFRMessageGenerator(Header, DefaultDataObjectWriterStrategy.Instance);

					var manifestSendOK = false;
					if (Header.NewVesselVoyage != null)
					{
						manifestSendOK = messageGenerator.SendCMVToCustoms(MessagingTypeList.Codes.BlanketVesselChange,
							(ZGuid billPK, ref string functionType) =>
							{
								functionType = FunctionTypeList.Codes.BlanketVesselChange;
								return billsToSend.FirstOrDefault(x => x.PK == billPK) != null;
							});
						var billsToSendCount = billsToSend.Length;
						if (billsToSendCount > 0)
						{
							manifestSuccessfulMessage = ResString.GetMultilingualString("d4cce6fc-c8e7-47c0-af30-2a7d83145573", "Sent one Blanket Vessel Change Message containing details of {0} {1}.\r\n", billsToSendCount, billsToSendCount > 1 ? Grammar.Instance.Pluralize(Bill) : Bill);
						}
					}
					if (manifestSendOK)
					{
						Globals.Message.ShowInformation(manifestSuccessfulMessage, MessageSentCaption);
					}
				}
			}
		}

		void DeclareAFRRegistration()
		{
			var messageSendingAction = new MessageSendingAction(Header, ActionCode.Registering);

			if (CheckAreAllBillsRegistered(messageSendingAction))
			{
				return;
			}

			if (CheckIsHeaderFinalized(Header.JPH_IsShippingLineEntry, Header.IsBillRegistrationCompleted, Header.IsDepartureTimeRegistered))
			{
				return;
			}

			var consolNoDischargeInJapanMessage = messageSendingAction.GetWarningForConsolWithoutLegGoesIntoJP();
			if (!consolNoDischargeInJapanMessage.IsEmpty &&
				Globals.Message.Show(consolNoDischargeInJapanMessage, Constants.Caption.ConsolNoDischargeInJapan, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
			{
				return;
			}

			MessageSendingNotificationCollection notifications = ValidateHeaderForSending(messageSendingAction, false);
			if (notifications.ContainsError())
			{
				Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), string.Empty);
			}
			else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				ZString warningMessage = messageSendingAction.GetWarningForBillsToSendThatAreWaitingForResponse();
				if (warningMessage.IsEmpty || Globals.Message.Show(warningMessage, Constants.Caption.BillsThatAreWaitingForResponseCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
				{
					var objectsToSend = messageSendingAction.ObjectsToSend;
					var sendOK = new AFRMessageGenerator(Header, DefaultDataObjectWriterStrategy.Instance).SendDataToCustoms(Header.JPH_IsShippingLineEntry ? MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster : MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
					{
						functionType = FunctionTypeList.Codes.Registration;
						return objectsToSend.FirstOrDefault(x => x.PK == billPK) != null;
					});
					if (sendOK)
					{
						Globals.Message.ShowInformation(Res.GetString("537c94db-1fea-426c-8237-4916f95b295a", "Sent one Manifest Registration Message containing details of {0} {1}.", objectsToSend.Count, objectsToSend.Count > 1 ? Grammar.Instance.Pluralize(Bill) : Bill), MessageSentCaption);
						Header.AFRMessages.Reload(true);
					}
				}
			}
		}

		void DeclareAFRRegistrationCompletion()
		{
			var messageSendingAction = new MessageSendingAction(Header, ActionCode.RegisterCompletionByRegistration);
			using (var form = new JPAFRHeaderLevelMessageForm(messageSendingAction))
			{
				if (messageSendingAction.MessageSendingObjects.Count == 0)
				{
					Globals.Message.ShowError(Constants.Message.NoBillsNotification, Constants.Caption.NoBillsCaption);
					return;
				}
				if (Header.IsBillRegistrationCompleted
					&&
					Globals.Message.Show(
						Constants.Message.BillRegistrationCompletionAlreadyDoneNotification,
						Constants.Caption.BillRegistrationCompletionNotificationCaption,
						MessageBoxButtons.YesNo,
						DialogResult.No) == DialogResult.No
					)
				{
					return;
				}
				if (!Header.AreAllBillsRegistered
					&&
					Globals.Message.Show(Constants.Message.NotAllBillsHaveAlreadyBeenRegistered,
						Constants.Caption.UnRegisteredBillNotificationCaption,
						MessageBoxButtons.YesNo,
						DialogResult.No) == DialogResult.No)
				{
					return;
				}
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var actionCode = messageSendingAction.ActionCode;
					var notifications = ValidateHeaderForSending(messageSendingAction, true);

					if (notifications.ContainsError())
					{
						Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), string.Empty);
					}
					else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						var warningMessage = messageSendingAction.GetWarningForBillsToSendThatAreWaitingForResponse();
						if (warningMessage.IsEmpty || Globals.Message.Show(warningMessage, Constants.Caption.BillsThatAreWaitingForResponseCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
						{
							var sendOK = new AFRMessageGenerator(Header, DefaultDataObjectWriterStrategy.Instance).SendCompletionMessageToCustoms(actionCode);
							if (sendOK)
							{
								Globals.Message.ShowInformation(ResString.GetMultilingualString("8C121A94-95CA-415B-90A7-9B993BAB2829", "One Manifest Registration Completion message sent."), MessageSentCaption);
								Header.AFRMessages.Reload(true);
							}
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline, but refactored to only access .Count once")]
		void DeclareATDRegistration()
		{
			var messageSendingAction = new MessageSendingAction(Header, ActionCode.RegisterDepartureTime);
			using (var form = new JPAFRHeaderLevelMessageForm(messageSendingAction))
			{
				var messageSendingObjects = messageSendingAction.MessageSendingObjects;
				var messageSendingObjectsCount = messageSendingObjects.Count;
				if (messageSendingObjectsCount == 0
					&& Globals.Message.Show(Constants.Message.NoBillsNotification + " " + Constants.Message.DepartureTimeSolelyRegistrationNotification,
					Constants.Caption.NoBillsCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.No)
				{
					return;
				}
				else if (Header.IsDepartureTimeRegistered)
				{
					Globals.Message.ShowError(Constants.Message.DepartureTimeRegistrationCompletionAlreadyDoneNotification, Constants.Caption.DepartureTimeRegistrationNotificationCaption);
					return;
				}
				else if (messageSendingObjectsCount > 0 && !Header.AreAllBillsRegistered && Globals.Message.Show(Constants.Message.NotAllBillsHaveAlreadyBeenRegistered, Constants.Caption.UnRegisteredBillNotificationCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
				{
					return;
				}
				else if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
				{
					MessageSendingNotificationCollection notifications = ValidateHeaderForSending(messageSendingAction, true);
					if (notifications.ContainsError())
					{
						Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), string.Empty);
					}
					else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						ZString warningMessage = messageSendingAction.GetWarningForBillsToSendThatAreWaitingForResponse();
						if (warningMessage.IsEmpty || Globals.Message.Show(warningMessage, Constants.Caption.BillsThatAreWaitingForResponseCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
						{
							var isCorrection = messageSendingAction.ActionCode == ActionCode.ChangeDepartureTimeAfterATD;
							var sendOK = new AFRMessageGenerator(Header, DefaultDataObjectWriterStrategy.Instance).SendDepartureTimeRegistrationToCustoms(isCorrection);
							if (sendOK)
							{
								Globals.Message.ShowInformation(
									isCorrection ? ResString.GetMultilingualString("9386CAC3-B678-4DCF-BAAB-EF080F046FAF", "Sent one Departure Time Correction Message\r\n") : ResString.GetMultilingualString("3C30099D-226D-4849-9427-A58CF14D3671", "One Departure Time Registration message sent.")
									, MessageSentCaption);
								Header.AFRMessages.Reload(true);
							}
						}
					}
				}
			}
		}

		void DeclareAFRAmendment(ActionCode actionCode)
		{
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(Header, actionCode), MainForm))
			{
				var messageSendingAction = form.BusinessEntity;
				if (messageSendingAction.MessageSendingObjects.Count == 0)
				{
					Globals.Message.ShowError(Constants.Message.NoBillsNotification, Constants.Caption.NoBillsCaption);
				}
				else if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					SendMessageForAmendmentForm(messageSendingAction);
				}
			}
		}

		public void SendMessageForAmendmentForm(MessageSendingAction messageSendingAction)
		{
			var warningMessage = messageSendingAction.GetWarningForBillsToSendThatAreWaitingForResponse();
			if (warningMessage.IsEmpty || Globals.Message.Show(warningMessage, Constants.Caption.BillsThatAreWaitingForResponseCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
			{
				var objectsToSend = messageSendingAction.ObjectsToSend;
				var objectsToSendCount = objectsToSend.Count;
				var manifestSuccessfulMessage = ManifestSuccessfulMessage;
				var logCancellationMessage = string.Empty;
				var aTD5Message = string.Empty;
				var messageGenerator = new AFRMessageGenerator(Header, DefaultDataObjectWriterStrategy.Instance);

				var manifestSendOK = false;
				if (objectsToSendCount > 0)
				{
					var messageType = Header.JPH_IsShippingLineEntry
						? MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster
						: MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse;
					manifestSendOK = messageGenerator.SendDataToCustoms(messageType
					,
						(ZGuid billPK, ref string functionType) =>
						{
							var shouldContinue = false;
							functionType = null;
							var objectToSend = objectsToSend.FirstOrDefault(x => x.PK == billPK);
							if (objectToSend != null)
							{
								functionType = GetFunctionType(objectToSend.JPM_ActionCode);
								shouldContinue = true;
							}
							return shouldContinue;
						});
					if (manifestSendOK)
					{
						manifestSuccessfulMessage = ResString.GetMultilingualString("62c3ef5d-8de4-4ed7-92b3-b491c713555c", "Sent one Manifest Amendment Message containing details of {0} {1}.\r\n", objectsToSendCount, objectsToSendCount > 1 ? Grammar.Instance.Pluralize(Bill) : Bill);
					}
				}
				if (messageSendingAction.ActionCode == ActionCode.ChangeDepartureTimeAfterATD)
				{
					var aTD5SendOK = messageGenerator.SendDepartureTimeRegistrationToCustoms(true);
					if (aTD5SendOK)
					{
						aTD5Message = ResString.GetMultilingualString("9386CAC3-B678-4DCF-BAAB-EF080F046FAF", "Sent one Departure Time Correction Message\r\n");
					}
				}
				if (manifestSendOK && messageSendingAction.ActionCode == ActionCode.ReRegisterMasterAfterATD)
				{
					messageSendingAction.Header.CancelDepartureTimeRegistrationLog();
					messageSendingAction.Header.Factory.Save();
					logCancellationMessage = ResString.GetMultilingualString("A4F3B5B4-8D02-4EBF-A995-4EBF353972DF", "Departure Time Registration Log has been Canceled\r\n");
				}

				if (manifestSendOK)
				{
					Globals.Message.ShowInformation(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", manifestSuccessfulMessage, aTD5Message, logCancellationMessage), MessageSentCaption);
					Header.AFRMessages.Reload(true);
				}
			}
		}

		#endregion

		#region Implementation

		MessageSendingNotificationCollection ValidateHeaderForSending(MessageSendingAction messageSendingAction, bool shouldExcludeBills)
		{
			MessageSendingNotificationCollection notifications = null;
			var headerIsTopLevel = Header.IsTopLevel;
			try
			{
				messageSendingAction.Header.IsTopLevel = false;
				messageSendingAction.RegisterEditableChildObject(messageSendingAction.Header);
				if (shouldExcludeBills)
				{
					messageSendingAction.ResetEditableChildObject();
				}
				var validation = JPAFRMessageSendingValidation.New(messageSendingAction, null);
				notifications = validation.CheckBusinessObjectLevelValidation();
			}
			finally
			{
				Header.IsTopLevel = headerIsTopLevel;
				Header.RegisterEditableChildObject(Header.Bills);
			}
			return notifications;
		}

		string GetFunctionType(ZString actionCode)
		{
			string result = null;
			switch (actionCode)
			{
				case AFRSendingActionCodeList.Codes.Add:
					result = FunctionTypeList.Codes.Add;
					break;
				case AFRSendingActionCodeList.Codes.Delete:
					result = FunctionTypeList.Codes.Delete;
					break;
				case AFRSendingActionCodeList.Codes.Update:
					result = FunctionTypeList.Codes.Update;
					break;
				case AFRSendingActionCodeList.Codes.Register:
					result = FunctionTypeList.Codes.Registration;
					break;
			}
			return result;
		}

		bool SaveData(BusinessObject parent)
		{
			bool result = true;

			if (parent.HasChanges)
			{
				if (Globals.Message.Show(Constants.Message.DataNotSavedNotification, Constants.Caption.SaveDataCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = MainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		ZForm MainForm => (ZForm)this.GetMainMenu()?.GetForm();

		#endregion

		#region string Value Section

		string ManifestSuccessfulMessage => ResString.GetMultilingualString("1CEBA938-09C2-4D14-9291-455446834AF9", "Message Sending Successful\r\n");

		string Bill => Res.GetString("81a49b87-7035-4342-bf3d-47b1ed0f268b", "Bill");

		string MessageSentCaption
		{
			get { return ResString.GetMultilingualString("A59E1B3A-32E9-4291-840F-42112505E429", "Message Sent"); }
		}

		public static class Constants
		{
			public static class Caption
			{
				public static string BillsThatAreWaitingForResponseCaption
				{
					get { return Res.GetString("JPAFRMainMenuItem|5C9BA397-1C8A-41CE-94EE-4B52CD7053A7", "Pending Messages"); }
				}

				public static string NoBillsCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|DFEBCBEA-C61D-48FE-8DF1-250CD7E3E182", "No Bills"); }
				}

				public static string NoHeaderCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|B33A3959-DDAD-4C40-B462-8AF54178BF0C", "No Advance Filing Rules Data"); }
				}

				public static string SaveDataCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|A42B28AD-0CBB-4D49-BDC5-4AF1EF4381A9", "Save Data"); }
				}

				public static string BillRegistrationCompletionNotificationCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|0729ED40-F0FA-4AAB-95BC-0CA174F10E1B", "Bill Registration Completed"); }
				}

				public static string DepartureTimeRegistrationNotificationCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|C2CFD2CA-C8E7-4FB7-8224-1B3B3B72310C", "Departure Time Registration Completed"); }
				}

				public static string UnRegisteredBillNotificationCaption
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|8E30501B-B00A-4D60-9530-A1D6FBEFCEFC", "Un-Registered Bill"); }
				}

				public static string ConsolNoDischargeInJapan
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|4C8E6381-8EC0-422F-8182-E2932C007701", "Consolidation doesn't have a Port of Discharge in Japan"); }
				}
			}

			public static class Message
			{
				public static string NoBillsNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|05205FA3-FF2A-4705-A77B-3F0D8B4FDD99", "No Bill of Lading has been entered."); }
				}

				public static string NoUnRegisteredBillsNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|D6CD2201-6B6B-47C4-BAE7-8BF9047F1814", "There are no un-registered Bills of Lading."); }
				}

				public static string NoHeaderNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|0A2855F8-4278-45A7-B971-9BC531072D57", "Please create Advance Filing Rules data by clicking on the AFR tab."); }
				}

				public static string DataNotSavedNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|56B1F5C0-901C-4324-B33B-47B879B99DFB", "The data has not yet been saved. Do you want to save and proceed?"); }
				}

				public static string NotAllBillsHaveAlreadyBeenRegistered
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|0FDFCB41-EE68-4503-8316-76E70584B27C", "Not all bills have been registered. Do you still want to proceed?"); }
				}

				public static string BillRegistrationCompletionNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|8340099F-C372-4CDC-AF79-CEBE173AC499", "Cannot register any new Bill once Bill Completion has been lodged. Please use the Amendment Manifest option to add a new Bill."); }
				}

				public static string BillRegistrationCompletionAlreadyDoneNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|7962982C-4C01-482D-BD8C-9DE797E57AA5", "Completion already sent. Would you like to Register Manifest Completion regardless?"); }
				}

				public static string DepartureTimeRegistrationCompletionNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|AC7566AD-889D-4C1C-B490-748ADF020A2D", "Cannot register any new Bill since the Departure Time of the Vessel Information has been successfully registered. Please use the Amendment Manifest option to add a new Bill."); }
				}

				public static string DepartureTimeRegistrationCompletionAlreadyDoneNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|8236098B-D99C-467D-969E-589E18A5FDFF", "Departure Time Registration has already been lodged. Please use the 'Amendment Manifest' if you wish to correct the departure time registration."); }
				}

				public static string DepartureTimeSolelyRegistrationNotification
				{
					get { return ResString.GetMultilingualString("JPAFRMainMenuItem|2C451B4B-6B4A-4F2F-92F5-83661ECE39AA", "Do you want to solely register the departure time for the Vessel Information."); }
				}

				public static string DepartureTimeRegistrationNotification
				{
					get
					{
						return ResString.GetMultilingualString("JPAFRMainMenuItem|6158D0F9-5643-49CD-AB02-56CB55DC1FC1", @"Do you want to register the Departure Time (ATD) for the manifest?

Once the Departure Time is successfully registered the departure time can be amended.

1. If you do need to change the vessel information* after a successful lodgement of Departure Time Registration, you will need to submit an amendment (‘Amend Manifest’ menu item), after which a new Departure Time Registration will need to be sent.

2. Note that once Departure Time is registered, bill details can only be amended on receipt of a customs assessment notice or by re-manifesting the bills onto a new manifest (vessel information* should change).

*(Carrier Code, Vessel Call Sign, Voyage Number, Port of Loading and Suffix)

Please ensure that all data is correct before proceeding.");
					}
				}
			}
		}

		#endregion
	}
}
