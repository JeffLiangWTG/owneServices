using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContactSalutationCollection))]
	sealed class ContactSalutationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ContactSalutationCollection>
	{
		public new void TestClone()
		{
			FallbackLevel currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			ContactSalutationCollection collection = new ContactSalutationCollection();
			ContactSalutation origin1 = collection.AddNew();
			origin1.EnglishSalutation = "Dear";
			origin1.Gender = "Male";

			ContactSalutation origin2 = collection.AddNew();
			origin2.EnglishSalutation = "Dearest";
			origin1.Gender = "Female";

			ContactSalutationCollection clone = (ContactSalutationCollection)collection.Clone(currentFallbackLevel, Factory);

			Assert("Clone should be a different instance.", collection != clone);
			Assert("Clone's elements should be different instances from the original's.", clone[0] != origin1);
			Assert("Clone's elements should be different instances from the original's.", clone[1] != origin2);
			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(ContactSalutationCollection)).GetValue(clone, null));

			ContactSalutation cloneElement1 = clone[0];
			ContactSalutation cloneElement2 = clone[1];

			AssertEquals("Clone[0].Salutation", origin1.Salutation, cloneElement1.Salutation);
			AssertEquals("Clone[0].Gender", origin1.Gender, cloneElement1.Gender);

			AssertEquals("Clone[1].Salutation", origin2.Salutation, cloneElement2.Salutation);
			AssertEquals("Clone[1].Gender", origin2.Gender, cloneElement2.Gender);
		}

		public void TestSystemDefaultSalutationLengthAreAllValid()
		{
			CombineAssertions(delegate
			{
				var salutations = new ContactSalutationCollection();
				SalutationHelper.LoadDefaultSalutations(salutations);
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					using (Res.TemporarilySwitchLanguage(language))
					{
						foreach (ContactSalutation salutation in salutations)
						{
							Assert(string.Format("The length of salutation which contains {0} in language {1} should be no longer than 50 chars", salutation.Salutation.ToString(), language), salutation.Salutation.ToString().Length <= salutation.SalutationInfo.MaxLength);
						}
					}
				}
			});
		}

		public void TestSystemDefaultSalutationMarkupsAreCorrect()
		{
			CombineAssertions(delegate
			{
				var salutations = new ContactSalutationCollection();
				SalutationHelper.LoadDefaultSalutations(salutations);
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					using (Res.TemporarilySwitchLanguage(language))
					{
						if (language != Res.DefaultLanguage)
						{
							foreach (ContactSalutation salutation in salutations)
							{
								if (salutation.RawSalutation.GetUnresolvedString().StartsWith("[m] "))
								{
									AssertStartsWith(string.Format("Translation of '{0}' in {1} does not start with gender markup [m]", salutation.RawSalutation.GetUnresolvedString(), language), "[m] ", salutation.RawSalutation.ToString(language));
								}
								else if (salutation.RawSalutation.GetUnresolvedString().StartsWith("[f] "))
								{
									AssertStartsWith(string.Format("Translation of '{0}' in {1} does not start with gender markup [f]", salutation.RawSalutation.GetUnresolvedString(), language), "[f] ", salutation.RawSalutation.ToString(language));
								}
								if (salutation.RawSalutation.GetUnresolvedString().Contains(Constants.SalutationMacros.Name))
								{
									AssertContains(string.Format("Translation of '{0}' in {1} does not contain macro [Name]", salutation.RawSalutation.GetUnresolvedString(), language), "Name", salutation.RawSalutation.ToString(language));
								}
								if (salutation.RawSalutation.GetUnresolvedString().Contains(Constants.SalutationMacros.JobCategory))
								{
									AssertContains(string.Format("Translation of '{0}' in {1} does not contain macro [JobCategory]", salutation.RawSalutation.GetUnresolvedString(), language), "JobCategory", salutation.RawSalutation.ToString(language));
								}
							}
						}
					}
				}
			});
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ContactSalutationCollection GetCollectionToTest()
		{
			return new ContactSalutationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContactSalutation();
		}

		#endregion
	}
}
