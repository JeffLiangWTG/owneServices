using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	abstract class LucalGenralResponseMaker
	{
		public static LucalGenralResponseMaker New(LucasGenralEnquiryHandler.StatusOfTheRequest status, EDIMessage inboundEdiMessage,
													string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects,
													LucasGenralEnquiryHandler.EnquiryObjectTypes objectRequestType,
													ZString commonAccessReference)
		{
			switch (status)
			{
				case LucasGenralEnquiryHandler.StatusOfTheRequest.BadRequest:
					return new LucalGenralResponseMaker_Failure("REQUEST REJECTED - INVALID DATA IN REQUEST", inboundEdiMessage, requestText, commonAccessReference);
				case LucasGenralEnquiryHandler.StatusOfTheRequest.InternalError:
					return new LucalGenralResponseMaker_Failure("REQUEST REJECTED - SYSTEM FAILURE. CONTACT DEP OPERATOR", inboundEdiMessage, requestText, commonAccessReference);
				case LucasGenralEnquiryHandler.StatusOfTheRequest.NoRecordFound:
					return new LucalGenralResponseMaker_Failure("REQUEST REJECTED - NO MATCHING RECORDS FOUND", inboundEdiMessage, requestText, commonAccessReference);
				default:
					return LucalGenralResponseMaker_Success.New(requestText, mainBusinessObject, childrenBusinessObjects, objectRequestType, inboundEdiMessage, commonAccessReference);
			}
		}

		protected LucalGenralResponseMaker(EDIMessage inboundEdiMessage, string enquiryString, ZString commonAccessReference)
		{
			this.inboundEdiMessage = inboundEdiMessage;
			this.enquiryString = enquiryString;
			this.commonAccessReference = commonAccessReference;
		}

		internal abstract void MakeAndSendAllResponses();
		protected EDIMessage inboundEdiMessage;
		protected string enquiryString;
		protected ZString commonAccessReference;
	}
}
