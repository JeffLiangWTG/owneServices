using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Utils;

namespace Enterprise.Customs.JP.Business
{
	public class BillValidation : CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckBillsCount();
		}

		void CheckBillsCount()
		{
			var declaration = Bill.Declaration;
			if (declaration?.Bills != null)
			{
				var masterBillCount = declaration.Bills.NumberOfMasterBill;
				var houseBillCount = declaration.Bills.NumberOfHouseBill;
				var billCount = declaration.Bills.Count;
				var maxMasterBillCount = 1;
				var maxHouseBillCount = declaration.IsSea ? 5 : 1;
				if ((declaration.IsAir || (declaration.IsSea && declaration.IsImport)) && (masterBillCount > maxMasterBillCount || houseBillCount > maxHouseBillCount || billCount > maxHouseBillCount + maxMasterBillCount))
				{
					Bill.AddRowMessageError(Res.GetString("85E5838B-A254-43B7-8FDC-44AEE2F47990", "There can only be at most {0} master bill, {1} house bills. The extra bills may be discarded when sending to customs.", maxMasterBillCount, maxHouseBillCount));
				}
			}
		}

		protected override void CheckCU_BillNum()
		{
			base.CheckCU_BillNum();

			var bill = Bill;
			if (bill.Declaration is JobDeclaration declaration && ((declaration.IsAir && bill.IsHouseBill && bill.CU_BillNum.Length > 20) || (declaration.IsSea && bill.CU_BillNum.Length < 5)))
			{
				bill.CU_BillNumInfo.AddMessageError(Res.GetString("978BD060-D1BF-4F66-A706-BD01F2B30A4E", "Bill Number is not of a correct length."));
			}
		}

		protected override void CheckCU_BillType()
		{
			base.CheckCU_BillType();

			var bill = Bill;
			if (bill.IsHouseBill
				&& bill.Declaration is JobDeclaration declaration
				&& declaration.IsSea && declaration.IsImport
				&& declaration.Bills.NumberOfHouseBill > 1)
			{
				var singleBillAttributeType = new ZString[] { BondedLocationCodeTypeList.Codes.BargeHandling, BondedLocationCodeTypeList.Codes.ShipHandling, BondedLocationCodeTypeList.Codes.DeclaredOnArrival, BondedLocationCodeTypeList.Codes.DeclaredPreArrival };

				var code = declaration.GetDepotCode();
				var refCusCode = JPRefCusCodeListTypes.GetJapanBondedAreaCode(bill.Factory, code);

				if (refCusCode != null && singleBillAttributeType.Contains(refCusCode.GetBondedLocationCodeType()))
				{
					bill.AddRowMessageError(Res.GetString("3695C0E3-354E-41FD-B302-B83E1B4DAB76", "Depot type does not allow multiple bills."));
				}
			}
		}
	}
}
