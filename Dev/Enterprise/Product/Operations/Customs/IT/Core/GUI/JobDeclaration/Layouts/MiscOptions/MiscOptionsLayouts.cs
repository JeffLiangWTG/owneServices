using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

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
		var itBag = MiscOptionsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
		builder.Add(itBag.PreClearingCheckBox, ControlWidthClass.Long);
		builder.Add(itBag.BadgeCodeDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.SubscriberDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.EntryAuthorisationDateEdit, ControlWidthClass.Long);
		builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Long);

		builder.Add(euBag.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.DefermentAccountNumberDropEdit, ControlWidthClass.Long);

		builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
		builder.Add(itBag.SupportingInformationUserControl, ControlWidthClass.LongControl);

		builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(itBag.SubscriberDropEdit, d => !d.IsUCC6);
		builder.SetVisibility(itBag.PreClearingCheckBox, d => d.IsPreClearingEditable, d => d.JE_MessageTypeInfo, d => d.JE_TransportModeInfo);

		return builder.Build();
	}
}

