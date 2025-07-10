using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	sealed class IADClearanceSlipTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper() => IADClearanceSlip.New(entryHeader, Factory);

		public void TestMovementReferenceNumber()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MovementReferenceNumber", "MRN001", Wrapper.MovementReferenceNumber);
		}

		[TestDate(2024, 6, 26)]
		public void TestPrintDate()
		{
			AssertEquals("PrintDate", new ZDateTime(2024, 6, 26), Wrapper.PrintDate);
		}

		public void TestIssuingDate()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001", new ZDateTime(2024, 6, 20));
			AssertEquals("IssuingDate", new ZDateTime(2024, 6, 20), Wrapper.IssuingDate);
		}

		public void TestRouting_IsOrangeForIM460()
		{
			AssertEquals("Routing, no message.", string.Empty, Wrapper.Routing);

			var im460Message = Factory.New<AESInboundEDIMessage>();
			entryHeader.Messages.Add(im460Message);
			im460Message.EM_Status = EDIMessage.Status.ProcessedOK;
			im460Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im460Message.EM_MessageType = "460";
			im460Message.EM_MessageText = @"<q1:IM460 xmlns:q1=""http://www.ros.ie/schemas/customs/IM460"">
  <q1:Declaration>
    <q1:MRN>12MRN345CDEFG678R9</q1:MRN>
    <q1:ControlNotificationDate>202402202359GMT</q1:ControlNotificationDate>
    <q1:TimeLimitForControl>202403071437GMT</q1:TimeLimitForControl>
    <q1:CustomsOffices>
      <q1:CustomsOfficeLodgement>OF123456</q1:CustomsOfficeLodgement>
    </q1:CustomsOffices>
  </q1:Declaration>
  <q1:OverallControlType>
    <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
  </q1:OverallControlType>
  <q1:GoodsShipment>
    <q1:GoodsShipmentItem>
      <q1:GoodsItemNumber_1_6>1</q1:GoodsItemNumber_1_6>
      <q1:ControlType>
        <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue</q1:ControlAgency>
      </q1:ControlType>
    </q1:GoodsShipmentItem>
    <q1:GoodsShipmentItem>
      <q1:GoodsItemNumber_1_6>2</q1:GoodsItemNumber_1_6>
      <q1:ControlType>
        <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue</q1:ControlAgency>
      </q1:ControlType>
      <q1:ControlType>
        <q1:ControlTypeCoded>1</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue1</q1:ControlAgency>
      </q1:ControlType>
    </q1:GoodsShipmentItem>
  </q1:GoodsShipment>
</q1:IM460>";

			AssertEquals("Routing, message containing ControlTypeCoded", "ORANGE", Wrapper.Routing);
		}

		public void TestRouting_IsGreenWhenIM429IsPresent()
		{
			AssertEquals("Routing, no message.", string.Empty, Wrapper.Routing);

			var im429Message = Factory.New<AESInboundEDIMessage>();
			entryHeader.Messages.Add(im429Message);
			im429Message.EM_Status = EDIMessage.Status.ProcessedOK;
			im429Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im429Message.EM_MessageType = "429";

			AssertEquals("When a IM429 is present.", "GREEN", Wrapper.Routing);

			var im460Message = Factory.New<AESInboundEDIMessage>();
			entryHeader.Messages.Add(im460Message);
			im460Message.EM_Status = EDIMessage.Status.ProcessedOK;
			im460Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im460Message.EM_MessageType = "460";
			im460Message.EM_MessageText = @"<q1:IM460 xmlns:q1=""http://www.ros.ie/schemas/customs/IM460"">
  <q1:Declaration>
    <q1:MRN>12MRN345CDEFG678R9</q1:MRN>
    <q1:ControlNotificationDate>202402202359GMT</q1:ControlNotificationDate>
    <q1:TimeLimitForControl>202403071437GMT</q1:TimeLimitForControl>
    <q1:CustomsOffices>
      <q1:CustomsOfficeLodgement>OF123456</q1:CustomsOfficeLodgement>
    </q1:CustomsOffices>
  </q1:Declaration>
  <q1:OverallControlType>
    <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
  </q1:OverallControlType>
  <q1:GoodsShipment>
    <q1:GoodsShipmentItem>
      <q1:GoodsItemNumber_1_6>1</q1:GoodsItemNumber_1_6>
      <q1:ControlType>
        <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue</q1:ControlAgency>
      </q1:ControlType>
    </q1:GoodsShipmentItem>
    <q1:GoodsShipmentItem>
      <q1:GoodsItemNumber_1_6>2</q1:GoodsItemNumber_1_6>
      <q1:ControlType>
        <q1:ControlTypeCoded>O</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue</q1:ControlAgency>
      </q1:ControlType>
      <q1:ControlType>
        <q1:ControlTypeCoded>1</q1:ControlTypeCoded>
        <q1:ControlAgency>Revenue1</q1:ControlAgency>
      </q1:ControlType>
    </q1:GoodsShipmentItem>
  </q1:GoodsShipment>
</q1:IM460>";

			AssertEquals("Still Green even if there is a newer IM460.", "GREEN", Wrapper.Routing);
		}

		public void TestJobNumber()
		{
			declaration.JE_DeclarationReference = "B001";
			AssertEquals("JobNumber", "B001", Wrapper.JobNumber);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LRN", "LRN001", Wrapper.LRN);
		}

		public void TestGoodsLocation()
		{
			var instruction = declaration.CustomsEntryInstructions.FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();

			var goodsLocation = instruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = "U";
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_AdditionalIdentifier = "AT000000";
			goodsLocation.CGL_CustomsOffice = "AT000000";
			entryHeader.CH_CEI_Instruction = instruction.PK;

			AssertEquals("GoodsLocation", "U A AT000000", Wrapper.GoodsLocation);
		}

		public void TestUCR()
		{
			declaration.JE_UCR = "DECUCR001";
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("UCR", "DECUCR001", Wrapper.UCR);

			declaration.JE_UCR = ZString.Empty;
			invoiceHeader.JZ_UCR = "UCR001";

			AssertEquals("UCR (fallback to JZ_UCR)", "UCR001", Wrapper.UCR);

			invoiceHeader.JZ_UCR = ZString.Empty;
			AssertEquals("UCR (both empty)", ZString.Empty, Wrapper.UCR);
		}

		public void TestImporterEORI()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPer";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMPREG002", Core.Constants.CountryCodes.Ireland);
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("ImporterEORI", "IEIMPREG002", Wrapper.ImporterEORI);
		}

		public void TestImporterAddressLines()
		{
			var importer = Factory.New<OrgHeader>();
			var mainAddress = importer.MainAddress;

			var importerAddress = "No. 1 Importer St.";
			mainAddress.Address1 = importerAddress;
			mainAddress.Postcode = "94043";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("ImporterAddressLines",
				$"{importerAddress}{System.Environment.NewLine}94043{System.Environment.NewLine}{Core.Constants.CountryCodes.Ireland}",
				Wrapper.ImporterAddressLines
			);
		}

		public void TestSupplierEORI()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "SUPlr";
			supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "SUPREG002", Core.Constants.CountryCodes.Ireland);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			AssertEquals("SupplierEORI", "IESUPREG002", Wrapper.SupplierEORI);
		}

		public void TestSupplierAddressLines()
		{
			var supplier = Factory.New<OrgHeader>();
			var mainAddress = supplier.MainAddress;

			var supplierAddress = "No. 1 Supplier St.";
			mainAddress.Address1 = supplierAddress;
			mainAddress.City = "NEW YORK";
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			AssertEquals("SupplierAddressLines",
				$"{supplierAddress}{System.Environment.NewLine}{mainAddress.City}{System.Environment.NewLine}{Core.Constants.CountryCodes.UnitedStates}",
				Wrapper.SupplierAddressLines
			);
		}

		public void TestPackages()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var pkgVG = declaration.Packages.AddNew();
			pkgVG.CW_PackType = "VG";
			pkgVG.CW_MarksAndNos = "VGMark";
			pkgVG.CW_PackQty = 4;
			var pkgNE = declaration.Packages.AddNew();
			pkgNE.CW_PackType = "NE";
			pkgNE.CW_MarksAndNos = "NEMark";
			pkgNE.CW_PackQty = 8;
			var pkg1A = declaration.Packages.AddNew();
			pkg1A.CW_PackType = "1A";
			pkg1A.CW_MarksAndNos = "1AMark";
			pkg1A.CW_PackQty = 16;

			var packPivots = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>();
			var pivotVG = packPivots.FirstOrDefault(v => v.Package == pkgVG);
			pivotVG.IsLinked = true;
			pivotVG.PackQty = 2;

			var pivotNE = packPivots.FirstOrDefault(v => v.Package == pkgNE);
			pivotNE.IsLinked = true;
			pivotNE.PackQty = 4;

			var pivot1A = packPivots.FirstOrDefault(v => v.Package == pkg1A);
			pivot1A.IsLinked = true;
			pivot1A.PackQty = 8;

			AssertEquals("Packages, should return TotalPackageNumber", 14, Wrapper.Packages);
		}

		public void TestNetWeight()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 123.456m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("NetWeight", "123.456", Wrapper.NetWeight);
		}

		public void TestGrossWeight()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 456.789m;

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("GrossWeight", "456.789", Wrapper.GrossWeight);
		}

		public void TestTransportTypeAndID()
		{
			declaration.JE_TransportMeans = "10";
			declaration.JE_TransportIDInland = "ABC123";
			AssertEquals("TransportTypeAndID", "10 ABC123", Wrapper.TransportTypeAndID);
		}

		public void TestContainersAndSeals()
		{
			var firstContainer = declaration.CusContainers.AddNew();
			firstContainer.CO_ContainerNumber = "OOCL3219032";
			firstContainer.CO_Seal = "SEAL01";

			var secondContainer = declaration.CusContainers.AddNew();
			secondContainer.CO_ContainerNumber = "OOCL3127895";
			secondContainer.CO_Seal = "SEAL02";

			CombineAssertions("Containers and Seals", () =>
			{
				AssertEquals("Containers", $"OOCL3127895{System.Environment.NewLine}OOCL3219032", Wrapper.Containers);
				AssertEquals("Seals", $"SEAL01{System.Environment.NewLine}SEAL02", Wrapper.Seals);
			});
		}

		public void TestTransportDocuments()
		{
			var invoiceHeader = declaration.Invoices.AddNew();

			var document1 = invoiceHeader.PreviousDocuments.AddNew();
			document1.CSI_Code = "701";
			document1.CSI_ReferenceNumber = "PRV701";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var document2 = invoiceLine.PreviousDocuments.AddNew();
			document2.CSI_Code = "702";
			document2.CSI_ReferenceNumber = "PRV702";
			var documentInvalid = invoiceLine.PreviousDocuments.AddNew();
			documentInvalid.CSI_Code = "802";
			documentInvalid.CSI_ReferenceNumber = "PRV801";

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("TransportDocuments", $"PRV701{System.Environment.NewLine}PRV702", Wrapper.TransportDocuments);
		}

		public void TestBillReferences()
		{
			var invoiceHeader = declaration.Invoices.AddNew();

			var document1 = invoiceHeader.SupportingDocuments.AddNew();
			document1.CSI_Code = "N703";
			document1.CSI_ReferenceNumber = "BILLN703";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "N705";
			document2.CSI_ReferenceNumber = "BILLN705";
			var documentInvalid = invoiceLine.SupportingDocuments.AddNew();
			documentInvalid.CSI_Code = "N235";
			documentInvalid.CSI_ReferenceNumber = "BILLN235";

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("BillReferences", $"BILLN703{System.Environment.NewLine}BILLN705", Wrapper.BillReferences);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Ireland);

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}

		CusEntryHeader entryHeader;
		JobDeclaration declaration;

		new IADClearanceSlip Wrapper => (IADClearanceSlip)base.Wrapper;
	}
}
