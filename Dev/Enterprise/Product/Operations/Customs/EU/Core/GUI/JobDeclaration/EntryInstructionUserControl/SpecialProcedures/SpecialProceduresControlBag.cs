using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class SpecialProceduresControlBag : ControlBag
	{
		public SpecialProceduresControlBag()
		{
			PrimaryOwnerOfGoodsUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.PrimaryOwnerOfGoodsUserControl));
			OwnerOfGoodsUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.OwnerOfGoodsUserControl));
			FirstPlaceOfUseOrProcessingUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.FirstPlaceOfUseOrProcessingUserControl));
			PlaceOfUseOrProcessingGoodsLocationUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.PlaceOfUseOrProcessingGoodsLocationUserControl));
			PeriodForDischargeUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.PeriodForDischargeUserControl));
			BillOfDischargeUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.BillOfDischargeUserControl));
			ActivitiesAndProceduresUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.ActivitiesAndProceduresUserControl));
			SpecialProceduresOthersUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.SpecialProceduresOthersUserControl));
			IdentificationOfGoodsUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.IdentificationOfGoodsUserControl));
			ConditionsAndTermsUserControl = RegisterControl(nameof(LayoutSpecialProceduresUserControl.ConditionsAndTermsUserControl));
		}

		public static SpecialProceduresControlBag Instance => instance ?? (instance = new SpecialProceduresControlBag());

		[ThreadStatic]
		static SpecialProceduresControlBag instance;

		public ControlReference PrimaryOwnerOfGoodsUserControl { get; }
		public ControlReference OwnerOfGoodsUserControl { get; }
		public ControlReference FirstPlaceOfUseOrProcessingUserControl { get; }
		public ControlReference PlaceOfUseOrProcessingGoodsLocationUserControl { get; }
		public ControlReference PeriodForDischargeUserControl { get; }
		public ControlReference BillOfDischargeUserControl { get; }
		public ControlReference ActivitiesAndProceduresUserControl { get; }
		public ControlReference SpecialProceduresOthersUserControl { get; }
		public ControlReference IdentificationOfGoodsUserControl { get; }
		public ControlReference ConditionsAndTermsUserControl { get; }

		protected override Control CreateTemplate() => new LayoutSpecialProceduresUserControl();
	}
}
