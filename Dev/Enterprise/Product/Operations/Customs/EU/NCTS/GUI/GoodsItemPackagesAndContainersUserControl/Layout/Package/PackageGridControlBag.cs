using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class PackageGridControlBag
	{
		public PackageGridControlBag()
		{
			SequenceNumberTextBox = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_SequenceNumber, 60);
			PackageTypeDropEdit = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPackage.Schema.B5_UnitType, 60, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			NumberOfPackagesCalcEdit = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPackage.Schema.B5_UnitCount, 80);
			MarksAndNumbersTextBox = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_MarksAndNumbers, 200);
			PackageIDTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_PackageID, 80);
			BrandTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_Brand, 80);
			ModelTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPackage.Schema.B5_Model, 80);
		}

		public static PackageGridControlBag Instance => instance ?? (instance = new PackageGridControlBag());

		[ThreadStatic]
		static PackageGridControlBag instance;

		public IGridColumnReference SequenceNumberTextBox { get; }

		public IGridColumnReference PackageTypeDropEdit { get; }

		public IGridColumnReference NumberOfPackagesCalcEdit { get; }

		public IGridColumnReference MarksAndNumbersTextBox { get; }

		public IGridColumnReference PackageIDTextBoxColumn { get; }

		public IGridColumnReference BrandTextBoxColumn { get; }

		public IGridColumnReference ModelTextBoxColumn { get; }
	}
}
