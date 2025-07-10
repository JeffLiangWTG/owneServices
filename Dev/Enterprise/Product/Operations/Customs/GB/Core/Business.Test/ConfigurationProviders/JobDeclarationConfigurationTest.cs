using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EU.Business.EntryHeaderConfiguration, EntryLineConfiguration>
	{
		public void TestUseEucdmSupportingDocumentGoodsShipment()
		{
			AssertEquals(false, configuration.UseEucdmSupportingDocumentGoodsShipment);
		}

		public override void TestUCCAdditionalInfosSupport()
		{
			AssertEquals("Disabled", false, configuration.UCCAdditionalInfosSupport(declaration));
		}

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.MiscAdditionalInfosSupport(declaration));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscSupportingDocumentsSupport(declaration));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscPreviousDocumentsSupport(declaration));
		}

		public override void TestMiscGuaranteesSupport()
		{
			AssertEquals(true, configuration.MiscGuaranteesSupport(declaration));
		}

		public override void TestUseUniversalFeeCalculation()
		{
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
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

		public override void TestIsUCC5()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				AssertEquals("Disabled when business object is not JobDeclaration", false, declaration.Configuration.IsUCC5(Factory.New<DummyBusinessObject>()));

				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("CDS", true, declaration.Configuration.IsUCC5(declaration));
			});
		}

		public override void TestIsUCC6()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			AssertEquals(false, declaration.Configuration.IsUCC6(declaration));
		}

		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
		}

		public void TestUseEoriForFiscalReference()
		{
			AssertEquals(true, configuration.UseEoriForFiscalReference);
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
}
