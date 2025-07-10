using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class UCCHelper
	{
		static void DoIfUCCCompliant(JobDeclaration declaration, Action<JobDeclaration> action)
		{
			if (declaration?.IsUCCCompliant ?? ZBool.False)
			{
				action?.Invoke(declaration);
			}
		}

		public void DoIfPaymentMethodAndDefermentAccountNumberBothAreSet(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			DoIfUCCCompliant(declaration, dec =>
			{
				if (ArePaymentMethodAndDefermentAccountNumberBothSet(dec, out var paymentMethod, out var defermentAccountNumber))
				{
					DoIfPaymentMethodAndDefermentAccountNumberBothAreSetCore(dec, paymentMethod, defermentAccountNumber, invoiceLine);
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected ZBool ArePaymentMethodAndDefermentAccountNumberBothSet(JobDeclaration declaration, out ZString paymentMethod, out ZString defermentAccountNumber)
		{
			paymentMethod = declaration?.JE_PaymentMethod ?? ZString.Empty;
			defermentAccountNumber = declaration?.JE_DefermentAccountNumber ?? ZString.Empty;
			return !paymentMethod.IsEmpty && !defermentAccountNumber.IsEmpty;
		}

		protected virtual void DoIfPaymentMethodAndDefermentAccountNumberBothAreSetCore(JobDeclaration declaration, ZString paymentMethod, ZString defermentAccountNumber, JobComInvoiceLine invoiceLine)
		{
		}

		public void DoIfRepresentationTypeIsSelf(JobDeclaration declaration)
		{
			DoIfUCCCompliant(declaration, dec =>
			{
				if (IsRepresentationTypeSelf(dec))
				{
					if (dec.IsImport)
					{
						AddAdditionalInformationIfNotExists(dec, Constants.AdditionalInformation.Codes.Importer, Constants.AdditionalInformation.Descriptions.Importer);
					}
					if (dec.IsExport)
					{
						AddAdditionalInformationIfNotExists(dec, Constants.AdditionalInformation.Codes.Exporter, Constants.AdditionalInformation.Descriptions.Exporter);
					}
				}
			});
		}

		protected ZBool IsRepresentationTypeSelf(JobDeclaration declaration)
		{
			if (declaration?.IsUCCCompliant ?? false)
			{
				return new ZBool(declaration.IsDeclarantSameAsLocalClientBasedOnEori());
			}
			else
			{
				var representationType = declaration?.JE_DeclarantType ?? ZString.Empty;
				return representationType == RepresentationTypeList.Codes._1Self;
			}
		}

		public void EnsureAdditionalInfoExistsForImportsIfDeclarantSameAsLocalClientBasedOnEori(JobDeclaration declaration)
		{
			if (declaration.IsDeclarantSameAsLocalClientBasedOnEori())
			{
				if (declaration.IsImport)
				{
					AddAdditionalInformationIfNotExists(declaration,
														Constants.AdditionalInformation.Codes.Importer,
														Constants.AdditionalInformation.Descriptions.Importer);
				}
				else if (declaration.IsExport)
				{
					AddAdditionalInformationIfNotExists(declaration,
														Constants.AdditionalInformation.Codes.Exporter,
														Constants.AdditionalInformation.Descriptions.Exporter);
				}
			}
		}

		protected void AddAdditionalInformationIfNotExists(JobDeclaration declaration, ZString code, ZString description)
		{
			if (declaration != null
				&& !declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
					x.CSI_Code == code
					&& x.CSI_Description == description))
			{
				var additionalInfo = declaration.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = code;
				additionalInfo.CSI_Description = description;
			}
		}

		protected void AddUpdateSupportingDocument(EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider supportingDocProvider, ZString documentCode, ZString reference, ZString status, ZString action, ZString availability)
		{
			if (supportingDocProvider != null)
			{
				var supportingDocument = supportingDocProvider.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == documentCode);

				if (supportingDocument == null)
				{
					supportingDocument = (SupportingDocument)supportingDocProvider.SupportingDocuments.AddNew();
					supportingDocument.CSI_Code = documentCode;
				}

				supportingDocument.CSI_ReferenceNumber = reference;
				supportingDocument.CSI_Status = status;
				supportingDocument.CSI_Actions = action;
				supportingDocument.CSI_Availability = availability;
			}
		}

		public void DoIfTaxLinePaymentMethodIsNOrP(JobComInvoiceLineTax lineTax)
		{
			DoIfUCCCompliant(lineTax?.Declaration, dec =>
			{
				if (IsTaxLinePaymentMethodNOrP(lineTax))
				{
					DoIfTaxLinePaymentMethodIsNOrPCore(dec);
				}
			});
		}

		protected ZBool IsTaxLinePaymentMethodNOrP(JobComInvoiceLineTax lineTax)
		{
			var mop = lineTax?.JLT_MethodOfPayment ?? ZString.Empty;
			return mop == Constants.TaxLinePaymentMethods.N || mop == Constants.TaxLinePaymentMethods.P;
		}

		protected virtual void DoIfTaxLinePaymentMethodIsNOrPCore(JobDeclaration declaration)
		{
		}

		protected void AddGuaranteeIfNotExists(JobDeclaration declaration, ZString type, ZString code, ZString holderId)
		{
			if (declaration != null
				&& !declaration.Guarantees.Cast<GBGuarantee>().Any(x =>
					x.PW_BondType == type
					&& x.PW_Password == code
					&& x.PW_HolderIdentification == holderId))
			{
				var guarantee = declaration.Guarantees.AddNew();
				guarantee.PW_BondType = type;
				guarantee.PW_Password = code;
				guarantee.PW_HolderIdentification = holderId;
			}
		}

		protected void AddAuthorisationIfNotExists(IEnumerable<JobComInvoiceLine> invoiceLines, ZString code, ZString holderId, ZGuid owner)
		{
			foreach (var invoiceLine in invoiceLines)
			{
				if (invoiceLine.EntryInstruction != null
					&& !((CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x =>
																																				x.AGC_Code == code
																																				&& x.AGC_Number == holderId
																																				&& x.AGC_OH_Owner == owner))
				{
					var cusAuthorisation = ((CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.AddNew();
					cusAuthorisation.AGC_Code = code;
					cusAuthorisation.AGC_Number = holderId;
					cusAuthorisation.AGC_OH_Owner = owner;
				}
			}
		}

		static class Constants
		{
			public static class TaxLinePaymentMethods
			{
				public const string N = "N";
				public const string P = "P";
			}

			public static class AdditionalInformation
			{
				public static class Codes
				{
					public const string Importer = "00500";
					public const string Exporter = "00400";
				}

				public static class Descriptions
				{
					public const string Importer = "IMPORTER";
					public const string Exporter = "EXPORTER";
				}
			}
		}
	}
}
