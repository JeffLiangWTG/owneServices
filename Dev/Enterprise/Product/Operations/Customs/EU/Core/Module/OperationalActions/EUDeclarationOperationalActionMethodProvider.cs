using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Module.OperationalActions
{
	public class EUDeclarationOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new DeclarationUpdateSupportingDocumentsOperationalActionMethod(),
				new DeclarationUpdatePreviousDocumentsOperationalActionMethod()
			};
		}
	}
}
