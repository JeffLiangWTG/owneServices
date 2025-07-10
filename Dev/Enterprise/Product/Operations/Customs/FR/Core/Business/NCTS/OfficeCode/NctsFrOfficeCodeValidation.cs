using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsFrOfficeCodeValidation : EU.NCTS.Business.NctsEuOfficeCodeValidation
	{
		public NctsFrOfficeCodeValidation(NctsFrOfficeCode parent)
			: base(parent)
		{
		}

		static string MessageError => Res.GetString("9540d110-1a32-43db-857a-3e3697825689", "The office of departure date cannot be empty and must less than 30 days in the future for pre-lodged departures");

		protected override void CheckCY_Date()
		{
			base.CheckCY_Date();

			var parent = Parent;
			var header = parent.Header ?? (parent.MovementHeader?.Header) ?? (NctsHeader)parent.ArrivalMovementHeader?.Header;
			if (header != null)
			{
				if (header.IsDepartureMovement && header.IsPrelodgedMovement && IsDepartureOffice())
				{
					var preLodgedDate = parent.CY_Date;

					if (preLodgedDate.IsEmpty || preLodgedDate <= ZDateTime.Now || preLodgedDate > ZDateTime.Now.AddDays(30))
					{
						parent.CY_DateInfo.AddMessageError(MessageError);
					}
				}
			}
		}

		bool IsDepartureOffice()
		{
			return Parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		}

		protected new NctsFrOfficeCode Parent => (NctsFrOfficeCode)base.Parent;
	}
}
