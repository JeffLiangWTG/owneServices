using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	sealed class EntryInstructionDetailsControlBag : ControlBag
	{
		EntryInstructionDetailsControlBag()
		{
			DateForDutyDateEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.DateForDutyDateEdit));
			FormattedProcedureDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.FormattedProcedureDropEdit));
			AutonomyRegionTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.AutonomyRegionTypeDropEdit));
			ToWarehouseAddressControl = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.ToWarehouseAddressControl));
			FromWarehouseAddressControl = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.FromWarehouseAddressControl));
			PackagesQtyCalcDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.PackagesQtyCalcDropEdit));
		}

		#region Instance
		public static EntryInstructionDetailsControlBag Instance => instance ??= new EntryInstructionDetailsControlBag();

		[ThreadStatic]
		static EntryInstructionDetailsControlBag instance;

		#endregion

		#region ControlReference

		public ControlReference DateForDutyDateEdit { get; }
		public ControlReference FormattedProcedureDropEdit { get; }
		public ControlReference ToWarehouseAddressControl { get; }
		public ControlReference AutonomyRegionTypeDropEdit { get; }
		public ControlReference FromWarehouseAddressControl { get; }
		public ControlReference PackagesQtyCalcDropEdit { get; }

		#endregion

		protected override Control CreateTemplate() => new EntryInstructionDetailsBasicUserControl();
	}
}
