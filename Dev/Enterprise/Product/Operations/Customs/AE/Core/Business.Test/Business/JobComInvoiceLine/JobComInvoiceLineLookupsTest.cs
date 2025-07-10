using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
namespace Enterprise.Customs.AE.Business.Testing;

class JobComInvoiceLineLookupsTest : TestCaseWithFactory
{
	public void TestProcedures() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedArabEmirates,
			"United Arab Emirates");
		helper.CreateNewOrGetExistingDataGrouping(AEDeclarationApplicationCodeList.Codes.Dubai, "Dubai");
		var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedArabEmirates, "A",
			"AA", "00", "", "", "", "");
		var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedArabEmirates, "B",
			"AA", "01", "", "", "", "");
		var procedure3 = helper.CreateOrFindExistingRefCusProcedure(AEDeclarationApplicationCodeList.Codes.Dubai, "A",
			"BB", "02", "", "", "", "");
		var procedure4 = helper.CreateOrFindExistingRefCusProcedure(AEDeclarationApplicationCodeList.Codes.Dubai, "B",
			"BB", "03", "", "", "", "");
		Factory.Save();

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		var procedures = lookups.Procedures;
		AssertContainsExactElementsInExactOrder(
			procedures,
			new[] { procedure1, procedure2 }
		);

		declaration.JE_ApplicationCode = AEDeclarationApplicationCodeList.Codes.Dubai;
		procedures = lookups.Procedures;
		AssertContainsExactElementsInExactOrder(
			procedures,
			new[] { procedure3, procedure4 }
		);
	});

	public void TestGoodsConditionList() => CombineAssertions(() =>
	{
		var cached = lookups.GoodsConditionList;
		AssertContainsExactElementsInExactOrder(["N", "U"], cached.GetAllCodes());
		AssertSame(cached, lookups.GoodsConditionList);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		lookups = invoiceLine.Lookups;
	}
	JobDeclaration declaration;
	JobComInvoiceLineLookups lookups;
}
