using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DHL.Business
{
	public class FlightBulkUpdateBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FlightBulkUpdateBusinessObject()
			: base(new BusinessObjectFactory())
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ExistingMAWB = "ExistingMAWB";
			public const string MAWB = "MAWB";
			public const string FlightNo = "FlightNo";
			public const string ArrivalDate = "ArrivalDate";
			public const string DepartureDate = "DepartureDate";
			public const string EDITransmitDate = "EDITransmitDate";
		}

		#endregion

		#region Properties

		#region ExistingMAWB

		[MaxLength(JobDeclaration.Schema.JE_MasterBillMaxLength)]
		public ZString ExistingMAWB
		{
			get { return existingMAWB; }
			set
			{
				CheckMaximumLength(ExistingMAWBInfo, value);
				SetNonPersistentPropertyValue<ZString>(ExistingMAWBInfo, ref existingMAWB, value.Replace("-", "").Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					ValidateExistingMAWB();
				}
				ExistingMAWBInfo.RefreshBinding();
			}
		}
		ZString existingMAWB;

		public ZPropertyInfo ExistingMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.ExistingMAWB); }
		}

		void ValidateExistingMAWB()
		{
			ExistingMAWBInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ExistingMAWBInfo);
			ZString stdWarnings = MasterBillValidator.GetMAWBFormatValidMessage(existingMAWB, Factory);
			if (!stdWarnings.IsEmpty)
			{
				ExistingMAWBInfo.AddWarning(stdWarnings);
			}
			if (!ExistingMAWBInfo.HasErrors() && !HasJobDeclaration)
			{
				ExistingMAWBInfo.AddError(ErrorNoExistingMAWB);
			}
		}

		bool HasJobDeclaration
		{
			get
			{
				JobDeclaration jobDec = Factory.LoadTop1<JobDeclaration>(StandaloneJobDecFilter(ExistingMAWB));
				return jobDec != null;
			}
		}

		#endregion

		#region MAWB

		[MaxLength(JobDeclaration.Schema.JE_MasterBillMaxLength)]
		public ZString MAWB
		{
			get { return mAWB; }
			set
			{
				CheckMaximumLength(MAWBInfo, value);
				SetNonPersistentPropertyValue<ZString>(MAWBInfo, ref mAWB, value.Replace("-", "").Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					ValidateMAWB();
				}
				MAWBInfo.RefreshBinding();
			}
		}
		ZString mAWB;

		public ZPropertyInfo MAWBInfo
		{
			get { return GetZPropertyInfo(Schema.MAWB); }
		}

		void ValidateMAWB()
		{
			MAWBInfo.ClearAllNotifications();
			ZString stdWarnings;

			if (!MAWB.IsEmpty)
			{
				stdWarnings = MasterBillValidator.GetMAWBFormatValidMessage(MAWB, Factory);
				if (!stdWarnings.IsEmpty)
				{
					MAWBInfo.AddWarning(stdWarnings);
				}
			}
		}

		#endregion

		#region Flight No

		[MaxLength(JobDeclaration.Schema.JE_VoyageFlightNoMaxLength)]
		public ZString FlightNo
		{
			get { return flightNo; }
			set
			{
				CheckMaximumLength(FlightNoInfo, value);
				SetNonPersistentPropertyValue<ZString>(FlightNoInfo, ref flightNo, value);
				FlightNoInfo.RefreshBinding();
			}
		}
		ZString flightNo;

		public ZPropertyInfo FlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.FlightNo); }
		}

		#endregion

		#region ArrivalDate

		public ZDateTime ArrivalDate
		{
			get { return arrivalDate; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(ArrivalDateInfo, ref arrivalDate, value);
				if (!IsValidationSuspended)
				{
					ValidateArrivalAndDepartureDates();
				}
				ArrivalDateInfo.RefreshBinding();
			}
		}
		ZDateTime arrivalDate;

		public ZPropertyInfo ArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalDate); }
		}

		#endregion

		#region DepartureDate

		public ZDateTime DepartureDate
		{
			get { return departureDate; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(DepartureDateInfo, ref departureDate, value);
				if (!IsValidationSuspended)
				{
					ValidateArrivalAndDepartureDates();
				}
				DepartureDateInfo.RefreshBinding();
			}
		}
		ZDateTime departureDate;

		public ZPropertyInfo DepartureDateInfo
		{
			get { return GetZPropertyInfo(Schema.DepartureDate); }
		}

		#endregion

		#region EDITransmitDate

		public ZDateTime EDITransmitDate
		{
			get { return ediTransmitDate; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(EDITransmitDateInfo, ref ediTransmitDate, value.Date);
				if (!IsValidationSuspended)
				{
					ValidateEDITransmitDate();
				}
				EDITransmitDateInfo.RefreshBinding();
			}
		}
		ZDateTime ediTransmitDate;

		public ZPropertyInfo EDITransmitDateInfo
		{
			get { return GetZPropertyInfo(Schema.EDITransmitDate); }
		}

		void ValidateEDITransmitDate()
		{
			EDITransmitDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(EDITransmitDateInfo);
			if (EDITransmitDate < ZDateTime.Now.Date)
			{
				EDITransmitDateInfo.AddError(ErrorDateBeforeToday);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateArrivalAndDepartureDates();
			ValidateEDITransmitDate();
			ValidateExistingMAWB();
			ValidateMAWB();
		}

		void ValidateArrivalAndDepartureDates()
		{
			ArrivalDateInfo.ClearAllNotifications();
			DepartureDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ArrivalDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(DepartureDateInfo);
			if (!ArrivalDateInfo.HasErrors() && !DepartureDateInfo.HasErrors() && (ArrivalDate < DepartureDate))
			{
				ArrivalDateInfo.AddError(ErrorDateArriveBeforeDepart);
				DepartureDateInfo.AddError(ErrorDateDepartAfterArrive);
			}
		}

		public bool HasMinimumRequirements
		{
			get
			{
				return !(ExistingMAWB.IsEmpty ||
						  (MAWB.IsEmpty &&
						  FlightNo.IsEmpty &&
						  ArrivalDate.IsEmpty &&
						  DepartureDate.IsEmpty &&
						  EDITransmitDate.IsEmpty));
			}
		}

		#endregion

		internal ZQuery StandaloneJobDecFilter(ZString masterBill)
		{
			ZQuery filter = new ZQuery(JobDeclarationSchema.JE_EntryStatus, LowValueConsignmentStatusList.Codes.NotSentToCustoms);
			filter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_EntryStatus, LowValueConsignmentStatusList.Codes.ReadyForManifesting);
			filter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_EntryStatus, LowValueConsignmentStatusList.Codes.ManifestedReadyToSend);
			filter = new ZQuery(filter);
			filter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MasterBill, masterBill);
			filter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_TransportMode, Enterprise.Core.Constants.TransportModes.Air);
			filter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_JS, null);

			List<ZGuid> branches = new List<ZGuid>();
			foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
			{
				branches.Add(branch.PK);
			}
			filter.AddToFilter(JobDeclarationSchema.JE_GB, branches);
			return filter;
		}

		const string ErrorDateArriveBeforeDepart = "Arrival date must occur after departure date.";
		const string ErrorDateDepartAfterArrive = "Departure date must occur before arrival date.";
		const string ErrorDateBeforeToday = "EDI date must occur today or later.";
		const string ErrorNoExistingMAWB = "There was no existing standalone, air freight declaration with the Existing MAWB found to update.";
	}
}
