using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class CusFRNctsHeaderPhase4Validation : CusFRNctsHeaderValidation
	{
		public CusFRNctsHeaderPhase4Validation(AutoCusFRNctsHeader parent) : base(parent)
		{
		}

		protected override void CheckCFN_NatureOfSeals()
		{
			base.CheckCFN_NatureOfSeals();

			if ((Parent.Header?.MovementHeader?.BM_InBondEntryType ?? ZString.Empty) == EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR && Parent.CFN_NatureOfSeals != NatureOfSealsList.Codes.NS1)
			{
				Parent.CFN_NatureOfSealsInfo.AddMessageError(Res.GetString("B5A758DB-1059-47E5-89CF-2FF14F138F8B", "Nature of Seals must be 1 when type of declaration is TIR."));
			}
		}

		protected override void CheckCFN_IsPrelodgedMovement()
		{
			base.CheckCFN_IsPrelodgedMovement();

			var parent = Parent;
			if (!parent.CFN_IsPrelodgedMovement && (parent.Header?.BH_FTZMove ?? false) && parent.Header?.SecurityConsignor != null)
			{
				var traderCountryGroup = EU.NCTS.Business.SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(parent.Header.SecurityConsignor);
				if (traderCountryGroup != SecurityTraderCountryGroup.EuForSafetyAndSecurity && traderCountryGroup != SecurityTraderCountryGroup.NorthernIreland)
				{
					parent.CFN_IsPrelodgedMovementInfo.AddMessageError(Res.GetString("038AC69E-1F51-4842-A33D-6F1139780676", "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked."));
				}
			}
		}
	}
}
