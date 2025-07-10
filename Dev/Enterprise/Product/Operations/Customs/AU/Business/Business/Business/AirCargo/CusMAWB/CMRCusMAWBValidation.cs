using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusMAWBValidation : AirCargoCusMAWBValidation
	{
		public CMRCusMAWBValidation(CusMAWB mAWB)
			: base(mAWB)
		{
		}

		protected override void CheckCM_FlightNo()
		{
			base.CheckCM_FlightNo();
			new CustomsValidation(MAWB.CM_FlightNoInfo).ErrorOnKeyData(MAWB);
			ZString airlinePart = MAWB.CM_FlightNo.Left(2);
			if (!airlinePart.IsEmpty)
			{
				if (!RefAirline.IsValidAirline2LetterCode(MAWB.Factory, airlinePart))
				{
					MAWB.CM_FlightNoInfo.AddMessageError("The flight number prefix is not a valid Airline code.");
				}
			}
		}

		protected override void CheckCM_ResponsiblePartyID()
		{
			base.CheckCM_ResponsiblePartyID();
			new CustomsValidation(MAWB.CM_ResponsiblePartyIDInfo).ErrorOnKeyData(MAWB);
		}

		protected override void CheckCM_ArrivalDate()
		{
			base.CheckCM_ArrivalDate();
			new CustomsValidation(MAWB.CM_ArrivalDateInfo).ErrorOnKeyData(MAWB);
		}

		protected override void CheckCM_MAWB()
		{
			base.CheckCM_MAWB();
			new CustomsValidation(MAWB.CM_MAWBInfo).ErrorOnKeyData(MAWB);
		}
	}
}
