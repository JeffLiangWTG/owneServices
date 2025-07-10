using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
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
			var frBag = MiscOptionsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddControlBag(frBag);

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

			builder.Add(frBag.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.DefermentAccountNumberDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.CustomsGuaranteeNumberDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.VATDeferTypeDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.VATDeferNumberTextBox, ControlWidthClass.Long);
			builder.Add(frBag.VatCanaDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.ChargePaymentOrDestinationIDsDropEdit, ControlWidthClass.Long);

			builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
			builder.Add(frBag.SupportingInformationUserControl, ControlWidthClass.LongControl);

			builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
