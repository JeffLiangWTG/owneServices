using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public abstract class BaseMessageSender
	{
		protected BaseMessageSender(AsycudaBill bill)
		{
			this.bill = bill;
		}
		protected readonly AsycudaBill bill;

		BusinessObjectFactory Factory => bill.Factory;

		protected abstract ZString MessageType { get; }

		protected virtual ZString MessageSubType { get; }

		protected abstract ZString MessageText { get; }

		public string SendMessage()
		{
			var result = ZString.Empty;

			var message = CreateEDIMessage();
			if (message != null)
			{
				try
				{
					bill.Messages.Add(message);
					SetStatus();
					Factory.Save();
					result = FormattableString.Invariant($"{MXMessageConstants.MessageSendSuccessful}");
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
					result = FormattableString.Invariant($"{MXMessageConstants.MessageCreateFailure}{System.Environment.NewLine}{e.Message}");
					message.Delete();
					RollBackStatus();
				}
			}
			return result;
		}

		EDIMessage CreateEDIMessage()
		{
			var credentialWrapper = GetCompanyCredential(GlbCompany.CurrentCompany);

			var message = Factory.New<MXMessage>();

			message.EM_ApplicationReference = bill.ABL_BillNumber;
			message.EM_IsTestMessage = MXCustomsDataRegistry.Instance.MXTestingSystem.Value;
			message.EM_LinkUniqueID = bill.PK;
			message.EM_MessageText = MessageText;
			message.EM_MessageType = MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			message.EM_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			message.EM_MessageOwner = credentialWrapper?.GP_UserID.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength) ?? ZString.Empty;
			message.EM_GP = credentialWrapper?.PK ?? ZGuid.Empty;

			return message;
		}

		protected abstract void SetStatus();

		protected virtual void RollBackStatus() { }

		static GlbCompanyCredential GetCompanyCredential(GlbCompany company) => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company)?.GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.MXB);
	}
}
