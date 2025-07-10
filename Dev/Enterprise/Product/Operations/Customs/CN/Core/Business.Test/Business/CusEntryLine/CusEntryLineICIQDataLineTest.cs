using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryLineICIQDataLineTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var testBondage = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var invoiceLine1 = testBondage.InvoiceLine;
			var invoiceLine2 = testBondage.EntryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine3 = testBondage.EntryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.CIQIngredient = "Ingredient1";
			invoiceLine2.CIQIngredient = "Ingredient2";
			invoiceLine3.CIQIngredient = "Ingredient0";
			invoiceLine1.JI_CIQExpiryDate = new ZDateTime(2019, 12, 2);
			invoiceLine2.JI_CIQExpiryDate = new ZDateTime(2019, 12, 3);
			invoiceLine3.JI_CIQExpiryDate = new ZDateTime(2019, 12, 1);
			invoiceLine1.JI_CIQQualityGuaranteePeriod = 20;
			invoiceLine1.JI_NDescription = "Spec1";
			invoiceLine2.JI_NDescription = "Spec1";
			invoiceLine3.JI_NDescription = "Spec3";
			invoiceLine1.JI_Model = "CIQ Model 1";
			invoiceLine2.JI_Model = "CIQ Model 2";
			invoiceLine3.JI_Model = "CIQ Model 1";
			invoiceLine1.JI_BrandName = "CIQ Brand 2";
			invoiceLine2.JI_BrandName = "CIQ Brand 3";
			invoiceLine3.JI_BrandName = "CIQ Brand 1";
			var manufacturer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Manufacturer 1", "", "", "CIQ01");
			invoiceLine1.JI_OA_ManufacturerAddress = manufacturer.PK;
			var pb11 = invoiceLine1.ProductionBatch.AddNew();
			pb11.CY_Data = "PB2";
			pb11.CY_Date = new ZDateTime(2019, 10, 1);
			var pb12 = invoiceLine1.ProductionBatch.AddNew();
			pb12.CY_Data = "PB1";
			pb12.CY_Date = new ZDateTime(2019, 10, 2);
			var pb21 = invoiceLine2.ProductionBatch.AddNew();
			pb21.CY_Data = "PB";
			pb21.CY_Date = new ZDateTime(2019, 12, 2);
			var ca11 = invoiceLine1.CargoAttributes.AddNew();
			ca11.CY_Code = "11";
			ca11.CY_Type = "CAT"; // 3C目录内
			invoiceLine1.JI_CIQEndUse = "19";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0000";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_FlashPoint = "-4 cc";
			subs.DG_PG = "III";
			subs.DG_PSN = "I am very dangerous";
			subs.DG_Class = "8";
			invoiceLine1.JI_NonDangerousChemicalFlag = false;
			invoiceLine1.JI_PackageTypeOfUNDG = "1D";
			var undg = invoiceLine1.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			var chsName = undg.Substance.Names.AddNew();
			chsName.DA_Language = Core.Constants.Languages.ChineseSimplified;
			chsName.DA_Descriptor = "Chinese 1"; // Name of UNDG
			var mergedCIQData = (ICIQDataLine)testBondage.EntryLine;
			CombineAssertions("Asserting all properties of MergedCIQData", () =>
			{
				AssertEquals("Ingredient", "Ingredient1", mergedCIQData.Ingredient);
				AssertEquals("ExpiryDateAsString", new ZDateTime(2019, 12, 1), mergedCIQData.ExpiryDate);
				AssertEquals("QGPByDays", (ZShort)20, mergedCIQData.QGPByDays);
				AssertEquals("Specification", "Spec1", mergedCIQData.Specification);
				AssertEquals("Model", "CIQ Model 1", mergedCIQData.Model);
				AssertEquals("Brand", "CIQ Brand 2", mergedCIQData.Brand);
				AssertEquals("ManufacturerCIQNum", "CIQ01", mergedCIQData.ManufacturerCIQ);
				AssertEquals("ManufacturerName", "Manufacturer 1", mergedCIQData.ManufacturerName);
				AssertEquals("ManufactureDate", new ZDateTime(2019, 10, 1), mergedCIQData.ManufactureDate);
				AssertEquals("BatchNumber", "PB;PB1;PB2", mergedCIQData.BatchNumber);
				AssertEquals("CargoAttribute", "11", mergedCIQData.CargoAttributes.JoinAsString());
				AssertEquals("EndUse", "19", mergedCIQData.EndUse);
				AssertEquals("NonDangerousChemical", false, mergedCIQData.NonDangerousChemical);
				AssertEquals("UNDGNumber", "0000A", mergedCIQData.UNDGNumber);
				AssertEquals("UNDGClass", "8", mergedCIQData.UNDGClass);
				AssertEquals("UNDGPackingGroup", "III", mergedCIQData.UNDGPackingGroup);
			}

			);
			var dgSubstance = invoiceLine1.DangerousGoods.Substance;
			dgSubstance.DG_SubLabel1 = "3A";
			dgSubstance.DG_SubLabel2 = "6.1D";
			AssertEquals("UNDGClass", "8+3+6.1", mergedCIQData.UNDGClass);
		}

		public void TestProductQualificationsAsString()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var pq1 = invoiceLine1.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "106";
			pq1.CSI_ReferenceNumber = "001";
			pq1.CSI_LineNo = 1;
			pq1.CSI_Quantity = 1;
			pq1.CSI_UnitOfQuantity = "010";
			var pq2 = invoiceLine2.CIQProductQualifications.AddNew();
			pq2.CSI_Code = "106";
			pq2.CSI_ReferenceNumber = "001";
			pq2.CSI_LineNo = 1;
			pq2.CSI_Quantity = 2;
			pq2.CSI_UnitOfQuantity = "010";
			AssertEquals("106:001/1/3 010", entryLine.ProductQualifications.JoinAsString());
			var pq3 = invoiceLine2.CIQProductQualifications.AddNew();
			pq3.CSI_Code = "107";
			pq3.CSI_ReferenceNumber = "002";
			pq3.CSI_LineNo = 2;
			pq3.CSI_Quantity = 1;
			pq3.CSI_UnitOfQuantity = "010";
			entryLine.ClearCachedMergedData();
			AssertEquals("106:001/1/3 010,107:002/2/1 010", entryLine.ProductQualifications.JoinAsString());
		}
	}
}
