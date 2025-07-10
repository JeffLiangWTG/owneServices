using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersonMergePreviewItemCollection))]
	sealed class PersonMergePreviewItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PersonMergePreviewItemCollection>
	{
		public void TestDefaultValue()
		{
			var defaultCollection = PersonMergePreviewItemCollection.DefaultValue;
			var expectedCollection = new PersonMergePreviewItemCollection()
			{
				new PersonMergePreviewItem() { FriendlyName = "Full Name", ColumnName = GlbPersonSchema.PER_FullName.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Friendly Name", ColumnName = GlbPersonSchema.PER_FriendlyName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Legal Name", ColumnName = GlbPersonSchema.PER_LegalName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Name Suffix", ColumnName = GlbPersonSchema.PER_NameSuffix.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Name Title", ColumnName = GlbPersonSchema.PER_NameTitle.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Gender", ColumnName = GlbPersonSchema.PER_Gender.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Birth Date", ColumnName = GlbPersonSchema.PER_BirthDate.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Drivers License Number", ColumnName = GlbPersonSchema.PER_DriversLicenseNumber.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Personal Info", ColumnName = GlbPersonSchema.PER_PersonalInfo.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Picture", ColumnName = GlbPersonSchema.PER_Picture.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Preferred Language", ColumnName = GlbPersonSchema.PER_PreferredLanguage.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Nationality Code", ColumnName = GlbPersonSchema.PER_RN_NKNationalityCodeISO.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Home Address 1", ColumnName = GlbPersonSchema.PER_HomeAddress1.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Home Address 2", ColumnName = GlbPersonSchema.PER_HomeAddress2.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "City", ColumnName = GlbPersonSchema.PER_City.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "State", ColumnName = GlbPersonSchema.PER_State.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Postcode", ColumnName = GlbPersonSchema.PER_Postcode.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Country/Region", ColumnName = GlbPersonSchema.PER_RN_NKCountry.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Home Phone", ColumnName = GlbPersonSchema.PER_HomePhone.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Fax Number", ColumnName = GlbPersonSchema.PER_FaxNumber.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone", ColumnName = GlbPersonSchema.PER_MobilePhone.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone 2", ColumnName = GlbPersonSchema.PER_MobilePhone2.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Email Address", ColumnName = GlbPersonSchema.PER_EmailAddress.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Email Address 2", ColumnName = GlbPersonSchema.PER_EmailAddress2.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Passport", ColumnName = GlbPersonSchema.PER_Passport.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Passport Expiry Date", ColumnName = GlbPersonSchema.PER_PassportExpiryDate.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Passport Place Of Issue", ColumnName = GlbPersonSchema.PER_PassportPlaceOfIssue.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase", ColumnName = GlbPersonSchema.PER_ChallengePhrase.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase Type", ColumnName = GlbPersonSchema.PER_ChallengePhraseType.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Password Hash", ColumnName = GlbPersonSchema.PER_PasswordHash.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Password Hash Iterations", ColumnName = GlbPersonSchema.PER_PasswordHashIterations.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Password Salt", ColumnName = GlbPersonSchema.PER_PasswordSalt.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Web Access Enabled", ColumnName = GlbPersonSchema.PER_WebAccessEnabled.Name, Visibility = true }
			};

			AssertEquals(expectedCollection.Count, defaultCollection.Count);
			foreach (var item in expectedCollection.Cast<PersonMergePreviewItem>())
			{
				AssertEquals(true, defaultCollection.Cast<PersonMergePreviewItem>().Any(t => t.FriendlyName.ToString() == item.FriendlyName.ToString() && t.ColumnName.ToString() == item.ColumnName.ToString() && t.Visibility == item.Visibility));
			}
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override PersonMergePreviewItemCollection GetCollectionToTest()
		{
			return new PersonMergePreviewItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PersonMergePreviewItem();
		}

		#endregion

	}
}
