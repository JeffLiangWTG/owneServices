using CargoWise.Application;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class MiscOptionsLayout : IPanelLayoutProvider
{
	public MiscOptionsLayout()
	{
		Layout = CreateMiscOptionsLayout();
	}

	public PanelLayout Layout { get; }

	static PanelLayout CreateMiscOptionsLayout()
	{
		var builder = new EU.GUI.MiscOptionsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		var euBag = EU.GUI.MiscOptionsControlBag.Instance;
		builder.AddControlBag(euBag);

		var esBag = MiscOptionsControlBag.Instance;
		builder.AddControlBag(esBag);

		builder.AddColumn();
		builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Long);
		builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.EntryAuthorisationDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Long);
		builder.Add(esBag.AuthPerDeclarationCheckBox, ControlWidthClass.Long);
		builder.Add(esBag.DeclEmailAddrTextBox, ControlWidthClass.Long);
		builder.Add(esBag.OtherEmailAddrTextBox, ControlWidthClass.Long);
		builder.Add(euBag.RouteFRequestedCheckBox, ControlWidthClass.Long);
		builder.Add(esBag.DontSendImporterIdCheckBox, ControlWidthClass.Long);
		builder.Add(euBag.TrainingCheckBox, ControlWidthClass.Long);

		builder.Add(euBag.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);

		builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
		builder.Add(esBag.SupportingInformationUserControl, ControlWidthClass.LongControl);

		builder.SetVisibility(esBag.AuthPerDeclarationCheckBox, d => d.IsImport || d.IsExport);
		builder.SetVisibility(esBag.DeclEmailAddrTextBox, d => d.IsImport || d.IsExport);
		builder.SetVisibility(esBag.DontSendImporterIdCheckBox, d => d.IsExport);
		builder.SetVisibility(euBag.TrainingCheckBox, d => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());
		builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		return builder.Build();
	}
}
