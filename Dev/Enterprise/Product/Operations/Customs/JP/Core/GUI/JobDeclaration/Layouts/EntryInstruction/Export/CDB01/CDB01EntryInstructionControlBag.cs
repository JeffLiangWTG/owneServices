using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CDB01EntryInstructionControlBag : ControlBag
	{
		public static CDB01EntryInstructionControlBag Instance => instance ??= new CDB01EntryInstructionControlBag();

		[ThreadStatic]
		static CDB01EntryInstructionControlBag instance;

		CDB01EntryInstructionControlBag()
		{
			CDB01BillNumberUserControl = RegisterControl(nameof(CDB01BillNumberUserControl));
			DateForDutyDateEdit = RegisterControl(nameof(DateForDutyDateEdit));
			CDB01CargoTypeDropEdit = RegisterControl(nameof(CDB01CargoTypeDropEdit));
			CDB01PermitNumberTextBox = RegisterControl(nameof(CDB01PermitNumberTextBox));
			CDB01MoveInUserControl = RegisterControl(nameof(CDB01MoveInUserControl));
			MAWBTextBox = RegisterControl(nameof(MAWBTextBox));
			PortOfLoadingPanel = RegisterControl(nameof(PortOfLoadingPanel));
			FinalDestinationPanel = RegisterControl(nameof(FinalDestinationPanel));
			ExternalBrokerGroupBox = RegisterControl(nameof(ExternalBrokerGroupBox));
			AirCargoAgentGroupBox = RegisterControl(nameof(AirCargoAgentGroupBox));
			ForwarderGroupBox = RegisterControl(nameof(ForwarderGroupBox));
			CarrierGroupBox = RegisterControl(nameof(CarrierGroupBox));
		}

		protected override Control CreateTemplate() => new CDB01EntryInstructionLayoutTemplate();

		public ControlReference CDB01BillNumberUserControl { get; }
		public ControlReference DateForDutyDateEdit { get; }
		public ControlReference CDB01CargoTypeDropEdit { get; }
		public ControlReference CDB01PermitNumberTextBox { get; }
		public ControlReference CDB01MoveInUserControl { get; }
		public ControlReference MAWBTextBox { get; }
		public ControlReference PortOfLoadingPanel { get; }
		public ControlReference FinalDestinationPanel { get; }
		public ControlReference ExternalBrokerGroupBox { get; }
		public ControlReference AirCargoAgentGroupBox { get; }
		public ControlReference ForwarderGroupBox { get; }
		public ControlReference CarrierGroupBox { get; }
	}
}
