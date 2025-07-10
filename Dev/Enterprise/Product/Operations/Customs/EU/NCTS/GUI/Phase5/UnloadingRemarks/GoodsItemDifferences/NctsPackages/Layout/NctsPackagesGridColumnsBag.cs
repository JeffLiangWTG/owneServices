using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class NctsPackagesGridColumnsBag
	{
		public static NctsPackagesGridColumnsBag Instance => instance ?? (instance = new NctsPackagesGridColumnsBag());

		[ThreadStatic]
		static NctsPackagesGridColumnsBag instance;

		public NctsPackagesGridColumnsBag()
		{
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPackage.Schema.B5_SequenceNumber, 80,
			c =>
			{
				c.IsReadOnly = true;
				c.Decimals = 0;
			});
			TypeOfDifferenceDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPackage.Schema.B5_TypeOfDifference, 80);
			UnitCountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPackage.Schema.B5_UnitCount, 80,
			c =>
			{
				c.BindToDecimalPlaces = null;
				c.Decimals = 0;
			});
			UnitTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPackage.Schema.B5_UnitType, 80);
			MarksAndNumbersTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_MarksAndNumbers, 80);
			PackageIDTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_PackageID, 80);
			BrandTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_Brand, 80);
			ModelTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_Model, 80);
			GrossWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPackage.Schema.B5_GrossWeight, 80,
			c =>
			{
				c.Decimals = NctsPackage.Schema.B5_GrossWeightDecimalPlaces;
			});
			GrossWeightUQDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPackage.Schema.B5_GrossWeightUQ, 80);
		}

		public IGridColumnReference SequenceNumberCalcEditColumn { get; }

		public IGridColumnReference TypeOfDifferenceDropEditColumn { get; }

		public IGridColumnReference UnitCountCalcEditColumn { get; }

		public IGridColumnReference UnitTypeDropEditColumn { get; }

		public IGridColumnReference MarksAndNumbersTextBoxColumn { get; }

		public IGridColumnReference PackageIDTextBoxColumn { get; }

		public IGridColumnReference BrandTextBoxColumn { get; }

		public IGridColumnReference ModelTextBoxColumn { get; }

		public IGridColumnReference GrossWeightCalcEditColumn { get; }

		public IGridColumnReference GrossWeightUQDropEditColumn { get; }
	}
}
