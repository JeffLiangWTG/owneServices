using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation
	{
		public NctsDepartureMovementHeaderPhase4Validation(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		public new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected override void CheckBM_GS_NKCusAgent()
		{
			base.CheckBM_GS_NKCusAgent();
			if (Parent.Header.IsBrokerNeeded)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_GS_NKCusAgentInfo);
			}
		}

		protected override void CheckBM_InlandTransportMode()
		{
			base.CheckBM_InlandTransportMode();
			CheckBM_InlandTransportModeFilled(Parent);
		}

		protected override void CheckBM_LocationOfGoodsCode()
		{
			base.CheckBM_LocationOfGoodsCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_LocationOfGoodsCodeInfo);
		}

		protected override bool ShouldListValidatePlaceOfUnloading => false;

		protected override void CheckConditionC547(EU.NCTS.Business.NctsHeader nctsHeader, ZPropertyInfo inBondEntryTypeInfo) { }//This method consists of eliminating the EU functionality.

		void CheckBM_InlandTransportModeFilled(NctsDepartureMovementHeader movementHeader)
		{
			if (!movementHeader.BM_InlandTransportMode.IsEmpty)
			{
				movementHeader.BM_InlandTransportModeInfo.AddMessageError(Res.GetString("5333DCEB-CA33-431A-B98F-F824820170AE", "This field is not used for Spanish Customs."));
			}
		}
	}
}
