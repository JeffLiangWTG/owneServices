using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.Business
{
	public class PropertyCopyTemplateBizo : CopyTemplateNodeBizo, IRootTypeProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System type name constant")]
		public const string GuidTypeName = "Guid";

		public PropertyCopyTemplateBizo(PropertyCopyTemplateNode propertyCopyTemplateNode, EntityCopyTemplateBizo parentEntity)
			: base(propertyCopyTemplateNode)
		{
			ParentEntity = parentEntity;
		}

		public EntityCopyTemplateBizo ParentEntity { get; }

		public new PropertyCopyTemplateNode CopyTemplateNode
		{
			get { return (PropertyCopyTemplateNode)base.CopyTemplateNode; }
		}

		#region Properties

		#region CopyMethod

		[List("CopyMethods")]
		[MaxLength(3)]
		public ZString CopyMethod
		{
			get
			{
				if (!wrongCopyMethod.IsEmpty)
				{
					return wrongCopyMethod;
				}

				switch (CopyTemplateNode.CopyMethod)
				{
					case CargoWise.UniversalCopy.CopyMethod.Empty:
						return CopyMethodCodes.Empty;
					case CargoWise.UniversalCopy.CopyMethod.Copy:
						return CopyMethodCodes.Copy;
					case CargoWise.UniversalCopy.CopyMethod.Property:
						return CopyMethodCodes.Property;
					case CargoWise.UniversalCopy.CopyMethod.Default:
						return CopyMethodCodes.Default;
					case CargoWise.UniversalCopy.CopyMethod.Value:
						return CopyMethodCodes.Value;
					case CargoWise.UniversalCopy.CopyMethod.Macro:
						return CopyMethodCodes.Macros;
					default:
						return CopyMethodCodes.DoNotCopy;
				}
			}
			set
			{
				CheckMaximumLength(CopyMethodInfo, value);
				wrongCopyMethod = ZString.Empty;

				switch (value.ToUpper())
				{
					case CopyMethodCodes.Empty:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Empty;
						break;
					case CopyMethodCodes.Copy:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Copy;
						break;
					case CopyMethodCodes.Property:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Property;
						break;
					case CopyMethodCodes.Default:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Default;
						break;
					case CopyMethodCodes.Value:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Value;
						break;
					case CopyMethodCodes.Macros:
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.Macro;
						break;
					default:
						wrongCopyMethod = value;
						CopyTemplateNode.CopyMethod = CargoWise.UniversalCopy.CopyMethod.None;
						break;
				}

				if (CopyTemplateNode.CopyMethod != CargoWise.UniversalCopy.CopyMethod.Value && !Value.IsEmpty)
				{
					Value = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					((PropertyCopyTemplateBizoValidation)Validation).ValidateValue();
				}

				HasChanges = true;
				CopyMethodInfo.RefreshBinding();
				ValueInfo.RefreshBinding();
			}
		}

		ZString wrongCopyMethod;

		public ZPropertyInfo CopyMethodInfo
		{
			get { return GetZPropertyInfo(nameof(CopyMethod)); }
		}

		public CodeDescriptionPairList CopyMethods
		{
			get
			{
				if (copyMethods == null)
				{
					copyMethods = new CodeDescriptionPairList();
					copyMethods.AddPair(CopyMethodCodes.DoNotCopy, ResString.GetMultilingualString("1847bca0-a828-486c-816e-83f9b5aa9eac", "Do not copy"));
					copyMethods.AddPair(CopyMethodCodes.Copy, ResString.GetMultilingualString("dc8cdeda-4c4b-4136-a977-5529d720e224", "Copy source value"));
					if (string.IsNullOrEmpty(CopyTemplateNode?.CustomCopyTemplateNode ?? string.Empty))
					{
						copyMethods.AddPair(CopyMethodCodes.Empty, ResString.GetMultilingualString("0ce98cd4-205d-4c6b-bcf9-dcb4523c5a27", "Empty"));
						copyMethods.AddPair(CopyMethodCodes.Default, ResString.GetMultilingualString("b82ad5e8-7e74-4d03-b919-b5b6fc6924b5", "Default value for this property"));
						copyMethods.AddPair(CopyMethodCodes.Property, ResString.GetMultilingualString("131a10ca-7135-4357-a917-18ef5baf4fde", "Other property on this object"));
						copyMethods.AddPair(CopyMethodCodes.Value, ResString.GetMultilingualString("aa03e8d2-b0bb-4b12-b7ef-1d11e0a6ff76", "Constant value"));

						if (ValueType != GuidTypeName)
						{
							copyMethods.AddPair(CopyMethodCodes.Macros, ResString.GetMultilingualString("a4c99c1d-2312-494e-b51b-3d73d2bf7121", "Use a Macro to set the value"));
						}
					}
				}
				return copyMethods;
			}
		}
		CodeDescriptionPairList copyMethods;

		public static class CopyMethodCodes
		{
			public const string DoNotCopy = "";
			public const string Empty = "EMP";
			public const string Copy = "CPY";
			public const string Property = "PRO";
			public const string Default = "DEF";
			public const string Value = "VAL";
			public const string Macros = "MAC";
		}

		#endregion

		#region CopyMethodDescription

#if DEBUG
		[BusinessObjectTestExclude]
#endif
		[List("CopyMethodDescriptions")]
		public ZString CopyMethodDescription
		{
			get { return CopyMethods.GetDescriptionFromCode(CopyMethod); }
			set
			{
				CopyMethod = CopyMethods.GetCodeFromDescription(value);
				CopyMethodDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CopyMethodDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CopyMethodDescription)); }
		}

		public CodeDescriptionPairList CopyMethodDescriptions
		{
			get
			{
				if (copyMethodDescriptions == null)
				{
					copyMethodDescriptions = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair copyMethod in CopyMethods)
					{
						copyMethodDescriptions.AddPair(copyMethod.MultilingualDescription);
					}
				}
				return copyMethodDescriptions;
			}
		}
		CodeDescriptionPairList copyMethodDescriptions;

		#endregion

		#region Value

		public ZString Value
		{
			get
			{
				if (!WrongValue.IsEmpty)
				{
					return WrongValue;
				}
				else
				{
					if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Value)
					{
						if (CopyTemplateNode.Value is DateTime || CopyTemplateNode.Value is ZDateTime)
						{
							var dateTime = new ZDateTime(CopyTemplateNode.Value);
							return dateTime.ToLongTimeString();
						}

						return Converter.CanConvertTo(typeof(string))
							? (ZString)((string)Converter.ConvertTo(CopyTemplateNode.Value, typeof(string)))
							: ZString.Empty;
					}
					else if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Default)
					{
						return Converter.CanConvertTo(typeof(string))
							? (ZString)((string)Converter.ConvertTo(CopyTemplateNode.DefaultValue, typeof(string)))
							: ZString.Empty;
					}
					else if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Property && ParentEntity != null)
					{
						var propertiesList = ParentEntityProperties;
						var propertyName = CopyTemplateNode.Value != null ? CopyTemplateNode.Value.ToString() : string.Empty;
						var propertyCaption = propertiesList.GetCodeFromDescription(propertyName);
						return !string.IsNullOrEmpty(propertyCaption) ? propertyCaption : propertyName;
					}
					else
					{
						return CopyTemplateNode.Value != null ? new ZString(CopyTemplateNode.Value.ToString()) : ZString.Empty;
					}
				}
			}
			set
			{
				WrongValue = ZString.Empty;

				if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Value)
				{
					if (Converter.CanConvertFrom(typeof(string)))
					{
						try
						{
							CopyTemplateNode.Value = Converter.ConvertFrom(value.ToString());
							HasChanges = true;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (ex.IsCriticalException())
							{
								throw;
							}

							WrongValue = value;
							WrongValueMessage = ex.Message;
						}
					}
				}
				else if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Property)
				{
					var propertiesList = ParentEntityProperties;
					var propertyCaption = value.ToString();
					var propertyName = propertiesList.GetDescriptionFromCode(propertyCaption);
					CopyTemplateNode.Value = !string.IsNullOrEmpty(propertyName) ? propertyName : propertyCaption;
				}
				else
				{
					CopyTemplateNode.Value = value.ToString();
				}

				HasChanges = true;
				ValueInfo.RefreshBinding();
			}
		}

		protected bool Value_ReadOnly
		{
			get
			{
				return
					(CopyTemplateNode.CopyMethod != CargoWise.UniversalCopy.CopyMethod.Value || IsUneditableType) &&
					CopyTemplateNode.CopyMethod != CargoWise.UniversalCopy.CopyMethod.Property &&
					CopyTemplateNode.CopyMethod != CargoWise.UniversalCopy.CopyMethod.Macro;
			}
		}

		internal ZString WrongValue { get; private set; }
		internal string WrongValueMessage { get; private set; }

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		internal TypeConverter Converter
		{
			get
			{
				if (converter == null)
				{
					Type dataType = RealDataType;
					if (dataType == typeof(bool) || dataType == typeof(string))
					{
						dataType = typeof(string);
					}
					converter = TypeDescriptor.GetConverter(dataType);
				}
				return converter;
			}
		}
		TypeConverter converter;

		internal Type RealDataType
		{
			get
			{
				if (realDataType == null && !string.IsNullOrEmpty(CopyTemplateNode.PropertyType))
				{
					realDataType = Type.GetType(CopyTemplateNode.PropertyType) ?? Type.GetType("System." + CopyTemplateNode.PropertyType);
				}
				return realDataType ?? (realDataType = typeof(object));
			}
		}
		Type realDataType;

		internal bool IsUneditableType
		{
			get { return RealDataType == typeof(object); }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList ValuesList
		{
			get
			{
				if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Property)
				{
					return ParentEntityProperties;
				}

				if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Value)
				{
					return externalValuesList;
				}

				return null;
			}
			set
			{
				externalValuesList = value;
			}
		}
		IList externalValuesList;

		internal CodeDescriptionPairList ParentEntityProperties
		{
			get
			{
				var propertiesList = new CodeDescriptionPairList();
				if (ParentEntity != null)
				{
					foreach (PropertyCopyTemplateBizo propertyBizo in ParentEntity.PropertyNodes)
					{
						propertiesList.Add(new CodeDescriptionPair(propertyBizo.Description.ToString(), propertyBizo.Name.ToString()));
					}
					foreach (PropertyCopyTemplateBizo valueOnlyPropertyBizo in ParentEntity.ValueOnlyPropertyNodes)
					{
						propertiesList.Add(new CodeDescriptionPair(valueOnlyPropertyBizo.Description.ToString(), valueOnlyPropertyBizo.Name.ToString()));
					}
				}
				return propertiesList;
			}
		}

		#endregion

		#region ValueType

		public ZString ValueType
		{
			get { return CopyTemplateNode.PropertyType; }
		}

		public ZString ValueColumnType
		{
			get { return ValueColumnTypeRaw.ToString(); }
		}

		public FieldType ValueColumnTypeRaw
		{
			get
			{
				FieldType fieldType;

				if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Property)
				{
					fieldType = FieldType.TextDropEdit;
				}
				else if (CopyTemplateNode.CopyMethod == CargoWise.UniversalCopy.CopyMethod.Macro)
				{
					fieldType = FieldType.TextMacro;
				}
				else if (ValueType == GuidTypeName)
				{
					fieldType = FieldType.Guid;
				}
				else if (ValueType == "Int32")
				{
					fieldType = FieldType.Integer;
				}
				else if (!Enum.TryParse(ValueType, out fieldType))
				{
					fieldType = FieldType.Text;
				}

				return fieldType;
			}
		}

		#endregion

		#region IsMandatory

		public bool IsMandatory
		{
			get { return CopyTemplateNode.IsMandatory; }
		}

		#endregion

		#region JustAdded

		public bool HasJustBeenAdded { get; set; }

		#endregion

		#region ModuleId

		public ModuleIdentifier ModuleId { get; set; }

		#endregion

		#endregion

		#region Overrides

		protected override string GetFriendlyName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}
			if (string.IsNullOrEmpty(friendlyName))
			{
				var resourceData = DataBoundResourceStrings.GetDataForProperty(null, name);
				if (resourceData != null)
				{
					friendlyName = !string.IsNullOrEmpty(resourceData.Caption) ? resourceData.Caption : resourceData.FullDescription;
				}
				else
				{
					friendlyName = base.GetFriendlyName(name);
				}
			}
			return friendlyName;
		}
		string friendlyName;

		protected override ZString HumanReadableNameCore => ParentEntity.HumanReadableName;

		#endregion

		#region Defaulting

		protected override void DefaultToDoNotCopyCore()
		{
			CopyMethod = CopyMethodCodes.DoNotCopy;
		}

		protected override void DefaultToCopyCore(int level)
		{
			CopyMethod = CopyMethodCodes.Copy;
		}

		#endregion

		#region Validation

		protected override CopyTemplateNodeBizoValidation GetNewValidation()
		{
			return new PropertyCopyTemplateBizoValidation(this);
		}

		#endregion

		#region Implementation of IRootTypeProvider

		[BusinessObjectTestExclude]
		public Type[] RootTypes { private get; set; }

		Type[] IRootTypeProvider.RootTypes => RootTypes;

		BusinessObject[] IRootTypeProvider.Roots => null;

		#endregion
	}

	#region Validation Class

	class PropertyCopyTemplateBizoValidation : CopyTemplateNodeBizoValidation
	{
		public PropertyCopyTemplateBizoValidation(PropertyCopyTemplateBizo parent)
			: base(parent)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCopyMethod();
			ValidateCopyMethodDescription();
			ValidateValue();
		}

		protected new PropertyCopyTemplateBizo Parent
		{
			get { return (PropertyCopyTemplateBizo)base.Parent; }
		}

		#region Properties Validation

		#region CopyMethod

		public void ValidateCopyMethod()
		{
			ZValidationInternals.Validate(Parent.CopyMethodInfo, CheckCopyMethod);
		}

		void CheckCopyMethod()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CopyMethodInfo);

			if (Parent.CopyTemplateNode.CopyMethod == CopyMethod.Value && Parent.IsUneditableType)
			{
				Parent.CopyMethodInfo.AddError(Res.GetString("d88b9129-4473-4361-9115-69252fd0b23a", "{0} cannot be edited manually.", Parent.Name));
			}

			if (Parent.IsMandatory && (Parent.ParentEntity == null || Parent.ParentEntity.NeedsChildrenForCopy()))
			{
				if (Parent.CopyTemplateNode.CopyMethod == CopyMethod.None || Parent.CopyTemplateNode.CopyMethod == CopyMethod.Empty ||
					(Parent.CopyTemplateNode.CopyMethod == CopyMethod.Default && Parent.CopyTemplateNode.DefaultValue == null))
				{
					Parent.CopyMethodInfo.AddError(Res.GetString("ecd788cd-bb49-43c2-95d7-080d4b5d2f76", "{0} is mandatory and should be initialized in some way (copied or set manually).", Parent.Name));
				}
			}
		}

		public void ValidateCopyMethodDescription()
		{
			ZValidationInternals.Validate(Parent.CopyMethodDescriptionInfo, CheckCopyMethodDescription);
		}

		void CheckCopyMethodDescription()
		{
			ValidateCopyMethod();

			if (Parent.CopyMethodInfo.HasNotifications())
			{
				Parent.CopyMethodDescriptionInfo.AddAllNotificationsFrom(Parent.CopyMethodInfo);
			}
		}

		#endregion

		#region Value

		public void ValidateValue()
		{
			ZValidationInternals.Validate(Parent.ValueInfo, CheckValue);
		}

		void CheckValue()
		{
			if (Parent.CopyTemplateNode.CopyMethod == CopyMethod.Property || Parent.CopyTemplateNode.CopyMethod == CopyMethod.Macro)
			{
				MandatoryValidation.CheckEntered(Parent.ValueInfo);
			}

			if (Parent.CopyTemplateNode.CopyMethod == CopyMethod.Property)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ValueInfo, Parent.ParentEntityProperties);

				if (Parent.ParentEntity != null)
				{
					var otherProperty = Parent.ParentEntity.PropertyNodes.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(property => property.Description == Parent.Value);
					if (otherProperty != null && Parent.RealDataType != otherProperty.RealDataType)
					{
						if (!Parent.Converter.CanConvertFrom(otherProperty.RealDataType) && !otherProperty.Converter.CanConvertTo(Parent.RealDataType))
						{
							Parent.ValueInfo.AddError(Res.GetString("a4617158-b068-4360-ba43-2deae8561303", "Selected property has values of incompatible type."));
						}
						else
						{
							Parent.ValueInfo.AddWarning(Res.GetString("61fad792-1eb3-4c9a-8a34-1e3e2c3f9138", "Selected property has values of different type, there can be errors during conversion."));
						}
					}
				}
			}

			if (!Parent.WrongValue.IsEmpty)
			{
				Parent.ValueInfo.AddError(Parent.WrongValueMessage);
			}

			if (Parent.CopyTemplateNode.CopyMethod == CopyMethod.Value && Parent.ValueType == PropertyCopyTemplateBizo.GuidTypeName && (Parent.ModuleId == null || Parent.ModuleId == ModuleIDs.NotAssigned))
			{
				Parent.ValueInfo.AddError(Res.GetString("66D8AA1B-EE18-4D4A-BE43-D36D8DE7504C", "Selected property doesn't have a Module Id, it can't have a Constant Value."));
			}
		}

		#endregion

		#endregion
	}

	#endregion
}
