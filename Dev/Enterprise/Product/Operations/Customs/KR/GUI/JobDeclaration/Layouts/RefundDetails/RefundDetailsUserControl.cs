using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class RefundDetailsUserControl : ZUserControl
	{
		public RefundDetailsUserControl()
		{
			InitializeComponent();
		}

		public void BindToMessageSending()
		{
			BindingSource.DataSourceType = typeof(JobDeclarationAmendmentMessageSendingObjectParent);

			BindingSource.SetBindingMember(TotalPaidAmountCalcEdit, "SendingObjectsCollection.Header.Statement929+TotalAmount");
			BindingSource.SetBindingMember(TotalRefundAmountCalcEdit, "SendingObjectsCollection.TotalAmountOfRefundAmount");

			BindingSource.SetBindingMember(PaidDutyAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+DutyAmount");
			BindingSource.SetBindingMember(RefundDutyAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfDutyAmount");

			BindingSource.SetBindingMember(PaidLiquorTaxAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+LiquorTax");
			BindingSource.SetBindingMember(RefundLiquorTaxAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfLiquorTax");

			BindingSource.SetBindingMember(PaidSpecialConsumptionTaxAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+SpecialConsumptionTax");
			BindingSource.SetBindingMember(RefundSpecialConsumptionTaxAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfSpecialConsumptionTax");

			BindingSource.SetBindingMember(PaidTransportTaxAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+TransportationTax");
			BindingSource.SetBindingMember(RefundTransportTaxAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfTransportationTax");

			BindingSource.SetBindingMember(PaidEducationTaxAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+EducationTax");
			BindingSource.SetBindingMember(RefundEducationTaxAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfEducationTax");

			BindingSource.SetBindingMember(PaidAgricultureTaxAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+AgricultureTax");
			BindingSource.SetBindingMember(RefundAgricultureTaxAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfAgricultureTax");

			BindingSource.SetBindingMember(PaidVATAmountCalcEdit, "SendingObjectsCollection.Header.StatementLine929+VAT");
			BindingSource.SetBindingMember(RefundVATAmountCalcEdit, "SendingObjectsCollection.RefundAmountOfVAT");

			BindingSource.SetBindingMember(PaidTotalPenaltyCalcEdit, "SendingObjectsCollection.Header.StatementLine929+PenaltyAndInterest");

			BindingSource.SetBindingMember(PaidLatePaymentPenaltyCalcEdit, "SendingObjectsCollection.Header.StatementLine929+PenaltyForLatePayment");
			BindingSource.SetBindingMember(TotalLateRefundAmountCalcEdit, "SendingObjectsCollection.TotalLateRefundAmount");

			BindingSource.SetBindingMember(PenaltyForLateDeclarationCalcEdit, "SendingObjectsCollection.RefundAmountOfPenaltyForLateDeclaration");

			BindingSource.SetBindingMember(PenaltyForMissedDeclarationCalcEdit, "SendingObjectsCollection.RefundAmountOfPenaltyForMissedDeclaration");

			BindingSource.SetBindingMember(PenaltyForLatePaymentCalcEdit, "SendingObjectsCollection.RefundAmountOfPenaltyForLatePayment");

			BindingSource.SetBindingMember(NonDutyTaxRevenueCalcEdit, "SendingObjectsCollection.RefundAmountOfNonDutyTaxRevenue");
		}
	}
}
