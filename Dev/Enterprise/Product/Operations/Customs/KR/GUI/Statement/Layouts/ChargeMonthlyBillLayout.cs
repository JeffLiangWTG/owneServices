using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ChargeMonthlyBillLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new StatementHeaderLayoutBuilder<CusStatementHeader>();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.FormattedNumberTextBox, ControlWidthClass.Auto);
			builder.Add(controlBag.ProcessPortCodeFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.ProcessAndDueDateUserControl, ControlWidthClass.Auto);
			builder.Add(controlBag.PaymentDateEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.StatementAmountCalcEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.ImporterGuidFindBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(controlBag.StatusDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.StatementTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.PeriodDateUserControl, ControlWidthClass.Auto);
			builder.Add(controlBag.RelatedFormattedAccountNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.RelatedProcessDateEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
