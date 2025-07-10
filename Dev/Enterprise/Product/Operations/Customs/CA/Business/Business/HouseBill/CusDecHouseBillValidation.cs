using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		protected override void CheckCU_PackTypeIsAValidCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.CU_PackTypeInfo, Parent.Lookups.NoOfPacksPackType_List, (IMultilingualString)CU_PackTypInvalideMessage);
		}

		protected override void CheckCU_BillNum()
		{
			base.CheckCU_BillNum();
			if (Parent.CU_BillNum.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.CU_BillNumInfo.AddWarning(Res.GetString("59d6efed-0ecc-464c-95ce-d25017638f3b", "The Bill Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}
		}
	}
}
