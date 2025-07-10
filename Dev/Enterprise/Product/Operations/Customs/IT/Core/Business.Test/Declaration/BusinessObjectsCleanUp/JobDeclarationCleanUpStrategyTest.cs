using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationCleanUpStrategy(declaration: null));
	}

	public void TestCleanUpZG_SpecificCircumstanceIndicator()
	{
		declaration.ZG_SpecificCircumstanceIndicator = "A";

		strategy.CleanUp();
		AssertEquals("ZG_SpecificCircumstanceIndicator", "", declaration.ZG_SpecificCircumstanceIndicator);
	}

	public void TestCleanUpZG_BorderTransportMeans()
	{
		declaration.ZG_BorderTransportMeans = "10";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			declaration.JE_MessageType = "EXP";
			strategy.CleanUp();
			AssertEquals("For Export UCC6 declaration, ZG_BorderTransportMeans after CleanUp", "10", declaration.ZG_BorderTransportMeans);

			declaration.JE_MessageType = "IMP";
			strategy.CleanUp();
			AssertEquals("For Import UCC6 declaration, ZG_BorderTransportMeans after CleanUp", "", declaration.ZG_BorderTransportMeans);
		}

		declaration.ZG_BorderTransportMeans = "11";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			declaration.JE_MessageType = "EXP";
			strategy.CleanUp();
			AssertEquals("For Export non UCC6 declaration, ZG_BorderTransportMeans after CleanUp", "", declaration.ZG_BorderTransportMeans);
		}
	}

	public void TestCleanUpZG_CTStatusID()
	{
		declaration.JE_MessageType = "IMP";
		declaration.ZG_CTStatusID = "T2L";
		strategy.CleanUp();
		AssertEquals("For Import declaration, ZG_CTStatusID after CleanUp", "T2L", declaration.ZG_CTStatusID);

		declaration.JE_MessageType = "EXP";
		strategy.CleanUp();
		AssertEquals("For Export Non UCC6 declaration, ZG_CTStatusID after CleanUp", "T2L", declaration.ZG_CTStatusID);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("For Export UCC6 declaration, ZG_CTStatusID after CleanUp", "", declaration.ZG_CTStatusID);
		}
	}

	public void TestCleanUpJE_SubLocationOfGoods()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_SubLocationOfGoods = "AAA";
		strategy.CleanUp();
		AssertEquals("For Export Non UCC6 declaration, JE_SubLocationOfGoods after CleanUp", "AAA", declaration.JE_SubLocationOfGoods);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("For Export UCC6 declaration, JE_SubLocationOfGoods after CleanUp", "", declaration.JE_SubLocationOfGoods);
		}
	}

	public void TestCleanUpJE_GS_NKCusAgent()
	{
		declaration.JE_GS_NKCusAgent = "BOB";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("JE_GS_NKCusAgent", "BOB", declaration.JE_GS_NKCusAgent);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("JE_GS_NKCusAgent", "", declaration.JE_GS_NKCusAgent);
		}
	}

	public void TestCleanUpGoodsLocationIfNoLongerApplicable()
	{
		ConfigureGoodsLocation();
		AssertEquals("[PRE-CONDITION] GoodsLocationDescription", "Y;C;Via Padova 1", declaration.GoodsLocationDescription);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("When Declaration is NOT UCC6, GoodsLocationDescription after CleanUp()", "", declaration.GoodsLocationDescription);
		}

		ConfigureGoodsLocation();
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("When Declaration is UCC6, GoodsLocationDescription after CleanUp", "Y;C;Via Padova 1", declaration.GoodsLocationDescription);
		}

		void ConfigureGoodsLocation()
		{
			declaration.GoodsLocation.CGL_Qualifier = "Y";
			declaration.GoodsLocation.CGL_Type = "C";
			declaration.GoodsLocation.Address.E2_Address1 = "Via Padova 1";
		}
	}

	public void TestCleanUpIsSecurityDeclaration()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			declaration.ZG_IsSecurityDeclaration = true;
			strategy.CleanUp();
			AssertEquals("When job is UCC6, ZG_IsSecurityDeclaration", true, declaration.ZG_IsSecurityDeclaration);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("When job is not UCC6, ZG_IsSecurityDeclaration", false, declaration.ZG_IsSecurityDeclaration);
		}
	}

	public void TestCleanUpJE_TransportMeans()
	{
		declaration.JE_TransportMeans = "10";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("JE_TransportMeans", "10", declaration.JE_TransportMeans);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("JE_TransportMeans", "", declaration.JE_TransportMeans);
		}
	}

	public void TestCleanUpAuthorisationNumber()
	{
		declaration.JE_MessageType = "EXP";

		declaration.ZG_AuthorisationNumber = "123";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("When job is not EXP UCC6, ZG_AuthorisationNumber", "123", declaration.ZG_AuthorisationNumber);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("When job is EXP UCC6, ZG_AuthorisationNumber", "", declaration.ZG_AuthorisationNumber);
		}

		declaration.JE_MessageType = "IMP";
		declaration.ZG_AuthorisationNumber = "123";
		strategy.CleanUp();
		AssertEquals("When job is IMP, ZG_AuthorisationNumber", "123", declaration.ZG_AuthorisationNumber);
	}

	public void TestCleanupMiscPreviousDocuments()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.PreviousDocuments.AddNew();
		declaration.PreviousDocuments.AddNew();

		AssertEquals("[PRE-CONDITION] PreviousDocuments Count", 2, declaration.PreviousDocuments.Count);

		strategy.CleanUp();
		AssertEquals("Declaration Type: EXP (Non-UCC6) PreviousDocuments Count", 2, declaration.PreviousDocuments.Count);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("Declaration Type: EXP (UCC6) PreviousDocuments Count", 0, declaration.PreviousDocuments.Count);
		}

		declaration.PreviousDocuments.AddNew();
		declaration.PreviousDocuments.AddNew();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		strategy.CleanUp();

		AssertEquals("Declaration Type: IMP PreviousDocuments Count", 0, declaration.PreviousDocuments.Count);
	}

	public void TestCleanupVesselName()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_VesselName = "VESSEL NAME";

		strategy.CleanUp();
		AssertEquals("Declaration Type: IMP, VesselName", "", declaration.JE_VesselName);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_VesselName = "VESSEL NAME";

		strategy.CleanUp();
		AssertEquals("Declaration Type: !IMP, VesselName", "VESSEL NAME", declaration.JE_VesselName);
	}

	public void TestCleanupVoyageFlightNo()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_VoyageFlightNo = "VOYAGE NO";

		strategy.CleanUp();
		AssertEquals("Declaration Type: IMP, VoyageFlightNo", "", declaration.JE_VoyageFlightNo);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_VoyageFlightNo = "VOYAGE NO";

		strategy.CleanUp();
		AssertEquals("Declaration Type: !IMP, VoyageFlightNo", "VOYAGE NO", declaration.JE_VoyageFlightNo);
	}

	public void TestCleanupZG_Box18TransportID()
	{
		declaration.JE_MessageType = "EXP";
		declaration.ZG_Box18TransportID = "A112";
		strategy.CleanUp();
		AssertEquals("For Export Non UCC6 declaration, ZG_Box18TransportID after CleanUp", "A112", declaration.ZG_Box18TransportID);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("For Export UCC6 declaration, ZG_Box18TransportID after CleanUp", "", declaration.ZG_Box18TransportID);
		}
	}

	public void TestCleanupZG_Box18TransportNationality()
	{
		declaration.JE_MessageType = "EXP";
		declaration.ZG_Box18TransportNationality = "IT";
		strategy.CleanUp();
		AssertEquals("For Export Non UCC6 declaration, ZG_Box18TransportNationality after CleanUp", "IT", declaration.ZG_Box18TransportNationality);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("For Export UCC6 declaration, ZG_Box18TransportNationality after CleanUp", "", declaration.ZG_Box18TransportNationality);
		}
	}

	public void TestCleanUpMiscSupportingDocuments()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.SupportingDocuments.AddNew();
		declaration.SupportingDocuments.AddNew();

		AssertEquals("[PRE-CONDITION] PreviousDocuments Count", 2, declaration.SupportingDocuments.Count);

		strategy.CleanUp();
		AssertEquals("Declaration Type: EXP (Non-UCC6)  SupportingDocuments Count", 2, declaration.SupportingDocuments.Count);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("Declaration Type: EXP (UCC6) SupportingDocuments Count", 0, declaration.SupportingDocuments.Count);
		}

		declaration.SupportingDocuments.AddNew();
		declaration.SupportingDocuments.AddNew();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		strategy.CleanUp();
		AssertEquals("Declaration Type: IMP SupportingDocuments Count", 0, declaration.SupportingDocuments.Count);
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		strategy = new JobDeclarationCleanUpStrategy(declaration);
	}

	JobDeclaration declaration;
	ICleanUpStrategy strategy;
}
