using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business.Testing;

public class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStyleList_Import()
	{
		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);
		EntryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals($"Ensty Codes List for IMP", "01, 02, 03, 04, 05, 06, 99", EntryInstruction.Lookups.StyleList.CodesAsString);
	}

	public void TestStyleList_Export()
	{
		RefCusCodeTestHelper.CreateInputControlList(Factory);
		EntryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals($"Input Control List for EXP", "1, 2", EntryInstruction.Lookups.StyleList.CodesAsString);
	}

	public void TestEntrySubStyleList()
	{
		AssertSame(CommonLookups.DeclarationTimeCodeList(EntryInstruction), EntryInstruction.Lookups.EntrySubStyleList);
	}

	public void TestCEI_ProcedureList()
	{
		var expected = RefCusCodeTestHelper.CreateProcedureList(Factory);

		EntryInstruction.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var proceduresCodeArray = ((RefCusProcedureCollection)EntryInstruction.Lookups.ProcedureCodeList).ToArray();
		AssertContainsExactElementsInExactOrder("Procedure code list for EXP", expected, proceduresCodeArray);
	}

	public void TestWarehouseTypeList()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		RefCusCodeTestHelper.CreateWarehouseCodeList(Factory);
		AssertEquals("List Codes", "1, 2", EntryInstruction.Lookups.WarehouseTypeList.CodesAsString);
	}

	public void TestDeclarationReasonList_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreatePreasCodeList(Factory);

		AssertEquals("List Codes", "1, 2, 3, 4", EntryInstruction.Lookups.DeclarationReasonList.CodesAsString);
	}

	public void TestTransportChargesMethodOfPaymentList()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			var transportChargesModeOfPaymentList = EntryInstruction.Lookups.TransportChargesMethodOfPaymentList;
			AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportChargesModeOfPaymentList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<TransportChargesModeOfPayment>(), transportChargesModeOfPaymentList);
		});
	}

	public void TestNextProcedureList()
	{
		AssertSame(CommonLookups.NextProcedureList(EntryInstruction), EntryInstruction.Lookups.NextProcedureList);
	}

	JobDeclaration Declaration => EntryInstruction.JobDeclaration;

	CusEntryInstruction EntryInstruction => entryInstruction ?? (entryInstruction = GetEntryInstruction());
	CusEntryInstruction entryInstruction;

	CusEntryInstruction GetEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.CustomsEntryInstructions.AddNew();
	}
}
