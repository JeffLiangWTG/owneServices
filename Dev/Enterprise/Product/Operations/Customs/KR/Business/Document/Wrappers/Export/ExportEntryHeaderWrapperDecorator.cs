using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public static class ExportEntryHeaderWrapperDecorator
	{
		public static void Decorate(this ExportEntryHeaderWrapper wrapper, CusEntryHeader entry)
		{
			wrapper.DeclarationDate = entry.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			wrapper.ExpectedLoadingDate = entry.CusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;
			wrapper.EntryReleaseDateTime = entry.CH_EntryReleaseDate;
			wrapper.CustomsMessageRemarks = entry.CH_CustomsMessageRemarks;
			wrapper.ResponsibleCustomsOfficer = entry.CustomsOfficers.GetCustomsOfficer(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer);
			wrapper.ActualLoadingDate = entry.Declaration.JE_EntryDate;
			wrapper.CurrencyConverter = entry.CurrencyConverter;
			wrapper.RegistryCompanyPK = entry.RegistryCompanyPK;
			wrapper.InvoiceCurrencyExchangeRate = entry.RandomHeader.JZ_InvoiceCurrExRate;
			wrapper.MessageStatus = entry.CH_Status;
			wrapper.CustomsOfficeName = MessageFunctions.GetCustomsOffice(entry.Factory, entry.Declaration.JE_CustomsOffice);
		}
	}
}
