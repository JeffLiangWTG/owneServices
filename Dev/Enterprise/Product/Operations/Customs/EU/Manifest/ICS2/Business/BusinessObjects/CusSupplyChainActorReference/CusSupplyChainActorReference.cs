using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusSupplyChainActorReference : EU.Business.Declaration.CusSupplyChainActorReference
	{
		public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("3E2A630A-08EB-44A9-96B2-BBB1B9AF7968", Caption = "Code")]
		[MaxLength(3)]
		public override ZString CFR_Code
		{
			get => base.CFR_Code;
			set => base.CFR_Code = value;
		}

		[ResourceStringData("6DC6485A-2EED-4E9A-B744-13B6512C39ED", Caption = "Organization")]
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
					var result = ownerOrg?.GetICS2EoriDetails() ?? ZString.Empty;
					CFR_Reference = result.Left(CFR_ReferenceInfo.MaxLength);
				}
			}
		}

		[MaxLength(17)]
		[ResourceStringData("5A956DE2-A4DB-470E-AF87-E6A0041D0297", Caption = "Identification Number", ShortCaption = "ID No.")]
		public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

		public new CusSupplyChainActorReferenceLookups Lookups => (CusSupplyChainActorReferenceLookups)base.Lookups;

		protected override CusReferenceLookups GetNewLookups() => new CusSupplyChainActorReferenceLookups(this);

		public new CusSupplyChainActorReferenceValidation Validation => (CusSupplyChainActorReferenceValidation)base.Validation;

		protected override CusReferenceValidation GetNewValidation() => new CusSupplyChainActorReferenceValidation(this);
	}
}
