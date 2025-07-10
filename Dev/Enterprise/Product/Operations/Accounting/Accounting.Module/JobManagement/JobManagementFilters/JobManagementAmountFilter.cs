using System;
using System.ComponentModel;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Module
{
	public delegate ZQuery GetJobManagementAmountQuery(SQLComparisonOperator comparisonOperator, ZDecimal value);

	public class JobManagementAmountFilter : ModuleFilter, IJobManagementAmountFilter
	{
		#region Construction

		public JobManagementAmountFilter(ZString description, SchemaNumericColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		protected JobManagementAmountFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public JobManagementAmountFilter(ZString description, GetJobManagementAmountQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new JobManagementAmountFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			JobManagementAmountFilter filter = (JobManagementAmountFilter)filterToCopyFrom;
			Property = filter.Property;

			if (SupportsComparisonOperatorSet)
			{
				ComparisonOperator = filter.ComparisonOperator;
			}
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion

		#region class ComparisonConstants

		public static class ComparisonConstants
		{
			[ThreadSafe]
			public static MultilingualString Exact = ResString.GetMultilingualString("03259E80-EA4B-4556-9065-46348D72C12C", "equals");

			[ThreadSafe]
			public static MultilingualString LessThan = ResString.GetMultilingualString("1603C79F-2153-451F-B467-41D817615058", "less than");

			[ThreadSafe]
			public static MultilingualString GreaterThan = ResString.GetMultilingualString("51B71C6F-F2FC-43D7-88D9-06A9229C4BCD", "greater than");

			[ThreadSafe]
			public static string Default = Exact.GetUnresolvedString();
		}

		#endregion

		#region Comparison Operator

		[BusinessObjectTestExclude] // "?" means invalid value
		[EditorBrowsable(EditorBrowsableState.Never)]
		[List("ComparisonOperator_List")]
		public virtual ZString ComparisonOperator // for binding
		{
			get { return fComparisonOperator; }
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (fComparisonOperator != value)
				{
					if (!value.IsEmpty &&
						!value.EqualsIgnoringCase(ComparisonConstants.Exact.GetUnresolvedString()) &&
						!value.EqualsIgnoringCase(ComparisonConstants.LessThan.GetUnresolvedString()) &&
						!value.EqualsIgnoringCase(ComparisonConstants.GreaterThan.GetUnresolvedString()))
					{
						fComparisonOperator = "?";
					}
					else
					{
						fComparisonOperator = value;
					}
					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public SQLComparisonOperator SqlComparisonOperator
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.Exact.GetUnresolvedString())
				{
					return SQLComparisonOperator.Equal;
				}

				if (ComparisonOperator == ComparisonConstants.LessThan.GetUnresolvedString())
				{
					return SQLComparisonOperator.LessThan;
				}

				if (ComparisonOperator == ComparisonConstants.GreaterThan.GetUnresolvedString())
				{
					return SQLComparisonOperator.GreaterThan;
				}

				return SQLComparisonOperator.Equal;
			}

			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (SqlComparisonOperator != value)
				{
					InvalidateCachedQuery();
				}

				if (value == SQLComparisonOperator.Equal)
				{
					ComparisonOperator = ComparisonConstants.Exact;
				}
				else if (value == SQLComparisonOperator.LessThan)
				{
					ComparisonOperator = ComparisonConstants.LessThan;
				}
				else if (value == SQLComparisonOperator.GreaterThan)
				{
					ComparisonOperator = ComparisonConstants.GreaterThan;
				}
				else
				{
					throw new ArgumentException("Only Equal, LessThan, and GreaterThan are supported.");
				}
			}
		}

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					fComparisonOperator_List = new CodeDescriptionPairList();
					fComparisonOperator_List.AddPair(ComparisonConstants.Exact, Res.GetString("72a32c86-34c7-42f5-a91b-1b1e75ab91e4", "Search for an Exact match"));
					fComparisonOperator_List.AddPair(ComparisonConstants.LessThan, Res.GetString("128df2f0-7470-4017-9b13-abdc46e14384", "Search for fields that Less Than the supplied value"));
					fComparisonOperator_List.AddPair(ComparisonConstants.GreaterThan, Res.GetString("23026e33-a1b0-42bf-aeaa-b5a5737df463", "Search for fields that Greater Than the supplied value"));
				}
				return fComparisonOperator_List;
			}
		}

		protected virtual bool SupportsComparisonOperatorSet
		{
			get { return true; }
		}

		void EnsureSupportsComparisonOperatorSet()
		{
			if (!SupportsComparisonOperatorSet)
			{
				ErrorReporter.ReportOnce("Cannot set SqlComparisonOperator on a module filter",
					"Cannot set SqlComparisonOperator on a module filter of type " + GetType().Name + ".\n" +
					"Filter Description: " + Description);
			}
		}

		ZString fComparisonOperator = ComparisonConstants.Default;
		CodeDescriptionPairList fComparisonOperator_List;

		#endregion

		#region Clear / IsEmpty / Defaults

		protected override void ClearCore()
		{
			Property = DefaultProperty;

			if (SupportsComparisonOperatorSet)
			{
				ComparisonOperator = DefaultComparisonOperator;
			}
		}

		protected override bool IsEmptyCore => false;

		public ZDecimal DefaultProperty
		{
			get { return fDefaultProperty; }
			set
			{
				fDefaultProperty = value;
				Property = value;
				InvalidateCachedQuery();
			}
		}

		ZDecimal fDefaultProperty;

		public ZString DefaultComparisonOperator
		{
			get { return fDefaultComparisonOperator; }
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (fDefaultComparisonOperator != value)
				{
					if (value.EqualsIgnoringCase(ComparisonConstants.Exact.GetUnresolvedString()) ||
						value.EqualsIgnoringCase(ComparisonConstants.LessThan.GetUnresolvedString()) ||
						value.EqualsIgnoringCase(ComparisonConstants.GreaterThan.GetUnresolvedString()))
					{
						fDefaultComparisonOperator = value;
						ComparisonOperator = value;
					}
					else
					{
						throw new ArgumentException("Only Equal, LessThan, and GreaterThan are supported.");
					}

					InvalidateCachedQuery();
				}
			}
		}

		ZString fDefaultComparisonOperator = ComparisonConstants.Default;

		#endregion

		#region Property

		public virtual ZDecimal Property
		{
			get { return fProperty; }
			set
			{
				if (fProperty != value)
				{
					fProperty = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}
					PropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo PropertyInfo
		{
			get { return GetZPropertyInfo(nameof(Property)); }
		}

		ZDecimal fProperty;

		#endregion

		#region PropertyValidation

		public Validation PropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fPropertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { fPropertyValidation = value; }
		}
		Validation fPropertyValidation;

		#endregion

		#region Validation

		public new JobManagementAmountFilterValidation Validation
		{
			get { return (JobManagementAmountFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new JobManagementAmountFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Property }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (Property > 0m)
			{
				return new ZQuery(FilterColumn, SqlComparisonOperator, Property);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			if (SupportsComparisonOperatorSet)
			{
				writer.WriteElementString("Comparer", ComparisonOperator);
			}
			writer.WriteElementString("Property", Property.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter string")]
		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Comparer")
			{
				string temp = reader.ReadElementString("Comparer");
				if (SupportsComparisonOperatorSet)
				{
					ComparisonOperator = temp;
				}
			}

			if (reader.Name == "Property")
			{
				string propertyAsString = reader.ReadElementString("Property");
				Property = ZDecimal.ParseSafe(propertyAsString, 0);
			}
		}

		#endregion

		#region IJobManagementAmountFilter Members

		public IComponent GetFilterControl()
		{
			return new JobManagementAmountFilterControl();
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property = 1M;
		}

#endif
		#endregion
	}

	#region class JobManagementAmountFilterValidation

	public class JobManagementAmountFilterValidation : ModuleFilterValidation
	{
		public JobManagementAmountFilterValidation(JobManagementAmountFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty

		public void ValidateProperty()
		{
			ValidateCalculatedProperty(Parent.PropertyInfo);
		}

		protected virtual void CheckProperty()
		{
			if (Parent.PropertyValidation != null)
			{
				Parent.PropertyValidation(Parent.PropertyInfo);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected readonly JobManagementAmountFilter Parent;
	}

	#endregion
}
