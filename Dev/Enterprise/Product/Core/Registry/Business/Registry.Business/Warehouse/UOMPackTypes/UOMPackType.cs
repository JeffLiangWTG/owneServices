using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class UOMPackType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const string NumberOfLabels = "NumberOfLabels";
		}

		#endregion

		#region Properties

		#region Code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsSystemDefined")]
		[ReadOnlyMember("IsSystemDefined")]
		[ResourceStringData("UOMPackType|Code", Caption = "Code")]
		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set { SetNonPersistentPropertyValue<ZString>(CodeInfo, ref code, value); }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(UOMPackType.Schema.Code); }
		}

		ZString code;

		#endregion

		#region Description

		[ResourceStringData("UOMPackType|Description", ShortCaption = "Desc.", Caption = "Description")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsSystemDefined")]
		[ReadOnlyMember("IsSystemDefined")]
		[MaxLength(MaxDescriptionLength)]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		MultilingualString description;
		internal const int MaxDescriptionLength = 50;

		#endregion

		#region EnglishDescription

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsSystemDefined")]
		[ReadOnlyMember("IsSystemDefined")]
		[MaxLength(MaxDescriptionLength)]
		[ResourceStringData("UOMPackType|Description", ShortCaption = "Desc.", Caption = "Description")]
		public virtual ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set { Description = (NoResString)value; }
		}

		public virtual ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		#endregion

		#region NumberOfLabels

		[ResourceStringData("UOMPackType|NumberOfLabels", ShortCaption = "Labels", Caption = "Number Of Labels")]
		public ZInt NumberOfLabels
		{
			get { return numberOfLabels; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfLabelsInfo, ref numberOfLabels, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberOfLabels();
				}
			}
		}

		public ZPropertyInfo NumberOfLabelsInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfLabels); }
		}

		ZInt numberOfLabels;

		public void ValidateNumberOfLabels()
		{
			NumberOfLabelsInfo.ClearAllNotifications();
			CompareValidation.CheckNumberGreaterThanZero(NumberOfLabelsInfo);
			CompareValidation.CheckNumberNotNegative(NumberOfLabelsInfo);
		}

		#endregion

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNumberOfLabels();
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UOMPackType();
			using (clone.GetValidationSuspender())
			{
				clone.Code = Code;
				clone.Description = Description;
				clone.NumberOfLabels = NumberOfLabels;
			}

			return clone;
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			EnglishDescription = reader.ReadElementString(Schema.Description);
			NumberOfLabels = reader.ReadElementStringAsZInt(Schema.NumberOfLabels);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(UOMPackType.Schema.Code, Code);
			writer.WriteElementString(UOMPackType.Schema.Description, EnglishDescription);
			writer.WriteElementString(UOMPackType.Schema.NumberOfLabels, NumberOfLabels.ToString());
		}

		#endregion
	}
}
