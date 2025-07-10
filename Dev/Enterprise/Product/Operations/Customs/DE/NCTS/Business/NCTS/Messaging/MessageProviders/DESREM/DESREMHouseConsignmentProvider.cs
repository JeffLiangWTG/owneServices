using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESREMHouseConsignmentProvider : IDESREMHouseConsignment
	{
		public DESREMHouseConsignmentProvider(NctsBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly NctsBill bill;

		public int SequenceNumber => int.Parse(bill.MovementDetail.B9_SeqNo);

		public decimal? GrossMass
		{
			get
			{
				decimal? result = decimal.Zero;
				var unloadedState = bill.MovementDetail.B9_UnloadedState;
				var effectiveWeight = ZWeight.Empty;
				if (unloadedState == NctsUnloadedStateList.Codes.NEW)
				{
					effectiveWeight = new ZWeight(bill.B0_Weight, bill.B0_WeightUQ);
				}
				else if (unloadedState == NctsUnloadedStateList.Codes.DIF && bill.MovementDetail.DifferenceMoveDetail is CusInBondMoveDetail differenceMoveDetail)
				{
					effectiveWeight = new ZWeight(bill.B0_GrossWeightUnloaded, differenceMoveDetail.DifferenceWeightUnit);
				}

				if (!effectiveWeight.IsEmpty)
				{
					result = effectiveWeight.InKilogramsSafe.Round(3).Normalize();
				}
				return result > decimal.Zero ? result : null;
			}
		}

		public IReadOnlyCollection<IDESREMTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		IReadOnlyCollection<IDESREMTransportMeans> departureTransportMeans;

		IDESREMTransportMeans[] GetDepartureTransportMeans()
			=> bill.ArrivalTransportInfos
				.Cast<ArrivalCusTransportMeans>()
				.Where(t => NCTSProviderHelpers.UnloadedStatusInNewMisDif(t.TPM_TransportState))
				.Select(t => DESREMTransportMeansProvider.NewOrNull(t))
				.ToArray();

		public IReadOnlyCollection<IDESREMConsignmentItem> Items => items ??= GetItems();
		IReadOnlyCollection<IDESREMConsignmentItem> items;

		IDESREMConsignmentItem[] GetItems()
		{
			var result = Array.Empty<IDESREMConsignmentItem>();
			if (bill.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.MIS)
			{
				result = bill.ArrivalGoodsItems
					.Where(g => NCTSProviderHelpers.UnloadedStatusInNewMisDif(g.BY_UnloadedState)
						|| g.Packages.Cast<NctsPackage>().Any(p => NCTSProviderHelpers.UnloadedStatusInNewMis(p.B5_TypeOfDifference)))
					.Select(g => new DESREMConsignmentItemProvider(g))
					.ToArray();
			}
			return result;
		}
	}
}
