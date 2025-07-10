#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingLineBase
	{
		public bool AL_JH_ReadOnly_ForTestOnly => AL_JH_ReadOnly;

		public bool AL_OSExTaxAmount_ReadOnly_ForTestOnly => AL_OSExTaxAmount_ReadOnly;

		public InvoicingLineBaseCollection GetParentLinesCollection_ForTestOnly()
		{
			return ParentLinesCollection;
		}

		public AccTaxRate GetFallbackTaxRate_ForTestOnly(out ZGuid overrideInvTaxMsg)
		{
			return GetFallbackTaxRate(out overrideInvTaxMsg);
		}

		public bool ShouldDefaultAL_JH_ForTestOnly => ShouldDefaultAL_JH;

		public SecurityCheckpoint ModifyDefaultChargeCodeDescription_ForTestOnly => ModifyDefaultChargeCodeDescription;

		public bool AL_Desc_ReadOnly_ForTestOnly => AL_Desc_ReadOnly;

		public bool CanChangeLineValues_ForTestOnly => CanChangeLineValues;

		public bool AL_GovtChargeCode_ReadOnly_ForTestOnly => AL_GovtChargeCode_ReadOnly;

		public bool AL_AT_ReadOnly_ForTestOnly => AL_AT_ReadOnly;

		public bool IsCachedTaxAmountDirty_ForTestOnly
		{
			get { return IsCachedTaxAmountDirty; }
			set { IsCachedTaxAmountDirty = value; }
		}

		public void ResetCachedCalculatedTaxAmount_ForTestOnly()
		{
			ResetCachedCalculatedTaxAmount();
		}

		public bool AL_JHSettingDefaultsSuspended_ForTestOnly
		{
			get { return AL_JHSettingDefaultsSuspended; }
			set { AL_JHSettingDefaultsSuspended = value; }
		}

		public AccTaxRate GetChargeCodeTaxRateOverride_ForTestOnly(AccChargeCode chargeCode, out ZGuid overrideInvTaxMsg)
		{
			return GetChargeCodeTaxRateOverride(chargeCode, out overrideInvTaxMsg);
		}

		public void SetOutstandingAmount_ForTestOnly(ZDecimal outstandingAmount)
		{
			fOutstandingAmount = outstandingAmount;
		}

		public void SetLocalOutstandingAmount_ForTestOnly(ZDecimal localOutstandingAmount)
		{
			fLocalOutstandingAmount = localOutstandingAmount;
		}

		public void RaiseApportionedLineModified_ForTestOnly() 
		{
			RaiseApportionedLineModified();
		}
	}
}

#endif
