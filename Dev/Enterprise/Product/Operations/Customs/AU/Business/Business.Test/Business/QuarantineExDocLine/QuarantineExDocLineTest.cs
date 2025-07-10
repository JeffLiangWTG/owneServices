using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocLine))]
	sealed class QuarantineExDocLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestQL_FarmCode()
		{
			AssertEquals("Caption", "Farm Code", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocLine), nameof(QuarantineExDocLine.QL_FarmCode)).Caption);
		}

		public void TestQL_FarmType()
		{
			AssertEquals("Caption", "Farm Type", DataBoundResourceStrings.GetDataForProperty(typeof(QuarantineExDocLine), nameof(QuarantineExDocLine.QL_FarmType)).Caption);
		}

		public void TestAmendPermissionMatrix()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_CutCodeInfo);
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_GrowerNumberInfo);
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_HalalProductIndicatorInfo);
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_NatureOfCommodityInfo);
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_TreatmentTypeInfo);
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineLine.QL_UngradedProductIndicatorInfo);
		}

		public void TestQL_FormattedCombinedNomenclature()
		{
			quarantineLine.QL_FormattedCombinedNomenclature = "1234567890";
			AssertEquals("QL_CombinedNomenclature", "12345678", quarantineLine.QL_CombinedNomenclature);
			AssertEquals("QL_FormattedCombinedNomenclature", "1234.56.78", quarantineLine.QL_FormattedCombinedNomenclature);
			quarantineLine.QL_FormattedCombinedNomenclature = "9876.54.32 10";
			AssertEquals("QL_CombinedNomenclature", "98765432", quarantineLine.QL_CombinedNomenclature);
			AssertEquals("QL_FormattedCombinedNomenclature", "9876.54.32", quarantineLine.QL_FormattedCombinedNomenclature);
			quarantineLine.QL_FormattedCombinedNomenclature = "9876.54";
			AssertEquals("QL_CombinedNomenclature", "987654", quarantineLine.QL_CombinedNomenclature);
			AssertEquals("QL_FormattedCombinedNomenclature", "9876.54", quarantineLine.QL_FormattedCombinedNomenclature);
		}

		public void TestEUTariffFormatter()
		{
			var tariffFormatter = quarantineLine.EUTariffFormatter;
			AssertSame("EUTariffFormatter is cached", tariffFormatter, quarantineLine.EUTariffFormatter);
			AssertEquals("tariffFormatter.Format", "12345678", tariffFormatter.Format("1234.56.78"));
			AssertEquals("tariffFormatter.DisplayFormat", "1234.56.78", tariffFormatter.DisplayFormat("12345678"));
		}

		public void TestQL_TreatmentType()
		{
			AssertEquals("QL_TreatmentType max length", 4, quarantineLine.QL_TreatmentTypeInfo.MaxLength);
		}

		public void TestQL_ProduceType()
		{
			var exdocLine = Factory.New<QuarantineExDocLine>();
			Assert("Should return empty as no header", exdocLine.QL_ProduceType.IsEmpty);

			quarantineHeader.QH_ProduceType = "ASD";
			AssertEquals("Proxy the Produce type through", "ASD", quarantineLine.QL_ProduceType);
		}

		[ExpectNoExceptions]
		public void TestUpdatingQuarantineWeightDoesNotLoop()
		{
			quarantineLine.QL_GrossMetricWeight = 8.2753;
			AssertEquals("Invoice Line is updated with rounded weight", 8.275m, invoiceLine.JI_Weight);
		}

		public void TestQL_EffectiveShippingMarks()
		{
			var exdocLine = Factory.New<QuarantineExDocLine>();
			Assert("Should return empty as no header", exdocLine.QL_EffectiveShippingMarks.IsEmpty);

			declaration.JE_MarksAndNumbers = "JUST SOME MARKS AND NUMBERS";
			AssertEquals("Proxy Declaration Marks and Numbers through", "JUST SOME MARKS AND NUMBERS", quarantineLine.QL_EffectiveShippingMarks);
		}

		public void TestQL_EffectiveHealthCertificateDescription()
		{
			quarantineLine.QL_HealthCertificateDescription = "Test HC Description";
			AssertEquals("Effective HC Description is from QL_HealthCertificateDescription", "Test HC Description", quarantineLine.QL_EffectiveHealthCertificateDescription);
			quarantineLine.QL_HealthCertificateDescription = ZString.Empty;
			invoiceLine.JI_Description = "Exporter Defined Description";
			AssertEquals("Effective HC Description is from blank", ZString.Empty, quarantineLine.QL_EffectiveHealthCertificateDescription);
			invoiceLine.QuarantineExDocLine.QL_SendHCDesc = true;
			AssertEquals("Effective HC Description is from ExporterDefinedProductDescription", "Exporter Defined Description", quarantineLine.QL_EffectiveHealthCertificateDescription);
			invoiceLine.QuarantineExDocLine.QL_HealthCertificateDescription = "Both set this wins";
			AssertEquals("Effective HC Description is from QL_HealthCertificateDescription as it override exporter defined", "Both set this wins", quarantineLine.QL_EffectiveHealthCertificateDescription);
		}

		public void TestQL_ImportAuthorityCode()
		{
			quarantineLine.QL_ImportAuthorityCode = "Test Importer Authority Code";
			AssertEquals("Importer authority code is from QL_ImportAuthorityCode", "Test Importer Authority Code", quarantineLine.QL_ImportAuthorityCode);
			quarantineLine.QL_ImportAuthorityCode = ZString.Empty;
			AssertEquals("Importer authority code is empty", ZString.Empty, quarantineLine.QL_ImportAuthorityCode);
		}

		public void TestQL_SendHCDesc()
		{
			quarantineLine.QL_SendHCDesc = true;
			Assert(quarantineLine.QL_SendHCDesc);
			quarantineLine.QL_SendHCDesc = false;
			Assert(!quarantineLine.QL_SendHCDesc);
		}

		public void TestProductCode()
		{
			quarantineLine.QL_ProductType = "AA";
			quarantineLine.QL_PackType = "CT";
			quarantineLine.QL_SupplimentaryCode = "BB";
			AssertEquals("Product code is the correct format", "XAA CTBB", quarantineLine.ProductCode);
			quarantineLine.QL_ProductType = "ZXC";
			quarantineLine.QL_PreservationType = "C";
			quarantineLine.QL_PackType = "BP";
			quarantineLine.QL_SupplimentaryCode = ZString.Empty;
			AssertEquals("Product code is the correct format", "CZXCBP  ", quarantineLine.ProductCode);
			quarantineLine.QL_ProductType = "F";
			quarantineLine.QL_PreservationType = "F";
			quarantineLine.QL_PackType = ZString.Empty;
			AssertEquals("Product code is the correct format", "FF      ", quarantineLine.ProductCode);
		}

		public void TestQL_DominantProduct()
		{
			quarantineLine.QL_DominantProduct = "DPCODE";
			AssertEquals("Dominant Product is from QL_DominantProduct", "DPCODE", quarantineLine.QL_DominantProduct);
			quarantineLine.QL_DominantProduct = ZString.Empty;
			AssertEquals("Dominant Product is empty", ZString.Empty, quarantineLine.QL_DominantProduct);
		}

		public void TestQL_AdditionalProducts()
		{
			quarantineLine.QL_AdditionalProducts = "APCODES";
			AssertEquals("Additional Products is from QL_AdditionalProducts", "APCODES", quarantineLine.QL_AdditionalProducts);
			quarantineLine.QL_AdditionalProducts = ZString.Empty;
			AssertEquals("Additional Products is empty", ZString.Empty, quarantineLine.QL_AdditionalProducts);
		}

		public void TestHasQuarantineLineBeenAccepted()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;

			var sender = declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var rfpmmm = new RFPMultiMessageManager(declaration, EXDOCMessageTypeCodes.Codes.ORD);
			rfpmmm.SendMessages(sender);

			quarantineHeader.QH_RequestForPermitNumber = "5";
			var msg = quarantineHeader.Messages.AddNew();
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg.EM_Status = EDIMessage.Status.Received;
			msg.EM_MessageType = EDIMessage.Status.Acknowledged;

			declaration.JE_MessageStatus = RFPMessage.Status.Received;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted()); //Requisite

			var invLine1 = invoiceLine;
			var invLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var invLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			using (invoiceHeader.GetLineNumberRenumberingSuspender())
			{
				invLine1.JI_LineNo = 2;
				invLine2.JI_LineNo = 3;
				invLine3.JI_LineNo = 4;
			}

			quarantineHeader.QH_QuarantineMessageMaxLine = 3;

			Assert(invLine1.QuarantineExDocLine.HasQuarantineLineBeenAccepted());
			Assert(invLine2.QuarantineExDocLine.HasQuarantineLineBeenAccepted());
			Assert(!invLine3.QuarantineExDocLine.HasQuarantineLineBeenAccepted());
		}

		public void TestClone()
		{
			var exdocProcess = quarantineLine.Processes.AddNew();
			var address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_CompanyName = "Company Name";
			exdocProcess.EE_E2_Address = address.PK;
			var clonedAddress = address.Clone();
			var jobDocAddressPKPairs = new Dictionary<ZGuid, ZGuid>() { { address.PK, clonedAddress.PK } };
			quarantineLine.QL_UseByStart = new ZDateTime(2006, 12, 12);
			quarantineLine.QL_UseByEnd = new ZDateTime(2006, 12, 13);
			quarantineLine.QL_ShippingMarks = "Marks and Shipping";
			quarantineLine.QL_HCFormatAllocated = "ABC/12";
			quarantineLine.QL_HCNumber = "925783";
			exdocProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			exdocProcess.EE_LeaseNumber = "1243345";
			exdocProcess.EE_StartDate = new ZDateTime(2050, 11, 23);
			exdocProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;

			var clonedInvoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			quarantineLine.Clone(clonedInvoiceLine, jobDocAddressPKPairs);
			var clonedExDocLine = clonedInvoiceLine.QuarantineExDocLine;
			var clonedExdocProcess = clonedExDocLine.Processes[0];

			AssertEquals("Parent ID set", clonedInvoiceLine.PK, clonedExDocLine.QL_JI);
			AssertEquals("Shipping Marks", "Marks and Shipping", clonedExDocLine.QL_ShippingMarks);
			AssertEquals("Use by Start", new ZDateTime(2006, 12, 12), clonedExDocLine.QL_UseByStart);
			AssertEquals("Use by End", new ZDateTime(2006, 12, 13), clonedExDocLine.QL_UseByEnd);
			AssertEquals("Processing Type", EXDOCProcessTypeCodes.Codes.CatcherVessel, clonedExdocProcess.EE_ProcessingType);
			AssertEquals("Lease Number", "1243345", clonedExdocProcess.EE_LeaseNumber);
			AssertEquals("Address", clonedAddress.PK, clonedExdocProcess.EE_E2_Address);
			AssertEquals("EE_EstablishmentPostedStatus", ZString.Empty, clonedExdocProcess.EE_EstablishmentPostedStatus);
			Assert("Health Certificate Format Allocated", clonedExDocLine.QL_HCFormatAllocated.IsEmpty);
			Assert("Health Certificate Number", clonedExDocLine.QL_HCNumber.IsEmpty);
		}

		public void TestOuterPackAccuracyDescription()
		{
			quarantineLine.QL_OuterPackAccuracy = ZString.Empty;
			Assert("Description is empty", quarantineLine.OuterPackAccuracyDescription.IsEmpty);
			quarantineLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			AssertEquals("Description is Approximate", EXDOCPackAccuracyCodes.Descriptions.Approximate, quarantineLine.OuterPackAccuracyDescription);
			quarantineLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			AssertEquals("Description is Equal", EXDOCPackAccuracyCodes.Descriptions.EqualTo, quarantineLine.OuterPackAccuracyDescription);
		}

		public void TestIntermediatePackAccuracyDescription()
		{
			quarantineLine.QL_IntermediatePackAccuracy = ZString.Empty;
			Assert("Description is empty", quarantineLine.IntermediatePackAccuracyDescription.IsEmpty);
			quarantineLine.QL_IntermediatePackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			AssertEquals("Description is Approximate", EXDOCPackAccuracyCodes.Descriptions.Approximate, quarantineLine.IntermediatePackAccuracyDescription);
			quarantineLine.QL_IntermediatePackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			AssertEquals("Description is Equal", EXDOCPackAccuracyCodes.Descriptions.EqualTo, quarantineLine.IntermediatePackAccuracyDescription);
		}

		public void TestInnerPackAccuracyDescription()
		{
			quarantineLine.QL_InnerPackAccuracy = ZString.Empty;
			Assert("Description is empty", quarantineLine.InnerPackAccuracyDescription.IsEmpty);
			quarantineLine.QL_InnerPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			AssertEquals("Description is Approximate", EXDOCPackAccuracyCodes.Descriptions.Approximate, quarantineLine.InnerPackAccuracyDescription);
			quarantineLine.QL_InnerPackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			AssertEquals("Description is Equal", EXDOCPackAccuracyCodes.Descriptions.EqualTo, quarantineLine.InnerPackAccuracyDescription);
		}

		public void TestQL_HCFormatAllocatedReadonly()
		{
			Assert("QL_HCFormatAllocated is ReadOnly", quarantineLine.QL_HCFormatAllocatedInfo.ReadOnly);
		}

		public void TestQL_HCNumberReadonly()
		{
			Assert("QL_HCNumber is ReadOnly", quarantineLine.QL_HCNumberInfo.ReadOnly);
		}

		public void TestFormattedProcesses()
		{
			var process = quarantineLine.Processes.AddNew();
			process.EE_AuthorisationEstablishmentID = "55";
			process.EE_EstablishmentIndicator = "1";
			process.EE_Depuration = new ZDateTime(2005, 12, 12);
			process.EE_EndDate = new ZDateTime(2005, 12, 23);
			process.EE_HarvestArea = "ABCSDG";
			process.EE_InspectionRequestedDate = new ZDateTime(2006, 11, 23);
			process.EE_LeaseNumber = "12345";
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process.EE_StartDate = new ZDateTime(2007, 10, 13);
			process.EE_TreatmentCode = "TRCODE";
			process.EE_TreatmentInfo = "NOT MUCH TO SAY";
			AssertMultilineASCIIEquals("Formatted process looks correct", FormattedProcessActual, quarantineLine.FormattedProcesses);
		}

		public void TestFormattedContainers()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABOC2321312";
			container1.CO_Seal = "23423";
			declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABO89745458";
			container1.CO_Seal = "98753";
			var containerPivot = invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("ABO89745458");
			containerPivot.IsForInvoiceLine = true;
			containerPivot.GrossWeightInKG = 123.34;
			containerPivot.NetWeightInKG = 8734.34;
			AssertMultilineASCIIEquals("Formatted Containers looks correct and only picks up one's for invoice lines", FormattedContainersActual, quarantineLine.FormattedContainers);
		}

		public void TestCustomsWeightUQDefaulting()
		{
			quarantineLine.QL_AqisCustomsWeight = 69m;
			AssertEquals("KGM", quarantineLine.QL_AqisCustomsWeightUQ);
		}

		public void TestLookups()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var lookups = quarantineLine.Lookups;
			AssertType(typeof(EXDOCSQuarantineExDocLineLookups), lookups);
			AssertSame("Should be cached.", quarantineLine.Lookups, quarantineLine.Lookups);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			lookups = quarantineLine.Lookups;
			AssertType(typeof(NEXDOCSQuarantineExDocLineLookups), lookups);
			AssertSame("Should be cached.", lookups, quarantineLine.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => quarantineLine;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			invoiceHeader = helper.Header1;
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			invoiceLine = helper.Line1;
			quarantineLine = invoiceLine.QuarantineExDocLine;
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		QuarantineExDocHeader quarantineHeader;
		QuarantineExDocLine quarantineLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			quarantineLine.Processes.AddNew();
			return quarantineLine;
		}

		const string FormattedProcessActual = @"Type: [Catcher Vessel]      Establishment: [55]      Establishment Indicator: [1]      Depuration Date: [12-Dec-05]      Harvest Area: [ABCSDG]      Start Date: [13-Oct-07]      End Date: [23-Dec-05]      Inspection Requested: [23-Nov-06]      Lease Number: [12345]      Treatment Code: [TRCODE]
Treatment Information:- NOT MUCH TO SAY";

		const string FormattedContainersActual = @"Container Number: [ABO89745458]      Seal: [98753]      IMA1 Net Weight: [8734.34]      IMA1 Gross Weight: [123.34]";
	}
}
