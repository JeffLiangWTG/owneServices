using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class SpecialProceduresLayout : IPanelLayoutProvider
	{
		public SpecialProceduresLayout()
		{
			Layout = CreateSpecialProceduresLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateSpecialProceduresLayout()
		{
			var builder = new SpecialProceduresLayoutBuilder();
			var commonBag = builder.CommonBag;
			var ieBag = builder.IEBag;

			builder.AddColumn();
			builder.Add(ieBag.PrimaryOwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);

			builder.Add(ieBag.OwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(ieBag.FirstPlaceOfUseOrProcessingUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(ieBag.PlaceOfUseOrProcessingGoodsLocationUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(ieBag.PeriodForDischargeUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(ieBag.BillOfDischargeUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ActivitiesAndProceduresUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.SpecialProceduresOthersUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(ieBag.IdentificationOfGoodsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(ieBag.ConditionsAndTermsUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
