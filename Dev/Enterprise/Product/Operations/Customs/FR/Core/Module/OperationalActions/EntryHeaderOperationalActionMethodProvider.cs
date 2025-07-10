using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class EntryHeaderOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new CreditCODOperationalActionMethod(),
				new SendCancellationMessageOperationalActionMethod()
			};
		}
	}
}
