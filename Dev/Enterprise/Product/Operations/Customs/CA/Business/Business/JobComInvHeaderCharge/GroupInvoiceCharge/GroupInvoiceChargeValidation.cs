using System.Linq;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public GroupInvoiceCharge GroupInvoiceCharge
		{
			get { return Parent; }
		}

		protected new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			var groupInvoiceCharge = GroupInvoiceCharge;
			if ((double)(groupInvoiceCharge.J7_Amount * groupInvoiceCharge.J7_ExchangeRate) > 99999.00 && groupInvoiceCharge.J7_ChargeType == CAChargeTypeList.Codes.OverseasFreight)
			{
				groupInvoiceCharge.J7_AmountInfo.AddMessageError(Res.GetString("f891f2b8-bf9c-41f6-87ba-3618f262fc92", "Amount for overseas freight may not exceed CAD 99,999"));
			}
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			ValidateJ7_Amount();
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateGroupInvoiceChargeForLVSJob();
		}

		void ValidateGroupInvoiceChargeForLVSJob()
		{
			var groupInvoice = Parent.GroupInvoice;
			var declaration = groupInvoice == null ? null : (JobDeclaration)groupInvoice.JobDeclaration;
			if (declaration != null && declaration.IsConsolidatedLVS && declaration.Invoices.Any(x => x.JZ_JE != declaration.PK))
			{
				Parent.AddRowWarning(Res.GetString("262ce889-9c04-446a-9eae-2947db8de741", "This charge will only be apportioned over LVS shipments manually entered on this consolidation. i.e. the charge will not be apportioned to any shipments attached from the Courier LVS Declarations module grid."));
			}
		}
	}
}
