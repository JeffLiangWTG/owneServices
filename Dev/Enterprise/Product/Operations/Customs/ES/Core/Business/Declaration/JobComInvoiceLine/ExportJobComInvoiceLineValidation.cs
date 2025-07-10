using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using JobComInvoiceLineMessageSchema = Enterprise.Customs.ES.Messaging.MessageSchema.JobComInvoiceLineMessageSchema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckAtLeastOneSupportingDocument();

			CheckMaxNumberOfAdditionalSupplyChainActor(Parent);
			CheckMaxAdditionalDocumentsTRA(Parent);

			CheckMaxAdditionalDocumentsINF(Parent);
			if (IsEXS)
			{
				CheckOneAdditionalDocumentsTRA_ForEntryEXS(Parent);
			}
		}

		void CheckMaxNumberOfAdditionalSupplyChainActor(JobComInvoiceLine parent)
		{
			var message = MaxNumberOfSupplyChainActorMessage;
			Parent.ClearRowNotificationsContaining(message);
			if (parent.CusSupplyChainActorReferences.Count > 99)
			{
				parent.AddRowMessageError(message);
			}
		}

		void CheckMaxAdditionalDocumentsTRA(JobComInvoiceLine parent)
		{
			var message = MaxNumberOfAdditionalDocumentsTRA;
			Parent.ClearRowNotificationsContaining(message);
			if (parent.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments) > 99)
			{
				parent.AddRowMessageError(message);
			}
		}

		void CheckOneAdditionalDocumentsTRA_ForEntryEXS(JobComInvoiceLine parent)
		{
			var message = OneAdditionalDocumentsTRA_ForEntryEXS;
			Parent.ClearRowNotificationsContaining(message);
			if (!parent.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments))
			{
				parent.AddRowMessageError(message);
			}
		}

		void CheckMaxAdditionalDocumentsINF(JobComInvoiceLine parent)
		{
			var message = MaxNumberOfAdditionalDocumentsINF;
			Parent.ClearRowNotificationsContaining(message);
			if (parent.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == AdditionalDocList.Codes.AdditionalInformation) > 99)
			{
				parent.AddRowMessageError(message);
			}
		}

		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();
			if (IsExportEntry)
			{
				if (Parent.Declaration.JE_GoodsOrigin.StartsWith(Core.Constants.CountryCodes.Spain, StringComparison.OrdinalIgnoreCase)
					|| Parent.JI_CountryOfOrigin.StartsWith(Core.Constants.CountryCodes.Spain, StringComparison.OrdinalIgnoreCase))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_StateOrRegionOfOriginInfo);
				}
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_StateOrRegionOfOriginInfo);
		}

		protected override bool IsTariffMandatory
		{
			get
			{
				return Parent.JI_Description.IsEmpty && !IsExportEntry || IsExportEntry;
			}
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			if (IsExportEntry)
			{
				base.CheckJI_CountryOfOriginMandatoryValidation();
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (IsExportEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
				if (Parent.Declaration.IsUCC6 && Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthExportAES)
				{
					Parent.JI_DescriptionInfo.AddWarning(GetExportDescriptionValidationMessage(JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthExportAES));
				}
				else if (!Parent.Declaration.IsUCC6 && Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthExport)
				{
					Parent.JI_DescriptionInfo.AddWarning(GetExportDescriptionValidationMessage(JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthExport));
				}
			}
			if (IsEXS && Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthEXS)
			{
				Parent.JI_DescriptionInfo.AddWarning(Res.GetString("7A4F30B1-5063-450B-9E69-C4164E8C622F", "When sending EXS declarations, the maximum length accepted for the description is {0}, so it will be trimmed", JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthEXS));
			}
			if (IsT2LExpedition && Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l)
			{
				Parent.JI_DescriptionInfo.AddWarning(Res.GetString("D629DA85-6CF4-4F1B-95FA-6B4C17799F36", "When sending T2L Expedition declarations, the maximum length accepted for the description is {0}, so it will be trimmed", JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l));
			}
		}

		protected override void CheckJI_CEI()
		{
			base.CheckJI_CEI();
			var entryInstruction = EntryInstruction;
			var procedure = Parent.JI_FormattedProcedure;
			var additionalProcedure = Parent.AdditionalProcedureCodesAsString;
			if (entryInstruction != null && entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.B && !(procedure.Contains("9VA") || procedure.Contains("9PV") || additionalProcedure.Contains("9VA") || additionalProcedure.Contains("9PV")))
			{
				Parent.JI_CEIInfo.AddMessageError(Res.GetString("4558895D-AE32-47FA-AFA8-824B83CD3FE4", "Simplified declarations (type B) require a national 9VA or 9PV concession in Box 37"));
			}
		}

		void CheckAtLeastOneSupportingDocument()
		{
			var invoiceLineNoSupDocs = !Parent.SupportingDocuments.Any();
			var invoiceHeaderNoSupDocs = !Parent.InvoiceHeader.SupportingDocuments.Any();
			var declarationNoSupDocs = !Parent.Declaration.SupportingDocuments.Any();
			if (IsT2LExpedition && invoiceLineNoSupDocs && invoiceHeaderNoSupDocs && declarationNoSupDocs)
			{
				Parent.AddRowMessageError(Res.GetString("53C55A4B-B481-4310-9F40-154D8329558B", "You have not entered any supporting documents for this line"));
			}
		}

		protected override bool HasValidPackagePivots
		{
			get
			{
				if (IsEXS && IsEXSWithCircunstanceEmptyOrE)
				{
					return base.HasValidPackagePivots;
				}
				else if (IsEXS && !IsEXSWithCircunstanceEmptyOrE)
				{
					return true;
				}
				else
				{
					return base.HasValidPackagePivots;
				}
			}
		}

		protected override bool IsJIProcedureMandatory => IsExportEntry;

		bool IsExportEntry => !(IsT2LExpedition || (EntryInstruction?.IsT2C ?? false) || IsEXS);

		bool IsT2LExpedition => EntryInstruction?.IsT2L ?? false;

		bool IsEXS => EntryInstruction?.IsEXS ?? false;

		bool IsEXSWithCircunstanceEmptyOrE => (Parent.Declaration.ZG_SpecificCircumstanceIndicator.IsEmpty || Parent.Declaration.ZG_SpecificCircumstanceIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators);

		ZString GetExportDescriptionValidationMessage(int exportMaxLength) => Res.GetString("108134B6-8C22-4B97-9A3E-56E519D21043", "When sending export declarations, the maximum length accepted for the description is {0}, so it will be trimmed", exportMaxLength);
	}
}
