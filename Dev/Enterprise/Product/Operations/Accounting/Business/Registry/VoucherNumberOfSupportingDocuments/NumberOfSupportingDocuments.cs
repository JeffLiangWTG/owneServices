using System;
using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DefaultNumberOfSupportingDocuments : RegistryBusinessObjectTemplate
	{
		public DefaultNumberOfSupportingDocuments()
		{
		}

		public DefaultNumberOfSupportingDocuments(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema
		public abstract class Schema
		{
			public const string Code = "Code";
			public const string DefaultDescription = "DefaultDescription";
			public const string Description = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const string NumberOfDefault = "NumberOfDefault";
			public const int DescriptionMaxLength = 256;
		}
		#endregion

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultNumberOfSupportingDocuments(fallbackLevel, factory);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.DefaultDescription, DefaultDescription);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.NumberOfDefault, NumberOfDefault.ToString());
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			DefaultDescription = (NoResString)reader.ReadElementString(Schema.DefaultDescription);
			Description = (NoResString)reader.ReadElementString(Schema.Description);
			NumberOfDefault = ZByte.ParseSafe(reader.ReadElementString(Schema.NumberOfDefault), 0);
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDescription();
			ValidateNumberOfDefault();
		}

		#endregion

		#endregion

		#region Bound Properties

		#region AttachmentCode
		[MaxLength(5)]
		[ReadOnly(true)]
		public ZString Code
		{
			get { return attachmentCode; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref attachmentCode, value, false);
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		ZString attachmentCode;
		#endregion

		#region DefaultDescription
		[MaxLength(Schema.DescriptionMaxLength)]
		[ReadOnly(true)]
		public MultilingualString DefaultDescription
		{
			get { return defaultDescription ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				SetNonPersistentPropertyValue(DefaultDescriptionInfo, ref defaultDescription, value, false);
			}
		}

		public ZPropertyInfo DefaultDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultDescription); }
		}

		MultilingualString defaultDescription;
		#endregion

		#region Description        
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
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
				EnglishDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
		}

		[MaxLength(Schema.DescriptionMaxLength)]
		public ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set
			{
				if (EnglishDescription != value)
				{
					CheckMaximumLength(DescriptionInfo, value);
					Description = (NoResString)value;
					DescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		MultilingualString description;

		#endregion

		#region NumberOfDefault        
		public virtual ZByte NumberOfDefault
		{
			get { return numberOfDefault; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfDefaultInfo, ref numberOfDefault, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberOfDefault();
				}
			}
		}

		public ZPropertyInfo NumberOfDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfDefault); }
		}

		void ValidateNumberOfDefault()
		{
			NumberOfDefaultInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(NumberOfDefaultInfo);
		}

		public bool NumberOfDefaultVisible
		{
			get
			{
				ZString companyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Guid fallbackLevelCompanyPK = CurrentFallbackLevel != null ? CurrentFallbackLevel.CompanyPK(true) : Guid.Empty;
				if (fallbackLevelCompanyPK != Guid.Empty && fallbackLevelCompanyPK != GlbCompany.CurrentCompany.PK)
				{
					GlbCompany company = CurrentFactory.Load<GlbCompany>(fallbackLevelCompanyPK);
					if (company != null)
					{
						companyCountryCode = company.GC_RN_NKCountryCode;
					}
				}

				return companyCountryCode == Constants.CountryCodes.China;
			}
		}

		ZByte numberOfDefault;
		#endregion

		#endregion
	}
}
