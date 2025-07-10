using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Ireland)]
	sealed class IEDocSADHLineTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHLineTest
	{
		public override void TestBox40Contents()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
			documentOnHeader.CSI_SubType = "X";
			documentOnHeader.CSI_Code = "280";
			documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

			var documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "3421789012";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);

			var documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber = "20070701-120-A12345E";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);

			var documentOnGroup = declaration.PreviousDocuments.AddNew();
			documentOnGroup.CSI_SubType = "A";
			documentOnGroup.CSI_Code = "123";
			documentOnGroup.CSI_ReferenceNumber = "987654321";

			var line = IEDocSADHLine.New(entryLine, Factory);
			AssertEquals("entryLine.Box40PreviousDocuments", "Z-380-3421789012-02/01/2000; Y-CLE-20070701-120-A12345E-03/01/2000", line.Box40PreviousDocuments);
		}

		public override void TestBox44_1ProducedDocumentsCertificates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gbGroup = helper.CreateNewOrGetExistingDataGrouping("IE");

			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, exportCodeType, "H001", "Code 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, exportCodeType, "L001", "Code 2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, exportCodeType, "L002", "Code 3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var headerDoc = invoiceHeader.SupportingDocuments.AddNew();
			headerDoc.CSI_Code = "H001";
			headerDoc.CSI_ReferenceNumber = "Header Doc";

			var lineDoc1 = invoiceLine1.SupportingDocuments.AddNew();
			lineDoc1.CSI_Code = "L001";
			lineDoc1.CSI_ReferenceNumber = "Line Doc 1";

			var lineDoc2 = invoiceLine2.SupportingDocuments.AddNew();
			lineDoc2.CSI_Code = "L002";
			lineDoc2.CSI_ReferenceNumber = "Line Doc 2";

			var line1 = IEDocSADHLine.New(entryLine1, Factory);
			var line2 = IEDocSADHLine.New(entryLine2, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("entryLine1.Box44Contents", @"H001 Header Doc, L001 Line Doc 1", line1.Box44_1ProducedDocumentsCertificates);
				AssertEquals("entryLine2.Box44Contents", @"H001 Header Doc, L002 Line Doc 2", line2.Box44_1ProducedDocumentsCertificates);
			});
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			return IEDocSADHLine.New(entryLine, Factory);
		}

		protected override DocSADHLine GetSADHLineForBox47Taxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var feeA00 = entryLine.Fees.AddNew();
			feeA00.CF_ChargeType = "A00";
			feeA00.CF_BaseValue = 1105.89m;
			feeA00.CF_ChargeAmount = 29.85m;
			var feeB00 = entryLine.Fees.AddNew();
			feeB00.CF_ChargeType = "B00";
			feeB00.CF_BaseValue = 1266.03m;
			feeB00.CF_ChargeAmount = 221.55m;
			return IEDocSADHLine.New(entryLine, Factory);
		}

		protected override DocSADHLine GetSADHLineForBox47TaxesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			foreach (var taxType in TaxTypesToTestOrder)
			{
				entryLine.Fees.AddNew().CF_ChargeType = taxType;
			}
			return IEDocSADHLine.New(entryLine, Factory);
		}

		protected override void SetUpForInwardProcedure()
		{
			zzzDataGrouping = "IE";
			procedureCode = "10";
			previousProcedureCode = "71";
			concession = "F61";
			country = Core.Constants.CountryCodes.Ireland;
			customsRegNo = "A12345678GB";
			shipmentType = "IMP";
			group = "10P";
		}

		protected override void SetUpForOutwardProcedure()
		{
			zzzDataGrouping = "IE";
			procedureCode = "42";
			previousProcedureCode = "71";
			concession = "C33";
			country = Core.Constants.CountryCodes.Ireland;
			customsRegNo = "A12345678GB";
			shipmentType = "IMP";
			group = "42P";
		}

		protected override DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			return IEDocSADHLine.New((CusEntryLine)entryLine, Factory);
		}

		protected override bool GetExportCountryFromInvoiceLine => true;
	}
}
