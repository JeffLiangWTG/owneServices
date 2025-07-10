using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusFiscalReference : CommonCusReference, Integration.Customs.EU.ICusFiscalReference
	{
		public CusFiscalReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("6831E2D9-074E-43BD-9FC5-3E376E20EC40", Caption = "Code", FullDescription = "[13 16 031 000] Additional Fiscal Reference < Role", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CFR_Code
		{
			get => base.CFR_Code;
			set
			{
				var oldValue = base.CFR_Code;
				base.CFR_Code = value;
				if (!IsCopying && oldValue != value)
				{
					Provider.RecalculateOwnerIfNeeded(this);
					Provider.RecalculateReferenceIfNeeded(this);
				}
			}
		}

		[MaxLength(nameof(CFR_ReferenceMaxLenght))]
		[ReadOnlyMember(nameof(ReferenceIsReadOnly))]
		[ResourceStringData("A890A4FA-722E-452C-B9C6-C58EF0ED67FB", Caption = "Reference", FullDescription = "[13 16 034 000] Additional Fiscal Reference < VAT Identification Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public CusEntryInstruction Instruction => Parent as CusEntryInstruction;

		public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		int CFR_ReferenceMaxLenght => Provider.GetReferenceMaxLength(this);

		protected bool ReferenceIsReadOnly => Provider.ReferenceIsReadOnly(this);

		[ReadOnlyMember(nameof(OwnerIsReadOnly))]
		public override ZGuid CFR_OA_Owner
		{
			get => base.CFR_OA_Owner;
			set
			{
				var oldValue = base.CFR_OA_Owner;
				base.CFR_OA_Owner = value;
				if (!IsCopying && oldValue != value)
				{
					Provider.RecalculateReferenceIfNeeded(this);
				}
			}
		}

		bool OwnerIsReadOnly => Provider.OwnerIsReadOnly(this);

		public CusFiscalReferenceProvider Provider
		{
			get
			{
				var dataGroupingCode = DataGroupingCode;
				if (provider == null || provider.DataGroupingCode != dataGroupingCode)
				{
					provider = CusFiscalReferenceProvider.GetByDataGroupingCode(dataGroupingCode);
				}
				return provider;
			}
		}
		CusFiscalReferenceProvider provider;

		public new CusFiscalReferenceLookups Lookups => (CusFiscalReferenceLookups)base.Lookups;

		public new CusFiscalReferenceValidation Validation => (CusFiscalReferenceValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFR_Type = CusReferenceTypeList.Codes.FiscalReference;
		}

		protected override CusReferenceLookups GetNewLookups() => Provider.GetNewLookups(this);

		protected override CusReferenceValidation GetNewValidation() => Provider.GetNewValidation(this);

		protected override bool SupportsCloneCore() => true;

		public IReadOnlyList<string> MultipleKeysToUse => Instruction?.MultipleKeysToUse ?? Array.Empty<string>();

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CFR_Type = CusReferenceTypeList.Codes.FiscalReference;
			CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

#endif

		protected override ZString HumanReadableNameCore => Res.GetString("45E352C0-6086-4D30-9668-3EFECBE27DBA", "Fiscal Reference");
	}
}
