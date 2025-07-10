using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CDB01EntryInstructionLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new CDB01EntryInstructionLayoutBuilder();
				var cdb01EntryInstructionControlBag = builder.CommonBag;

				var commonBag = EntryInstructionControlBag.Instance;
				var ecrBag = ECREntryInstructionControlBag.Instance;
				var exportCommonBag = ExportEntryInstructionControlBag.Instance;
				var commonDeclarationBag = CommonDeclarationControlBag.Instance;
				builder.AddControlBag(commonBag);
				builder.AddControlBag(ecrBag);
				builder.AddControlBag(exportCommonBag);
				builder.AddControlBag(commonDeclarationBag);

				builder.AddColumn();
				builder.Add(cdb01EntryInstructionControlBag.CDB01BillNumberUserControl, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.DateForDutyDateEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.WeightzCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.CDB01CargoTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(ecrBag.SpecialCargoCodeFindBox, ControlWidthClass.Auto);
				builder.Add(exportCommonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.CDB01PermitNumberTextBox, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.CDB01MoveInUserControl, ControlWidthClass.LongNoCaption);

				builder.AddColumn();
				builder.Add(commonDeclarationBag.AllEntryInsSeparatorUserControl, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.MAWBTextBox, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.PortOfLoadingPanel, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.FinalDestinationPanel, ControlWidthClass.Auto);
				builder.Add(cdb01EntryInstructionControlBag.ExternalBrokerGroupBox, ControlWidthClass.LongNoCaption);
				builder.Add(cdb01EntryInstructionControlBag.AirCargoAgentGroupBox, ControlWidthClass.LongNoCaption);
				builder.Add(cdb01EntryInstructionControlBag.ForwarderGroupBox, ControlWidthClass.LongNoCaption);
				builder.Add(cdb01EntryInstructionControlBag.CarrierGroupBox, ControlWidthClass.LongNoCaption);
				return builder.Build();
			}
		}
	}
}
