using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public partial class EventInfo
	{
		public EventInfo(IXmlEventValueObject eventDataObject)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, nameof(eventDataObject));
		}

		public ZString MasterBillNumber => masterBillNumber ?? (masterBillNumber = VesselDetailContext.MBOLNumber.GetValueOrDefault());
		string masterBillNumber;

		public ZString CarrierCode => carrierCode ?? (carrierCode = (VesselDetailContext.CarrierCode ?? ZString.Empty));
		string carrierCode;

		public ZString VesselCallSign => vesselCallSign ?? (vesselCallSign = VesselDetailContext.VesselCallSign);
		string vesselCallSign;

		public ZString VoyageNumber => voyageNumber ?? (voyageNumber = VesselDetailContext.VoyageNumber ?? ZString.Empty);
		string voyageNumber;

		public ZString LoadingPortCode => loadingPortCode ?? (loadingPortCode = VesselDetailContext.PortOfLoadingUNLOCO ?? ZString.Empty);
		string loadingPortCode;

		public ZString LoadingPortSuffix => loadingPortSuffix ?? (loadingPortSuffix = VesselDetailContext.PortOfLoadingSuffix ?? ZString.Empty);
		string loadingPortSuffix;

		List<Context> contextCollection;
		protected IEnumerable<Context> ContextCollection => contextCollection ?? (contextCollection = (eventDataObject as Event)?.ContextCollection ?? new List<Context>());

		Context[] billInfoContexts;
		protected IEnumerable<Context> BillInfoContexts => billInfoContexts ?? (billInfoContexts = ContextCollection.Where(x => x.IsMatch(Constants.BillInformation)).ToArray());

		IXmlEventValueObjectContextValueList vesselDetailContext;
		protected IXmlEventValueObjectContextValueList VesselDetailContext => vesselDetailContext ?? (vesselDetailContext = eventDataObject.Context);

		readonly IXmlEventValueObject eventDataObject;
	}
}
