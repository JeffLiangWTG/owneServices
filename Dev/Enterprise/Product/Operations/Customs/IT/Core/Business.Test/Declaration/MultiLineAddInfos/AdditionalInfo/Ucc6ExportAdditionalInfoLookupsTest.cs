using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOmitLevelAttribute()
	{
		var lookups = new Ucc6ExportAdditionalInfoLookupsForTest(invoiceLineAdditionalInfo);
		AssertEquals(true, lookups.OmitLevelAttributeExposed);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	AdditionalInfo invoiceLineAdditionalInfo;

	sealed class Ucc6ExportAdditionalInfoLookupsForTest : Ucc6ExportAdditionalInfoLookups
	{
		public Ucc6ExportAdditionalInfoLookupsForTest(AdditionalInfo parent) : base(parent)
		{
		}

		public ZBool OmitLevelAttributeExposed => OmitLevelAttribute;
	}
}
