using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRMessageSendingActionForm : ZChildForm
	{
		public JPAFRMessageSendingActionForm()
		{
		}

		public JPAFRMessageSendingActionForm(MessageSendingAction sendingAction, ZForm mainForm)
			: base(sendingAction)
		{
			this.header = Argument.NotNull(sendingAction.Header, "Header");
			this.mainForm = Argument.NotNull(mainForm, "MainForm");
			isShippingLineEntry = header.JPH_IsShippingLineEntry;

			formCaption = "";
			switch (sendingAction.ActionCode)
			{
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingUpdate:
				case ActionCode.AmendingDelete:
					formCaption = Res.GetString("A9FA7045-BA4D-4669-9CD0-FD35EA6F8164", "Manifest Amendments");
					break;
			}

			if (isShippingLineEntry)
			{
				UpdateLayoutForShippingLineEntry();
			}

			this.VesselInformationUserControl.UpdateControlLayout(isShippingLineEntry);

			AddTickOptionToGrid(sendingAction.ActionCode);

			HookVesselInfoamtionChangeRelated();

			this.DescriptionLabel.CaptionResourceString = isShippingLineEntry ? DefaultDescriptionForVOCC : DefaultDescriptionForNVOCC;
			this.DescriptionLabel.UpdateCaption();
		}

		#region Initial Layout Change

		void AddTickOptionToGrid(ActionCode actionCode)
		{
			BillsGrid.GridId += actionCode.ToString();
			BillsGrid.ContextMenu.MenuItems.Add("-");
			BillsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("28882A50-3FD1-461E-9C50-DF162A6C1DB1", "Tick 'Send' for Selected"), TickSendForSelected_Clicked));
			BillsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("4AECE1B1-20C2-46D8-8865-B308F49D3187", "Untick 'Send' for Selected"), UntickSendForSelected_Clicked));
		}

		void UpdateLayoutForShippingLineEntry()
		{
			this.HasATDBeenSentCheckBox.Visible = false;

			var currentLocation = this.DescriptionLabel.Location;
			var currentSize = this.DescriptionLabel.Size;
			this.DescriptionLabel.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(currentLocation.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(currentLocation.Y) - 26);
			this.DescriptionLabel.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(currentSize.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(currentSize.Height) + 26);
		}

		#endregion

		bool isSaving;

		readonly ZForm mainForm;

		public JPAFRHeader Header
		{
			get { return header; }
		}
		readonly JPAFRHeader header;

		readonly bool isShippingLineEntry;

		public new MessageSendingAction BusinessEntity
		{
			get { return (MessageSendingAction)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var messageAction = BusinessEntity;
				if (messageAction != null)
				{
					messageAction.ResetEditableChildObject();
				}
				UnhookVesselInfoamtionChangeRelated();
			}
			base.Dispose(disposing);
		}

		public override string FormVerb => Res.GetString("98fb48e4-bb20-41d4-b61e-df546bf06bcf", "Send");

		public override string FormCaption
		{
			get { return formCaption; }
		}
		readonly string formCaption;

		bool NoBillWasFlaggedToSend
		{
			get
			{
				bool result = true;
				foreach (MessageSendingObject obj in BusinessEntity.MessageSendingObjects)
				{
					if (obj.JPM_Send)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		void UnhookEventHandlersAndClose()
		{
			UnhookVesselInfoamtionChangeRelated();
			Close();
		}

		#region Event Handlers

		void SendButton_Click(object sender, EventArgs e)
		{
			if (NoBillWasFlaggedToSend && BusinessEntity.ActionCode != ActionCode.ChangeDepartureTimeAfterATD)
			{
				Globals.Message.ShowError(NoBillWasFlaggedToSendMessage, AFRReportingCaption);
			}
			else
			{
				Customs.Business.MessageSendingNotificationCollection notifications;
				var headerIsTopLevel = Header.IsTopLevel;
				try
				{
					Header.IsTopLevel = false;
					Header.UnRegisterEditableChildObject(Header.Bills);
					BusinessEntity.RegisterEditableChildObject(Header);
					var validation = JPAFRMessageSendingValidation.New(BusinessEntity, null);
					notifications = validation.CheckBusinessObjectLevelValidation();
				}
				finally
				{
					Header.IsTopLevel = headerIsTopLevel;
					BusinessEntity.UnRegisterEditableChildObject(Header);
					Header.RegisterEditableChildObject(Header.Bills);
				}

				if (notifications.ContainsError())
				{
					Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), AFRReportingCaption);
				}
				else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), AFRReportingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (!Header.HasChanges || SaveData(Header, mainForm))
					{
						this.DialogResult = DialogResult.OK;
						UnhookEventHandlersAndClose();
					}
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			UnhookEventHandlersAndClose();
		}

		void TickSendForSelected_Clicked(object sender, EventArgs e)
		{
			SetSendForSelected(true);
		}

		void UntickSendForSelected_Clicked(object sender, EventArgs e)
		{
			SetSendForSelected(false);
		}

		void SetSendForSelected(bool ticked)
		{
			foreach (MessageSendingObject obj in BillsGrid.SelectedElements)
			{
				obj.JPM_Send = ticked;
			}
		}

		bool SaveData(JPAFRHeader header, ZForm mainForm)
		{
			bool result = false;

			if (header.HasChanges && mainForm != null)
			{
				this.isSaving = true;
				result = mainForm.FireSaveButton() == ContinueWithSave.Yes;
				this.isSaving = false;
			}
			return result;
		}

		#endregion

		#region For In-Form Changing Reaction

		#region Shared Methods

		void HookVesselInfoamtionChangeRelated()
		{
			Header.HasChangesChanged -= UpdateGridStatus;
			Header.HasChangesChanged += UpdateGridStatus;
			BusinessEntity.OnHasATDBeenSentChanged -= UpdateGridStatus;
			if (!isShippingLineEntry)
			{
				BusinessEntity.OnHasATDBeenSentChanged += UpdateGridStatus;
			}
			Header.ShouldValidateVesselInformationChange = new JPAFRHeader.ShouldValidateVesselInformationChangeDelegate(() => { return false; });
		}

		void UnhookVesselInfoamtionChangeRelated()
		{
			if (Header != null)
			{
				Header.ShouldValidateVesselInformationChange = new JPAFRHeader.ShouldValidateVesselInformationChangeDelegate(() => { return true; });
				Header.HasChangesChanged -= UpdateGridStatus;
				Header.MarkAsNeedingValidationIncludingChildren();
			}
			if (BusinessEntity != null)
			{
				BusinessEntity.OnHasATDBeenSentChanged -= UpdateGridStatus;
			}
		}

		void UpdateGridStatus(object sender, EventArgs e)
		{
			var sendingAction = sender as MessageSendingAction;
			if (sendingAction != null)
			{
				UpdateGridStatus(sendingAction.Header, null);
			}
		}

		void UpdateGridStatus(object sender, HasChangesChangedEventArgs e)
		{
			var header = sender as JPAFRHeader;
			if (BusinessEntity != null && header != null && !isSaving)
			{
				var hasChanges = header.HasChanges;
				var hasVesselInformationChanged = header.HasVesselInformationChanged;
				var hasMasterInformationChanged = header.HasMasterInformationChanged;
				var hasATDBeenSent = BusinessEntity.HasATDBeenSent;

				if (isShippingLineEntry)
				{
					var hasATDInformationChanged = Header.HasATDInformationChanged;
					UpdateMessageSendingObjectsInGridForVOCC(hasVesselInformationChanged, hasATDBeenSent, hasMasterInformationChanged, hasATDInformationChanged);
					UpdateDescriptionCaptionForVOCC();
				}
				else
				{
					UpdateMessageSendingObjectsInGridForNVOCC(hasVesselInformationChanged, hasMasterInformationChanged, hasATDBeenSent);
					UpdateDescriptionCaptionForNVOCC(hasVesselInformationChanged, hasMasterInformationChanged, hasATDBeenSent);
				}
				UpdateSaveButtonCaption(hasChanges);
			}
		}

		void UpdateSaveButtonCaption(bool hasChanges)
		{
			if (hasChanges)
			{
				this.SendButton.CaptionResourceString = Res.GetData("5CD283C1-070F-4390-AFE7-E2654E7C5982", "Save && &Send");
				this.SendButton.UpdateCaption();
			}
			else
			{
				this.SendButton.CaptionResourceString = Res.GetData("8A0C5F01-3FD4-4A6C-8E71-011BCE5B36A3", "&Send");
				this.SendButton.UpdateCaption();
			}
		}

		#endregion

		void UpdateMessageSendingObjectsInGridForNVOCC(bool hasVesselInformationChanged, bool hasMasterInformationChanged, bool hasATDBeenSent)
		{
			if (hasVesselInformationChanged)
			{
				BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
				{
					if (sendingObject.IsBillAlreadyRegistered)
					{
						sendingObject.UpdateAction(hasATDBeenSent ? ActionCode.CorrectVesselInformationByAmendment : ActionCode.CorrectVesselInformationByRegistration);
					}
					else if (sendingObject.ActionCode == ActionCode.NewBill)
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false);
					}
					else
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false, false);
					}
				});
			}
			else if (hasMasterInformationChanged)
			{
				BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
				{
					if (sendingObject.IsBillAlreadyRegistered)
					{
						sendingObject.UpdateAction(ActionCode.CorrectMasterInformation);
					}
					else if (sendingObject.ActionCode == ActionCode.NewBill)
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false);
					}
					else
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false, false);
					}
				});
			}
			else
			{
				BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
				{
					if (sendingObject.JPM_SendInfo.ReadOnly)
					{
						sendingObject.UpdateAction(ActionCode.AmendingAdd);
					}
					else
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false, false);
					}
				});
			}
			BusinessEntity.UpdateMessageSendingAction();
		}

		void UpdateDescriptionCaptionForNVOCC(bool hasVesselInformationChanged, bool hasMasterInformationChanged, bool hasATDBeenSent)
		{
			ResourceStringData newDescription = null;
			if (hasVesselInformationChanged)
			{
				if (hasATDBeenSent)
				{
					newDescription = VesselInformationChangedAfterATDDescriptionForNVOCC;
				}
				else
				{
					newDescription = VesselInformationChangedBeforeATDDescriptionForNVOCC;
				}
			}
			else if (hasMasterInformationChanged)
			{
				newDescription = MasterInformationChangedDescription;
			}
			else
			{
				newDescription = DefaultDescriptionForNVOCC;
			}

			if (newDescription != null)
			{
				this.DescriptionLabel.CaptionResourceString = newDescription;
				this.DescriptionLabel.UpdateCaption();
			}
		}

		void UpdateMessageSendingObjectsInGridForVOCC(bool hasVesselInformationChanged, bool hasATDBeenSent, bool hasMasterInformationChanged, bool hasATDInformationChanged)
		{
			var currentActionCode = BusinessEntity.ActionCode;
			if (hasVesselInformationChanged)
			{
				if (currentActionCode != ActionCode.ReRegisterMasterAfterATD && currentActionCode != ActionCode.ReRegisterMasterBeforeATD)
				{
					if (hasATDBeenSent)
					{
						if (Globals.Message.Show(ReRegisterNotificationMessageForVOCC, ReRegisterNotificationCaptionForVOCC, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
						{
							this.BusinessEntity.UpdateMessageSendingAction(ActionCode.ReRegisterMasterAfterATD);
							this.BusinessEntity.HasATDBeenSent = false;
							this.BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
							{
								if (sendingObject.ActionCode != ActionCode.NewBill)
								{
									sendingObject.UpdateAction(ActionCode.ReRegisterMasterAfterATD);
								}
								else
								{
									sendingObject.UpdateAction(sendingObject.ActionCode, false);
								}
							});
						}
						else
						{
							Header.JPH_CarrierCode = (ZString)Header.JPH_CarrierCodeInfo.OriginalValue;
							Header.JPH_Voyage = (ZString)Header.JPH_VoyageInfo.OriginalValue;
							Header.JPH_VesselName = (ZString)Header.JPH_VesselNameInfo.OriginalValue;
							Header.JPH_RL_NKLoading = (ZString)Header.JPH_RL_NKLoadingInfo.OriginalValue;
							Header.JPH_LoadingPortSuffix = (ZString)Header.JPH_LoadingPortSuffixInfo.OriginalValue;
						}
					}
					else
					{
						this.BusinessEntity.UpdateMessageSendingAction(ActionCode.ReRegisterMasterBeforeATD);
						BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
						{
							if (sendingObject.IsBillAlreadyRegistered)
							{
								sendingObject.UpdateAction(ActionCode.ReRegisterMasterBeforeATD);
							}
							else if (sendingObject.ActionCode == ActionCode.NewBill)
							{
								sendingObject.UpdateAction(sendingObject.ActionCode, false);
							}
							else
							{
								sendingObject.UpdateAction(sendingObject.ActionCode, false, false);
							}
						});
					}
				}
			}
			else if (hasMasterInformationChanged)
			{
				if (currentActionCode == ActionCode.ReRegisterMasterAfterATD)
				{
					hasATDBeenSent = ResetHasATDBeenSentForVOCC();
				}

				var targetAction = ActionCode.CorrectMasterInformation;
				if (hasATDInformationChanged && hasATDBeenSent)
				{
					this.BusinessEntity.UpdateMessageSendingAction(ActionCode.ChangeDepartureTimeAfterATD);
					targetAction = ActionCode.ChangeDepartureTimeAfterATD;
				}
				else
				{
					this.BusinessEntity.UpdateMessageSendingAction(ActionCode.CorrectMasterInformation);
				}
				BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
				{
					if (sendingObject.ActionCode != ActionCode.NewBill)
					{
						sendingObject.UpdateAction(targetAction);
						if (targetAction == ActionCode.ChangeDepartureTimeAfterATD)
						{
							sendingObject.JPM_Send = false;
						}
					}
					else
					{
						sendingObject.UpdateAction(ActionCode.NewBill, false, true);
					}
				});
			}
			else
			{
				BusinessEntity.UpdateMessageSendingAction(ActionCode.AmendingAdd);
				hasATDBeenSent = ResetHasATDBeenSentForVOCC();

				BusinessEntity.MessageSendingObjects.OfType<MessageSendingObject>().ToList().ForEach((sendingObject) =>
				{
					if (sendingObject.ActionCode == ActionCode.NewBill)
					{
						if (sendingObject.IsBillAlreadyRegistered)
						{
							sendingObject.UpdateAction(ActionCode.AmendingAdd);
						}
						else
						{
							sendingObject.UpdateAction(ActionCode.NewBill);
						}
					}
					else if (sendingObject.ActionCode == ActionCode.AmendingAdd)
					{
						sendingObject.UpdateAction(sendingObject.ActionCode, false, false);
					}
					else
					{
						sendingObject.UpdateAction(ActionCode.AmendingAdd);
					}
				});
			}
		}

		void UpdateDescriptionCaptionForVOCC()
		{
			ResourceStringData newDescription = null;
			switch (BusinessEntity.ActionCode)
			{
				case ActionCode.ReRegisterMasterAfterATD:
					newDescription = VesselInformationChangedAfterATDDescriptionForVOCC;
					break;
				case ActionCode.ReRegisterMasterBeforeATD:
					newDescription = VesselInformationChangedBeforeATDDescriptionForVOCC;
					break;
				case ActionCode.ChangeDepartureTimeAfterATD:
					newDescription = ATDInformationChangedDescriptionForVOCC;
					break;
				case ActionCode.CorrectMasterInformation:
					newDescription = MasterInformationChangedDescription;
					break;
				case ActionCode.AmendingAdd:
					newDescription = DefaultDescriptionForVOCC;
					break;
			}

			if (newDescription != null)
			{
				this.DescriptionLabel.CaptionResourceString = newDescription;
				this.DescriptionLabel.UpdateCaption();
			}
		}

		bool ResetHasATDBeenSentForVOCC()
		{
			var currentATDFlag = BusinessEntity.HasATDBeenSent;
			var originalATDFlag = Header.IsDepartureTimeRegistered;
			if (currentATDFlag != originalATDFlag)
			{
				BusinessEntity.HasATDBeenSent = originalATDFlag;
				return originalATDFlag;
			}
			else
			{
				return currentATDFlag;
			}
		}

		#endregion

		#region ResourceStrings

		static string NoBillWasFlaggedToSendMessage
		{
			get { return ResString.GetMultilingualString("JPAFRMessageSendingActionForm|D4B4A1D3-2F25-4D23-BDD1-C8C9631D4B7F", "No bills have been selected for AFR reporting"); }
		}

		static string AFRReportingCaption
		{
			get { return ResString.GetMultilingualString("JPAFRMessageSendingActionForm |23DCADB0-D5AB-4925-824E-FE2D7D4F6B78", "AFR Reporting"); }
		}

		static ResourceStringData DefaultDescriptionForNVOCC
		{
			get { return Res.GetData("4B00E499-8546-42B5-842A-F11AED7EE72F", "In order to change the House Bill level information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action."); }
		}

		static ResourceStringData MasterInformationChangedDescription
		{
			get { return Res.GetData("33630992-FC9F-4CC4-9CFE-AC56EEE92F82", "Since the corresponding Master Level Information has been changed, all the registered bills should be updated to keep the consistency between the information recorded in Japan Customs and the records in your system. You can also choose to delete the unwanted bill that has already been registered."); }
		}

		static ResourceStringData VesselInformationChangedBeforeATDDescriptionForNVOCC
		{
			get { return Res.GetData("D7733CAB-4D56-48EF-A437-D100A31EA4F2", "To change the Vessel Information (Carrier Code, Vessel Code. Voyage Number, Port of Loading and Port of Loading Suffix) before the carrier has sent the ATD message a 'Registration' message is used to re-file the information. Any other changes on the Bill level will also be submitted through the 'Registration' message."); }
		}

		static ResourceStringData VesselInformationChangedAfterATDDescriptionForNVOCC
		{
			get { return Res.GetData("F52D958E-519E-4519-A066-442EC9B321E1", "To change the Vessel Information (Carrier Code, Vessel Code. Voyage Number, Port of Loading and Port of Loading Suffix) after the carrier has sent the ATD message a 'Amendment – Add’ message is used to re-file the information. Any other changes on the Bill level will also be submitted through the 'Amendment - Add' message."); }
		}

		static ResourceStringData DefaultDescriptionForVOCC
		{
			get { return Res.GetData("JPAFRMessageSendingActionForm|F13F0972-7935-4A69-8832-4352DAF4E7F4", "In order to change the Bill information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action."); }
		}

		static ResourceStringData VesselInformationChangedBeforeATDDescriptionForVOCC
		{
			get { return Res.GetData("D5922902-E0C0-46EA-A073-1169ABB53399", "To change the Vessel Information (Carrier Code, Vessel Code. Voyage Number, Port of Loading and Port of Loading Suffix) before sending the ATD message, a ‘Registration’ message is used to re-manifest all the registered bills. Any other changes on the Bill level will also be submitted through the re-file."); }
		}

		static ResourceStringData VesselInformationChangedAfterATDDescriptionForVOCC
		{
			get { return Res.GetData("8A8C3FA8-33CC-41C6-9176-5D4229A6264C", "To change the Vessel Information (Carrier Code, Vessel Code. Voyage Number, Port of Loading and Port of Loading Suffix) after sending the ATD message, a ‘Registration’ message is used to re-file all the registered bills on the new Vessel Information set. Any other changes on the Bill level will also be submitted through the re-file. Please note that the ATD registration status on your system will be automatically canceled when the re-file messages sent out of your system. You will need to manually send the ATD Registration messages for the new Vessel Information when the Bill registrations are confirmed. Also, due to the limitation from JP Customs’ system, the departure time information registration for the old Vessel Information set is not allow to be deleted in their system. This may affect your message actions if you reuse the old Vessel Information for another job."); }
		}

		static ResourceStringData ATDInformationChangedDescriptionForVOCC
		{
			get { return Res.GetData("JPAFRMessageSendingActionForm|39445C07-A01D-4352-94FC-4EFCAD5FBE96", "Since the existing Departure Time Information has already been registered in Japan Customs' system and you are making changes to the corresponding field (Depart from relaxed Area, ETD), a Departure Time Correction will be in order. Also all the registered bills should be updated to keep the consistency between the information recorded in Japan Customs and the records in your system."); }
		}

		static ResourceString ReRegisterNotificationMessageForVOCC
		{
			get
			{
				return ResString.GetMultilingualString("JPAFRMessageSendingActionForm|EA381626-2581-4917-B924-4414B01C5A19", @"Are you sure you want to change to another Vessel Information?

1. If 'No', the changes you have made on the Vessel Information will be revert back to their original value.

2. If 'Yes', the current 'ATD Registration Status' will be canceled in your system when the message is generated successfully.

!! Please remember that you need to register Departure Time again when the bills are successfully registered with JP Customs.
!! But already registered Bill/Departure Time on the old Vessel Information will not be allowed to be removed from JP Customs systems. You may experience some message errors when reusing the information in other jobs.
");
			}
		}

		static ResourceString ReRegisterNotificationCaptionForVOCC
		{
			get { return ResString.GetMultilingualString("JPAFRMessageSendingActionForm|A1054B2C-44BE-41CB-A75C-C0F8B428A0C0", "Change Vessel Information?"); }
		}

		#endregion

		public void InjectHeaderHasChangesChangedEventHandlerForTest(EventHandler<HasChangesChangedEventArgs> handler)
		{
			Header.HasChangesChanged -= UpdateGridStatus;
			Header.HasChangesChanged += handler;
			Header.HasChangesChanged += UpdateGridStatus;
		}
	}
}
