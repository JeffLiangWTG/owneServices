using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class MiscOptionsLayout : IPanelLayoutProvider
	{
		public MiscOptionsLayout()
		{
			MiscOptions = CreateMiscOptionsLayouts();
		}

		PanelLayout MiscOptions { get; }

		PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

		static PanelLayout CreateMiscOptionsLayouts()
		{
			var builder = new EU.GUI.MiscOptionsLayoutBuilder<EU.Business.Declaration.JobDeclaration>();
			var commonBag = builder.CommonBag;

			var euBag = EU.GUI.MiscOptionsControlBag.Instance;
			builder.AddControlBag(euBag);

			var deBag = MiscOptionsLayoutControlBag.Instance;
			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.StatisticStatusDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.VATClaimBackDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Long);

			builder.Add(euBag.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.DutyAccountNumberDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.VatPaymentPartyDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.VATAccountNumberDropEdit, ControlWidthClass.Long);

			builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);

			builder.SetVisibility(deBag.StatisticStatusDropEdit, h => h.IsImport);
			builder.SetVisibility(deBag.VATClaimBackDropEdit, h => h.IsImport);
			builder.SetVisibility(euBag.DeferralSeparatorUserControl, h => h.IsImport);
			builder.SetVisibility(euBag.PaymentMethodDropEdit, h => h.IsImport);
			builder.SetVisibility(deBag.VATAccountNumberDropEdit, h => h.IsImport);
			builder.SetVisibility(deBag.VatPaymentPartyDropEdit, h => h.IsImport);
			builder.SetVisibility(deBag.DutyAccountNumberDropEdit, h => h.IsImport);
			builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
