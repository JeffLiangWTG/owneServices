using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS
{
	public class IE507DataWrapper : ECSDataWrapper
	{
		public IE507DataWrapper(CusExitDetail cusExitDetail) : base(cusExitDetail) { }

		protected override ECSEnveloppeMessageWrapper GetECSEnveloppeMessageWrapper() => new IE507EnveloppeMessageWrapper(ExitDetail);
	}
}
