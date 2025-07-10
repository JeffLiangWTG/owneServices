using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk
{
	public class CcsukOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				 new QueryWithUpdateMethod() ,
				 new RenominateMethod(),
				 new DetachFromForwardingMethod()
			};
		}
	}
}
