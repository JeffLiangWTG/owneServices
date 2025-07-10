using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceRollupAndGroupDescription : RegistryBusinessObjectTemplate
	{
		public InvoiceRollupAndGroupDescription()
		{
		}

		public InvoiceRollupAndGroupDescription(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string Style = "Style";
			public const string Group = "Group";
			public const string Description = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const int StyleMaxLength = 3;
			public const int GroupMaxLength = 23;
			public const int DescriptionMaxLength = 100;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateEnglishDescription();
		}

		public InvoiceRollupAndGroupDescriptionValidation Validation => validation ?? (validation = new InvoiceRollupAndGroupDescriptionValidation(this));

		InvoiceRollupAndGroupDescriptionValidation validation;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRollupAndGroupDescription(fallbackLevel, factory);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Style, Style);
			writer.WriteElementString(Schema.Group, Group);
			writer.WriteElementString(Schema.Description, Description);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Style = (NoResString)reader.ReadElementString(Schema.Style);
			Group = (NoResString)reader.ReadElementString(Schema.Group);
			Description = (NoResString)reader.ReadElementString(Schema.Description);
		}

		[MaxLength(Schema.StyleMaxLength)]
		[ReadOnly(true)]
		public ZString Style
		{
			get { return style; }
			set
			{
				CheckMaximumLength(StyleInfo, value);
				SetNonPersistentPropertyValue(StyleInfo, ref style, value, false);
			}
		}

		public ZPropertyInfo StyleInfo
		{
			get { return GetZPropertyInfo(Schema.Style); }
		}
		ZString style;

		[MaxLength(Schema.GroupMaxLength)]
		[ReadOnly(true)]
		public ZString Group
		{
			get { return group; }
			set
			{
				CheckMaximumLength(GroupInfo, value);
				SetNonPersistentPropertyValue(GroupInfo, ref group, value, false);
			}
		}

		public ZPropertyInfo GroupInfo
		{
			get { return GetZPropertyInfo(Schema.Group); }
		}
		ZString group;

		[MaxLength(Schema.DescriptionMaxLength)]
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

		[MaxLength(Schema.DescriptionMaxLength)]
		public ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set
			{
				if (EnglishDescription != value)
				{
					CheckMaximumLength(EnglishDescriptionInfo, value);
					Description = (NoResString)value;
					DescriptionInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateEnglishDescription();
				}
			}
		}

		public ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		MultilingualString description;
	}
}