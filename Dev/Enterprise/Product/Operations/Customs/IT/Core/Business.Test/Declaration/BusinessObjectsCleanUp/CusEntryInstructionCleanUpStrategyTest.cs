using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryInstructionCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryInstruction is null", () => new CusEntryInstructionCleanUpStrategy(entryInstruction: null));
		AssertExceptionThrown<ArgumentNullException>("When entryInstruction.Declaration is null", () => new CusEntryInstructionCleanUpStrategy(Factory.New<CusEntryInstruction>()));
	}

	public void TestCleanUpFiscalReferences()
	{
		declaration.JE_MessageType = "IMP";
		entryInstruction.FiscalReferences.AddNew();
		entryInstruction.FiscalReferences.AddNew();

		strategy.CleanUp();
		AssertEquals("FiscalReferences Count", 2, entryInstruction.FiscalReferences.Count);

		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("FiscalReferences Count", 0, entryInstruction.FiscalReferences.Count);
	}

	public void TestCleanUpClearanceByEntryLine()
	{
		declaration.JE_MessageType = "IMP";
		entryInstruction.ClearanceByEntryLine = true;

		strategy.CleanUp();
		AssertEquals("ClearanceByEntryLine", true, entryInstruction.ClearanceByEntryLine);

		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("ClearanceByEntryLine", false, entryInstruction.ClearanceByEntryLine);
	}

	public void TestCleanUpZG_PresentationStartDate()
	{
		declaration.JE_MessageType = "EXP";
		entryInstruction.ZG_PresentationStartDate = new ZDateTime(2022, 10, 28);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("ZG_PresentationStartDate", new ZDateTime(2022, 10, 28), entryInstruction.ZG_PresentationStartDate);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("ZG_PresentationStartDate", ZDateTime.Empty, entryInstruction.ZG_PresentationStartDate);
		}
	}

	public void TestCleanUpSupportingDocuments()
	{
		declaration.JE_MessageType = "EXP";
		entryInstruction.SupportingDocuments.AddNew();
		entryInstruction.SupportingDocuments.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("Supporting Documents Count", 2, entryInstruction.SupportingDocuments.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("Supporting Documents Count", 0, entryInstruction.SupportingDocuments.Count);
		}
	}

	public void TestCleanupWarehouseTypeAndIdFields()
	{
		entryInstruction.ZG_FromWarehouseID = "1234";
		entryInstruction.ZG_FromWarehouseType = "R";
		entryInstruction.ZG_ToWarehouseID = "2222";
		entryInstruction.ZG_ToWarehouseType = "S";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		strategy.CleanUp();

		CombineAssertions(() =>
		{
			AssertEquals("1234", entryInstruction.ZG_FromWarehouseID);
			AssertEquals("R", entryInstruction.ZG_FromWarehouseType);
			AssertEquals("2222", entryInstruction.ZG_ToWarehouseID);
			AssertEquals("S", entryInstruction.ZG_ToWarehouseType);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		strategy.CleanUp();

		CombineAssertions(() =>
		{
			AssertNullOrEmpty(entryInstruction.ZG_FromWarehouseID);
			AssertNullOrEmpty(entryInstruction.ZG_FromWarehouseType);
			AssertNullOrEmpty(entryInstruction.ZG_ToWarehouseType);
			AssertNullOrEmpty(entryInstruction.ZG_ToWarehouseID);
		});
	}

	public void TestCleanupWarehouseTypeAndIds()
	{
		entryInstruction.ZG_FromWarehouseID = "1234";
		entryInstruction.ZG_FromWarehouseType = "R";
		entryInstruction.ZG_ToWarehouseID = "2222";
		entryInstruction.ZG_ToWarehouseType = "S";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		strategy.CleanUp();
		CombineAssertions("For Import", () =>
		{
			AssertEquals("1234", entryInstruction.ZG_FromWarehouseID);
			AssertEquals("R", entryInstruction.ZG_FromWarehouseType);
			AssertEquals("2222", entryInstruction.ZG_ToWarehouseID);
			AssertEquals("S", entryInstruction.ZG_ToWarehouseType);
		});

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			strategy.CleanUp();
			CombineAssertions("For UCC6 Export", () =>
			{
				AssertEquals("1234", entryInstruction.ZG_FromWarehouseID);
				AssertEquals("R", entryInstruction.ZG_FromWarehouseType);
				AssertEquals("2222", entryInstruction.ZG_ToWarehouseID);
				AssertEquals("S", entryInstruction.ZG_ToWarehouseType);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			CombineAssertions("For Non UCC6 Export", () =>
			{
				AssertNullOrEmpty(entryInstruction.ZG_FromWarehouseID);
				AssertNullOrEmpty(entryInstruction.ZG_FromWarehouseType);
				AssertNullOrEmpty(entryInstruction.ZG_ToWarehouseType);
				AssertNullOrEmpty(entryInstruction.ZG_ToWarehouseID);
			});
		}
	}

	public void TestCleanupAdditionalInfos()
	{
		AddAdditionalInfoRecordsToEntryInstruction();
		AssertEquals("[PRE-CONDITION] Additional Info Count", 2, entryInstruction.AdditionalInfos.Count);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		strategy.CleanUp();
		AssertEquals("Additional Info Count for Non-UCC6 Export", 0, entryInstruction.AdditionalInfos.Count);

		AddAdditionalInfoRecordsToEntryInstruction();
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			strategy.CleanUp();
			AssertEquals("Additional Info Count for UCC6 Export", 2, entryInstruction.AdditionalInfos.Count);
		}

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		strategy.CleanUp();
		AssertEquals("Additional Info Count for Import", 0, entryInstruction.AdditionalInfos.Count);

		void AddAdditionalInfoRecordsToEntryInstruction()
		{
			entryInstruction.AdditionalInfos.AddNew();
			entryInstruction.AdditionalInfos.AddNew();
		}
	}

	public void TestCleanUpSealsInfo()
	{
		entryInstruction.ZG_SealsCount = 99;
		entryInstruction.Seals.AddNew();
		entryInstruction.Seals.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			CombineAssertions("For non UCC6 declarations", () =>
			{
				AssertEquals("ZG_SealsCount", 99, entryInstruction.ZG_SealsCount);
				AssertEquals("Seals Count", 2, entryInstruction.Seals.Count);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			CombineAssertions("For UCC6 declarations", () =>
			{
				AssertEquals("ZG_SealsCount", 0, entryInstruction.ZG_SealsCount);
				AssertEquals("Seals Count", 0, entryInstruction.Seals.Count);
			});
		}
	}

	public void TestCleanUpPreviousDocuments()
	{
		declaration.JE_MessageType = "IMP";
		entryInstruction.PreviousDocuments.AddNew();
		entryInstruction.PreviousDocuments.AddNew();

		strategy.CleanUp();
		AssertEquals("PreviousDocuments Count", 2, entryInstruction.PreviousDocuments.Count);

		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("PreviousDocuments Count", 0, entryInstruction.PreviousDocuments.Count);
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		strategy = new CusEntryInstructionCleanUpStrategy(entryInstruction);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	ICleanUpStrategy strategy;
}
