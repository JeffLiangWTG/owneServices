
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Testing.CommonGoodsItemsIntegration;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DECommonGoodsItemsIntegratorTest : BaseCommonGoodsItemsIntegratorTest<JobDeclaration, Declaration.CusEntryHeader, DECommonGoodsItemsIntegrator, NctsHeaderToAttachCollection>
	{
		protected override (Customs.Business.CusEntryHeader, BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2) CreateEntryWithTwoInvoiceLines(
			BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryinstruction.PK;
			entryHeader.CH_CEI_Instruction = entryinstruction.PK;

			entryHeader.CH_BGMReference = "123ABC";
			entryHeader.MovementReferenceNumberSetter("MRN123");

			invoiceLine1.FillWithValidTestData();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryinstruction.PK;
			invoiceLine2.FillWithValidTestData();

			return (entryHeader, invoiceLine1, invoiceLine2);
		}

		protected override void AssertExpectedPropertiesOnGoodsItem(ICommonGoodsItem goodsItem, BaseJobComInvoiceLine line1)
		{
			AssertEquals("EntryNumber", ((ZString)"Z", (ZString)"N830", (ZString)"MRN123", (ZInt?)5), goodsItem.EntryNumber);
		}

		protected override ICommonGoodsItem CreateCommonGoodsItem()
		{
			return new CommonGoodsItem()
			{
				GoodsDescription = "description",
				GrossMass = 12000m,
				NetMass = 10m,
				GrossMassUnit = "G",
				NetMassUnit = "KG",
				CommodityCode = "1202.30.00 01",
				Value = 8m,
				JobReference = "jobreference",
				EntryNumber = (PreviousDocumentClassList.Codes.PreviousDocument, EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.Other, "MRN123", 1),
				DispatchCountry = "DE",
				DestinationCountry = "US",
			};
		}

		protected override void AssertExpectedPropertiesOnInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
		}
	}
}
