using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();

		MiscOptionsControlBag()
		{
			TrainingCheckBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.TrainingCheckBox));
			ShipmentTypeDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.ShipmentTypeDropEdit));
			RouteFRequestedCheckBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.RouteFRequestedCheckBox));
			LCPDepartDateEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.LCPDepartDateEdit));
			LCPInspectDateEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.LCPInspectDateEdit));
			RelatedDeclarationsUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.RelatedDeclarationsUserControl));
			SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.SupportingInformationUserControl));
			PaymentMethodDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentMethodDropEdit));
			DeferralSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.DeferralSeparatorUserControl));
			ItineraryCountriesUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.ItineraryCountriesUserControl));
			ItineraryCountriesSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.ItineraryCountriesSeparatorUserControl));
		}

		public ControlReference TrainingCheckBox { get; }
		public ControlReference ShipmentTypeDropEdit { get; }
		public ControlReference RouteFRequestedCheckBox { get; }
		public ControlReference LCPDepartDateEdit { get; }
		public ControlReference LCPInspectDateEdit { get; }
		public ControlReference RelatedDeclarationsUserControl { get; }
		public ControlReference SupportingInformationUserControl { get; }
		public ControlReference PaymentMethodDropEdit { get; }
		public ControlReference DeferralSeparatorUserControl { get; }
		public ControlReference ItineraryCountriesUserControl { get; }
		public ControlReference ItineraryCountriesSeparatorUserControl { get; }
	}
}
