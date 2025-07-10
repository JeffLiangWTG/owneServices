using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class MiscOptionsLayouts : IPanelLayoutProvider
{
	PanelLayout MiscOptions { get; }

	PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

	public MiscOptionsLayouts()
	{
		MiscOptions = CreateMiscOptionsLayouts();
	}

	PanelLayout CreateMiscOptionsLayouts()
	{
		var builder = new EU.GUI.MiscOptionsLayoutBuilder<Business.Declaration.JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.MiscOptionsControlBag.Instance;
		var beBag = MiscOptionsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(beBag);

		builder.AddColumn();
		builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.EntryAuthorisationDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.RouteFRequestedCheckBox, ControlWidthClass.Long);
		builder.Add(euBag.TrainingCheckBox, ControlWidthClass.Long);

		builder.Add(euBag.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DefermentAccountNumberTextBox, ControlWidthClass.Long);
		builder.Add(beBag.VATDeferTypeDropEdit, ControlWidthClass.Long);

		builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);

		builder.SetVisibility(beBag.VATDeferTypeDropEdit, d => d.IsInterface, d => d.JE_ApplicationCodeInfo);
		builder.SetVisibility(commonBag.PaidByDropEdit, d => d.IsInterface, d => d.JE_ApplicationCodeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.DefermentAccountNumberTextBox, d => d.IsImport && (d.JE_PaymentMethod == PaymentMethodList.Codes.Deferral || d.JE_PaymentMethod == PaymentMethodList.Codes.AgentCashAccount), d => d.JE_MessageTypeInfo, d => d.JE_PaymentMethodInfo);

		return builder.Build();
	}
}
