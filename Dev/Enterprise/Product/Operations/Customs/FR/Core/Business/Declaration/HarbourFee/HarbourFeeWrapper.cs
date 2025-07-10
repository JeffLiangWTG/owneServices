using System;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class HarbourFeeWrapper
	{
		public HarbourFeeWrapper()
		{
		}

		public int NumberOfTwentyFootLCLContainers => NumberOfTwentyFootLCLContainersCore;
		protected abstract int NumberOfTwentyFootLCLContainersCore { get; }

		public int NumberOfFortyFootLCLContainers => NumberOfFortyFootLCLContainersCore;
		protected abstract int NumberOfFortyFootLCLContainersCore { get; }

		public int NumberOfFortyFiveFootLCLContainers => NumberOfFortyFiveFootLCLContainersCore;
		protected abstract int NumberOfFortyFiveFootLCLContainersCore { get; }

		public ZDecimal TotalLCLContainersMassInTonnes => TotalLCLContainersMassInTonnesCore;
		protected abstract ZDecimal TotalLCLContainersMassInTonnesCore { get; }

		public int NumberOfTwentyFootFCLContainers => NumberOfTwentyFootFCLContainersCore;
		protected abstract int NumberOfTwentyFootFCLContainersCore { get; }

		public int NumberOfFortyFootFCLContainers => NumberOfFortyFootFCLContainersCore;
		protected abstract int NumberOfFortyFootFCLContainersCore { get; }

		public int NumberOfFortyFiveFootFCLContainers => NumberOfFortyFiveFootFCLContainersCore;
		protected abstract int NumberOfFortyFiveFootFCLContainersCore { get; }

		public ZDecimal TotalFCLContainersMassInTonnes => TotalFCLContainersMassInTonnesCore;
		protected abstract ZDecimal TotalFCLContainersMassInTonnesCore { get; }

		public bool IsDangerousGoods => IsDangerousGoodsCore;
		protected abstract bool IsDangerousGoodsCore { get; }

		public DateTime DateOfValuation => DateOfValuationCore;
		protected abstract DateTime DateOfValuationCore { get; }

		public decimal CustomsValue => CustomsValueCore;
		protected abstract decimal CustomsValueCore { get; }

		public int ContainerCount => ContainerCountCore;
		protected abstract int ContainerCountCore { get; }
	}
}
