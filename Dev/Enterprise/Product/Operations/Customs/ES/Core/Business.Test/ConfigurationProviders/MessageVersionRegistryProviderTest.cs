using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Registry;

namespace Enterprise.Customs.ES.Business.Testing;

sealed class MessageVersionRegistryProviderTest : TestCaseWithFactory
{
	public void TestIsExportVersionAes()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var isExportVersionAes = MessageVersionRegistryProvider.IsExportVersionAes();
				AssertEquals("IsExportVersionAes when ESExportMessageVersion is AES", true, isExportVersionAes);
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				var isExportVersionAes = MessageVersionRegistryProvider.IsExportVersionAes();
				AssertEquals("IsExportVersionAes when ESExportMessageVersion is AES11", false, isExportVersionAes);
			}
		});
	}

	public void TestIsExportVersionAes11()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				var isExportVersionAes11 = MessageVersionRegistryProvider.IsExportVersionAes11();
				AssertEquals("IsExportVersionAes11 when ESExportMessageVersion is AES11", true, isExportVersionAes11);
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var isExportVersionAes11 = MessageVersionRegistryProvider.IsExportVersionAes11();
				AssertEquals("IsExportVersionAes11 when ESExportMessageVersion is AES", false, isExportVersionAes11);
			}
		});
	}

	public void TestIsExportAndAnyVersionAes()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var isExportAndAnyVersionAes = MessageVersionRegistryProvider.IsExportAndAnyVersionAes();
				AssertEquals("IsExportAndAnyVersionAes when CustomsMessageVersion is AES", true, isExportAndAnyVersionAes);
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				var isExportAndAnyVersionAes = MessageVersionRegistryProvider.IsExportAndAnyVersionAes();
				AssertEquals("IsExportAndAnyVersionAes when CustomsMessageVersion is AES11", true, isExportAndAnyVersionAes);
			}
		});
	}

	public void TestIsT2LVersionPOUS()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				var isT2LVersionPOUS = MessageVersionRegistryProvider.IsT2LVersionPOUS();
				AssertEquals("isT2LVersionPOUS when EST2LMessageVersion is POUS2", false, isT2LVersionPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				var isT2LVersionPOUS = MessageVersionRegistryProvider.IsT2LVersionPOUS();
				AssertEquals("isT2LVersionPOUS when EST2LMessageVersion is POUS", true, isT2LVersionPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				var isT2LVersionPOUS = MessageVersionRegistryProvider.IsT2LVersionPOUS();
				AssertEquals("isT2LVersionPOUS when EST2LMessageVersion is NoPOUS", false, isT2LVersionPOUS);
			}
		});
	}

	public void TestIsT2LVersionPOUS2()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				var isT2LVersionPOUS2 = MessageVersionRegistryProvider.IsT2LVersionPOUS2();
				AssertEquals("isT2LVersionPOUS2 when EST2LMessageVersion is POUS", false, isT2LVersionPOUS2);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				var isT2LVersionPOUS2 = MessageVersionRegistryProvider.IsT2LVersionPOUS2();
				AssertEquals("isT2LVersionPOUS2 when EST2LMessageVersion is POUS2", true, isT2LVersionPOUS2);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				var isT2LVersionPOUS2 = MessageVersionRegistryProvider.IsT2LVersionPOUS2();
				AssertEquals("isT2LVersionPOUS2 when EST2LMessageVersion is NoPOUS", false, isT2LVersionPOUS2);
			}
		});
	}

	public void TestIsT2LAndAnyVersionPOUS()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				var isT2LAndAnyVersionPOUS = MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS();
				AssertEquals("isT2LAndAnyVersionPOUS when EST2LMessageVersion is POUS2", true, isT2LAndAnyVersionPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				var isT2LAndAnyVersionPOUS = MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS();
				AssertEquals("isT2LAndAnyVersionPOUS when EST2LMessageVersion is NoPOUS", false, isT2LAndAnyVersionPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				var isT2LAndAnyVersionPOUS = MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS();
				AssertEquals("isT2LAndAnyVersionPOUS when EST2LMessageVersion is POUS", true, isT2LAndAnyVersionPOUS);
			}
		});
	}

	public void TestIsT2LVersionNoPOUS()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				var isT2LVersionNoPOUS = MessageVersionRegistryProvider.IsT2LVersionNoPOUS();
				AssertEquals("isT2LVersionNoPOUS when EST2LMessageVersion is POUS2", false, isT2LVersionNoPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				var isT2LVersionNoPOUS = MessageVersionRegistryProvider.IsT2LVersionNoPOUS();
				AssertEquals("isT2LVersionNoPOUS when EST2LMessageVersion is NoPOUS", true, isT2LVersionNoPOUS);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				var isT2LVersionNoPOUS = MessageVersionRegistryProvider.IsT2LVersionNoPOUS();
				AssertEquals("isT2LVersionNoPOUS when EST2LMessageVersion is POUS", false, isT2LVersionNoPOUS);
			}
		});
	}

	public void TestIsImportVersionICS()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.Ics))
			{
				var isImportVersionICS = MessageVersionRegistryProvider.IsImportVersionICS();
				AssertEquals("isImportVersionICS when ESImportMessageVersion is ICS", true, isImportVersionICS);
			}

			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.H1))
			{
				var isImportVersionICS = MessageVersionRegistryProvider.IsImportVersionICS();
				AssertEquals("isImportVersionICS when ESImportMessageVersion is H1", false, isImportVersionICS);
			}
		});
	}

	public void TestIsImportVersionH1()
	{
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.H1))
			{
				var isImportVersionH1 = MessageVersionRegistryProvider.IsImportVersionH1();
				AssertEquals("isImportVersionH1 when ESImportMessageVersion is H1", true, isImportVersionH1);
			}

			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.Ics))
			{
				var isImportVersionH1 = MessageVersionRegistryProvider.IsImportVersionH1();
				AssertEquals("isImportVersionH1 when ESImportMessageVersion is ICS", false, isImportVersionH1);
			}
		});
	}
}
