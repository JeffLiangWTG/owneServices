using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			CheckMaxPreviousDocument(Parent);
			CheckChildrenPreviousDocument(Parent);
		}

		void CheckMaxPreviousDocument(JobComInvoiceHeader parent)
		{
			if (parent.PreviousDocuments.Count > 1)
			{
				parent.AddRowMessageError(Res.GetString("2A11E688-7851-4B8B-B2A4-BA2F95C58E0A", "Customs will not accept a declaration with more than 1 previous document for Box 40 per line"));
			}
		}

		void CheckChildrenPreviousDocument(JobComInvoiceHeader parent)
		{
			var allInvoiceLinesHavePreviousDocuments = true;

			foreach (JobComInvoiceLine invLine in parent.InvoiceLines)
			{
				if (!invLine.PreviousDocuments.Any())
				{
					allInvoiceLinesHavePreviousDocuments = false;
				}
			}

			if (allInvoiceLinesHavePreviousDocuments && (Parent.PreviousDocuments.Any() || Parent.JobDeclaration.PreviousDocuments.Any()))
			{
				parent.AddRowMessageError(Res.GetString("455805BD-4143-4D54-948C-8E2A9FE4C1AB", "Header's previous document will not be declared as all invoice lines have its own previous document for Box 40"));
			}
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning; }
		}

		protected override ZBool RequireJZ_IncoTermPlaceMandatory => ZBool.False;

		protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine => true;
	}
}
