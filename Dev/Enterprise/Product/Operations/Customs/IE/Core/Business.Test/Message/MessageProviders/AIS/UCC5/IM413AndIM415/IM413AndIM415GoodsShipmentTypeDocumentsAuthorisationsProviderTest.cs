using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider>
	{
		public void TestIGoodsShipmentTypeDocumentsAuthorisations()
		{
			Assert("Should implement IGoodsShipmentTypeDocumentsAuthorisations", Provider is IGoodsShipmentTypeDocumentsAuthorisations);
		}

		public void TestWarehouse_Normal()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			var toWarehouseOwnerAddress = toWarehouseOwner.MainAddress;
			toWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U18033IE", Core.Constants.CountryCodes.Ireland);
			instruction.CEI_OA_Warehouse2 = toWarehouseOwnerAddress.PK;

			var toWarehouseUsage = instruction.CusAuthorizationUsages.AddNew();
			toWarehouseUsage.AGC_Code = "CW1";
			toWarehouseUsage.AGC_Number = "R17017IE";
			toWarehouseUsage.AGC_OH_Owner = toWarehouseOwner.PK;

			var warehouse = Provider.Warehouse;
			AssertEquals("Type", "R", warehouse.Type);
			AssertEquals("Id", "U18033IE", warehouse.Id);
			AssertSame("Cached", warehouse, Provider.Warehouse);
		}

		public void TestWarehouse_WhenTypeIsEmpty()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			var toWarehouseOwnerAddress = toWarehouseOwner.MainAddress;
			toWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U18033IE", Core.Constants.CountryCodes.Ireland);
			instruction.CEI_OA_Warehouse2 = toWarehouseOwnerAddress.PK;

			AssertNull("Warehouse should be null when procedure code is not 0700", Provider.Warehouse);

			invoiceLine.JI_Procedure = "0700";

			var warehouse = GetProvider().Warehouse;
			AssertNotNull("Warehouse should NOT be null when procedure code is 0700 and Id is set", warehouse);
			AssertEquals("Type", "Y", warehouse.Type);
			AssertEquals("Id", "U18033IE", warehouse.Id);
		}

		public void TestWarehouse_WhenIdIsEmpty()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			instruction.CEI_OA_Warehouse2 = toWarehouseOwner.MainAddress.PK;

			var toWarehouseUsage = instruction.CusAuthorizationUsages.AddNew();
			toWarehouseUsage.AGC_Code = "CW1";
			toWarehouseUsage.AGC_Number = "R17017IE";
			toWarehouseUsage.AGC_OH_Owner = toWarehouseOwner.PK;

			AssertNull(Provider.Warehouse);
		}

		public void TestUCR()
		{
			SetUpTestData();

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_UCR = "IMPUCR001";
			invoiceHeader.JZ_UCR = "FALLBACK001";
			AssertEquals("Reference Number UCR", "IMPUCR001", Provider.UCR);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("Reference Number UCR", "IMPUCR001", Provider.UCR);

			declaration.JE_UCR = string.Empty;
			AssertEquals("Reference Number UCR - JE_UCR empty, fallback to JZ_UCR", "FALLBACK001", Provider.UCR);

			invoiceHeader.JZ_UCR = string.Empty;
			AssertEquals("Reference Number UCR - both empty", string.Empty, Provider.UCR);
		}

		public void TestProducedDocuments()
		{
			SetUpTestData();
			var doc1 = instruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_AdditionalDescription = "DOC001";
			doc1.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1.CSI_ReferenceNumber = "REF001";

			var doc2 = instruction.SupportingDocuments.AddNew();
			doc2.CSI_Code = "SD2";
			doc2.CSI_ItemNumber = 2;
			doc2.CSI_AdditionalDescription = "DOC002";
			doc2.CSI_DateOfExpiry = ZDate.BrettsBirthday.AddDays(1);
			doc2.CSI_ReferenceNumber = "REF002";

			var doc3 = invoiceHeader.SupportingDocuments.AddNew();
			doc3.CSI_Code = "SD3";
			doc3.CSI_ItemNumber = 3;
			doc3.CSI_AdditionalDescription = "DOC003";
			doc3.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc3.CSI_ReferenceNumber = "REF003";

			var doc1Duplicate = invoiceHeader.SupportingDocuments.AddNew();
			doc1Duplicate.CSI_Code = "SD1";
			doc1Duplicate.CSI_ItemNumber = 1;
			doc1Duplicate.CSI_AdditionalDescription = "DOC001";
			doc1Duplicate.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1Duplicate.CSI_ReferenceNumber = "REF001";

			AssertContainsExactElementsInAnyOrder(new[] { "SD1|REF001", "SD2|REF002", "SD3|REF003", }, Provider.ProducedDocuments.Select(x => x.Type + "|" + x.Id));
		}

		public void TestSimplifiedDeclarationDocuments()
		{
			SetUpTestData();
			var doc1 = invoiceHeader.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";
			doc1.CSI_LineNo = 1;

			var doc2 = instruction.PreviousDocuments.AddNew();
			doc2.CSI_Code = "PD2";
			doc2.CSI_ReferenceNumber = "DOC002";
			doc2.CSI_LineNo = 2;

			var doc3 = invoiceHeader.PreviousDocuments.AddNew();
			doc3.CSI_Code = "PD3";
			doc3.CSI_ReferenceNumber = "DOC003";
			doc3.CSI_LineNo = 3;

			var doc1Duplicate = invoiceHeader.PreviousDocuments.AddNew();
			doc1Duplicate.CSI_Code = "PD1";
			doc1Duplicate.CSI_ReferenceNumber = "DOC001";
			doc1Duplicate.CSI_LineNo = 1;

			AssertContainsExactElementsInAnyOrder(new[] { "PD1|DOC001|1", "PD2|DOC002|2", "PD3|DOC003|3" }, Provider.SimplifiedDeclarationDocuments.Select(x => x.PreviousDocumentType + "|" + x.PreviousDocumentIdentifier + "|" + x.PreviousDocumentLineId));
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var inf1 = instruction.AdditionalInfos.AddNew();
			inf1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1.CSI_Code = "AI1";
			inf1.CSI_Description = "DESC001";

			var inf2 = instruction.AdditionalInfos.AddNew();
			inf2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf2.CSI_Code = "AI2";
			inf2.CSI_Description = "DESC002";

			var inf3 = invoiceHeader.AdditionalInfos.AddNew();
			inf3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf3.CSI_Code = "AI3";
			inf3.CSI_Description = "DESC003";

			var inf1Duplicate = invoiceHeader.AdditionalInfos.AddNew();
			inf1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1Duplicate.CSI_Code = "AI1";
			inf1Duplicate.CSI_Description = "DESC001";

			AssertContainsExactElementsInAnyOrder(new[] { "AI1|DESC001", "AI2|DESC002", "AI3|DESC003", }, Provider.AdditionalInformations.Select(x => x.Code + "|" + x.Text));
		}

		protected override IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider(new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
