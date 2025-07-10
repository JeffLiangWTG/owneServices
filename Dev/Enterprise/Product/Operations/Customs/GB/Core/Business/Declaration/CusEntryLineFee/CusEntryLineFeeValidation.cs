using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.GB.Business.GBCommonConstants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryLineFeeValidation : EU.Business.Declaration.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
		{
		}

		protected override void CheckCF_RateOverrideReasonCode()
		{
			base.CheckCF_RateOverrideReasonCode();

			if (Parent.CF_RateOverrideReasonCode == TaxOverrideReasonCodes.Override)
			{
				if (!Parent.EntryLine.Fees.Cast<CusEntryLineFee>().All(x => x.CF_RateOverrideReasonCode == TaxOverrideReasonCodes.Override))
				{
					Parent.CF_RateOverrideReasonCodeInfo.AddMessageError("If one fee has Action of 'OVR' then all other fees must also have Action of 'OVR'");
				}

				if (!Parent.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Any(x => x.CSI_Code == AdditonalInfoCodes.Override))
				{
					Parent.CF_RateOverrideReasonCodeInfo.AddMessageError("If the fee has Action of 'OVR' then the Entry Line must have Additional Info of type 'OVR01'");
				}
			}
		}
	}
}
