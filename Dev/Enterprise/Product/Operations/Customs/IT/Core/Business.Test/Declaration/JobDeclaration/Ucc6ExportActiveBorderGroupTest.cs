using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportActiveBorderGroupTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportActiveBorderGroup(declaration: null));
		AssertNoExceptionThrown(() => new Ucc6ExportActiveBorderGroup(declaration: Factory.New<JobDeclaration>()));
	}

	public void TestHasEntryStyleExportNormalAndHasAllowedProcedureCodes()
	{
		var activeBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "EX";
			instruction1.CEI_Procedure = "11";
			instruction2.CEI_Procedure = "";
			AssertEquals("When EntryStyle=EX and Procedure Codes=11, EMPTY", true, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "33";
			instruction2.CEI_Procedure = "";
			AssertEquals("When EntryStyle=EX and Procedure Codes=33 EMPTY", false, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "34";
			instruction2.CEI_Procedure = "10";
			AssertEquals("When EntryStyle=EX and Procedure Codes=34, 10", true, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "23";
			instruction2.CEI_Procedure = "";
			AssertEquals("When EntryStyle=EX and Procedure Codes=23, EMPTY", true, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "31";
			AssertEquals("When EntryStyle=EX and Procedure Codes=31, EMPTY", true, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());

			declaration.JE_EntryStyle = "CO";
			AssertEquals("When EntryStyle=CO and Procedure Codes=23, EMPTY", false, activeBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes());
		});
	}

	public void TestHasEntryStyleExportNormalAndHasNotAllowedTransportMode()
	{
		var activeBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "EX";
			declaration.JE_TransportMode = "SEA";
			AssertEquals("When EntryStyle=EX and JE_TransportMode=SEA", false, activeBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=RAI", true, activeBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=FIX", true, activeBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=MAI", true, activeBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode());

			declaration.JE_EntryStyle = "CO";
			AssertEquals("When EntryStyle=CO and JE_TransportMode=FIX", false, activeBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode());
		});
	}

	public void TestHasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes()
	{
		var activeBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "CO";
			instruction1.CEI_Procedure = "76";
			instruction2.CEI_Procedure = "";
			AssertEquals("When EntryStyle=CO and Procedure Codes=76, EMPTY", true, activeBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "33";
			instruction2.CEI_Procedure = "";
			AssertEquals("When EntryStyle=CO and Procedure Codes=33 EMPTY", false, activeBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes());

			instruction1.CEI_Procedure = "34";
			instruction2.CEI_Procedure = "77";
			AssertEquals("When EntryStyle=CO and Procedure Codes=34, 77", true, activeBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes());

			declaration.JE_EntryStyle = "EX";
			AssertEquals("When EntryStyle=CO and Procedure Codes=23, EMPTY", false, activeBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes());
		});
	}

	public void TestHasEntryStyleExportToSpecialAndHasNotAllowedTransportMode()
	{
		var activeBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "CO";
			declaration.JE_TransportMode = "SEA";
			AssertEquals("When EntryStyle=EX and JE_TransportMode=SEA", false, activeBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=RAI", false, activeBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=FIX", true, activeBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode());

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("When EntryStyle=EX and JE_TransportMode=MAI", true, activeBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode());

			declaration.JE_EntryStyle = "EX";
			AssertEquals("When EntryStyle=CO and JE_TransportMode=FIX", false, activeBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode());
		});
	}

	public void TestAreAllFieldsEmptyOrFilled()
	{
		var activeBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);

		SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled("AIR", declaration.JE_VoyageFlightNoInfo);
		SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled("FIX", declaration.JE_VesselNameInfo);
		SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled("ROA", declaration.JE_VesselNameInfo);
		SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled("OWN", declaration.JE_VesselNameInfo);
		SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled("RAI", declaration.JE_VesselNameInfo);

		CombineAssertions("When TransportMode=SEA", () =>
		{
			declaration.JE_TransportMode = "SEA";
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			declaration.ZG_BorderTransportMeans = ZString.Empty;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.JE_VesselName = ZString.Empty;
			AssertEquals("When All fields are empty", true, activeBorderGroup.AreAllFieldsEmptyOrFilled());

			declaration.JE_RN_NKTransportNationality = "DE";
			AssertEquals("When JE_RN_NKTransportNationality is not EMPTY", false, activeBorderGroup.AreAllFieldsEmptyOrFilled());

			declaration.ZG_BorderTransportMeans = "11";
			AssertEquals("When ZG_BorderTransportMeans is not EMPTY", false, activeBorderGroup.AreAllFieldsEmptyOrFilled());

			declaration.JE_VesselName = "VE123";
			AssertEquals("When All Fields are not empty except JE_VoyageFlightNo", true, activeBorderGroup.AreAllFieldsEmptyOrFilled());

			declaration.JE_VoyageFlightNo = "FL123";
			declaration.JE_VesselName = ZString.Empty;
			AssertEquals("When All Fields are not empty except JE_VesselName", true, activeBorderGroup.AreAllFieldsEmptyOrFilled());
		});

		void SetTransportModeWithDataAndAssertAreAllFieldsEmptyOrFilled(string transportMode, ZPropertyInfo transportFieldInfo)
		{
			CombineAssertions($"When TransportMode={transportMode}", () =>
			{
				declaration.JE_TransportMode = transportMode;
				declaration.JE_RN_NKTransportNationality = ZString.Empty;
				declaration.ZG_BorderTransportMeans = ZString.Empty;
				transportFieldInfo.Value = ZString.Empty;
				AssertEquals("When All fields are empty", true, activeBorderGroup.AreAllFieldsEmptyOrFilled());

				declaration.JE_RN_NKTransportNationality = "DE";
				AssertEquals("When JE_RN_NKTransportNationality is not EMPTY", false, activeBorderGroup.AreAllFieldsEmptyOrFilled());

				declaration.ZG_BorderTransportMeans = "11";
				AssertEquals("When ZG_BorderTransportMeans is not EMPTY", false, activeBorderGroup.AreAllFieldsEmptyOrFilled());

				transportFieldInfo.Value = new ZString("NR123");
				AssertEquals("When All fields are not empty", true, activeBorderGroup.AreAllFieldsEmptyOrFilled());
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		instruction1 = declaration.CustomsEntryInstructions.AddNew();
		instruction2 = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction instruction1, instruction2;
}
