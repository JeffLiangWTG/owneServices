using System.Linq;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryCusEntryInstructionValidation : CommonExportCusEntryInstructionValidation
	{
		public ExitSummaryCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckTransportDocuments();
		}

		void CheckTransportDocuments()
		{
			var parent = Parent;
			var hasInstructionTransportDocuments = parent.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsATransportDocument);
			if (!hasInstructionTransportDocuments)
			{
				var hasInvoiceHeaderTransportDocuments = parent.Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsATransportDocument));
				if (!hasInvoiceHeaderTransportDocuments)
				{
					parent.AddRowMessageError(CommonResStrings.ProvideAtLeastOneTransportDocument);
				}
			}
		}
	}
}
