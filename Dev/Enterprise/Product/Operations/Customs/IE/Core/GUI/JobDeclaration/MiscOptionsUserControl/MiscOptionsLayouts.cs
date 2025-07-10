using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
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
			var euBag = EU.GUI.MiscOptionsControlBag.Instance;
			var ieBag = MiscOptionsControlBag.Instance;

			builder.AddControlBag(euBag);
			builder.AddControlBag(ieBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Medium);
			builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.EntryAuthorisationDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.RouteFRequestedCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.TrainingCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

			builder.Add(ieBag.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DefermentAccountNumberTextBox, ControlWidthClass.Long);

			builder.SetVisibility(euBag.RouteFRequestedCheckBox, d => !d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.TrainingCheckBox, d => !d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(ieBag.PaymentSeparatorUserControl, d => !d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.PaymentMethodDropEdit, d => !d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.DefermentAccountNumberTextBox, d => !d.IsExport && d.JE_PaymentMethod == PaymentMethodList.Codes.E, d => d.JE_MessageTypeInfo, d => d.JE_PaymentMethodInfo);

			return builder.Build();
		}
	}
}
