using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class EnglishStrictCharactersValidationTest : TestCaseWithDummy
	{
		public void TestErrorIfNotEnglish()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any errors", false, Dummy.HasErrors);

				Dummy.Z0_NVarCharMax = "Australian English - Good on ya, mate!";
				EnglishStrictCharactersValidation.ErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
				AssertEquals(true, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
				AssertEquals(false, Dummy.Z0_NVarCharMaxInfo.HasErrors());

				var nonEnglishStrings = new List<string>() { "German - Die Straße", "French - Français", "Spanish - niña", "abc \u069A" };

				foreach (var s in nonEnglishStrings)
				{
					Dummy.Z0_NVarCharMax = s;
					EnglishStrictCharactersValidation.ErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
					AssertEquals(false, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetErrors().Length", 1, Dummy.Z0_NVarCharMaxInfo.GetErrors().GetUniqueMessageList().Length);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetErrors().GetFirst()", ExpectedNotificationMessage, Dummy.Z0_NVarCharMaxInfo.GetErrors().GetFirstMessage());
				}
			}
		}

		public void TestMessageErrorIfNotEnglish()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any message errors", false, Dummy.HasMessageErrors);

				Dummy.Z0_NVarCharMax = "Australian English - Good on ya, mate!";
				EnglishStrictCharactersValidation.MessageErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
				AssertEquals(true, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
				AssertEquals(false, Dummy.Z0_NVarCharMaxInfo.HasMessageErrors());

				var nonEnglishStrings = new List<string>() { "German - Die Straße", "French - Français", "Spanish - niña", "abc \u069A" };

				foreach (var s in nonEnglishStrings)
				{
					Dummy.Z0_NVarCharMax = s;
					EnglishStrictCharactersValidation.MessageErrorIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
					AssertEquals(false, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Length", 1, Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().Count());
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetMessageErrors()[0]", ExpectedNotificationMessage, Dummy.Z0_NVarCharMaxInfo.GetMessageErrors().GetFirstMessage());
				}
			}
		}

		public void TestWarnIfNotEnglish()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any warnings", false, Dummy.HasWarnings);

				Dummy.Z0_NVarCharMax = "Australian English - Good on ya, mate!";
				EnglishStrictCharactersValidation.WarnIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
				AssertEquals(true, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
				AssertEquals(false, Dummy.Z0_NVarCharMaxInfo.HasWarnings());

				var nonEnglishStrings = new List<string>() { "German - Die Straße", "French - Français", "Spanish - niña", "abc \u069A" };

				foreach (var s in nonEnglishStrings)
				{
					Dummy.Z0_NVarCharMax = s;
					EnglishStrictCharactersValidation.WarnIfNotEnglish(Dummy.Z0_NVarCharMaxInfo);
					AssertEquals(false, Dummy.Z0_NVarCharMax.IsEnglishOnlyOrEmpty);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetWarnings().Length", 1, Dummy.Z0_NVarCharMaxInfo.GetWarnings().GetUniqueMessageList().Length);
					AssertEquals("Dummy.Z0_NVarCharMaxInfo.GetWarnings().GetFirst()", ExpectedNotificationMessage, Dummy.Z0_NVarCharMaxInfo.GetWarnings().GetFirstMessage());
				}
			}
		}

		#region Implementation

		ZString ExpectedNotificationMessage
		{
			get { return "N Var Char Max only accepts English language characters."; }
		}

		#endregion
	}
}
