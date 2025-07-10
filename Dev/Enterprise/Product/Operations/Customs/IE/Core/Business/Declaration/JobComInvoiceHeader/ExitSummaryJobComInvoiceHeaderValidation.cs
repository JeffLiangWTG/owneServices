using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryJobComInvoiceHeaderValidation : CommonJobComInvoiceHeaderValidation
	{
		public ExitSummaryJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
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
			var hasInvoiceHeaderTransportDocuments = parent.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsATransportDocument);
			if (!hasInvoiceHeaderTransportDocuments)
			{
				var instructions = parent.CusEntryInstructions.Cast<CusEntryInstruction>();
				var hasInstructionTransportDocuments = instructions.Any(instruction => instruction.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsATransportDocument));
				if (!hasInstructionTransportDocuments)
				{
					var doesSiblingInvoiceHasTransportDocuments = instructions.Any(instruction => instruction.Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsATransportDocument)));
					if (doesSiblingInvoiceHasTransportDocuments)
					{
						parent.AddRowMessageError(CommonResStrings.ProvideAtLeastOneTransportDocument);
					}
				}
			}
		}

		protected override void CheckJZ_InvoiceDate()
		{
		}

		protected override void CheckJZ_IncoTerm()
		{
			ListValidation.WarnIfInvalidCode(Parent.JZ_IncoTermInfo);
		}

		protected override void CheckJZ_ValuationCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.JZ_ValuationCodeInfo);
		}
	}
}
