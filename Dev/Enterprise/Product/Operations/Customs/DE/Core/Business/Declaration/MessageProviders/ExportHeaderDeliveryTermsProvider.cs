using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class ExportHeaderDeliveryTermsProvider : IDeliveryTerms
	{
		public ExportHeaderDeliveryTermsProvider(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}
		readonly JobComInvoiceHeader invoiceHeader;

		public string IncotermCode => invoiceHeader.JZ_IncoTerm;

		public string UNLocode => IncotermCode != Core.Constants.IncoTerms.Other && IncoTermUNLOCO != null && IncoTermUNLOCO.RL_IsSystem ? (string)IncoTermUNLOCO.RL_Code : null;

		public string Country => IncotermCode != Core.Constants.IncoTerms.Other && IncoTermUNLOCO != null && !IncoTermUNLOCO.RL_IsSystem ? (string)IncoTermUNLOCO.RL_RN_NKCountryCode : null;

		public string Location => IncotermCode != Core.Constants.IncoTerms.Other && IncoTermUNLOCO != null && !IncoTermUNLOCO.RL_IsSystem ? IncoTermUNLOCO.RL_NameWithDiacriticals : string.Empty;

		public string Text => IncotermCode == Core.Constants.IncoTerms.Other ? (NoResString)"Location" : null;// Constant string defined by Customs

		RefUNLOCO IncoTermUNLOCO => incoTermUNLOCO ?? (incoTermUNLOCO = new RefUNLOCO.Loader(invoiceHeader.Factory).Load(invoiceHeader.JZ_IncoTermPlace));
		RefUNLOCO incoTermUNLOCO;
	}
}
