using System;
using System.Globalization;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class LeadTimeFilter : ModuleFilter
	{
		public LeadTimeFilter(BusinessObjectFactory factory)
			: base(ProcessHeader.ModuleFilterConstants.LeadTime, factory)
		{
			MultilingualDescription = ResString.GetMultilingualString("392cbbf4-98b7-4fa5-b83f-95a5eafffe55", "Lead Time to an Agreed Delivery Date");
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			LeadTimeEstimateFactor = DefaultLeadTimeEstimateFactor;
		}

		const decimal DefaultLeadTimeEstimateFactor = 1.5m;

		#endregion

		#region Schema

		public static class Schema
		{
			public const string LeadTimeRangeCode = "LeadTimeRangeCode";
			public const string LeadTimeRangeCodeList = "LeadTimeRangeCodeList";
			public const string BufferPK = "BufferPK";
			public const string BuffersList = "BuffersList";
			public const string LeadTimeEstimateFactor = "LeadTimeEstimateFactor";
		}

		#endregion

		#region Properties

		#region LeadTimeRangeCode

		[List(Schema.LeadTimeRangeCodeList)]
		[ResourceStringData("LeadTimeFilter|LeadTimeRangeCode", Caption = "Range")] // For validation only. Resource strings used by the GUI are in ProcessHeaderFilterStrip.cs.
		public ZString LeadTimeRangeCode
		{
			get { return leadTimeRangeCode; }
			set
			{
				if (LeadTimeRangeCode != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(LeadTimeRangeCodeInfo, ref leadTimeRangeCode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLeadTimeRangeCode();
				}
			}
		}

		ZString leadTimeRangeCode;

		public ZPropertyInfo LeadTimeRangeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LeadTimeRangeCode)); }
		}

		public ICodeDescriptionPairList LeadTimeRangeCodeList
		{
			get { return Factory.GetCachedValue<LeadTimeRangeTypeList>(); }
		}

		#endregion

		#region BufferPK

		[List(Schema.BuffersList)]
		[ResourceStringData("LeadTimeFilter|BufferPK", Caption = "Buffer")] // For validation only. Resource strings used by the GUI are in ProcessHeaderFilterStrip.cs.
		public ZGuid BufferPK
		{
			get { return bufferPK; }
			set
			{
				if (BufferPK != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(BufferPKInfo, ref bufferPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBufferPK();
				}
			}
		}

		ZGuid bufferPK;

		public ZPropertyInfo BufferPKInfo
		{
			get { return GetZPropertyInfo(nameof(BufferPK)); }
		}

		public IBusinessObjectCollection BuffersList
		{
			get { return Factory.GetCachedValue("LeadTimeFilter.BuffersList", () => new BMComponentCollection(Factory, new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer))); }
		}

		#endregion

		#region LeadTimeEstimateFactor

		[ResourceStringData("LeadTimeFilter|LeadTimeEstimateFactor", Caption = "Lead Time Estimate Factor")] // For validation only. Resource strings used by the GUI are in ProcessHeaderFilterStrip.cs.
		public ZDecimal LeadTimeEstimateFactor
		{
			get { return leadTimeEstimateFactor; }
			set
			{
				if (LeadTimeEstimateFactor != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(LeadTimeEstimateFactorInfo, ref leadTimeEstimateFactor, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLeadTimeEstimateFactor();
				}
			}
		}

		ZDecimal leadTimeEstimateFactor;

		public ZPropertyInfo LeadTimeEstimateFactorInfo
		{
			get { return GetZPropertyInfo(nameof(LeadTimeEstimateFactor)); }
		}

		#endregion

		#endregion

		#region ModuleFilter Overrides

		protected override void ClearCore()
		{
			LeadTimeRangeCode = ZString.Empty;
			BufferPK = ZGuid.Empty;
			LeadTimeEstimateFactor = DefaultLeadTimeEstimateFactor;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new LeadTimeFilter(Factory);
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new LeadTimeFilterValidation(this);
		}

		protected override bool IsEmptyCore => LeadTimeRangeCode.IsEmpty && BufferPK.IsEmpty;

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { LeadTimeRangeCode, BufferPK, LeadTimeEstimateFactor, }; }
		}

		#endregion

		#region Query

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, @"
				FH_PK in
				(
					SELECT
						ProcessHeader.FH_PK
					FROM
						dbo.ProcessHeader wfl
					JOIN
						dbo.ProcessHeader job on wfl.FH_FH_ParentHeader = job.FH_PK
					CROSS APPLY
						(
							SELECT FC_BufferTimespanInMinutes as Mins
							FROM dbo.BMComponent
							WHERE FC_PK = '{0}'
						) BufferSize
					CROSS APPLY
						(
							SELECT dateadd(minute, -((BufferSize.Mins / 2.0) + (wfl.FH_PlannedDurationInMinutes * {1})), coalesce(wfl.FH_AgreedDeliveryDate, job.FH_AgreedDeliveryDate)) as Value
						) LeadTimeStart
					WHERE 1 = 1
						AND coalesce(wfl.FH_AgreedDeliveryDate, job.FH_AgreedDeliveryDate) is not null
						AND {2}
						AND wfl.FH_Status in ('{3}', '{4}')
				)
				",
				/*0*/ BufferPK,
				/*1*/ LeadTimeEstimateFactor,
				/*2*/ LeadTimeRangePredicate,
				/*3*/ WorkflowStatusList.Codes.Open,
				/*4*/ WorkflowStatusList.Codes.Blocked
				), null);

			return query;
		}

		string LeadTimeRangePredicate
		{
			get
			{
				switch (LeadTimeRangeCode)
				{
					case LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime:
						return string.Format(CultureInfo.InvariantCulture, (ZArchitecture.Core.NoResString)"LeadTimeStart.Value <= '{0}'", ZDateTime.UtcNow.SqlFormat); // Part of a SQL statement

					case LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime:
						return string.Format(CultureInfo.InvariantCulture, (ZArchitecture.Core.NoResString)"LeadTimeStart.Value > '{0}'", ZDateTime.UtcNow.SqlFormat); // Part of a SQL statement

					default:
						return "1 = 1";
				}
			}
		}

		#endregion

		#region Serialisation

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = filterToCopyFrom as LeadTimeFilter;

			if (filter != null)
			{
				BufferPK = filter.BufferPK;
				LeadTimeRangeCode = filter.LeadTimeRangeCode;
				LeadTimeEstimateFactor = filter.LeadTimeEstimateFactor;
			}
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.BufferPK)
			{
				BufferPK = new ZGuid(reader.ReadElementContentAsString());
			}

			if (reader.Name == Schema.LeadTimeRangeCode)
			{
				LeadTimeRangeCode = reader.ReadElementContentAsString();
			}

			if (reader.Name == Schema.LeadTimeEstimateFactor)
			{
				LeadTimeEstimateFactor = reader.ReadElementContentAsDecimal();
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.BufferPK, BufferPK.ToString());
			writer.WriteElementString(Schema.LeadTimeRangeCode, LeadTimeRangeCode);
			writer.WriteElementString(Schema.LeadTimeEstimateFactor, LeadTimeEstimateFactor.ToString());
		}

		#endregion

		#region Validation

		public new LeadTimeFilterValidation Validation
		{
			get { return (LeadTimeFilterValidation)base.Validation; }
		}

		public class LeadTimeFilterValidation : ModuleFilterValidation
		{
			internal LeadTimeFilterValidation(LeadTimeFilter filter)
				: base(filter)
			{
				this.filter = filter;
			}

			readonly LeadTimeFilter filter;

			public override Type AutoValidationType
			{
				get { return GetType(); }
			}

			public override void ValidateAll()
			{
				ValidateLeadTimeRangeCode();
				ValidateBufferPK();
				ValidateLeadTimeEstimateFactor();
			}

			public void ValidateLeadTimeRangeCode()
			{
				ValidateCalculatedProperty(filter.LeadTimeRangeCodeInfo);
			}

			protected void CheckLeadTimeRangeCode()
			{
				MandatoryValidation.CheckEntered(filter.LeadTimeRangeCodeInfo);
				ListValidation.ErrorIfInvalidCode(filter.LeadTimeRangeCodeInfo);
			}

			public void ValidateBufferPK()
			{
				ValidateCalculatedProperty(filter.BufferPKInfo);
			}

			protected void CheckBufferPK()
			{
				MandatoryValidation.CheckEntered(filter.BufferPKInfo);
				ListValidation.ErrorIfInvalidPK(filter.BufferPKInfo);
			}

			public void ValidateLeadTimeEstimateFactor()
			{
				ValidateCalculatedProperty(filter.LeadTimeEstimateFactorInfo);
			}

			protected void CheckLeadTimeEstimateFactor()
			{
				CompareValidation.CheckGreaterThanOrEqualTo(filter.LeadTimeEstimateFactorInfo, 0m);
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			LeadTimeRangeCode = RandomString(MaxLength);
			BufferPK = ZGuid.NewZGuid();
			LeadTimeEstimateFactor = 1M;
		}

#endif
		#endregion
	}
}
