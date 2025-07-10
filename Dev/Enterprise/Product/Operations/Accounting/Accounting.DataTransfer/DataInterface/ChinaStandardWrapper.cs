using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class ChinaStandardWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChinaStandardWrapper() : this(new BusinessObjectFactory())
		{
		}

		public ChinaStandardWrapper(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string Branch = "Branch";
			public const string Period = "Period";
			public const string ExportDirectory = "ExportDirectory";
			public const string Log = "Log";
			public const string OLdChartType = "OLdChartType";
			public const string NewChartType = "NewChartType";
		}

		#endregion

		public ChinaStandardExporter Exporter { get; set; }

		#region Public Properties

		#region Path

		ZString fExportDirectory;

		[BusinessObjectTestExclude]
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

		#region Branch

		[List("Branches")]
		public virtual ZGuid Branch
		{
			get { return fBranch; }
			set
			{
				if (fBranch != value)
				{
					SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
					if (!IsValidationSuspended)
					{
						ValidateBranch();
					}
				}
			}
		}

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(Schema.Branch); }
		}

		public virtual ZString BranchCode
		{
			get
			{
				var selectedBranch = Factory.Load<GlbBranch>(Branch);
				return selectedBranch == null ? ZString.Empty : selectedBranch.GB_Code;
			}
		}

		#endregion

		#region ChartType

		public virtual ZBool OLdChartType
		{
			get { return fOLdChartType; }
			set
			{
				if (fOLdChartType != value)
				{
					SetNonPersistentPropertyValue(OLdChartTypeInfo, ref fOLdChartType, value);
				}
			}
		}

		public ZPropertyInfo OLdChartTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OLdChartType); }
		}

		public virtual ZBool NewChartType
		{
			get { return fNewChartType; }
			set
			{
				if (fNewChartType != value)
				{
					SetNonPersistentPropertyValue(NewChartTypeInfo, ref fNewChartType, value);
				}
			}
		}

		public ZPropertyInfo NewChartTypeInfo
		{
			get { return GetZPropertyInfo(Schema.NewChartType); }
		}

		#endregion

		#region Period

		public virtual ZInt Period
		{
			get { return fPeriod; }
			set
			{
				if (fPeriod != value)
				{
					SetNonPersistentPropertyValue(PeriodInfo, ref fPeriod, value);
					if (!IsValidationSuspended)
					{
						ValidatePeriod();
					}
				}
			}
		}

		public ZPropertyInfo PeriodInfo
		{
			get { return GetZPropertyInfo(Schema.Period); }
		}

		#endregion

		#region Finance Year

		public ZInt FinanceYear
		{
			get { return financeYear; }
			set { SetNonPersistentPropertyValue(FinanceYearInfo, ref financeYear, value); }
		}

		public ZPropertyInfo FinanceYearInfo
		{
			get { return GetZPropertyInfo(nameof(FinanceYear)); }
		}

		#endregion

		#endregion

		ZBool fNewChartType;
		ZBool fOLdChartType;
		ZInt financeYear;
		ZInt fPeriod;
		ZString fLog;
		ZGuid fBranch;

		#region Log

		[BusinessObjectTestExclude]
		[MaxLength(10000)]
		public ZString Log
		{
			get { return fLog; }
		}

		public ZPropertyInfo LogInfo
		{
			get { return GetZPropertyInfo(Schema.Log); }
		}

		public void AddToLog(string message)
		{
			fLog += message;
			if (fLog.Length >= LogInfo.MaxLength)
			{
				fLog = fLog.SubstringSafe(fLog.Length - (LogInfo.MaxLength * 9 / 10));
			}
			SetPropertyValue(LogInfo, fLog);
			Refresh();
		}

		#endregion

		AccountingPeriodCalculator fPeriodCalculator;

		public AccountingPeriodCalculator PeriodCalculator
		{
			get { return fPeriodCalculator ?? (fPeriodCalculator = new AccountingPeriodCalculator(Factory)); }
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateFinanceYear();
			ValidateExportDirectory();
			ValidatePeriod();
			ValidateBranch();
		}

		protected virtual void ValidateBranch()
		{
			BranchInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchInfo, Branches);
		}

		protected virtual void ValidatePeriod()
		{
			PeriodInfo.ClearAllNotifications();

			if (Period.IsEmpty || Period == 0)
			{
				string errorMessage = Res.GetString("0C3BF8A7-5072-48B4-B926-33E888551010", "The 'Period' should have data.");
				PeriodInfo.AddError(errorMessage);
			}
		}

		public virtual void ValidateFinanceYear()
		{
			FinanceYearInfo.ClearAllNotifications();
			if (!FinanceYear.IsEmpty)
			{
				MandatoryValidation.CheckEntered(FinanceYearInfo);

				if (Period.IsEmpty || Period == 0)
				{
					Period = PeriodCalculator.GetLastPeriodForYear(FinanceYear);
					PeriodInfo.RefreshBinding();
				}
			}
		}

		protected virtual void ValidateExportDirectory()
		{
			ExportDirectoryInfo.ClearAllNotifications();
			if (ExportDirectory.IsEmpty || !Directory.Exists(ExportDirectory))
			{
				ExportDirectoryInfo.AddError(Res.GetString("5147b342-5b4e-4d1a-8238-2ded14f3efeb", "Please enter a valid directory."));
			}
		}

		#endregion

		GlbBranchCollection fBranchesList;

		public virtual GlbBranchCollection Branches
		{
			get { return fBranchesList ?? (fBranchesList = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK))); }
		}
	}
}
