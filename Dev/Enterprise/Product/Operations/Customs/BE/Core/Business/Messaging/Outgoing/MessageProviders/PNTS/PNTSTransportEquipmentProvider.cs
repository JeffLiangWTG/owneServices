using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSTransportEquipmentProvider : IPNTSTransportEquipment
{
	public PNTSTransportEquipmentProvider(TemporaryStorageContainer temporaryStorageContainer, TemporaryStorageBill temporaryStorageBill)
	{
		this.temporaryStorageContainer = Argument.NotNull(temporaryStorageContainer, nameof(temporaryStorageContainer));
		this.temporaryStorageBill = Argument.NotNull(temporaryStorageBill, nameof(temporaryStorageBill));
	}
	readonly TemporaryStorageContainer temporaryStorageContainer;
	readonly TemporaryStorageBill temporaryStorageBill;

	public string ContainerIdentificationNumber => temporaryStorageContainer.ACN_ContainerNumber;

	public string ContainerPackedStatus => temporaryStorageContainer.ACN_EmptyFullIndicator;

	public int NumberOfSeals => temporaryStorageBill.Packs.Where(x => x.Container?.PK == temporaryStorageContainer.PK).Sum(x => x.APA_PackQty);

	public IReadOnlyCollection<IPNTSSeal> Seals => seals ??= GetSeals().Select((x) => new PNTSSealProvider(x)).ToArray<IPNTSSeal>();
	IReadOnlyCollection<IPNTSSeal> seals;

	IEnumerable<ZString> GetSeals()
	{
		if (!temporaryStorageContainer.ACN_Seal1.IsEmpty)
		{
			yield return temporaryStorageContainer.ACN_Seal1;
		}

		if (!temporaryStorageContainer.ACN_Seal2.IsEmpty)
		{
			yield return temporaryStorageContainer.ACN_Seal2;
		}

		if (!temporaryStorageContainer.ACN_Seal3.IsEmpty)
		{
			yield return temporaryStorageContainer.ACN_Seal3;
		}

		foreach (var seal in temporaryStorageContainer.AdditionalSeals)
		{
			yield return seal.BK_SealNumber;
		}
	}
}
