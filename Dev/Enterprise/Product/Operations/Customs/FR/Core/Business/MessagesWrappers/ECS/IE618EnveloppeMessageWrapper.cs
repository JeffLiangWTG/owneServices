using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS
{
	public class IE618EnveloppeMessageWrapper : ECSEnveloppeMessageWrapper
	{
		public IE618EnveloppeMessageWrapper(CusExitDetail exitDetail) : base(exitDetail) { }

		protected override ZString GetSchemaID() => "MessageIE618";
	}
}
