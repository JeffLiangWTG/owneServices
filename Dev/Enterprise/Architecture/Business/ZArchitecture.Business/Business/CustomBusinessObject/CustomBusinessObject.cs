using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	//No access to Enterprise.Masterfiles.Business from here.
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class CustomBusinessObject : NonPersistentBusinessObject, IDynamicBusinessObject, IBusinessObjectInternals, ICustomPropertyContainer, ICustomFieldProvider, ICustomTextTemplateContext, ICustomTextTemplateAlternateContexts, IObsoleteValidation
	{
		public CustomBusinessObject(BusinessObjectFactory factory, BusinessObject parent, ICustomPropertyCollection properties)
			: base(factory)
		{
			this.Parent = parent;
			this.properties = properties;
		}

		public CustomBusinessObject(BusinessObject parent, ICustomPropertyCollection properties)
			: this(null, parent, properties)
		{ }

		public BusinessObject Parent { get; }

		public override object this[string propertyIdentifier]
		{
			get
			{
				ZPropertyInfo propInfo;
				if (TryGetZPropertyInfo(propertyIdentifier, out propInfo))
				{
					return propInfo;
				}

				object metaData;
				if (TryGetMetaData(propertyIdentifier, out metaData))
				{
					return metaData;
				}

				ICustomProperty property = GetCustomProperty(propertyIdentifier);
				if (property != null)
				{
					var propertyType = property.Info.Type;
					var emptyValue = typeof(IZType).IsAssignableFrom(propertyType) ? ZDataType.ZTypeToEmptyValue(propertyType) : null;

					object propertyValue = null;
					try
					{
						propertyValue = property.GetValue(this);
					}
					catch (TargetInvocationException ex)
					{
						NotifyValueReadingError(propertyIdentifier, ex.InnerException ?? ex);
						property.TrySetValue(this, emptyValue);
					}
					catch (FormatException ex)
					{
						NotifyValueReadingError(propertyIdentifier, ex);
						property.TrySetValue(this, emptyValue);
					}

					return propertyValue ?? emptyValue;
				}
				else
				{
					return base[propertyIdentifier];
				}
			}
			set
			{
				ICustomProperty property = GetCustomProperty(propertyIdentifier);
				if (property != null)
				{
					Type propertyType = property.Info.Type;
					var oldValue = ZString.Empty;
					if (value != null && typeof(IZType).IsAssignableFrom(propertyType))
					{
						ZPropertyInfo info = GetZPropertyInfo(propertyIdentifier);

						value = ZDataType.ObjectToZType(propertyType, value);

						if (value is ZString)
						{
							ZString stringValue = ((ZString)value).TrimEnd(' ');
							oldValue = info.Value.ToString();
							value = stringValue;
						}

						bool hasChanges = !value.Equals(property.GetValue(this));
						if (hasChanges && property.TrySetValue(this, value))
						{
							HasChanges = true;
						}

						info.RefreshBinding();
						if (!IsValidationSuspended)
						{
							Validation.Validate(propertyIdentifier);
							foreach (var otherProperty in property.RelatedProperties)
							{
								Validation.Validate(otherProperty.Identifier);
							}
						}

						if (value is ZString)
						{
							CheckMaximumLengthWithOutReportError(info, oldValue);
						}
					}
					else
					{
						property.TrySetValue(this, value);
					}
				}
				else
				{
					base[propertyIdentifier] = value;
				}
			}
		}

		public void AddCustomProperties(IEnumerable<ICustomProperty> newProperties)
		{
			properties.AddCustomProperties(newProperties);
		}

		void CheckMaximumLengthWithOutReportError(ZPropertyInfo info, ZString oldValue)
		{
			var infoMaxLength = info.MaxLength;
			var newValue = info.Value.ToString();
			if (infoMaxLength > -1 && newValue.Length > infoMaxLength)
			{
				var description = GetMaximumLengthErrorDescription(info.Name, info.MaxLength, newValue, oldValue);
				info.AddError(description);
			}
		}

		void NotifyValueReadingError(string propertyIdentifier, Exception exception)
		{
			var propertyInfo = FindPropertyInfo(propertyIdentifier);
			if (propertyInfo != null)
			{
				propertyInfo.AddError(
						Res.GetString("d8d5c40e-7382-4b2c-a251-0e6871fc92c6", "Error retrieving custom value.") +
							" " + exception.Message + " " +
							Res.GetString("b455f415-3aa7-4d3a-a149-7a9f24b237d5", "If you save, original value will be overridden."));
			}
		}

		protected internal new ZPropertyInfo GetZPropertyInfo(string propertyIdentifier)
		{
			return base.GetZPropertyInfo(propertyIdentifier);
		}

		protected internal new KPropertyDescriptorCollection GetProperties()
		{
			return base.GetProperties();
		}

		protected internal ICustomProperty GetCustomProperty(string propertyIdentifier)
		{
			if (!String.IsNullOrEmpty(propertyIdentifier))
			{
				return properties.GetCustomProperty(propertyIdentifier);
			}
			else
			{
				return null;
			}
		}

		bool TryGetMetaData(string metaDataPropertyName, out object metaDataValue)
		{
			DynamicMetaData metaData = this.GetMetaData(metaDataPropertyName);
			if (metaData != null)
			{
				metaDataValue = metaData.Value;
				return true;
			}
			else
			{
				metaDataValue = null;
				return false;
			}
		}

		bool TryGetZPropertyInfo(string propertyIdentifier, out ZPropertyInfo propInfo)
		{
			ICustomProperty property;
			if (IsZPropertyInfo(propertyIdentifier, out property))
			{
				propInfo = GetZPropertyInfo(property.Identifier);
				return true;
			}

			propInfo = null;
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
		bool IsZPropertyInfo(string propertyIdentifier, out ICustomProperty property)
		{
			if (!String.IsNullOrEmpty(propertyIdentifier) && propertyIdentifier.EndsWith("Info", StringComparison.Ordinal))
			{
				property = GetCustomProperty(propertyIdentifier.Substring(0, propertyIdentifier.Length - 4));
				if (property != null)
				{
					return true;
				}
			}

			property = null;
			return false;
		}

		public override ZPropertyInfoHashtable ZPropertyInfoHash
		{
			get
			{
				if (propertyInfoHash == null)
				{
					propertyInfoHash = new CustomBusinessObjectPropertyInfoHashtable(this);
				}

				return propertyInfoHash;
			}
		}
		ZPropertyInfoHashtable propertyInfoHash;

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CustomBusinessObjectValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual CustomBusinessObjectValidation GetNewValidation()
		{
			return new CustomBusinessObjectValidation(this);
		}

		void IBusinessObjectInternals.Validate(string propertyName)
		{
			Validation.Validate(propertyName);
			GetZPropertyInfo(propertyName).RefreshBinding();
		}

		#endregion

		#region IDynamicBusinessObject Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not visible to user")]
		string[] IDynamicBusinessObject.PropertyNames
		{
			get
			{
				string[] propertyIdentifiers = properties.GetIdentifiers();
				int numProp = propertyIdentifiers.Length;
				Array.Resize(ref propertyIdentifiers, numProp * 2);
				for (int i = 0; i < numProp; i++)
				{
					propertyIdentifiers[numProp + i] = propertyIdentifiers[i] + "Info";
				}

				return propertyIdentifiers;
			}
		}

		DynamicBusinessObjectProperty IDynamicBusinessObject.GetProperty(string propertyIdentifier)
		{
			ICustomProperty property = GetCustomProperty(propertyIdentifier);
			if (property != null)
			{
				return property.Info;
			}
			else if (IsZPropertyInfo(propertyIdentifier, out property))
			{
				return new DynamicBusinessObjectProperty(typeof(ZPropertyInfo), true);
			}

			return null;
		}

		#endregion

		#region ICustomPropertyContainer Members

		IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
		{
			get { return properties.GetProperties(); }
		}

		#endregion

		#region ICustomFieldProvider Members
		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			return this;
		}
		#endregion

		#region ICustomTextTemplateContext Members
		string ICustomTextTemplateContext.GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			var property = GetPropertyAndDescription(bindingMemberInfo.BindingField).Property;
			var propertyIdentifier = property == null ? bindingMemberInfo.BindingField : property.Identifier;

			if (this.Parent != null)
			{
				return this.Parent.TableName + "." + propertyIdentifier;
			}
			else
			{
				return propertyIdentifier;
			}
		}
		BusinessObject[] ICustomTextTemplateContext.GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return this.Parent != null ? new[] { this, this.Parent } : new BusinessObject[] { this };
		}
		#endregion

		#region ICustomTextTemplateAlternateContexts Members
		IReadOnlyCollection<string> ICustomTextTemplateAlternateContexts.GetAlternateTextTemplateContextIDs(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			var (property, description) = this.GetPropertyAndDescription(bindingMemberInfo.BindingField);

			if (property == null || description == null)
			{
				return Array.Empty<string>();
			}

			var customFieldDescription = description.GetDescription(0, CultureInfo.InvariantCulture);
			var propertyType = property.Info.Type;
			return new[] { "." + GetLegacyIdentifier1(customFieldDescription, propertyType), "." + GetLegacyIdentifier2(customFieldDescription, propertyType) };
		}
		#endregion

