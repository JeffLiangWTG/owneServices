using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ExchangeHedgeValidation : CusSupportingInfoValidation
	{
		public ExchangeHedgeValidation(ExchangeHedge parent) : base(parent)
		{
		}

		public new ExchangeHedge Parent => (ExchangeHedge)base.Parent;

		JobComInvoiceHeader InvoiceHeader => Parent.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (InvoiceHeader.IsExchangeHedgeAppliable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			base.CheckCSI_AdditionalDescription();
			if (InvoiceHeader.IsExchangeHedgeAppliable && !Parent.CSI_AdditionalDescription_ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_AdditionalDescriptionInfo);
			}
		}

		protected override void CheckCSI_IssuerType()
		{
			base.CheckCSI_IssuerType();
			if (InvoiceHeader.IsExchangeHedgeAppliable && !Parent.CSI_IssuerType_ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_IssuerTypeInfo);
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			if (InvoiceHeader.IsExchangeHedgeAppliable && !Parent.CSI_SubType_ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
			}
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();
			TypeValidation.CheckValidDecimal(Parent.CSI_ValueInfo, 15, 2);
			if (InvoiceHeader.IsImportExcludingLicense && !Parent.CSI_Value_ReadOnly)
			{
				MandatoryValidation.CheckNotNegative(Parent.CSI_ValueInfo);
				MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_ValueInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (InvoiceHeader.IsImportExcludingLicense && !Parent.CSI_ReferenceNumber_ReadOnly)
			{
				if (InvoiceHeader.IsImportSiscomex)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.CSI_ReferenceNumberInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
				}
			}
		}
	}
}
