using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ImportEntryInstructionControlBag : ControlBag
	{
		public static ImportEntryInstructionControlBag Instance => instance ?? (instance = new ImportEntryInstructionControlBag());

		[ThreadStatic]
		static ImportEntryInstructionControlBag instance;

		ImportEntryInstructionControlBag()
		{
			DutyDrawbackDropEdit = RegisterControl(nameof(DutyDrawbackDropEdit));
			ContentInspectionResultDropEdit = RegisterControl(nameof(ContentInspectionResultDropEdit));
			BeforePermitApplicationReasonDropEdit = RegisterControl(nameof(BeforePermitApplicationReasonDropEdit));
			SpecialDeclarationOfficeGroupBox = RegisterControl(nameof(SpecialDeclarationOfficeGroupBox));
			BondedLocationCodeFindBox = RegisterControl(nameof(BondedLocationCodeFindBox));
			BondedLocationNameTextBox = RegisterControl(nameof(BondedLocationNameTextBox));
			DeclarationCargoTypeDropEdit = RegisterControl(nameof(DeclarationCargoTypeDropEdit));
			SpecialDeclarationTypeDropEdit = RegisterControl(nameof(SpecialDeclarationTypeDropEdit));
		}

		public ControlReference DutyDrawbackDropEdit { get; }
		public ControlReference ContentInspectionResultDropEdit { get; }
		public ControlReference BeforePermitApplicationReasonDropEdit { get; }
		public ControlReference SpecialDeclarationOfficeGroupBox { get; }
		public ControlReference BondedLocationCodeFindBox { get; }
		public ControlReference BondedLocationNameTextBox { get; }
		public ControlReference DeclarationCargoTypeDropEdit { get; }
		public ControlReference SpecialDeclarationTypeDropEdit { get; }

		protected override Control CreateTemplate() => new ImportEntryInstructionLayoutTemplate();
	}
}
