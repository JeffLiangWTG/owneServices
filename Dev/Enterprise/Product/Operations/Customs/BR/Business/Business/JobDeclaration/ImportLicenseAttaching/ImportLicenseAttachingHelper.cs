using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public static class ImportLicenseAttachingHelper
	{
		public static IEnumerable<JobComInvoiceLine> AttachImportLicense(this JobDeclaration declaration, IEnumerable<ImportLicenseAttachingObject> importLicenseAttachingObject)
		{
			var clonedInvoiceLines = new List<JobComInvoiceLine>();

			var firstEntryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			foreach (var importLicenseAttaching in importLicenseAttachingObject)
			{
				if (importLicenseAttaching.EntryInstruction is CusEntryInstruction entryInstruction)
				{
					foreach (var invoiceLine in entryInstruction.InvoiceLines)
					{
						var invoice = declaration.Invoices.FirstOrDefault(x => x.JZ_InvoiceNumber == invoiceLine.InvoiceHeader.JZ_InvoiceNumber);
						if (invoice == null)
						{
							invoice = invoiceLine.InvoiceHeader.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromInvoiceHeaderAttachingCopy)) as JobComInvoiceHeader;
							declaration.Invoices.Add(invoice);
						}

						var clonedLine = invoiceLine.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromInvoiceLineAttachingCopy)) as JobComInvoiceLine;
						declaration.InvoiceLines.Add(clonedLine);

						clonedLine.JI_JZ = invoice.PK;
						clonedLine.ImportLicenseFeeType = importLicenseAttaching.FeeType;
						clonedLine.JI_CEI = firstEntryInstruction.PK;
						CloneInvoiceLineForAttaching(clonedLine, invoiceLine);

						clonedInvoiceLines.Add(clonedLine);
					}
					declaration.AttachedImportLicenseEntries.AddPivotFor(entryInstruction);
				}
			}

			return clonedInvoiceLines;
		}

		static JobComInvoiceLine CloneInvoiceLineForAttaching(JobComInvoiceLine clonedLine, JobComInvoiceLine invoiceLine)
		{
			using (clonedLine.GetValidationSuspender())
			{
				clonedLine.ImportLicenseNumber = invoiceLine.EntryInstruction?.EntryHeader?.MovementReferenceNumber ?? ZString.Empty;
				clonedLine.ImportLicenseAuthorizationDate = invoiceLine.EntryInstruction?.EntryHeader?.CH_EntryReleaseDate ?? ZDateTime.Empty;

				clonedLine.EffectiveManufacturerAddressPK = invoiceLine.EffectiveManufacturerAddressPK;

				clonedLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
				clonedLine.JI_ParentID = invoiceLine.PK;

				if (clonedLine.IsImportSiscomex)
				{
					clonedLine.DutyTaxRegime = invoiceLine.DutyTaxRegime;
					clonedLine.DutyLegalBase = invoiceLine.DutyLegalBase;
					if (!invoiceLine.JI_SecondaryPreference.IsEmpty)
					{
						var additionalTariff = clonedLine.AdditionalTariffs.AddNew();
						additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
						additionalTariff.TariffType = invoiceLine.JI_SecondaryPreference.Left(additionalTariff.TariffTypeInfo.MaxLength);
						clonedLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
					}
					else if (!invoiceLine.DutyTaxRegime.IsEmpty)
					{
						if (invoiceLine.DutyTaxRegime == TaxRegimeList.Codes.FullCollection)
						{
							clonedLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;
						}
						else if (invoiceLine.DutyTaxRegime == TaxRegimeList.Codes.Reduction)
						{
							clonedLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
						}
					}
				}
			}

			return clonedLine;
		}

		static string[] columnNamesToExcludeFromInvoiceHeaderAttachingCopy => new[]
		{
			JobComInvoiceHeaderSchema.Constants.JZ_JE,
			JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
			JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey,
			JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill,
		};

		static string[] columnNamesToExcludeFromInvoiceLineAttachingCopy => new[]
		{
			JobComInvoiceLineSchema.Constants.JI_JZ,
			JobComInvoiceLineSchema.Constants.JI_CEI,
			JobComInvoiceLineSchema.Constants.JI_CL,
			JobComInvoiceLineSchema.Constants.JI_ClusterKey,
			JobComInvoiceLineSchema.Constants.JI_MatchingKey,
			JobComInvoiceLineSchema.Constants.JI_CO,
		};

		public static void DetachImportLicense(this JobDeclaration declaration, IEnumerable<CusEntryInstruction> entryInstructions)
		{
			var linkedLinesQuery = new ZQuery(JobComInvoiceLineSchema.JI_CEI, entryInstructions.Select(x => x.PK));
			var linkedLinePKs = declaration.Factory.Load<JobComInvoiceLine>(linkedLinesQuery).Select(x => x.PK).ToArray();

			var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_ParentID.IsValid && linkedLinePKs.Contains(x.JI_ParentID)).ToList();
			var invoiceHeaders = invoiceLines.Select(x => x.InvoiceHeader).Distinct().ToList();

			foreach (var invoiceLine in invoiceLines.Where(x => !x.IsDeleted))
			{
				if (invoiceLine.IsImportSiscomexWithGeneratedImportLicenseLine && invoiceLine.AttachedImportLicenseLine is JobComInvoiceLine importLicenseLine)
				{
					invoiceLine.ImportLicenseNumber = ZString.Empty;
					invoiceLine.JI_ParentTableCode = ZString.Empty;
					invoiceLine.JI_ParentID = ZGuid.Empty;

					importLicenseLine.JI_ParentTableCode = ZString.Empty;
					importLicenseLine.JI_ParentID = ZGuid.Empty;
				}
				else
				{
					invoiceLines.ForEach(x => x.Delete());
				}
			}

			invoiceHeaders.ForEach(x =>
			{
				if (x.InvoiceLines.Count == 0)
				{
					x.Delete();
				}
			});

			foreach (var entryInstruction in entryInstructions)
			{
				declaration.AttachedImportLicenseEntries.DeletePivotFor(entryInstruction);
			}
		}

		public static CusEntryInstruction[] GetPossibleEntryInstructionForAttachment(this JobDeclaration declaration)
		{
			var instructionQuery = new ZDBOnlyQuery(typeof(CusEntryInstruction));
			instructionQuery.AddToFilter(CusEntryInstructionSchema.CEI_ClusterKey, declaration.JE_ClusterKey);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CEI_Instruction);
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			entryNumberQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, "");
			entryHeaderQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);
			instructionQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);

			var pivotQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, true);
			pivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot);
			instructionQuery.AddSubQuery(pivotQuery, JoinCondition.And);

			return declaration.Factory.Load<CusEntryInstruction>(instructionQuery);
		}
	}
}
