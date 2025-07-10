using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationShipmentIncoTermCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationShipmentIncoTermCleanUpStrategy(declaration: null));
	}

	public void TestCleanUpDeliveryTerms_WhenDeclarationIsExpUcc6()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.ZG_AdditionalDeliveryTerms = "Add Delivery Terms";
			declaration.EUD_AgreedPlaceCode = "123";
			declaration.JE_ShipmentIncoTermPlace = "Place";
			declaration.ZG_AgreedPlaceCode = "ZG";

			strategy.CleanUp();
			AssertDeliveryTerms("When Dec is EXP UCC6 and IncoTerm is XXX", "Add Delivery Terms", "", "", "");

			declaration.ZG_AgreedPlaceCode = "ZG";
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.EUD_AgreedPlaceCode = "123";
			declaration.JE_ShipmentIncoTermPlace = "Place";

			strategy.CleanUp();
			AssertDeliveryTerms("When Dec is EXP UCC6 and IncoTerm is EXW", "", "123", "Place", "");
		}
	}

	public void TestCleanUpDeliveryTerms_WhenDeclarationIsExpNotUcc6OrImp()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.ZG_AdditionalDeliveryTerms = "Add Delivery Terms";
			declaration.EUD_AgreedPlaceCode = "123";
			declaration.JE_ShipmentIncoTermPlace = "Place";
			declaration.ZG_AgreedPlaceCode = "ZG";

			strategy.CleanUp();
			AssertDeliveryTerms("When Dec is EXP not UCC6 and IncoTerm is XXX", "", "123", "Place", "ZG");

			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.ZG_AdditionalDeliveryTerms = "Add Delivery Terms";

			strategy.CleanUp();
			AssertDeliveryTerms("When Dec is EXP not UCC6 and IncoTerm is EXW", "", "123", "Place", "1");
		}

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_ShipmentIncoTerm = "XXX";
		declaration.ZG_AdditionalDeliveryTerms = "Add Delivery Terms";
		declaration.EUD_AgreedPlaceCode = "123";
		declaration.JE_ShipmentIncoTermPlace = "Place";
		declaration.ZG_AgreedPlaceCode = "ZG";

		strategy.CleanUp();
		AssertDeliveryTerms("When Dec is EXP not UCC6 and IncoTerm is XXX", "", "123", "Place", "ZG");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		strategy = new JobDeclarationShipmentIncoTermCleanUpStrategy(declaration);
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	void AssertDeliveryTerms(string assertionMessage
		, string expectedAdditionalDeliveryTerms
		, string expectedAgreedPlaceCode
		, string expectedShipmentIncoTermPlace
		, string expectedZGAgreedPlaceCode)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals(nameof(declaration.ZG_AdditionalDeliveryTerms), expectedAdditionalDeliveryTerms, declaration.ZG_AdditionalDeliveryTerms);
			AssertEquals(nameof(declaration.EUD_AgreedPlaceCode), expectedAgreedPlaceCode, declaration.EUD_AgreedPlaceCode);
			AssertEquals(nameof(declaration.JE_ShipmentIncoTerm), expectedShipmentIncoTermPlace, declaration.JE_ShipmentIncoTermPlace);
			AssertEquals(nameof(declaration.ZG_AgreedPlaceCode), expectedZGAgreedPlaceCode, declaration.ZG_AgreedPlaceCode);
		});
	}

	JobDeclaration declaration;
	ICleanUpStrategy strategy;
}
