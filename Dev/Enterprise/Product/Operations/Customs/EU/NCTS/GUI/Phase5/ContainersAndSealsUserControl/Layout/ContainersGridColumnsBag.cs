using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class ContainersGridColumnsBag
	{
		public static ContainersGridColumnsBag Instance => instance ?? (instance = new ContainersGridColumnsBag());

		[ThreadStatic]
		static ContainersGridColumnsBag instance;

		public ContainersGridColumnsBag()
		{
			SequenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureHeaderContainer.Schema.BC_SequenceNumber, 100);
			ModeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureHeaderContainer.Schema.BC_Mode, 100, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			ContainerNumTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureHeaderContainer.Schema.BC_ContainerNum, 175);
			Seal1TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(NctsDepartureHeaderContainer.Seal1), 110);
			Seal2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(NctsDepartureHeaderContainer.Seal2), 110);
			TotalSealCountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(NctsDepartureHeaderContainer.TotalSealCount), 95, c => c.BindToDecimalPlaces = null);
		}

		public IGridColumnReference SequenceNumberTextBoxColumn { get; }

		public IGridColumnReference ModeDropEditColumn { get; }

		public IGridColumnReference ContainerNumTextBoxColumn { get; }

		public IGridColumnReference Seal1TextBoxColumn { get; }

		public IGridColumnReference Seal2TextBoxColumn { get; }

		public IGridColumnReference TotalSealCountCalcEditColumn { get; }
	}
}
