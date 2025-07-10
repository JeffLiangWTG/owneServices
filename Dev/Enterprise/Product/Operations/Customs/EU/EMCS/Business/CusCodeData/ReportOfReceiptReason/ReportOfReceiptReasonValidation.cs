using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptReasonValidation : CusCodeDataValidation
	{
		public ReportOfReceiptReasonValidation(ReportOfReceiptReason parent) : base(parent)
		{
		}

		protected new ReportOfReceiptReason Parent => (ReportOfReceiptReason)base.Parent;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Code == EMCSReceiptReasonCodeList.Codes.Other)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			}
		}
	}
}
