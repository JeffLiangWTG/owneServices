using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationTransportModeInlandCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationTransportModeInlandCleanUpStrategy(declaration: null));
	}

	public void TestCleanUpAircraftRegistrationInlandIfNotApplicable()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";
			AssertPropertyValueNotEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "AIR", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "RAI", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "ROA", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "FIX", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "IWT", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "OWN", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "MAI", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
			AssertPropertyValueEmptyForTransportModeInland("JE_AircraftRegistrationInland", transportModeInland: "SEA", SetAircraftRegistrationInland, GetAircraftRegistrationInland);
		}

		void SetAircraftRegistrationInland(ZString value) => declaration.JE_AircraftRegistrationInland = value;
		ZString GetAircraftRegistrationInland() => declaration.JE_AircraftRegistrationInland;
	}

	public void TestCleanUpTrailer1RegNoIfNotApplicable()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "AIR", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "RAI", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "ROA", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "FIX", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "IWT", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "OWN", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "MAI", SetTrailer1RegNo, GetTrailer1RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer1RegNo", transportModeInland: "SEA", SetTrailer1RegNo, GetTrailer1RegNo);
		}

		void SetTrailer1RegNo(ZString value) => declaration.JE_Trailer1RegNo = value;
		ZString GetTrailer1RegNo() => declaration.JE_Trailer1RegNo;
	}

	public void TestCleanUpTrailer2RegNoIfNotApplicable()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "AIR", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "RAI", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "ROA", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "FIX", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "IWT", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "OWN", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "MAI", SetTrailer2RegNo, GetTrailer2RegNo);
			AssertPropertyValueEmptyForTransportModeInland("JE_Trailer2RegNo", transportModeInland: "SEA", SetTrailer2RegNo, GetTrailer2RegNo);
		}

		void SetTrailer2RegNo(ZString value) => declaration.JE_Trailer2RegNo = value;
		ZString GetTrailer2RegNo() => declaration.JE_Trailer2RegNo;
	}

	public void TestCleanUpTrailer1NationalityIfNotApplicable()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";
			AssertPropertyValueNotEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "AIR", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "RAI", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "ROA", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "FIX", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "IWT", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "OWN", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "MAI", SetTrailer1Nationality, GetTrailer1Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer1Nationality", transportModeInland: "SEA", SetTrailer1Nationality, GetTrailer1Nationality);
		}

		void SetTrailer1Nationality(ZString value) => declaration.JE_RN_NKTrailer1Nationality = value;
		ZString GetTrailer1Nationality() => declaration.JE_RN_NKTrailer1Nationality;
	}

	public void TestCleanUpTrailer2NationalityIfNotApplicable()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "AIR", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "RAI", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueNotEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "ROA", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "FIX", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "IWT", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "OWN", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "MAI", SetTrailer2Nationality, GetTrailer2Nationality);
			AssertPropertyValueEmptyForTransportModeInland("JE_RN_NKTrailer2Nationality", transportModeInland: "SEA", SetTrailer2Nationality, GetTrailer2Nationality);
		}

		void SetTrailer2Nationality(ZString value) => declaration.JE_RN_NKTrailer2Nationality = value;
		ZString GetTrailer2Nationality() => declaration.JE_RN_NKTrailer2Nationality;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		strategy = new JobDeclarationCleanUpStrategy(declaration);
	}

	JobDeclaration declaration;
	ICleanUpStrategy strategy;

	#region Implementation

	void AssertPropertyValueEmptyForTransportModeInland(string propertyName, ZString transportModeInland, Action<ZString> setPropertyValue, Func<ZString> getPropertyValue)
	{
		setPropertyValue("10");
		declaration.JE_TransportModeInland = transportModeInland;
		strategy.CleanUp();
		AssertEquals($"When JE_TransportModeInland set to {transportModeInland}, {propertyName} after CleanUp", ZString.Empty, getPropertyValue());
	}

	void AssertPropertyValueNotEmptyForTransportModeInland(string propertyName, ZString transportModeInland, Action<ZString> setPropertyValue, Func<ZString> getPropertyValue)
	{
		setPropertyValue("11");
		declaration.JE_TransportModeInland = transportModeInland;
		strategy.CleanUp();
		AssertEquals($"When JE_TransportModeInland set to {transportModeInland}, {propertyName} after CleanUp", "11", getPropertyValue());
	}

	#endregion
}
