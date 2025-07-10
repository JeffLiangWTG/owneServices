using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CustomsOfficeRequirementTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestOfficeRoleList()
		{
			var requirement = new CustomsOfficeRequirement();
			NUnit.Framework.Assert.That(requirement.OfficeRoleList, NUnit.Framework.Is.TypeOf<EuOfficeCodesTypes>());
		}

		[ExpectNoExceptions]
		public void TestEquality()
		{
			var first = new CustomsOfficeRequirement
			{
				OfficeRole = EuOfficeCodesTypes.Codes.ActualExitOffice,
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit },
				IsMandatory = true,
				IsLocalCountryOnly = true,
				IsRecommended = true,
				FriendlyName = "AEC office",
				MaxOfficeCountLimit = 9
			};

			var second = new CustomsOfficeRequirement
			{
				OfficeRole = EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep,
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport },
				IsMandatory = false,
				IsLocalCountryOnly = false,
				IsRecommended = false,
				FriendlyName = "CAU office",
				MaxOfficeCountLimit = 1
			};

			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.OfficeRole = EuOfficeCodesTypes.Codes.ActualExitOffice;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit };
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.IsMandatory = true;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.IsLocalCountryOnly = true;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.IsRecommended = true;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.FriendlyName = "AEC office";
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.MaxOfficeCountLimit = 9;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			first.IsForeignCountryOnly = true;
			NUnit.Framework.Assert.That(second, NUnit.Framework.Is.Not.EqualTo(first));

			second.IsForeignCountryOnly = true;
			AssertCustomsOfficeRequirementEquals(first, second);

			first.OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland };
			second.OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland };
			AssertCustomsOfficeRequirementEquals(first, second);
		}

		[ExpectNoExceptions]
		public void TestFriendlyName()
		{
			var requirement = new CustomsOfficeRequirement();
			requirement.OfficeRole = EuOfficeCodesTypes.Codes.ActualExitOffice;
			NUnit.Framework.Assert.That(requirement.FriendlyName, NUnit.Framework.Is.EqualTo("Actual Exit Office").Using(CustomComparers.TypeComparison));
			requirement.OfficeRole = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
			NUnit.Framework.Assert.That(requirement.FriendlyName, NUnit.Framework.Is.EqualTo("Office of Departure").Using(CustomComparers.TypeComparison));
			requirement.OfficeRole = ZString.Empty;
			NUnit.Framework.Assert.That(requirement.FriendlyName, NUnit.Framework.Is.EqualTo("Customs Office").Using(CustomComparers.TypeComparison));
			requirement.FriendlyName = "Override";
			NUnit.Framework.Assert.That(requirement.FriendlyName, NUnit.Framework.Is.EqualTo("Override").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOfficeRoleForLookup()
		{
			CombineAssertions(() =>
			{
				var requirement = new CustomsOfficeRequirement();
				NUnit.Framework.Assert.That(requirement.OfficeRolesForLookup.First(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The default OfficeRoleForLookup is empty.");

				requirement.OfficeRole = EuOfficeCodesTypes.Codes.ActualExitOffice;
				NUnit.Framework.Assert.That(requirement.OfficeRolesForLookup.First(), NUnit.Framework.Is.EqualTo(EuOfficeCodesTypes.Codes.ActualExitOffice).Using(CustomComparers.TypeComparison), "If OfficeRoleForLookup is empty, it returns Office.");

				requirement.OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfDeparture };
				NUnit.Framework.Assert.That(requirement.OfficeRolesForLookup.First(), NUnit.Framework.Is.EqualTo(EuOfficeCodesTypes.Codes.OfficeOfDeparture).Using(CustomComparers.TypeComparison), "If OfficeRoleForLookup is not empty, it returns itself.");
				NUnit.Framework.Assert.That(requirement.OfficeRole, NUnit.Framework.Is.EqualTo(EuOfficeCodesTypes.Codes.ActualExitOffice).Using(CustomComparers.TypeComparison), "Setting OfficeRoleForLookup won't affect OfficeRole");
			});
		}

		[ExpectNoExceptions]
		public void TestConstructors()
		{
			var first = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, true, false);
			var second = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, true, false, false);
			var third = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, true, false, false, true);

			AssertCustomsOfficeRequirementEquals(first, second);
			AssertCustomsOfficeRequirementEquals(second, third);

			first = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, true, false, true, false);
			NUnit.Framework.Assert.That(first.IsForeignCountryOnly, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsForeignCountryOnly");
			NUnit.Framework.Assert.That(!first.IsRecommended, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsRecommended");
		}

		[ExpectNoExceptions]
		public void TestMaxOfficeCountLimitDefaultValue()
		{
			var requirement = new CustomsOfficeRequirement();
			NUnit.Framework.Assert.That(requirement.MaxOfficeCountLimit, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "MaxOfficeCountLimit default value");
		}

		public void TestMaxOfficeNonZeroAndNonNegative()
		{
			var requirement = new CustomsOfficeRequirement();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>("Should throw exception when MaxOfficeCountLimit is zero", () => requirement.MaxOfficeCountLimit = 0);
				AssertExceptionThrown<ArgumentOutOfRangeException>("Should throw exception when MaxOfficeCountLimit is negative", () => requirement.MaxOfficeCountLimit = -1);
			});
		}

		[ExpectNoExceptions]
		void AssertCustomsOfficeRequirementEquals(CustomsOfficeRequirement first, CustomsOfficeRequirement second)
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(second.GetType(), NUnit.Framework.Is.EqualTo(first.GetType()), "Type");
				NUnit.Framework.Assert.That(second.OfficeRole, NUnit.Framework.Is.EqualTo(first.OfficeRole), "Office Role");
				NUnit.Framework.Assert.That(second.OfficeRolesForLookup.First(), NUnit.Framework.Is.EqualTo(first.OfficeRolesForLookup.First()), "Office Roles For Lookup");
				NUnit.Framework.Assert.That(second.IsMandatory, NUnit.Framework.Is.EqualTo(first.IsMandatory), "Is Mandatory");
				NUnit.Framework.Assert.That(second.IsLocalCountryOnly, NUnit.Framework.Is.EqualTo(first.IsLocalCountryOnly), "Is Local Country Only");
				NUnit.Framework.Assert.That(second.FriendlyName, NUnit.Framework.Is.EqualTo(first.FriendlyName), "Friendly Name");
				NUnit.Framework.Assert.That(second.IsForeignCountryOnly, NUnit.Framework.Is.EqualTo(first.IsForeignCountryOnly), "Is Foreign Country Only");
				NUnit.Framework.Assert.That(second.IsRecommended, NUnit.Framework.Is.EqualTo(first.IsRecommended), "Is Recommended");
				NUnit.Framework.Assert.That(second.MaxOfficeCountLimit, NUnit.Framework.Is.EqualTo(first.MaxOfficeCountLimit), "Max Office Count Limit");
			});
		}
	}
}
