using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase5Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
	{
		public NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected override bool IsRuleTR0050Applicable => base.IsRuleTR0050Applicable && !Parent.IsInPhase5TransitionPeriod;

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			var nctsHeader = (NctsHeader)NctsHeader;
			var departureMovementHeader = Parent;

			if (departureMovementHeader.IsPhaseStatusTNN && nctsHeader.EDocPivotCollection.Count == 0)
			{
				departureMovementHeader.BM_InBondEntryTypeInfo.AddMessageError(Res.GetString("081831F0-CFCE-422B-BDB4-69164A58E03C", "A copy of TNN Document must be sent. Please add the document to eDocs tab in this declaration and then select the eDoc in the Annexes tab."));
			}
		}

		protected override void CheckBM_InlandTransportMode()
		{
			base.CheckBM_InlandTransportMode();
			var departureMovementHeader = Parent;
			if (departureMovementHeader.IsPhaseStatusTNN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_InlandTransportModeInfo, ResString.GetMultilingualString("D6136B84-9677-4B7D-8506-C5678EEEDBF7", "Inland M.O.T."));
			}
		}

		protected override void CheckMandatoryGuaranteeIfNeeded(EU.NCTS.Business.NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (!Parent.IsPhaseStatusTNN)
			{
				base.CheckMandatoryGuaranteeIfNeeded(nctsHeader, info);
			}
		}

		protected override void CheckGoodsLocationDescription()
		{
		}

		protected override void CheckBM_TransportAtDepartureType_RuleB1891_1()
		{
			if (!Parent.IsPhaseStatusTNN)
			{
				base.CheckBM_TransportAtDepartureType_RuleB1891_1();
			}
		}
	}
}
