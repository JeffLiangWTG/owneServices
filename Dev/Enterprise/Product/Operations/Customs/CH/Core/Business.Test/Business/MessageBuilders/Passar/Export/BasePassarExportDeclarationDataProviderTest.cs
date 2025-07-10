using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BasePassarExportDeclarationDataProvider))]
abstract class BasePassarExportDeclarationDataProviderTest<TDataProvider> : BasePassarMessageDataProviderTest<TDataProvider>
														where TDataProvider : BasePassarExportDeclarationDataProvider
{
	public void TestExportOperation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.ExportOperation);
		AssertSame("cached", DataProvider.ExportOperation, DataProvider.ExportOperation);
	});

	public void TestIntendedUse() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.IntendedUse);
		AssertSame("cached", DataProvider.IntendedUse, DataProvider.IntendedUse);
	});

	public void TestConsignment() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.Consignment);
		AssertSame("cached", DataProvider.Consignment, DataProvider.Consignment);
	});

	public void TestRepresentative() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID123");
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO456");
		var orgAddress = orgHeader.Addresses.AddNew();
		Declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		var user = Factory.New<GlbStaff>();
		user.GS_Code = "JHN";
		user.GS_FullName = "John";
		Declaration.JE_GS_NKCusAgent = user.GS_Code;

		AssertNotNull(DataProvider.Representative);
		AssertSame("cached", DataProvider.Representative, DataProvider.Representative);

		AssertEquals("BID123", DataProvider.Representative.IdentificationNumber);
		AssertEquals("AEO456", DataProvider.Representative.AeoReferenceNumber);
		AssertEquals("John", DataProvider.Representative.ContactPerson.Name);
	});

	public void TestFinanceData() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.FinanceData);
		AssertSame("cached", DataProvider.FinanceData, DataProvider.FinanceData);
	});
}
