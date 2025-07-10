using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCacheKeyCombination()
		{
			NUnit.Framework.Assert.That(officeHelper.CacheKeyCombination, NUnit.Framework.Is.EqualTo("CustomsOfficeRequirementHelper"));
		}

		[ExpectNoExceptions]
		public void TestMainOffice()
		{
			NUnit.Framework.Assert.That(officeHelper.MainOffice, NUnit.Framework.Is.EqualTo(default(CustomsOfficeRequirement)));
		}

		[ExpectNoExceptions]
		public void TestOtherRequirements()
		{
			NUnit.Framework.Assert.That(officeHelper.OtherRequirements.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestGetOtherRequirementByRole()
		{
			const string availableRole = "B";
			var additionalRequirement = new CustomsOfficeRequirement("A", true, false);
			var availableRequirement = new CustomsOfficeRequirement(availableRole, true, false);
			officeHelper.OtherRequirementsToTest = new[] { additionalRequirement, availableRequirement };

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(officeHelper.GetOtherRequirementByRole(availableRole), NUnit.Framework.Is.Not.EqualTo(default(CustomsOfficeRequirement)), "Available Role - should not be [null]");
				NUnit.Framework.Assert.That(officeHelper.GetOtherRequirementByRole("UnavailableRole"), NUnit.Framework.Is.EqualTo(default(CustomsOfficeRequirement)), "Unavailable Role - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestValidationMessage()
		{
			var standardValidationMessageRequirement = new CustomsOfficeRequirement("A", true, false);
			var specifiedValidationMessageRequirement = new CustomsOfficeRequirement("B", true, false) { ValidationMessage = "ABC" };
			officeHelper.OtherRequirementsToTest = new[] { standardValidationMessageRequirement, specifiedValidationMessageRequirement };

			var validationMessges = officeHelper.Validate().ToArray();

			NUnit.Framework.Assert.That(validationMessges.Length, NUnit.Framework.Is.EqualTo(2), "Precondition - number of messages");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(validationMessges[0], NUnit.Framework.Is.EqualTo("The declaration requires an office of type Customs Office with purpose A."), "Standard validation message");
				NUnit.Framework.Assert.That(validationMessges[1], NUnit.Framework.Is.EqualTo("ABC"), "Specified validation message");
			});
		}

		public static void AssertCustomsOfficeRequirementEquals(ZString shortComment, CustomsOfficeRequirement first, CustomsOfficeRequirement second)
		{
			AssertNotNull($"{shortComment}: First Custom Office Requirement", first);
			AssertNotNull($"{shortComment}: Second Custom Office Requirement", second);
			CombineAssertions(() =>
			{
				AssertEquals(shortComment + "Type", first.GetType(), second.GetType());
				AssertEquals(shortComment + "Office Role", first.OfficeRole, second.OfficeRole);
				AssertContainsExactElementsInAnyOrder(shortComment + "Office Roles For Lookup", first.OfficeRolesForLookup, second.OfficeRolesForLookup);
				AssertEquals(shortComment + "Is Mandatory", first.IsMandatory, second.IsMandatory);
				AssertEquals(shortComment + "Is Local Country Only", first.IsLocalCountryOnly, second.IsLocalCountryOnly);
				AssertEquals(shortComment + "Friendly Name", first.FriendlyName, second.FriendlyName);
				AssertEquals(shortComment + "Max Office Count Limit", first.MaxOfficeCountLimit, second.MaxOfficeCountLimit);
				AssertEquals(shortComment + "Is Foreign Country Only", first.IsForeignCountryOnly, second.IsForeignCountryOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			officeHelper = new CustomsOfficeRequirementHelperForTest(declaration);
		}
		CustomsOfficeRequirementHelperForTest officeHelper;

		class CustomsOfficeRequirementHelperForTest : CustomsOfficeRequirementHelper
		{
			public IEnumerable<CustomsOfficeRequirement> OtherRequirementsToTest { get; set; }

			public CustomsOfficeRequirementHelperForTest(IEuOfficeCodeProvider declaration)
				: base(declaration)
			{
			}

			protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => OtherRequirementsToTest ?? base.GetOtherRequirements();
		}
	}
}
