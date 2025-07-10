using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Service
{
	public static class PropertyLoaderHelper
	{
		#region SuppressResourceStringsCheckRegion

		internal static Dictionary<string, object> LoadProperties(this IWorkflowProvider workflowProvider, PropertyCache cache, BMControlCustomisation layout)
		{
			if (!(workflowProvider is ICodeDescription codeDescription))
			{
				return null;
			}

			if (layout == null)
			{
				return new Dictionary<string, object>
				{
					{ "code", codeDescription.Code },
					{ "description", codeDescription.Description }
				};
			}

			var properties = LoadPropertiesForLayout(workflowProvider, layout, PropertySourceList.Codes.Job, cache);
			AddCompulsoryJobProperties(codeDescription, properties);
			return properties;
		}

		static void AddCompulsoryJobProperties(ICodeDescription codeDescription, Dictionary<string, object> properties)
		{
			AddPropertyIfAbsent("code", properties, () => codeDescription.Code); // Property names
			AddPropertyIfAbsent("description", properties, () => codeDescription.Description); // Property names
		}

		internal static void AddCompulsoryWorkflowProperties(ProcessHeader workflow, Dictionary<string, object> properties)
		{
			AddPropertyIfAbsent("description", properties, () => workflow.Description); // Property names
			AddPropertyIfAbsent("status", properties, () => workflow.FH_Status); // Property names
		}

		internal static void AddCompulsoryTaskProperties(ProcessTask task, Dictionary<string, object> properties)
		{
			AddPropertyIfAbsent("status", properties, () => task.P9_Status); // Property names
		}

		static void AddPropertyIfAbsent(string propertyName, Dictionary<string, object> properties, Func<object> valueGetter)
		{
			if (!properties.ContainsKey(propertyName))
			{
				properties.Add(propertyName, valueGetter());
			}
		}

		#endregion

		internal static Dictionary<string, object> LoadPropertiesForLayout(object target, BMControlCustomisation layout, string sourceType, PropertyCache propertyInfoCache, Func<Type, string, PropertyInfo> propertyInfoGetter = null, Func<object, PropertyInfo, string, object> propertyValueGetter = null)
		{
			var propertyValuesCache = new Dictionary<string, object>();
			var lines = layout.CustomisationLines.Cast<BMControlCustomisationLine>().Where(line => line.PropertySource == sourceType);

			return lines
				.Select(line => new { PropertyName = RemoveAngleBrackets(line.PropertyName), line.ControlType })
				.Select(pair => GetFormatterPropertyNameAndValue(target, pair.ControlType, pair.PropertyName, pair.PropertyName, string.Empty, propertyInfoCache, propertyInfoGetter, propertyValuesCache, propertyValueGetter))
				.WhereNotNull()
				.ToDictionary(pair => pair.Name, pair => pair.Value);
		}

		static string RemoveAngleBrackets(string path)
		{
			if (path[0] == '<')
			{
				path = path.Substring(1, path.Length - 2); // removing leading < and trailing >
			}
			return path;
		}

		static PropertyNameAndValue GetFormatterPropertyNameAndValue(object target,
			string controlType,
			string relativePath,
			string fullPath,
			string traversedPath,
			PropertyCache propertyInfoCache,
			Func<Type, string, PropertyInfo> propertyInfoGetter,
			Dictionary<string, object> propertyValuesCache,
			Func<object, PropertyInfo, string, object> propertyValueGetter)
		{
			PropertyInfo defaultPropertyInfoGetter(Type @type, string propName) => CustomisationLineReflectionHelper.GetPropertyInfo(@type, propName);
			propertyInfoGetter = propertyInfoGetter ?? defaultPropertyInfoGetter;

			object defaultPropertyValueGetter(object targ, PropertyInfo propInfo, string valueKey) => propInfo.GetValue(targ);
			propertyValueGetter = propertyValueGetter ?? defaultPropertyValueGetter;

			var targetType = target.GetType();

			if (relativePath.IsNullOrEmpty())
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Property path is null or empty for type {0}", targetType));
				return null;
			}

			var pathParts = relativePath.Split(new char[] { '.' }, 2); // split into 2 parts
			var propertyName = pathParts[0].Trim();
			var pathRest = pathParts.Length > 1 ? pathParts[1].Trim() : null;
			var newTraversedPath = traversedPath.IsNullOrEmpty() ? propertyName : traversedPath + propertyName;

			var propertyInfo = propertyInfoCache.GetCachedValue(targetType.GUID, propertyName, () => propertyInfoGetter(targetType, propertyName));

			if (propertyInfo == null)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Cannot find property {0} from path {1} on type {2}", propertyName, fullPath, targetType));
				return GetPostProcessedPropertyNameAndValue(newTraversedPath, null, controlType);
			}

			propertyValuesCache.TryGetValue(newTraversedPath, out object value);

			if (value == null)
			{
				value = propertyValueGetter(target, propertyInfo, newTraversedPath);
				propertyValuesCache.Add(newTraversedPath, value);
			}

			if (value == null || pathRest.IsNullOrEmpty())
			{
				return GetPostProcessedPropertyNameAndValue(fullPath, value, controlType);
			}

			return GetFormatterPropertyNameAndValue(value, controlType, pathRest, fullPath, newTraversedPath, propertyInfoCache, propertyInfoGetter, propertyValuesCache, propertyValueGetter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property names")]
		static PropertyNameAndValue GetPostProcessedPropertyNameAndValue(string path, object value, string controlType)
		{
			if (controlType == PropertyTypeList.Codes.Number && path.EndsWith((NoResString)"Hours", StringComparison.Ordinal))
			{
				path = path.Substring(0, path.Length - "Hours".Length) + (NoResString)"Minutes";

				if (value != null && value is ZDecimal hours) // value can be null when some intermediate object within the path is null
				{
					value = (ZDecimal)(hours * 60);
				}
			}
			return new PropertyNameAndValue(GetProcessedPath(path), value);
		}

		static string GetProcessedPath(string path)
		{
			return string.Join("", path.Split('.').Select((propertyName, index) => GetProcessedPropertyName(propertyName, camelCase: index == 0)));
		}

		static string GetProcessedPropertyName(string propertyName, bool camelCase = false)
		{
			var underscoreIndex = propertyName.IndexOf('_');

			if (underscoreIndex <= 3)
			{
				propertyName = propertyName.Substring(underscoreIndex + 1); // removing table prefix
			}

			if (!propertyName.IsNullOrEmpty() && camelCase)
			{
				propertyName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
			}

			return propertyName;
		}

		class PropertyNameAndValue
		{
			public PropertyNameAndValue(string name, object value)
			{
				Name = name;
				Value = value;
			}
			public string Name { get; }
			public object Value { get; }
		}
	}
}
