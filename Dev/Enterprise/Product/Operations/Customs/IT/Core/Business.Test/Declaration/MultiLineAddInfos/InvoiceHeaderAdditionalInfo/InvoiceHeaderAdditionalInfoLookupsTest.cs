using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceHeaderAdditionalInfoLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
{
	public void TestSubTypeList()
	{
		AssertType<EU.Business.AdditionalInfoSubTypeList>("Type", invoiceHeaderAdditionalInfo.Lookups.SubTypeList);
	}

	public void TestCodeListWhenSubTypeIsEmptyOrUnknown()
	{
		SetUpReferenceData();
		Factory.Save();

		CombineAssertions(() =>
		{
			invoiceHeaderAdditionalInfo.CSI_SubType = "";
			var codeList = (ZZRefCusCodeListCombinedCollection)invoiceHeaderAdditionalInfo.Lookups.CodeList;
			AssertEquals("When SubType is empty, CodeList", 0, codeList.Count);

			invoiceHeaderAdditionalInfo.CSI_SubType = "XXX";
			codeList = (ZZRefCusCodeListCombinedCollection)invoiceHeaderAdditionalInfo.Lookups.CodeList;
			AssertEquals("When SubType is unknown, CodeList", 0, codeList.Count);
		});
	}

	public void TestCodeListWhenSubTypeIsAdditionalInformation()
	{
		AssertCodeListBasedOnSubType(subType: "INF", expectedCode: "AI");
	}

	public void TestCodeListWhenSubTypeIsAdditionalReference()
	{
		AssertCodeListBasedOnSubType(subType: "REF", expectedCode: "AR");
	}

	public void TestCodeListWhenSubTypeIsTransportDocument()
	{
		AssertCodeListBasedOnSubType(subType: "TRA", expectedCode: "TD");
	}

	void AssertCodeListBasedOnSubType(string subType, string expectedCode)
	{
		SetUpReferenceData();
		Factory.Save();

		invoiceHeaderAdditionalInfo.CSI_SubType = subType;
		var refCusCodeListCollection = (ZZRefCusCodeListCombinedCollection)invoiceHeaderAdditionalInfo.Lookups.CodeList;
		AssertEquals("CodeList", expectedCode, refCusCodeListCollection.Select(x => x.ZZD_Code).Aggregate((a, b) => a + "," + b));
	}

	void SetUpReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("IT", "Italy");

		helper.CreateNewOrGetExistingCusCodeType("AI44E", "Additional Information Code Type");
		helper.CreateNewOrGetExistingCusCodeList("IT", "AI44E", "AI", "Additional Information Code List", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateNewOrGetExistingCusCodeType("AR44E", "Additional Reference Code Type");
		helper.CreateNewOrGetExistingCusCodeList("IT", "AR44E", "AR", "Additional Reference Code List", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateNewOrGetExistingCusCodeType("TD44E", "Transport Document Code Type");
		helper.CreateNewOrGetExistingCusCodeList("IT", "TD44E", "TD", "Transport Document Code List", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceHeaderAdditionalInfo = invoice.AdditionalInfos.AddNew();
	}

	InvoiceHeaderAdditionalInfo invoiceHeaderAdditionalInfo;
}
