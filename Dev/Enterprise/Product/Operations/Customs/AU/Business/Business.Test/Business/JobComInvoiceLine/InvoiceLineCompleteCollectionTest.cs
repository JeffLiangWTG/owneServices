using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestOriginAndStateDefaultedFromInvoiceHeaderForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();//It is important to use FilteredInvoiceLines here
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_AUState = "NSW";

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Line 2 Origin copied from the previous line", "AU", line2.JI_CountryOfOrigin);
			AssertEquals("Line 2 AU State copied from the previous line", "NSW", line2.JI_AUState);
		}

		public void TestPreferenceAndOriginNotDefaultedFromInvoiceHeaderForImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.AddInfo.ZA_ORG = "NZ";
			invoiceHeader.AddInfo.ZA_PRF = "S";

			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();//It is important to use FilteredInvoiceLines here
			AssertEquals("PreCondition:InvoiceHeader", invoiceHeader.PK, invoiceLine.JI_JZ);
			AssertEquals("InvoiceLine Preference", "", invoiceLine.AddInfo.ZA_PRF);
			AssertEquals("AddInfo Invoice line origin", "", invoiceLine.AddInfo.ZA_ORG);
			AssertEquals("Invoice line origin", "", invoiceLine.JI_CountryOfOrigin);
		}

		public void TestDefaultsForDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.AddInfo.ZA_DAM_Hidden = "B";
			invoiceHeader.AddInfo.ZA_EDN_Hidden = "EDN";
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("B", line1.DrawbackAssesmentMethod);
			AssertEquals("EDN", line1.AddInfo.ZA_EDN_Hidden);
			line1.AddInfo.ZA_DAM_Hidden = "C";
			line1.AddInfo.ZA_EDN_Hidden = "EDN2";
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("B", line2.DrawbackAssesmentMethod);
			AssertEquals("EDN", line2.AddInfo.ZA_EDN_Hidden);
		}

		public void TestEDNDefaultForDrawbackChangesWhenInvoiceChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV00239-1";
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.AddInfo.ZA_EDN_Hidden = "ACMXG34KC";

			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV00239-2";
			invoiceHeader2.JZ_InvoiceAmount = 2000m;
			invoiceHeader2.AddInfo.ZA_EDN_Hidden = "ACNR4EE7T";

			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_JZ = invoiceHeader1.PK;
			AssertEquals("InvoiceNumber", "INV00239-1", line1.InvoiceNumber);
			AssertEquals("EDN for line 1 should default from Invoice Header 1", "ACMXG34KC", line1.AddInfo.ZA_EDN_Hidden);

			line1.JI_JZ = invoiceHeader2.PK;
			AssertEquals("InvoiceNumber changed to Invoice Header 1", "INV00239-2", line1.InvoiceNumber);
			AssertEquals("EDN for line 1 should now default from Invoice Header 2", "ACNR4EE7T", line1.AddInfo.ZA_EDN_Hidden);
		}

		public void TestWhenEDNIsChangedOnHeaderAllRelatedLinesForThisInvoiceEDNValueChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV00239-1";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.AddInfo.ZA_EDN_Hidden = "ACMXG34KC";

			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV00239-2";
			invoiceHeader2.JZ_InvoiceAmount = 2000m;
			invoiceHeader2.AddInfo.ZA_EDN_Hidden = "ACNR4EE7T";

			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_JZ = invoiceHeader.PK;
			AssertEquals("InvoiceNumber", "INV00239-1", line1.InvoiceNumber);
			AssertEquals("EDN for line 1 should default from Invoice Header 1", "ACMXG34KC", line1.AddInfo.ZA_EDN_Hidden);

			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_JZ = invoiceHeader.PK;
			AssertEquals("InvoiceNumber", "INV00239-1", line2.InvoiceNumber);
			AssertEquals("EDN for line 2 should default from Invoice Header 1", "ACMXG34KC", line2.AddInfo.ZA_EDN_Hidden);

			var line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_JZ = invoiceHeader.PK;
			AssertEquals("InvoiceNumber", "INV00239-1", line3.InvoiceNumber);
			AssertEquals("EDN for line 3 should default from Invoice Header 1", "ACMXG34KC", line3.AddInfo.ZA_EDN_Hidden);

			var line4 = declaration.FilteredInvoiceLines.AddNew();
			line4.JI_JZ = invoiceHeader2.PK;
			AssertEquals("InvoiceNumber should match Invoice 2", "INV00239-2", line4.InvoiceNumber);
			AssertEquals("EDN for line 4 should default from Invoice Header 2 EDN", "ACNR4EE7T", line4.AddInfo.ZA_EDN_Hidden);

			var line5 = declaration.FilteredInvoiceLines.AddNew();
			line5.JI_JZ = invoiceHeader2.PK;
			AssertEquals("InvoiceNumber should match Invoice 2", "INV00239-2", line5.InvoiceNumber);
			AssertEquals("EDN for line 5 should default from Invoice Header 2 EDN", "ACNR4EE7T", line5.AddInfo.ZA_EDN_Hidden);
			line5.AddInfo.ZA_EDN_Hidden = "AAAC64TEC";  // line value is no longer effective value from invoice

			invoiceHeader.AddInfo.ZA_EDN_Hidden = "ACMTR721B";
			AssertEquals("EDN for line 1 should have changed to match Invoice Header 1 changed value", "ACMTR721B", line1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 2 should have changed to match Invoice Header 1 changed value", "ACMTR721B", line2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 3 should have changed to match Invoice Header 1 changed value", "ACMTR721B", line3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 4 should not have been changed", "ACNR4EE7T", line4.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 5 should also be unaffected", "AAAC64TEC", line5.AddInfo.ZA_EDN_Hidden);

			invoiceHeader2.AddInfo.ZA_EDN_Hidden = "AAAC64RXG";
			AssertEquals("EDN for line 1 should be unaffected", "ACMTR721B", line1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 2 should be unaffected", "ACMTR721B", line2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 3 should be unaffected", "ACMTR721B", line3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 4 should now have been changed as related invoice value has changed", "AAAC64RXG", line4.AddInfo.ZA_EDN_Hidden);
			AssertEquals("EDN for line 5 should not have changed as it already had a specific value entered at line level", "AAAC64TEC", line5.AddInfo.ZA_EDN_Hidden);
		}

		public void TestDrawbackBOMProcessing()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			AUOrgSupplierPart testPart1 = CreatePart(importer, "NORMAL PART", "TESTPART1", "BOX", 0m, "", "");
			AUOrgSupplierPart testPart2 = CreatePart(importer, "PARENT ITEM 1", "TESTPART2", "BOX", 10m, "UNT", "BOX");
			AUOrgSupplierPart testPart2A = CreatePart(importer, "CHILD 1 OF TESTPART2", "TESTPART2A", "PKG", 0m, "", "");
			AUOrgSupplierPart testPart2B = CreatePart(importer, "CHILD 2 OF TESTPART2", "TESTPART2B", "T", 0m, "", "");
			AUOrgSupplierPart testPart2C = CreatePart(importer, "CHILD 3 OF TESTPART2", "TESTPART2C", "BOX", 5m, "UNT", "BOX");
			AUOrgSupplierPart testPart2D = CreatePart(importer, "CHILD 4 OF TESTPART2", "TESTPART2D", "UNT", 0m, "", "");
			AUOrgSupplierPart testPart2E = CreatePart(importer, "CHILD 5 OF TESTPART2 - SUB ASSEMBLY", "TESTPART2E", "CAS", 2m, "UNT", "CAS");
			AUOrgSupplierPart testPart3A = CreatePart(importer, "CHILD 1 OF testPart2E", "TESTPART3A", "BOX", 25m, "UNT", "BOX");
			AUOrgSupplierPart testPart3B = CreatePart(importer, "CHILD 3 OF testPart2E", "TESTPART3B", "UNT", 0m, "", "");
			AUOrgSupplierPart testPart4 = CreatePart(importer, "PARENT ITEM 2", "TESTPART4", "T", 0m, "", "");
			AUOrgSupplierPart testPart4A = CreatePart(importer, "CHILD 1 OF testPart4 (KG/T CONVERSION)", "TESTPART4A", "UNT", 0m, "", "");
			AUOrgSupplierPart testPart5 = CreatePart(importer, "PARENT ITEM 3", "TESTPART5", "T", 0m, "", "");
			AUOrgSupplierPart testPart5A = CreatePart(importer, "CHILD 1 OF testPart5 (INVALID CONVERSION)", "TESTPART5A", "UNT", 0m, "", "");

			OrgPartBOM bom1 = CreateBOM(testPart2, testPart2A, 2m, "PKG");
			OrgPartBOM bom2 = CreateBOM(testPart2, testPart2B, 100m, "KG");
			OrgPartBOM bom3 = CreateBOM(testPart2, testPart2C, 3m, "UNT");
			OrgPartBOM bom4 = CreateBOM(testPart2, testPart2D, 1.5m, "BOX");
			OrgPartBOM bom5 = CreateBOM(testPart2, testPart2E, 4m, "UNT");
			OrgPartBOM bom6 = CreateBOM(testPart2E, testPart3A, 5m, "UNT");
			OrgPartBOM bom7 = CreateBOM(testPart2E, testPart2B, 1250m, "G");
			OrgPartBOM bom8 = CreateBOM(testPart2E, testPart3B, 4m, "UNT");
			OrgPartBOM bom9 = CreateBOM(testPart4, testPart4A, 30m, "UNT");
			OrgPartBOM bom10 = CreateBOM(testPart5, testPart5A, 30m, "UNT");

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OH_Importer = importer.PK;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 4000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceDate = new ZDateTime(2005, 4, 1);

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "TESTPART2";
			invoiceLine1.JI_Tariff = "8544.41.90 23";
			invoiceLine1.AddInfo.ZA_PST = "GEN";
			invoiceLine1.AddInfo.ZA_RNO = "001";
			invoiceLine1.JI_InvoiceQuantity = 25m;
			invoiceLine1.JI_InvoiceUQ = "UNT";
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.AddInfo.ZA_DAM_Hidden = "C";
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "TESTPART1";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "BOX";
			invoiceLine2.JI_LinePrice = 1000m;
			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "TESTPART4";
			invoiceLine3.JI_InvoiceQuantity = 500m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_LinePrice = 1000m;
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = "TESTPART5";
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_InvoiceUQ = "BOX";
			invoiceLine4.JI_LinePrice = 1000m;

			AssertEquals("4 line initially", 4, testDec.InvoiceLines.Count);
			AssertEquals("original line 1 correct line number", (ZShort)1, invoiceLine1.JI_LineNo);
			AssertEquals("original line 2 correct line number", (ZShort)2, invoiceLine2.JI_LineNo);
			AssertEquals("original line 3 correct line number", (ZShort)3, invoiceLine3.JI_LineNo);
			AssertEquals("original line 4 correct line number", (ZShort)4, invoiceLine4.JI_LineNo);
			AssertEquals("calculated customs value on collapsed line", 300m, invoiceLine1.DrawbackCustomsValue);
			AssertEquals("calculated duty rate collapsed line", 5m, invoiceLine1.DrawbackDutyRate);
			AssertEquals("calculated claim amount on collapsed line", 15m, invoiceLine1.DrawbackDutyAmount);
			testDec.InvoiceLines.ExpandAllBOMProductLines();
			AssertEquals("10 new lines created", 14, testDec.InvoiceLines.Count);
			AssertEquals("original line 1 correct line number", (ZShort)1, invoiceLine1.JI_LineNo);
			AssertEquals("original line 2 correct line number", (ZShort)10, invoiceLine2.JI_LineNo);
			AssertEquals("original line 3 correct line number", (ZShort)11, invoiceLine3.JI_LineNo);
			AssertEquals("original line 4 correct line number", (ZShort)13, invoiceLine4.JI_LineNo);
			AssertEquals("calculated customs value on expanded line", 0m, invoiceLine1.DrawbackCustomsValue);
			AssertEquals("calculated duty rate expanded line", 0m, invoiceLine1.DrawbackDutyRate);
			AssertEquals("calculated claim amount on expanded line", 0m, invoiceLine1.DrawbackDutyAmount);
			AssertLineIsCorrect(invoiceLine1.PK, testDec.InvoiceLines, testPart2A.OP_PartNum, 5m, "PKG", (ZShort)2);
			AssertLineIsCorrect(invoiceLine1.PK, testDec.InvoiceLines, testPart2B.OP_PartNum, 250m, "KG", (ZShort)3);
			AssertLineIsCorrect(invoiceLine1.PK, testDec.InvoiceLines, testPart2C.OP_PartNum, 7.5m, "UNT", (ZShort)4);
			AssertLineIsCorrect(invoiceLine1.PK, testDec.InvoiceLines, testPart2D.OP_PartNum, 3.75m, "BOX", (ZShort)5);
			AssertLineIsCorrect(invoiceLine1.PK, testDec.InvoiceLines, testPart2E.OP_PartNum, 10m, "UNT", (ZShort)6);
			JobComInvoiceLine foundLine = null;
			foreach (JobComInvoiceLine line in testDec.InvoiceLines)
			{
				if (line.JI_PartNo == testPart2E.OP_PartNum)
				{
					foundLine = line;
					break;
				}
			}
			if (foundLine == null)
			{
				Fail("testPart2E Line not found");
			}
			else
			{
				AssertLineIsCorrect(foundLine.PK, testDec.InvoiceLines, testPart3A.OP_PartNum, 25m, "UNT", (ZShort)7);
				AssertLineIsCorrect(foundLine.PK, testDec.InvoiceLines, testPart2B.OP_PartNum, 6250m, "G", (ZShort)8);
				AssertLineIsCorrect(foundLine.PK, testDec.InvoiceLines, testPart3B.OP_PartNum, 20m, "UNT", (ZShort)9);
			}
			AssertLineIsCorrect(invoiceLine3.PK, testDec.InvoiceLines, testPart4A.OP_PartNum, 15m, "UNT", (ZShort)12);
			AssertLineIsCorrect(invoiceLine4.PK, testDec.InvoiceLines, testPart5A.OP_PartNum, 0m, "UNT", (ZShort)14);
			testDec.InvoiceLines.ExpandAllBOMProductLines();
			AssertEquals("Should not be any more lines added", 14, testDec.InvoiceLines.Count);

			testDec.InvoiceLines.CollapseOneBOMProductLine(invoiceLine1);
			AssertEquals("Should now be 6 lines", 6, testDec.InvoiceLines.Count);
			AssertEquals("InvoiceLine1 should be BOM line", true, invoiceLine1.IsBOMParentLine);
			AssertEquals("InvoiceLine1 should not be expanded", false, invoiceLine1.IsBOMLineExpanded);
			AssertLineIsCorrect(invoiceLine3.PK, testDec.InvoiceLines, testPart4A.OP_PartNum, 15m, "UNT", (ZShort)0);
			AssertLineIsCorrect(invoiceLine4.PK, testDec.InvoiceLines, testPart5A.OP_PartNum, 0m, "UNT", (ZShort)0);
			testDec.InvoiceLines.CollapseAllBOMProductLines();
			AssertEquals("Should now be 4 lines", 4, testDec.InvoiceLines.Count);
			AssertEquals("InvoiceLine3 should be BOM line", true, invoiceLine3.IsBOMParentLine);
			AssertEquals("InvoiceLine3 should not be expanded", false, invoiceLine3.IsBOMLineExpanded);
			AssertEquals("InvoiceLine4 should be BOM line", true, invoiceLine4.IsBOMParentLine);
			AssertEquals("InvoiceLine4 should not be expanded", false, invoiceLine4.IsBOMLineExpanded);

			testDec.InvoiceLines.ExpandOneBOMProductLine(invoiceLine4);
			AssertEquals("Should now be 5 lines", 5, testDec.InvoiceLines.Count);
			testDec.InvoiceLines.ExpandOneBOMProductLine(invoiceLine1);
			AssertEquals("Should now be 13 lines", 13, testDec.InvoiceLines.Count);
			testDec.InvoiceLines.ExpandAllBOMProductLines();
			AssertEquals("Should now be 14 lines", 14, testDec.InvoiceLines.Count);
			testDec.InvoiceLines.CollapseAllBOMProductLines();
			AssertEquals("Should now be 4 lines", 4, testDec.InvoiceLines.Count);
			AssertEquals("calculated customs value on collapsed line", 300m, invoiceLine1.DrawbackCustomsValue);
			AssertEquals("calculated duty rate collapsed line", 5m, invoiceLine1.DrawbackDutyRate);
			AssertEquals("calculated claim amount on collapsed line", 15m, invoiceLine1.DrawbackDutyAmount);
		}

		void AssertLineIsCorrect(ZGuid parentLine, InvoiceLineCompleteCollection invoiceLines, ZString partNum, ZDecimal invoiceQty, ZString invoiceUnits, ZShort lineNumber)
		{
			JobComInvoiceLine foundLine = null;
			string lineID = partNum + "/" + invoiceQty.ToString() + "/" + invoiceUnits;
			foreach (JobComInvoiceLine line in invoiceLines)
			{
				if (line.BOMParentLine != null && line.BOMParentLine.PK == parentLine && line.JI_PartNo == partNum)
				{
					foundLine = line;
					break;
				}
			}
			if (foundLine == null)
			{
				Fail("Required Invoice Line not found: " + lineID);
			}
			else
			{
				AssertEquals("Quantity incorrect on: " + lineID, invoiceQty, foundLine.JI_InvoiceQuantity);
				AssertEquals("Units incorrect on: " + lineID, invoiceUnits, foundLine.JI_InvoiceUQ);
				if (lineNumber > 0)
				{
					AssertEquals("Line number incorrect on: " + lineID, lineNumber, foundLine.JI_LineNo);
				}
			}
		}

		AUOrgSupplierPart CreatePart(OrgHeader owner, ZString description, string partNum, ZString stockKeepingUnit, ZDecimal unitConversionQty, ZString package, ZString parentPackage)
		{
			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			part.OP_PartNum = partNum;
			part.OP_Desc = description;
			part.OP_StockKeepingUnit = stockKeepingUnit;
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = owner.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			if (!package.IsEmpty)
			{
				OrgPartUnit uNTtoCTN = part.PartUnits.AddNew();
				uNTtoCTN.OF_QuantityInParent = unitConversionQty;
				uNTtoCTN.OF_PackType = package;
				uNTtoCTN.OF_ParentPackType = parentPackage;
			}
			return part;
		}

		OrgPartBOM CreateBOM(AUOrgSupplierPart parentPart, AUOrgSupplierPart childPart, ZDecimal quantityPer, ZString stockUnits)
		{
			OrgPartBOM bom = parentPart.BillOfMaterials.AddNew();
			bom.OE_OP_Component = childPart.PK;
			bom.OE_ComponentQty = quantityPer;
			bom.OE_F3_NKPackType = stockUnits;
			return bom;
		}

		public void TestSetDefaultValuesForNewChildFromPreviousRow()
		{
			ZGuid supplierPK = Factory.New(typeof(OrgHeader)).PK;
			ZGuid secondSupplierPK = Factory.New(typeof(OrgHeader)).PK;

			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier = supplierPK;
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_ValuationBasis_Hidden = "UT";

			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader firstLine = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			JobComInvoiceHeader secondLine = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1];
			Assert("PK's should be different", firstLine.PK != secondLine.PK);
			AssertEquals("Supplier", firstLine.JZ_OH_Supplier, secondLine.JZ_OH_Supplier);
			AssertEquals("IncoTerm", firstLine.JZ_IncoTerm, secondLine.JZ_IncoTerm);
			AssertEquals("ValB", firstLine.AddInfo.ZA_ValuationBasis_Hidden, secondLine.AddInfo.ZA_ValuationBasis_Hidden);

			secondLine.JZ_OH_Supplier = secondSupplierPK;
			secondLine.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			secondLine.AddInfo.ZA_ValuationBasis_Hidden = "RT";

			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader thirdLine = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[2];

			Assert("PK's should be different", firstLine.PK != thirdLine.PK);
			Assert("PK's should be different", thirdLine.PK != secondLine.PK);
			AssertEquals("Supplier", secondLine.JZ_OH_Supplier, thirdLine.JZ_OH_Supplier);
			AssertEquals("IncoTerm", secondLine.JZ_IncoTerm, thirdLine.JZ_IncoTerm);
			AssertEquals("ValB", secondLine.AddInfo.ZA_ValuationBasis_Hidden, thirdLine.AddInfo.ZA_ValuationBasis_Hidden);
		}

		public void TestDefaultCommodityCodeFromOrgHeader()
		{
			RefCommodityCode commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "XXXX";
			RefCommodityCode commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "YYYY";
			RefCommodityCode commodity3 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "XXYY";
			RefCommodityCode commodity4 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "YYXX";

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_RH_NKCMMainImportCmdty = commodity1.RH_Code;
			importer.MiscServ.OM_RH_NKCMMainExportCmdty = commodity2.RH_Code;

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.MiscServ.OM_RH_NKCMMainImportCmdty = commodity3.RH_Code;
			supplier.MiscServ.OM_RH_NKCMMainExportCmdty = commodity4.RH_Code;

			declaration.JE_OH_Importer = importer.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();//It is important to use FilteredInvoiceLines here
			AssertEquals("Invoice Line's commodity code should default from Importer's Import Cmdty Code", commodity1.RH_Code, invoiceLine.JI_RH_NKCommodity_Code);
		}

		public void TestHasAnAmberReason()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			AssertEquals("HasAnAmberReason", false, declaration.InvoiceLines.HasAnAmberReason);

			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("HasAnAmberReason", false, declaration.InvoiceLines.HasAnAmberReason);

			invoiceLine.AddInfo.ZA_AMB = "C";
			AssertEquals("HasAnAmberReason", true, declaration.InvoiceLines.HasAnAmberReason);

			invoiceLine.AddInfo.ZA_AMB = "";
			AssertEquals("HasAnAmberReason", false, declaration.InvoiceLines.HasAnAmberReason);
		}

		public void TestHasADrawbackAmberReason()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			AssertEquals("HasADrawbackAmberReason", false, declaration.InvoiceLines.HasADrawbackAmberReason);

			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("HasADrawbackAmberReason", false, declaration.InvoiceLines.HasADrawbackAmberReason);

			invoiceLine.AddInfo.ZA_DARC_Hidden = "C";
			AssertEquals("HasADrawbackAmberReason", true, declaration.InvoiceLines.HasADrawbackAmberReason);

			invoiceLine.AddInfo.ZA_DARC_Hidden = "";
			AssertEquals("HasADrawbackAmberReason", false, declaration.InvoiceLines.HasADrawbackAmberReason);
		}

		public void TestSetDefaultsForNewChild()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Invoice Line 1 doesn't have draw back for Quarantine", !invoiceLine1.JI_Drawback);
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Invoice Line 2 doesn't have draw back for Quarantine", !invoiceLine2.JI_Drawback);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader = Declaration.Invoices[0];
			invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Invoice Line 1 doesn't have draw back for Export", invoiceLine1.JI_Drawback);
			invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Invoice Line 2 doesn't have draw back for Export", invoiceLine2.JI_Drawback);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		new JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		#endregion
	}
}
