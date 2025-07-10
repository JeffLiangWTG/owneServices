using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsMessageSendingObject : GenericMessageSendingObject
	{
		public NctsMessageSendingObject(NctsHeader header, GlbStaff broker) : base(broker, header?.BH_CustomsProfile ?? ZString.Empty)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public NctsHeader Header { get; }
	}
}
