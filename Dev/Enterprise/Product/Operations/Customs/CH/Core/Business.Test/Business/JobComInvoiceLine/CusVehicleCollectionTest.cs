using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusVehicleCollection))]
sealed class CusVehicleCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCount() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest();

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("EXP", 1, collection.MaxCount);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("IMP", -1, collection.MaxCount);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("EDA", -1, collection.MaxCount);
	});

	public void TestMaxCountValidation() => CombineAssertions(() =>
	{
		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var vehicles = JobComInvoiceLine.Vehicles;

		var vehicle1 = vehicles.AddNew();
		AssertNoRowMessageErrors("EXP - No message errors for 1st vehicle", vehicle1);

		var vehicle2 = vehicles.AddNew();
		AssertHasRowMessageError("EXP - NS30104 message error for 2nd vehicle", vehicle2, PassarValidationMessages.MessageNS30104_Vehicles);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertNoRowMessageErrors("IMP - No message errors for 2st vehicle", vehicle2);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertNoRowMessageErrors("EDA - No message errors for 2st vehicle", vehicle2);
	});

	protected override BusinessObjectCollection GetCollectionToTest() => JobComInvoiceLine.Vehicles as BusinessObjectCollection;

	JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
	JobDeclaration jobDeclaration;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
	JobComInvoiceLine jobComInvoiceLine;
}
