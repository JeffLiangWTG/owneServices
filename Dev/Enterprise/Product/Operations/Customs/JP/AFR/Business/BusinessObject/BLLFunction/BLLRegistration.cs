using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	public sealed class BLLRegistration : BLLFunction
	{
		public BLLRegistration(JPAFRHeader header, BLLFunctionCode functionCode, JPAFRBills bill = null) : base(header, functionCode, bill)
		{
			RegisteredBills.RemoveRange(RegisteredBills.Cast<BLLFunctionBill>().Where(x => x.AFRBill.BLLFunctionInfo != null).ToArray());

			if (SelectedAFRBill == null)
			{
				Reset(RegisteredBills.Cast<BLLFunctionBill>());
			}
			else
			{
				JPM_BillOfLadingNumber = SelectedAFRBill.JPB_BillNumber;
			}
		}

		public override ZString JPM_BillOfLadingNumber
		{
			get => base.JPM_BillOfLadingNumber;
			set
			{
				var oldValue = JPM_BillOfLadingNumber;
				if (oldValue != value)
				{
					base.JPM_BillOfLadingNumber = value;

					Reset(RegisteredBills.Cast<BLLFunctionBill>().Where(x => x.JPM_BillOfLadingNumber != JPM_BillOfLadingNumber));
				}
			}
		}

		public override ZBool SelectEnabled =>
			AvailableBills.Any()
			&& (
				FunctionCode != BLLFunctionCode.RegisterSwitch
				&& SelectedBills.Count < 10
				|| FunctionCode == BLLFunctionCode.RegisterSwitch
				&& SelectedBills.Count == 0
			);

		public override ZBool UnselectEnabled => SelectedBills.Any();

		public override ZBool SendEnabled =>
			FunctionCode != BLLFunctionCode.RegisterSwitch
			&& SelectedBills.Count <= 10
			&& SelectedBills.Count >= 2
			|| FunctionCode == BLLFunctionCode.RegisterSwitch
			&& SelectedBills.Count == 1;

		void Reset(IEnumerable<BLLFunctionBill> bills)
		{
			SelectedBills.RemoveAll();
			AvailableBills.RemoveAll();
			AvailableBills.AddRange(bills);
		}
	}
}
