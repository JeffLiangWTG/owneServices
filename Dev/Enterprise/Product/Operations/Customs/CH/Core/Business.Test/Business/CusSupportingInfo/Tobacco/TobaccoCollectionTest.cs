using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TobaccoCollection))]
sealed class TobaccoCollectionTest : BusinessObjectCollectionTestCase
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
		var tobaccos = JobComInvoiceLine.Tobaccos;

		var tobacco1 = tobaccos.AddNew();
		AssertNoRowMessageErrors("EXP - No message errors for 1st tobacco", tobacco1);

		var tobacco2 = tobaccos.AddNew();
		AssertHasRowMessageError("EXP - NS30104 message error for 2nd tobacco", tobacco2, PassarValidationMessages.MessageNS30104_Tobacco);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertNoRowMessageErrors("IMP - No message errors for 2st tobacco", tobacco2);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertNoRowMessageErrors("EDA - No message errors for 2st tobacco", tobacco2);
	});

	protected override BusinessObjectCollection GetCollectionToTest() => JobComInvoiceLine.Tobaccos;

	JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
	JobDeclaration jobDeclaration;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
	JobComInvoiceLine jobComInvoiceLine;
}
