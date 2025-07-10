using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonTransportEquipment
{
	ZString SequenceNumber { get; }
	ZString ContainerNumber { get; }
	IReadOnlyCollection<ICommonGoodsReference> GoodsReference { get; }
}

public interface ICommonGoodsReference
{
	ZString SequenceNumber { get; }
	ZString GoodsItemNumber { get; }
}
