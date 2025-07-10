using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCSQuarantineExDocEstablishmentAndTimeValidation : QuarantineExDocEstablishmentAndTimeValidation
	{
		public NEXDOCSQuarantineExDocEstablishmentAndTimeValidation(AutoQuarantineExDocEstablishmentAndTime parent)
			: base(parent)
		{
		}

		protected override void CheckEE_E2_Address()
		{
			if (IsValidationRequired)
			{
				var parent = Parent;
				if (parent.EE_ProcessingType.In(new ZString[] { EXDOCProcessTypeCodes.Codes.AquacultureFarm, EXDOCProcessTypeCodes.Codes.CatcherBoat })
					&& parent.EE_AuthorisationEstablishmentID.IsEmpty && IsAddressEmpty)
				{
					parent.EE_E2_AddressInfo.AddMessageError(EstablishmentIdOrAddressRequired);
				}
			}
		}

		protected override void CheckEE_AuthorisationEstablishmentID()
		{
			if (IsValidationRequired)
			{
				var parent = Parent;
				switch (parent.EE_ProcessingType)
				{
					case EXDOCProcessTypeCodes.Codes.Harvest:
					case EXDOCProcessTypeCodes.Codes.Treatment:
						// EE_AuthorisationEstablishmentID may be omitted
						break;
					case EXDOCProcessTypeCodes.Codes.AquacultureFarm:
					case EXDOCProcessTypeCodes.Codes.CatcherBoat:
						if (parent.EE_AuthorisationEstablishmentID.IsEmpty && IsAddressEmpty)
						{
							parent.EE_AuthorisationEstablishmentIDInfo.AddMessageError(EstablishmentIdOrAddressRequired);
						}
						break;
					default:
						if (parent.EE_AuthorisationEstablishmentID.IsEmpty)
						{
							parent.EE_AuthorisationEstablishmentIDInfo.AddMessageError(EstablishmentIdRequired);
						}
						break;
				}
			}
		}

		protected override void CheckEE_EstablishmentIndicator()
		{
			if (IsValidationRequired)
			{
				if (Parent.EE_EstablishmentIndicator.IsEmpty && (!Parent.EE_AuthorisationEstablishmentID.IsEmpty || !IsAddressEmpty) && Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Harvest)
				{
					Parent.EE_EstablishmentIndicatorInfo.AddMessageError(EstablishmentIndicatorIsMandatory);
				}
			}
		}

		internal string EstablishmentIdRequired => Res.GetString("NEXDOCSQuarantineExDocEstablishmentAndTimeValidation|EstablishmentIdRequired", "Process establishment ID must be entered.");
		internal string EstablishmentIdOrAddressRequired => Res.GetString("NEXDOCSQuarantineExDocEstablishmentAndTimeValidation|EstablishmentIdOrAddressRequired", "Process establishment address or ID must be entered.");
		internal string EstablishmentIndicatorIsMandatory => Res.GetString("NEXDOCSQuarantineExDocEstablishmentAndTimeValidation|EstablishmentIndicatorIsMandatory", "Establishment Indicator is required when either Processing Establishment or Establishment ID is entered.");
	}
}
