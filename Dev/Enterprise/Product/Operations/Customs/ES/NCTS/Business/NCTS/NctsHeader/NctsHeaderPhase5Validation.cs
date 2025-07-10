using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation
	{
		public NctsHeaderPhase5Validation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMovementReferenceNumber();
		}

		protected override void CheckBH_CustomsProfile()
		{
			base.CheckBH_CustomsProfile();

			var header = Parent;
			var broker = header.IsArrivalMovement ? header.ArrivalMovementHeader?.CusAgent : header.MovementHeader?.CusAgent;
			if (!Parent.IsPhaseStatusTNN)
			{
				CertificateHelper.CheckCustomsProfile(header.BH_CustomsProfileInfo, header.BH_CustomsProfile, broker);
			}
		}

		public void ValidateMovementReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.MovementReferenceNumberInfo);
		}

		protected void CheckMovementReferenceNumber()
		{
			if (Parent.IsPhaseStatusTNN && Parent.TNNDocumentType == ESNCTS5ArrivalTNNTypeList.Codes.OtherThanTadOrWithNoMrn)
			{
				if (IsMrnOfficeAndPosition18TAreNotCorrect())
				{
					Parent.MovementReferenceNumberInfo.AddWarning(Res.GetString("10171051-B0BE-49A7-BBC8-25B21B18B25A", "If TNN Document is 4, MRN number 18 character must be T and characters 3 to 10 must match with departure custom office."));
				}
				if (IsSecurityNONAndPosition17JAreNotCorrect())
				{
					Parent.MovementReferenceNumberInfo.AddWarning(Res.GetString("295C7F8B-9B86-49A4-885D-57CBCDF50913", "If Non Security Data, MRN 17 character must be J."));
				}
				if (IsSecurityEXIAndPosition17KAreNotCorrect())
				{
					Parent.MovementReferenceNumberInfo.AddWarning(Res.GetString("56CC2931-914C-4B49-B39B-1C89DBE952B6", "If Security Data, MRN 17 character must be K."));
				}
			}
		}

		bool IsMrnOfficeAndPosition18TAreNotCorrect()
		{
			var mrn = Parent.MovementReferenceNumber;
			var officeOfDeparture = CustomsOfficeOfDeparture;

			return isPos18OfMRNIsNotT() || isPos3To10NotEqualToDestinationCustomsOffice();
			bool isPos18OfMRNIsNotT() => mrn.IsEmpty || mrn.SubstringSafe(17, 1) != "T";
			bool isPos3To10NotEqualToDestinationCustomsOffice() => officeOfDeparture.IsEmpty || mrn.SubstringSafe(2, 8) != officeOfDeparture;
		}

		bool IsSecurityNONAndPosition17JAreNotCorrect()
		{
			var mrn = Parent.MovementReferenceNumber;
			var officeOfDeparture = CustomsOfficeOfDeparture;

			return isPos18OfMRNIsNotJ() && isSecurityCodeNON();
			bool isPos18OfMRNIsNotJ() => mrn.IsEmpty || mrn.SubstringSafe(16, 1) != "J";
			bool isSecurityCodeNON() => Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON;
		}

		bool IsSecurityEXIAndPosition17KAreNotCorrect()
		{
			var mrn = Parent.MovementReferenceNumber;
			var officeOfDeparture = CustomsOfficeOfDeparture;

			return isPos18OfMRNIsNotK() && isSecurityCodeEXI();
			bool isPos18OfMRNIsNotK() => mrn.IsEmpty || mrn.SubstringSafe(16, 1) != "K";
			bool isSecurityCodeEXI() => Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI;
		}
		ZString CustomsOfficeOfDeparture => Parent.MovementHeader.CustomsOfficesForDeparture.FirstOrDefault()?.CY_Data ?? ZString.Empty;

		protected override bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled => false;
	}
}
