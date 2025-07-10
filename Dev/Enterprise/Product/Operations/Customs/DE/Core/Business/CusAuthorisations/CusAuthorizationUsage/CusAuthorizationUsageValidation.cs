using System.Linq;
using CargoWise.Types;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.DE.Business
{
	public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
	{
		public CusAuthorizationUsageValidation(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();
			var parent = Parent;
			var code = parent.AGC_Code;
			var targetInfo = parent.AGC_CodeInfo;
			var instruction = parent.Instruction;
			if (instruction != null)
			{
				var authorizations = instruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().ToArray();
				CheckCodeIsNotDuplicated();
				CheckCwpCw1Cw2AreMutuallyExclusive();

				void CheckCodeIsNotDuplicated()
				{
					var isCodeDuplicated = authorizations.Any(a => a.AGC_Code == code && a.PK != parent.PK);
					if (isCodeDuplicated)
					{
						targetInfo.AddMessageError(Res.GetString("FB15B770-FAF9-4C46-AB72-BEF44241E4DC", "A code of Type {0} has already been entered.", code));
					}
				}

				void CheckCwpCw1Cw2AreMutuallyExclusive()
				{
					var mutuallyExclusive = new ZString[]
					{
						CustomsWarehousingCW1, CustomsWarehousingCW2, CustomsWarehousingCWP,
					};

					if (mutuallyExclusive.Contains(code) && authorizations.Any(a => a.AGC_Code != code && mutuallyExclusive.Contains(a.AGC_Code)))
					{
						targetInfo.AddMessageError(Res.GetString("66A8F269-E98E-46BD-AFCC-CBABDE39BF27", "The Authorization Types CWP, CW1 and CW2 are mutually exclusive."));
					}
				}
			}
		}

		protected override void CheckAGC_OH_Owner()
		{
			base.CheckAGC_OH_Owner();
			var parent = Parent;
			var errorMessage = CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(parent);
			if (!errorMessage.IsEmpty)
			{
				parent.AGC_OH_OwnerInfo.AddMessageError(errorMessage);
			}
		}
	}
}
