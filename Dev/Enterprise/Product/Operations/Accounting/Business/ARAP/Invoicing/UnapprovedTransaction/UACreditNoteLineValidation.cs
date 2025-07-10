using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UACreditNoteLineValidation : CreditNoteLineValidation
	{
		public UACreditNoteLineValidation(UACreditNoteLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			if (ParentHeader == null || !ParentHeader.IsInDatabase ||
				ParentHeader.RelatedClaim == null || !ParentHeader.RelatedClaim.IsIntercompanyClaim)
			{
				base.ValidateAll();
			}
		}

		protected override void CheckGenericCharge()
		{
			base.CheckGenericCharge();

			if (Parent.AL_AC.IsValid && !Parent.AL_ACInfo.HasErrors() && ParentHeader != null && ParentHeader.IsRelatedToClaim && ParentHeader.RelatedClaim.TransactionHeader != null)
			{
				ZQuery chargeCodeInInvoicesCompanyQuery = new ZQuery(AccChargeCodeSchema.AC_GC, ParentHeader.RelatedClaim.TransactionHeader.AH_GC);
				chargeCodeInInvoicesCompanyQuery.AddToFilter(AccChargeCodeSchema.AC_Code, Parent.ChargeCode.AC_Code);
				if (!Parent.Factory.ExistsInDatabase(AccChargeCodeSchema.Constants.TableName, chargeCodeInInvoicesCompanyQuery))
				{
					Parent.GenericChargeInfo.AddError(Res.GetString("1155db86-87cc-4b85-b686-207294d2ecb3", @"This Charge Code cannot be used here. 
Charge Code must exist in Creditor company."));
				}
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		protected new UACreditNoteLine Parent
		{
			get { return base.Parent as UACreditNoteLine; }
		}

		UACreditNote fParentHeader;
		UACreditNote ParentHeader
		{
			get { return fParentHeader ?? (fParentHeader = Parent.Factory.Load<UACreditNote>(Parent.TransactionHeader?.PK ?? ZGuid.Empty)); }
		}
	}
}
