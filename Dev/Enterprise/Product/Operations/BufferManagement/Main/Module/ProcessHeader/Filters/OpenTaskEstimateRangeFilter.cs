using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.BufferManagement.Business.ProcessHeader;

namespace Enterprise.BufferManagement.Module
{
	public class OpenTaskEstimateRangeFilter : ModuleFilter
	{
		public OpenTaskEstimateRangeFilter(BusinessObjectFactory factory)
			: base(Schema.Identifier, factory)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|OpenTaskEstimateRangeFilter", "Open Task Estimate Range");
		}

		#region Schema

		public static class Schema
		{
			public const string Identifier = ModuleFilterConstants.TaskOpenEstimateRange;
			public const string Scope = "Scope";
			public const string ScopeList = "ScopeList";
			public const string MinStdEstimate = "MinStdEstimate";
			public const string MaxStdEstimate = "MaxStdEstimate";
			public const string MinStdEstHour = "MinStdEstHour";
			public const string MaxStdEstHour = "MaxStdEstHour";
		}

		#endregion

		#region Properties

		#region Scope

		[List(Schema.ScopeList)]
		public ZString Scope
		{
			get { return scope; }
			set
			{
				if (Scope != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(ScopeInfo, ref scope, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateScope();
				}
			}
		}

		ZString scope;

		public ZPropertyInfo ScopeInfo
		{
			get { return GetZPropertyInfo(Schema.Scope); }
		}

		#endregion

		#region Min Std Estimate

		[BusinessObjectTestExclude]
		[ResourceStringData("OpenTaskEstimateRangeFilter.MinStdEstimate", Caption = "Min Std Estimate")]
		public ZDateTime MinStdEstimate
		{
			get { return GetTimeFromDecimalValue(MinStdEstHour); }
			set { MinStdEstHour = GetDecimalFromTimeValue(value); }
		}

		public ZWrappedPropertyInfo MinStdEstimateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MinStdEstimate, x => MinStdEstHourInfo); }
		}

		ZDecimal MinStdEstHour
		{
			get { return minStdEstHour; }
			set
			{
				if (MinStdEstHour != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(MinStdEstHourInfo, ref minStdEstHour, value > hourValueLimit ? hourValueLimit : value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMinStdEstHour();
				}
			}
		}

		ZDecimal minStdEstHour;

		ZPropertyInfo MinStdEstHourInfo
		{
			get { return GetZPropertyInfo(Schema.MinStdEstHour); }
		}

		#endregion

		#region Max Std Estimate

		[BusinessObjectTestExclude]
		[ResourceStringData("OpenTaskEstimateRangeFilter.MaxStdEstimate", Caption = "Max Std Estimate")]
		public ZDateTime MaxStdEstimate
		{
			get { return GetTimeFromDecimalValue(MaxStdEstHour); }
			set { MaxStdEstHour = GetDecimalFromTimeValue(value); }
		}

		public ZWrappedPropertyInfo MaxStdEstimateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MaxStdEstimate, x => MaxStdEstHourInfo); }
		}

		ZDecimal MaxStdEstHour
		{
			get { return maxStdEstHour; }
			set
			{
				if (MaxStdEstHour != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(MaxStdEstHourInfo, ref maxStdEstHour, value > hourValueLimit ? hourValueLimit : value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMaxStdEstHour();
				}
			}
		}

		ZDecimal maxStdEstHour;

		ZPropertyInfo MaxStdEstHourInfo
		{
			get { return GetZPropertyInfo(Schema.MaxStdEstHour); }
		}

		#endregion

		const double hourValueLimit = 999 + (1 / 60d * 59);

		static ZDateTime GetTimeFromDecimalValue(ZDecimal value) => TimeSpan.FromHours((double)value);

		static ZDecimal GetDecimalFromTimeValue(ZDateTime timeValue)
		{
			return timeValue.GetMinutesFromDateTimeSpan() / 60;
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public void SetTimeValuesFromHoursForTest(decimal minHours, decimal maxHours)
		{
			MinStdEstHour = minHours;
			MaxStdEstHour = maxHours;
		}
#endif

		#endregion

		#region Query Implementation

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var functionName = Scope == ScopeRangeList.Codes.InsideRange ? (NoResString)"Inside" : (NoResString)"Outside"; // SQL function name
			var sql = string.Format(CultureInfo.InvariantCulture, "FH_PK IN (SELECT FH_PK FROM dbo.BMGetProcessHeadersForOpenTaskEstimateRange{0}(@min, @max, @taskTypesToExclude))", functionName);
			var parameterCollection = new ZSqlParameterCollection(
				ZSqlParameter.New((NoResString)"@min", MinStdEstHour, CargoWise.Schema.Schema.GenericDecimalColumn), // SQL parameter
				ZSqlParameter.New((NoResString)"@max", MaxStdEstHour, CargoWise.Schema.Schema.GenericDecimalColumn), // SQL parameter
				ZSqlParameter.New("@taskTypesToExclude", TaskTypesToExcludeXml, CargoWise.Schema.Schema.GenericMaxStringSchemaColumn));

			query.AddFilterAndZSQLParameterCollection(sql, parameterCollection);

			return query;
		}

		static string GetTaskTypesToExcludeXml()
		{
			var doc = new XmlDocument();
			var xml = doc.AppendChild(doc.CreateElement((NoResString)"xml")); // SQL parameter xml
			var registryItem = WorkflowDataRegistry.Instance.TaskTypes.Value;

			foreach (var workflowTypeCategory in registryItem.Cast<CategorisedWorkflowTaskTypes>())
			{
				var workflowType = workflowTypeCategory.Code.Trim();
				var types = workflowTypeCategory.TaskTypes.Cast<WorkflowTaskType>().Where(type => TaskInclusionHelper.IsIgnoredTaskTypeForChannelingAndChunkingConsiderations(type));

				foreach (var type in types)
				{
					var row = xml.AppendChild(doc.CreateElement((NoResString)"row")); // SQL parameter xml
					row.AppendChild(doc.CreateElement("WorkflowType")).InnerText = workflowType; // SQL parameter xml
					row.AppendChild(doc.CreateElement("TaskType")).InnerText = type.Code.Trim(); // SQL parameter xml
				}
			}

			return xml.InnerXml;
		}

		string TaskTypesToExcludeXml => taskTypesToExcludeXml ?? (taskTypesToExcludeXml = GetTaskTypesToExcludeXml());

		string taskTypesToExcludeXml;

		protected override bool IsEmptyCore => false;

		public override bool IsExpensiveQuery => false;

		#endregion

		#region Filter Implementation

		protected override FilterCategory DefaultCategory
		{
			get { return ProcessHeaderFilterBusinessObject.TasksFilterCategory; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OpenTaskEstimateRangeFilter(Factory);
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { MinStdEstHour, MaxStdEstHour, Scope }; }
		}

		protected override void ClearCore()
		{
			MinStdEstHour = 0;
			MaxStdEstHour = 0;
			Scope = ZString.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = filterToCopyFrom as OpenTaskEstimateRangeFilter;
			if (filter != null)
			{
				MinStdEstHour = filter.MinStdEstHour;
				MaxStdEstHour = filter.MaxStdEstHour;
				Scope = filter.Scope;
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Scope, Scope);
			writer.WriteElementString(Schema.MinStdEstHour, MinStdEstHour.ToString());
			writer.WriteElementString(Schema.MaxStdEstHour, MaxStdEstHour.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.Scope)
			{
				Scope = reader.ReadElementContentAsString();
			}

			if (reader.Name == Schema.MinStdEstHour)
			{
				MinStdEstHour = ZDecimal.ParseSafe(reader.ReadElementContentAsString(), 0.0);
			}

			if (reader.Name == Schema.MaxStdEstHour)
			{
				MaxStdEstHour = ZDecimal.ParseSafe(reader.ReadElementContentAsString(), 0.0);
			}
		}

		#endregion

		#region Related Business Objects

		public CodeDescriptionPairList ScopeList
		{
			get { return Factory.GetCachedValue<ScopeRangeList>(); }
		}

		#endregion

		#region Validation

		public new OpenTaskEstimateRangeFilterValidation Validation
		{
			get { return (OpenTaskEstimateRangeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OpenTaskEstimateRangeFilterValidation(this);
		}

		public class OpenTaskEstimateRangeFilterValidation : ModuleFilterValidation
		{
			public OpenTaskEstimateRangeFilterValidation(OpenTaskEstimateRangeFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly OpenTaskEstimateRangeFilter parent;

			public void ValidateScope()
			{
				ValidateCalculatedProperty(parent.ScopeInfo);
			}

			protected virtual void CheckScope()
			{
				MandatoryValidation.CheckEntered(parent.ScopeInfo);
				ListValidation.ErrorIfInvalidCode(parent.ScopeInfo);
			}

			public void ValidateMinStdEstHour()
			{
				ValidateCalculatedProperty(parent.MinStdEstHourInfo);
				ValidateCalculatedProperty(parent.MaxStdEstHourInfo);
			}

			protected virtual void CheckMinStdEstHour()
			{
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.MinStdEstHourInfo, parent.MaxStdEstHourInfo);
			}

			public void ValidateMaxStdEstHour()
			{
				ValidateCalculatedProperty(parent.MaxStdEstHourInfo);
				ValidateCalculatedProperty(parent.MinStdEstHourInfo);
			}

			protected virtual void CheckMaxStdEstHour()
			{
				CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.MaxStdEstHourInfo, parent.MinStdEstHourInfo);
			}

			public override void ValidateAll()
			{
				ValidateScope();
				ValidateMinStdEstHour();
				ValidateMaxStdEstHour();
			}

			public override Type AutoValidationType
			{
				get { return GetType(); }
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Scope = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}
