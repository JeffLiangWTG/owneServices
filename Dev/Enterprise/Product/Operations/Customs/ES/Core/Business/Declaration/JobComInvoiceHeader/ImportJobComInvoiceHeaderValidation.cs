using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateNeedAtLeastOneTransportSupportingDocument();
		}

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();
			if (HasImportEntry && HasEntryHeaderValidationModeImportNone && HasAnyDiffH2Entry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_IncoTermPlaceInfo);
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			if (HasImportEntry && HasEntryHeaderValidationModeImportNone)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceAmountInfo);
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			if (HasImportEntry && HasEntryHeaderValidationModeImportNone)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_SupplierInfo);
			}
		}

		public void ValidateNeedAtLeastOneTransportSupportingDocument()
		{
			var message = AtLeastOneTransportSupportingDocumentMessage;
			Parent.ClearRowNotificationsContaining(message);
			if (HasImportEntry && NeedsTransportSupportingDocument && HasEntryHeaderValidationModeImportNoneOrPDS)
			{
				Parent.AddRowNotification(new Notification(CargoWise.EntityFramework.NotificationType.Warning, message + ZString.Join(", ", Parent.SupportingDocuments.Helper.GetInvoiceTransportSupportingDocumentTypes().ToArray())));
			}
		}

		protected override bool IsJZ_ValuationCodeMandatory => HasAnyDiffH2AndT2CEntry;

		protected override bool IncoTermRequired => HasImportEntry && HasEntryHeaderValidationModeImportNone && HasAnyDiffH2Entry;

		public ZString AtLeastOneTransportSupportingDocumentMessage => Res.GetString("2DBBC37D-A6A4-4312-AAE5-45A00A5B953A", "At least one Transport Supporting Document of the following types is needed for this Invoice: ");

		bool HasImportEntry => Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsT2C && !x.IsT2L);

		bool HasAnyDiffH2Entry => !Parent.CusEntryInstructions.Any() || Parent.CusEntryInstructions.Any(x => !((CusEntryInstruction)x).IsH2);

		bool HasAnyDiffH2AndT2CEntry => !Parent.CusEntryInstructions.Any() || Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsH2 && !x.IsT2C);

		bool NeedsTransportSupportingDocument => !CheckForTransportSupportingDocument(Parent.SupportingDocuments)
			&& !CheckForTransportSupportingDocument(Parent.JobDeclaration.SupportingDocuments)
			&& !Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => CheckForTransportSupportingDocument(invoiceLine.SupportingDocuments))
			&& !Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(entryInstruction => CheckForTransportSupportingDocument(entryInstruction.SupportingDocuments));

		bool CheckForTransportSupportingDocument(SupportingDocumentCollection documents) =>
			documents.Cast<SupportingDocument>().Any(x => Parent.SupportingDocuments.Helper.IsTransportType(x.CSI_Code));

		bool HasEntryHeaderValidationModeImportNoneOrPDS => Parent.InvoiceLines.Count == 0 || Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryHeaderValidationModeIsImportNoneOrPDS);

		bool HasEntryHeaderValidationModeImportNone => Parent.InvoiceLines.Count == 0 || Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryHeaderValidationModeIsImportNone);
	}
}
