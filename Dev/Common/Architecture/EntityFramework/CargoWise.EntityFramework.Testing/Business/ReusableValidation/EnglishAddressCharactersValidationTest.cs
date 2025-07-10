using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class EnglishAddressCharactersValidationTest : TestCaseWithDummy
	{
		public void TestMessageErrorIfNotEnglish()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any message errors", false, Dummy.HasMessageErrors);

				Dummy.Z0_NVarCharMax = "Australian English - Good on ya, mate!";
				EnglishStrictCharactersValidation.MessageErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
				AssertEquals(true, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
				AssertEquals(false, Dummy.Z0_NVarCharMaxInfo.HasMessageErrors());

				var stringsWithDiacritic = new List<string>() { "Stràsse", "Français", "Niña", "Trèma" };
				foreach (var s in stringsWithDiacritic)
				{
					Dummy.Z0_NVarCharMax = s;
					EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
					AssertEquals(false, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
					AssertEquals(true, Dummy.Z0_NVarCharMax.RemoveDiacritics().IsEnglishOnlyOrEmpty);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Length", 0, Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Count());
				}

				var expectedNotificationMessage = "N Var Char Max only accepts English language characters.";
				var nonEnglishStrings = new List<string>() { "Fœtus", "ß-globin", "archæology" };
				foreach (var s in nonEnglishStrings)
				{
					Dummy.Z0_NVarCharMax = s;
					EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
					AssertEquals(false, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
					AssertEquals(false, Dummy.Z0_NVarCharMax.RemoveDiacritics().IsEnglishOnlyOrEmpty);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Length", 1, Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Count());
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetWarnings().GetFirst()", expectedNotificationMessage, Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().GetFirstMessage());
				}
			}
		}
	}
}
