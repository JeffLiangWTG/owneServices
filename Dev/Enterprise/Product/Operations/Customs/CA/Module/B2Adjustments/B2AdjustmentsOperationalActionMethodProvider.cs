using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new B2AdjustmentsOperationalActionMethod()
			};
		}
	}
}
