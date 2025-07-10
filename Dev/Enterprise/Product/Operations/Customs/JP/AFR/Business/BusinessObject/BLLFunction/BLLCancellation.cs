using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	public sealed class BLLCancellation : BLLFunction
	{
		public BLLCancellation(JPAFRHeader header, BLLFunctionCode functionCode, JPAFRBills bill = null) : base(header, functionCode, bill)
		{
			if (SelectedAFRBill != null)
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

					var masterBill = MasterAFRBill;
					var bllFunctionInfo = masterBill?.BLLFunctionInfo;

					IEnumerable<BLLFunctionBill> bills;
					if (masterBill != null && bllFunctionInfo != null)
					{
						bills = bllFunctionInfo.LinkedBills
							.Select(GetBLLFunctionBill)
							.Where(x => x != null);

						JPM_ChangeReasonCode = bllFunctionInfo.JP_ChangeReasonCode;
					}
					else
					{
						bills = Enumerable.Empty<BLLFunctionBill>();
					}

					Reset(bills.ToArray());
				}
			}
		}

		BLLFunctionBill GetBLLFunctionBill(ZString billNumber)
		{
			return RegisteredBills.Cast<BLLFunctionBill>().FirstOrDefault(x => x.JPM_BillOfLadingNumber == billNumber);
		}

		public override ZBool SelectEnabled => false;

		public override ZBool UnselectEnabled => false;

		public override ZBool SendEnabled =>
			FunctionCode != BLLFunctionCode.CancelSwitch
			&& SelectedBills.Count <= 10
			&& SelectedBills.Count >= 2
			|| FunctionCode == BLLFunctionCode.CancelSwitch
			&& SelectedBills.Count == 1;

		protected override BLLFunctionLookups CreateLookups()
		{
			return new BLLCancellationLookups(this);
		}

		void Reset(BLLFunctionBill[] bills)
		{
			AvailableBills.RemoveAll();
			AvailableBills.AddRange(RegisteredBills.Except(bills.Union(new[] { GetBLLFunctionBill(JPM_BillOfLadingNumber) })));
			SelectedBills.RemoveAll();
			SelectedBills.AddRange(bills);
		}
	}
}
