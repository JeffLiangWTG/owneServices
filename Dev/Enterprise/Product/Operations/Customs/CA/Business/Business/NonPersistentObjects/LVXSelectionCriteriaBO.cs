using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LVXSelectionCriteriaBO : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string PeriodYear = "PeriodYear";
			public const string PeriodMonth = "PeriodMonth";
			public const string OH_Importer = "OH_Importer";
			public const string GS_NKBroker = "GS_NKBroker";
			public const string Branch = "Branch";
			public const string ProvinceOfClearance = "ProvinceOfClearance";
		}

		#endregion

		public LVXSelectionCriteriaBO()
			: base()
		{
		}

		public LVXSelectionCriteriaBO(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region PeriodYear

		[MaxLength(4)]
		public ZInt PeriodYear
		{
			get
			{
				return fPeriodYear;
			}
			set
			{
				var year = Math.Max(Math.Min(value, ZDateTime.MaxSmallDateTimeValue.Year - 1), ZDateTime.MinSmallDateTimeValue.Year);
				SetNonPersistentPropertyValue(PeriodYearInfo, ref fPeriodYear, year);
				ValidatePeriodYear();
			}
		}

		ZInt fPeriodYear;

		public ZPropertyInfo PeriodYearInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodYear); }
		}

		#endregion

		#region PeriodMonth

		[MaxLength(2)]
		public ZInt PeriodMonth
		{
			get
			{
				return fPeriodMonth;
			}
			set
			{
				var month = Math.Max(Math.Min(value, 12), 1);
				SetNonPersistentPropertyValue(PeriodMonthInfo, ref fPeriodMonth, month);
				ValidatePeriodMonth();
			}
		}

		ZInt fPeriodMonth;

		public ZPropertyInfo PeriodMonthInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodMonth); }
		}

		#endregion

		#region OH_Importer

		[RelatedBusinessObject("Importer")]
		[List(nameof(Importers))]
		public ZGuid OH_Importer
		{
			get
			{
				return fOH_Importer;
			}
			set
			{
				SetNonPersistentPropertyValue(OH_ImporterInfo, ref fOH_Importer, value);
				ValidateOH_Importer();
			}
		}

		ZGuid fOH_Importer;

		public ZPropertyInfo OH_ImporterInfo
		{
			get { return GetZPropertyInfo(Schema.OH_Importer); }
		}

		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(OH_Importer); }
		}

		#endregion

		#region GS_NKBroker

		[List(nameof(CusAgents))]
		public ZString GS_NKBroker
		{
			get
			{
				return fGS_NKBroker;
			}
			set
			{
				SetNonPersistentPropertyValue(GS_NKBrokerInfo, ref fGS_NKBroker, value);
				ValidateGS_NKBroker();
			}
		}

		ZString fGS_NKBroker;

		public ZPropertyInfo GS_NKBrokerInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NKBroker); }
		}

		#endregion

		#region Branch

		[List(nameof(Branches))]
		public ZGuid Branch
		{
			get
			{
				return fBranch;
			}
			set
			{
				SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
				ValidateBranch();
			}
		}

		ZGuid fBranch;

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(Schema.Branch); }
		}

		#endregion

		#region ProvinceOfClearance

		[MaxLength(2)]
		[List(nameof(CanadianProvinces))]
		public ZString ProvinceOfClearance
		{
			get
			{
				return fProvinceOfClearance;
			}
			set
			{
				SetNonPersistentPropertyValue(ProvinceOfClearanceInfo, ref fProvinceOfClearance, value);
				ValidateProvinceOfClearance();
			}
		}

		ZString fProvinceOfClearance;

		public ZPropertyInfo ProvinceOfClearanceInfo
		{
			get { return GetZPropertyInfo(Schema.ProvinceOfClearance); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePeriodYear();
			ValidatePeriodMonth();
			ValidateOH_Importer();
			ValidateGS_NKBroker();
			ValidateBranch();
			ValidateProvinceOfClearance();
		}

		public void ValidatePeriodYear()
		{
			if (!IsValidationSuspended)
			{
				PeriodYearInfo.ClearAllNotifications();
				if (PeriodYear == ZInt.Zero)
				{
					PeriodYearInfo.AddError(Res.GetString("48186526-1f5e-48d4-9449-1b8fec5ffe1c", "Please enter a valid Period Year."));
				}
				else if (PeriodYear != ZInt.Zero && (PeriodYear < ZDateTime.MinSmallDateTimeValue.Year || PeriodYear > ZDateTime.MaxSmallDateTimeValue.Year))
				{
					PeriodYearInfo.AddError(Res.GetString("dde84203-256d-49e9-8d16-800832435750", "Period Year should bigger than {0} and smaller than {1}", ZDateTime.MinSmallDateTimeValue.Year, ZDateTime.MaxSmallDateTimeValue.Year));
				}
			}
		}

		public void ValidatePeriodMonth()
		{
			if (!IsValidationSuspended)
			{
				PeriodMonthInfo.ClearAllNotifications();
				if (PeriodMonth == ZInt.Zero)
				{
					PeriodMonthInfo.AddError(Res.GetString("c8e49093-fe8c-4cd7-abac-f103ceed8cf1", "Please enter a valid Period Month."));
				}
				if (PeriodMonth != ZInt.Zero && (PeriodMonth < 1 || PeriodMonth > 12))
				{
					PeriodMonthInfo.AddError(Res.GetString("29e3a42b-a134-46ad-b76d-5fe3520837ef", "Period Month should bigger than 1 and smaller than 12"));
				}
			}
		}

		public void ValidateOH_Importer()
		{
			if (!IsValidationSuspended)
			{
				OH_ImporterInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidPK(OH_ImporterInfo);
			}
		}

		public void ValidateGS_NKBroker()
		{
			if (!IsValidationSuspended)
			{
				GS_NKBrokerInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(GS_NKBrokerInfo);
			}
		}

		public void ValidateBranch()
		{
			if (!IsValidationSuspended)
			{
				BranchInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidPK(BranchInfo);
			}
		}

		public void ValidateProvinceOfClearance()
		{
			if (!IsValidationSuspended)
			{
				ProvinceOfClearanceInfo.ClearAllNotifications();
				ListValidation.MessageErrorIfInvalidCode(ProvinceOfClearanceInfo, CanadianProvinces);
			}
		}

		#endregion

		#region Lookups

		public GlbBranchCollection Branches
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public GlbStaffCollection CusAgents
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public OrgHeaderCollection Importers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public CanadianProvinceList CanadianProvinces
		{
			get { return Factory.GetCachedValue<CanadianProvinceList>(); }
		}

		#endregion
	}
}
