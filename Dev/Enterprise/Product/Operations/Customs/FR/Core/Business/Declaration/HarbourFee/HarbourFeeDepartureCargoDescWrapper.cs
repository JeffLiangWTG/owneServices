using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NctsDepartureCargoDesc = Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class HarbourFeeDepartureCargoDescWrapper : HarbourFeeWrapper
	{
		public HarbourFeeDepartureCargoDescWrapper(NctsDepartureMovementHeader movementHeader) : base()
		{
			this.MovementHeader = Argument.NotNull(movementHeader, "movementHeader");
		}

		IReadOnlyList<NctsDepartureCargoDesc> GoodsItem => MovementHeader.Header.DepartureGoodsItems;

		public NctsDepartureMovementHeader MovementHeader { get; }

		protected override int NumberOfTwentyFootLCLContainersCore => (numberOfTwentyFootLCLContainers ?? (numberOfTwentyFootLCLContainers = GetContainersOfLengthCount(20, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfTwentyFootLCLContainers;

		protected override int NumberOfFortyFootLCLContainersCore => (numberOfFortyFootLCLContainers ?? (numberOfFortyFootLCLContainers = GetContainersOfLengthCount(40, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfFortyFootLCLContainers;

		protected override int NumberOfFortyFiveFootLCLContainersCore => (numberOfFortyFiveFootLCLContainers ?? (numberOfFortyFiveFootLCLContainers = GetContainersOfLengthCount(45, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfFortyFiveFootLCLContainers;

		protected override ZDecimal TotalLCLContainersMassInTonnesCore => (totalContainerMassInTonnesLCL ?? (totalContainerMassInTonnesLCL = GetSumGrossWeightInTonnes(Core.Constants.ContainerModes.LCL))).Value;
		ZDecimal? totalContainerMassInTonnesLCL;

		protected override int NumberOfTwentyFootFCLContainersCore => (numberOfTwentyFootFCLContainers ?? (numberOfTwentyFootFCLContainers = GetContainersOfLengthCount(20, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfTwentyFootFCLContainers;

		protected override int NumberOfFortyFootFCLContainersCore => (numberOfFortyFootFCLContainers ?? (numberOfFortyFootFCLContainers = GetContainersOfLengthCount(40, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfFortyFootFCLContainers;

		protected override int NumberOfFortyFiveFootFCLContainersCore => (numberOfFortyFiveFootFCLContainers ?? (numberOfFortyFiveFootFCLContainers = GetContainersOfLengthCount(45, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfFortyFiveFootFCLContainers;

		protected override ZDecimal TotalFCLContainersMassInTonnesCore => (totalContainerMassInTonnesFCL ?? (totalContainerMassInTonnesFCL = GetSumGrossWeightInTonnes(Core.Constants.ContainerModes.FCL))).Value;
		ZDecimal? totalContainerMassInTonnesFCL;

		protected override bool IsDangerousGoodsCore => (isDangerousGoods ?? (isDangerousGoods = GoodsItem.Any(x => x.UNDGs.Any()))).Value;
		bool? isDangerousGoods;

		int GetContainersOfLengthCount(int lengthInFeet, string containerMode) => ContainersSelected.Count(x => x.BC_Mode == containerMode && x.Container is RefContainer refContainer && refContainer.RC_StorageClass.StartsWith(lengthInFeet.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

		ZDecimal GetSumGrossWeightInTonnes(string containerMode) => MovementHeader.Header.IsPhase4 ? GetSumGrossWeightInTonnesInPhase4(containerMode) : GetSumGrossWeightInTonnesInPhase5(containerMode);

		ZDecimal GetSumGrossWeightInTonnesInPhase4(string containerMode) => GoodsItem.Where(x => x.SelectedContainers.Any(y => y.BC_Mode == containerMode)).Sum(x => Math.Ceiling(Core.Constants.Weight.ConvertSafe(x.BY_GrossWeight, x.BY_GrossWeightUnit, Core.Constants.Weight.Tonnes)));

		ZDecimal GetSumGrossWeightInTonnesInPhase5 (string containerMode) => MovementHeader.Header.DepartureHeaderContainers.Any(x => x.BC_Mode == containerMode) ? Math.Ceiling(Core.Constants.Weight.ConvertSafe(MovementHeader.BM_GrossWeight, MovementHeader.BM_GrossWeightUQ, Core.Constants.Weight.Tonnes)) : 0;

		protected override DateTime DateOfValuationCore => MovementHeader.ValuationDate.ToDateTime();

		protected override decimal CustomsValueCore => GoodsItem.Sum(x => x.BY_MonetaryValue);

		protected override int ContainerCountCore => ContainersSelected.Count();

		IEnumerable<EU.NCTS.Business.NctsCusInBondContainer> ContainersSelected => GoodsItem.SelectMany(x => x.SelectedContainers).Distinct();
	}
}
