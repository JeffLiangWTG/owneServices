using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.KR.Business
{
	[CodeProperty("Code")]
	[DescriptionProperty("Description")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public sealed class FamilyRelation : RegistryBusinessObjectTemplate
	{
		public FamilyRelation() { }

		public FamilyRelation(FallbackLevel fallbackLevel, BusinessObjectFactory factory, FamilyRelationCollection parentCollection) : base(fallbackLevel, factory)
		{
			this.parentCollection = parentCollection;
		}
		readonly FamilyRelationCollection parentCollection;

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string IsSystemOne = "IsSystemOne";

			public const int CodeMaxLength = 2;
			public const int DescriptionLength = 10;
		}

		#region Code
		[MaxLength(Schema.CodeMaxLength)]
		[ReadOnlyMember(nameof(IsSystemOne))]
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
		ZString fCode;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			if (parentCollection != null)
			{
				foreach (FamilyRelation familyRelation in parentCollection)
				{
					if (familyRelation != this && familyRelation.Code == Code)
					{
						CodeInfo.AddError(string.Format(System.Globalization.CultureInfo.CurrentCulture, DuplicateCode, Code));
						break;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Duplicate code warning message")]
		internal const string DuplicateCode = "You have already entered the code, '{0}'.";
		#endregion

		#region Description
		[MaxLength(Schema.DescriptionLength)]
		[ReadOnlyMember(nameof(IsSystemOne))]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

		public void ValidateDescription()
		{
			if (Description.IsEmpty)
			{
				MandatoryValidation.CheckEntered(DescriptionInfo);
			}
		}
		#endregion

		#region IsSystemOne
		public ZBool IsSystemOne
		{
			get { return fIsSystemOne; }
			set
			{
				SetNonPersistentPropertyValue(IsSystemOneInfo, ref fIsSystemOne, value);
			}
		}
		ZBool fIsSystemOne;

		public ZPropertyInfo IsSystemOneInfo => GetZPropertyInfo(Schema.IsSystemOne);
		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FamilyRelation(fallbackLevel, factory, null);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Description = reader.ReadElementString(Schema.Description);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description);
		}
	}
}
