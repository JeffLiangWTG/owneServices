using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBGuarantee : EU.Business.Declaration.GuaranteeForDeclaration
	{
		public GBGuarantee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override CusBondDetailLookups GetNewLookups() => new GBGuaranteeLookups(this);

		public new GBGuaranteeLookups Lookups => (GBGuaranteeLookups)base.Lookups;

		protected override bool IsLookupsCachedInBase => false;

		protected override CusBondDetailValidation GetNewValidation() => new GBGuaranteeValidation(this);

		[ResourceStringData("F2C70CE6-757B-4FDE-BB2C-65417AD8A10A", Caption = "Code", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("359912A2-3260-4ECB-A8F0-042865CFB34F", Caption = "[UCC 8/2] Code", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("5C2EB04C-DA9C-48A5-8721-C5A47DDBAC07", Caption = "[UCC 8/2] Code", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		[List(nameof(Lookups) + "." + nameof(GBGuaranteeLookups.AuthorisationTypeList))]
		public override ZString PW_Password
		{
			get => base.PW_Password;
			set
			{
				base.PW_Password = value;
				HandleHolderIdentificationChange();
			}
		}

		[ReadOnlyMember(nameof(BondNumbersShouldBeReadOnly))]
		[ResourceStringData("BFFB5566-0582-483C-A618-1F72FBD8AFCF", Caption = "GRN", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("9CA3C343-9B77-40CC-84F8-94067162C1B0", Caption = "[UCC 8/3] GRN", FullDescription = "[UCC 8/3] Reference. Field 8/3 should be completed using only one of the Reference or the GRN columns, depending on business context. Do not complete both. Refer to The Tariff for more information.", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("2B3F2167-9744-4259-8FFA-20CA79E22F53", Caption = "[UCC 8/3] GRN", FullDescription = "[UCC 8/3] Reference. Field 8/3 should be completed using only one of the Reference or the GRN columns, depending on business context. Do not complete both. Refer to The Tariff for more information.", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

		bool BondNumbersShouldBeReadOnly => PW_HolderIdentification != ZString.Empty;

		[ReadOnlyMember(nameof(BondNumbersShouldBeReadOnly))]
		[ResourceStringData("951EBD94-61B8-4FCC-8F52-60A1C8BC4794", Caption = "Other Guarantee Reference", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("386FC483-C961-4101-B695-E424080AEB9E", Caption = "[UCC 8/3] Other Guarantee Reference", FullDescription = "[UCC 8/3] GRN. Field 8/3 should be completed using only one of the Reference or the GRN columns, depending on business context. Do not complete both. Refer to The Tariff for more information.", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("F9E9D5B7-FFBA-47EF-8DEA-D2BCBCC55A0F", Caption = "[UCC 8/3] Other Guarantee Reference", FullDescription = "[UCC 8/3] GRN. Field 8/3 should be completed using only one of the Reference or the GRN columns, depending on business context. Do not complete both. Refer to The Tariff for more information.", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		public override ZString PW_BondNumber2 { get => base.PW_BondNumber2; set => base.PW_BondNumber2 = value; }

		public override ZString AccessCodeFieldType => GetCodeFieldType();

		string GetCodeFieldType()
		{
			if (Declaration != null)
			{
				return nameof(FieldType.TextDropEdit);
			}
			return nameof(FieldType.Text);
		}

		public ZBool IsAuthorisation => false; //TODO remove reference to this property

		public ZBool IsGuarantee => PW_BondType == GuaranteeTypeList.Codes.Guarantee;

		public ZBool IsRelatedToEntryInstruction(CusEntryInstruction entryInstruction)
		{
			return EntryInstruction == null || EntryInstruction == entryInstruction;
		}

		public override ZString ReferenceNumberFieldType => IsGuarantee ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new GBGuaranteeValidation Validation => (GBGuaranteeValidation)base.Validation;

		[ResourceStringData("6ED1001D-3EA3-48FD-9314-465D483E2D71", Caption = "[UCC 8/3] Holder Identification")]
		public override ZString PW_HolderIdentification
		{
			get => base.PW_HolderIdentification;
			set
			{
				base.PW_HolderIdentification = value;
				HandleHolderIdentificationChange();
			}
		}

		void HandleHolderIdentificationChange()
		{
			if (!PW_HolderIdentification.IsEmpty)
			{
				var validGRNFee = Declaration?.InvoiceLines?.OfType<JobComInvoiceLine>()?.Any(x => (x.JI_CEI == EntryInstructionID || EntryInstructionID.IsValid) && (x.ZG_MethodOfPayment == MethodOfPaymentCodes.P || x.ZG_MethodOfPayment == MethodOfPaymentCodes.N)) ?? false;

				validGRNFee = validGRNFee || (Declaration?.CustomsEntryHeaders?.Any(x => (x.EntryInstruction?.PK == EntryInstructionID || EntryInstructionID.IsValid) &&
							  x.MergedLines.OfType<CusEntryLine>().Any(cel => cel.Fees.OfType<CusEntryLineFee>().Any(f => f.CF_MethodOfPayment == MethodOfPaymentCodes.P || f.CF_MethodOfPayment == MethodOfPaymentCodes.N))) ?? false);

				if (PW_Password == "Y" || validGRNFee)
				{
					PW_BondNumber2 = ZString.Empty;
					PW_BondNumber = PW_HolderIdentification;
				}
				else
				{
					PW_BondNumber = ZString.Empty;
					PW_BondNumber2 = PW_HolderIdentification;
				}
			}
		}

		protected override void SyncroniseWithGuaranteeHeader(Customs.Business.BaseCusGuaranteeHeader guaranteeHeader)
		{
			if (Declaration == null)
			{
				base.SyncroniseWithGuaranteeHeader(guaranteeHeader);
			}
		}
	}
}
