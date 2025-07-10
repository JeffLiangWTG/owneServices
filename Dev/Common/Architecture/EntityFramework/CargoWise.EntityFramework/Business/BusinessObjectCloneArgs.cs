using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectCloneArgs
	{
		public BusinessObjectCloneArgs()
			: this(Array.Empty<string>())
		{
		}

		public BusinessObjectCloneArgs(IEnumerable<string> columnNamesToExcludeFromCopy)
			: this(columnNamesToExcludeFromCopy, null)
		{
		}

		public BusinessObjectCloneArgs(IEnumerable<string> columnNamesToExcludeFromCopy, Type typeToCloneAs)
			: this(columnNamesToExcludeFromCopy, typeToCloneAs, false)
		{
		}

		public BusinessObjectCloneArgs(IEnumerable<string> columnNamesToExcludeFromCopy, bool performRowCopyWithoutTriggeringValidationAndSetter)
			: this(columnNamesToExcludeFromCopy, null, performRowCopyWithoutTriggeringValidationAndSetter)
		{
		}

		public BusinessObjectCloneArgs(IEnumerable<string> columnNamesToExcludeFromCopy, Type typeToCloneAs, bool performRowCopyWithoutTriggeringValidationAndSetter)
		{
			this.columnNamesToExcludeFromCopy = new HashSet<string>(columnNamesToExcludeFromCopy);
			this.TypeToCloneAs = typeToCloneAs;
			this.PerformRowCopyWithoutTriggeringValidationAndSetter = performRowCopyWithoutTriggeringValidationAndSetter;
			this.businessObjectValueOverrideMapping = new Dictionary<Type, FixValueProvider>();
		}

		public BusinessObjectCloneArgs(BusinessObjectFactory factory, IEnumerable<string> columnNamesToExcludeFromCopy, Type typeToCloneAs, bool performRowCopyWithoutTriggeringValidationAndSetter)
			: this(columnNamesToExcludeFromCopy, typeToCloneAs, performRowCopyWithoutTriggeringValidationAndSetter)
		{
			this.AlternativeFactoryToInstantiateCloneIn = factory;
		}

		public BusinessObjectCloneArgs(BusinessObjectFactory factory, IEnumerable<string> columnNamesToExcludeFromCopy, Type typeToCloneAs, bool performRowCopyWithoutTriggeringValidationAndSetter, Func<BusinessObject, BusinessObject, DataColumn, bool> copyDecider)
			: this(factory, columnNamesToExcludeFromCopy, typeToCloneAs, performRowCopyWithoutTriggeringValidationAndSetter)
		{
			this.CopyDecider = copyDecider;
		}

		/// <summary>
		/// Can be null
		/// </summary>
		public readonly BusinessObjectFactory AlternativeFactoryToInstantiateCloneIn;

		/// <summary>
		/// Can be null
		/// </summary>
		public readonly Type TypeToCloneAs;
		public readonly bool PerformRowCopyWithoutTriggeringValidationAndSetter;
		public readonly Func<BusinessObject, BusinessObject, DataColumn, bool> CopyDecider;
		readonly HashSet<string> columnNamesToExcludeFromCopy;

		public void AddExcludedColumns(IEnumerable<string> excludedColumns)
		{
			foreach (string column in excludedColumns)
			{
				if (!columnNamesToExcludeFromCopy.Contains(column))
				{
					columnNamesToExcludeFromCopy.Add(column);
				}
			}
		}

		public bool IsExcludedFromCloning(string columnName)
		{
			return columnNamesToExcludeFromCopy.Contains(columnName);
		}

		public IEnumerable<string> GetExcludedColumns()
		{
			return columnNamesToExcludeFromCopy;
		}

		public void AddValueOverride(Type bizObjType, string columnName, IZTypeInternals value)
		{
			var overrideValueMapping = businessObjectValueOverrideMapping.GetOrAdd(bizObjType);
			overrideValueMapping.SetupMapping(columnName, value);
		}

		readonly Dictionary<Type, FixValueProvider> businessObjectValueOverrideMapping;

		internal IValueOverrideProvider GetValueOverrideProvider(Type type)
		{
			businessObjectValueOverrideMapping.TryGetValue(type, out var provider);
			return provider;
		}

		class FixValueProvider : IValueOverrideProvider
		{
			public void SetupMapping(string columnName, IZTypeInternals value) => fixValueMapping[columnName] = value;
			public bool TryGetValue(string columnName, out IZTypeInternals value) => fixValueMapping.TryGetValue(columnName, out value);

			readonly Dictionary<string, IZTypeInternals> fixValueMapping = new Dictionary<string, IZTypeInternals>();
		}

		internal interface IValueOverrideProvider
		{
			bool TryGetValue(string columnName, out IZTypeInternals value);
		}
	}
}
