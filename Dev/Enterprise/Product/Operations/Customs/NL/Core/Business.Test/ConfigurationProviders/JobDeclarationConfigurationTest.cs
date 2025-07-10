using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;
using JobDeclaration = Enterprise.Customs.NL.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(DeclarationConfiguration))]
sealed class DeclarationConfigurationTest : DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
{
	public override void TestUCCAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.UCCAdditionalInfosSupport(declaration));
	}
	public override void TestMiscAdditionalInfosSupport()
	{
		AssertEquals(false, configuration.MiscAdditionalInfosSupport(declaration));
	}

	public override void TestMiscSupportingDocumentsSupport()
	{
		AssertEquals(false, configuration.MiscSupportingDocumentsSupport(declaration));
	}

	public override void TestMiscPreviousDocumentsSupport()
	{
		AssertEquals(false, configuration.MiscPreviousDocumentsSupport(declaration));
	}

	public override void TestMiscGuaranteesSupport()
	{
		AssertEquals(false, configuration.MiscGuaranteesSupport(declaration));
	}

	public override void TestUseUniversalFeeCalculation()
	{
		AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));
	}

	public override void TestDV1DetailsSupport()
	{
		AssertEquals(false, configuration.DV1DetailsSupport(declaration));
	}

	public override void TestLockNumberOfEntryLinesForRegisteredEntry()
	{
		AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
	}

	public void TestInvoiceLineConfigurationType()
	{
		AssertType<InvoiceLineConfiguration>(configuration.InvoiceLineConfiguration);
	}

	public override void TestIsUCC5()
	{
		AssertEquals(false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));
	}

	public override void TestIsUCC6()
	{
		AssertEquals(true, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));
	}

	public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
	{
		AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
	}

	public void TestInstructionConfigurationType()
	{
		AssertType<InstructionConfiguration>(configuration.InstructionConfiguration);
	}

	public override void TestShouldCheckLegalByDeclarantType()
	{
		AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	[ExpectNoExceptions]
	public void TestGetValidationDecider()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ExportDeclarationValidationDecider>(), "UCC6 EXP");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ImportDeclarationValidationDecider>(), "UCC6 IMP");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IDeclarationValidationDecider)), "Not UCC6 EXP - should be [null]");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IDeclarationValidationDecider)), "Not UCC6 IMP - should be [null]");
			}
		});
	}

	JobDeclaration declaration;
}
