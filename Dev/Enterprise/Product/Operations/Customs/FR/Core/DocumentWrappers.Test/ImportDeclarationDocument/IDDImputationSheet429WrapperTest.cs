using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDDImputationSheet429WrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDDImputationSheet429Wrapper.New(supportingDocument, goodsItemNumber, Factory);
	}

	public void TestGoodsItemNumber()
	{
		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, "2", Factory);
		AssertEquals("GoodsItemNumber", "2", wrapper.GoodsItemNumber);
	}

	public void TestDocumentType()
	{
		supportingDocument.Type = "ABC";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("DocumentType", "ABC", wrapper.DocumentType);
	}

	public void TestDocumentReference()
	{
		supportingDocument.ReferenceNumber = "REF123";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("DocumentReference", "REF123", wrapper.DocumentReference);
	}

	public void TestLineItemNumber()
	{
		supportingDocument.DocumentLineItemNumber = "LINE456";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("LineItemNumber", "LINE456", wrapper.LineItemNumber);
	}

	public void TestInformation()
	{
		supportingDocument.ComplementOfInformation = "Additional Info";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("Information", "Additional Info", wrapper.Information);
	}

	public void TestQuantity()
	{
		supportingDocument.Quantity = 100;

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("Quantity", "100", wrapper.Quantity);
	}

	public void TestMeasurementUnitAndQualifier()
	{
		supportingDocument.MeasurementUnitAndQualifier = "KG";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("MeasurementUnitAndQualifier", "KG", wrapper.MeasurementUnitAndQualifier);
	}

	public void TestAmount()
	{
		supportingDocument.Amount = 250.75;

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("Amount", "250.75", wrapper.Amount);
	}

	public void TestCurrency()
	{
		supportingDocument.Currency = "USD";

		var wrapper = new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, Factory);
		AssertEquals("Currency", "USD", wrapper.Currency);
	}

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = new MSupportingDocumentType01FR();
		goodsItemNumber = "1";
	}

	MSupportingDocumentType01FR supportingDocument;
	ZString goodsItemNumber;
}
