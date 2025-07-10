using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class MessageSendingObject : GenericMessageSendingObject
	{
		public MessageSendingObject(CusExitHeader cusExitHeader, GlbStaff broker) : base(broker, cusExitHeader?.CXH_CustomsProfile ?? ZString.Empty)
		{
			ExitHeader = Argument.NotNull(cusExitHeader, nameof(cusExitHeader));
		}
		public CusExitHeader ExitHeader { get; }
	}
}
