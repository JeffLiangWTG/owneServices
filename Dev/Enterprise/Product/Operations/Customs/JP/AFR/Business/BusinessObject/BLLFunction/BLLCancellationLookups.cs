using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLCancellationLookups : BLLFunctionLookups
	{
		public BLLCancellationLookups(BLLFunction parent) : base(parent)
		{
		}

		public override ICodeDescriptionPairList RegisteredBillList
		{
			get
			{
				var functionCode = Parent.FunctionCode;
				return Factory.GetCachedValue(Invariant($"BLLFunction|RegisteredBillList|{functionCode}"), () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(Parent.RegisteredBills.Cast<BLLFunctionBill>().Where(x =>
					{
						var res = false;
						var billFuncitonInfo = x.AFRBill?.BLLFunctionInfo;
						if (billFuncitonInfo != null && billFuncitonInfo.JP_MasterBillNumber == x.AFRBill.JPB_BillNumber)
						{
							switch (functionCode)
							{
								case BLLFunctionCode.CancelSplit:
									res = billFuncitonInfo.JP_FunctionCode == (int)BLLFunctionCode.RegisterSplit;
									break;
								case BLLFunctionCode.CancelSwitch:
									res = billFuncitonInfo.JP_FunctionCode == (int)BLLFunctionCode.RegisterSwitch;
									break;
								case BLLFunctionCode.CancelMerge:
									res = billFuncitonInfo.JP_FunctionCode == (int)BLLFunctionCode.RegisterMerge;
									break;
							}
						}

						return res;
					}).ToArray<ICodeDescription>());

					return result;
				});
			}
		}
	}
}
