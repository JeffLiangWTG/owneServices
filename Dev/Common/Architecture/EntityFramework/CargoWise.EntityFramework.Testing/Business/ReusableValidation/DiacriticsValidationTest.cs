using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DiacriticsValidationTest : TestCaseWithDummy
	{
		public void TestErrorIfContainsAnyDiacritics()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any errors", false, Dummy.HasErrors);

				Dummy.Z0_Description = "A normal string";
				DiacriticsValidation.ErrorIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(false, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertNoErrors(Dummy.Z0_DescriptionInfo);
				});

				Dummy.Z0_Description = "À strïng with dïácrïtïcs";
				DiacriticsValidation.ErrorIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(true, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertHasError(Dummy.Z0_DescriptionInfo, ExpectedNotificationMessage);
				});
			}
		}

		public void TestMessageErrorIfContainsAnyDiacritics()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any errors", false, Dummy.HasErrors);

				Dummy.Z0_Description = "A normal string";
				DiacriticsValidation.MessageErrorIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(false, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertNoMessageErrors(Dummy.Z0_DescriptionInfo);
				});

				Dummy.Z0_Description = "À strïng with dïácrïtïcs";
				DiacriticsValidation.MessageErrorIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(true, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertHasMessageError(Dummy.Z0_DescriptionInfo, ExpectedNotificationMessage);
				});
			}
		}

		public void TestWarnIfContainsAnyDiacritics()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals("Precondition - Dummy should not have any errors", false, Dummy.HasErrors);

				Dummy.Z0_Description = "A normal string";
				DiacriticsValidation.WarnIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(false, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertNoWarnings(Dummy.Z0_DescriptionInfo);
				});

				Dummy.Z0_Description = "À strïng with dïácrïtïcs";
				DiacriticsValidation.WarnIfContainsAnyDiacritics(Dummy.Z0_DescriptionInfo);

				CombineAssertions(() =>
				{
					AssertEquals(true, Dummy.Z0_Description.ContainsAnyDiacritics);
					AssertHasWarning(Dummy.Z0_DescriptionInfo, ExpectedNotificationMessage);
				});
			}
		}

		#region Implementation

		ZString ExpectedNotificationMessage => "Description should not contain any characters with diacritics.";

		#endregion
	}
}
