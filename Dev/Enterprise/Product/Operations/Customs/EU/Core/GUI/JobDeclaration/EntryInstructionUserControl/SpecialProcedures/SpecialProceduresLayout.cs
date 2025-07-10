using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
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
			var builder = new SpecialProceduresLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PrimaryOwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.OwnerOfGoodsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.FirstPlaceOfUseOrProcessingUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.PlaceOfUseOrProcessingGoodsLocationUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.PeriodForDischargeUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BillOfDischargeUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ActivitiesAndProceduresUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.SpecialProceduresOthersUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.IdentificationOfGoodsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ConditionsAndTermsUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
