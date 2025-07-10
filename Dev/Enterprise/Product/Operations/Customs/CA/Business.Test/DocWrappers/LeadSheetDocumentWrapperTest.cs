using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LeadSheetDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			var wrapper = new LeadSheetDocumentWrapper(declaration);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("LeadSheetDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", declaration.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestWrapperMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER NAME";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "IMPORTER NUMBER", Core.Constants.CountryCodes.Canada);
			declaration.JE_OH_Importer = importer.PK;

			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "1234567";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_PageNumber = 1;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_PageNumber = 2;

			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN1";
			declaration.JE_LocationOfGoods = "LOG";
			declaration.CA_CarrierName = "CARRIER NAME";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2013, 01, 01, 1, 1, 1);

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CON1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CON2";

			declaration.JE_TotalWeight = 100m;
			declaration.JE_TotalWeightUnit = "KG";

			var mb1 = declaration.Bills.AddNew();
			mb1.CU_BillType = BillTypeList.Codes.MasterBill;
			mb1.CU_BillNum = "MB1";
			var hb1 = declaration.Bills.AddNew();
			hb1.CU_BillType = BillTypeList.Codes.HouseBill;
			hb1.CU_BillNum = "HB1";
			hb1.CU_CU_ParentBill = mb1.PK;

			var mb2 = declaration.Bills.AddNew();
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			mb2.CU_BillNum = "MB2";
			var hb2 = declaration.Bills.AddNew();
			hb2.CU_BillType = BillTypeList.Codes.HouseBill;
			hb2.CU_BillNum = "HB2";
			hb2.CU_CU_ParentBill = mb2.PK;

			declaration.JE_OwnerRef = "OWNER REF";
			var comment = @"A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT
A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT
A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT
A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT A VERY LONG COMMENT";
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.LeadSheetComments.Description, comment);

			declaration.JE_CustomsOffice = "0009";
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;

			var wrapper = new LeadSheetDocumentWrapper(declaration);
			AssertEquals("IMPORTER NAME", wrapper.ImporterName);
			AssertEquals("IMPORTER NUMBER", wrapper.ImporterNumber);
			AssertEquals("12345012345672", wrapper.TransactionNumber);
			AssertEquals("2", wrapper.NoOfInvoicePages);
			AssertEquals("CCN1", wrapper.CargoControlNumber);
			AssertEquals("LOG", wrapper.LocationOfGoods);
			AssertEquals(string.Format("CARRIER NAME{0}1/Jan/2013 1:01 AM", System.Environment.NewLine), wrapper.ArrivingPer);
			AssertEquals("CON1, CON2", wrapper.ContainerNumber);
			AssertEquals("100 KG", wrapper.Weight);
			AssertEquals(string.Format("MB1{0}HB1{0}MB2{0}HB2", System.Environment.NewLine), wrapper.BLNumber);
			AssertEquals("OWNER REF", wrapper.Reference);
			AssertEquals(comment, wrapper.Miscellaneous);
			AssertEquals("9", wrapper.Port);
			AssertEquals("PARS", wrapper.PARS);

			var ccn = declaration.AdditionalReferenceNumbers.AddNew();
			ccn.CE_EntryType = CusCodeDataTypeList.Codes.CCN;
			ccn.CE_EntryNum = "CCN2";
			AssertEquals("CCN2", wrapper.CargoControlNumber);

			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.OH_FullName = "IMPORTER OR RECORD";
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "IMPORTER OR RECORD NO", Core.Constants.CountryCodes.Canada);
			importerOfRecord.MainAddress.OA_CompanyNameOverride = "IMPORTER OR RECORD OVERRIDE NAME";
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			AssertEquals("IMPORTER OR RECORD OVERRIDE NAME", wrapper.ImporterName);
			AssertEquals("IMPORTER OR RECORD NO", wrapper.ImporterNumber);
		}
	}
}
