using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLTransactionBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GLTransactionBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string FromPeriod = "FromPeriod";
			public const string ToPeriod = "ToPeriod";
			public const string FromDate = "FromDate";
			public const string ToDate = "ToDate";
			public const string StartGLAccountPK = "StartGLAccountPK";
			public const string EndGLAccountPK = "EndGLAccountPK";
			public const string BranchPK = "BranchPK";
			public const string DepartmentPK = "DepartmentPK";
			public const string DescriptionDisplay = "DescriptionDisplay";
			public const string SortByPeriod = "SortByPeriod";
			public const string SortBySource = "SortBySource";
			public const string CreateAndExportBatch = "CreateAndExportBatch";
			public const string ExportExistingBatch = "ExportExistingBatch";
			public const string BatchNumber = "BatchNumber";
			public const string ExportDirectory = "ExportDirectory";
			public const string Log = "Log"; // May be an identifier or GUID.
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (CreateAndExportBatch)
			{
				BatchNumber = ZInt.Parse(Env.NumberFountains.GenExportBatchSequenceBatchNo.GetTodaysPeriodFountain().GetNextFormatted(new BusinessObjectFactory()));
			}

			bool dataExportSucceeded = Exporter != null && Exporter.ExportData();

			if (!dataExportSucceeded)
			{
				if (CreateAndExportBatch)
				{
					BatchNumber = ZInt.Zero;
					throw new ZCannotSaveException("Export failed.", "GL Transaction Export");
				}
			}
		}

		#region Properties

		#region Bound Properties

		#region FromPeriod

		[ReadOnlyMember(nameof(FromPeriod_ReadOnly))]
		public ZInt FromPeriod
		{
			get { return fFromPeriod; }
			set
			{
				if (fFromPeriod != value)
				{
					SetNonPersistentPropertyValue(FromPeriodInfo, ref fFromPeriod, value);
					if (!IsValidationSuspended)
					{
						ValidatePeriodAndDateRanges();
					}
				}
			}
		}

		public ZPropertyInfo FromPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.FromPeriod); }
		}

		public ZBool FromPeriod_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		[ReadOnlyMember(nameof(ToPeriod_ReadOnly))]
		#region ToPeriod

		public ZInt ToPeriod
		{
			get { return fToPeriod; }
			set
			{
				if (fToPeriod != value)
				{
					SetNonPersistentPropertyValue(ToPeriodInfo, ref fToPeriod, value);
					if (!IsValidationSuspended)
					{
						ValidatePeriodAndDateRanges();
					}
				}
			}
		}

		public ZPropertyInfo ToPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.ToPeriod); }
		}

		public ZBool ToPeriod_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		#region FromDate

		[ReadOnlyMember(nameof(FromDate_ReadOnly))]
		public ZDateTime FromDate
		{
			get { return fFromDate; }
			set
			{
				if (fFromDate != value)
				{
					SetNonPersistentPropertyValue(FromDateInfo, ref fFromDate, value);
					if (!IsValidationSuspended)
					{
						ValidatePeriodAndDateRanges();
					}
				}
			}
		}

		public ZPropertyInfo FromDateInfo
		{
			get { return GetZPropertyInfo(Schema.FromDate); }
		}

		public ZBool FromDate_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		#region ToDate

		[ReadOnlyMember(nameof(ToDate_ReadOnly))]
		public ZDateTime ToDate
		{
			get { return fToDate; }
			set
			{
				if (fToDate != value)
				{
					SetNonPersistentPropertyValue(ToDateInfo, ref fToDate, value);
					if (!IsValidationSuspended)
					{
						ValidatePeriodAndDateRanges();
					}
				}
			}
		}

		public ZPropertyInfo ToDateInfo
		{
			get { return GetZPropertyInfo(Schema.ToDate); }
		}

		public ZBool ToDate_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		#region StartGLAccountPK

		[ReadOnlyMember(nameof(StartGLAccountPK_ReadOnly))]
		[List("GLAccountList")]
		public ZGuid StartGLAccountPK
		{
			get { return fStartGLAccountPK; }
			set
			{
				if (fStartGLAccountPK != value)
				{
					SetNonPersistentPropertyValue(StartGLAccountPKInfo, ref fStartGLAccountPK, value);
					if (!IsValidationSuspended)
					{
						ValidateStartGLAccountPK();
					}
				}
			}
		}

		public ZPropertyInfo StartGLAccountPKInfo
		{
			get { return GetZPropertyInfo(Schema.StartGLAccountPK); }
		}

		public ZBool StartGLAccountPK_ReadOnly
		{
			get { return ExportExistingBatch || CreateAndExportBatch; }
		}

		#endregion

		[ReadOnlyMember(nameof(EndGLAccountPK_ReadOnly))]
		#region EndGLAccountPK

		[List("GLAccountList")]
		public ZGuid EndGLAccountPK
		{
			get { return fEndGLAccountPK; }
			set
			{
				if (fEndGLAccountPK != value)
				{
					SetNonPersistentPropertyValue(EndGLAccountPKInfo, ref fEndGLAccountPK, value);
					if (!IsValidationSuspended)
					{
						ValidateEndGLAccountPK();
					}
				}
			}
		}

		public ZPropertyInfo EndGLAccountPKInfo
		{
			get { return GetZPropertyInfo(Schema.EndGLAccountPK); }
		}

		public ZBool EndGLAccountPK_ReadOnly
		{
			get { return ExportExistingBatch || CreateAndExportBatch; }
		}

		#endregion

		#region BranchPK

		[ReadOnlyMember(nameof(BranchPK_ReadOnly))]
		[List("BranchList")]
		public ZGuid BranchPK
		{
			get { return fBranchPK; }
			set
			{
				if (fBranchPK != value)
				{
					SetNonPersistentPropertyValue(BranchPKInfo, ref fBranchPK, value);
					if (!IsValidationSuspended)
					{
						ValidateBranchPK();
					}
				}
			}
		}

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPK); }
		}

		public ZBool BranchPK_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		#region DepartmentPK

		[ReadOnlyMember(nameof(DepartmentPK_ReadOnly))]
		[List("DepartmentList")]
		public ZGuid DepartmentPK
		{
			get { return fDepartmentPK; }
			set
			{
				if (fDepartmentPK != value)
				{
					SetNonPersistentPropertyValue(DepartmentPKInfo, ref fDepartmentPK, value);
					if (!IsValidationSuspended)
					{
						ValidateDepartmentPK();
					}
				}
			}
		}

		public ZPropertyInfo DepartmentPKInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentPK); }
		}

		public ZBool DepartmentPK_ReadOnly
		{
			get { return ExportExistingBatch; }
		}

		#endregion

		#region DescriptionDisplay

		[MaxLength(3)]
		[List("DescriptionDisplayList")]
		public ZString DescriptionDisplay
		{
			get { return fDescriptionDisplay; }
			set
			{
				if (fDescriptionDisplay != value)
				{
					SetNonPersistentPropertyValue(DescriptionDisplayInfo, ref fDescriptionDisplay, value);
					if (!IsValidationSuspended)
					{
						ValidateDescriptionDisplay();
					}
				}
			}
		}

		public ZPropertyInfo DescriptionDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.DescriptionDisplay); }
		}

		#endregion

		#region SortByPeriod

		public ZBool SortByPeriod
		{
			get { return fSortByPeriod; }
			set
			{
				if (fSortByPeriod != value)
				{
					SetNonPersistentPropertyValue(SortByPeriodInfo, ref fSortByPeriod, value);
					if (!IsValidationSuspended)
					{
						ValidateSortByPeriod();
					}
				}
			}
		}

		public ZPropertyInfo SortByPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.SortByPeriod); }
		}

		#endregion

		#region SortBySource

		public ZBool SortBySource
		{
			get { return fSortBySource; }
			set
			{
				if (fSortBySource != value)
				{
					SetNonPersistentPropertyValue(SortBySourceInfo, ref fSortBySource, value);
					if (!IsValidationSuspended)
					{
						ValidateSortBySource();
					}
				}
			}
		}

		public ZPropertyInfo SortBySourceInfo
		{
			get { return GetZPropertyInfo(Schema.SortBySource); }
		}

		#endregion

		#region CreateAndExportBatch

		public ZBool CreateAndExportBatch
		{
			get { return fCreateAndExportBatch; }
			set
			{
				bool newValueSet = SetNonPersistentPropertyValue(CreateAndExportBatchInfo, ref fCreateAndExportBatch, value);

				if (newValueSet)
				{
					StartGLAccountPK = ZGuid.Empty;
					EndGLAccountPK = ZGuid.Empty;

					if (!IsValidationSuspended)
					{
						if (fCreateAndExportBatch && ExportExistingBatch)
						{
							ExportExistingBatch = false;
						}
						ValidateCreateAndExportBatch();
					}
				}
			}
		}

		public ZPropertyInfo CreateAndExportBatchInfo
		{
			get { return GetZPropertyInfo(Schema.CreateAndExportBatch); }
		}

		#endregion

		#region ExportExistingBatch

		public ZBool ExportExistingBatch
		{
			get { return fExportExistingBatch; }
			set
			{
				bool newValueSet = SetNonPersistentPropertyValue(ExportExistingBatchInfo, ref fExportExistingBatch, value);
				if (newValueSet && !IsValidationSuspended)
				{
					if (fExportExistingBatch)
					{
						if (CreateAndExportBatch)
						{
							CreateAndExportBatch = false;
						}
						FromPeriod = 0;
						ToPeriod = 0;
						FromDate = ZDateTime.Empty;
						ToDate = ZDateTime.Empty;
						StartGLAccountPK = ZGuid.Empty;
						EndGLAccountPK = ZGuid.Empty;
						BranchPK = ZGuid.Empty;
						DepartmentPK = ZGuid.Empty;
					}
					ValidateExportExistingBatch();
				}
				BatchNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportExistingBatchInfo
		{
			get { return GetZPropertyInfo(Schema.ExportExistingBatch); }
		}

		#endregion

		#region BatchNumber

		public ZInt BatchNumber
		{
			get { return fBatchNumber; }
			set
			{
				bool newValueSet = SetNonPersistentPropertyValue(BatchNumberInfo, ref fBatchNumber, value);
				if (newValueSet && !IsValidationSuspended)
				{
					ValidateBatchNumber();
				}
			}
		}

		public ZPropertyInfo BatchNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BatchNumber); }
		}

		protected bool BatchNumber_ReadOnly
		{
			get { return !ExportExistingBatch || CreateAndExportBatch; }
		}

		#endregion

		#region ExportDirectory

		[MaxLength(1000)]
		public ZString ExportDirectory
		{
			get
			{
				return fExportDirectory;
			}
			set
			{
				if (fExportDirectory != value)
				{
					SetNonPersistentPropertyValue(ExportDirectoryInfo, ref fExportDirectory, value);
					if (!IsValidationSuspended)
					{
						ValidateExportDirectory();
					}
				}
			}
		}

		public ZPropertyInfo ExportDirectoryInfo
		{
			get { return GetZPropertyInfo(Schema.ExportDirectory); }
		}

		#endregion

		#region Log

		[MaxLength(10000)]
		[BusinessObjectTestExclude]
		public ZString Log
		{
			get
			{
				return fLog;
			}
		}

		public ZPropertyInfo LogInfo
		{
			get { return GetZPropertyInfo(Schema.Log); }
		}

		#endregion

		#endregion

		#region SortBy

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public ZString SortBy
		{
			get
			{
				return SortByPeriod ? "Period, GLAccount" : (SortBySource ? "Ledger, GLAccount" : "");
			}
		}

		#endregion

		public GLTransactionExporter Exporter
		{
			get;
			set;
		}

		#endregion

		#region Lists

		#region GLAccountList

		public AccGLHeaderCollection GLAccountList
		{
			get
			{
				ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_IsActive, ZBool.True);
				return new AccGLHeaderCollection(Factory, filter);
			}
		}

		#endregion

		#region BranchList

		public GlbBranchCollection BranchList
		{
			get
			{
				ZQuery filter = new ZQuery(GlbBranchSchema.GB_IsActive, ZBool.True.ToString());
				filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				return new GlbBranchCollection(Factory, filter);
			}
		}

		#endregion

		#region DepartmentList

		public GlbDepartmentCollection DepartmentList
		{
			get
			{
				ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True.ToString());
				return new GlbDepartmentCollection(Factory, filter);
			}
		}

		#endregion

		#region DescriptionDisplayList

		public CodeDescriptionPairList DescriptionDisplayList
		{
			get
			{
				if (fDescriptionDisplayList == null)
				{
					fDescriptionDisplayList = new CodeDescriptionPairList();
					fDescriptionDisplayList.AddPair("HDR", Res.GetString("f347b689-a9aa-43e2-a964-2fec1a1d291d", "Show Transaction Header Descriptions"));
					fDescriptionDisplayList.AddPair("LDR", Res.GetString("6a048658-2d92-467d-9d98-18baf218b4c8", "Show Transaction Lines Descriptions"));
				}
				return fDescriptionDisplayList;
			}
		}
		CodeDescriptionPairList fDescriptionDisplayList;

		#endregion

		#endregion

		#region Implementation

		ZInt fFromPeriod;
		ZInt fToPeriod;
		ZDateTime fFromDate;
		ZDateTime fToDate;
		ZGuid fStartGLAccountPK;
		ZGuid fEndGLAccountPK;
		ZGuid fBranchPK;
		ZGuid fDepartmentPK;
		ZString fDescriptionDisplay;
		ZBool fSortByPeriod;
		ZBool fSortBySource;
		ZBool fCreateAndExportBatch;
		ZBool fExportExistingBatch;
		ZInt fBatchNumber;
		ZString fExportDirectory;
		ZString fLog;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DescriptionDisplay = "HDR";
			SortByPeriod = true;
		}

		#region AddToLog

		public void AddToLog(string message)
		{
			fLog += message;
			if (fLog.Length >= LogInfo.MaxLength)
			{
				fLog = fLog.SubstringSafe(fLog.Length - (LogInfo.MaxLength * 9 / 10));
			}
			SetPropertyValue(LogInfo, fLog);
			this.Refresh();
		}

		#endregion

		bool NoMandatoryFilters
		{
			get { return CreateAndExportBatch || ExportExistingBatch; }
		}

		#region High Water Mark

		public bool HasRestrictiveFilters
		{
			get
			{
				return FromPeriod != 0 || ToPeriod != 0 ||
					!FromDate.IsEmpty || !ToDate.IsEmpty ||
					StartGLAccountPK.IsValid || EndGLAccountPK.IsValid ||
					BranchPK.IsValid || DepartmentPK.IsValid;
			}
		}

		#endregion

		#region Validations

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePeriodAndDateRanges();
			ValidateStartGLAccountPK();
			ValidateEndGLAccountPK();
			ValidateBranchPK();
			ValidateDepartmentPK();
			ValidateDescriptionDisplay();
			ValidateExportDirectory();
			ValidateCreateAndExportBatch();
			ValidateExportExistingBatch();
			ValidateBatchNumber();
		}

		protected void ValidatePeriodAndDateRanges()
		{
			FromPeriodInfo.ClearAllNotifications();
			ToPeriodInfo.ClearAllNotifications();
			FromDateInfo.ClearAllNotifications();
			ToDateInfo.ClearAllNotifications();
			if (!NoMandatoryFilters && (FromPeriod.IsEmpty || ToPeriod.IsEmpty) && FromDate.IsEmpty && ToDate.IsEmpty)
			{
				string errorMessage = Res.GetString("2be53733-c154-4767-a381-e99a7d568143", "At least one of the 'Period Range' and 'Date Range' should have data.");
				FromPeriodInfo.AddError(errorMessage);
				ToPeriodInfo.AddError(errorMessage);
				FromDateInfo.AddError(errorMessage);
				ToDateInfo.AddError(errorMessage);
			}
			else if (!FromDate.IsEmpty && !ToDate.IsEmpty && FromDate > ToDate)
			{
				FromDateInfo.AddError(Res.GetString("1aa3b01e-1729-44c4-b398-1b76e7b16a5c", "The 'From Date' cannot be after the 'To Date'."));
				ToDateInfo.AddError(Res.GetString("d9f3100f-101b-40a5-86de-7ab36f90b749", "The 'To Date' cannot be before the 'From Date'."));
			}
			else if (!FromDate.IsValid || !ToDate.IsValid)
			{
				if (!ToDate.IsEmpty && !ToDate.IsValid)
				{
					ToDateInfo.AddError((Res.GetString("67d9f950-6f3c-40f8-982c-445bdf9fd21d", "Enter a valid To Date")));
				}
				if (!FromDate.IsEmpty && !FromDate.IsValid)
				{
					FromDateInfo.AddError((Res.GetString("2051a8f6-d017-41d4-b663-ee60657fafcc", "Enter a valid From Date")));
				}
			}
		}

		protected void ValidateStartGLAccountPK()
		{
			StartGLAccountPKInfo.ClearAllNotifications();
			if (!NoMandatoryFilters && !StartGLAccountPK.IsValid)
			{
				StartGLAccountPKInfo.AddError(Res.GetString("ca7f65bb-b0cc-42b0-bac5-6e2c24dc397e", "Please enter a valid GL Account."));
			}
		}

		protected void ValidateEndGLAccountPK()
		{
			EndGLAccountPKInfo.ClearAllNotifications();
			if (!NoMandatoryFilters && !EndGLAccountPK.IsValid)
			{
				EndGLAccountPKInfo.AddError(Res.GetString("26fb7de8-03a3-4cd1-8989-5e961a33db02", "Please enter a valid GL Account."));
			}
		}

		protected void ValidateBranchPK()
		{
			BranchPKInfo.ClearAllNotifications();
			if (!BranchPK.IsEmpty && !BranchPK.IsValid)
			{
				BranchPKInfo.AddError(Res.GetString("ce786903-6292-4258-8130-32da4e667275", "Please enter a valid Branch."));
			}
		}

		protected void ValidateDepartmentPK()
		{
			DepartmentPKInfo.ClearAllNotifications();
			if (!DepartmentPK.IsEmpty && !DepartmentPK.IsValid)
			{
				DepartmentPKInfo.AddError(Res.GetString("f867ec5b-4a75-4588-a564-8d08633e7e9e", "Please enter a valid Department."));
			}
		}

		protected void ValidateDescriptionDisplay()
		{
			DescriptionDisplayInfo.ClearAllNotifications();
			if (!DescriptionDisplayList.ContainsCode(DescriptionDisplay))
			{
				DescriptionDisplayInfo.AddError(Res.GetString("3dfbaf6c-461c-4b52-861f-e8e797e33ddc", "Please enter a valid selection."));
			}
		}

		protected void ValidateSortByPeriod()
		{
			SortByPeriodInfo.ClearAllNotifications();
		}

		protected void ValidateSortBySource()
		{
			SortBySourceInfo.ClearAllNotifications();
		}

		protected void ValidateCreateAndExportBatch()
		{
			CreateAndExportBatchInfo.ClearAllNotifications();

			ValidatePeriodAndDateRanges();
			ValidateStartGLAccountPK();
			ValidateEndGLAccountPK();

			ValidateBatchNumber();
		}

		protected void ValidateExportExistingBatch()
		{
			ExportExistingBatchInfo.ClearAllNotifications();

			ValidatePeriodAndDateRanges();
			ValidateStartGLAccountPK();
			ValidateEndGLAccountPK();

			ValidateBatchNumber();
		}

		protected void ValidateBatchNumber()
		{
			BatchNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(BatchNumberInfo);
			if (!BatchNumberInfo.HasErrors() && !BatchNumberInfo.ReadOnly)
			{
				ZQuery exportBatchSequenceFilter = new ZQuery(GenExportBatchSequenceSchema.XB_BatchNumber, BatchNumber);
				ZQuery queryTypeFilter = new ZQuery(GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.GeneralLedgerPost);
				queryTypeFilter.AddToFilter(JoinCondition.Or, GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.GeneralLedgerReverse);
				queryTypeFilter.AddToFilter(JoinCondition.Or, GenExportBatchSequenceSchema.XB_Type, "");
				exportBatchSequenceFilter.AddToFilter(queryTypeFilter, JoinCondition.And);

				if (!Factory.ExistsInDatabase(GenExportBatchSequenceSchema.Constants.TableName, exportBatchSequenceFilter))
				{
					BatchNumberInfo.AddError(Res.GetString("43d7dfd4-e7bc-47a1-82c7-40e5048d56b8", "There is not a batch with this number. Please enter existent Batch Number."));
				}
			}
		}

		protected void ValidateExportDirectory()
		{
			ExportDirectoryInfo.ClearAllNotifications();
			if (ObjectFactory.Get<IFileMapper>().IsRemote)
			{
				if (!Directory.Exists(ObjectFactory.Get<IMappedClientPath>().GetMappedPath(ExportDirectory)))
				{
					ExportDirectoryInfo.AddError(Res.GetString("4EDD8BAC-7297-4BB9-9ACB-ECC16AA154B3", "Please only enter valid directory from a local or mapped drive."));
				}
			}
			else if (!Directory.Exists(ExportDirectory))
			{
				ExportDirectoryInfo.AddError(Res.GetString("5147b342-5b4e-4d1a-8238-2ded14f3efeb", "Please enter a valid directory."));
			}
		}

		#endregion

		#endregion
	}
}
