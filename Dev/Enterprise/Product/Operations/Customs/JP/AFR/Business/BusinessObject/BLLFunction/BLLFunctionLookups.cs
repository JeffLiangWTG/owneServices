using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLFunctionLookups : ZLookups
	{
		public BLLFunctionLookups(BLLFunction parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList ChangeReasonCodeList => Factory.GetCachedValue<AFRBLLChangeReasonCodeList>();

		public virtual ICodeDescriptionPairList RegisteredBillList
		{
			get
			{
				return Factory.GetCachedValue(Invariant($"BLLFunction|RegisteredBillList|{Parent.FunctionCode}"), () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(Parent.RegisteredBills);
					return result;
				});
			}
		}

		protected new BLLFunction Parent => (BLLFunction)base.Parent;
	}
}
