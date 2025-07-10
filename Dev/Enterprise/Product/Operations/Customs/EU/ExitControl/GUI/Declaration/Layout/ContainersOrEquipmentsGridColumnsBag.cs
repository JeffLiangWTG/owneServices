using System;
using System.Windows.Forms;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ContainersOrEquipmentsGridColumnsBag
	{
		public static ContainersOrEquipmentsGridColumnsBag Instance => instance ?? (instance = new ContainersOrEquipmentsGridColumnsBag());

		[ThreadStatic]
		static ContainersOrEquipmentsGridColumnsBag instance;

		ContainersOrEquipmentsGridColumnsBag()
		{
			ContainerNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusExitContainer.Schema.CXN_ContainerNumber, 120,
				c =>
				{
					c.CharacterCasing = CharacterCasing.Upper;
				});
			IsEquipmentCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(CusExitContainer.Schema.CXN_IsEquipment, 87);
			SequenceCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusExitContainer.Schema.CXN_Sequence, 75);
			StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusExitContainer.Schema.CXN_Status, 75);
			SealCountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusExitContainer.Schema.CXN_SealCount, 75,
				c => c.BindToDecimalPlaces = null);
		}

		public IGridColumnReference ContainerNumberTextBoxColumn { get; }
		public IGridColumnReference IsEquipmentCheckBoxColumn { get; }
		public IGridColumnReference SequenceCalcEditColumn { get; }
		public IGridColumnReference SealCountCalcEditColumn { get; }
		public IGridColumnReference StatusDropEditColumn { get; }
	}
}