#if DEBUG
		public
#endif
		(ICustomProperty Property, IDescription Description) GetPropertyAndDescription(string propertyIdentifier)
		{
			var property = this.GetCustomProperty(propertyIdentifier);

			if (property == null)
			{
				return (null, null);
			}

			var description = (IDescription)property.Info.MetaData.FirstOrDefault(x => x.Id == MetaDataTypes.Description)?.Value;

			if (description == null)
			{
				return (property, null);
			}

			return (property, description);
		}

#if DEBUG
		public
#else
		internal
#endif
		static string GetLegacyIdentifier1(string customFieldDescription, Type propertyType)
		{
			var typeName = propertyType.Name;
			var typeLength = typeName.Length;
			StringBuilder sb = new StringBuilder(customFieldDescription.Length + typeLength + 10);
			sb.Append("__");
			// We cannot have + or . in the custom field name as these interfere with gui binding, so they are replaced 
			// with chinese characters to maintain uniqueness, and these characters cannot be entered by the user.
			sb.Append(customFieldDescription.Replace('+', '加').Replace('.', '点'));
			sb.Append("__prop__");
			sb.Append(typeName);
			return sb.ToString();
		}

#if DEBUG
		public
#else
		internal
#endif
		static string GetLegacyIdentifier2(string customFieldDescription, Type propertyType)
		{
			var typeName = propertyType.Name;
			var typeLength = typeName.Length;
			StringBuilder sb = new StringBuilder(customFieldDescription.Length + typeLength + 10);
			sb.Append("__");
			foreach (char c in customFieldDescription)
			{
				if (char.IsLetterOrDigit(c) || c.Equals('_'))
				{
					sb.Append(c);
				}
			}
			sb.Append("__prop__");
			sb.Append(typeName);
			return sb.ToString();
		}

#if DEBUG
		public
#else
		internal
#endif
		static string GetLegacyIdentifier3(string customFieldDescription, Type propertyType)
		{
			var alias = GetLegacyIdentifier1(customFieldDescription, propertyType);
			return alias.Replace(" ", "_");
		}

		readonly ICustomPropertyCollection properties;
	}
}
