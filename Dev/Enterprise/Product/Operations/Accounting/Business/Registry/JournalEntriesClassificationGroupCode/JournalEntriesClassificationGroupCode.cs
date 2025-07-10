using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[CodeProperty(Schema.Code)]
	[DescriptionProperty(Schema.Description)]
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesClassificationGroupCode : RegistryBusinessObjectTemplate
	{
		public JournalEntriesClassificationGroupCode()
		{
		}

		public JournalEntriesClassificationGroupCode(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string Prefix = "Prefix";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateDescription();
			ValidatePrefix();
		}

		#region Properties

		#region Code

		ZString fCode;

		[MaxLength(3)]
		public ZString Code
		{
			get { return fCode; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);

				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(CodeInfo);

			if (Code.Length != 3)
			{
				CodeInfo.AddError(ResString.GetMultilingualString("68EEAB85-3772-4A2A-974B-9F877CA26E1E", "Group Code length should be 3."));
			}

			if (ParentCollection.Cast<JournalEntriesClassificationGroupCode>().Any(x => x.Code == Code && x.PK != PK))
			{
				CodeInfo.AddError(ResString.GetMultilingualString("00C7D333-8819-42BE-A566-5082815EF2EE", "Group Code should be unique."));
			}
		}

		#endregion

		#region Description

		ZString fDescription;

		public ZString Description
		{
			get { return fDescription; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		#endregion

		#region Prefix

		ZString fPrefix;

		public ZString Prefix
		{
			get { return fPrefix; }
			set
			{
				SetNonPersistentPropertyValue(PrefixInfo, ref fPrefix, value);

				if (!IsValidationSuspended)
				{
					ValidatePrefix();
				}
			}
		}

		public ZPropertyInfo PrefixInfo
		{
			get { return GetZPropertyInfo(Schema.Prefix); }
		}

		void ValidatePrefix()
		{
			PrefixInfo.ClearAllNotifications();

			if (Prefix.Length > 3)
			{
				PrefixInfo.AddError(ResString.GetMultilingualString("9E3E1E06-9419-49AC-AAFA-F220FB2B46BE", "Prefix length should be less than 3."));
			}

			if (!Prefix.IsEmpty && ParentCollection.Cast<JournalEntriesClassificationGroupCode>().Any(x => x.Prefix == Prefix && x.PK != PK))
			{
				PrefixInfo.AddError(ResString.GetMultilingualString("04C6A90A-F00A-4112-9152-2CAC4BAE0769", "Prefix should be unique."));
			}
		}

		#endregion

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(JournalEntriesClassificationGroupCodeCollection)) ?? new JournalEntriesClassificationGroupCodeCollection(CurrentFallbackLevel); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesClassificationGroupCode(fallbackLevel);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.Prefix, Prefix);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Description = reader.ReadElementString(Schema.Description);
			Prefix = reader.ReadElementString(Schema.Prefix);
		}

		#endregion
	}
}
