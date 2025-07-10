using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportAmendmentHeaderTest : TestCaseWithFactory
	{
		public void Test5DSHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			entry.CH_BGMReference = "12345678901234";

			var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			CreateSnapShot(entry, originalHeader, ElectronicDocumentTypeList.Codes._5DQ);

			var current5DSHeader = new LocalExportAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>());
			AssertEquals(EDIMessage.EntryNumberPlaceHolder, current5DSHeader.DeclarationNumber);
			AssertEquals("12345678901234", current5DSHeader.CustomsReceiptNumber);
			AssertEquals("01010", current5DSHeader.DeclarationCustomsOfficeAndDivision);

			AssertEquals("0000000000", current5DSHeader.Supplier.BusinessRegNo);
			AssertEquals("111111111111111", current5DSHeader.Supplier.UnipassIDForOrganization);

			var supplier = entry.Declaration.Supplier;
			supplier.CustomsCodes.RemoveAndDeleteAll();

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			current5DSHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertNullOrEmpty(current5DSHeader.Supplier.BusinessRegNo);
			AssertNullOrEmpty(current5DSHeader.Supplier.UnipassIDForOrganization);

			AssertNull("Registration numbers are not amendable", current5DSHeader.AmendedItems);
		}

		public void Test5DRHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			entry.CH_BGMReference = "12345678901234";

			var originalHeader = new LocalExport5DPEntryHeaderCreator().Create(entry);
			CreateSnapShot(entry, originalHeader, ElectronicDocumentTypeList.Codes._5DP);

			var current5DRHeader = new LocalExportAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>());
			AssertEquals(EDIMessage.EntryNumberPlaceHolder, current5DRHeader.DeclarationNumber);
			AssertEquals("12345678901234", current5DRHeader.CustomsReceiptNumber);
			AssertEquals("01010", current5DRHeader.DeclarationCustomsOfficeAndDivision);

			AssertEquals("사업자등록번호", current5DRHeader.Supplier.BusinessRegNo);
			AssertEquals("통관고유부호", current5DRHeader.Supplier.UnipassIDForOrganization);

			var supplier = entry.Declaration.Supplier;
			supplier.CustomsCodes.RemoveAndDeleteAll();

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DR).AmendedItems;
			current5DRHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertNullOrEmpty(current5DRHeader.Supplier.BusinessRegNo);
			AssertNullOrEmpty(current5DRHeader.Supplier.UnipassIDForOrganization);

			AssertNull("No registration numbers are amendable", current5DRHeader.AmendedItems);
		}

		public void Test5DSAmendmentItems()
		{
			var vessel = CreateRefVessel("M/V MARIA", "1234567", "M/V MARIA");
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			CreateSnapShot(entry, originalHeader, ElectronicDocumentTypeList.Codes._5DQ);

			AssertEquals("경남창고", originalHeader.BondedAreaCode);
			AssertEquals("M/V MARIA", originalHeader.VesselRadioCallSign);
			AssertEquals("20GLKO0080I", originalHeader.MRNNo);
			AssertEquals("1", originalHeader.GoodsType);
			AssertEquals("1", originalHeader.DrawbackApplicantType);
			AssertEquals(900m, originalHeader.TotalGrossWeight);
			AssertEquals(100, originalHeader.TotalPackages);
			AssertEquals(15, originalHeader.CrewCount);
			AssertEquals(24, originalHeader.ScheduledSailingDays);

			AssertEquals("STAINLESS STEEL", originalHeader.EntryLines[0].InvoiceDescription);
			AssertEquals("물품식별번호", originalHeader.EntryLines[0].GoodsNo);
			AssertEquals("1234567890", originalHeader.EntryLines[0].HSCode);
			AssertEquals(9999m, originalHeader.EntryLines[0].Quantity);
			AssertEquals("KG", originalHeader.EntryLines[0].QuantityUnit);
			AssertEquals(9999m, originalHeader.EntryLines[0].NetWeight);
			AssertEquals(1000m, originalHeader.EntryLines[0].FOBAmount);
			AssertEquals(99, originalHeader.EntryLines[0].Packages);
			AssertEquals("VL", originalHeader.EntryLines[0].PackagesType);
			AssertEquals("1", originalHeader.EntryLines[0].DocumentType);
			AssertEquals("L172770912345", originalHeader.EntryLines[0].DocumentNo);
			AssertEquals(new ZDateTime("2013-01-01"), originalHeader.EntryLines[0].InboundDate);
			AssertEquals("010151234567001999", originalHeader.EntryLines[0].PreviousTransactionReferenceNo);
			AssertEquals("01", originalHeader.EntryLines[0].PreviousTransactionReferenceNoType);

			AssertEquals("STAINLESS STEEL2", originalHeader.EntryLines[1].InvoiceDescription);
			AssertEquals("물품식별번호2", originalHeader.EntryLines[1].GoodsNo);
			AssertEquals("1234567891", originalHeader.EntryLines[1].HSCode);
			AssertEquals(999m, originalHeader.EntryLines[1].Quantity);
			AssertEquals("KG", originalHeader.EntryLines[1].QuantityUnit);
			AssertEquals(999m, originalHeader.EntryLines[1].NetWeight);
			AssertEquals(999m, originalHeader.EntryLines[1].FOBAmount);
			AssertEquals(9, originalHeader.EntryLines[1].Packages);
			AssertEquals("VL", originalHeader.EntryLines[1].PackagesType);
			AssertEquals("2", originalHeader.EntryLines[1].DocumentType);
			AssertEquals("L172770925458", originalHeader.EntryLines[1].DocumentNo);
			AssertEquals(new ZDateTime("2013-01-02"), originalHeader.EntryLines[1].InboundDate);
			AssertEquals("010151234567001990", originalHeader.EntryLines[1].PreviousTransactionReferenceNo);
			AssertEquals("02", originalHeader.EntryLines[1].PreviousTransactionReferenceNoType);

			Update5DSHeaderAmendmentItems(entry, vessel);
			Update5DSLineAmendmentItems(entry);
		}
		void Update5DSHeaderAmendmentItems(CusEntryHeader entry, RefVessel vessel)
		{
			var declaration = entry.Declaration;
			declaration.JE_LocationOfGoods = "";
			declaration.JE_SubLocationOfGoods = "새 반입장소";
			vessel.RV_RadioCallSign = "M/V MARIA2";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;
			declaration.Invoices[0].JZ_DRWApplicantType = "2";
			declaration.DeclarationRefs[0].J3_ReferenceNumber = "NEW 20GLKO0080I";
			declaration.JE_NoOfCrew = 30;
			declaration.JE_VoyageDuration = 17;

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertHeader(amendmentHeader, "11A", "경남창고", "새 반입장소");
			AssertHeader(amendmentHeader, "12", "M/V MARIA", "M/V MARIA2");
			AssertHeader(amendmentHeader, "13", "20GLKO0080I", "NEW 20GLKO0080I");
			AssertHeader(amendmentHeader, "14", "1", "2");
			AssertHeader(amendmentHeader, "15", "1", "2");
			AssertHeader(amendmentHeader, "16", "100", "10");
			AssertHeader(amendmentHeader, "18", "900", "90");
			AssertHeader(amendmentHeader, "38", "15", "30");
			AssertHeader(amendmentHeader, "39", "24", "17");
		}
		void Update5DSLineAmendmentItems(CusEntryHeader entry)
		{
			var entryLine = entry.MergedLines[1];
			entry.MergedLines[0].InvoiceLines[0].Delete();
			entry.MergedLines[0].Delete();

			entryLine.CL_AdValoremTariff = "1234567900";
			entryLine.CL_Description = "Change STAINLESS STEEL";
			entryLine.CL_CustomsValue = 500m;

			entryLine.InvoiceLines[0].Delete();
			var invoiceline = (JobComInvoiceLine)entryLine.InvoiceLines[0];
			invoiceline.SupportingDocumentReferenceNumber = "L172770912350";
			invoiceline.SupportingDocumentCode = "2";
			invoiceline.JI_InboundDate = new ZDateTime(2013, 12, 31);
			invoiceline.JI_PreviousEntryNumber = "010151234567002000";
			invoiceline.JI_OriginalStateDocType = "02";
			invoiceline.JI_SerialNumber = "정정물품식별번호";
			invoiceline.JI_InvoiceUQ = Core.Constants.Weight.Grams;
			invoiceline.JI_PackType = "PL";

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertLine(amendmentHeader, "21", "STAINLESS STEEL", "Change STAINLESS STEEL", 1, "3");
			AssertLine(amendmentHeader, "22", "물품식별번호", "정정물품식별번호", 1, "3");
			AssertLine(amendmentHeader, "23", "1234567890", "1234567900", 1, "3");
			AssertLine(amendmentHeader, "24A", "9999", "4444", 1, "3");
			AssertLine(amendmentHeader, "24B", "KG", "G", 1, "3");
			AssertLine(amendmentHeader, "25", "9999", "4444", 1, "3");
			AssertLine(amendmentHeader, "26", "1000", "500", 1, "3");
			AssertLine(amendmentHeader, "27", "99", "44", 1, "3");
			AssertLine(amendmentHeader, "28", "VL", "PL", 1, "3");
			AssertLine(amendmentHeader, "29", "1", "2", 1, "3");
			AssertLine(amendmentHeader, "30", "L172770912345", "L172770912350", 1, "3");
			AssertLine(amendmentHeader, "31", "20130101", "20131231", 1, "3");
			AssertLine(amendmentHeader, "32A", "010151234567001999", "010151234567002000", 1, "3");
			AssertLine(amendmentHeader, "32B", "01", "02", 1, "3");

			AssertLine(amendmentHeader, "", "", "", 2, "2");
		}
		public void Test5DSOtherTransportMeansAmendmentItems()
		{
			CreateRefVessel("SAMARIA TEST", "9182643", "");
			CreateRefVessel("Oerssleff TEST", "ZDNI9", "");
			CreateRefVessel("Change SAMARIA TEST", "9182", "");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			declaration.JE_TotalWeight = 900000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;
			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var transportMean1 = declaration.TransportMeans.AddNew();
			transportMean1.CY_Order = 1;
			transportMean1.CY_Code = "SAMARIA TEST";
			transportMean1.CY_Data = "서울 허12 3456";

			var transportMean2 = declaration.TransportMeans.AddNew();
			transportMean2.CY_Order = 2;
			transportMean2.CY_Code = "Oerssleff TEST";
			transportMean2.CY_Data = "서울 라24 7890";

			var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			CreateSnapShot(entry, originalHeader, ElectronicDocumentTypeList.Codes._5DQ);

			AssertEquals("SequenceNo", 1, originalHeader.OtherTransportMeans[0].SequenceNo);
			AssertEquals("WorkingVesselName", "SAMARIA TEST", originalHeader.OtherTransportMeans[0].WorkingVesselName);
			AssertEquals("WorkingVesselLloydsNumber", "9182643", originalHeader.OtherTransportMeans[0].WorkingVesselLloydsNumber);
			AssertEquals("TransportVehicleRegNo", "서울 허12 3456", originalHeader.OtherTransportMeans[0].TransportVehicleRegNo);
			AssertEquals("SequenceNo", 2, originalHeader.OtherTransportMeans[1].SequenceNo);
			AssertEquals("WorkingVesselName", "Oerssleff TEST", originalHeader.OtherTransportMeans[1].WorkingVesselName);
			AssertEquals("WorkingVesselLloydsNumber", "ZDNI9", originalHeader.OtherTransportMeans[1].WorkingVesselLloydsNumber);
			AssertEquals("TransportVehicleRegNo", "서울 라24 7890", originalHeader.OtherTransportMeans[1].TransportVehicleRegNo);

			transportMean1 = declaration.TransportMeans[0];
			transportMean1.CY_Order = 1;
			transportMean1.CY_Code = "Change SAMARIA TEST";
			transportMean1.CY_Data = "제주 허12 3456";
			declaration.TransportMeans[1].Delete();

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertLine(amendmentHeader, "11B", "SAMARIA TEST", "Change SAMARIA TEST", 1, "3");
			AssertLine(amendmentHeader, "11C", "9182643", "9182", 1, "3");
			AssertLine(amendmentHeader, "11D", "서울 허12 3456", "제주 허12 3456", 1, "3");

			AssertLine(amendmentHeader, "11B", "", "", 2, "2");
		}

		public void Test5DRAmendmentItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			var originalHeader = new LocalExport5DPEntryHeaderCreator().Create(entry);
			CreateSnapShot(entry, originalHeader, ElectronicDocumentTypeList.Codes._5DP);
			AssertEquals("01023010-보세구역이름", originalHeader.BondedAreaCode);
			AssertEquals("5DP is not used this column.", null, originalHeader.VesselRadioCallSign);
			AssertEquals("5DP is not used this column.", null, originalHeader.MRNNo);
			AssertEquals("1", originalHeader.GoodsType);
			AssertEquals("1", originalHeader.DrawbackApplicantType);
			AssertEquals(900m, originalHeader.TotalGrossWeight);
			AssertEquals(100, originalHeader.TotalPackages);
			AssertEquals("5DP is not used this column.", 0, originalHeader.CrewCount);
			AssertEquals("5DP is not used this column.", 0, originalHeader.ScheduledSailingDays);

			AssertEquals("STAINLESS STEEL", originalHeader.EntryLines[0].InvoiceDescription);
			AssertEquals("000000000", originalHeader.EntryLines[0].GoodsNo);
			AssertEquals("1234567890", originalHeader.EntryLines[0].HSCode);
			AssertEquals(9999m, originalHeader.EntryLines[0].Quantity);
			AssertEquals("KG", originalHeader.EntryLines[0].QuantityUnit);
			AssertEquals(10000m, originalHeader.EntryLines[0].NetWeight);
			AssertEquals(1000m, originalHeader.EntryLines[0].FOBAmount);
			AssertEquals(99, originalHeader.EntryLines[0].Packages);
			AssertEquals("VL", originalHeader.EntryLines[0].PackagesType);
			AssertEquals("1", originalHeader.EntryLines[0].DocumentType);
			AssertEquals("L172770912345", originalHeader.EntryLines[0].DocumentNo);
			AssertEquals(new ZDateTime(2013, 01, 01), originalHeader.EntryLines[0].InboundDate);
			AssertEquals("010151234567001999", originalHeader.EntryLines[0].PreviousTransactionReferenceNo);
			AssertEquals("01", originalHeader.EntryLines[0].PreviousTransactionReferenceNoType);
			AssertEquals("ABCD9589375", originalHeader.EntryLines[0].MaterialCode);

			AssertEquals("STAINLESS STEEL2", originalHeader.EntryLines[1].InvoiceDescription);
			AssertEquals("1111111111", originalHeader.EntryLines[1].GoodsNo);
			AssertEquals("0987654321", originalHeader.EntryLines[1].HSCode);
			AssertEquals(1111m, originalHeader.EntryLines[1].Quantity);
			AssertEquals("KG", originalHeader.EntryLines[1].QuantityUnit);
			AssertEquals(2222m, originalHeader.EntryLines[1].NetWeight);
			AssertEquals(3333m, originalHeader.EntryLines[1].FOBAmount);
			AssertEquals(11, originalHeader.EntryLines[1].Packages);
			AssertEquals("VL", originalHeader.EntryLines[1].PackagesType);
			AssertEquals("2", originalHeader.EntryLines[1].DocumentType);
			AssertEquals("L172770925459", originalHeader.EntryLines[1].DocumentNo);
			AssertEquals(new ZDateTime(2020, 02, 02), originalHeader.EntryLines[1].InboundDate);
			AssertEquals("999100765432151010", originalHeader.EntryLines[1].PreviousTransactionReferenceNo);
			AssertEquals("02", originalHeader.EntryLines[1].PreviousTransactionReferenceNoType);
			AssertEquals("5739859DCBA", originalHeader.EntryLines[1].MaterialCode);

			Update5DRHeaderAmendmentItems(entry);
			Update5DRLineAmendmentItems(entry);
		}
		void Update5DRHeaderAmendmentItems(CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			declaration.JE_LocationOfGoods = "";
			declaration.JE_SubLocationOfGoods = "새 경남창고";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_DRWApplicantType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DR).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertHeader(amendmentHeader, "11A", "01023010-보세구역이름", "01023010-새 경남창고");
			AssertHeader(amendmentHeader, "14", "1", "2");
			AssertHeader(amendmentHeader, "15", "1", "2");
			AssertHeader(amendmentHeader, "16", "100", "10");
			AssertHeader(amendmentHeader, "18", "900", "90");
		}
		void Update5DRLineAmendmentItems(CusEntryHeader entry)
		{
			var entryline = entry.MergedLines[0];
			entryline.CL_AdValoremTariff = "0987654321";
			entryline.CL_Description = "Change STAINLESS STEEL";
			entryline.CL_CustomsValue = 500m;

			entryline.InvoiceLines[1].Delete();
			var invoiceline = (JobComInvoiceLine)entryline.InvoiceLines[0];
			invoiceline.JI_InvoiceQuantity = 8888m;
			invoiceline.JI_InvoiceUQ = "G";
			invoiceline.JI_NoOfPacks = 88;
			invoiceline.JI_PackType = "BK";
			invoiceline.JI_NetWeight = 9999m;
			invoiceline.JI_NetWeightUQ = "G";
			invoiceline.JI_LinePrice = 9999m;
			invoiceline.JI_InboundDate = new ZDateTime(2013, 01, 02);
			invoiceline.JI_PreviousEntryNumber = "010151234567002000";
			invoiceline.JI_OriginalStateDocType = "02";
			invoiceline.JI_SerialNumber = "010101010";
			invoiceline.JI_SequenceNumber = 1;
			invoiceline.JI_Ingredient = "ABCD9589376";
			invoiceline.SupportingDocumentReferenceNumber = "L172770912346";
			invoiceline.SupportingDocumentCode = "2";
			entry.MergedLines[1].Delete();

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DR).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertLine(amendmentHeader, "21", "STAINLESS STEEL", "Change STAINLESS STEEL", 1, "3");
			AssertLine(amendmentHeader, "22", "000000000", "010101010", 1, "3");
			AssertLine(amendmentHeader, "23", "1234567890", "0987654321", 1, "3");
			AssertLine(amendmentHeader, "24A", "9999", "8888", 1, "3");
			AssertLine(amendmentHeader, "24B", "KG", "G", 1, "3");
			AssertLine(amendmentHeader, "25", "10000", "9.999", 1, "3");
			AssertLine(amendmentHeader, "26", "1000", "500", 1, "3");
			AssertLine(amendmentHeader, "27", "99", "88", 1, "3");
			AssertLine(amendmentHeader, "28", "VL", "BK", 1, "3");
			AssertLine(amendmentHeader, "29", "1", "2", 1, "3");
			AssertLine(amendmentHeader, "30", "L172770912345", "L172770912346", 1, "3");
			AssertLine(amendmentHeader, "31", "20130101", "20130102", 1, "3");
			AssertLine(amendmentHeader, "32A", "010151234567001999", "010151234567002000", 1, "3");
			AssertLine(amendmentHeader, "32B", "01", "02", 1, "3");
			AssertLine(amendmentHeader, "37", "ABCD9589375", "ABCD9589376", 1, "3");

			AssertLine(amendmentHeader, "", "", "", 2, "2");
		}

		void AssertHeader(LocalExportAmendEntryHeader amendmentHeader, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = amendmentHeader.AmendedItems.Cast<LocalExportAmendItem>().FirstOrDefault(x => x.ItemSequenceNumber == 0 && x.DataItemNo == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals("3", amendItem.AmendType);
			AssertEquals(beforeDescription, amendItem.BeforeValue);
			AssertEquals(afterDescription, amendItem.AfterValue);
		}

		void AssertLine(LocalExportAmendEntryHeader amendmentHeader, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription, ZInt itemSeqNo, ZString amendType)
		{
			var amendItem = amendmentHeader.AmendedItems.Cast<LocalExportAmendItem>().FirstOrDefault(x => x.ItemSequenceNumber == itemSeqNo && x.DataItemNo == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals(amendType, amendItem.AmendType);
			AssertEquals(beforeDescription, amendItem.BeforeValue);
			AssertEquals(afterDescription, amendItem.AfterValue);
		}

		void CreateSnapShot(CusEntryHeader entry, LocalExportEntryHeader originalHeader, string messageType)
		{
			using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, messageType);
				Factory.Save();
			}
		}

		RefVessel CreateRefVessel(string rv_Code, string rv_MalaysiaVesselId, string rv_RadioCallSign)
		{
			RefVessel result = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, rv_Code));
			if (result == null)
			{
				result = RefVessel.New(Factory);
				result.RV_Code = rv_Code;
				result.RV_MalaysiaVesselId = rv_MalaysiaVesselId;
				result.RV_RadioCallSign = rv_RadioCallSign;
			}
			return result;
		}
	}
}
