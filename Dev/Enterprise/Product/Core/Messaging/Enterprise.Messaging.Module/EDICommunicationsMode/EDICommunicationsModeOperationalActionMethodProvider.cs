using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return
			[
				new EDICommunicationsModeOperationalActionMethod()
			];
		}
	}
}
