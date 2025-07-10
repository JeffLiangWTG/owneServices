using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class MiscOptionsLayouts : IPanelLayoutProvider
	{
		PanelLayout MiscOptions { get; }

		PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

		public MiscOptionsLayouts()
		{
			MiscOptions = CreateMiscOptionsLayouts();
		}

		PanelLayout CreateMiscOptionsLayouts()
		{
			var builder = new MiscOptionsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var euBag = MiscOptionsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddColumn();

			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Medium);
			builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.EntryAuthorisationDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.RouteFRequestedCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.TrainingCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
			builder.Add(euBag.SupportingInformationUserControl, ControlWidthClass.LongControl);

			builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
