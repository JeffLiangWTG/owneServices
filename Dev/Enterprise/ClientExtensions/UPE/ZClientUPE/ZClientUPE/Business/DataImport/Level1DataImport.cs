using System;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1DataImport : NonPersistentBusinessObject, IObsoleteValidation
	{
		public Level1DataImport(BusinessObjectFactory factory, bool isImportToManifest = false)
			: base(factory)
		{
			IsImportToManifest = isImportToManifest;
		}
		public readonly bool IsImportToManifest;

		public static class Schema
		{
			public const string UnmatchedFilenameNote = "UnmatchedFilenameNote";
			public const string FlightNumber = "FlightNumber";
			public const string FlightNotInScheduleNote = "FlightNotInScheduleNote";
			public const string ArrivalDate = "ArrivalDate";
			public const string ArrivalDateWarningNote = "ArrivalDateWarningNote";
			public const string PortOfLoading = "PortOfLoading";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string ShippingAgentAddress = "ShippingAgentAddress";
			public const string MasterBill = "MasterBill";
			public const string MasterbillWarningNote = "MasterbillWarningNote";
			public const string CoLoadMasterBill = "CoLoadMasterBill";
			public const string IsSurplus = "IsSurplus";
			public const string SurplusIndicatedNote = "SurplusIndicatedNote";
			public const string DepartureDate = "DepartureDate";
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
			public const string IsRoad = "IsRoad";
			public const string DisableDecisionProvider = "DisableDecisionProvider";
		}

		public void ValidateAll()
		{
			ValidateArrivalDate();
			ValidateArrivalDateWarningNote();
			ValidateFlightNumber();
			ValidateFlightNotInScheduleNote();
			ValidateMasterBill();
			ValidateMasterBillWarningNote();
			ValidatePortOfDischarge();
			ValidatePortOfLoading();
			ValidateDepartureDate();
			ValidateCycleDate();
			ValidateCycleNumber();
			ValidateSurplusIndicatedNote();
			ValidateDuplicateHAWBsNote();
			ValidateUnmatchedFilenameNote();
		}

		public ZString FileName { get; set; }

		public int PecentageOfDuplicateHAWBs { get; set; }

		#region Duplicate HAWBs Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString DuplicateHAWBsNote
		{
			get { return fDuplicateHAWBsNote; }
			set
			{
				CheckMaximumLength(DuplicateHAWBsNoteInfo, value);
				fDuplicateHAWBsNote = value;
				DuplicateHAWBsNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateDuplicateHAWBsNote();
				}
			}
		}
		ZString fDuplicateHAWBsNote;

		public ZPropertyInfo DuplicateHAWBsNoteInfo
		{
			get { return GetZPropertyInfo(nameof(DuplicateHAWBsNote)); }
		}

		public void ValidateDuplicateHAWBsNote()
		{
			DuplicateHAWBsNoteInfo.ClearAllNotifications();
			if (!IsImportToManifest && PecentageOfDuplicateHAWBs > 15 && DuplicateHAWBsNote.IsEmpty)
			{
				DuplicateHAWBsNoteInfo.AddError("Please indicate why you are proceeding with the import when the number of duplicate HAWBs is greater than 15% of the total.");
			}
		}

		#endregion

		#region FileName Does not Match Port of Discharge Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString UnmatchedFilenameNote
		{
			get { return fUnmatchedFilenameNote; }
			set
			{
				CheckMaximumLength(UnmatchedFilenameNoteInfo, value);
				fUnmatchedFilenameNote = value;
				UnmatchedFilenameNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateUnmatchedFilenameNote();
				}
			}
		}
		ZString fUnmatchedFilenameNote;

		public ZPropertyInfo UnmatchedFilenameNoteInfo => GetZPropertyInfo(Schema.UnmatchedFilenameNote);

		public void ValidateUnmatchedFilenameNote()
		{
			UnmatchedFilenameNoteInfo.ClearAllNotifications();

			var shortFileName = Path.GetFileNameWithoutExtension(FileName);
			if (!FileName.IsEmpty && !IsFileNameValid(shortFileName) && UnmatchedFilenameNote.IsEmpty)
			{
				var errorMessage = string.Format("The Filename of the file that you are loading, '{0}' does not match the Port of Discharge, '{1}'", shortFileName, PortOfDischarge);
				UnmatchedFilenameNoteInfo.AddError(errorMessage);
			}
		}

		bool IsFileNameValid(ZString shortFileName)
		{
			var result = false;
			if (shortFileName.Length > 2)
			{
				var refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, shortFileName.SubstringSafe(2, 4), shortFileName.Left(2), UPEDataLine.Constants.RefLocoSystemUsage);
				if (refUNLOCO != null)
				{
					result = (refUNLOCO.Code == PortOfDischarge);
				}
			}

			return result;
		}

		#endregion

		#region Properties For Binding

		#region Flight Number

		[ReadOnlyMember(nameof(FlightNumber_ReadOnly))]
		[MaxLength(CusMAWB.Schema.CM_FlightNoMaxLength)]
		public ZString FlightNumber
		{
			get { return fFlightNumber; }
			set
			{
				CheckMaximumLength(FlightNumberInfo, value);
				SetNonPersistentPropertyValue(FlightNumberInfo, ref fFlightNumber, value);
				if (!IsRoad)
				{
					MasterBill = AirlinePrefix;
					TrySetOriginDestinationDefaults();
				}
				if (!IsValidationSuspended)
				{
					ValidateFlightNumber();
				}
			}
		}
		ZString fFlightNumber;

		public ZPropertyInfo FlightNumberInfo => GetZPropertyInfo(Schema.FlightNumber);

		bool FlightNumber_ReadOnly => IsRoad;

		public void ValidateFlightNumber()
		{
			FlightNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FlightNumberInfo);
		}

		#endregion

		#region Flight Not In Schedule Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString FlightNotInScheduleNote
		{
			get { return fFlightNotInScheduleNote; }
			set
			{
				CheckMaximumLength(FlightNotInScheduleNoteInfo, value);
				fFlightNotInScheduleNote = value;
				FlightNotInScheduleNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateFlightNotInScheduleNote();
				}
			}
		}
		ZString fFlightNotInScheduleNote;

		public ZPropertyInfo FlightNotInScheduleNoteInfo => GetZPropertyInfo(Schema.FlightNotInScheduleNote);

		public void ValidateFlightNotInScheduleNote()
		{
			FlightNotInScheduleNoteInfo.ClearAllNotifications();
			if (!IsRoad && !IsFlightInSchedule)
			{
				MandatoryValidation.CheckEntered(FlightNotInScheduleNoteInfo);
			}
		}

		bool IsFlightInSchedule
		{
			get { return ScheduledFlight != null; }
		}

		void TrySetOriginDestinationDefaults()
		{
			if (FlightNumber.IsValid && ArrivalDate.IsValid)
			{
				var scheduledFlight = ScheduledFlight;
				if (scheduledFlight != null)
				{
					if (scheduledFlight.Origins.Count == 1)
					{
						var loadingLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, scheduledFlight.Origins[0].JA_RL_NKPortOfLoading);
						if (loadingLOCO != null)
						{
							PortOfLoading = loadingLOCO.RL_Code;
						}
					}

					if (scheduledFlight.Destinations.Count == 1)
					{
						var dischargeLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, scheduledFlight.Destinations[0].JB_RL_NKPortOfDischarge);
						if (dischargeLOCO != null)
						{
							PortOfDischarge = dischargeLOCO.RL_Code;
						}
					}
				}
			}
		}

		JobVoyage ScheduledFlight
		{
			get
			{
				JobVoyage result = null;
				if (FlightNumber.IsValid && ArrivalDate.IsValid)
				{
					ZDBOnlyQuery scheduledFlightQuery = new ZDBOnlyQuery(typeof(JobVoyage));
					scheduledFlightQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, FlightNumber);

					ZDBOnlySubQuery arrivalLegQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);

					arrivalLegQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.GreaterThanOrEqualTo, GetValidSmallDateTime(ArrivalDate));
					arrivalLegQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.LessThanOrEqualTo, GetValidSmallDateTime(ArrivalDate.EndOfDay()));

					scheduledFlightQuery.AddSubQuery(arrivalLegQuery, JoinCondition.And);

					result = Factory.LoadTop1<JobVoyage>(scheduledFlightQuery);
				}

				return result;
			}
		}

		ZDateTime GetValidSmallDateTime(ZDateTime originalDate)
		{
			if (originalDate > ZDateTime.MaxSmallDateTimeValue)
			{
				originalDate = ZDateTime.MaxSmallDateTimeValue;
			}
			else if (originalDate < ZDateTime.MinSmallDateTimeValue)
			{
				originalDate = ZDateTime.MinSmallDateTimeValue;
			}
			return originalDate;
		}

		#endregion

		#region Arrival Date

		public ZDateTime ArrivalDate
		{
			get { return fArrivalDate; }
			set
			{
				fArrivalDate = value;
				ArrivalDateInfo.RefreshBinding();
				TrySetOriginDestinationDefaults();
				if (!IsValidationSuspended)
				{
					ValidateArrivalDate();
				}
			}
		}
		ZDateTime fArrivalDate;

		public ZPropertyInfo ArrivalDateInfo => GetZPropertyInfo(Schema.ArrivalDate);

		public void ValidateArrivalDate()
		{
			ArrivalDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ArrivalDateInfo);
			MandatoryValidation.CheckEntered(ArrivalDateInfo);
			if (ArrivalDate.IsValid)
			{
				if (ArrivalDate > ZDateTime.Now.AddDays(5) || ArrivalDate < ZDateTime.Now.AddDays(-5))
				{
					ArrivalDateInfo.AddWarning("Selected date is not within 5 days of today.");
				}
			}
		}

		#endregion

		#region Arrival Date Warning Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString ArrivalDateWarningNote
		{
			get { return fArrivalDateWarningNote; }
			set
			{
				CheckMaximumLength(ArrivalDateWarningNoteInfo, value);
				fArrivalDateWarningNote = value;
				ArrivalDateWarningNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateArrivalDateWarningNote();
				}
			}
		}
		ZString fArrivalDateWarningNote;

		public ZPropertyInfo ArrivalDateWarningNoteInfo => GetZPropertyInfo(Schema.ArrivalDateWarningNote);

		public void ValidateArrivalDateWarningNote()
		{
			ArrivalDateWarningNoteInfo.ClearAllNotifications();
			if (ArrivalDateInfo.HasWarnings() && ArrivalDateWarningNote.IsEmpty)
			{
				ArrivalDateWarningNoteInfo.AddError("Please indicate why you are importing when the arrival date has warnings.");
			}
		}

		#endregion

		#region Port Of Loading

		[List("PortList")]
		public ZString PortOfLoading
		{
			get { return fPortOfLoading; }
			set
			{
				if (PortOfLoading != value)
				{
					fPortOfLoading = value;
					DefaultDecisionSupportProvider();
					DefaultShippingAgentAddress();
					if (!IsValidationSuspended)
					{
						ValidatePortOfLoading();
					}
					PortOfLoadingInfo.RefreshBinding();
				}
			}
		}
		ZString fPortOfLoading;

		public ZPropertyInfo PortOfLoadingInfo => GetZPropertyInfo(Schema.PortOfLoading);

		public void ValidatePortOfLoading()
		{
			PortOfLoadingInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PortOfLoadingInfo);
			ListValidation.ErrorIfInvalidCode(PortOfLoadingInfo);
		}

		#endregion

		#region PortOfDischarge

		[List("PortList")]
		public ZString PortOfDischarge
		{
			get { return fPortOfDischarge; }
			set
			{
				if (PortOfDischarge != value)
				{
					fPortOfDischarge = value;
					DefaultDecisionSupportProvider();
					DefaultShippingAgentAddress();
					if (!IsValidationSuspended)
					{
						ValidatePortOfDischarge();
					}
					PortOfDischargeInfo.RefreshBinding();
				}
			}
		}
		ZString fPortOfDischarge;

		public ZPropertyInfo PortOfDischargeInfo => GetZPropertyInfo(Schema.PortOfDischarge);

		public void ValidatePortOfDischarge()
		{
			PortOfDischargeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PortOfDischargeInfo);
			ListValidation.ErrorIfInvalidCode(PortOfDischargeInfo);
			if (IsImportToManifest && !IsImport && !IsExport)
			{
				PortOfDischargeInfo.AddError(string.Format(CultureInfo.InvariantCulture,
						"Either the Port of Loading or Discharge must be a local port for {0}",
						Env.CurrentCompany.Country.Description));
			}
		}

		#endregion

		#region Carrier Agent

		[List("ShippingAgentList")]
		public ZGuid ShippingAgentAddress
		{
			get => shippingAgentAddress;
			set
			{
				if (ShippingAgentAddress != value)
				{
					shippingAgentAddress = value;
					if (!IsValidationSuspended)
					{
						ValidateShippingAgentAddress();
					}
				}
				ShippingAgentAddressInfo.RefreshBinding();
			}
		}
		ZGuid shippingAgentAddress;

		public ZPropertyInfo ShippingAgentAddressInfo => GetZPropertyInfo(Schema.ShippingAgentAddress);

		public void ValidateShippingAgentAddress()
		{
			ShippingAgentAddressInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(ShippingAgentAddressInfo);
		}

		public ZAddress ShippingAgentAddress_ZAddress
		{
			get
			{
				if (shippingAgentAddress_ZAddress == null)
				{
					shippingAgentAddress_ZAddress = new ZAddress(ShippingAgentAddressInfo)
					{
						DefaultAddressType = AddressType.OFC,
						OrgPKValidation = delegate(ZPropertyInfo info)
						{
							TypeValidation.CheckValidGuid(info);
						},
					};
				}

				return shippingAgentAddress_ZAddress;
			}
		}
		ZAddress shippingAgentAddress_ZAddress;

		#endregion

		#region MasterBill

		[MaxLength(11)]
		public ZString MasterBill
		{
			get { return fMasterBill; }
			set
			{
				ZString formattedValue = value.Replace(" ", "").Replace("-", "");
				CheckMaximumLength(MasterBillInfo, formattedValue);
				fMasterBill = formattedValue;
				MasterBillInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateMasterBill();
				}
			}
		}
		ZString fMasterBill;

		public ZPropertyInfo MasterBillInfo => GetZPropertyInfo(Schema.MasterBill);

		public void ValidateMasterBill()
		{
			MasterBillInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MasterBillInfo);

			if (!MasterBill.IsEmpty)
			{
				if (IsImportToManifest)
				{
					if (!IsRoad)
					{
						ZString warningMessage = AirWayBillValidator.GetWarningMessage(MasterBill);
						if (!warningMessage.IsEmpty)
						{
							MasterBillInfo.AddError(warningMessage);
						}
					}
					if (IsMasterbillDuplicateInManifest)
					{
						if (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill)
						{
							MasterBillInfo.AddWarning(MasterBillAlreadyInDatabase);
						}
						else
						{
							MasterBillInfo.AddError(MasterBillAlreadyInDatabase);
						}
					}
				}
				else
				{
					ZString warningMessage = AirWayBillValidator.GetWarningMessage(MasterBill);
					if (MasterBill.Left(3) != AirlinePrefix)
					{
						MasterBillInfo.AddError("The MAWB may be incorrect. The airline prefix and flight number do not match.");
					}
					if (!warningMessage.IsEmpty)
					{
						MasterBillInfo.AddError(warningMessage);
					}

					if (!FlightNumber.StartsWith("5X") && !IsSurplus)
					{
						if (IsMasterbillDuplicate)
						{
							MasterBillInfo.AddWarning(MasterBillAlreadyInDatabase);
						}
					}
				}
			}
		}

		bool IsMasterbillDuplicate => Factory.ExistsInDatabase(CusMAWBSchema.Constants.TableName, new ZQuery(CusMAWBSchema.CM_MAWB, MasterBill));

		bool IsMasterbillDuplicateInManifest
		{
			get
			{
				var masterbillFilter = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				masterbillFilter.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, true);

				var mawbRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
				if (mawbRecyclePeriod > 0)
				{
					masterbillFilter.AddToFilter(AsycudaManifestHeaderSchema.AMA_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mawbRecyclePeriod));
				}

				var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
				billSubQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, MasterBill);
				billSubQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
				billSubQuery.AddToFilter(AsycudaBillSchema.ABL_IsActive, true);
				masterbillFilter.AddSubQuery(billSubQuery, JoinCondition.And);

				return Factory.Exists(typeof(AsycudaManifestHeader), masterbillFilter);
			}
		}

		AirWayBillValidator AirWayBillValidator
		{
			get
			{
				if (fAirWayBillValidator == null)
				{
					fAirWayBillValidator = new AirWayBillValidator();
				}

				return fAirWayBillValidator;
			}
		}
		AirWayBillValidator fAirWayBillValidator;

		#endregion

		#region Masterbill Warning Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString MasterbillWarningNote
		{
			get { return fMasterbillWarningNote; }
			set
			{
				CheckMaximumLength(MasterbillWarningNoteInfo, value);
				fMasterbillWarningNote = value;
				MasterbillWarningNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateMasterBillWarningNote();
				}
			}
		}
		ZString fMasterbillWarningNote;

		public ZPropertyInfo MasterbillWarningNoteInfo => GetZPropertyInfo(Schema.MasterbillWarningNote);

		public void ValidateMasterBillWarningNote()
		{
			MasterbillWarningNoteInfo.ClearAllNotifications();
			if (!IsImportToManifest && MasterBillInfo.HasWarnings() && MasterbillWarningNote.IsEmpty)
			{
				MasterbillWarningNoteInfo.AddError("Please indicate why you are importing a Masterbill which has warnings.");
			}
		}

		#endregion

		#region Co-load Master

		[MaxLength(CusHAWB.Schema.CS_MasterHouseBillMaxLength)]
		public ZString CoLoadMasterBill
		{
			get { return fCoLoadMasterBill; }
			set
			{
				CheckMaximumLength(CoLoadMasterBillInfo, value);
				fCoLoadMasterBill = value;
				CoLoadMasterBillInfo.RefreshBinding();
			}
		}
		ZString fCoLoadMasterBill;

		public ZPropertyInfo CoLoadMasterBillInfo => GetZPropertyInfo(Schema.CoLoadMasterBill);

		#endregion

		#region Surplus

		public ZBool IsSurplus
		{
			get { return fIsSurplus; }
			set
			{
				SetNonPersistentPropertyValue(IsSurplusInfo, ref fIsSurplus, value);
				if (!IsValidationSuspended)
				{
					ValidateMasterBill();
				}
			}
		}
		ZBool fIsSurplus;

		public ZPropertyInfo IsSurplusInfo => GetZPropertyInfo(Schema.IsSurplus);

		#endregion

		#region Surplus Indicated Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString SurplusIndicatedNote
		{
			get { return fSurplusIndicatedNote; }
			set
			{
				CheckMaximumLength(SurplusIndicatedNoteInfo, value);
				fSurplusIndicatedNote = value;
				SurplusIndicatedNoteInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateSurplusIndicatedNote();
				}
			}
		}
		ZString fSurplusIndicatedNote;

		public ZPropertyInfo SurplusIndicatedNoteInfo => GetZPropertyInfo(Schema.SurplusIndicatedNote);

		public void ValidateSurplusIndicatedNote()
		{
			SurplusIndicatedNoteInfo.ClearAllNotifications();
			if (!IsImportToManifest && IsSurplus)
			{
				MandatoryValidation.CheckEntered(SurplusIndicatedNoteInfo);
			}
		}

		#endregion

		#region LoadSummaryInformation

		public ZString LoadSummaryInformation
		{
			get { return fLoadSummaryInformation; }
			set { fLoadSummaryInformation = value.Left(5000); }
		}
		ZString fLoadSummaryInformation;

		#endregion

		#region DepartureDate

		public ZDateTime DepartureDate
		{
			get => fDepartureDate;
			set
			{
				var oldValue = DepartureDate;
				if (SetNonPersistentPropertyValue(DepartureDateInfo, ref fDepartureDate, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateDepartureDate();
					}
				}
				DepartureDateInfo.RefreshBinding(oldValue);
			}
		}
		ZDateTime fDepartureDate;

		public ZPropertyInfo DepartureDateInfo => GetZPropertyInfo(Schema.DepartureDate);

		public void ValidateDepartureDate()
		{
			DepartureDateInfo.ClearAllNotifications();
			if (IsImportToManifest)
			{
				MandatoryValidation.CheckEntered(DepartureDateInfo);
			}
		}

		#endregion

		#region CycleDate

		public ZDateTime CycleDate
		{
			get => fCycleDate;
			set
			{
				var oldValue = CycleDate;
				if (SetNonPersistentPropertyValue(CycleDateInfo, ref fCycleDate, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateCycleDate();
					}
				}
				CycleDateInfo.RefreshBinding(oldValue);
			}
		}
		ZDateTime fCycleDate;

		public ZPropertyInfo CycleDateInfo => GetZPropertyInfo(Schema.CycleDate);

		public void ValidateCycleDate()
		{
			CycleDateInfo.ClearAllNotifications();
			if (IsImportToManifest)
			{
				if (IsImport)
				{
					TypeValidation.CheckValidZDateTimeAndRange(CycleDateInfo);
					MandatoryValidation.CheckEntered(CycleDateInfo);
				}
				else if (IsExport && !CycleDate.IsEmpty)
				{
					CycleDateInfo.AddWarning("Cycle Date is not required for an export shipment");
				}
			}
		}

		#endregion

		#region CycleNumber

		[MaxLength(10)]
		[List("CycleNumbers")]
		public ZString CycleNumber
		{
			get => fCycleNumber;
			set
			{
				var oldValue = CycleNumber;
				CheckMaximumLength(CycleNumberInfo, value);
				if (SetNonPersistentPropertyValue(CycleNumberInfo, ref fCycleNumber, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateCycleNumber();
					}
				}
				CycleNumberInfo.RefreshBinding(oldValue);
			}
		}
		ZString fCycleNumber;

		public ZPropertyInfo CycleNumberInfo => GetZPropertyInfo(Schema.CycleNumber);

		public CodeDescriptionPairList CycleNumbers => Factory.GetCycleNumbers();

		public void ValidateCycleNumber()
		{
			CycleNumberInfo.ClearAllNotifications();
			if (IsImportToManifest)
			{
				if (IsImport)
				{
					MandatoryValidation.CheckEntered(CycleNumberInfo);
					ListValidation.ErrorIfInvalidCode(CycleNumberInfo);
				}
				else if (IsExport && !CycleNumber.IsEmpty)
				{
					CycleNumberInfo.AddWarning("Cycle Number is not required for an export shipment");
				}
			}
		}

		#endregion

		#region Export Or Import

		bool IsImport => PortOfDischarge.StartsWith(Env.CurrentCompany.Country.Code, StringComparison.OrdinalIgnoreCase) && !PortOfLoading.IsEmpty && !PortOfLoading.StartsWith(Env.CurrentCompany.Country.Code, StringComparison.OrdinalIgnoreCase);

		public bool IsExport
		{
			get
			{
				if (!isExport.HasValue)
				{
					isExport = PortOfLoading.StartsWith(Env.CurrentCompany.Country.Code, StringComparison.OrdinalIgnoreCase) && !PortOfDischarge.IsEmpty && !PortOfDischarge.StartsWith(Env.CurrentCompany.Country.Code, StringComparison.OrdinalIgnoreCase);
				}
				return isExport.Value;
			}
		}
		bool? isExport;

		#endregion

		#region IsRoad
		[ResourceStringData("Level1DataImport.IsRoad", Caption = "Road?")]
		public ZBool IsRoad
		{
			get => fIsRoad;
			set
			{
				if (SetNonPersistentPropertyValue(IsRoadInfo, ref fIsRoad, value))
				{
					if (IsRoad && FlightNumber != RoadFlightNumber)
					{
						FlightNumber = RoadFlightNumber;
					}
				}
			}
		}
		ZBool fIsRoad;

		public ZPropertyInfo IsRoadInfo => GetZPropertyInfo(Schema.IsRoad);

		#endregion

		#region Disable Decision Provider

		[ReadOnlyMember(nameof(DisableDecisionProvider_ReadOnly))]
		public ZBool DisableDecisionProvider
		{
			get { return fDisableDecisionProvider; }
			set
			{
				fDisableDecisionProvider = value;
				DisableDecisionProviderInfo.RefreshBinding();
			}
		}
		ZBool fDisableDecisionProvider;

		public ZPropertyInfo DisableDecisionProviderInfo => GetZPropertyInfo(Schema.DisableDecisionProvider);

		public bool DisableDecisionProvider_ReadOnly =>
			(!UPEDataRegistry.Instance.EnableDecisionSupportImportShipments &&
			!UPEDataRegistry.Instance.EnableDecisionSupportExportShipments) ||
			(IsImport && !UPEDataRegistry.Instance.EnableDecisionSupportImportShipments) ||
			(IsExport && !UPEDataRegistry.Instance.EnableDecisionSupportExportShipments);

		#endregion

		#endregion

		public RefUNLOCOCollection PortList => portList ?? (portList = new RefUNLOCOCollection(Factory));
		RefUNLOCOCollection portList;

		public OrganisationsFindBoxCollection ShippingAgentList => shippingAgentList ?? (shippingAgentList = new OrganisationsFindBoxCollection(Factory));
		OrganisationsFindBoxCollection shippingAgentList;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DisableDecisionProvider = !UPEDataRegistry.Instance.EnableDecisionSupportExportShipments && !UPEDataRegistry.Instance.EnableDecisionSupportImportShipments;
		}

		void DefaultDecisionSupportProvider()
		{
			isExport = null;
			if (IsImportToManifest)
			{
				if (IsImport)
				{
					DisableDecisionProvider = !UPEDataRegistry.Instance.EnableDecisionSupportImportShipments;
				}
				else if (IsExport)
				{
					DisableDecisionProvider = !UPEDataRegistry.Instance.EnableDecisionSupportExportShipments;
				}
			}
		}

		void DefaultShippingAgentAddress()
		{
			if (IsImportToManifest && ShippingAgentAddress.IsEmpty)
			{
				if (IsImport)
				{
					ShippingAgentAddress = UPEDataRegistry.Instance.DefaultLevelOneImportCarrierAgentItem.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ShippingAgentAddress;
				}
				else if (IsExport)
				{
					ShippingAgentAddress = UPEDataRegistry.Instance.DefaultLevelOneExportCarrierAgentItem.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ShippingAgentAddress;
				}
			}
		}

		string AirlinePrefix => RefAirline.LoadFromAirline2LetterCode(Factory, FlightNumber.Left(2))?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? ZString.Empty;

		const string MasterBillAlreadyInDatabase = "This Masterbill number is already in the database.";
		const string RoadFlightNumber = "ROAD";
	}
}

