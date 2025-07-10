using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ECCNCode))]
sealed class ECCNCodeTest : Customs.Business.Testing.CusCodeDataTest<ECCNCode>
{
	public void TestHumanReadableName()
	{
		var result = Factory.New<ECCNCode>();
		AssertEquals("Export Control Classification Number", result.HumanReadableName);
	}

	public void TestCY_Code_MaxLength()
	{
		var result = Factory.New<ECCNCode>();
		AssertEquals(10, result.CY_CodeInfo.MaxLength);
	}

	public void TestCY_Code_Caption()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		var eccnCode = line.ECCNCodes.AddNew();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(eccnCode.CY_CodeInfo);

		CombineAssertions(() =>
		{
			AssertEquals("CY_Code Short Caption", "Add. ECCN", captionResourceString.ShortCaption);
			AssertEquals("CY_Code Caption", "Additional ECCN Code", captionResourceString.Caption);
		});
	}

	public void TestSetDefaultValues()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		var eccnCode = line.ECCNCodes.AddNew();
		AssertEquals(CusCodeDataTypeList.Codes.ExportControlClassificationNumber, eccnCode.CY_Type);
	}

	public void TestParent()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		var eccnCode = line.ECCNCodes.AddNew();
		AssertType<JobComInvoiceLine>(eccnCode.Parent);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		return line.ECCNCodes.AddNew();
	}

	protected override IEnumerable<ECCNCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var result = factory.New<ECCNCode>();

		var declaration = factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		line.ECCNCodes.Add(result);

		yield return result;
	}
}
