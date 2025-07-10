using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ZeroAmountTaxTypesDescriptions : RegistryBusinessObjectTemplate
	{
		protected ZeroAmountTaxTypesDescriptionsCollection GetParentCollection()
		{
			return (ZeroAmountTaxTypesDescriptionsCollection)GetParentCollection(this, typeof(ZeroAmountTaxTypesDescriptionsCollection));
		}

		#region Schema

		public static class Schema
		{
			public const string TaxType = "TaxType";
			public const string Description = "Description";
			public const string DefaultValue = "DefaultValue";
			public const string EnglishDefaultValue = "EnglishDefaultValue";
			public const string OverrideValue = "OverrideValue";
			public const string EnglishOverrideValue = "EnglishOverrideValue";

			public const int TaxTypeMaxLength = 3;
			public const int DescriptionMaxLength = 400;
			public const int DefaultValueMaxLength = 50;
			public const int OverrideValueMaxLength = 50;
		}

		#endregion

		#region Bound Properties

		[ResourceStringData("19ad0708-567c-4693-9322-f731028ae40f", Caption = "Tax Type")]
		[MaxLength(Schema.TaxTypeMaxLength)]
		public ZString TaxType
		{
			get { return taxType; }
			set
			{
				SetNonPersistentPropertyValue(TaxTypeInfo, ref taxType, value);
				TaxTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TaxTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxType); }
		}

		ZString taxType;

		[ResourceStringData("6e9c5abc-5baa-4e00-a2d6-90d3a012f961", ShortCaption = "Desc.", Caption = "Description")]
		[MaxLength(Schema.DescriptionMaxLength)]
		public ZString Description
		{
			get { return description; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		ZString description;

		[MaxLength(Schema.DefaultValueMaxLength)]
		public MultilingualString DefaultValue
		{
			get { return defaultValue ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DefaultValueInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DefaultValueInfo, ref defaultValue, value, false);
				EnglishDefaultValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DefaultValueInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultValue); }
		}

		[MaxLength(Schema.DefaultValueMaxLength)]
		[ResourceStringData("b4ff8fc2-4439-4c2e-a6ae-7b405144ebcc", ShortCaption = "Def. Val", Caption = "Default Value")]
		public ZString EnglishDefaultValue
		{
			get { return DefaultValue.GetUnresolvedString(); }
			set { DefaultValue = (NoResString)value; }
		}

		MultilingualString defaultValue;

		public ZPropertyInfo EnglishDefaultValueInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDefaultValue); }
		}

		[MaxLength(Schema.OverrideValueMaxLength)]
		public MultilingualString OverrideValue
		{
			get { return overrideValue ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(OverrideValueInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(OverrideValueInfo, ref overrideValue, value, false);
				EnglishOverrideValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OverrideValueInfo
		{
			get { return GetZPropertyInfo(Schema.OverrideValue); }
		}

		[MaxLength(Schema.OverrideValueMaxLength)]
		[ResourceStringData("c541caf1-aa02-4d53-9066-5597d49018a0", ShortCaption = "Ovr. Val", Caption = "Override Value")]
		public ZString EnglishOverrideValue
		{
			get { return OverrideValue.GetUnresolvedString(); }
			set { OverrideValue = (NoResString)value; }
		}

		public ZPropertyInfo EnglishOverrideValueInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishOverrideValue); }
		}

		MultilingualString overrideValue;

		#endregion

		#region RegistryBusinessObjectTemplate Members

		#region XML Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TaxType = reader.ReadElementString(Schema.TaxType);
			Description = reader.ReadElementString(Schema.Description);
			EnglishDefaultValue	= reader.ReadElementString(Schema.DefaultValue);
			EnglishOverrideValue = reader.ReadElementString(Schema.OverrideValue);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.TaxType, TaxType);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.DefaultValue, EnglishDefaultValue);
			writer.WriteElementString(Schema.OverrideValue, EnglishOverrideValue);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new ZeroAmountTaxTypesDescriptions();
			using (clone.GetValidationSuspender())
			{
				clone.TaxType = TaxType;
				clone.Description = Description;
				clone.DefaultValue = DefaultValue;
				clone.OverrideValue = OverrideValue;
			}

			return clone;
		}

		#endregion
	}
}
