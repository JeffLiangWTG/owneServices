using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageBillDetailControlBag : ControlBag
	{
		protected UCC6TemporaryStorageBillDetailControlBag()
		{
			TypeDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.typeDropEdit));
			BillNumberTextEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.billNumberTextBox));
			UCRNumberTextEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.uCRNumberTextBox));
			GrossWeightWithUnitUserControl = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.uCC6TemporaryStorageGrossWeightWithUnitUserControl));
			ConsignorAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.consignorAddressControl));
			ConsigneeAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.consigneeAddressControl));
			NotifyPartyAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.notifyPartyAddressControl));
		}

		public ControlReference TypeDropEdit { get; }

		public ControlReference BillNumberTextEdit { get; }

		public ControlReference UCRNumberTextEdit { get; }

		public ControlReference GrossWeightWithUnitUserControl { get; }

		public ControlReference ConsignorAddressControl { get; }

		public ControlReference ConsigneeAddressControl { get; }

		public ControlReference NotifyPartyAddressControl { get; }

		public static UCC6TemporaryStorageBillDetailControlBag Instance => uCC6TemporaryStorageBillDetailControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStorageBillDetailControlBag> uCC6TemporaryStorageBillDetailControlBag = new Lazy<UCC6TemporaryStorageBillDetailControlBag>(() => new UCC6TemporaryStorageBillDetailControlBag());

		protected override Control CreateTemplate() => new UCC6TemporaryStorageBillDetailControl();
	}
}
