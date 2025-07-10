using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	[DebuggerDisplay("{PropertyName} ({PropertyType})")]
	sealed class MacroBusinessObjectProperty : IMacroBusinessObjectProperty
	{
		public MacroBusinessObjectProperty(string caption, IDynamicData data)
		{
			Argument.NotNullOrEmpty(caption, nameof(caption));
			Argument.NotNull(data, nameof(data));

			this.caption = caption;
			this.data = data;
		}

		readonly string caption;
		readonly IDynamicData data;

		#region IMacroBusinessObjectProperty members

		public string DisplayName
		{
			get
			{
				if (displayName == null)
				{
					displayName = caption.Substring(0, Math.Min(caption.Length, displayNameMaxLength));
				}

				return displayName;
			}
		}

		string displayName;
		const int displayNameMaxLength = 20;

		public string PropertyName
		{
			get
			{
				if (string.IsNullOrWhiteSpace(propertyName))
				{
					propertyName = CreatePropertyName(caption);
				}

				return propertyName;
			}
		}

		string propertyName;

		string CreatePropertyName(string name)
		{
			var invalidNameCharactersRegex = new Regex("[^a-zA-Z0-9_]");

			return string.Concat("prop_", invalidNameCharactersRegex.Replace(name ?? (NoResString)"Undefined", ""));
		}

		public Type PropertyType
		{
			get { return Nullable.GetUnderlyingType(data.Type) ?? data.Type; }
		}

		public object Value
		{
			get { return GetValue(); }
			set { SetValue(value); }
		}

		object GetValue()
		{
			var type = PropertyType;
			var value = data.Value;

			if (value == null && type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return value;
		}

		void SetValue(object value)
		{
			var originalValue = GetValue();

			if (!Equals(originalValue, value))
			{
				data.SetValue(value);
			}
		}

		public IReadOnlyCollection<DynamicMetaData> MetaData
		{
			get { return metaData ?? (metaData = data.GetMetaData()); }
		}

		DynamicMetaData[] metaData;

		public void Validate(ZPropertyInfo propertyInfo)
		{
			// empty
		}

		#endregion
	}
}
