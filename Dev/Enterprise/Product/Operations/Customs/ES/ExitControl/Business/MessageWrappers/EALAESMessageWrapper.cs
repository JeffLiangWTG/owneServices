using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESMessageWrapper : IAESCommonMessage
	{
		public EALAESMessageWrapper(CusExitHeader exitHeader)
		{
			this.exitHeader = Argument.NotNull(exitHeader, nameof(exitHeader));
		}
		readonly CusExitHeader exitHeader;

		public ZString Sender => OrgHeaderExtension.GetIDCode(exitHeader.Carrier?.Header);

		public ZString MessageIdentification => EDIMessage.MessageNumberPlaceHolder;
	}
}
