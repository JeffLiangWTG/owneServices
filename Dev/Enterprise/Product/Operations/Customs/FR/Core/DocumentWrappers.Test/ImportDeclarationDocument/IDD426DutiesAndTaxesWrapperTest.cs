using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDD426DutiesAndTaxesWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var dutiesAndTaxes = new DutiesAndTaxesType();
		var dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
		return IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
	}

	public void TestNationalTaxType()
	{
		dutiesAndTaxes.NationalTaxType = "ABC";

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("NationalTaxType", "ABC", wrapper.NationalTaxType);
	}

	public void TestTaxType()
	{
		dutiesAndTaxes.TaxType = "VAT";

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("TaxType", "VAT", wrapper.TaxType);
	}

	public void TestPayableTaxAmount()
	{
		dutiesAndTaxes.PayableTaxAmount = 100.50;

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("PayableTaxAmount", "100.5", wrapper.PayableTaxAmount);
	}

	public void TestAmount()
	{
		var taxBaseItem = new MTaxBaseType01 { Amount = 200.75 };
		dutiesAndTaxes.TaxBase = new List<MTaxBaseType01> { taxBaseItem };

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("Amount", "200.75", wrapper.Amount);
	}

	public void TestTaxRate()
	{
		var taxBaseItem = new MTaxBaseType01 { TaxRate = 5.0 };
		dutiesAndTaxes.TaxBase = new List<MTaxBaseType01> { taxBaseItem };

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("TaxRate", "5", wrapper.TaxRate);
	}

	public void TestTaxAmount()
	{
		var taxBaseItem = new MTaxBaseType01 { TaxAmount = 10.25 };
		dutiesAndTaxes.TaxBase = new List<MTaxBaseType01> { taxBaseItem };

		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("TaxAmount", "10.25", wrapper.TaxAmount);
	}

	public void TestStatus()
	{
		dutiesAndTaxes.NationalTaxType = "ABC";
		dutiesAndTaxesSummaries.Add(new DutiesAndTaxesSummariesType
		{
			NationalTaxType = "ABC",
			TaxationStatus = "status"
		});
		var wrapper = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		AssertEquals("TaxationStatus", "status", wrapper.Status);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dutiesAndTaxes = new DutiesAndTaxesType();
		dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
	}

	DutiesAndTaxesType dutiesAndTaxes;
	ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;
}
