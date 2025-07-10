using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineFeeValidation : EUUniversalCusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(CusEntryLineFee parent) : base(parent)
		{
		}

		public new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		const int DecimalPlacesAllowedForBaseValuePrecisionImportNoPercentage = 6;

		const int DecimalPlacesAllowedForBaseValuePrecisionNotImportOrPercentage = 2;

		protected override void CheckCF_ChargeAmount()
		{
			base.CheckCF_ChargeAmount();

			if (Parent.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional || Parent.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CF_ChargeAmountInfo);
			}
		}

		protected override ZInt DecimalPlacesAllowedForBaseValuePrecision
		{
			get
			{
				var entryHeader = (CusEntryHeader)Parent.EntryLine.Header;
				if (entryHeader.Declaration.IsUCC6)
				{
					return Parent.CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage ? DecimalPlacesAllowedForBaseValuePrecisionNotImportOrPercentage : DecimalPlacesAllowedForBaseValuePrecisionImportNoPercentage;
				}
				else
				{
					return DecimalPlacesAllowedForBaseValuePrecisionNotImportOrPercentage;
				}
			}
		}
	}
}
