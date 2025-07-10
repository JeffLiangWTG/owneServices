using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgCusCodeLookups : OrgCusCodeLookups
	{
		public UPEOrgCusCodeLookups(UPEOrgCusCode parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetOK_CodeType_List()
		{
			CodeDescriptionPairList result = base.GetOK_CodeType_List();
			CodeDescriptionPair uPECustomerAccountNumber = new CodeDescriptionPair(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumberDescription);
			result.Insert(0, uPECustomerAccountNumber);
			return result;
		}
	}
}
