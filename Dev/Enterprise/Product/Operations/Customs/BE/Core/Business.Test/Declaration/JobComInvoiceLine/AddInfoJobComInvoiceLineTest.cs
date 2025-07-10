using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(AddInfoJobComInvoiceLine))]
sealed class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
{
	public void TestParent()
	{
		AssertType<JobComInvoiceLine>(((AddInfoJobComInvoiceLine)GetNewBusinessObject()).Parent);
	}

	public void TestGetNewValidation_Import()
	{
		AssertType<ImportAddInfoJobComInvoiceLineValidation>(GetInvoiceLine(MessageTypeList.Codes.Import).AddInfoValidation);
	}

	public void TestGetNewValidation_Export()
	{
		AssertType<AddInfoJobComInvoiceLineValidation>(GetInvoiceLine(MessageTypeList.Codes.Export).AddInfoValidation);
	}

	public void TestGetNewValidation_Miscellaneous()
	{
		AssertType<AddInfoJobComInvoiceLineValidation>(GetInvoiceLine(MessageTypeList.Codes.MiscellaneousCustoms).AddInfoValidation);
	}

	public void TestPropertyAttributes()
	{
		var addInfoJobComInvoiceLine = (AddInfoJobComInvoiceLine)GetNewBusinessObject();
		AssertEquals("ZG_CountryOfDestination: List", "Lookups.CountryOfDestinationList", addInfoJobComInvoiceLine.ZG_CountryOfDestinationInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public void TestZG_RegionOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertEquals(1, invoiceLine.ZG_RegionOfDestinationInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(GetInvoiceLine().JI_AddInfoInfo);

	JobComInvoiceLine GetInvoiceLine(string messageType = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (messageType != null)
		{
			declaration.JE_MessageType = messageType;
		}
		var invoice = declaration.Invoices.AddNew();
		return invoice.InvoiceLines.AddNew();
	}
}
