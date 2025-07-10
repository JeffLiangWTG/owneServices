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
	public class CustomsVoyageDestinationWrapper : NonPersistentBusinessObject, IAirActualArrivalReportInformation, IImpendingArrivalReportLineInformation, IStatusNeedsRecalculationProvider, ICMRMessageRespondee
	{
		#region Schema

		public abstract class Schema
		{
			public const string PortOfArrival = "PortOfArrival";
			public const string EstimatedDateTimeOfArrivalUTC = "EstimatedDateTimeOfArrivalUTC";
			public const string ActualArrivalDateTimeUTC = "ActualArrivalDateTimeUTC";
			public const string DischargeCTOEstablishmentID = "DischargeCTOEstablishmentID";
			public const string StevedoreID = "StevedoreID";
		}

		#endregion

		public CustomsVoyageDestinationWrapper(CustomsJobVoyageWrapper voyageWrapper, VoyageDestination destination)
			: base(voyageWrapper.Factory)
		{
			this.voyageWrapper = voyageWrapper;
			this.Destination = destination;
			Calculator = new VoyageDestinationActualArrivalStatusCalculator(this);
		}
		public readonly VoyageDestinationActualArrivalStatusCalculator Calculator;

		public static CustomsVoyageDestinationWrapper Load(BusinessObjectFactory factory, ZGuid destinationPK)
		{
			var destination = factory.Load<VoyageDestination>(destinationPK);
			return destination != null ? new CustomsVoyageDestinationWrapper(new CustomsJobVoyageWrapper(destination.Voyage), destination) : null;
		}

		#region Validation

		public CustomsVoyageDestinationWrapperValidation Validation
		{
			get { return new CustomsVoyageDestinationWrapperValidation(this); }
		}

		#endregion

		#region Properties

		#region EstimatedDateTimeOfArrival

		public ZDateTime EstimatedDateTimeOfArrivalUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				RefUNLOCO discharge = Destination.PortOfDischarge;
				if (discharge != null && !Destination.JB_E_ARV.IsEmpty && Destination.JB_E_ARV.IsValid)
				{
					result = discharge.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(Destination.JB_E_ARV.ToDateTime());
				}
				return result;
			}
		}

		public ZPropertyInfo EstimatedDateTimeOfArrivalUTCInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedDateTimeOfArrivalUTC); }
		}

		#endregion

		#region ActualArrivalDateTime

		public ZDateTime ActualArrivalDateTimeUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				RefUNLOCO discharge = Destination.PortOfDischarge;
				if (discharge != null && !Destination.JB_A_ARV.IsEmpty && Destination.JB_A_ARV.IsValid)
				{
					result = discharge.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(Destination.JB_A_ARV.ToDateTime());
				}
				return result;
			}
		}

		public ZPropertyInfo ActualArrivalDateTimeUTCInfo
		{
			get { return GetZPropertyInfo(Schema.ActualArrivalDateTimeUTC); }
		}

		#endregion

		#region PortOfArrival

		[CargoWise.ComponentModel.MaxLength(5)]
		public ZString PortOfArrival
		{
			get { return Destination.JB_RL_NKPortOfDischarge; }
		}

		public ZDateTime EstimatedArrivalDate
		{
			get { return Destination.JB_E_ARV; }
		}

		public ZPropertyInfo PortOfArrivalInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfArrival); }
		}

		#endregion

		public ZString ResponsiblePartyID
		{
			get { return GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number; }
		}

		#endregion

		#region IAirActualArrivalReportInformation Members

		#region FlightNo

		public ZString FlightNo
		{
			get { return voyageWrapper.FlightNo; }
		}

		public ZPropertyInfo FlightNoInfo
		{
			get { return voyageWrapper == null ? null : voyageWrapper.FlightNoInfo; }
		}

		#endregion

		#region DateTimeOfDeparture

		public ZDateTime DateTimeOfDepartureUTC
		{
			get { return voyageWrapper.DateTimeOfDepartureUTC; }
		}

		public ZPropertyInfo DateTimeOfDepartureInfo
		{
			get { return voyageWrapper == null ? null : voyageWrapper.DateTimeOfDepartureUTCInfo; }
		}

		#endregion

		#region LastOverseasPortOfDeparture

		public ZString LastOverseasPortOfDeparture
		{
			get { return voyageWrapper.LastOverseasPortOfDeparture; }
		}

		public ZPropertyInfo LastOverseasPortOfDepartureInfo
		{
			get { return voyageWrapper == null ? null : voyageWrapper.LastOverseasPortOfDepartureInfo; }
		}

		#endregion

		#endregion

		#region IImpendingArrivalReportLineInformation members

		public ZDateTime EstimatedDateOfArrival
		{
			get { return Destination.JB_E_ARV; }
		}

		#region DischargeCTOEstablishmentID

		public ZString DischargeCTOEstablishmentID
		{
			get
			{
				OrgAddress cTOAddress = Destination.ArrivalCTOAddress;
				return cTOAddress != null ? cTOAddress.LocalControlledPremisesID : ZString.Empty;
			}
		}

		public ZPropertyInfo DischargeCTOEstablishmentIDInfo
		{
			get { return GetZPropertyInfo(Schema.DischargeCTOEstablishmentID); }
		}

		#endregion

		public ZString StevedoreID
		{
			get { return ZString.Empty; }
		}

		public bool DischargeIndicator
		{
			get { return !DischargeCTOEstablishmentID.IsEmpty; }
		}

		#endregion

		#region Related Business Objects

		CusEntryNumStatus actualArrivalStatus;
		public CusEntryNumStatus ActualArrivalStatus
		{
			get
			{
				if (actualArrivalStatus == null)
				{
					actualArrivalStatus = new CusEntryNumStatus(Destination, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.ActualArrivalResponseStatus, Core.Constants.CountryCodes.Australia);
				}
				return actualArrivalStatus;
			}
		}

		public EDIMessageCollection Messages
		{
			get { return Destination == null ? DummyMessageCollection : Destination.Messages; }
		}

		EDIMessageCollection dummyMessageCollection;
		EDIMessageCollection DummyMessageCollection
		{
			get
			{
				if (dummyMessageCollection == null)
				{
					dummyMessageCollection = new EDIMessageCollection(null, Factory);
				}
				return dummyMessageCollection;
			}
		}

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
			get
			{
				return ((ICMRMessageRespondee)voyageWrapper).Details + "Port: " + this.PortOfArrival + "\r\n";
			}
		}

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return ((ICMRMessageRespondee)voyageWrapper).ShortDescription + " Port: " + this.PortOfArrival; }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public readonly VoyageDestination Destination;
		readonly CustomsJobVoyageWrapper voyageWrapper;
	}
}
