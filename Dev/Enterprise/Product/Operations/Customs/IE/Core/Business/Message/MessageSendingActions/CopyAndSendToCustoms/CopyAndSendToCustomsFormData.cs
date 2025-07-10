using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using MessageTypesList = Enterprise.Customs.IE.Messaging.AESOutgoingMessageTypeList.Codes;

namespace Enterprise.Customs.IE.Business
{
	public class CopyAndSendToCustomsFormData : NonPersistentBusinessObject
	{
		[ResourceStringData("EA8EA1B5-1345-4252-AA27-596A55888DE8", Caption = "Number of Copies")]
		public ZInt NumberOfCopies
		{
			get { return numberOfCopies; }
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value))
				{
					Validation.ValidateNumberOfCopies();
				}
			}
		}
		ZInt numberOfCopies;

		public ZPropertyInfo NumberOfCopiesInfo => GetZPropertyInfo(nameof(NumberOfCopies));

		[ResourceStringData("A19E4079-DF0B-4D76-8F89-DA9433CB31A0", Caption = "Number of invoice lines to duplicate")]
		public ZInt NumberOfInvoiceLinesCopies
		{
			get { return numberOfInvoiceLinesCopies; }
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfInvoiceLinesCopiesInfo, ref numberOfInvoiceLinesCopies, value))
				{
					Validation.ValidateNumberOfInvoiceLinesCopies();
				}
			}
		}
		ZInt numberOfInvoiceLinesCopies;

		public ZPropertyInfo NumberOfInvoiceLinesCopiesInfo => GetZPropertyInfo(nameof(NumberOfInvoiceLinesCopies));

		[ResourceStringData("8041C0A8-AF92-48D7-8AB3-E59B8AAAC631", Caption = "Send To Customs")]
		public ZBool SendToCustoms
		{
			get { return sendToCustoms; }
			set
			{
				if (sendToCustoms != value)
				{
					SetNonPersistentPropertyValue(SendToCustomsInfo, ref sendToCustoms, value);
					SetDefaultMessageType(value);
				}
			}
		}
		ZBool sendToCustoms;

		public ZPropertyInfo SendToCustomsInfo => GetZPropertyInfo(nameof(SendToCustoms));

		[ResourceStringData("834790D9-EA70-423A-B35D-256A13235A14", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(CopyAndSendToCustomsFormDataLookups.SendingActionTypeList))]
		[MaxLength(EDIMessage.Schema.EM_MessageTypeMaxLength)]
		public ZString MessageType
		{
			get { return messageType; }
			set
			{
				if (SetNonPersistentPropertyValue(MessageTypeInfo, ref messageType, value))
				{
					Validation.ValidateMessageType();
				}
			}
		}
		ZString messageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		protected bool MessageType_ReadOnly => !SendToCustoms;

		public CopyAndSendToCustomsFormDataLookups Lookups => new CopyAndSendToCustomsFormDataLookups(this);

		public CopyAndSendToCustomsFormDataValidation Validation => new CopyAndSendToCustomsFormDataValidation(this);

		void SetDefaultMessageType(bool sendToCustoms)
		{
			if (sendToCustoms)
			{
				MessageType = MessageTypesList.ExportOriginal;
			}
			else
			{
				MessageType = string.Empty;
			}
		}
	}
}
