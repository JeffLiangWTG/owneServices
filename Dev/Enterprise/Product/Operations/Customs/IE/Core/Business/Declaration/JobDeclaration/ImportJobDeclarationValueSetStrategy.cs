using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportJobDeclarationValueSetStrategy : JobDeclarationValueSetStrategy
	{
		public ImportJobDeclarationValueSetStrategy(JobDeclaration declaration) : base(declaration)
		{
			this.declaration = declaration;
		}
		protected readonly JobDeclaration declaration;

		protected override void DefaultImporterChanged()
		{
			base.DefaultImporterChanged();

			var importer = declaration.Importer;
			RemoveDefaultSupportingDocuments(importer);
			PopulateSupportingDocumentsForEntryInstructions(importer);
			if (importer != null)
			{
				PopulateDutyPayer(importer);
			}
		}

		protected override void DefaultDutyPayerChanged()
		{
			if (declaration.DutyPayer is { } dutypayer)
			{
				PopulateJE_DefermentAccountNumber(dutypayer);
			}
		}

		void PopulateDutyPayer(OrgHeader importer)
		{
			if (declaration.JE_OH_DutyPayer.IsEmpty)
			{
				if (
					declaration.IsUCC5
					&& !importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.IrelandCodeTypes.TraderAccountNumber).IsEmpty
					||
					declaration.JE_PaymentMethod == PaymentMethodList.Codes.E
					&& !declaration.JE_DefermentAccountNumber.IsEmpty
					&& declaration.CustomsEntryInstructions.Any(IsAppliableForPaymentMethodE)
					&& !importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber).IsEmpty
					&& EUOrgImpAddInfo.Get(importer, declaration.CountryCode) is EUOrgImpAddInfo { ZO_OtherDeferType: var deferType } && deferType == PaymentMethodList.Codes.E
				)
				{
					declaration.JE_OH_DutyPayer = importer.PK;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable const")]
		const string AdditionalDescriptionForDefaultDoc = "System added supporting document";

		void PopulateJE_DefermentAccountNumber(OrgHeader dutypayer)
		{
			if (declaration.JE_PaymentMethod == PaymentMethodList.Codes.E
				&& declaration.CustomsEntryInstructions.Any(IsAppliableForPaymentMethodE)
				&& !dutypayer.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber).IsEmpty
			)
			{
				declaration.JE_DefermentAccountNumber = dutypayer.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber);
			}
		}

		static bool IsAppliableForPaymentMethodE(CusEntryInstruction instruction) => instruction.IsH1 || instruction.IsH5 || instruction.IsI1;

		void RemoveDefaultSupportingDocuments(OrgHeader importer)
		{
			var isUCC5 = declaration.IsUCC5;
			var shouldDelete1A01 = (importer is null) || isUCC5 && !importer.CustomsCodes.Cast<OrgCusCode>().Any(p => p.OK_CodeType == OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation);
			var shouldDelete1A03 = (importer is null) || isUCC5 && !importer.CustomsCodes.Cast<OrgCusCode>().Any(p => p.OK_CodeType == OrgCusCode.IrelandCodeTypes.VatZeroRatedAct2010);
			var shouldDelete1A05 = (importer is null) || EUOrgImpAddInfo.Get(importer, declaration.CountryCode) is EUOrgImpAddInfo orgAddInfo1 && !orgAddInfo1.ZO_UseFr3FiscalRepresentation;

			if (shouldDelete1A01 || shouldDelete1A03 || shouldDelete1A05)
			{
				foreach (var entryInstruction in declaration.CustomsEntryInstructions)
				{
					var supportingDocuments = entryInstruction.SupportingDocuments;
					if (shouldDelete1A01)
					{
						var docToRemove = supportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == Constants.SupportingDocumentCodes._1A01 && x.CSI_AdditionalDescription == AdditionalDescriptionForDefaultDoc);
						if (docToRemove != null)
						{
							supportingDocuments.RemoveAndDelete(docToRemove);
						}
					}

					if (shouldDelete1A03)
					{
						var docToRemove = supportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == Constants.SupportingDocumentCodes._1A03 && x.CSI_AdditionalDescription == AdditionalDescriptionForDefaultDoc);
						if (docToRemove != null)
						{
							supportingDocuments.RemoveAndDelete(docToRemove);
						}
					}

					if (shouldDelete1A05)
					{
						var docToRemove = supportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == Constants.SupportingDocumentCodes._1A05 && x.CSI_ReferenceNumber == Constants.SupportingDocumentReferenceReferenceNumbers.IEPOSTPONED && x.CSI_AdditionalDescription == AdditionalDescriptionForDefaultDoc);
						if (docToRemove != null)
						{
							supportingDocuments.RemoveAndDelete(docToRemove);
						}
					}
				}
			}
		}

		void PopulateSupportingDocumentsForEntryInstructions(OrgHeader importer)
		{
			var isUCC5 = declaration.IsUCC5;

			var shouldPopulate1A01 = false;
			var vfaReferenceNumber = ZString.Empty;
			if (isUCC5 && importer?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(p => p.OK_CodeType == OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation) is OrgCusCode vfaCusCode)
			{
				shouldPopulate1A01 = true;
				vfaReferenceNumber = vfaCusCode.SecuredCustomsRegNo;
			}

			var shouldPopulate1A03 = false;
			var vzrReferenceNumber = ZString.Empty;
			if(isUCC5 && importer?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(p => p.OK_CodeType == OrgCusCode.IrelandCodeTypes.VatZeroRatedAct2010) is OrgCusCode vzrCusCode)
			{
				shouldPopulate1A03 = true;
				vzrReferenceNumber = vzrCusCode.SecuredCustomsRegNo;
			}

			var shouldPopulate1A05 = EUOrgImpAddInfo.Get(importer, declaration.CountryCode) is EUOrgImpAddInfo orgAddInfo && orgAddInfo.ZO_UseFr3FiscalRepresentation;

			if (shouldPopulate1A01 || shouldPopulate1A03 || shouldPopulate1A05)
			{
				foreach (var entryInstruction in declaration.CustomsEntryInstructions)
				{
					var supportingDocuments = entryInstruction.SupportingDocuments;
					var isH1toH5 = entryInstruction.IsH1 || entryInstruction.IsH2 || entryInstruction.IsH3 || entryInstruction.IsH4 || entryInstruction.IsH5;

					if (isH1toH5 && shouldPopulate1A01)
					{
						AddSupportingDocumentIfNeeded(supportingDocuments, Constants.SupportingDocumentCodes._1A01, vfaReferenceNumber);
					}

					if (isH1toH5 && shouldPopulate1A03)
					{
						AddSupportingDocumentIfNeeded(supportingDocuments, Constants.SupportingDocumentCodes._1A03, vzrReferenceNumber);
					}

					if (shouldPopulate1A05)
					{
						AddSupportingDocumentIfNeeded(supportingDocuments, Constants.SupportingDocumentCodes._1A05, Constants.SupportingDocumentReferenceReferenceNumbers.IEPOSTPONED);
					}
				}
			}
		}

		void AddSupportingDocumentIfNeeded(SupportingDocumentCollection supportingDocuments, ZString code, ZString referenceNumber)
		{
			if (!supportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == code && x.CSI_ReferenceNumber == referenceNumber))
			{
				var doc = supportingDocuments.AddNew(code, referenceNumber);
				doc.CSI_AdditionalDescription = AdditionalDescriptionForDefaultDoc;
			}
		}
	}
}
