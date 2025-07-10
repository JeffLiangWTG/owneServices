using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DeclarationConfiguration))]
	public class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
	{
		public void TestUseEucdmSupportingDocumentGoodsShipmentAndItem()
		{
			AssertEquals(true, configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem);
		}

		public override void TestUCCAdditionalInfosSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Enabled", true, configuration.UCCAdditionalInfosSupport(declaration));
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
			AssertEquals(false, configuration.DV1DetailsSupport(declaration));
		}

		public override void TestLockNumberOfEntryLinesForRegisteredEntry()
		{
			AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
		}

		public override void TestIsUCC5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("UCC5 disabled when business object is not JobDeclaration", false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("UCC5 disabled for export declaration", false, configuration.IsUCC5(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("UCC5 disabled for import declaration", false, configuration.IsUCC5(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("UCC5 disabled for import interfaced declaration", false, configuration.IsUCC5(declaration));
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("UCC5 disabled for import built-in declaration", false, configuration.IsUCC5(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 enabled for import V1 declaration", true, configuration.IsUCC5(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC5 disabled for import V2 declaration", false, configuration.IsUCC5(declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("UCC5 disabled for other", false, configuration.IsUCC5(declaration));
			});
		}

		public override void TestIsUCC6()
		{
			CombineAssertions(() =>
			{
				AssertEquals("UCC6 disabled when business object is not JobDeclaration", false, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("UCC6 enabled for export declaration", true, configuration.IsUCC6(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("UCC6 enabled for import declaration", true, configuration.IsUCC6(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("UCC6 enabled for import interfaced declaration", true, configuration.IsUCC6(declaration));
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("UCC6 enabled for import built-in declaration", true, configuration.IsUCC6(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC6 disabled for import V1 declaration", false, configuration.IsUCC6(declaration));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 enabled for import V2 declaration", true, configuration.IsUCC6(declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("UCC6 enabled for other", true, configuration.IsUCC6(declaration));
			});
		}

		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
		}

		public void TestIsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertEquals(true, configuration.IsTransitionPeriodAES30(declaration));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				AssertEquals(false, configuration.IsTransitionPeriodAES30(declaration));
			}
		}

		public override void TestShouldCheckLegalByDeclarantType()
		{
			AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
		}

		public void TestIsPopulateAuthorisationsForOfficeOfPresentationEnabledCore()
		{
			CombineAssertions(() =>
			{
				declaration.IsUXMLImportingData = true;

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export", false, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", false, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));

				declaration.IsUXMLImportingData = false;

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export", true, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", false, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
