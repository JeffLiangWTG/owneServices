using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class CustomsCodeAuthorizationWrapper : AuthorizationWrapperBase
{
	public CustomsCodeAuthorizationWrapper(EU.Business.CusAuthorizationUsage authorization) : base(authorization, x => x.CustomsCode)
	{
	}
}
