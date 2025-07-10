using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaIEFREDIMessage : FREDIMessage
	{
		public DeltaIEFREDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.DEC;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			switch (EM_MessageSubType)
			{
				case DeltaIEResponseMessageSubTypeList.Codes.AmendmentConfirmation:
					return new IE404MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.InvalidationApprovalNotificationFeedback:
					return new IE410MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance:
					return new IE426MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance:
					return new IE428MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification:
					return new IE429MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.TimerExpirySupplementaryDeclaration:
					return new IE431MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.FunctionalRejection:
					return new IE456MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.UnderControlNotification:
					return new IE460MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.ToNotifyAPaymentOrAnInsufficientCredit:
					return new FRA101MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.RegistrationOfTheInvalidationOrAmendment:
					return new FRA102MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.ValidationRequestExtension:
					return new FRA103MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.TechnicalRejection:
					return new IE917MessageDataObject(this);
				case DeltaIEResponseMessageSubTypeList.Codes.ReleaseRejection:
					return new IE451MessageDataObject(this);
				default:
					return new DeltaIEMessageDataObject(this);
			}
		}
	}
}
