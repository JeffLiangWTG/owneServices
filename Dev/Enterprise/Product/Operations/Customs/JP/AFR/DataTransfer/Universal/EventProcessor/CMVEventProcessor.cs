using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class CMVEventProcessor : AFREventProcessor
	{
		public CMVEventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory) : base(eventDataObject, logger, factory)
		{
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (IsSuccessResponse && header != null)
			{
				VesselInfoUpdater.UpdateVesselInformationDetails(logger, header,
					eventDataObject.Context.CarrierCode ?? ZString.Empty,
					eventDataObject.Context.VesselCallSign,
					eventDataObject.Context.VoyageNumber ?? ZString.Empty,
					eventDataObject.Context.PortOfLoadingUNLOCO ?? ZString.Empty,
					eventDataObject.Context.PortOfLoadingSuffix ?? ZString.Empty);

				var originalCMVMessage = GetOriginalMessage(header);
				if (originalCMVMessage != null)
				{
					foreach (JPAFRBills bill in header.Bills)
					{
						var newStatus = GetNewReleaseStatus(bill.JPB_ReleaseStatus);
						VesselInfoUpdater.UpdateBillReleaseStatus(logger, bill, newStatus);
					}
				}
			}
		}

		public static ZString GetNewReleaseStatus(ZString oldStatus)
		{
			var result = oldStatus;
			switch (oldStatus)
			{
				case AFRBillCustomsStatusList.Codes.Registered:
					result = AFRBillCustomsStatusList.Codes.NL3;
					break;
				case AFRBillCustomsStatusList.Codes.NL1:
					result = AFRBillCustomsStatusList.Codes.NL4;
					break;
				case AFRBillCustomsStatusList.Codes.NL2:
					result = AFRBillCustomsStatusList.Codes.NL5;
					break;
			}
			return result;
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.BlanketVesselChangeResponseReceived;
		}
	}
}
