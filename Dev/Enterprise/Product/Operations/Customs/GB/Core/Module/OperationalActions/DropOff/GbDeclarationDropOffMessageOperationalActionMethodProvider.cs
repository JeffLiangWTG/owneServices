using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.DropOff
{
	public class GbDeclarationDropOffMessageOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				 new GbDeclarationDropOffMessageOperationalActionMethod(),
			};
		}
	}
}
