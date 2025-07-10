using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSShipmentStatusProvider : Freight.CFS.Business.CFSShipmentStatusProvider, Integration.Customs.AU.ICFSShipmentStatusProvider
	{
		public CFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
			shipment.Factory.AddFetchHint(typeof(DepotCusOutturn), CusOutturnSchema.C5_ParentID, shipment.PK);
			this.shipment = shipment;
		}

		public static new CFSShipmentStatusProvider New(CFSShipment shipment)
		{
			return (CFSShipmentStatusProvider)Freight.CFS.Business.CFSShipmentStatusProvider.New(shipment);
		}

		readonly new CFSShipment shipment;

		#region Status

		#region Overrides

		protected override ZString StatusCore()
		{
			var cargoStatus = CargoStatus;
			return !cargoStatus.IsEmpty ? (ZString)GatePassStatuses.GetDescriptionFromCode(cargoStatus) : ZString.Empty;
		}

		protected override ZString ShortStatusCore() => CargoStatus;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:Avoid excessive complexity")]
		protected override StatusClass StatusClassCore()
		{
			StatusClass result = StatusClass.Held;

			switch (ShortStatus)
			{
				case CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
				case CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
				case CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction:
					result = StatusClass.Clear;
					break;

				case CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
				case CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
				case CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
					result = StatusClass.Warning;
					break;

				case CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl:
				case CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms:
				case CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine:
				case CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed:
				case CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn:
					result = StatusClass.Held;
					break;

				case CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived:
					result = StatusClass.Underbonded;
					break;
			}

			return result;
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			bool result = false;

			if (shipment.HasErrors)
			{
				ui.ShowError("Please clear all errors before saving.");
			}
			else
			{
				switch (ShortStatus)
				{
					case CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
					case CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
					case CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction:
						result = true;
						break;
					case CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
						result = ui.Ask("Confirm conditional clearance actions have been completed.\r\nHave the conditional clearance requirements been met?");
						break;
					case CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms:
						ui.ShowError("This shipment has been seized by customs and may not be gate passed.");
						break;
					case CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine:
						ui.ShowError("This shipment has been seized by Quarantine and may not be gate passed");
						break;
					case CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
					case CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
						ui.ShowWarning("This shipment is marked as high risk");
						result = true;
						break;
					case CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed:
						ui.ShowWarning("This shipment is clear to be moved underbond but may not be delivered for home consumption");
						result = true;
						break;
					case CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived:
						ui.ShowWarning("This shipment is clear to be moved underbond.");
						result = true;
						break;
					default:
						result = ui.Ask("This shipment has not been cleared by customs.  Continue with Contingency Release?");
						break;
				}
			}

			return result;
		}

		#endregion

		#region Cargo Status

		DepotCusOutturn RelatedOutturn
		{
			get { return relatedOutturn ?? (relatedOutturn = shipment.IsAir ? GetRelatedAirOutturn() : GetRelatedSeaOutturn()); }
		}
		DepotCusOutturn relatedOutturn;

		DepotCusOutturn GetRelatedSeaOutturn()
		{
			foreach (var outturn in ShipmentWrapper.Outturns.Cast<DepotCusOutturn>().OrderByDescending(x => x.C5_SystemCreateTimeUtc))
			{
				if (OutturnMatchesShipment(outturn))
				{
					return outturn;
				}
			}

			return null;
		}

		DepotCusOutturn GetRelatedAirOutturn()
		{
			var outturnQuery = new ZDBOnlyQuery(typeof(CusOutturn));
			outturnQuery.AddToFilter(CusOutturnSchema.C5_HouseBill, shipment.JS_HouseBill);
			var underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusOutturnSchema.C5_C4_Underbond);
			underbondQuery.AddToFilter(CusUnderbondSchema.C4_MAWB, shipment.JS_JK_MasterBillNum);

			if (shipment.JS_Calc_CurrentETA.IsValid)
			{
				var arrivalDate = shipment.JS_Calc_CurrentETA.EndOfDay().AddDays(-1);
				underbondQuery.AddToFilter(CusUnderbondSchema.C4_ArrivalDate, SQLComparisonOperator.GreaterThanOrEqualTo, arrivalDate);
			}

			outturnQuery.AddSubQuery(underbondQuery, JoinCondition.And);
			outturnQuery.OrderBy = CusOutturnSchema.C5_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			return shipment.Factory.LoadTop1<DepotCusOutturn>(outturnQuery);
		}

		ZString CargoStatus
		{
			get
			{
				var hawbStatus = shipment.IsAir ? CusHAWB.Load(shipment)?.CMRCargoStatus.Code ?? ZString.Empty : ZString.Empty;
				return !hawbStatus.IsEmpty && hawbStatus != AirCargoMessage.NewStatus.NotSent
					? hawbStatus
					: RelatedOutturn?.CombinedStatus.Code ?? ZString.Empty;
			}
		}

		protected bool OutturnMatchesShipment(DepotCusOutturn outturn)
		{
			bool result = true;
			result &= shipment.JS_HouseBill == outturn.C5_HouseBill.ToUpper();
			result &= !outturn.CombinedStatus.Code.IsEmpty;
			return result;
		}

		protected override ZString DetailsFromMessagesCore
		{
			get { return (RelatedOutturn != null ? RelatedOutturn.CombinedStatus.UserFriendlyStatuses : null); }
		}

		#endregion

		#endregion

		#region Status List

		protected CodeDescriptionPairList GatePassStatuses
		{
			get { return CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(shipment.Factory); }
		}

		#endregion

		#region Implementation

		#region ShipmentWrapper

		protected CFSShipmentWrapper ShipmentWrapper
		{
			get
			{
				if (shipmentWrapper == null)
				{
					shipmentWrapper = CFSShipmentWrapper.Load(shipment);
				}
				return shipmentWrapper;
			}
		}
		CFSShipmentWrapper shipmentWrapper;

		#endregion

		#endregion
	}
}
