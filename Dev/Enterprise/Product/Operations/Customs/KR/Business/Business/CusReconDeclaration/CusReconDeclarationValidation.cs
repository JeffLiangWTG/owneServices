using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconDeclarationValidation : AutoKRCusReconDeclarationValidation
	{
		public CusReconDeclarationValidation(CusReconDeclaration parent)
			: base(parent)
		{
		}

		public new CusReconDeclaration Parent => (CusReconDeclaration)base.Parent;

		protected override void CheckCRD_CustomsOffice()
		{
			base.CheckCRD_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CRD_CustomsOfficeInfo);
		}

		protected override void CheckCRD_DeclarationType()
		{
			base.CheckCRD_DeclarationType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_DeclarationTypeInfo);
		}
		protected override void CheckCRD_RefundCauseCode()
		{
			base.CheckCRD_RefundCauseCode();
			if (Parent.CRD_DeclarationType == RefundTypeList.Codes.A)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_RefundCauseCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CRD_RefundCauseCodeInfo);
		}
		protected override void CheckCRD_RefundReasonCode()
		{
			base.CheckCRD_RefundReasonCode();
			if (Parent.IsRefundReasonCodeMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_RefundReasonCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CRD_RefundReasonCodeInfo);
		}
		protected override void CheckCRD_CustomsDivision()
		{
			base.CheckCRD_CustomsDivision();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CRD_CustomsDivisionInfo);
		}
		protected override void CheckCRD_TaxOffice()
		{
			base.CheckCRD_TaxOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CRD_TaxOfficeInfo);
		}
	}
}
