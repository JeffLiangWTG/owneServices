using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class EntryInstructionControlBag : ControlBag
	{
		public static EntryInstructionControlBag Instance => instance ?? (instance = new EntryInstructionControlBag());

		[ThreadStatic]
		static EntryInstructionControlBag instance;

		public EntryInstructionControlBag()
		{
			TradeTypePanel = RegisterControl(nameof(TradeTypePanel));
			CustomsInspectionCodeTextBox = RegisterControl(nameof(CustomsInspectionCodeTextBox));
			WeightzCalcDropEdit = RegisterControl(nameof(WeightzCalcDropEdit));
			CargoQuantityCalcDropEdit = RegisterControl(nameof(CargoQuantityCalcDropEdit));
			ContainerCountCalcEdit = RegisterControl(nameof(ContainerCountCalcEdit));
			CustomsWeightCalcDropEdit = RegisterControl(nameof(CustomsWeightCalcDropEdit));
			ValueTypeDropEdit = RegisterControl(nameof(ValueTypeDropEdit));
			DeclarationTypeDropEdit = RegisterControl(nameof(DeclarationTypeDropEdit));
			AdditionalDeclarationTypeDropEdit = RegisterControl(nameof(AdditionalDeclarationTypeDropEdit));
		}

		protected override Control CreateTemplate() => new EntryInstructionLayoutTemplate();

		public ControlReference TradeTypePanel { get; }
		public ControlReference CustomsInspectionCodeTextBox { get; }
		public ControlReference WeightzCalcDropEdit { get; }
		public ControlReference CargoQuantityCalcDropEdit { get; }
		public ControlReference ContainerCountCalcEdit { get; }
		public ControlReference CustomsWeightCalcDropEdit { get; }
		public ControlReference ValueTypeDropEdit { get; }
		public ControlReference DeclarationTypeDropEdit { get; }
		public ControlReference AdditionalDeclarationTypeDropEdit { get; }
	}
}
