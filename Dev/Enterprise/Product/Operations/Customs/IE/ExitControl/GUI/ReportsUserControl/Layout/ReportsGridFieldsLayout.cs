using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ReportsGridFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ReportsGridFieldsLayout()
		{
			Layout = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var builder = new EU.ExitControl.GUI.ReportsGridFieldsLayoutBuilder<CusExitReport>();

			var euBag = EU.ExitControl.GUI.ReportsGridFieldsControlBag.Instance;
			var ieBag = ReportsGridFieldsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddControlBag(ieBag);

			builder.AddColumn();
			builder.Add(euBag.ConsignmentGuidDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.OfficeOfExportCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.FormattedDateTimeDateEdit, ControlWidthClass.Medium);
			builder.Add(ieBag.LongFormattedDateTimeDateEdit, ControlWidthClass.Medium);
			builder.Add(ieBag.DiscrepanciesCheckBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(ieBag.TypeOfLocationDropEdit, ControlWidthClass.Long);
			builder.Add(ieBag.UNLOCOCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.EnquiryInformationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(ieBag.DeclarantAddressDropEdit, ControlWidthClass.Long);
			builder.Add(ieBag.RepresentativeAddressDropEdit, ControlWidthClass.Long);
			builder.Add(ieBag.DeclarantTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(ieBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(ieBag.LocationTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(ieBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.TransportIDTextBox, ControlWidthClass.Long);
			builder.Add(euBag.TransportNationalityDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.TransportTypeDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(euBag.ConsignmentGuidDropEdit, report => report.CER_Status.IsEmpty && report.CER_MessageStatus != LogicalStatusList.Codes.Sent, report => report.CER_StatusInfo, report => report.CER_MessageStatusInfo);
			builder.SetVisibility(ieBag.OfficeOfExportCodeFindBox, report => report.IsInformationOnNonExitedExport, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.FormattedDateTimeDateEdit, report => !report.IsPresentation, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.LongFormattedDateTimeDateEdit, report => report.IsPresentation, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.DiscrepanciesCheckBox, report => !report.IsInformationOnNonExitedExport, report => report.CER_TypeInfo);

			builder.SetVisibility(ieBag.TypeOfLocationDropEdit, report => report.IsPresentation, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.UNLOCOCodeFindBox, report => report.IsPresentation, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.EnquiryInformationCodeDropEdit, report => report.IsInformationOnNonExitedExport, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.DeclarantAddressDropEdit, report => report.IsOrganisationFieldsRequired, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.RepresentativeAddressDropEdit, report => report.IsOrganisationFieldsRequired, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.DeclarantTypeDropEdit, report => report.IsOrganisationFieldsRequired, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.AdditionalDeclarationTypeDropEdit, report => report.IsExitNotification, report => report.CER_TypeInfo);
			builder.SetVisibility(ieBag.LocationTextBox, report => report.IsPresentation, report => report.CER_TypeInfo);

			builder.SetVisibility(ieBag.TransportModeDropEdit, report => report.IsTransportFieldsRequired, report => report.CER_TransportModeInfo);
			builder.SetVisibility(euBag.TransportIDTextBox, report => report.IsTransportFieldsRequired, report => report.CER_TypeInfo, report => report.CER_BehaviorInfo);
			builder.SetVisibility(euBag.TransportNationalityDropEdit, report => report.IsTransportFieldsRequired, report => report.CER_TypeInfo, report => report.CER_BehaviorInfo);
			builder.SetVisibility(euBag.TransportTypeDropEdit, report => report.IsTransportFieldsRequired, report => report.CER_TypeInfo, report => report.CER_BehaviorInfo);

			return builder.Build();
		}
	}
}
