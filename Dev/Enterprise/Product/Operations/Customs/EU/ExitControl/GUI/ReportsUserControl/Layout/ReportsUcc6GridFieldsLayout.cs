using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ReportsUcc6GridFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ReportsUcc6GridFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new ReportsGridFieldsLayoutBuilder<CusExitReport>();

		var euBag = ReportsGridFieldsControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.ConsignmentGuidDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.FormattedDateTimeDateEdit, ControlWidthClass.Medium);
		builder.Add(euBag.LocationCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.DiscrepanciesCheckBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(euBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.TransportTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.TransportIDTextBox, ControlWidthClass.Long);
		builder.Add(euBag.TransportNationalityDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.LocationOfGoodsUserControl, ControlWidthClass.Long);

		return builder.Build();
	}
}
