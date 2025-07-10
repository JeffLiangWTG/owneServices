using Enterprise.Customs.EU.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class FrDeclarationOperationalActionMethodProvider : EUDeclarationOperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				 new FrDeclarationDeltaSecondStepMessageOperationalActionMethod(),
				 new FrDeclarationSendValideeMessageOperationalActionMethod(),
				 new FrDeclarationCreditD48OperationalActionMethod(),
				 new FrDeclarationSendPrelodgeAmendmentOperationalActionMethod(),
				 new CreditCODOperationalActionMethod(),
				 new SendCancellationMessageOperationalActionMethod()
			};
		}
	}
}
