using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7ManifestControlBag : ControlBag
	{
		public static EUH7ManifestControlBag Instance => manifestControlBag.Value;

		public EUH7ManifestControlBag()
		{
			CustomsOfficeLabel = RegisterControl(nameof(EUH7ManifestFieldsUserControl.CustomsOfficeLabel));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(EUH7ManifestFieldsUserControl.CustomsOfficeCodeFindBox));
			PresentationOfficeCodeFindBox = RegisterControl(nameof(EUH7ManifestFieldsUserControl.PresentationOfficeCodeFindBox));
			SubmitTypeDropEdit = RegisterControl(nameof(EUH7ManifestFieldsUserControl.SubmitTypeDropEdit));
			PresenterAddressControl = RegisterControl(nameof(EUH7ManifestFieldsUserControl.PresenterAddressControl));
			ConsolidatedStatusSeparatorUserControl = RegisterControl(nameof(EUH7ManifestFieldsUserControl.ConsolidatedStatusSeparatorUserControl));
			ConsolidatedCustomsStatusDropEdit = RegisterControl(nameof(EUH7ManifestFieldsUserControl.ConsolidatedCustomsStatusDropEdit));
		}

		public ControlReference CustomsOfficeLabel { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference PresentationOfficeCodeFindBox { get; }
		public ControlReference SubmitTypeDropEdit { get; }
		public ControlReference PresenterAddressControl { get; }
		public ControlReference ConsolidatedStatusSeparatorUserControl { get; }
		public ControlReference ConsolidatedCustomsStatusDropEdit { get; }

		protected override Control CreateTemplate() => new EUH7ManifestFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUH7ManifestControlBag> manifestControlBag = new Lazy<EUH7ManifestControlBag>(() => new EUH7ManifestControlBag());
	}
}
