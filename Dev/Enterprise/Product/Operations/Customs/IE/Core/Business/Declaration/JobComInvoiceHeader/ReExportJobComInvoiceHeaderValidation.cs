using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ReExportJobComInvoiceHeaderValidation : CommonJobComInvoiceHeaderValidation
	{
		public ReExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckAdditionalInfoTransportDocumentsRequired();
		}

		protected override void CheckJZ_IncoTerm()
		{
			ListValidation.WarnIfInvalidCode(Parent.JZ_IncoTermInfo);
		}

		protected override void CheckJZ_ValuationCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.JZ_ValuationCodeInfo);
		}

		void CheckAdditionalInfoTransportDocumentsRequired()
		{
			if (!Parent.HasEntryInstructionWithTransportDocument && !Parent.HasTransportDocument && Parent.HasEntryInstructionWithInvoiceHeaderHavingTransportDocument)
			{
				Parent.AddRowMessageError(Res.GetString("581609A5-1154-4836-B350-FA4E3A7B5641", "Please provide at least one Transport Document."));
			}
		}

		protected override void CheckJZ_InvoiceDate()
		{
		}
	}
}
