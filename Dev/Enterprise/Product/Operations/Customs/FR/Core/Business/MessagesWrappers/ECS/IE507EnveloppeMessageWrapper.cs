using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS
{
	public class IE507EnveloppeMessageWrapper : ECSEnveloppeMessageWrapper
	{
		public IE507EnveloppeMessageWrapper(CusExitDetail exitDetail) : base(exitDetail) { }

		protected override ZString GetSchemaID() => "MessageIE507";
	}
}
