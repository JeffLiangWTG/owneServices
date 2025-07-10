using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsJobVoyageWrapper : NonPersistentBusinessObject
		, IAirImpendingArrivalReportInformation
		, IStatusNeedsRecalculationProvider
		, ICMRMessageRespondee
		, IMessageManageableBizObj
		, IDataExportCSVFileNameProvider
		, IDocManagerSupport
	{
		public CustomsJobVoyageWrapper(JobVoyage voyage)
			: base(voyage.Factory)
		{
			this.Voyage = voyage;
			Calculator = new JobVoyageImpendingArrivalStatusCalculator(this);
		}
		public readonly JobVoyageImpendingArrivalStatusCalculator Calculator;

		public static CustomsJobVoyageWrapper Load(BusinessObjectFactory factory, ZGuid voyagePK)
		{
			var voyage = factory.Load<JobVoyage>(voyagePK);
			return voyage != null ? new CustomsJobVoyageWrapper(voyage) : null;
		}

		#region Schema

		public abstract class Schema
		{
			public const string LastOverseasPortOfDeparture = "LastOverseasPortOfDeparture";
			public const string PortOfFirstArrival = "PortOfFirstArrival";
			public const string DateTimeOfDepartureUTC = "DateTimeOfDepartureUTC";
			public const string FlightNo = "FlightNo";
			public const string EstimatedDateTimeOfArrival = "EstimatedDateTimeOfArrival";
		}

		#endregion

		#region Properties

		#region FlightNo

		public ZString FlightNo
		{
			get { return Voyage.JV_VoyageFlight; }
		}

		public ZPropertyInfo FlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.FlightNo); }
		}

		#endregion

		#region PortOfFirstArrival

		public ZString PortOfFirstArrival
		{
			get
			{
				VoyageDestination firstLocalDestination = this.FirstLocalDestination;
				return firstLocalDestination != null ? firstLocalDestination.JB_RL_NKPortOfDischarge : ZString.Empty;
			}
		}

		public ZPropertyInfo PortOfFirstArrivalInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfFirstArrival); }
		}

		#endregion

		#region DateTimeOfDeparture

		public ZDateTime DateTimeOfDepartureUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				VoyageOrigin lastOverseasOrigin = this.LastOverseasOrigin;
				if (lastOverseasOrigin != null)
				{
					RefUNLOCO loading = lastOverseasOrigin.PortOfLoading;
					if (loading != null && !lastOverseasOrigin.JA_A_DEP.IsEmpty && lastOverseasOrigin.JA_A_DEP.IsValid)
					{
						result = loading.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(lastOverseasOrigin.JA_A_DEP.ToDateTime());
					}
				}
				return result;
			}
		}

		public ZPropertyInfo DateTimeOfDepartureUTCInfo
		{
			get { return GetZPropertyInfo(Schema.DateTimeOfDepartureUTC); }
		}

		#endregion

		#region LastOverseasPortOfDeparture

		public ZString LastOverseasPortOfDeparture
		{
			get
			{
				VoyageOrigin lastOverseasOrigin = this.LastOverseasOrigin;
				return lastOverseasOrigin != null ? lastOverseasOrigin.JA_RL_NKPortOfLoading : ZString.Empty;
			}
		}

		public ZPropertyInfo LastOverseasPortOfDepartureInfo
		{
			get { return GetZPropertyInfo(Schema.LastOverseasPortOfDeparture); }
		}

		#endregion

		public ZString ResponsiblePartyID
		{
			get { return GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Replace(" ", ""); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CustomsJobVoyageWrapperValidation Validation
		{
			get { return new CustomsJobVoyageWrapperValidation(this); }
		}

		#endregion

		#region IAirImpendingArrivalReportInformation Members

		public IImpendingArrivalReportLineInformation[] Lines
		{
			get
			{
				return GetLines(this);
			}
		}

		public IImpendingArrivalReportLineInformation[] DatabaseLines
		{
			get
			{
				JobVoyage dBJobVoyage = new BusinessObjectFactory().Load<JobVoyage>(Voyage.PK);
				return GetLines(dBJobVoyage == null ? null : new CustomsJobVoyageWrapper(dBJobVoyage));
			}
		}

		IImpendingArrivalReportLineInformation[] GetLines(CustomsJobVoyageWrapper voyageWrapper)
		{
			ArrayList result = new ArrayList();
			if (voyageWrapper != null)
			{
				foreach (CustomsVoyageDestinationWrapper destinationWrapper in voyageWrapper.Destinations)
				{
					if (destinationWrapper.PortOfArrival.StartsWith(Core.Constants.CountryCodes.Australia))
					{
						result.Add(destinationWrapper);
					}
				}
			}
			return (IImpendingArrivalReportLineInformation[])result.ToArray(typeof(IImpendingArrivalReportLineInformation));
		}

		#endregion

		#region Related Business Objects

		#region LastOverseasOrigin

		public VoyageOrigin LastOverseasOrigin
		{
			get
			{
				VoyageOrigin result = null;
				foreach (VoyageOrigin origin in Voyage.Origins)
				{
					if (!origin.JA_RL_NKPortOfLoading.IsEmpty
						&& !origin.JA_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.Australia)
						&& (result == null || origin.JA_A_DEP > result.JA_A_DEP))
					{
						result = origin;
					}
				}
				return result;
			}
		}

		#endregion

		#region FirstLocalDestination

		public VoyageDestination FirstLocalDestination
		{
			get
			{
				VoyageDestination result = null;
				foreach (VoyageDestination destination in Voyage.Destinations)
				{
					if (!destination.JB_RL_NKPortOfDischarge.IsEmpty
						&& destination.JB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Australia)
						&& (result == null || destination.JB_E_ARV < result.JB_E_ARV))
					{
						result = destination;
					}
				}
				return result;
			}
		}

		#endregion

		#region Destinations

		CustomsVoyageDestinationWrapperCollection destinations;
		public CustomsVoyageDestinationWrapperCollection Destinations
		{
			get
			{
				if (destinations == null)
				{
					destinations = new CustomsVoyageDestinationWrapperCollection(this);
					RegisterEditableChildObject(destinations);
				}
				return destinations;
			}
		}

		#endregion

		#region ImpendingArrivalStatus

		CusEntryNumStatus impendingArrivalStatus;
		public CusEntryNumStatus ImpendingArrivalStatus
		{
			get
			{
				if (impendingArrivalStatus == null)
				{
					impendingArrivalStatus = new CusEntryNumStatus(Voyage, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.ImpendingArrivalResponseStatus, Core.Constants.CountryCodes.Australia);
				}
				return impendingArrivalStatus;
			}
		}

		#endregion

		#region Messages

		public EDIMessageCollection Messages
		{
			get { return Voyage.Messages; }
		}

		#endregion

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get { return Messages.HasChanges; }
		}

		#endregion

		#region ICMRMessageRespondee Members

		ZString ICMRMessageRespondee.Details
		{
			get { return "Flight: " + FlightNo + "\r\n"; }
		}

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return "Flight: " + FlightNo; }
		}

		#endregion

		public readonly JobVoyage Voyage;

		#region IMessageManageableBizObj Members

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new JobVoyageMessageManager(this);
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(FlightNo);
				if (LastOverseasOrigin != null)
				{
					builder.Append(LastOverseasOrigin.JA_RL_NKPortOfLoading);
					var depTime = LastOverseasOrigin.JA_A_DEP;
					builder.Append(depTime.IsValid ? (ZString)depTime.ToString("yyMMdd") : ZString.Empty);
				}
				if (FirstLocalDestination != null)
				{
					builder.Append(FirstLocalDestination.JB_RL_NKPortOfDischarge);
					var arvTime = FirstLocalDestination.JB_E_ARV;
					builder.Append(arvTime.IsValid ? (ZString)arvTime.ToString("yyMMdd") : ZString.Empty);
				}
				return builder.ToStringWithDelimiterBetweenAppends("_");
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return ((IDocManagerSupport)Voyage).DocManagerInfo; }
		}

		#endregion
	}
}
