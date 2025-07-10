using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAirArrivalExporter : CMRDataExporterCSV
	{
		public CMRAirArrivalExporter(BusinessObject voyageWrapper, ZString messageType)
			: base(voyageWrapper)
		{
			this.messageType = messageType;
		}

		CustomsJobVoyageWrapper VoyageWrapper
		{
			get { return (CustomsJobVoyageWrapper)BizObj; }
		}

		readonly ZString messageType;

		public override string PartFileName
		{
			get
			{
				switch (messageType)
				{
					case "IA":
						return "IARAIR";
					case "AA":
						return "AARAIR";
					default:
						return "";
				}
			}
		}

		public override string MailSubject
		{
			get
			{
				switch (messageType)
				{
					case "IA":
						return "Contingency Impending Arrival Report - Air";
					case "AA":
						return "Contingency Actual Arrival Report - Air";
					default:
						return "";
				}
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return VoyageWrapper; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return VoyageWrapper; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					switch (messageType)
					{
						case "IA":
						case "AA":
							fAdditionalData.IsOriginPremiseReadOnly = true;
							break;
					}
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		protected override StringCollectionX[] Values
		{
			get
			{
				ArrayList list = new ArrayList();

				switch (messageType)
				{
					case "IA":
						ValuesForImpendingArrival(list);
						break;
					case "AA":
						ValuesForActualArrival(list);
						break;
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		void ValuesForImpendingArrival(ArrayList list)
		{
			foreach (CustomsVoyageDestinationWrapper destination in VoyageWrapper.Destinations)
			{
				StringCollectionX result = new StringCollectionX();
				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				result.Add(ZString.Empty);// TODO:  aircraft rego ?
				result.Add(VoyageWrapper.FlightNo);
				result.Add(VoyageWrapper.LastOverseasPortOfDeparture);
				result.Add(CMRDataExporterCSV.CMRDateString(VoyageWrapper.DateTimeOfDepartureUTC));
				result.Add(CMRDataExporterCSV.CMRTimeString(VoyageWrapper.DateTimeOfDepartureUTC));
				result.Add(VoyageWrapper.PortOfFirstArrival);
				result.Add(destination.PortOfArrival);
				result.Add(CMRDataExporterCSV.CMRDateString(destination.EstimatedDateTimeOfArrivalUTC));
				result.Add(CMRDataExporterCSV.CMRTimeString(destination.EstimatedDateTimeOfArrivalUTC));
				result.Add(destination.DischargeCTOEstablishmentID);
				result.Add(destination.DischargeIndicator ? "YES" : "");
				result.Add(destination.Destination.JB_SendersMessageReference);
				list.Add(result);
			}
		}

		void ValuesForActualArrival(ArrayList list)
		{
			foreach (CustomsVoyageDestinationWrapper destination in VoyageWrapper.Destinations)
			{
				if (destination.ActualArrivalDateTimeUTC.IsValid && destination.ActualArrivalStatus.Code == CMRBaseStatuses.Codes.NotSent)
				{
					StringCollectionX result = new StringCollectionX();
					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(VoyageWrapper.FlightNo);
					result.Add(VoyageWrapper.LastOverseasPortOfDeparture);
					result.Add(CMRDataExporterCSV.CMRDateString(destination.ActualArrivalDateTimeUTC));
					result.Add(CMRDataExporterCSV.CMRTimeString(destination.ActualArrivalDateTimeUTC));
					result.Add(destination.PortOfArrival);
					result.Add(destination.Destination.JB_SendersMessageReference);
					list.Add(result);
				}
			}
		}
	}
}
