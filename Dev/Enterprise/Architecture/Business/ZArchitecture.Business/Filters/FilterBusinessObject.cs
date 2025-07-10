using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public enum DateComparisonOperator
	{
		HasDateEntered,
		HasNoDateEntered,
		HasDateInRange
	}
	public enum TimeComparisonOperator
	{
		HasTimeEntered,
		HasNoTimeEntered,
		HasTimeInRange
	}

	public static class FilterConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public const string QueryDeciderNoSelectionCode = "None";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter strip 'name'")]
	public static class FilterDescriptions
	{
		public const string ActiveStatus = "Active Status";
		public const string CreatingUser = "Creating User";
		public const string CreatingBranch = "Creating Branch";
		public const string CreatingDepartment = "Creating Department";
		public const string CreatedTime = "Created Time";
		public const string LastEditUser = "Last Edit User";
		public const string LastEditTime = "Last Edit Time";
		public const string CreatedOnWeb = "Created On Web/Internal";
		public const string Cashier = "Cashier";
		public const string AuditingUser = "Auditing User";
	}

	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class FilterBusinessObject : BusinessObject, IObsoleteValidation
	{
		protected FilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Filter

		public void ResetToDefaultValues()
		{
			SetDefaultValues();
			RefreshBinding();
		}

		public abstract ZQuery Filter { get; }

		#endregion

		#region Expensive Query

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public virtual string ExpensiveQueryWarning
		{
			get { return "This search could take a while to complete.\nAre you sure you want to continue?"; }
		}

		public virtual bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		protected static ZString GetNotForInSubqueryIfNegative(SQLComparisonOperator comparisonOperator)
		{
			if ((comparisonOperator == SQLComparisonOperator.DoesNotStartWith) ||
				 (comparisonOperator == SQLComparisonOperator.NotEqual) ||
				 (comparisonOperator == SQLComparisonOperator.NotContains) ||
				 (comparisonOperator == SpecialComparisonOperator.IsNotBlank))
			{
				return "NOT";
			}
			else
			{
				return "";
			}
		}

		#region Defaults

		[Serializable]
		public class BadExternalDefaultException : Exception
		{
			public BadExternalDefaultException(string message, Exception inner)
				: base(message, inner)
			{
			}

#if NETFRAMEWORK
			protected BadExternalDefaultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		public virtual SearchType SearchType { get; set; } = SearchType.Sql;

		public bool IsCurrentSearchTypeSupportedByCurrentDefaults => Defaults == null || Defaults.Count == 0 || Defaults.HasDefaultsFor(SearchType);

		protected void SwitchSearchType()
		{
			SearchType = SearchType == SearchType.Sql ? SearchType.Index : SearchType.Sql;
		}

		/// <summary>
		/// Setting an external default results in the provided filters being set as
		/// non-changeable, non-removable defaults for which the user can enter
		/// a value on the right-side but cannot change the left-side
		/// </summary>
		public void SetExternalDefaults(IFilterBusinessObjectDefaultsProvider defaultsProvider)
		{
			if (defaultsProvider.FilterBusinessObjectDefaults != null)
			{
				SetExternalDefaults(defaultsProvider.FilterBusinessObjectDefaults);
			}
		}

		/// <summary>
		/// Setting an external default results in the provided filters being set as
		/// non-changeable, non-removable defaults for which the user can enter
		/// a value on the right-side but cannot change the left-side
		/// </summary>
		public void SetExternalDefaults(FilterBusinessObjectDefaults defaults)
		{
			Defaults = defaults;
			ApplyDefaults();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		protected virtual void ApplyDefaults(IEnumerable<ZString> skippedOrCategory = null)
		{
			if (Defaults == null)
			{
				return;
			}

			foreach (FilterBusinessObjectDefault filterDefault in Defaults.GetDefaultsToUse(SearchType))
			{
				try
				{
					SetExternalDefaultsCore(filterDefault, skippedOrCategory);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					string message = string.Format("Setting defaults for FilterBizO '{0}'. Failed to set property '{1}' to '{2}' of type '{3}'",
						this.GetType().FullName, filterDefault.PropertyName, filterDefault.Value.ToString(), filterDefault.Value.GetType().Name);

					throw new BadExternalDefaultException(message, e);
				}
			}
		}

		FilterBusinessObjectDefaults Defaults;

		protected virtual void SetExternalDefaultsCore(FilterBusinessObjectDefault filterDefault, IEnumerable<ZString> skippedOrCategory = null)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(filterDefault.Value);
			Type destinationType = this[filterDefault.PropertyName].GetType();

			if (converter.CanConvertTo(destinationType))
			{
				this[filterDefault.PropertyName] = converter.ConvertTo(filterDefault.Value, destinationType);
			}
			else
			{
				this[filterDefault.PropertyName] = filterDefault.Value;
			}
		}

		/// <summary>
		/// Copies a code (eg, from findbox textbox) across into the filter business object (eg, used by the findbox popup).
		/// Override this to perform custom logic for the findbox popup.
		/// </summary>
		public virtual void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			string codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(typeOfElementsToFind);
			if (ZPropertyInfoHash.ContainsKey(codePropertyName))
			{
				this[codePropertyName] = code;
			}
		}

		public virtual void SetAdditionalFilterDefaults(ZString code, IBusinessObjectCollection bizObjCollection)
		{
		}

		#endregion

		#region Add Filter

		public void AddIfNotEmpty(ZQuery query, SchemaColumn property, SQLComparisonOperator comparisonOperator, IZType value)
		{
			if (value is ZBool boolValue)
			{
				if (boolValue == ZBool.True)
				{
					query.AddToFilter(property, comparisonOperator, value);
				}
			}
			else if (!value.IsEmpty)
			{
				query.AddToFilter(property, comparisonOperator, value);
			}
		}

		public void AddIfNotEmpty(ZQuery query, SchemaColumn property, SQLComparisonOperator comparisonOperator, IZType value, bool useMultiSearch)
		{
			if (!useMultiSearch)
			{ AddIfNotEmpty(query, property, comparisonOperator, value); return; }

			if (value is ZBool boolValue)
			{
				if (boolValue == ZBool.True)
				{
					query.AddToFilter_PossiblyCommaSeparated(property, comparisonOperator, value);
				}
			}
			else if (!value.IsEmpty)
			{
				query.AddToFilter_PossiblyCommaSeparated(property, comparisonOperator, value);
			}
		}

		protected void AddQueryProviderFilter(
			ZQuery query, string queryDeciderCodeBindTo, string queryDeciderCodeBindToList,
			SQLComparisonOperator comparisonOperator, string bindTo, int queryProviderIndex)
		{
			ZQueryProviderCodeDescriptionListBase list = (ZQueryProviderCodeDescriptionListBase)GetPropertyValueByName(queryDeciderCodeBindToList);
			ZQueryProviderCodeDescription selection = list.GetElementFromCode(new ZString(this[queryDeciderCodeBindTo]));

			if (selection != null)
			{
				IQueryProvider provider = selection.QueryProviders[queryProviderIndex];
				IZType value = (IZType)this[bindTo];
				if (value.IsValid && !value.IsEmpty)
				{
					query.AddToFilter(provider.GetQuery(selection.GetDefaultableOperator(comparisonOperator), value));
				}
			}
		}

		protected void SetQueryProviderParameterPropertyValue(ZPropertyInfo info, IZType value, string queryDeciderCodeBindTo, params string[] filterValueBindTos)
		{
			SetPropertyValue(info, value);
			UpdateReadOnlyOnQueryDeciderParameter(queryDeciderCodeBindTo, filterValueBindTos);
		}

		public const string QueryDeciderNoSelectionCode = FilterConstants.QueryDeciderNoSelectionCode;

		protected void UpdateReadOnlyOnQueryDeciderParameter(string queryDeciderCodeBindTo, params string[] filterValueBindTos)
		{
			bool valueControlsReadOnly = this[queryDeciderCodeBindTo].ToString() == QueryDeciderNoSelectionCode;
			foreach (string filterValueBindTo in filterValueBindTos)
			{
				ZPropertyInfo info = ZPropertyInfoHash[filterValueBindTo];

				if (valueControlsReadOnly)
				{
					if (info.Value is ZDateTime)
					{
						SetPropertyValue(info, ZDateTime.Empty);
					}
					else if (info.Value is ZGuid)
					{
						SetPropertyValue(info, ZGuid.Empty);
					}
					else if (info.Value is ZString)
					{
						SetPropertyValue(info, ZString.Empty);
					}
					info.ClearAllNotifications();
				}
				PropertiesWithReadOnly[filterValueBindTo] = valueControlsReadOnly;
			}
		}

		protected Dictionary<string, bool> PropertiesWithReadOnly { get; } = new Dictionary<string, bool>();
		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			PropertiesWithReadOnly.TryGetValue(property.Name, out bool result);
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Implementation

		protected IBusinessObjectInternals BizOInternals
		{
			get { return this; }
		}

		public virtual string PK_ColumnName
		{
			get { return PKSchemaColumn.Name; }
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return CargoWise.Schema.Schema.GenericPkColumn; }
		}

		#endregion
	}
}


