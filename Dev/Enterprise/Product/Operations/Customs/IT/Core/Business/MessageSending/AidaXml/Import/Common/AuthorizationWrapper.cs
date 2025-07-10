using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class AuthorizationWrapper : AuthorizationWrapperBase
{
	public AuthorizationWrapper(EU.Business.CusAuthorizationUsage authorization) : base(authorization, x => x.AGC_Code)
	{
	}
}
