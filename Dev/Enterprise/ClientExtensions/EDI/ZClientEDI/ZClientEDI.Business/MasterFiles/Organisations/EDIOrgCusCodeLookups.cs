
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgCusCodeLookups : OrgCusCodeLookups
	{
		public EDIOrgCusCodeLookups(EDIOrgCusCode parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetOK_CodeType_List()
		{
			var result = base.GetOK_CodeType_List();
			result.AddPair(BillingConstants.BillingSystem.ABMCustoms, "ABM Customs Company Code");
			return result;
		}
	}
}

