using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeaderMessageSendingObjectValidation : EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation
	{
		public NctsHeaderMessageSendingObjectValidation(EU.NCTS.Business.AutoNctsHeaderMessageSendingObject parent) : base(parent)
		{
		}

		protected new NctsHeaderMessageSendingObject Parent => (NctsHeaderMessageSendingObject)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			var parent = Parent;
			var nctsHeader = parent.NctsHeader;
			if (nctsHeader.BH_HeaderType == EU.NCTS.Business.NctsMovementType.Codes.Departure)
			{
				var info = parent.MessageTypeInfo;
				var sendingMessageType = parent.MessageType.ToUpperInvariant();
				var customsStatus = nctsHeader.MovementHeader.BM_CustomsStatus.ToUpperInvariant();
				var isMrnEmpty = nctsHeader.MovementReferenceNumber.IsEmpty;
				var isLrnEmpty = nctsHeader.LocalReferenceNumber.IsEmpty;

				if (sendingMessageType == NCTSOutgoingMessageTypeList.Codes.DeclarationData)
				{
					if (nctsHeader.MovementHeader.CustomsOfficesForDeparture.Cast<NctsIEOfficeCode>().Any(office =>
							office.IsOfficeOfTransit &&
							!office.CY_Date.IsEmpty &&
							office.CY_Date.IsInThePast()
							))
					{
						info.AddMessageError(Res.GetString("21305E93-9181-4445-8EED-45B6EB58D1F6", "The Estimated Arrival Date Time for Customs Office of Transit can't be earlier or equal to current date time. Please check Details -> Customs Offices."));
					}
				}
				else
				{
					if (customsStatus.IsEmpty)
					{
						info.AddMessageError(Res.GetString("4905F453-2F5C-4619-96BE-2A25D9BD3981", "Message Type should be Declaration (015) when Customs Status is empty"));
					}
					if (isMrnEmpty && isLrnEmpty)
					{
						info.AddMessageError(Res.GetString("FBFDA005-1C0D-4771-BB24-A9D8930DC783", "LRN or MRN required to send message other than Declaration (015)"));
					}

					if (sendingMessageType == NCTSOutgoingMessageTypeList.Codes.PresentationNotification
						&& nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader
						&& movementHeader.BM_AdditionalDeclarationType == EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.D
						&& movementHeader.BM_PortOfPresentationCode.Length + movementHeader.BM_PlaceOfLoading.Length < 3
						&& !MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader)
					)
					{
						info.AddMessageError(Res.GetString("5F488AFC-6287-4BAE-92C0-ECE6D913E0F6", "[C0404] Place of Loading is required for IE170, when it was not sent as part of IE015 or IE013."));
					}
				}
			}
		}

		protected override void CheckEnquiryText()
		{
			var parent = Parent;
			if (!parent.EnquiryTextInfo.ReadOnly)
			{
				AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered("[C0220]", parent.EnquiryTextInfo, parent.TC11DeliveryDateInfo);
			}
		}

		protected override void CheckDestinationCustomsOfficeCode()
		{
			base.CheckDestinationCustomsOfficeCode();

			var parent = Parent;
			if (!parent.DestinationCustomsOfficeCodeInfo.ReadOnly)
			{
				AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered("[C0315]", parent.DestinationCustomsOfficeCodeInfo, parent.TC11DeliveryDateInfo);
			}
		}

		static void AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(string messagePrefix, ZPropertyInfo propertyInfo, ZPropertyInfo dependentPropertyInfo)
		{
			var validationMessageError = MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfo, dependentPropertyInfo);
			var message = FormattableString.Invariant($"{messagePrefix} {validationMessageError}");
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(propertyInfo, dependentPropertyInfo, message);
		}
	}
}
