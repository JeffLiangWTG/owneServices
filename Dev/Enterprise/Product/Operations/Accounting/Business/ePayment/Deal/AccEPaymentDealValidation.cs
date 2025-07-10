using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business
{
	public class AccEPaymentDealValidation : AutoAccEPaymentDealValidation
	{
		public AccEPaymentDealValidation(AutoAccEPaymentDeal parent) : base(parent)
		{
			Parent = parent as AccEPaymentDeal;
		}

		protected new AccEPaymentDeal Parent;

		protected override void CheckAED_GC_Company()
		{
			base.CheckAED_GC_Company();
			if (!Parent.AED_GC_CompanyInfo.HasErrors() && Parent.Company == null)
			{
				Parent.AED_GC_CompanyInfo.AddError(ResString.GetMultilingualString("07db23ee-0e97-4e05-9104-b9f420efa9ab", "Deal must specify a valid Company."));
			}
		}

		protected override void CheckAED_QU_Quote()
		{
			base.CheckAED_QU_Quote();
			if (!Parent.AED_QU_QuoteInfo.HasErrors() && Parent.Quote == null)
			{
				Parent.AED_QU_QuoteInfo.AddError(ResString.GetMultilingualString("7a025ab8-e789-43ef-8e7f-c6c5747d8925", "Deal must be attached to a valid Quote."));
			}
		}

		protected override void CheckAED_InternalReference()
		{
			base.CheckAED_InternalReference();
			MandatoryValidation.CheckEntered(Parent.AED_InternalReferenceInfo);
		}

		protected override void CheckAED_ProviderReference()
		{
			base.CheckAED_ProviderReference();
			if (!Parent.AED_ProviderReferenceInfo.HasErrors())
			{
				if (Parent.HasProviderSentAReference)
				{
					if (Parent.AED_ProviderReference.IsEmpty)
					{
						Parent.AED_ProviderReferenceInfo.AddError(ResString.GetMultilingualString("adc55de6-b38c-43d5-819c-22a0211af4ea", "A Response has been received from the provider but no Reference has been entered."));
					}
				}
				else if (!Parent.AED_ProviderReference.IsEmpty)
				{
					Parent.AED_ProviderReferenceInfo.AddError(ResString.GetMultilingualString("2512d7ab-276b-4b88-b3f8-15918622b78d", "A reference cannot be entered before a response is received from the provider."));
				}
			}
		}

		protected override void CheckAED_ProviderCode()
		{
			base.CheckAED_ProviderCode();
			MandatoryValidation.CheckEntered(Parent.AED_ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AED_ProviderCodeInfo);
		}

		protected override void CheckAED_Status()
		{
			base.CheckAED_Status();
			MandatoryValidation.CheckEntered(Parent.AED_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AED_StatusInfo);
			if (!Parent.AED_StatusInfo.HasErrors())
			{
				var activeDealStatus = Parent.FindActiveDealAleadyInDatabase();
				if (!activeDealStatus.IsEmpty)
				{
					Parent.AED_StatusInfo.AddError(ResString.GetMultilingualString("9844cea6-8fa4-4f70-8ef6-0eb03323f073", "Cannot create deal with {0} status when a deal with {1} status already exists for Quote {2}.", Parent.AED_Status, activeDealStatus, Parent.Quote.QU_InternalReference));
				}
			}
		}

		protected override void CheckAED_LastResponseReceivedUtc()
		{
			base.CheckAED_LastResponseReceivedUtc();
			if (!Parent.AED_LastResponseReceivedUtcInfo.HasErrors())
			{
				if (Parent.HasProviderSentAResponse)
				{
					if (Parent.AED_LastResponseReceivedUtc.IsEmpty)
					{
						Parent.AED_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("6d1dfce3-0c86-4373-b69c-d7a150b944b0", "Response time must be recorded if a response from the provider has been received."));
					}
					else if (Parent.AED_LastResponseReceivedUtc < Parent.AED_SystemCreateTimeUtc)
					{
						Parent.AED_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("8e2c2d75-612e-4210-93d2-09227da066bb", "Response time cannot be earlier than the creation time."));
					}
				}
				else if (!Parent.AED_LastResponseReceivedUtc.IsEmpty)
				{
					Parent.AED_LastResponseReceivedUtcInfo.AddError(ResString.GetMultilingualString("d78a10a8-142a-482b-8024-ca5cfc0c750e", "Response Time cannot be entered before a response is received from the provider."));
				}
			}
		}

		protected override void CheckAED_ErrorDescription()
		{
			base.CheckAED_ErrorDescription();
			if (!Parent.AED_ErrorDescriptionInfo.HasErrors() && !Parent.AED_ErrorDescription.IsEmpty && !(Parent.AED_Status == StatusCodes.SubmissionFailed || Parent.AED_Status == StatusCodes.Failed || Parent.AED_Status == StatusCodes.Declined))
			{
				Parent.AED_ErrorDescriptionInfo.AddError(ResString.GetMultilingualString("615265ca-b631-4cb4-9931-3b27e9f134ab", "Error Description should only be recorded if the status is SMF, FAL or DEC."));
			}
		}
	}
}
