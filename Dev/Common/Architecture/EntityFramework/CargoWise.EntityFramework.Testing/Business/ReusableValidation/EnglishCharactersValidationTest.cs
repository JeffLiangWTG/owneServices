using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class EnglishCharactersValidationTest : TestCaseWithDummy
	{
		public void TestErrorIfNotWesternEuropean()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any errors", false, Dummy.HasErrors);

				Dummy.Z0_Description = "engrish123";
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(true, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals(false, Dummy.Z0_DescriptionInfo.HasErrors());

				Dummy.Z0_Description = "abc \u069A";
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(false, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals("Dummy.Z0_DescriptionInfo.GetErrors().Length", 1, Dummy.Z0_DescriptionInfo.GetErrors().GetUniqueMessageList().Length);
				AssertEquals("Dummy.Z0_DescriptionInfo.GetErrors().GetFirst()", ExpectedNotificationMessage, Dummy.Z0_DescriptionInfo.GetErrors().GetFirstMessage());
			}
		}

		public void TestMessageErrorIfNotWesternEuropean()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any message errors", false, Dummy.HasMessageErrors);

				Dummy.Z0_Description = "engrish123";
				EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(true, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals(false, Dummy.Z0_DescriptionInfo.HasMessageErrors());

				Dummy.Z0_Description = "abc \u069A";
				EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(false, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals("Dummy.Z0_DescriptionInfo.GetMessageErrors().Length", 1, Dummy.Z0_DescriptionInfo.GetMessageErrors().Count());
				AssertEquals("Dummy.Z0_DescriptionInfo.GetMessageErrors()[0]", ExpectedNotificationMessage, Dummy.Z0_DescriptionInfo.GetMessageErrors().GetFirstMessage());
			}
		}

		public void TestWarnIfNotWesternEuropean()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any warnings", false, Dummy.HasWarnings);

				Dummy.Z0_Description = "engrish123";
				EnglishCharactersValidation.WarnIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(true, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals(false, Dummy.Z0_DescriptionInfo.HasWarnings());

				Dummy.Z0_Description = "abc \u069A";
				EnglishCharactersValidation.WarnIfNotWesternEuropean(Dummy.Z0_DescriptionInfo);
				AssertEquals(false, Dummy.Z0_Description.IsWesternEuropeanOrEmpty);
				AssertEquals("Dummy.Z0_DescriptionInfo.GetWarnings().Length", 1, Dummy.Z0_DescriptionInfo.GetWarnings().GetUniqueMessageList().Length);
				AssertEquals("Dummy.Z0_DescriptionInfo.GetWarnings().GetFirst()", ExpectedNotificationMessage, Dummy.Z0_DescriptionInfo.GetWarnings().GetFirstMessage());
			}
		}

		#region Implementation

		ZString ExpectedNotificationMessage
		{
			get { return "Description only accepts Western European languages characters."; }
		}

		#endregion
	}
}
