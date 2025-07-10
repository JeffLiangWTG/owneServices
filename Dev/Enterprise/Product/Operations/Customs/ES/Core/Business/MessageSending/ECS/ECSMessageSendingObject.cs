using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class ECSMessageSendingObject : GenericMessageSendingObject
	{
		public ECSMessageSendingObject(CusExitControlHeader cusExitHeader, GlbStaff broker) : base(broker, cusExitHeader?.CEH_CustomsProfile ?? ZString.Empty)
		{
			ExitHeader = Argument.NotNull(cusExitHeader, nameof(cusExitHeader));
		}
		public CusExitControlHeader ExitHeader { get; }
	}
}
