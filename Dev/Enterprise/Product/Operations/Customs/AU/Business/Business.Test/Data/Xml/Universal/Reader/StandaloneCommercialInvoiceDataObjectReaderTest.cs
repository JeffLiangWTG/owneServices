using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestTILVAddInfo()
		{
			var buyer = CreateOrganisation("JOO", "ABC!@#12");
			var supplier2 = CreateOrganisation("GARY", "ABC!@#13");

			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoice.InvoiceNumber = "INVABC123";
			invoice.InvoiceAmount = 150m;
			invoice.Supplier = invoice.AddOrgAddress(writeManager, supplier2, AddressTypes.Supplier);
			invoice.Buyer = invoice.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			var addInfo = new UniversalAddInfo();
			invoiceLine.AddInfoCollection.Add(addInfo);
			addInfo.Key = UniversalExtensions.TILV4Warehouse;
			addInfo.Value = "100.00";
			addInfo = new UniversalAddInfo();
			invoiceLine.AddInfoCollection.Add(addInfo);
			addInfo.Key = AutoAUAddInfo.Schema.ZA_REL_Hidden.Substring(3);
			addInfo.Value = CMRRelatedTransaction.Yes.Code;

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoice }))
			};

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoice, Logger, Factory);
			var invoiceBO = reader.ReadIntoBusinessObject();
			var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];
			AssertContains(AutoAUAddInfo.Schema.ZA_REL_Hidden.Substring(3) + "=" + CMRRelatedTransaction.Yes.Code, invoiceLineBO.JI_AddInfo);
			AssertNotContains(UniversalExtensions.TILV4Warehouse + "=100.00AUD", invoiceLineBO.JI_AddInfo);
		}
	}
}
