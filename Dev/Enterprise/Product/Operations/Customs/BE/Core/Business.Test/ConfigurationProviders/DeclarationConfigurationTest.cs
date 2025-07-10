using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(DeclarationConfiguration))]
sealed class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EU.Business.EntryHeaderConfiguration, EU.Business.EntryLineConfiguration>
{
	public void TestUseEucdmSupportingDocumentGoodsShipmentAndItem()
	{
		AssertEquals(true, configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem);
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
		CombineAssertions(() =>
		{
			declaration.SetImport();
			AssertEquals("Import", true, configuration.DV1DetailsSupport(declaration));
			declaration.SetExport();
			AssertEquals("Export", false, configuration.DV1DetailsSupport(declaration));
		});
	}

	public override void TestLockNumberOfEntryLinesForRegisteredEntry()
	{
		AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
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

	public override void TestUCCAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.UCCAdditionalInfosSupport(declaration));
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

	JobDeclaration declaration;
}
