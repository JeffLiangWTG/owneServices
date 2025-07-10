using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public static class ValidationHelper
	{
		public static void CheckValidPercentage(ZPropertyInfo propertyInfo)
		{
			var percentage = (ZDecimal)propertyInfo.Value;
			if (percentage > 100 || percentage < 0)
			{
				propertyInfo.AddError(Res.GetString("BE948BE9-643D-429B-A5E1-8D5359335F81", "Percentage should be a value between 0 and 100."));
			}
		}

		public static void CheckPaymentMethod(JobDeclaration declaration)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(declaration.JE_PaymentMethodInfo);

			if (declaration.JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Importer)
			{
				var orgImpAddInfo = declaration.ImporterAddInfo;
				if (orgImpAddInfo != null && orgImpAddInfo.ZO_AccountNumber.IsEmpty)
				{
					declaration.JE_PaymentMethodInfo.AddMessageError(Res.GetString("A5C32938-993C-4DDD-B070-C40CE7C5B0CA", "No Bank Account has been configured against the Importer Bank Details."));
				}
			}
		}

		public static void CheckRateIsOverridenForSpecialCases(ZPropertyInfo propertyInfo, JobComInvoiceLine invoiceLine)
		{
			var taxGroup = (ZString)propertyInfo.Value;
			if ((taxGroup == Constants.RateCodes.IPI && invoiceLine.IPIRateIsOverridden) || (taxGroup == Constants.RateCodes.PIS && invoiceLine.PisRateIsOverridden)
				|| (taxGroup == Constants.RateCodes.Cofins && invoiceLine.CofinsRateIsOverridden))
			{
				propertyInfo.AddError(Res.GetString("CD942589-B555-4968-BF6B-79A254469EB3", "Ad Valorem rate has been overridden. You cannot add Special Rates."));
			}
		}
	}
}
