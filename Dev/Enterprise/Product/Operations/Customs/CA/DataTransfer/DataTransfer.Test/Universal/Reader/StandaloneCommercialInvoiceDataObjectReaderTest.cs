using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestDefaultSIMAInfoAndPGAFlagFromProduct()
		{
			var importer = CreateOrganisation("IMP", "ABC!@#12");
			var supplier = CreateOrganisation("SUP", "ABC!@#13");

			var part = Factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddOwner(importer);
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = Business.ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "1234567890";
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			var add = importPivot.DutiesAndTaxes.AddNew();
			add.C1_TaxType = Business.DutyAndTaxTypes.Codes.ADD;
			add.C1_Rate = 101m;
			var cvd = importPivot.DutiesAndTaxes.AddNew();
			cvd.C1_TaxType = Business.DutyAndTaxTypes.Codes.CVD;
			cvd.C1_ForeignRate = 20m;
			importPivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			importPivot.HCPGAHeader.CA_DSEProgramInd = YesNoList.Codes.Yes;
			importPivot.HCPGAHeader.CA_CPRProgramInd = YesNoList.Codes.No;

			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoice.InvoiceNumber = "INVABC123";
			invoice.Supplier = invoice.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			invoice.Buyer = invoice.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country() { Code = "US" },
					PartNo = part.OP_PartNum,
					HarmonisedCode = "1234567890"
				}
			});

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = Business.JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoice }))
			};

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoice, Logger, Factory);
				var invoiceBO = reader.ReadIntoBusinessObject();
				var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];
				var dutiesAndTaxes = invoiceLineBO.DutiesAndTaxes;
				AssertEquals(2, dutiesAndTaxes.Count);
				AssertEquals(101m, dutiesAndTaxes.First(x => x.C1_TaxType == Business.DutyAndTaxTypes.Codes.ADD).C1_Rate);
				AssertEquals(20m, dutiesAndTaxes.First(x => x.C1_TaxType == Business.DutyAndTaxTypes.Codes.CVD).C1_ForeignRate);
				AssertEquals(YesNoList.Codes.Yes, invoiceLineBO.CA_HCInd);
				AssertEquals(YesNoList.Codes.Yes, invoiceLineBO.HCPGAHeader.CA_DSEProgramInd);
				AssertEquals(YesNoList.Codes.No, invoiceLineBO.HCPGAHeader.CA_CPRProgramInd);
			}
		}

		public void TestImportStandaloneCommercialInvoiceDataForStateOfOrigin()
		{
			var importer = CreateOrganisation("IMP", "ABC!@#12");
			var supplier = CreateOrganisation("SUP", "ABC!@#13");

			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoice.InvoiceNumber = "INVABC123";
			invoice.InvoiceAmount = 150m;
			invoice.Supplier = invoice.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			invoice.Buyer = invoice.AddOrgAddress(writeManager, importer, AddressTypes.Importer);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			var addInfo = new UniversalAddInfo()
			{
				Key = "ProvinceOfOrigin",
				Value = "ON"
			};
			invoiceLine.AddInfoCollection.Add(addInfo);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = Business.JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoice }))
			};

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoice, Logger, Factory);
			var invoiceBO = reader.ReadIntoBusinessObject();
			var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];

			AssertEquals("JI_StateOrRegionOfOrigin Exists", "ON", invoiceLineBO.JI_StateOrRegionOfOrigin);
		}
	}
}
