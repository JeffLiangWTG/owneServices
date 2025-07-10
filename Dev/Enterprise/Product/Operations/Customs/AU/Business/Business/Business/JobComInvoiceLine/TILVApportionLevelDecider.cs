namespace Enterprise.Customs.AU.Declaration.Business
{
	internal static class TILVApportionLevelDecider
	{
		public static bool IsManuallyAdjustedInvoice(JobDeclaration declaration, JobComInvoiceHeader invoice)
		{
			return
				!invoice.AddInfo.ZA_TILV.IsEmpty ||
				invoice.Charges.HasNonDutiableGSTApplicableCharges() ||
				DoParentSubGroupHeadersHaveTransportAndInsurance(invoice, (JobComInvoiceGroupHeader)declaration.TopGroupInvoice);
		}

		static bool DoParentSubGroupHeadersHaveTransportAndInsurance(JobComInvoiceHeader invoice, JobComInvoiceGroupHeader topGroupInvoice)
		{
			JobComInvoiceGroupHeader groupHeader = invoice.GroupHeader;

			while (groupHeader != null && groupHeader != topGroupInvoice)
			{
				if (groupHeader.Charges.HasNonDutiableGSTApplicableCharges())
				{
					return true;
				}

				groupHeader = (JobComInvoiceGroupHeader)groupHeader.GroupHeader;
			}

			return false;
		}
	}
}
