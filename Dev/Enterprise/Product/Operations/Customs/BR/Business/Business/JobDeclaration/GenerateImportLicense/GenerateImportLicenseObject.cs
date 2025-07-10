using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Integration.Freight;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GenerateImportLicenseObject : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string ImportLicenseDeclarationPK = "ImportLicenseDeclarationPK";
		}

		public GenerateImportLicenseObject(CusEntryLine entryLine) : base(entryLine.Factory)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		protected override ZString HumanReadableNameCore => Res.GetString("8955BEA8-F039-4A63-A95C-353F3DD1685E", "Generate Import License");

		internal readonly CusEntryLine EntryLine;
		internal JobDeclaration ImportLicenseDeclaration => Factory.Load<JobDeclaration>(importLicenseDeclarationPK);

		[ResourceStringData("Enterprise.Customs.BR.Business.GenerateImportLicenseObject|ImportLicenseDeclarationPK", Caption = "Import License Declaration")]
		public ZGuid ImportLicenseDeclarationPK
		{
			get => importLicenseDeclarationPK;
			set
			{
				SetNonPersistentPropertyValue(ImportLicenseDeclarationPKInfo, ref importLicenseDeclarationPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateImportLicenseDeclarationPK();
				}
			}
		}
		ZGuid importLicenseDeclarationPK;

		public ZPropertyInfo ImportLicenseDeclarationPKInfo => GetZPropertyInfo(Schema.ImportLicenseDeclarationPK);

		public JobDeclarationCollection PossibleImportLicenseDeclarationForGenerate_List => GenerateLicenseJobDeclarationModuleCollection.GetListForGenerateImportLicense(EntryLine.Declaration);

		#region GenerateImportLicense

		public bool GenerateImportLicense()
		{
			var result = false;

			if (ImportLicenseDeclaration != null)
			{
				var entryInstruction = ImportLicenseDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Description = $"{EntryLine.Declaration.JE_DeclarationReference}-{EntryLine.CL_LineNumber}";

				var invoices = new HashSet<JobComInvoiceHeader>();
				foreach (var invoiceLine in EntryLine.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					var invoice = ImportLicenseDeclaration.Invoices.FirstOrDefault(x => x.JZ_InvoiceNumber == invoiceLine.InvoiceHeader.JZ_InvoiceNumber) as JobComInvoiceHeader;
					if (invoice == null)
					{
						invoice = invoiceLine.InvoiceHeader.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromInvoiceHeader)) as JobComInvoiceHeader;
						invoice.ExchangeHedgeCollection.CloneFrom(invoiceLine.InvoiceHeader.ExchangeHedgeCollection);
						ImportLicenseDeclaration.Invoices.Add(invoice);
					}

					AddChargesOnInvoice(invoice, invoiceLine.ApportionedCharges);

					invoices.Add(invoice);

					var clonedLine = invoiceLine.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromInvoiceLine)) as JobComInvoiceLine;
					ImportLicenseDeclaration.InvoiceLines.Add(clonedLine);
					clonedLine.JI_JZ = invoice.PK;
					clonedLine.JI_CEI = entryInstruction.PK;

					clonedLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
					clonedLine.JI_ParentID = invoiceLine.PK;
					invoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
					invoiceLine.JI_ParentID = clonedLine.PK;

					CopyInvoiceLineData(invoiceLine, clonedLine);
				}

				invoices.ForEach(x => x.UpdateTotalValuesFromInvoiceLines());
				EntryLine.Declaration.AttachedImportLicenseEntries.AddPivotFor(entryInstruction);

				try
				{
					ImportLicenseDeclaration.Factory.Save();
					result = true;
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return result;
		}

		void AddChargesOnInvoice(JobComInvoiceHeader invoice, JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> apportionedCharges)
		{
			foreach (var apportionedCharge in apportionedCharges)
			{
				invoice.Charges.AddCharge(apportionedCharge.ApportionChargeKey, apportionedCharge.J7_Amount, apportionedCharge.J7_RX_NKCurrency);
			}
		}

		public ZString CanGenerateImportLicense()
		{
			var result = ZString.Empty;
			if (!EntryLine.Header.MovementReferenceNumber.IsEmpty)
			{
				result = Res.GetString("BE5DDCC0-DB63-41D4-A49D-2718202B17F7", "Entry Header is already registered");
			}
			else if (EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.ImportLicenseNumber.IsEmpty))
			{
				result = Res.GetString("72D9F953-86F1-41DE-92BF-424DEEC34044", "There is already an Import License registered from this Entry Line");
			}
			else if (EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.IsImportSiscomexWithGeneratedImportLicenseLine))
			{
				result = Res.GetString("00338094-84f6-434e-8d16-7f1ea306bd8f", "There is License Job created from this Entry Line. Please, detach License and repeat the operation.");
			}

			return result;
		}

		void CopyInvoiceLineData(JobComInvoiceLine invoiceLine, JobComInvoiceLine clonedLine)
		{
			clonedLine.TariffDetachs.CloneFrom(invoiceLine.TariffDetachs);
			clonedLine.NVECusCodeDataCollection.CopyDataFrom(invoiceLine.NVECusCodeDataCollection);
			clonedLine.NaladiHs = invoiceLine.NaladiHs;

			clonedLine.DutyTaxRegime = invoiceLine.DutyTaxRegime;
			clonedLine.DutyLegalBase = invoiceLine.DutyLegalBase;

			clonedLine.EffectiveManufacturerAddressPK = invoiceLine.EffectiveManufacturerAddressPK;

			var tariffType = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement)?.TariffType ?? ZString.Empty;
			if (!tariffType.IsEmpty)
			{
				clonedLine.JI_SecondaryPreference = tariffType;
			}

			invoiceLine.Charges.Cast<InvoiceLineCharge>().Where(x => !x.J7_IsNotIncludedInInvoice).ForEach(charge =>
			{
				var clonedCharge = charge.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromCharges));
				clonedLine.Charges.Add(clonedCharge);
			});
		}

		readonly string[] columnNamesToExcludeFromInvoiceHeader = new[]
		{
			JobComInvoiceHeaderSchema.Constants.JZ_JE,
			JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
			JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey,
			JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill,
			JobComInvoiceHeaderSchema.Constants.JZ_StandAloneInvoiceDirection,
			JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
			JobComInvoiceHeaderSchema.Constants.JZ_ValuationDateOverride,
			JobComInvoiceHeaderSchema.Constants.JZ_Weight,
			JobComInvoiceHeaderSchema.Constants.JZ_NetWeight,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
		};

		readonly string[] columnNamesToExcludeFromInvoiceLine = new[]
		{
			JobComInvoiceLineSchema.Constants.JI_JZ,
			JobComInvoiceLineSchema.Constants.JI_CEI,
			JobComInvoiceLineSchema.Constants.JI_CL,
			JobComInvoiceLineSchema.Constants.JI_CO,
			JobComInvoiceLineSchema.Constants.JI_JO,
			JobComInvoiceLineSchema.Constants.JI_ClusterKey,
			JobComInvoiceLineSchema.Constants.JI_MatchingKey,
			JobComInvoiceLineSchema.Constants.JI_PrimaryPreference,
			JobComInvoiceLineSchema.Constants.JI_ValuationCode,
			JobComInvoiceLineSchema.Constants.JI_BrandName,
			JobComInvoiceLineSchema.Constants.JI_Model,
			JobComInvoiceLineSchema.Constants.JI_SerialNumber,
		};

		readonly string[] columnNamesToExcludeFromCharges = new[]
		{
			InvoiceLineCharge.Schema.J7_ParentID,
			InvoiceLineCharge.Schema.J7_ParentTableCode,
		};

		#endregion

		#region Validation

		public GenerateImportLicenseObjectValidation Validation
		{
			get { return new GenerateImportLicenseObjectValidation(this); }
		}

		#endregion
	}
}
