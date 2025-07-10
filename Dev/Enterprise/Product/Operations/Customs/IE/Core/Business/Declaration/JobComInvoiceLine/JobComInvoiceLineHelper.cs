using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	static class JobComInvoiceLineHelper
	{
		public static bool HasProcedureCodeConcessionF15(IEnumerable<JobComInvoiceLine> invoiceLines) => HasMainProcedureCodeConcessionF15(invoiceLines) || HasAdditionalProcedureCodeConcessionF15(invoiceLines);

		public static bool HasAddtionalInfoINF00200(this JobComInvoiceLine line) => line.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.IsAnAdditionalInformation && x.CSI_Code == Constants.AdditionalInformationCodes._00200);

		static bool HasMainProcedureCodeConcessionF15(IEnumerable<JobComInvoiceLine> invoiceLines) => invoiceLines.Any(i => i.JI_Procedure.EndsWith(UniversalReferenceConstants.ProcedureCodes.Concession.F15));

		static bool HasAdditionalProcedureCodeConcessionF15(IEnumerable<JobComInvoiceLine> invoiceLines) => invoiceLines.Any(x => x.HasAdditionalProcedureCodeConcessionF15);
	}
}
