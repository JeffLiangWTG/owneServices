using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class TransportEquipmentWrapper : ITransportEquipment
{
	public TransportEquipmentWrapper(ZString containerNumber, ImmutableHashSet<int> entryLineNumbers, ImmutableList<string> seals) : this(entryLineNumbers, seals)
	{
		this.containerNumber = Argument.NotNullOrEmpty(containerNumber, nameof(containerNumber));
	}

	public TransportEquipmentWrapper(ImmutableHashSet<int> entryLineNumbers, ImmutableList<string> seals)
	{
		this.entryLineNumbers = Argument.NotNull(entryLineNumbers, nameof(entryLineNumbers));
		this.seals = Argument.NotNull(seals, nameof(seals));
	}

	readonly ZString containerNumber;
	readonly ImmutableHashSet<int> entryLineNumbers;
	readonly ImmutableList<string> seals;

	string ITransportEquipment.ContainerID => containerNumber;

	int ITransportEquipment.NumberOfSeals => seals.Count;

	IReadOnlyCollection<int> ITransportEquipment.LinkedGoodsItemNumbers => entryLineNumbers;

	IReadOnlyCollection<string> ITransportEquipment.Seals => seals;
}
