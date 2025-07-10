using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS
{
	public class IE618DataWrapper : ECSDataWrapper
	{
		public IE618DataWrapper(CusExitDetail cusExitDetail) : base(cusExitDetail) { }

		protected override ECSEnveloppeMessageWrapper GetECSEnveloppeMessageWrapper() => new IE618EnveloppeMessageWrapper(ExitDetail);
	}
}
