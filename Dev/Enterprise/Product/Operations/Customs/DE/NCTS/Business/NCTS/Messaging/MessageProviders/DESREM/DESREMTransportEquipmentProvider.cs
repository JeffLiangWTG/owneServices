using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DESREMTransportEquipmentProvider : IDESREMTransportEquipment
	{
		public static DESREMTransportEquipmentProvider NewOrNull(NctsArrivalHeaderContainer arrivalHeaderContainer) =>
			arrivalHeaderContainer != null
				? new DESREMTransportEquipmentProvider(arrivalHeaderContainer)
				: null;

		DESREMTransportEquipmentProvider(NctsArrivalHeaderContainer arrivalHeaderContainer)
		{
			this.arrivalHeaderContainer = arrivalHeaderContainer;
		}

		public int SequenceNumber => arrivalHeaderContainer.BC_SequenceNumber;

		public string IdentificationNumber => UnloadedStateIsDifOrNew ? (string)arrivalHeaderContainer.BC_ContainerNum : null;

		public int? NumberOfSeals => ShouldMapSeals ? Seals.Count : null;

		public IReadOnlyCollection<IDESREMSeal> Seals
		{
			get
			{
				if (seals == null)
				{
					if (ShouldMapSeals)
					{
						seals = arrivalHeaderContainer.Seals
							.Where(SealsUnloadingStateIsDecOrNew)
							.Select((e, i) => DESREMSealProvider.NewOrNull(e, i + 1))
							.ToArray();
					}
					else
					{
						seals = Array.Empty<IDESREMSeal>();
					}
				}
				return seals;
			}
		}

		bool ShouldMapSeals => CachedValueHelper.GetValue(ref shouldMapSeals,
			() => arrivalHeaderContainer.BC_UnloadedState != NctsUnloadedStateList.Codes.MIS &&
			!arrivalHeaderContainer.NctsArrival.ArrivalMovementHeader.BM_StateOfSealsBoolean);

		CachedValue<bool> shouldMapSeals;

		IReadOnlyCollection<IDESREMSeal> seals;

		public IReadOnlyCollection<int> GoodsReferences => goodsReferences ?? (goodsReferences = GetGoodsReferences());
		IReadOnlyCollection<int> goodsReferences;

		IReadOnlyCollection<int> GetGoodsReferences() => ShouldMapGoodsReferences
			? arrivalHeaderContainer.NctsArrival.Bills
				.SelectMany(x => x.ArrivalGoodsItems).Where(GoodsItemsUnloadingStateIsNotNewOrMis)
				.SelectMany(x => x.Packages).Cast<NctsPackage>().Where(x => PackagesTypeOfDifferenceNotMis(x) && x.ContainersSelected.Contains(arrivalHeaderContainer.BC_ContainerNum))
				.Select(x => (int)x.Parent.BY_DeclarationGoodsItemNumber)
				.Distinct()
				.ToArray()
			: Array.Empty<int>();

		bool ShouldMapGoodsReferences => CachedValueHelper.GetValue(ref shouldMapGoodsReferences, () => UnloadedStateIsDifOrNew && arrivalHeaderContainer.NctsArrival.ArrivalHeaderContainers.Where(x => x.BC_UnloadedState != NctsUnloadedStateList.Codes.MIS).Count() > 1);
		CachedValue<bool> shouldMapGoodsReferences;

		readonly NctsArrivalHeaderContainer arrivalHeaderContainer;

		bool UnloadedStateIsDifOrNew => CachedValueHelper.GetValue(ref unloadedStateIsDifOrNew, () => NCTSProviderHelpers.UnloadedStatusInNewDif(arrivalHeaderContainer.BC_UnloadedState));
		CachedValue<bool> unloadedStateIsDifOrNew;

		bool SealsUnloadingStateIsDecOrNew(CusSeal seal) => seal.BK_UnloadingState.In(new ZString[] { NctsUnloadedStateList.Codes.DEC, NctsUnloadedStateList.Codes.NEW });

		bool GoodsItemsUnloadingStateIsNotNewOrMis(NctsCommonCargoDesc goodsItem) => !goodsItem.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS });

		bool PackagesTypeOfDifferenceNotMis(NctsPackage package) => package.B5_TypeOfDifference != NctsUnloadedStateList.Codes.MIS;
	}
}
