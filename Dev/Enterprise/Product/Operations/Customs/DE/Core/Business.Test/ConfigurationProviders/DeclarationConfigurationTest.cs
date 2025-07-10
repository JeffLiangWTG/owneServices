using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(DeclarationConfiguration))]
	class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EU.Business.EntryHeaderConfiguration, EU.Business.EntryLineConfiguration>
	{
		[ExpectNoExceptions]
		public void TestUseEucdmSupportingDocumentGoodsShipment()
		{
			NUnit.Framework.Assert.That(configuration.UseEucdmSupportingDocumentGoodsShipment, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUCCAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestMiscAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscAdditionalInfosSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscSupportingDocumentsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscPreviousDocumentsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscGuaranteesSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscGuaranteesSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUseUniversalFeeCalculation()
		{
			NUnit.Framework.Assert.That(configuration.UseUniversalFeeCalculation(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDV1DetailsSupport()
		{
			NUnit.Framework.Assert.That(configuration.DV1DetailsSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestLockNumberOfEntryLinesForRegisteredEntry()
		{
			NUnit.Framework.Assert.That(configuration.LockNumberOfEntryLinesForRegisteredEntry, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestIsUCC5()
		{
			NUnit.Framework.Assert.That(configuration.IsUCC5(Factory.New<DummyBusinessObject>()), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestIsUCC6()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.IsUCC6(Factory.New<DummyBusinessObject>()), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled when business object is not JobDeclaration");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.IsUCC6(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.IsUCC6(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				NUnit.Framework.Assert.That(configuration.IsUCC6(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for other");
			});
		}

		[ExpectNoExceptions]
		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			NUnit.Framework.Assert.That(configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestShouldCheckLegalByDeclarantType()
		{
			NUnit.Framework.Assert.That(configuration.ShouldCheckLegalByDeclarantType, Is.EqualTo(false));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
