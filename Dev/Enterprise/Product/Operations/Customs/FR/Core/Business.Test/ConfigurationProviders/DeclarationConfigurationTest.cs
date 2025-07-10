using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(DeclarationConfiguration))]
	class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
	{
		public void TestUseEucdmSupportingDocumentGoodsShipment()
		{
			AssertEquals(false, configuration.UseEucdmSupportingDocumentGoodsShipment);
		}

		public override void TestUCCAdditionalInfosSupport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("Enabled for DeltaIE", true, configuration.UCCAdditionalInfosSupport(declaration));
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("Disabled for DeltaG", false, configuration.UCCAdditionalInfosSupport(declaration));
			});
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
			AssertEquals(false, configuration.MiscGuaranteesSupport(declaration));
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));
		}

		public override void TestDV1DetailsSupport()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Disabled for DeltaIE", false, configuration.DV1DetailsSupport(declaration));
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Enabled for DeltaG", true, configuration.DV1DetailsSupport(declaration));
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
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Only DeltaIE declaration should be flagged as UCC6.", false, configuration.IsUCC6(declaration));
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("DeltaIE declaration should be flagged as UCC6.", true, configuration.IsUCC6(declaration));
		}

		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			AssertEquals(true, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
		}

		public override void TestShouldCheckLegalByDeclarantType()
		{
			AssertEquals(true, configuration.ShouldCheckLegalByDeclarantType);
		}

		public void TestUseIDDDocument()
		{
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("UseIDDDocument should be false when DeltaG Import.", false, configuration.UseIDDDocument(declaration));
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("UseIDDDocument should be true when DeltaIE Import", true, configuration.UseIDDDocument(declaration));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("UseIDDDocument should be false when DeltaG Export.", false, configuration.UseIDDDocument(declaration));
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("UseIDDDocument should be false when DeltaIE Export", false, configuration.UseIDDDocument(declaration));
		}

		public void TestGetPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportPreviousDocumentValidationDecider>("IsUCC6 IMP", configuration.GetPreviousDocumentValidationDecider(declaration));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
