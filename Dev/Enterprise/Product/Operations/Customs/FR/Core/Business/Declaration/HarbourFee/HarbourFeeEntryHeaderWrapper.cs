#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class HarbourFeeEntryHeaderWrapper : HarbourFeeWrapper
	{
		readonly List<EU.Business.Declaration.CusContainer> containers;

		public HarbourFeeEntryHeaderWrapper(JobDeclaration declaration) : base()
		{
			Declaration = Argument.NotNull(declaration, "declaration");
			containers = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(i => i.ContainersPivot.Select(p => p.Container)).DistinctBy(s => s.PK).Cast<EU.Business.Declaration.CusContainer>().ToList();
		}

		public JobDeclaration Declaration { get; }

		protected override int NumberOfTwentyFootLCLContainersCore => (numberOfTwentyFootLCLContainers ?? (numberOfTwentyFootLCLContainers = GetContainersOfLengthCount(20, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfTwentyFootLCLContainers;

		protected override int NumberOfFortyFootLCLContainersCore => (numberOfFortyFootLCLContainers ?? (numberOfFortyFootLCLContainers = GetContainersOfLengthCount(40, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfFortyFootLCLContainers;

		protected override int NumberOfFortyFiveFootLCLContainersCore => (numberOfFortyFiveFootLCLContainers ?? (numberOfFortyFiveFootLCLContainers = GetContainersOfLengthCount(45, Core.Constants.ContainerModes.LCL))).Value;
		int? numberOfFortyFiveFootLCLContainers;

		protected override ZDecimal TotalLCLContainersMassInTonnesCore => (totalContainerMassInTonnesLCL ?? (totalContainerMassInTonnesLCL = containers.Where(x => x.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.LCL).Sum(x => Math.Ceiling(Core.Constants.Weight.ConvertSafe(x.CO_Weight, x.CO_WeightUQ, Core.Constants.Weight.Tonnes))))).Value;
		ZDecimal? totalContainerMassInTonnesLCL;

		protected override int NumberOfTwentyFootFCLContainersCore => (numberOfTwentyFootFCLContainers ?? (numberOfTwentyFootFCLContainers = GetContainersOfLengthCount(20, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfTwentyFootFCLContainers;

		protected override int NumberOfFortyFootFCLContainersCore => (numberOfFortyFootFCLContainers ?? (numberOfFortyFootFCLContainers = GetContainersOfLengthCount(40, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfFortyFootFCLContainers;

		protected override int NumberOfFortyFiveFootFCLContainersCore => (numberOfFortyFiveFootFCLContainers ?? (numberOfFortyFiveFootFCLContainers = GetContainersOfLengthCount(45, Core.Constants.ContainerModes.FCL))).Value;
		int? numberOfFortyFiveFootFCLContainers;

		protected override ZDecimal TotalFCLContainersMassInTonnesCore => (totalContainerMassInTonnesFCL ?? (totalContainerMassInTonnesFCL = containers.Where(x => x.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.FCL).Sum(x => Math.Ceiling(Core.Constants.Weight.ConvertSafe(x.CO_Weight, x.CO_WeightUQ, Core.Constants.Weight.Tonnes))))).Value;
		ZDecimal? totalContainerMassInTonnesFCL;

		protected override bool IsDangerousGoodsCore => (isDangerousGoods ?? (isDangerousGoods = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.UNDGs.Any()))).Value;
		bool? isDangerousGoods;

		int GetContainersOfLengthCount(int lengthInFeet, string containerMode) => containers.Where(x => x.CO_Ref_StorageClass.StartsWith(lengthInFeet.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) && x.CO_FCL_LCL_AIR == containerMode).Count();

		protected override DateTime DateOfValuationCore => Declaration.DateOfValuation.ToDateTime();

		protected override decimal CustomsValueCore => Declaration.TotalCustomsValueInLocalCurrency;

		protected override int ContainerCountCore => containers.Count;
	}
}
