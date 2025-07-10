using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class G5MessageSendingObject : GenericMessageSendingObject
{
	public G5MessageSendingObject(TemporaryStorageHeader header, GlbStaff broker) : base(broker, header?.AMA_CustomsProfile ?? ZString.Empty)
	{
		Header = Argument.NotNull(header, nameof(header));
	}

	public TemporaryStorageHeader Header { get; }
}
