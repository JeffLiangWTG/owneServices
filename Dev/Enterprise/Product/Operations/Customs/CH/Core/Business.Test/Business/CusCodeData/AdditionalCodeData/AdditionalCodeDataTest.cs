using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalCodeData))]
public class AdditionalCodeDataTest : SingleCusCodeDataTest<AdditionalCodeData>
{
	public void TestCY_Type()
	{
		var code = Factory.New<AdditionalCodeData>();
		AssertEquals(code.CY_Type, CusCodeDataTypeList.Codes.AdditionalCode);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var additionalCodeData = GetNewCusCodeData(factory);
		additionalCodeData.CY_Code = "X";
		return additionalCodeData;
	}

	protected override IEnumerable<AdditionalCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObjectForDeleteTest(factory) as AdditionalCodeData;
	}

	protected override AdditionalCodeData GetNewCusCodeData(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalCodeData = factory.New<AdditionalCodeData>();
		additionalCodeData.Parent = invoiceLine;
		return additionalCodeData;
	}

	protected override IEnumerable<string> GetUsedFieldsNames()
	{
		yield return nameof(AdditionalCodeData.CY_Code);
		yield return nameof(AdditionalCodeData.CY_IsOverridden);
	}
}
