using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

partial class UniversalCustomsDataObjectProviderTest
{
	public void TestCusSupportingInfoGetListForJobComInvoiceHeader() => AssertCusSupportingInfoGetList(JobComInvoiceHeaderSchema.Constants.Prefix, "PRE, SUP, TRA");

	public void TestCusSupportingInfoGetListForJobComInvoiceLine() => AssertCusSupportingInfoGetList(JobComInvoiceLineSchema.Constants.Prefix, "PRE, SUP, RST, IOP, GID");

	public void TestCusSupportingInfoGetListForEntryInstruction() => AssertCusSupportingInfoGetList(CusEntryInstructionSchema.Constants.Prefix, "PRE, SUP, TRA, GID");

	static void AssertCusSupportingInfoGetList(string tableCode, string expectedCodesAsString)
	{
		var list = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusSupportingInfoTypeList(tableCode, "");
		AssertEquals(expectedCodesAsString, list.CodesAsString);
	}
}
