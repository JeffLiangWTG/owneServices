using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocDeclaration))]
	class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		public override void TestInvoiceLinesSortedByMergedNumericLineNoInternal()
		{
			var groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Inv1";

			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Description = "A";
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.CA_PageRelativeLineNumber = 1;

			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Description = "B";
			invoiceLine2.CA_PageNumber = 2;

			var entryLine3 = entry2.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 2;
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Description = "C";
			invoiceLine3.CA_PageNumber = 1;

			var entryLine4 = entry1.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 1;
			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Description = "D";
			invoiceLine4.CA_PageNumber = 1;
			invoiceLine4.CA_PageRelativeLineNumber = 2;

			var entryLine5 = entry1.MergedLines.AddNew();
			entryLine5.CL_LineNumber = 3;
			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LineNo = 5;
			invoiceLine5.JI_CL = entryLine5.PK;
			invoiceLine5.JI_Description = "E";

			var entryLine6 = entry1.MergedLines.AddNew();
			entryLine6.CL_LineNumber = 3;
			var invoiceLine6 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_LineNo = 6;
			invoiceLine6.JI_CL = entryLine5.PK;
			invoiceLine6.JI_Description = "E";
			var decTestClassWrapper = DocDeclaration.New(Declaration, Factory);
			var lines = decTestClassWrapper.InvoiceLinesSortedByMergedNumericLineNo;
			AssertEquals("InvoiceLinesSortedByMergedNumericLineNoInternalTestMethod.Count", 6, lines.Count);
			AssertLine(lines[0], 5, invoiceLine5.JI_Description);
			AssertLine(lines[1], 6, invoiceLine6.JI_Description);
			AssertLine(lines[2], 3, invoiceLine3.JI_Description);
			AssertLine(lines[3], 1, invoiceLine.JI_Description);
			AssertLine(lines[4], 4, invoiceLine4.JI_Description);
			AssertLine(lines[5], 2, invoiceLine2.JI_Description);
		}

		public void TestCommercialInvoiceOriginatorAddress()
		{
			var org = Factory.New<OrgHeader>();
			Declaration.CommercialInvoiceOriginator.OrganisationPK = org.PK;
			AssertEquals(new AddressFormatter(Factory, org, GlbCompany.CurrentCompany, false).PostalAddress(), DeclarationWrapper.CommercialInvoiceOriginatorAddress);
		}

		public void TestOveridesForB2()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Declaration.CA_OriginalTransactionNo = "12345987654321";
			Declaration.JE_RL_NKFinalDestination = "CATOR";
			Declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			Declaration.JE_OwnerRef = "OREF1";
			Declaration.JE_TotalWeight = 12m;
			Declaration.JE_TotalVolume = 34m;
			Declaration.JE_TotalNoOfPacks = 56;
			Declaration.JE_OH_Importer = CreateTestOrgHeader("IMXXX", "Importer Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.JE_OH_Supplier = CreateTestOrgHeader("SUXXX", "Supplier Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.TransactionNumber.AccountSecurityCode = "12345";
			Declaration.TransactionNumber.SequentialNumber = "00006790";
			var order = Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "456";
			Declaration.Invoices.AddNew().JZ_InvoiceNumber = "INVNO";

			AssertEquals("Description", "Blanket B2", DeclarationWrapper.GoodsDescription);
			Declaration.CA_B2Type = B2TypeList.Codes.Specific;
			AssertEquals("Description", "B2 for Original Transaction # 12345987654321", DeclarationWrapper.GoodsDescription);
			AssertNull("Destination", DeclarationWrapper.FinalDestination);
			AssertEquals("Owner ref", "B2 Transaction # 12345000067900", DeclarationWrapper.OwnerRef);
			AssertEquals("SupplierInvoiceNumbers", "", DeclarationWrapper.SupplierInvoiceNumbers);
			AssertNull("Supplier", DeclarationWrapper.Supplier);
			AssertEquals("Importer", "Importer Full Name", DeclarationWrapper.Importer.Name);
			AssertEquals("OrderRef", "", DeclarationWrapper.OrderRef);
			AssertEquals("Weight", "", DeclarationWrapper.Weight);
			AssertEquals("WeightUnit", "", DeclarationWrapper.WeightUnit);
			AssertEquals("Volume", "", DeclarationWrapper.Volume);
			AssertEquals("VolumeUnit", "", DeclarationWrapper.VolumeUnit);
			AssertEquals("Packages", "", DeclarationWrapper.Packages);
		}

		public void TestOverridesForLVS()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.CA_ServiceOption = ZString.Empty;
			Declaration.JE_OH_Importer = CreateTestOrgHeader("IMXXX", "Importer Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.JE_OH_Supplier = CreateTestOrgHeader("SUXXX", "Supplier Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.JE_GoodsDescription = "THINGOES";
			Declaration.JE_OwnerRef = "OREF1";
			var order = Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "456";
			Declaration.JE_TotalWeight = 12m;
			Declaration.JE_TotalVolume = 34m;
			Declaration.JE_TotalNoOfPacks = 56;
			var invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Inv1";
			AssertEquals("SupplierInvoiceNumbers", "INV1", DeclarationWrapper.SupplierInvoiceNumbers);
			AssertNotNull("Supplier", DeclarationWrapper.Supplier);
			AssertEquals("Importer", "Importer Full Name", DeclarationWrapper.Importer.Name);
			AssertEquals("GoodsDescription", "THINGOES", DeclarationWrapper.GoodsDescription);
			AssertEquals("OwnerRef", "OREF1", DeclarationWrapper.OwnerRef);
			AssertEquals("OrderRef", "456", DeclarationWrapper.OrderRef);
			AssertEquals("Weight", "12.00", DeclarationWrapper.Weight);
			AssertEquals("WeightUnit", "KG", DeclarationWrapper.WeightUnit);
			AssertEquals("Volume", "34.00", DeclarationWrapper.Volume);
			AssertEquals("VolumeUnit", "M3", DeclarationWrapper.VolumeUnit);
			AssertEquals("Packages", "56 PKG (OUTER)", DeclarationWrapper.Packages);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals("SupplierInvoiceNumbers", "INV1", DeclarationWrapper.SupplierInvoiceNumbers);
			AssertNull("Supplier", DeclarationWrapper.Supplier);
			AssertEquals("Importer", "Importer Full Name", DeclarationWrapper.Importer.Name);
			AssertEquals("GoodsDescription", "Various low value shipments", DeclarationWrapper.GoodsDescription);
			AssertEquals("OwnerRef", "", DeclarationWrapper.OwnerRef);
			AssertEquals("OrderRef", "", DeclarationWrapper.OrderRef);
			AssertEquals("Weight", "", DeclarationWrapper.Weight);
			AssertEquals("WeightUnit", "", DeclarationWrapper.WeightUnit);
			AssertEquals("Volume", "", DeclarationWrapper.Volume);
			AssertEquals("VolumeUnit", "", DeclarationWrapper.VolumeUnit);
			AssertEquals("Packages", "", DeclarationWrapper.Packages);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertEquals("SupplierInvoiceNumbers", "", DeclarationWrapper.SupplierInvoiceNumbers);
			AssertNull("Supplier", DeclarationWrapper.Supplier);
			AssertNull("Importer", DeclarationWrapper.Importer);
			AssertEquals("GoodsDescription", "Various low value shipments", DeclarationWrapper.GoodsDescription);
			AssertEquals("OwnerRef", "", DeclarationWrapper.OwnerRef);
			AssertEquals("OrderRef", "", DeclarationWrapper.OrderRef);
			AssertEquals("Weight", "", DeclarationWrapper.Weight);
			AssertEquals("WeightUnit", "", DeclarationWrapper.WeightUnit);
			AssertEquals("Volume", "", DeclarationWrapper.Volume);
			AssertEquals("VolumeUnit", "", DeclarationWrapper.VolumeUnit);
			AssertEquals("Packages", "", DeclarationWrapper.Packages);
		}

		public void TestOveridesForLVX()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Declaration.CA_OriginalTransactionNo = "12345987654321";
			Declaration.JE_RL_NKFinalDestination = "CATOR";
			Declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			Declaration.JE_OwnerRef = "OREF1";
			Declaration.JE_TotalWeight = 12m;
			Declaration.JE_TotalVolume = 34m;
			Declaration.JE_TotalNoOfPacks = 56;
			Declaration.JE_OH_Importer = CreateTestOrgHeader("IMXXX", "Importer Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.JE_OH_Supplier = CreateTestOrgHeader("SUXXX", "Supplier Full Name", "000 Street", "ZYZ Lane", "Sydney").PK;
			Declaration.TransactionNumber.AccountSecurityCode = "12345";
			Declaration.TransactionNumber.SequentialNumber = "00006790";
			Declaration.LVXInvoiceHeader.JZ_InvoiceNumber = "LVX00001";
			var order = Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "456";
			Declaration.Invoices.AddNew().JZ_InvoiceNumber = "INVNO";

			AssertEquals("Description", "Low Value Shipment", DeclarationWrapper.GoodsDescription);
			AssertNull("Destination", DeclarationWrapper.FinalDestination);
			AssertEquals("Owner ref", "LVS ID: LVX00001", DeclarationWrapper.OwnerRef);
			AssertEquals("SupplierInvoiceNumbers", "", DeclarationWrapper.SupplierInvoiceNumbers);
			AssertEquals("Supplier", "SUXXX", DeclarationWrapper.Supplier.Code);
			AssertEquals("Importer", "IMXXX", DeclarationWrapper.Importer.Code);
			AssertEquals("OrderRef", "", DeclarationWrapper.OrderRef);
			AssertEquals("Weight", "", DeclarationWrapper.Weight);
			AssertEquals("WeightUnit", "", DeclarationWrapper.WeightUnit);
			AssertEquals("Volume", "", DeclarationWrapper.Volume);
			AssertEquals("VolumeUnit", "", DeclarationWrapper.VolumeUnit);
			AssertEquals("Packages", "", DeclarationWrapper.Packages);
		}

		public void TestCarrierName()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			Declaration.JE_CarrierCode = "4646";
			AssertEquals("CarrierName", "4646 - Carrier Name", DeclarationWrapper.CarrierName);
		}

		public void TestPreviousCargoControlNumber()
		{
			var pccn = CusEntryNumber.New(Declaration, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Declaration.CountryCode);
			pccn.CE_EntryNum = "12345XX";
			AssertEquals("Previous CCN", "12345XX", DeclarationWrapper.PreviousCargoControlNumber);
		}

		public void TestB3MergedBy()
		{
			Declaration.CA_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			AssertEquals("B3MergedBy", OrgConstants.MergeInvoiceLines.Tariff, DeclarationWrapper.B3MergedBy);
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "Air Freight", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "Sea Freight", DeclarationWrapper.TransportModeDescription);
		}

		public void TestReleaseStatuses()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			Declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
			Declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN4";
			Declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN5";

			var entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", "1"));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", "2"));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", "3"));

			AssertEquals("ReleaseStatuses.Count", 1, DeclarationWrapper.ReleaseStatuses.Count);
		}

		public virtual void TestInvoiceLinesSortedByMergedLineNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 5, 5);

			var header1 = Declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "Inv1";

			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 2;

			AddLine(header1, entryLine1, 1, 1, 1);
			AddLine(header1, entryLine2, 2, 1, 3);
			AddLine(header1, entryLine1, 3, 1, 2);
			AddLine(header1, entryLine1, 4, 2, 2);
			AddLine(header1, entryLine1, 5, 2, 1);

			var lines = DeclarationWrapper.InvoiceLinesSortedByMergedLineNo;
			AssertEquals("InvoiceLinesSortedByLineNo.Count", 5, lines.Count);
			AssertLine(lines[0], 1, 1, 1);
			AssertLine(lines[1], 3, 1, 2);
			AssertLine(lines[2], 5, 2, 1);
			AssertLine(lines[3], 4, 2, 2);
			AssertLine(lines[4], 2, 1, 3);
		}

		public virtual void TestInvoiceLinesSortedByMergedNumericLineNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2021, 4, 15);

			var header1 = Declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "Inv1";
			var header2 = Declaration.Invoices.AddNew();
			header2.JZ_InvoiceNumber = "Inv2";

			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = Factory.New<CusEntryLine>();
			entryLine3.CL_LineNumber = 11;

			AddLine(header1, entryLine1, 1, 1, 1);
			AddLine(header2, entryLine2, 1, 2, 3);
			AddLine(header2, entryLine3, 2, 2, 2);
			AddLine(header1, entryLine1, 2, 1, 2);
			AddLine(header1, entryLine1, 3, 1, 3);

			var lines = DeclarationWrapper.InvoiceLinesSortedByMergedNumericLineNo;
			AssertEquals("InvoiceLinesSortedByMergedNumericLineNo.Count", 5, lines.Count);
			AssertLine(lines[0], 1, 1, 1);
			AssertLine(lines[1], 2, 1, 2);
			AssertLine(lines[2], 3, 1, 3);
			AssertLine(lines[3], 1, 2, 3);
			AssertLine(lines[4], 2, 2, 2);
		}

		static void AssertLine(DocJobComInvoiceLine line, ZShort lineNum, ZInt page, ZInt pageLineNum)
		{
			AssertEquals("LineNo", lineNum, line.LineNo);
			AssertEquals("InvoicePageNo", page, line.InvoicePageNo);
			AssertEquals("InvoicePageRelativeLineNumber", pageLineNum, line.InvoicePageRelativeLineNumber);
		}

		void AddLine(JobComInvoiceHeader invoice, CusEntryLine entryLine, short lineNum, int page, int pageLineNum)
		{
			var mock = Factory.NewMoq<JobComInvoiceLine>();
			mock.Setup(m => m.B3EntryLine).Returns(entryLine);
			var line = mock.Object;
			line.JI_JZ = invoice.PK;
			line.JI_LineNo = lineNum;
			line.CA_PageNumber = page;
			line.CA_PageRelativeLineNumber = pageLineNum;
			Declaration.InvoiceLines.Add(line);
		}

		public void TestUSPortOfExit()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Code", "Description", startDate, endDate);
			newFactory.Save();

			AssertEquals("USPortOfExit", ZString.Empty, DeclarationWrapper.USPortOfExit);
			Declaration.Invoices.AddNew().CA_USPortOfExit = "Code";
			AssertEquals("USPortOfExit", "Code - Description", DeclarationWrapper.USPortOfExit);
		}

		public void TestCargoControlNumber()
		{
			AssertEquals("CargoControlNumber", ZString.Empty, DeclarationWrapper.CargoControlNumber);
			Declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "1111";
			AssertEquals("CargoControlNumber", "1111", DeclarationWrapper.CargoControlNumber);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}

		#endregion
	}
}
