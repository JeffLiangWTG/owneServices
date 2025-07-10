using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.DE.DataTransfer.Testing.Universal
{
	public class JobDeclarationDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestImportUXML_Bills_NewDeclaration()
		{
			AssertImportUXML_Bills(true);
		}

		public void TestImportUXML_Bills_UpdateExistingDeclaration()
		{
			AssertImportUXML_Bills(false);
		}

		void AssertImportUXML_Bills(bool importNewDeclaration)
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			declaration.JE_VesselName = "1 QEM 190";

			Factory.SaveForTesting();
			declaration.Bills.RemoveAndDeleteAll();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "001050716";

			declaration.Bills.Add(bill);

			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));

			var xmlData = writer.GetDataObject(declaration);

			if (importNewDeclaration)
			{
				declaration.IsCancelled = true;
				Factory.SaveForTesting();
			}

			var importedDeclaration = new JobDeclarationDataObjectReader(xmlData, new DummyLogger(), Factory, null).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(importedDeclaration.PK);

			CombineAssertions(() =>
			{
				if (importNewDeclaration)
				{
					AssertNotEquals("New Declaration should be created by importing shipment", declaration.PK, reloadedDeclaration.PK);
				}
				else
				{
					AssertEquals("Existing declaration should be updated by importing shipment", declaration.PK, reloadedDeclaration.PK);
				}

				AssertEquals("Only one bill should be present on the declaration", 1, reloadedDeclaration.FilteredBills.Count);
				var importedBill = reloadedDeclaration.FilteredBills[0];
				AssertEquals("Original bill should be the only one present on the declaration imported from UXML", "001050716", importedBill.CU_BillNum);
			});
		}

		public void TestCommercialInvoiceHeader_CustomsSupportingInformation_CodeDataObject_ShouldReadCodesInOriginalCase()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.SupportingDocuments.AddNew();
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
			var xmlData = writer.GetDataObject(declaration);

			var customsSupportingInformation = xmlData.CommercialInfo.CommercialInvoiceCollection[0].CustomsSupportingInformationCollection[0];
			customsSupportingInformation.UnitOfQuantity = new CodeDescriptionPair4Char { Code = "Uq" };
			customsSupportingInformation.UnitOfQuantity2 = new CodeDescriptionPair4Char { Code = "UQ" };
			customsSupportingInformation.UnitOfQuantity3 = new CodeDescriptionPair4Char { Code = "uq" };

			var declarationDataObject = new JobDeclarationDataObjectReader(xmlData, new DummyLogger(), Factory, null).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declarationDataObject.PK);
			var supportingDocument = reloadedDeclaration.Invoices[0].SupportingDocuments[0];

			AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity", "Uq", supportingDocument.CSI_UnitOfQuantity);
			AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity2", "UQ", supportingDocument.CSI_UnitOfQuantity2);
			AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity3", "uq", supportingDocument.CSI_UnitOfQuantity3);
		}
	}
}
