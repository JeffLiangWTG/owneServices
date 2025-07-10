using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class ExtendLastFinancialYearSettings : NonPersistentBusinessObject<ExtendLastFinancialYearSettingsValidation>, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string FinancialYear = "FinancialYear";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string PeriodFormat = "PeriodFormat";
		}

		#endregion

		public ExtendLastFinancialYearSettings()
		{
			using (SuspendSettingHasChanges())
			{
				PeriodFormat = Core.Constants.ACPeriodFormat.Month;
			}
		}

		#region Financial Year

		public ZShort FinancialYear
		{
			get
			{
				return financialYear;
			}
			set
			{
				SetNonPersistentPropertyValue(FinancialYearInfo, ref financialYear, value);
				FinancialYearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FinancialYearInfo
		{
			get { return GetZPropertyInfo(Schema.FinancialYear); }
		}

		#endregion

		#region Start Date
		public ZDateTime StartDate
		{
			get { return startDate; }
			set
			{
				SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value);
				StartDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		#endregion

		#region EndDate

		public ZDateTime EndDate
		{
			get { return endDate; }
			set
			{
				SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEndDate();
				}
				EndDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		#endregion

		public ZDateTime LastPeriodEndDate
		{
			get { return lastPeriodEndDate; }
			set
			{
				lastPeriodEndDate = value;
			}
		}

		public int LastPeriod
		{
			get { return lastPeriod; }
			set
			{
				lastPeriod = value;
			}
		}

		#region PeriodFormat

		[MaxLength(3)]
		[List("PeriodType")]
		public ZString PeriodFormat
		{
			get { return periodFormat; }
			set
			{
				if (periodFormat != value)
				{
					CheckMaximumLength(PeriodFormatInfo, value);
					SetNonPersistentPropertyValue(PeriodFormatInfo, ref periodFormat, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePeriodFormat();
					}
					PeriodFormatInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PeriodFormatInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodFormat); }
		}

		#endregion

		public CodeDescriptionPairList PeriodType
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ACPeriodCountType); }
		}

		#region Validation

		public override ExtendLastFinancialYearSettingsValidation GetNewValidation()
		{
			return new ExtendLastFinancialYearSettingsValidation(this);
		}

		#endregion

		ZShort financialYear;
		ZDateTime startDate;
		ZDateTime endDate;
		ZDateTime lastPeriodEndDate;
		int lastPeriod;
		ZString periodFormat;
	}
}
