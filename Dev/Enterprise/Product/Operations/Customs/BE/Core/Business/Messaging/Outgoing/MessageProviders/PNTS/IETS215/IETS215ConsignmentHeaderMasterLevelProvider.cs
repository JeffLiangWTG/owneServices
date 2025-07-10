using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class IETS215ConsignmentHeaderMasterLevelProvider : IIETS215ConsignmentHeaderMasterLevel
{
	public IETS215ConsignmentHeaderMasterLevelProvider(TemporaryStorageHeader temporaryStorageHeader)
	{
		this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		this.masterBill = Argument.NotNull(temporaryStorageHeader.MasterBill, nameof(masterBill));
	}
	readonly TemporaryStorageHeader temporaryStorageHeader;
	readonly EU.Business.CusTempStorage.TemporaryStorageBill masterBill;

	public IIETS215ConsignmentMasterLevel ConsignmentMasterLevel => consignmentMasterLevel ??= new IETS215ConsignmentMasterLevelProvider(masterBill);
	IIETS215ConsignmentMasterLevel consignmentMasterLevel;

	public IReadOnlyCollection<IIETS215ConsignmentHouseLevel> ConsignmentHouseLevels => consignmentHouseLevels ?? (consignmentHouseLevels =
		temporaryStorageHeader.HouseBills.Cast<EU.Business.CusTempStorage.TemporaryStorageBill>()
			.Select((x, i) => new IETS215ConsignmentHouseLevelProvider(x)).ToArray<IIETS215ConsignmentHouseLevel>());
	IReadOnlyCollection<IIETS215ConsignmentHouseLevel> consignmentHouseLevels;
}
