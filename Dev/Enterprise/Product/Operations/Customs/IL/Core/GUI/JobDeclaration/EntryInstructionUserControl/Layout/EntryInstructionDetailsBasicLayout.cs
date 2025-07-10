using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	class EntryInstructionDetailsBasicLayout : IPanelLayoutProvider
	{
		#region IPanelLayoutProvider

		PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateLayout();
		PanelLayout layout;

		#endregion

		PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
			var commonBag = builder.CommonBag;
			var ilBag = EntryInstructionDetailsControlBag.Instance;
			builder.AddControlBag(ilBag);

			builder.AddColumn();
			builder.Add(ilBag.DateForDutyDateEdit, ControlWidthClass.Long);
			builder.Add(ilBag.FormattedProcedureDropEdit, ControlWidthClass.Long);
			builder.Add(ilBag.ToWarehouseAddressControl, ControlWidthClass.Long);
			builder.Add(ilBag.FromWarehouseAddressControl, ControlWidthClass.Long);
			builder.Add(ilBag.AutonomyRegionTypeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);
			builder.Add(ilBag.PackagesQtyCalcDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
