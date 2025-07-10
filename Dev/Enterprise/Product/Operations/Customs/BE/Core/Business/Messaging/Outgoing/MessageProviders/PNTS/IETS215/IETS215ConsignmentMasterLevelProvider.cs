using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class IETS215ConsignmentMasterLevelProvider : IIETS215ConsignmentMasterLevel
{
	public IETS215ConsignmentMasterLevelProvider(TemporaryStorageBill temporaryStorageBill)
	{
		this.temporaryStorageBill = Argument.NotNull(temporaryStorageBill, nameof(temporaryStorageBill));
	}
	readonly TemporaryStorageBill temporaryStorageBill;

	public IPNTSDocument TransportDocument => transportDocument ??= new PNTSDocumentProvider(temporaryStorageBill);
	IPNTSDocument transportDocument;
}
