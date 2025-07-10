using System;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a property that provides a default constant value for a meta data item.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MetaDataValueAttribute : SingleMetaDataAttribute
	{
		public MetaDataValueAttribute(string metaDataTypeId, object value)
			: base(metaDataTypeId)
		{
			Argument.NotNull(metaDataTypeId, nameof(metaDataTypeId));
			fValue = value;
		}

		public MetaDataValueAttribute(string metaDataTypeId, Type componentWithConstantValue, string memberWithConstantValue)
			: base(metaDataTypeId)
		{
			Argument.NotNull(metaDataTypeId, nameof(metaDataTypeId));
			ComponentWithConstantValue = componentWithConstantValue;
			MemberWithConstantValue = memberWithConstantValue;
			valueNeedsReflection = true;
		}

		/// <summary>
		/// Get the Type of the component with the value stored in a static field or property. Null
		/// if not specified.
		/// </summary>
		public Type ComponentWithConstantValue { get; private set; }

		/// <summary>
		/// Get the member name of the static field or property the constant value is stored in. Null
		/// if not specified.
		/// </summary>
		public string MemberWithConstantValue { get; private set; }

		/// <summary>
		/// Get whether a constant value could be found.
		/// </summary>
		public bool HasValue
		{
			get { return !valueNeedsReflection || FieldOrProperty != null; }
		}

		/// <summary>
		/// Get the constant value of the meta data item.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public object Value
		{
			get
			{
				if (valueNeedsReflection)
				{
					if (FieldOrProperty == null)
					{
						var expectedMemberWithConstantValue = MemberWithConstantValue ?? "[MemberWithConstantValue is null]";
						var expectedComponentWithConstantValue = ComponentWithConstantValue != null ? ComponentWithConstantValue.FullName : "[ComponentWithConstantValue is null]";
						throw new InvalidOperationException(
							string.Format("Could not find static field or property '{0}' on component '{1}'", expectedMemberWithConstantValue, expectedComponentWithConstantValue));
					}
					var field = FieldOrProperty as FieldInfo;
					var property = FieldOrProperty as PropertyInfo;
					if (field != null)
					{
						fValue = field.GetValue(null);
					}
					else if (property != null)
					{
						fValue = property.GetValue(null, Array.Empty<object>());
					}
					valueNeedsReflection = false;
				}
				return fValue;
			}
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypeId;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return false;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			if (metaDataTypeId == MetaDataTypeId)
			{
				if (HasValue)
				{
					return Value;
				}
			}
			return null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return null;
		}

		#region Implementation

		object fValue;
		bool valueNeedsReflection;

		MemberInfo FieldOrProperty
		{
			get
			{
				if (!fieldOrPropertyPopulated)
				{
					if (!string.IsNullOrEmpty(MemberWithConstantValue) && ComponentWithConstantValue != null)
					{
						fieldOrProperty =
							(MemberInfo)ComponentWithConstantValue.GetField(MemberWithConstantValue, BindingFlags.Public | BindingFlags.Static) ??
							ComponentWithConstantValue.GetProperty(MemberWithConstantValue, BindingFlags.Public | BindingFlags.Static);
						fieldOrPropertyPopulated = true;
					}
				}
				return fieldOrProperty;
			}
		}
		MemberInfo fieldOrProperty;
		bool fieldOrPropertyPopulated;

		#endregion
	}
}
