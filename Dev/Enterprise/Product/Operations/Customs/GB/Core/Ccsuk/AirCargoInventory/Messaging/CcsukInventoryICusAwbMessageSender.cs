using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators;
using Enterprise.Environment;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CcsukInventoryICusAwbMessageSender : GbDeclarationMessageSender
	{
		public override void Send(BaseJobDeclaration baseDeclaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			throw new NotImplementedException("Do not use this - use the overide that takes ICcsukCusAwb");
		}

		public void Send(ICcsukCusAwb awb, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			if (ValidateAndShowUserAnyWarningsOrErrors(awb, sendMessagesToCustoms, how))
			{
				GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
				var manager = new CcsukInventoryMessageManager(awb, (CcsukTransmissionMessageFunction)how, sendMessagesToCustoms);
				manager.SendToCommunity();
			}
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new string[] { ServiceTask.CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode, ServiceTask.CcsukServiceTaskConstants.CcsukServiceTaskCode };
		}

		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
		{
			throw new NotImplementedException();
		}

		public bool CanSend { get; private set; }

		protected override bool ValidateAndShowUserAnyWarningsOrErrors(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			var ccsukTransmissionMessageFunction = (CcsukTransmissionMessageFunction)functionNewDeletedAmended;
			CcsukTransmissionMessageValidator validator = null;

			switch (ccsukTransmissionMessageFunction.MessageType)
			{
				case CcsukTransmissionMessageFunction.CUSCAR.Code:
					switch (ccsukTransmissionMessageFunction.MessageSubType)
					{
						case CcsukTransmissionMessageFunction.CUSCAR.FRX.Subcode:
							validator = new CARFRXMessageValidator(bizO);
							break;
						case CcsukTransmissionMessageFunction.CUSCAR.FCS.Subcode:
							validator = new CARFCSMessageValidator(bizO);
							break;
						case CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode:
							validator = new CARMessageValidator(bizO);
							break;
						case CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode:
							validator = new CARMessageValidator(bizO);
							break;
					}
					break;
				case CcsukTransmissionMessageFunction.CIM.Code:
					validator = new CIMFRNMessageValidator(bizO);
					break;
				case CcsukTransmissionMessageFunction.CUKFSR.Code:      //MessageSubType FSA and FSN 
					validator = new FSRMessageValidator(bizO);
					break;
				case CcsukTransmissionMessageFunction.CUSDEC.Code:      //MessageSubType IAR, ISR, TSR, FBK
					validator = new DECMessageValidator(bizO);
					break;
			}

			CanSend = false;
			if (validator != null)
			{
				var bo = bizO as BusinessObject;
				var coreErrorValidator = MessageSendingValidation.New(bo, null);
				var redErrorNotifications = coreErrorValidator.CheckBusinessObjectLevelValidation();
				if (redErrorNotifications.ErrorCount == 0 || coreErrorValidator.IsErrorsExistWithNoSecurityRight)
				{
					var notifications = validator.Validate();
					if (notifications.ContainsError())
					{
						var errorsAsString = notifications.NotificationsAsString();
						if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
						{
							var warningForSuperUser = "It is likely that your message(s) will be rejected by CCSUK as they have the following message errors:\r\n\r\n" + errorsAsString + "\r\nDo you want to send the message(s) despite these errors?";
							CanSend = sendMessagesToCustoms.AskUserToContinueWithAction(warningForSuperUser, "Warning", bo);
						}
						else
						{
							sendMessagesToCustoms.NotifyUserOfAnInvalidOperation(MessageSendingValidation.MessageErrorsExistWithNoSecurityRight + "\r\n\r\nThe errors are as follows.\r\n\r\n" + notifications.NotificationsAsString());
							CanSend = false;
						}
					}
					else if (!notifications.ContainsWarning() || sendMessagesToCustoms.AskUserToContinueWithAction(notifications.NotificationsAsString(), "Warning", bo))
					{
						CanSend = true;
					}
				}
				else
				{
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation(redErrorNotifications.ErrorNotificationsAsString());
					CanSend = false;
				}
			}
			return CanSend;
		}
	}
}
