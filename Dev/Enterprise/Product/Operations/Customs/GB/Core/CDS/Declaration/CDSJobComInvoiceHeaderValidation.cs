using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSJobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		readonly string[] preCIF = { Core.Constants.IncoTerms.ExWorks, Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeAlongsideShip, Core.Constants.IncoTerms.FreeOnBoard,
									 Core.Constants.IncoTerms.CarriagePaidTo, Core.Constants.IncoTerms.CostAndFreight };

		public CDSJobComInvoiceHeaderValidation(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();

			var distinctIncoTerms = (from JobComInvoiceLine ji in Parent.JobDeclaration.InvoiceLines where Parent.CusEntryInstructions.Contains(ji.EntryInstruction) select ji.InvoiceHeader.JZ_IncoTerm);
			if (distinctIncoTerms.Distinct().Take(2).Count() > 1)
			{
				Parent.JZ_IncoTermInfo.AddMessageError("Mixed Incoterms are not allowed");
			}

			ValidateIncoTermAndPlaceOverEntryInstructions();
			ValidateIncoTermPreCIFCharges();
		}

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();
			CDSIncoTermPlaceValidator.Validate(Parent.Factory, Parent.JZ_IncoTermPlace, Parent.JZ_IncoTermPlaceInfo);
			ValidateJZ_IncoTerm();
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			JobDeclaration dec = Parent.JobDeclaration;
			var jobDocAddressOnDec = dec.ImporterDocumentaryAddress;

			ZGuid invoiceParty = Parent.JZ_OH_Buyer;
			ZPropertyInfo propertyInfoForErrorMessage = Parent.JZ_OH_BuyerInfo;

			CheckPartyDefinedAtDeclarationOrInvoiceButNotBothAndNotNeitherAndChiefForEori(dec, jobDocAddressOnDec, invoiceParty, propertyInfoForErrorMessage);
			if (dec.IsImport)
			{
				CheckForEori(Parent.Buyer, Parent.JZ_OH_BuyerInfo);
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			JobDeclaration dec = Parent.JobDeclaration;
			var jobDocAddressOnDec = dec.SupplierDocumentaryAddress;
			ZGuid invoiceParty = Parent.JZ_OH_Supplier;
			ZPropertyInfo propertyInfoForErrorMessage = Parent.JZ_OH_SupplierInfo;

			CheckPartyDefinedAtDeclarationOrInvoiceButNotBothAndNotNeitherAndChiefForEori(dec, jobDocAddressOnDec, invoiceParty, propertyInfoForErrorMessage);
			if (dec.IsExport)
			{
				CheckForEori(Parent.Supplier, Parent.JZ_OH_SupplierInfo);
			}
		}

		void CheckForEori(OrgHeader org, ZPropertyInfo propertyInfoForErrorMessage)
		{
			if (org != null)
			{
				string eoriCode = org.GetEuIdentificationNumber();
				CDS.MessagingRules.EoriCodeValidationHelper.CheckNoEoriCodeInPropertyInfo(eoriCode, propertyInfoForErrorMessage);
			}
		}

		void CheckPartyDefinedAtDeclarationOrInvoiceButNotBothAndNotNeitherAndChiefForEori(JobDeclaration dec, JobDocAddress jobDocAddressOnDec, ZGuid invoiceParty, ZPropertyInfo propertyInfoForErrorMessage)
		{
			if (dec != null)
			{
				var decOrgPk = jobDocAddressOnDec.OrganisationPK;
				if (jobDocAddressOnDec.E2_CompanyName.IsEmpty && decOrgPk.IsEmpty && invoiceParty.IsEmpty)
				{
					propertyInfoForErrorMessage.AddMessageError("You need to select a party since you have not selected one at declaration header level");
				}
				else if (!jobDocAddressOnDec.IsEmpty && !invoiceParty.IsEmpty && decOrgPk != invoiceParty)
				{
					propertyInfoForErrorMessage.AddMessageError("The header-level and invoice-level parties differ. For a bulk entry, remove the declaration party.");
				}
			}
		}

		void ValidateIncoTermAndPlaceOverEntryInstructions()
		{
			var distinctIncoTerms = (from JobComInvoiceLine ji in Parent.JobDeclaration.InvoiceLines
									 where Parent.CusEntryInstructions.Contains(ji.EntryInstruction)
									 select string.Format(CultureInfo.InvariantCulture, "{0}-{1}", ji.InvoiceHeader.JZ_IncoTerm, ji.InvoiceHeader.JZ_IncoTermPlace));
			if (distinctIncoTerms.Distinct().Count() > 1)
			{
				Parent.JZ_IncoTermInfo.AddMessageError("All invoice lines on this entry instruction must be on invoices with the same Incoterm and place");
			}
		}

		void ValidateIncoTermPreCIFCharges()
		{
			if (Parent.IsImport && preCIF.Contains<string>(Parent.JZ_IncoTerm) &&
				!Parent.Charges.Any() && !Parent.GroupCharges.Any())
			{
				Parent.JZ_IncoTermInfo.AddMessageError("Pre-CIF requires charges (make sure that the invoice, its invoice lines and all group charges have sufficient data, such as values and currency, for apportionment to execute)");
			}
		}

		protected override IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes
		{
			get
			{
				return new ZString[]
				{
					ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration,
					ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration
				};
			}
		}

		protected override CargoWise.ComponentModel.INotificationType AtLeastOneSupportingDocumentMessageNotificationType => NotificationType.Warning;
	}
}
