using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Contants = Enterprise.Customs.IE.Business.Constants.SupportingDocumentCodes;

namespace Enterprise.Customs.IE.H7.Business
{
	public static class SupportingDocumentValidationHelper
	{
		public static void ValidateRequiredCodesForProcedureNonC08(AsycudaBill bill, Action<string> addMessageError)
		{
			Argument.NotNull(addMessageError, "The addMessageError function cannot be null.");
			if (bill.ABL_Procedure != EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C08 && !bill.SupportingDocuments.Any(d => AcceptedTypesForProcedureNotC08.Contains(d.CSI_Code)))
			{
				addMessageError(bill.ValidationConfiguration.ValidationMessage.GetBR2037RuleMessage());
			}
		}

		public static void Validate1D24CodeRequired(AsycudaBill bill, Action<string> addMessageError)
		{
			Argument.NotNull(addMessageError, "The addMessageError function cannot be null.");
			if (!bill.SupportingDocuments.Any(b => b.CSI_Code == Contants._1D24))
			{
				addMessageError(bill.ValidationConfiguration.ValidationMessage.GetBR20319Rule1D24SUPRequiredMessage());
			}
		}

		static readonly List<string> AcceptedTypesForProcedureNotC08 = new List<string> { Contants._D005, Contants._D008, Contants._N325, Contants._N380, Contants._N864, Contants._N935 };
	}
}
