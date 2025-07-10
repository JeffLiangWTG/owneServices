using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class TaxRegimeValidation : CusSupportingInfoValidation
	{
		public TaxRegimeValidation(TaxRegime parent) : base(parent)
		{
		}

		public new TaxRegime Parent => (TaxRegime)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (Parent.CSI_SubType == TaxRegimeTypeList.Codes.FMM)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
			else if (Parent.Parent is JobComInvoiceLine invoiceLine
				&& (invoiceLine.IsImportSiscomex || (invoiceLine.IsImportLicense && Parent.CSI_SubType == TaxRegimeTypeList.Codes.Duty) || (invoiceLine.IsImportOnly && Parent.CSI_SubType == TaxRegimeTypeList.Codes.ICMS)))
			{
				if (invoiceLine.IsAttachedToPersistentDeclaration)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ProcedureInfo);
			if (!Parent.CSI_Code.IsEmpty
				&& Parent.CSI_Code != TaxRegimeList.Codes.FullCollection
				&& Parent.CSI_Code != TaxRegimeList.Codes.PaymentMade
				&& (Parent.CSI_SubType == TaxRegimeTypeList.Codes.Duty || Parent.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ProcedureInfo);
			}
		}
	}
}
