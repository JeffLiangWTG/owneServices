using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PersonMergePreviewItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PersonMergePreviewItemCollection() : base()
		{
		}

		public PersonMergePreviewItemCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new PersonMergePreviewItem this[int i]
		{
			get { return (PersonMergePreviewItem)Elements[i]; }
		}

		public new PersonMergePreviewItem AddNew()
		{
			return (PersonMergePreviewItem)base.AddNew();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowSort => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PersonMergePreviewItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PersonMergePreviewItemCollection();
		}

		public static PersonMergePreviewItemCollection DefaultValue => new PersonMergePreviewItemCollection()
		{
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Full Name", ColumnName = GlbPersonSchema.PER_FullName.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Friendly Name", ColumnName = GlbPersonSchema.PER_FriendlyName.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Legal Name", ColumnName = GlbPersonSchema.PER_LegalName.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Name Suffix", ColumnName = GlbPersonSchema.PER_NameSuffix.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Name Title", ColumnName = GlbPersonSchema.PER_NameTitle.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Gender", ColumnName = GlbPersonSchema.PER_Gender.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Birth Date", ColumnName = GlbPersonSchema.PER_BirthDate.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Drivers License Number", ColumnName = GlbPersonSchema.PER_DriversLicenseNumber.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Personal Info", ColumnName = GlbPersonSchema.PER_PersonalInfo.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Picture", ColumnName = GlbPersonSchema.PER_Picture.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Preferred Language", ColumnName = GlbPersonSchema.PER_PreferredLanguage.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Nationality Code", ColumnName = GlbPersonSchema.PER_RN_NKNationalityCodeISO.Name, Visibility = true },

			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Home Address 1", ColumnName = GlbPersonSchema.PER_HomeAddress1.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Home Address 2", ColumnName = GlbPersonSchema.PER_HomeAddress2.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"City", ColumnName = GlbPersonSchema.PER_City.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"State", ColumnName = GlbPersonSchema.PER_State.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Postcode", ColumnName = GlbPersonSchema.PER_Postcode.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Country/Region", ColumnName = GlbPersonSchema.PER_RN_NKCountry.Name, Visibility = true },

			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Home Phone", ColumnName = GlbPersonSchema.PER_HomePhone.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Fax Number", ColumnName = GlbPersonSchema.PER_FaxNumber.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Mobile Phone", ColumnName = GlbPersonSchema.PER_MobilePhone.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Mobile Phone 2", ColumnName = GlbPersonSchema.PER_MobilePhone2.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Email Address", ColumnName = GlbPersonSchema.PER_EmailAddress.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Email Address 2", ColumnName = GlbPersonSchema.PER_EmailAddress2.Name, Visibility = true },

			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Passport", ColumnName = GlbPersonSchema.PER_Passport.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Passport Expiry Date", ColumnName = GlbPersonSchema.PER_PassportExpiryDate.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Passport Place Of Issue", ColumnName = GlbPersonSchema.PER_PassportPlaceOfIssue.Name, Visibility = true },

			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Challenge Phrase", ColumnName = GlbPersonSchema.PER_ChallengePhrase.Name, Visibility = false },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Challenge Phrase Type", ColumnName = GlbPersonSchema.PER_ChallengePhraseType.Name, Visibility = false },

			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Password Hash", ColumnName = GlbPersonSchema.PER_PasswordHash.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Password Hash Iterations", ColumnName = GlbPersonSchema.PER_PasswordHashIterations.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Password Salt", ColumnName = GlbPersonSchema.PER_PasswordSalt.Name, Visibility = true },
			new PersonMergePreviewItem() { FriendlyName = (NoResString)"Web Access Enabled", ColumnName = GlbPersonSchema.PER_WebAccessEnabled.Name, Visibility = true }
		};
	}
}
