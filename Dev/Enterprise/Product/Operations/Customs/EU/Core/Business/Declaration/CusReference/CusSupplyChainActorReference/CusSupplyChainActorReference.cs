using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusSupplyChainActorReference : CommonCusReference, Integration.Customs.EU.ICusSupplyChainActorReference
	{
		public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("E9C550F3-41F5-43E1-8D3E-F62D19903029", "Supply Chain Actor Reference");

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = ReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);

			if (!result && property.Name != nameof(CFR_ParentTableCode) && property.Name != nameof(CFR_ParentID))
			{
				var parent = Parent;
				var header = parent is TemporaryStoragePackedItem packedItem ? packedItem.Bill?.Header : parent is TemporaryStorageBill bill ? bill.Header : null;

				if (header != null)
				{
					result = header.IsNoEditAllowedCustomsStatus();
				}
			}

			return result;
		}

		public new CusSupplyChainActorReferenceLookups Lookups => (CusSupplyChainActorReferenceLookups)base.Lookups;

		public new CusSupplyChainActorReferenceValidation Validation => (CusSupplyChainActorReferenceValidation)base.Validation;

		[ResourceStringData("EF6207DC-A6E5-4F6B-AA2C-B515F2C8641C", Caption = "Role")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference|CFR_Code|UCC6", Caption = "Role", FullDescription = "[13 14 031 000] Additional Supply Chain Actor < Role", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CFR_Code
		{
			get => base.CFR_Code;
			set => base.CFR_Code = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference|CFR_Reference|UCC6", Caption = "Identification Number", FullDescription = "[13 14 017 000] Additional Supply Chain Actor < Identification Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CFR_Reference
		{
			get => base.CFR_Reference;
			set
			{
				value = Provider.ReferenceColumnCasingToUpper ? value.ToUpperInvariant() : value;
				base.CFR_Reference = value;
			}
		}

		public override ZGuid OwnerOrgPK
		{
			get => base.OwnerOrgPK;
			set
			{
				var oldValue = OwnerOrgPK;
				base.OwnerOrgPK = value;
				if (!IsCopying && oldValue != OwnerOrgPK)
				{
					AutopopulateReference();
				}

				void AutopopulateReference()
				{
					var ownerOrg = Owner?.Header;
					CFR_Reference = Provider.GetReferenceFromOwner(ownerOrg).Left(CFR_ReferenceInfo.MaxLength);
				}
			}
		}

		public override ZString CFR_ParentTableCode
		{
			get => base.CFR_ParentTableCode;
			set
			{
				var oldValue = CFR_ParentTableCode;
				base.CFR_ParentTableCode = value;
				if (oldValue != CFR_ParentTableCode)
				{
					provider = CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(DataGroupingCode, CFR_ParentTableCode);
				}
			}
		}

		public CusSupplyChainActorReferenceProvider Provider
		{
			get
			{
				var dataGroupingCode = DataGroupingCode;
				if (provider == null || provider.DataGroupingCode != dataGroupingCode)
				{
					provider = CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(dataGroupingCode, CFR_ParentTableCode);
				}
				return provider;
			}
		}
		CusSupplyChainActorReferenceProvider provider;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFR_Type = CusReferenceTypeList.Codes.SupplyChainActor;
			if (Parent is TemporaryStorageBill)
			{
				CFR_ParentTableCode = AsycudaBillSchema.Constants.Prefix;
			}
		}

		protected override CusReferenceLookups GetNewLookups() => new CusSupplyChainActorReferenceLookups(this);

		protected override CusReferenceValidation GetNewValidation() => Provider.GetNewValidation(this);

		protected override bool SupportsCloneCore() => true;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CFR_Type = CusReferenceTypeList.Codes.SupplyChainActor;
			CFR_Code = SupplyChainActorRoleList.Codes.CS;
			CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}
#endif
	}
}
