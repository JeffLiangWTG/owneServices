using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class EntryInstructionBasicDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new EntryInstructionBasicDetailsControl();

		public static EntryInstructionBasicDetailsControlBag Instance => instance ?? (instance = new EntryInstructionBasicDetailsControlBag());

		[ThreadStatic]
		static EntryInstructionBasicDetailsControlBag instance;

		EntryInstructionBasicDetailsControlBag()
		{
			LocationOfGoodsUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.LocationOfGoodsUserControl));
			ToWarehouseUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseUserControl));
			ToWarehouseLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseLabel));
			ToWarehouseTypeTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseTypeTextBox));
			ToWarehouseCodeTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseCodeTextBox));
			FromWarehouseLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseLabel));
			FromWarehouseUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseUserControl));
			FromWarehouseTypeTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseTypeTextBox));
			FromWarehouseCodeTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseCodeTextBox));
			NewOwnerOrganisationControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.NewOwnerOrganisationControl));
			AcceptanceDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.AcceptanceDateEdit));
			RequestedDocumentsGroupBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.RequestedDocumentsGroupBox));
			ToWarehouseAddressControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseAddressControl));
			FromWarehouseAddressControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseAddressControl));
		}

		public ControlReference LocationOfGoodsUserControl { get; }

		public ControlReference ToWarehouseUserControl { get; }

		public ControlReference ToWarehouseLabel { get; }

		public ControlReference ToWarehouseTypeTextBox { get; }

		public ControlReference ToWarehouseCodeTextBox { get; }

		public ControlReference FromWarehouseLabel { get; }

		public ControlReference FromWarehouseUserControl { get; }

		public ControlReference FromWarehouseTypeTextBox { get; }

		public ControlReference FromWarehouseCodeTextBox { get; }

		public ControlReference NewOwnerOrganisationControl { get; }

		public ControlReference AcceptanceDateEdit { get; }

		public ControlReference RequestedDocumentsGroupBox { get; }

		public ControlReference ToWarehouseAddressControl { get; }

		public ControlReference FromWarehouseAddressControl { get; }
	}
}
