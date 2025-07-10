using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewStandaloneCommercialInvoiceDataObjectReader()
		{
			AssertType(typeof(StandaloneCommercialInvoiceDataObjectReader), new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader(), new TestErrorLogger(), Factory));
		}

		public void TestStandaloneCommercialInvoiceDataObjectWriter()
		{
			AssertType(typeof(StandaloneCommercialInvoiceDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobComInvoiceHeader>()))));
		}

		public void TestTableSpecificCusAddInfoTypeListEndToEnd()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var dutyAndTax = invoiceLine.DutiesAndTaxes.AddNew();
			dutyAndTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			var dummyAddInfo = invoiceLine.DutiesAndTaxes.AddNew();
			dummyAddInfo.B7_Type = "!@#";
			dummyAddInfo.C1_Code = "1";
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(1, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoice = dataObject.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals(1, commercialInvoice.CommercialInvoiceLineCollection.Count);
			var commercialInvoiceLine = commercialInvoice.CommercialInvoiceLineCollection[0];
			AssertEquals("Invalid types for invoiceline should not be included", 1, commercialInvoiceLine.AddInfoGroupCollection.Count);
			var addInfoGroup = commercialInvoiceLine.AddInfoGroupCollection[0];
			AssertEquals("addInfoGroup.Type.Code", Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.CADutyAndTax, addInfoGroup.Type.GetCodeAsUpperCase());
			AssertEquals("addInfoGroup.Type.Description", "Duty And Tax", addInfoGroup.Type.Description.GetValueOrDefault());
		}

		public void TestTableSpecificCusCodeDataCodeListEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var permit1 = invoiceLine.Permits.AddNew();
			permit1.CY_Code = CusCodeDataTypeList.Codes.Permit;
			permit1.CY_Data = "1";
			var permit2 = invoiceLine.Permits.AddNew();
			permit2.CY_Code = CusCodeDataTypeList.Codes.CCN;
			permit2.CY_Data = "2";
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(1, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoice = dataObject.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals(1, commercialInvoice.CommercialInvoiceLineCollection.Count);
			var commercialInvoiceLine = commercialInvoice.CommercialInvoiceLineCollection[0];
			AssertEquals(2, commercialInvoiceLine.CustomsReferenceCollection.Count);
			var customsReference1 = commercialInvoiceLine.CustomsReferenceCollection[0];
			var customsReference2 = commercialInvoiceLine.CustomsReferenceCollection[1];
			if (customsReference2.SubType.GetCodeAsUpperCase() == CusCodeDataTypeList.Codes.Permit)
			{
				customsReference1 = commercialInvoiceLine.CustomsReferenceCollection[1];
				customsReference2 = commercialInvoiceLine.CustomsReferenceCollection[0];
			}
			AssertEquals("customsReference1.SubType.Code", CusCodeDataTypeList.Codes.Permit, customsReference1.SubType.GetCodeAsUpperCase());
			AssertEquals("customsReference1.SubType.Description", CusCodeDataTypeList.Descriptions.Permit, customsReference1.SubType.Description);
			AssertEquals("customsReference2.SubType.Code", CusCodeDataTypeList.Codes.CCN, customsReference2.SubType.GetCodeAsUpperCase());
			AssertNull("customsReference2.SubType.Description", customsReference2.SubType.Description);
		}

		public void TestTableSpecificCusCodeDataTypeListEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var permit1 = invoiceLine.Permits.AddNew();
			permit1.CY_Type = CusCodeDataTypeList.Codes.Permit;
			permit1.CY_Data = "1";
			var permit2 = invoiceLine.Permits.AddNew();
			permit2.CY_Type = CusCodeDataTypeList.Codes.CCN;
			permit2.CY_Data = "2";
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(1, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoice = dataObject.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals(1, commercialInvoice.CommercialInvoiceLineCollection.Count);
			var commercialInvoiceLine = commercialInvoice.CommercialInvoiceLineCollection[0];
			AssertEquals(1, commercialInvoiceLine.CustomsReferenceCollection.Count);
			var customsReference = commercialInvoiceLine.CustomsReferenceCollection[0];
			AssertEquals("customsReference.Type.Code", CusCodeDataTypeList.Codes.Permit, customsReference.Type.GetCodeAsUpperCase());
			AssertEquals("customsReference.Type.Description", CusCodeDataTypeList.Descriptions.Permit, customsReference.Type.Description);
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			var helper = new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Canada);
			AssertEquals(typeof(UniversalDataObjectReaderHelper), helper.GetType());
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
