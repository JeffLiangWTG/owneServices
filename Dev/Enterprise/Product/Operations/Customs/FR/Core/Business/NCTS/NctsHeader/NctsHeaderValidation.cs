using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderValidation
	{
		public NctsHeaderValidation(NctsHeader parent) : base(parent)
		{
		}

		protected override void CheckBH_MessageStatus()
		{
			base.CheckBH_MessageStatus();
			var parent = Parent;
			if (!parent.IsPhase5 && parent.BH_MessageStatus == EDIMessageStatusList.Codes.Rejected)
			{
				var rejectionDetails = parent.GetLastFRMEventErrorDescription();

				if (!string.IsNullOrEmpty(rejectionDetails))
				{
					parent.BH_MessageStatusInfo.AddWarning(rejectionDetails);
				}
			}
		}

		protected override void CheckDestinationCustomsOfficeCodeForArrivalCore()
		{
			base.CheckDestinationCustomsOfficeCodeForArrivalCore();
			var parent = Parent;

			if (parent.IsArrivalMovement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.DestinationCustomsOfficeCodeForArrivalInfo);

				if (!parent.IsPhase5 && (parent.ArrivalMovementHeader?.IsSimplifiedNctsProcedure ?? false))
				{
					var ruleValue = parent.DestinationACEAuthorisationOFCRuleValue;
					var info = parent.DestinationCustomsOfficeCodeForArrivalInfo;
					if (ruleValue.IsEmpty)
					{
						info.AddMessageError(Res.GetString("C18996C0-EC56-43A5-AB5F-AFC5F978AEFE", "No Declarant ACE authorization matching Destination Trader address with OFC rule could be found."));
					}
					else if (parent.DestinationCustomsOfficeCodeForArrival != ruleValue)
					{
						info.AddMessageError(Res.GetString("7ADAB042-3162-46E2-B9B2-765BED1C0E51", "This Customs office doesn't match the office set in Declarant ACE authorization matching Destination Trader address."));
					}
				}
			}
		}

		protected override void CheckArrivalMrnFromUser()
		{
			base.CheckArrivalMrnFromUser();
			var parent = Parent;
			var arrivalMrnFromUser = parent.ArrivalMrnFromUser;
			var arrivalMrnFromUserInfo = parent.ArrivalMrnFromUserInfo;

			if (parent.IsArrivalMovement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(arrivalMrnFromUserInfo);
			}

			if (!arrivalMrnFromUser.IsEmpty && !SatisfyRequiredFormat())
			{
				arrivalMrnFromUserInfo.AddMessageError(Res.GetString("aaaa40a7-dc5a-4f34-b1e1-5cece3f9b4e5", "You have not entered a valid MRN number. The valid format should be 2 digits + 2 letters of country code + 14 letters/digits."));
			}

			bool SatisfyRequiredFormat()
			{
				return Regex.IsMatch(arrivalMrnFromUser, @"^[0-9]{2}[a-zA-Z]{2}[a-zA-Z0-9]{14}$") && new RefCountryCollection(parent.Factory).Any(c => c.Code == arrivalMrnFromUser.Substring(2, 2));
			}
		}

		protected override void CheckBH_OH_Carrier()
		{
			base.CheckBH_OH_Carrier();

			var parent = Parent;
			if (parent.BH_FTZMove)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BH_OH_CarrierInfo);
				if (!parent.BH_OH_Carrier.IsEmpty)
				{
					var carrier = parent?.Carrier;
					if (carrier != null)
					{
						var identificationNumber = carrier.GetEORI(Core.Constants.CountryCodes.France, false);
						if (identificationNumber.IsEmpty)
						{
							parent.BH_OH_CarrierInfo.AddMessageError(Res.GetString("4EE32712 - 6BCB - 4FB8 - 989F - 2B62D16C7D99", "Your Security Carrier has no EORI Code."));
						}
					}
				}
			}
		}

		protected override void CheckDepartureCustomsOfficeAgainstConsignorAuthorisationCore(ZPropertyInfo info)
		{
			base.CheckDepartureCustomsOfficeAgainstConsignorAuthorisationCore(info);

			var parent = Parent;
			if (!parent.IsPhase5 && parent.IsDepartureMovement &&  parent.MovementHeader.IsSimplifiedNctsProcedure)
			{
				var consignorAuthorisationOffice = parent.ConsignorACRAuthorisationOFCRuleValue;
				if (consignorAuthorisationOffice.IsEmpty)
				{
					info.AddMessageError(Res.GetString("A501F360-2E69-4114-8132-FA38518264BD", "No Declarant ACR authorization matching Consignor address with OFC rule could be found."));
				}
				else if (parent.DepartureCustomsOfficeCode != consignorAuthorisationOffice)
				{
					info.AddMessageError(Res.GetString("E5BA2683-28A8-440E-BA95-17A080C24466", "This Customs office doesn't match the office set in Declarant authorization matching Consignor address."));
				}
			}
		}
		public new NctsHeader Parent => (NctsHeader)base.Parent;
	}
}
