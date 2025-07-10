using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SalutationHelperTest : TestCaseWithFactory
	{
		public void TestMaleFemaleShouldReferenceSameSalutation()
		{
			var expectedAllMaleFemaleSalutations = OrganisationsDataRegistry.Instance.ContactSalutation.Value.Cast<ContactSalutation>();
			var expectedAllSalutations = expectedAllMaleFemaleSalutations.Select(s => s.RawSalutation).ToArray();
			var expectedMaleSalutations = expectedAllMaleFemaleSalutations.Where(s => s.Gender == Constants.SalutationGenders.Man || s.Gender == Constants.SalutationGenders.All).Select(s => s.RawSalutation).ToArray();
			var expectedFemaleSalutations = expectedAllMaleFemaleSalutations.Where(s => s.Gender == Constants.SalutationGenders.Woman || s.Gender == Constants.SalutationGenders.All).Select(s => s.RawSalutation).ToArray();

			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements(expectedMaleSalutations, SalutationHelper.GetSalutations("M").ToArray());
				AssertArrayEqualsByElements(expectedFemaleSalutations, SalutationHelper.GetSalutations("F").ToArray());
				AssertArrayEqualsByElements(expectedAllSalutations, SalutationHelper.GetSalutations("O").ToArray());
				AssertArrayEqualsByElements(expectedAllSalutations, SalutationHelper.GetSalutations("N").ToArray());
				AssertArrayEqualsByElements(expectedAllSalutations, SalutationHelper.GetSalutations("NOTEXIST").ToArray());
			});
		}

		public void TestSalutionsWithDuplicatesReturnDistinctSalutation()
		{
			var expectedAllMaleFemaleSalutations = OrganisationsDataRegistry.Instance.ContactSalutation.Value.Cast<ContactSalutation>();
			var expectedAllSalutations = expectedAllMaleFemaleSalutations.Select(s => s.RawSalutation).ToArray();
			var expectedMaleSalutations = expectedAllMaleFemaleSalutations.Where(s => s.Gender == Constants.SalutationGenders.Man || s.Gender == Constants.SalutationGenders.All).ToArray();
			var expectedFemaleSalutations = expectedAllMaleFemaleSalutations.Where(s => s.Gender == Constants.SalutationGenders.Woman || s.Gender == Constants.SalutationGenders.All).ToArray();

			OrganisationsDataRegistry.Instance.ContactSalutation.Value.Add(expectedMaleSalutations[0]);
			OrganisationsDataRegistry.Instance.ContactSalutation.Value.Add(expectedFemaleSalutations[0]);

			AssertArrayEqualsByElements(expectedMaleSalutations.Select(s => s.RawSalutation).ToArray(), SalutationHelper.GetSalutations("M").ToArray());
			AssertArrayEqualsByElements(expectedFemaleSalutations.Select(s => s.RawSalutation).ToArray(), SalutationHelper.GetSalutations("F").ToArray());
		}
	}
}
