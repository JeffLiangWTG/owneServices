using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodePhase5Validation : NctsEuOfficeCodeValidation
	{
		public NctsEuOfficeCodePhase5Validation(NctsEuOfficeCode parent) : base(parent)
		{
		}

		protected override ZString NotificationMessageForEmptyCY_Data
		{
			get
			{
				var parent = Parent;
				return parent.CY_Code.EqualsIgnoringCase(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival)
					&& parent.Parent is BusinessObject bizObj
					&& ((bizObj as NctsDepartureMovementHeader)?.Header ?? (bizObj as NctsArrivalMovementHeader)?.Header) is NctsHeader header
					? (ZString)MandatoryValidation.YouHaveNotEnteredMessage(header.CommonMovementHeader.DestinationCustomsOfficeCodeForArrivalInfo.HumanReadableName)
					: base.NotificationMessageForEmptyCY_Data;
			}
		}

		protected override string GetTooManyOfficesMessage(int maxSupported)
		{
			string ruleName = null;
			switch (Parent.CY_Code)
			{
				case OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit:
					ruleName = "TR0003";
					break;

				case OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit:
					ruleName = "TR0002";
					break;

				case OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination:
					ruleName = "TR0008";
					break;

				case OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture:
					ruleName = "TR0009";
					break;
			}

			return ruleName is null
				? base.GetTooManyOfficesMessage(maxSupported)
				: GetMaxCustomsOfficesOfTypeXExceededMessageError(ruleName, maxSupported, Parent.CY_Code);
		}
	}
}
