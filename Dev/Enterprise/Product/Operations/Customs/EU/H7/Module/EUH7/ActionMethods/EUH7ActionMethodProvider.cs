using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.H7.Module;

public class EUH7ActionMethodProvider : OperationalActionMethodProvider
{
	public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
	{
		return new OperationalActionMethod[]
		{
			new SendH7ToCustomsActionMethod()
		};
	}
}
