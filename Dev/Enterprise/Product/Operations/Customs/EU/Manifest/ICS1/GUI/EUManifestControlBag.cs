using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.GUI
{
	public sealed class EUManifestControlBag : ControlBag
	{
		public static EUManifestControlBag Instance => manifestControlBag.Value;

		public EUManifestControlBag()
		{
			SpecificCircumstanceIndicatorDropEdit = RegisterControl(nameof(EuropeanUnionFieldsUserControl.SpecificCircumstanceIndicatorDropEdit));
			MethodOfPaymentDropEdit = RegisterControl(nameof(EuropeanUnionFieldsUserControl.MethodOfPaymentDropEdit));
			SpecialMentionsDropEdit = RegisterControl(nameof(EuropeanUnionFieldsUserControl.SpecialMentionsDropEdit));
			ETAatFirstCustomsOfficeDateEdit = RegisterControl(nameof(EuropeanUnionFieldsUserControl.ETAatFirstCustomsOfficeDateEdit));
			ATAatFirstCustomsOfficeDateEdit = RegisterControl(nameof(EuropeanUnionFieldsUserControl.ATAatFirstCustomsOfficeDateEdit));
			EUCustomsOfficesUserControl = RegisterControl(nameof(EuropeanUnionFieldsUserControl.EUCustomsOfficesUserControl));
		}

		public ControlReference SpecificCircumstanceIndicatorDropEdit { get; }
		public ControlReference MethodOfPaymentDropEdit { get; }
		public ControlReference SpecialMentionsDropEdit { get; }
		public ControlReference ETAatFirstCustomsOfficeDateEdit { get; }
		public ControlReference ATAatFirstCustomsOfficeDateEdit { get; }
		public ControlReference EUCustomsOfficesUserControl { get; }

		protected override Control CreateTemplate() => new EuropeanUnionFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUManifestControlBag> manifestControlBag = new Lazy<EUManifestControlBag>(() => new EUManifestControlBag());
	}
}
