using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class UniversalNCTSDataObjectProvider : EU.NCTS.DataTransfer.IUniversalNCTSDataObjectProvider
{
	ITopLevelDataObjectReader EU.NCTS.DataTransfer.IUniversalNCTSDataObjectProvider.GetNewNctsHeaderDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZString applicationCode)
	{
		if (applicationCode == CusInBondApplicationCodeList.Codes.NCTS5)
		{
			return new EU.NCTS.DataTransfer.Phase5.NctsDepartureMovementHeaderDataObjectReader(declarationDataObject, logger, factory);
		}
		return new NctsHeaderDataObjectReader(declarationDataObject, logger, factory);
	}

	ITopLevelDataObjectWriter EU.NCTS.DataTransfer.IUniversalNCTSDataObjectProvider.GetNewNctsHeaderDataObjectWriter(IDataWritingManager manager, ZString applicationCode)
	{
		if (manager.Action?.ParentBO is NctsHeader nctsHeader && nctsHeader.IsPhase5)
		{
			switch (nctsHeader.BH_HeaderType.ToUpperInvariant())
			{
				case NctsMovementType.Codes.Departure:
					return new EU.NCTS.DataTransfer.Phase5.NctsDepartureMovementHeaderDataObjectWriter(manager);
				case NctsMovementType.Codes.Arrival:
					return new EU.NCTS.DataTransfer.Phase5.NctsArrivalMovementHeaderDataObjectWriter(manager);
				default:
					ErrorReporter.ReportOnce("No NCTS Phase 5 UXML Writer Support");
					break;
			}
		}

		return new NctsHeaderDataObjectWriter(manager);
	}
}
