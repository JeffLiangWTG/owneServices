
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business
{
	public class AccEPaymentQuoteValidation : AutoAccEPaymentQuoteValidation
	{
		public AccEPaymentQuoteValidation(AutoAccEPaymentQuote parent) : base(parent)
		{
			Parent = parent as AccEPaymentQuote;
		}

		protected new AccEPaymentQuote Parent;

		protected override void CheckQU_AV()
		{
			base.CheckQU_AV();
			if (!Parent.QU_AVInfo.HasErrors() && Parent.PaymentApproval == null)
			{
				Parent.QU_AVInfo.AddError(ResString.GetMultilingualString("6E585C6D-E2F9-4E36-9CB1-5350C169B2A8", "Quote must be attached to a valid Payment Approval."));
			}
		}

		protected override void CheckQU_GC()
		{
			base.CheckQU_GC();
			if (!Parent.QU_GCInfo.HasErrors() && Parent.Company == null)
			{
				Parent.QU_GCInfo.AddError(ResString.GetMultilingualString("6AE49EF1-952F-47FB-A059-21BFD6FB6556", "Quote must specify a valid Company."));
			}
		}

		protected override void CheckQU_InternalReference()
		{
			base.CheckQU_InternalReference();
			MandatoryValidation.CheckEntered(Parent.QU_InternalReferenceInfo);
		}

		protected override void CheckQU_ProviderReference()
		{
			base.CheckQU_ProviderReference();
			if (!Parent.QU_ProviderReferenceInfo.HasErrors())
			{
				if (Parent.HasReceivedValidResponseFromProvider)
				{
					if (Parent.QU_ProviderReference.IsEmpty)
					{
						Parent.QU_ProviderReferenceInfo.AddError(ResString.GetMultilingualString("D8755849-5E35-42BE-924B-98BEEFE118E0", "A Response has been received from the provider but no Reference has been entered."));
					}
				}
				else if (!Parent.QU_ProviderReference.IsEmpty)
				{
					Parent.QU_ProviderReferenceInfo.AddError(ResString.GetMultilingualString("3024124B-5BD4-4068-BF8B-FDE33D74F3F5", "A reference cannot be entered before a response is received from the provider."));
				}
			}
		}

		protected override void CheckQU_ProviderCode()
		{
			base.CheckQU_ProviderCode();
			MandatoryValidation.CheckEntered(Parent.QU_ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.QU_ProviderCodeInfo);
		}

		protected override void CheckQU_Status()
		{
			base.CheckQU_Status();
			MandatoryValidation.CheckEntered(Parent.QU_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.QU_StatusInfo);
		}

		protected override void CheckQU_ToAmount()
		{
			base.CheckQU_ToAmount();
			if (!Parent.QU_ToAmountInfo.HasErrors())
			{
				CheckAmountPropertyIsValid(Parent.QU_ToAmountInfo as ZPropertyInfo<ZDecimal>);
			}
		}

		protected override void CheckQU_RX_NKToCurrency()
		{
			base.CheckQU_RX_NKFeeCurrency();
			MandatoryValidation.CheckEntered(Parent.QU_RX_NKToCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.QU_RX_NKToCurrencyInfo);
		}

		protected override void CheckQU_FromAmount()
		{
			base.CheckQU_FromAmount();
			if (!Parent.QU_FromAmountInfo.HasErrors())
			{
				CheckAmountPropertyIsValid(Parent.QU_FromAmountInfo as ZPropertyInfo<ZDecimal>);
			}
		}

		protected override void CheckQU_RX_NKFromCurrency()
		{
			base.CheckQU_RX_NKFeeCurrency();
			MandatoryValidation.CheckEntered(Parent.QU_RX_NKFromCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.QU_RX_NKFromCurrencyInfo);
		}

		protected virtual void CheckAmountPropertyIsValid(ZPropertyInfo<ZDecimal> amountInfo)
		{
			if (!Parent.HasReceivedValidResponseFromProvider)
			{
				if (amountInfo.Value < 0)
				{
					amountInfo.AddError(ResString.GetMultilingualString("742DF4FA-55A9-4A18-82D0-21F7B10511D3", "Quote amounts cannot be negative."));
				}
				else if ((Parent.QU_ToAmount == 0 && Parent.QU_FromAmount == 0) || (Parent.QU_ToAmount > 0 && Parent.QU_FromAmount > 0))
				{
					amountInfo.AddError(ResString.GetMultilingualString("08A38EC9-2F33-4BFC-AD1C-9171A9FE33103", "Exactly one amount must be specified when requesting a Quote."));
				}
			}
			else if (amountInfo.Value <= 0)
			{
				amountInfo.AddError(ResString.GetMultilingualString("D9136457-660E-4641-AFB9-C9FD4C5296C0", "Quote amounts must be greater than 0 when recording the details of a Quote Response from a provider."));
			}
		}

		protected override void CheckQU_ExchangeRate()
		{
			base.CheckQU_ExchangeRate();
			if (!Parent.QU_ExchangeRateInfo.HasErrors())
			{
				CheckExRatePropertyIsValid(Parent.QU_ExchangeRateInfo as ZPropertyInfo<ZDecimal>);
			}
		}

		protected override void CheckQU_ExchangeRateInverted()
		{
			base.CheckQU_ExchangeRateInverted();
			if (!Parent.QU_ExchangeRateInvertedInfo.HasErrors())
			{
				CheckExRatePropertyIsValid(Parent.QU_ExchangeRateInvertedInfo as ZPropertyInfo<ZDecimal>);
			}
		}

		protected virtual void CheckExRatePropertyIsValid(ZPropertyInfo<ZDecimal> exRateInfo)
		{
			if (Parent.HasReceivedValidResponseFromProvider)
			{
				if (exRateInfo.Value <= 0)
				{
					exRateInfo.AddError(ResString.GetMultilingualString("BBE56C68-A5C7-4AD7-A4FF-453DD2E1BAB7", "Exchange rates cannot be 0 or negative."));
				}
				else if (Parent.QU_RX_NKFromCurrency == Parent.QU_RX_NKToCurrency && exRateInfo.Value != 1)
				{
					exRateInfo.AddError(ResString.GetMultilingualString("DD0437E7-56A0-4B5E-A27C-15C552E09943", "If the currencies are equal, then exchange rate must be 1."));
				}
				else if (Parent.QU_ExchangeRate < 1 && Parent.QU_ExchangeRate > 0 && Parent.QU_ExchangeRateInverted < 1 && Parent.QU_ExchangeRateInverted > 0
					|| Parent.QU_ExchangeRate > 1 && Parent.QU_ExchangeRateInverted > 1)
				{
					exRateInfo.AddError(ResString.GetMultilingualString("38E7072F-8B28-438E-B62D-AAD5AA92E82C", "The Exchange Rate must be the reciprocal of the Inverted Exchange Rate."));
				}
			}
			else if (exRateInfo.Value != 0)
			{
				exRateInfo.AddError(ResString.GetMultilingualString("0B90CEE4-3C8E-4073-92AA-8A1BA3D763C5", "Exchange rates cannot be entered before a response is received from the provider."));
			}
		}

		protected override void CheckQU_FeeAmount()
		{
			base.CheckQU_FeeAmount();
			if (!Parent.QU_FeeAmountInfo.HasErrors())
			{
				if (Parent.HasReceivedValidResponseFromProvider)
				{
					if (Parent.QU_FeeAmount < 0)
					{
						Parent.QU_FeeAmountInfo.AddError(ResString.GetMultilingualString("B0F3B903-8E8A-4B59-BB2A-38F0FD239546", "Fee amount cannot be negative."));
					}
				}
				else if (Parent.QU_FeeAmount != 0)
				{
					Parent.QU_FeeAmountInfo.AddError(ResString.GetMultilingualString("7FC8A95F-CC1D-48F7-AA4F-49DBA4091B47", "Fee amount cannot be entered before a response is received from the provider."));
				}
				if (Parent.QU_ProviderCode == EPaymentProviderCodes.Codes.OFX
					&& Parent.QU_Status == StatusCodes.Received && Parent.QU_ProviderReference.IsEmpty)
				{
					Parent.QU_FeeAmountInfo.AddWarning(ResString.GetMultilingualString("D474D219-901A-4B9E-A41E-B36AEB826BA2", "This is indicative rate only. Processing Fee may be applied on the formal quote. To request a formal quote, please select an OFX E-Payment Account as the Bank Account for this payment, and ensure you have authorized your OFX User Account."));
				}
			}
		}

		protected override void CheckQU_RX_NKFeeCurrency()
		{
			base.CheckQU_RX_NKFeeCurrency();
			if (!Parent.QU_RX_NKFeeCurrencyInfo.HasErrors())
			{
				if (Parent.HasReceivedValidResponseFromProvider)
				{
					if (Parent.QU_FeeAmount > 0 && Parent.QU_RX_NKFeeCurrency.IsEmpty)
					{
						Parent.QU_RX_NKFeeCurrencyInfo.AddError(ResString.GetMultilingualString("503AD58C-71C8-4C28-BA62-48D0C73B48C2", "If a Fee is specified then a Fee Currency must be specified."));
					}
					// The below check is needed to match database constraint, may be removed if this constraint is changed
					else if (Parent.QU_FeeAmount == 0 && !Parent.QU_RX_NKFeeCurrency.IsEmpty)
					{
						Parent.QU_RX_NKFeeCurrencyInfo.AddError(ResString.GetMultilingualString("0E0E57BD-CFA8-4C7D-9149-1A074B8BD965", "There should be no Fee Currency if there is no Fee."));
					}
				}
				else if (!Parent.QU_RX_NKFeeCurrency.IsEmpty)
				{
					Parent.QU_RX_NKFeeCurrencyInfo.AddError(ResString.GetMultilingualString("95B078EB-2632-4565-8636-EB3AD09A403B", "Fee currency cannot be entered before a response is received from the provider."));
				}
			}
		}

		protected override void CheckQU_LastResponseReceivedUtc()
		{
			base.CheckQU_LastResponseReceivedUtc();
			if (!Parent.QU_LastResponseReceivedUtcInfo.HasErrors())
			{
				if (Parent.HasReceivedValidResponseFromProvider)
				{
					if (Parent.QU_LastResponseReceivedUtc.IsEmpty)
					{
						Parent.QU_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("FD00C61D-C9CB-4394-8037-658741B11E95", "Response time must be recorded if a response from the provider has been received."));
					}
					else if (Parent.QU_LastResponseReceivedUtc < Parent.QU_SystemCreateTimeUtc)
					{
						Parent.QU_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("7988283F-3765-410C-9C96-0CC5A3390C1C", "Response time cannot be earlier than the creation time."));
					}
				}
				else if (!Parent.QU_LastResponseReceivedUtc.IsEmpty)
				{
					Parent.QU_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("BF1CB068-5093-449A-9C82-04DE47E8FC53", "Response Time cannot be entered before a response is received from the provider."));
				}
			}
		}

		protected override void CheckQU_ErrorDescription()
		{
			base.CheckQU_ErrorDescription();
			if (!Parent.QU_ErrorDescriptionInfo.HasErrors() && !Parent.QU_ErrorDescription.IsEmpty && !(Parent.QU_Status == StatusCodes.Failed || Parent.QU_Status == StatusCodes.Error))
			{
				Parent.QU_ErrorDescriptionInfo.AddError(ResString.GetMultilingualString("0EF957E2-332B-44FA-922A-DB489C733238", "Error Description should only be recorded if the status is either RQF or ERR."));
			}
		}
	}
}
