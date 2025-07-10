using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusVehicleLookups))]
sealed class CusVehicleLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestModelNameCodeListExport() => CombineAssertions(() =>
	{
		AssertCodeList(MessageTypeCodeList.Codes.Export, RefCusCodeTestHelper.ValidVehicleModelNameCode, RefCusCodeTestHelper.InvalidVehicleModelNameCode);
		AssertCodeList(MessageTypeCodeList.Codes.Export, RefCusCodeTestHelper.ValidVehicleModelNameCode, RefCusCodeTestHelper.InvalidVehicleModelNameCode, new ZDateTime(2023, 12, 31));
	});

	public void TestModelNameCodeListImport() => CombineAssertions(() =>
	{
		AssertCodeList(MessageTypeCodeList.Codes.Import, RefCusCodeTestHelper.ValidVehicleModelNameCode, RefCusCodeTestHelper.InvalidVehicleModelNameCode);
		AssertCodeList(MessageTypeCodeList.Codes.Import, RefCusCodeTestHelper.ValidVehicleModelNameCodePast, RefCusCodeTestHelper.ValidVehicleModelNameCode, new ZDateTime(2023, 12, 31));
	});

	void AssertCodeList(string messageType, string validVehicleModelNameCode, string invalidVehicleModelNameCode, ZDateTime? assessmentDate = null)
	{
		var effAssessmentDate = assessmentDate ?? ZDateTime.Today;

		RefCusCodeTestHelper.CreateVehicleModelNameCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var vehicle = invoiceLine.Vehicles.AddNew();

		if (effAssessmentDate != ZDateTime.Today)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = effAssessmentDate;
			invoiceLine.JI_CEI = entryInstruction.PK;
		}

		var list = vehicle.Lookups.ModelNameCodeList;
		list.Load();

		AssertEquals("Valid code", true, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == validVehicleModelNameCode));
		AssertEquals("Invalid code", false, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == invalidVehicleModelNameCode));
	}
}
