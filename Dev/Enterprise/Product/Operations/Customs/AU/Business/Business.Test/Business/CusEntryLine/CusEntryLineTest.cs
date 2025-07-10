using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestZA_AggregateEntryLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.ZA_AggregateEntryLineNumber = 1;
			Factory.Save();

			var entryLineFromAnotherFactory = new BusinessObjectFactory().Load<CusEntryLine>(entryLine.PK);
			AssertEquals("Should be able to get the addinfo value from database", (ZShort)1, entryLineFromAnotherFactory.ZA_AggregateEntryLineNumber);
		}

		public void TestSouceToDefaultShouldNotContainsDuplicateElementForPerformance()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "Part";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "10203040";

			var classification = Factory.New<Classification>();
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.SetPartForTesting(part);
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.SetPartForTesting(part);
			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine.PK;
			line3.JI_CC = classification.PK;
			var line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine.PK;
			line4.JI_CC = classification.PK;
			var line5 = invoice.JobComInvoiceLines.AddNew();
			line5.JI_CL = entryLine.PK;
			var line6 = invoice.JobComInvoiceLines.AddNew();
			line6.JI_CL = entryLine.PK;

			AssertEquals(3, ((ICPQALineAttachee)entryLine).SourcesToDefault.Length);
		}

		public void TestOrderedVINs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.SetSupportsBondedWarehousingForTesting(true);

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "21 003 980 130");

			declaration.JE_OH_Importer = importer.PK;

			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "Part1";
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OH = importer.PK;

			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_PartNo = "Part1";
			invoiceLine.JI_PartAttrib1 = "TMBGE61Z582031468";

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			ZString[] vIDs = entryLine.OrderedVIDs;
			AssertEquals("There should be a VIN number from Part Attribute1", "TMBGE61Z582031468", vIDs[0]);
		}

		public void TestFlatRateDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_FlatAmount = 10.12345m;
			entryLine.CL_FlatAmountUQ = "LA";
			AssertEquals("Flat rate description", "10.12345/LA", entryLine.FlatRateDescription);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryLine entryLineInFactory2 = factory2.Load<CusEntryLine>(entryLine.PK);
			AssertEquals("Flat rate description ex DB", "10.12345/LA", entryLineInFactory2.FlatRateDescription);

			entryLine.CL_FlatAmount = 0m;
			entryLine.CL_FlatAmountUQ = "";
			AssertEquals("Flat rate description", "", entryLine.FlatRateDescription);
		}

		public void TestTransportAndInsuranceRefreshedWhenMergeIsDone()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_ExportDate = ZDateTime.Today;
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				invoice.JZ_IncoTerm = "FOB";
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, "AUD");

				JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;

				LineMerger merger = new LineMerger(declaration);
				merger.DoMerge();

				AssertEquals("There should be one entry header created", 1, declaration.CustomsEntryHeaders.Count);

				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				CusEntryLine entryLine = entryHeader.MergedLines[0];
				AssertEquals("Transport and Insurance", 500m, entryHeader.TransportAndInsurance.Amount);
				AssertEquals("Transport and Insurance Currency", "AUD", entryHeader.TransportAndInsurance.Currency.Code);
				AssertEquals("Transport and Insurance", 500m, entryLine.TransportAndInsurance.Amount);
				AssertEquals("Transport and Insurance Currency", "AUD", entryLine.TransportAndInsurance.Currency.Code);

				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, "AUD");
				merger.DoMerge();
				entryHeader = declaration.CustomsEntryHeaders[0];
				entryLine = entryHeader.MergedLines[0];
				AssertEquals("Transport and Insurance", 550m, entryHeader.TransportAndInsurance.Amount);
				AssertEquals("Transport and Insurance Currency", "AUD", entryHeader.TransportAndInsurance.Currency.Code);
				AssertEquals("Transport and Insurance", 550m, entryLine.TransportAndInsurance.Amount);
				AssertEquals("Transport and Insurance Currency", "AUD", entryLine.TransportAndInsurance.Currency.Code);
			}
		}

		public void TestTILV()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			line.AddInfo.ZA_TILV = "1.23AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine = line.CusEntryLine;
			AssertEquals("TILV equals calculated", 1.23m, cusEntryLine.TILV);

			cusEntryLine.UpdateTILV("2.46AUD");
			AssertEquals("TILV equals Customs Response Value", 2.46m, cusEntryLine.TILV);
		}

		public void TestDoesTILVExist()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, "AUD");

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			line.AddInfo.ZA_TILV = "0.00AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DoesTILVExist", true, line.CusEntryLine.DoesTILVExist);

			line.AddInfo.ZA_TILV = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DoesTILVExist", false, line.CusEntryLine.DoesTILVExist);

			line.AddInfo.ZA_TILV = "30AUD";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DoesTILVExist", true, line.CusEntryLine.DoesTILVExist);
		}

		public void TestTransportAndInsuranceInACalculatedCurrencyForMessage()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.75m, helper.USDCurrency);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.5m, helper.EURCurrency);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 5000m;
			line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "USD");
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m, "EUR");

			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine2.PK;

			AssertEquals("TAndI for Line1", "USD", entryLine1.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line1", 100m, entryLine1.TransportAndInsurance.Amount);

			AssertEquals("TAndI for Line2", "EUR", entryLine2.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line2", 10m, entryLine2.TransportAndInsurance.Amount);

			AssertEquals("TAndI for Header", "AUD", entryHeader.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Header", 153.33m, entryHeader.TransportAndInsuranceForMessage.Amount);

			AssertEquals("TAndI for Line1 message", "AUD", entryLine1.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line1 message", 133.33m, entryLine1.TransportAndInsuranceForMessage.Amount);

			AssertEquals("TAndI for Line2 message", "AUD", entryLine2.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line2 message", 20m, entryLine2.TransportAndInsuranceForMessage.Amount);
		}

		public void TestTransportAndInsuranceInAConsolidatedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 6000m;
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 4000m;

			declaration.ResumeApportionment();
			AssertEquals("Invoice line 1 apportioned freight", 60m, line1.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Invoice line 2 apportioned freight", 40m, line2.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine2.PK;

			AssertEquals("TAndI for Line1 Currency", "AUD", entryLine1.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line1 Amount", 60m, entryLine1.TransportAndInsurance.Amount);
			AssertEquals("TAndI for Line2 Currency", "AUD", entryLine2.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line2 Amount", 40m, entryLine2.TransportAndInsurance.Amount);
			AssertEquals("TAndI for Header Currency", "AUD", entryHeader.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Header Amount", 0m, entryHeader.TransportAndInsuranceForMessage.Amount);
			AssertEquals("TAndI for Line1 message Currency", "AUD", entryLine1.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line1 message Amount", 0m, entryLine1.TransportAndInsuranceForMessage.Amount);
			AssertEquals("TAndI for Line2 message Currency", "AUD", entryLine2.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line2 message Amount", 0m, entryLine2.TransportAndInsuranceForMessage.Amount);

			declaration.JE_EntryStatus = "RFC";
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 0);
			consolidatedDeclaration.JobDeclarations.Add(declaration);

			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("TAndI for Line1 Currency", "AUD", entryLine1.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line1 Amount", 60m, entryLine1.TransportAndInsurance.Amount);
			AssertEquals("TAndI for Line2 Currency", "AUD", entryLine2.TransportAndInsurance.Currency.Code);
			AssertEquals("TAndI for Line2 Amount", 40m, entryLine2.TransportAndInsurance.Amount);
			AssertEquals("TAndI for Header Currency", "AUD", entryHeader.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Header Amount", 100m, entryHeader.TransportAndInsuranceForMessage.Amount);
			AssertEquals("TAndI for Line1 message Currency", "AUD", entryLine1.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line1 message Amount", 60m, entryLine1.TransportAndInsuranceForMessage.Amount);
			AssertEquals("TAndI for Line2 message Currency", "AUD", entryLine2.TransportAndInsuranceForMessage.Currency.Code);
			AssertEquals("TAndI for Line2 message Amount", 40m, entryLine2.TransportAndInsuranceForMessage.Amount);
		}

		#region ICusEntryLine Members

		public void TestWARAndImportPermitNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_WAR = "9515C";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERT2";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERT1";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERT3";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERT2";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals("WAR", "9515C", entryLine.WAR);
			AssertArrayEqualsByElements(new ZString[] { "PERT1", "PERT2", "PERT3" }, entryLine.ImportPermitNumbers.ToArray());

			invoiceLine.JI_IsPackToBondForLine = true;
			declaration.DoMerge();
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals("WAR", "", entryLine.WAR);
			AssertArrayEqualsByElements(new ZString[] { "PERT1", "PERT2", "PERT3" }, entryLine.ImportPermitNumbers.ToArray());
		}

		public void TestStatCodeForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234.23.56 1";
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Stat code for cmr", "01", entryLine.StatCodeForCMR);

			invoiceLine.JI_Tariff = "1234.23.56 21";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Stat code for cmr", "21", entryLine.StatCodeForCMR);

			invoiceLine.JI_Tariff = "1234.23.56";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Stat code for cmr", "", entryLine.StatCodeForCMR);
		}

		public void TestIsGeneralRate()
		{
			AssertEquals("IsGeneralRate", testCusLine.RandomLine.AddInfo.IsGeneralRate, testCusLine.IsGeneralRate);
		}

		public void TestSecondCustomsQuantity()
		{
			AssertEquals("SecondCustomsQuantity", testCusLine.RandomLine.AddInfo.ZA_QT2, testCusLine.SecondCustomsQuantity);
		}

		public void TestSecondCustomsUnitQty()
		{
			AssertEquals("SecondCustomsUnitQty", testCusLine.RandomLine.AddInfo.ZA_UQ2, testCusLine.SecondCustomsUnitQty);
		}

		public void TestGSTE()
		{
			var mockEntryLine = Factory.NewMoq<CusEntryLine>();
			CusEntryLine entryLine = mockEntryLine.Object;

			var mockInvoiceLine = Factory.NewMoq<JobComInvoiceLine>();
			JobComInvoiceLine invoiceLine = mockInvoiceLine.Object;

			mockInvoiceLine.Setup(m => m.DoesTariffRateOrTreatmentCodeDeemGSTExemption).Returns(ZBool.True);
			mockEntryLine.Setup(m => m.RandomLine).Returns(invoiceLine);

			invoiceLine.AddInfo.ZA_GSTE = "FOOD";
			AssertEquals("GSTE should be empty as tariff rate or treatment code deems gst exemption", "", entryLine.GSTE);

			mockInvoiceLine.Reset();
			mockInvoiceLine.Setup(m => m.DoesTariffRateOrTreatmentCodeDeemGSTExemption).Returns(ZBool.False);
			AssertEquals("GSTE should be the same as InvoiceLine", "FOOD", entryLine.GSTE);
		}

		public void TestWETE()
		{
			AssertEquals("WETE", testCusLine.RandomLine.AddInfo.ZA_WETE, testCusLine.WETE);
		}

		public void TestWETQ()
		{
			AssertEquals("WETQ", testCusLine.RandomLine.AddInfo.ZA_WETQ, testCusLine.WETQ);
		}

		public void TestWMC()
		{
			AssertEquals("WMC", testCusLine.RandomLine.AddInfo.ZA_WMC, testCusLine.WMC);
		}

		public void TestAMBExcpetion()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			AssertEquals("AMB", 0, entryLine.OrderedAMBs.Length);
		}

		public void TestOrderedAMBs()
		{
			testCusLine.RandomLine.AddInfo.ZA_AMB = "CoD";
			AssertEquals("AMB", 'C', testCusLine.OrderedAMBs[0]);
			AssertEquals("AMB", 'D', testCusLine.OrderedAMBs[1]);
			AssertEquals("AMB", 'o', testCusLine.OrderedAMBs[2]);
		}

		public void TestORG()
		{
			AssertEquals("ORG", testCusLine.RandomLine.AddInfo.ZA_ORG, testCusLine.ORG);
		}

		public void TestPOCExcpetion()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			AssertEquals("POC", ZString.Empty, entryLine.POC);
		}

		public void TestPOC()
		{
			AssertEquals("POC", testCusLine.RandomLine.AddInfo.ZA_POC, testCusLine.POC);
		}

		public void TestDCX()
		{
			AssertEquals("DCX", testCusLine.RandomLine.AddInfo.ZA_DCX, testCusLine.DCX);
		}

		public void TestFOD()
		{
			AssertEquals("FOD", testCusLine.RandomLine.AddInfo.FOD, testCusLine.FOD);
		}

		public void TestWRQ()
		{
			AssertEquals("WRQ", testCusLine.RandomLine.AddInfo.ZA_WRQ, testCusLine.WRQ);
			JobComInvoiceLine invoiceLine2 = testCusLine.Declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = testCusLine.PK;
			testCusLine.RefreshInvoiceLines();
			invoiceLine2.AddInfo.ZA_WRQ = 5;
			AssertEquals("WRQ", testCusLine.InvoiceLines[0].AddInfo.ZA_WRQ + testCusLine.InvoiceLines[1].AddInfo.ZA_WRQ, testCusLine.WRQ);
		}

		public void TestWRU()
		{
			AssertEquals("WRU", testCusLine.RandomLine.AddInfo.ZA_WRU, testCusLine.WRU);
		}

		public void TestSupplier()
		{
			var supplier1 = Factory.New<OrgHeader>();
			testInvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("Supplier on Invoice", supplier1, testCusLine.Supplier);
			var supplier2 = Factory.New<OrgHeader>();
			testInvoiceLine.JI_OH_Supplier = supplier2.PK;
			AssertEquals("Supplier on Line", supplier2, testCusLine.Supplier);
		}

		public void TestSupplierCode()
		{
			testInvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("Blank without a supplier", ZString.Empty, testCusLine.SupplierCode);

			var supplier1 = Factory.New<OrgHeader>();
			testInvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("Blank without a CustomsClientID", ZString.Empty, testCusLine.SupplierCode);

			supplier1.CustomsClientID = "ABC123";
			var cusCode = supplier1.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			cusCode.OK_OA_PremisesAddress = supplier1.MainAddress.PK;
			AssertEquals("CustomsClientID", "ABC123", testCusLine.SupplierCode);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertEquals("Blank when Premises Address not matching Main Address", ZString.Empty, testCusLine.SupplierCode);

			var address = Factory.New<OrgAddress>();
			testInvoiceHeader.JZ_OA_SupplierAddress = address.PK;
			cusCode.OK_OA_PremisesAddress = address.PK;
			AssertEquals("CustomsClientID", "ABC123", testCusLine.SupplierCode);
		}

		public void TestConsignorVendor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Test 2";
			consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
			testCusLine.RandomLine.InvoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("123", testCusLine.ConsignorVendor);
		}

		public void TestVAN()
		{
			AssertEquals("VAN", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_VAN), testCusLine.VAN);
		}

		public void TestLCP()
		{
			AssertEquals("LCP", testCusLine.RandomLine.AddInfo.ZA_LCP, testCusLine.LCP);
		}

		public void TestISS()
		{
			AssertEquals("ISS", testCusLine.RandomLine.AddInfo.ZA_ISS, testCusLine.ISS);
		}

		public void TestSCN()
		{
			AssertEquals("SCN", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_SCN), testCusLine.SCN);
		}

		public void TestPST()
		{
			AssertEquals("PST", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_PST), testCusLine.PST);
		}

		public void TestPRT()
		{
			AssertEquals("PRT", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_PRT), testCusLine.PRT);
		}

		public void TestWRN()
		{
			AssertEquals("WRN", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRN), testCusLine.WRN);
		}

		public void TestDSN()
		{
			AssertEquals("DSN", testCusLine.RandomLine.AddInfo.ZA_DSN, testCusLine.DSN);
		}

		public void TestLCTE()
		{
			AssertEquals("LCTE", testCusLine.RandomLine.AddInfo.ZA_LCTE, testCusLine.LCTE);
		}

		public void TestTR2()
		{
			AssertEquals("TR2", testCusLine.RandomLine.AddInfo.ZA_TR2, testCusLine.TR2);
		}

		public void TestCL2()
		{
			AssertEquals("CL2", testCusLine.RandomLine.AddInfo.ZA_CL2, testCusLine.CL2);
		}

		public void TestTRN()
		{
			AssertEquals("TRN", testCusLine.RandomLine.AddInfo.ZA_TRN, testCusLine.TRN);
		}

		public void TestISC()
		{
			AssertEquals("ISC", testCusLine.RandomLine.AddInfo.ZA_ISC, testCusLine.ISC);
		}

		public void TestTAN()
		{
			AssertEquals("TAN", testCusLine.RandomLine.AddInfo.ZA_TAN, testCusLine.TAN);
		}

		public void TestRNO()
		{
			AssertEquals("RNO", testCusLine.RandomLine.AddInfo.ZA_RNO, testCusLine.RNO);
		}

		public void TestICN()
		{
			AssertEquals("ICN", testCusLine.RandomLine.AddInfo.ZA_ICN, testCusLine.ICN);
		}

		public void TestTCI_InstrumentType()
		{
			AssertEquals("TCI_InstrumentType", testCusLine.RandomLine.AddInfo.TCI_InstrumentType, testCusLine.TCI_InstrumentType);
		}

		public void TestTI2_InstrumentType()
		{
			AssertEquals("TI2_InstrumentType", testCusLine.RandomLine.AddInfo.TI2_InstrumentType, testCusLine.TI2_InstrumentType);
		}

		public void TestPRI_InstrumentType()
		{
			AssertEquals("PRI_InstrumentType", testCusLine.RandomLine.AddInfo.PRI_InstrumentType, testCusLine.PRI_InstrumentType);
		}

		public void TestDXT()
		{
			AssertEquals("DXT", testCusLine.RandomLine.AddInfo.ZA_DXT, testCusLine.DXT);
		}

		public void TestInstrumentCode()
		{
			AssertEquals("InstrumentCode", testCusLine.RandomLine.InstrumentCode, testCusLine.InstrumentCode);
		}

		public void TestInstrumentType()
		{
			AssertEquals("InstrumentType", testCusLine.RandomLine.InstrumentType, testCusLine.InstrumentType);
		}

		public void TestOrderedELAs()
		{
			testCusLine.RandomLine.AddInfo.ZA_ELA = "222222, 111111";
			AssertEquals("ELA", "111111", testCusLine.OrderedELAs[0]);
			AssertEquals("ELA", "222222", testCusLine.OrderedELAs[1]);
		}

		public void TestDRE()
		{
			AssertEquals("DRE", testCusLine.RandomLine.AddInfo.ZA_DRE, testCusLine.DRE);
		}

		public void TestREL()
		{
			RunRELCheck(ZString.Empty, ZString.Empty, "N");
			RunRELCheck(ZString.Empty, CMRRelatedTransaction.Default.Code, "N");
			RunRELCheck(ZString.Empty, CMRRelatedTransaction.No.Code, "N");
			RunRELCheck(ZString.Empty, CMRRelatedTransaction.Yes.Code, "Y");

			RunRELCheck(CMRRelatedTransaction.No.Code, ZString.Empty, "N");
			RunRELCheck(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.Default.Code, "N");
			RunRELCheck(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.No.Code, "N");
			RunRELCheck(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.Yes.Code, "Y");

			RunRELCheck(CMRRelatedTransaction.Yes.Code, ZString.Empty, "Y");
			RunRELCheck(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.Default.Code, "Y");
			RunRELCheck(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.No.Code, "N");
			RunRELCheck(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.Yes.Code, "Y");
		}

		public void RunRELCheck(ZString headerREL, ZString lineREL, ZString expectedResult)
		{
			testInvoiceHeader.AddInfo.ZA_HeaderREL_Hidden = headerREL;
			testInvoiceLine.AddInfo.ZA_REL_Hidden = lineREL;
			AssertEquals("REL: " + headerREL + ":" + lineREL, expectedResult, testCusLine.REL);
		}

		public void TestSEC()
		{
			AssertEquals("SEC", testCusLine.RandomLine.AddInfo.ZA_SEC, testCusLine.SEC);
		}

		public void TestContainersForInvoiceLines()
		{
			var declaration = testCusLine.Declaration;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var containersForInvoiceLines = testCusLine.RandomLine.ContainersForInvoiceLinesForBindingOnly;
			containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine = true;

			containersForInvoiceLines = testCusLine.RandomLine.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1", false, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2", true, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);
		}

		public void TestValuationBasisForCMR()
		{
			AssertEquals("ValuationBasisForCMR", (ZString)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_VALB_Hidden), testCusLine.ValuationBasisForCMR);
		}

		public void TestWRL()
		{
			AssertEquals("WRL", (ZInt)testCusLine.InvoiceLineAddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRL), testCusLine.WRL);
		}

		public void TestOrderedVIDs()
		{
			testCusLine.RandomLine.AddInfo.ZA_VID = "22222222, 11111111";
			AssertEquals("Ordered VID", "11111111", testCusLine.OrderedVIDs[0]);
			AssertEquals("Ordered VID", "22222222", testCusLine.OrderedVIDs[1]);
		}

		public void TestSendZeroManualDuty()
		{
			testCusLine.RandomLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;
			AssertEquals("SendZeroManualDuty", true, testCusLine.SendZeroManualDuty);

			testCusLine.RandomLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = false;
			AssertEquals("SendZeroManualDuty", false, testCusLine.SendZeroManualDuty);
		}

		public void TestVIDWithBondedWarehousing()
		{
			CusEntryLine entryLine = (CusEntryLine)GetNewBusinessObject();
			JobDeclaration declaration = entryLine.Declaration;
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.OH_IsConsignee = true;
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;

			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 2, true);

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib1";
			invoiceLine.JI_PartAttrib2 = "FromPartAttrib2";
			invoiceLine.JI_IsPackToBondForLine = true;

			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			invoiceLine.AddInfo.ZA_VID = "";
			AssertEquals("FromPartAttrib1", entryLine.OrderedVIDs[0]);

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			declaration.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			invoiceLine.AddInfo.ZA_VID = "";
			AssertEquals("FromPartAttrib2", entryLine.OrderedVIDs[0]);

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			declaration.SetSupportsBondedWarehousingForTesting(false);
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			declaration.SetSupportsBondedWarehousingForTesting(true);
			invoiceLine.SetPartForTesting(null);
			invoiceLine.JI_OP = ZGuid.Empty;
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			invoiceLine.SetPartForTesting(part);
			declaration.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			declaration.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			invoiceLine.JI_IsPackToBondForLine = false;
			AssertEquals("FromAddInfo", entryLine.OrderedVIDs[0]);

			/* Should be present, but ICS has bug which rejects VINs on Nature 30. Should be fixed in November.
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("FromPartAttrib2", EntryLine.VID);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			*/
			invoiceLine.JI_IsPackToBondForLine = true;

			JobComInvoiceLine bondLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			bondLine.SetPartForTesting(part);
			bondLine.JI_IsPackToBondForLine = true;
			bondLine.JI_PartAttrib2 = "AnotherAttrib2";

			JobComInvoiceLine nonBondLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			nonBondLine.SetPartForTesting(part);
			nonBondLine.JI_IsPackToBondForLine = false;
			nonBondLine.JI_PartAttrib2 = "NonBond";

			bondLine.JI_CL = entryLine.PK;
			nonBondLine.JI_CL = entryLine.PK;
			entryLine.RefreshInvoiceLines();
			AssertEquals("First VID in OrderedVIDs", "AnotherAttrib2", entryLine.OrderedVIDs[0]);
			AssertEquals("Second VID in OrderedVIDs", "FromAddInfo", entryLine.OrderedVIDs[1]);
		}

		public void TestVIDForN10Entry()
		{
			/*
			 *  N10 entries now need to report Vehicle ID's (VINs).
			 *  N10 entries will therefore now check the addinfo field for VID but if this is empty will then check if part attributes are used for VID's
			 */
			CusEntryLine entryLine = (CusEntryLine)GetNewBusinessObject();
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.FormalEntry;
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.SetSupportsBondedWarehousingForTesting(false);
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.OH_IsConsignee = true;

			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 2, true);

			invoiceLine.JI_PartAttrib1 = "FromPartAttrib1";
			invoiceLine.JI_PartAttrib2 = "FromPartAttrib2";

			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals("N10 will now send VIN from Part Attribute as a fall back", "FromPartAttrib1", entryLine.OrderedVIDs[0]);

			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			declaration.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals("N10 will now send VIN from Part Attribute as a fall back", "FromPartAttrib2", entryLine.OrderedVIDs[0]);

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			declaration.SetSupportsBondedWarehousingForTesting(false);
			AssertEquals("VID should be picked up from AddInfo value for N10 in first instance if it exists", "FromAddInfo", entryLine.OrderedVIDs[0]);
		}

		public void TestLCTI()
		{
			AssertEquals("LCTI", testCusLine.RandomLine.AddInfo.ZA_LCTI, testCusLine.LCTI);
		}

		public void TestLCTQ()
		{
			AssertEquals("LCTQ", testCusLine.RandomLine.AddInfo.ZA_LCTQ, testCusLine.LCTQ);
		}

		public void TestMLPI()
		{
			AssertEquals("MLPI", testCusLine.RandomLine.AddInfo.ZA_MLPI, testCusLine.MLPI);
		}

		public void TestPUP()
		{
			AssertEquals("PUP", testCusLine.RandomLine.AddInfo.ZA_PUP, testCusLine.PUP);
		}

		public void TestIsExWarehouse()
		{
			AssertEquals("IsExWarehouse", testCusLine.Declaration.IsExWarehouse, testCusLine.IsExWarehouse);
		}

		public void TestTCI_InstrumentNo()
		{
			AssertEquals("TCI_InstrumentNo", testCusLine.RandomLine.AddInfo.TCI_InstrumentNo, testCusLine.TCI_InstrumentNo);
		}

		public void TestTI2_InstrumentNo()
		{
			AssertEquals("TI2_InstrumentNo", testCusLine.RandomLine.AddInfo.TI2_InstrumentNo, testCusLine.TI2_InstrumentNo);
		}

		public void TestPRI_InstrumentNo()
		{
			AssertEquals("PRI_InstrumentNo", testCusLine.RandomLine.AddInfo.PRI_InstrumentNo, testCusLine.PRI_InstrumentNo);
		}

		public void TestActionCodeForMessage()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];

			AssertEquals("Insert", LineAction.Insert, entryLine.ActionCodeForMessage);

			Factory.Save();
			entryHeader.CH_HighestLineNumber = 1;
			AssertEquals("Amend", LineAction.Amend, entryLine.ActionCodeForMessage);
		}

		#region AQIS Collections

		public void TestAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPermitId decProducer = declaration.AQISPermitIds.AddNew();
			decProducer.Code = "Dec";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPermitIds.Count);
			AssertEquals("Code", "Dec", entryLine.OrderedAQISPermitIds[0].Code);

			AQISPermitId headerProducer = invoiceHeader.AQISPermitIds.AddNew();
			headerProducer.Code = "Inv";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPermitIds.Count);
			AssertEquals("Code", "Inv", entryLine.OrderedAQISPermitIds[0].Code);

			AQISPermitId invoiceProducer = invoiceLine.AQISPermitIds.AddNew();
			invoiceProducer.Code = "Line";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPermitIds.Count);
			AssertEquals("Code", "Line", entryLine.OrderedAQISPermitIds[0].Code);
		}

		public void TestOrderedAQISPermitIDs()
		{
			AssertOrderedAQISCollection("AQISPermitIds", "OrderedAQISPermitIds");
		}

		public void TestAQISProducerCodes()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISProducerCode decProducer = declaration.AQISProducerCodes.AddNew();
			decProducer.Code = "Dec";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISProducerCodes.Count);
			AssertEquals("Code", "Dec", entryLine.OrderedAQISProducerCodes[0].Code);

			AQISProducerCode headerProducer = invoiceHeader.AQISProducerCodes.AddNew();
			headerProducer.Code = "Inv";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISProducerCodes.Count);
			AssertEquals("Code", "Inv", entryLine.OrderedAQISProducerCodes[0].Code);

			AQISProducerCode invoiceProducer = invoiceLine.AQISProducerCodes.AddNew();
			invoiceProducer.Code = "Line";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISProducerCodes.Count);
			AssertEquals("Code", "Line", entryLine.OrderedAQISProducerCodes[0].Code);
		}

		public void TestOrderedAQISProducerCodes()
		{
			AssertOrderedAQISCollection("AQISProducerCodes", "OrderedAQISProducerCodes");
		}

		public void TestAQISEntityIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISEntityId decEntity = declaration.AQISEntityIds.AddNew();
			decEntity.Code = "Dec";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISEntityIds.Count);
			AssertEquals("Code", "Dec", entryLine.OrderedAQISEntityIds[0].Code);

			AQISEntityId headerEntity = invoiceHeader.AQISEntityIds.AddNew();
			headerEntity.Code = "Inv";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISEntityIds.Count);
			AssertEquals("Code", "Inv", entryLine.OrderedAQISEntityIds[0].Code);

			AQISEntityId invoiceEntity = invoiceLine.AQISEntityIds.AddNew();
			invoiceEntity.Code = "Line";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISEntityIds.Count);
			AssertEquals("Code", "Line", entryLine.OrderedAQISEntityIds[0].Code);
		}

		public void TestOrderedAQISEntityIDs()
		{
			AssertOrderedAQISCollection("AQISEntityIds", "OrderedAQISEntityIds");
		}

		public void TestAQISCommodityCodes()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISCommodityCode decComm = declaration.AQISCommodityCodes.AddNew();
			decComm.Code = "Dec";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISCommodityCodes.Count);
			AssertEquals("Code", "Dec", entryLine.OrderedAQISCommodityCodes[0].Code);

			AQISCommodityCode headerComm = invoiceHeader.AQISCommodityCodes.AddNew();
			headerComm.Code = "Inv";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISCommodityCodes.Count);
			AssertEquals("Code", "Inv", entryLine.OrderedAQISCommodityCodes[0].Code);

			AQISCommodityCode invoiceComm = invoiceLine.AQISCommodityCodes.AddNew();
			invoiceComm.Code = "Line";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISCommodityCodes.Count);
			AssertEquals("Code", "Line", entryLine.OrderedAQISCommodityCodes[0].Code);
		}

		public void TestOrderedAQISCommodityCodes()
		{
			AssertOrderedAQISCollection("AQISCommodityCodes", "OrderedAQISCommodityCodes");
		}

		public void TestAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPremisesIdAndProcessingType decPremises = declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			decPremises.PremisesId = "PremD";
			decPremises.ProcessingType = "ProcD";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("PremisesId", "PremD", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("ProcessingType", "ProcD", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].ProcessingType);

			AQISPremisesIdAndProcessingType headerPremises = invoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			headerPremises.PremisesId = "PremH";
			headerPremises.ProcessingType = "ProcH";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("PremisesId", "PremH", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("ProcessingType", "ProcH", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].ProcessingType);

			AQISPremisesIdAndProcessingType invoicePremises = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			invoicePremises.PremisesId = "PremI";
			invoicePremises.ProcessingType = "ProcI";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Collection", 1, entryLine.OrderedAQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("PremisesId", "PremI", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("ProcessingType", "ProcI", entryLine.OrderedAQISPremisesIdAndProcessingTypes[0].ProcessingType);
		}

		public void TestOrderedAQISPremisesIDAndProcessingTypes()
		{
			AssertOrderedAQISCollection("AQISPremisesIdAndProcessingTypes", "OrderedAQISPremisesIdAndProcessingTypes");
		}

		public void TestAQISDocuments()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISDocument decDocument = declaration.AQISDocuments.AddNew();
			decDocument.Type = "DD";
			decDocument.Number = "DecNum";
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Documents", 1, entryLine.OrderedAQISDocuments.Count);
			AssertEquals("Document Type", "DD", entryLine.OrderedAQISDocuments[0].Type);
			AssertEquals("Document Num", "DecNum", entryLine.OrderedAQISDocuments[0].Number);

			AQISDocument headerDocument = invoiceHeader.AQISDocuments.AddNew();
			headerDocument.Type = "HH";
			headerDocument.Number = "HeaderNum";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Documents", 1, entryLine.OrderedAQISDocuments.Count);
			AssertEquals("Document Type", "HH", entryLine.OrderedAQISDocuments[0].Type);
			AssertEquals("Document Num", "HeaderNum", entryLine.OrderedAQISDocuments[0].Number);

			AQISDocument invoiceDocument = invoiceLine.AQISDocuments.AddNew();
			invoiceDocument.Type = "II";
			invoiceDocument.Number = "InvNum";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AQIS Documents", 1, entryLine.OrderedAQISDocuments.Count);
			AssertEquals("Document Type", "II", entryLine.OrderedAQISDocuments[0].Type);
			AssertEquals("Document Num", "InvNum", entryLine.OrderedAQISDocuments[0].Number);
		}

		public void TestOrderedAQISDocuments()
		{
			AssertOrderedAQISCollection("AQISDocuments", "OrderedAQISDocuments");
		}

		void AssertOrderedAQISCollection(string invoiceLineCollectionName, string entryLineOrderedCollectionName)
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			BusinessObjectCollection invoiceLineAQISCollection = (BusinessObjectCollection)invoiceLine[invoiceLineCollectionName];

			IAQISUniqueCodeForSort bizO1 = (IAQISUniqueCodeForSort)invoiceLineAQISCollection.AddNew();
			foreach (ZPropertyInfo info in bizO1.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("ZZZ");
			}

			IAQISUniqueCodeForSort bizO2 = (IAQISUniqueCodeForSort)invoiceLineAQISCollection.AddNew();
			foreach (ZPropertyInfo info in bizO2.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("AAA");
			}
			BusinessObjectCollection orderedEntryLineAQIS = (BusinessObjectCollection)entryLine[entryLineOrderedCollectionName];
			IAQISUniqueCodeForSort firstItemAfterSort = null;
			IAQISUniqueCodeForSort secondItemAfterSort = null;

			int index = 1;
			foreach (IAQISUniqueCodeForSort item in orderedEntryLineAQIS)
			{
				if (index == 1)
				{
					firstItemAfterSort = item;
				}
				else
				{
					secondItemAfterSort = item;
				}
				index++;
			}

			AssertEquals("Items should be ordered before used in the message", "AAA", firstItemAfterSort.CodeInfosToSortByForTestingOnly[0].Value);
			AssertEquals("Items should be ordered before used in the message", "ZZZ", secondItemAfterSort.CodeInfosToSortByForTestingOnly[0].Value);
		}

		#endregion

		#endregion

		public void TestAllOtherDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 300m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 400m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 500m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimDumpingDuty, 600m);

			AssertEquals("All other duty for entry line", 1800m, entryLine.AllOtherDuty);
			AssertEquals("Countervailingduty", 100m, entryLine.CountervailingDuty);
			AssertEquals("DumpingDuty", 200m, entryLine.DumpingDuty);
			AssertEquals("InterimAntiDumpingDuty", 400m, entryLine.InterimAntiDumpingDuty);
			AssertEquals("InterimCountervailingDuty", 500m, entryLine.InterimCountervailingDuty);
			AssertEquals("InterimDumpingDuty", 600m, entryLine.InterimDumpingDuty);
		}

		public void TestDeferrableFeesAndCharges()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 200m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 300m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 400m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.InterimDumpingDuty, 500m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 600m);

			AssertEquals("DutyAmount and InterimDumpingDuty are excluded.", 1000m, entryLine.DeferrableFeesAndCharges);
		}

		public void TestPreference()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CMRTariffRatePeriodSnapshot tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_PreferenceSchemeType = "GEN";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);

			invoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("IsGeneralRate", true, invoiceLine.IsGeneralRate);

			DutyDataFromInvoiceLine randomLineDutyData = ((ICMRDutyData)entryLine).RandomLineDutyData;
			AssertEquals("Preference", "GEN", randomLineDutyData.Preference);
		}

		public void TestIsSubjectToDutyOrGSTAndRelatedFlags()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);

			entryLine.CL_CustomsValue = deminimus;
			AssertEquals("Not subject to duty and tax", false, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);

			testDec.JE_MessageType = "EXW";
			AssertEquals("IsNature 30", true, entryLine.IsNature30);
			AssertEquals("subject to duty and tax as this is N30", true, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryLine.CL_CustomsValue = deminimus + 1;
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("subject to duty and tax", true, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);

			testDec.JE_MessageSubType = "SAC";
			AssertEquals("is not subject to duty and tax", false, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);

			testDec.JE_MessageSubType = "FRM";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "352";

			AssertEquals("Security claimed", true, entryLine.IsSecurityClaimed);
			AssertEquals("is not subject to duty and tax", false, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "";

			entryLine.CL_CustomsValue = deminimus + 1;
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("is subject to duty and tax", true, ((ICMRDutyData)entryLine).IsSubjectToDutyAndTax);
		}

		public void TestManualDutyAmountAndInvoiceQuantity()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.AddInfo.ZA_DTY = 100m;
			invoiceLine.JI_InvoiceQuantity = 1M;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.AddInfo.ZA_DTY = 200m;
			invoiceLine2.JI_InvoiceQuantity = 2M;

			AssertEquals("ManualDutyAmount", 300m, ((ICMRDutyData)entryLine).ManualDutyAmount.Amount);
			AssertEquals("ManualDutyAmount", "AUD", ((ICMRDutyData)entryLine).ManualDutyAmount.Currency.Code);

			invoiceLine.AddInfo.ZA_DTY = 0m;
			invoiceLine2.AddInfo.ZA_DTY = 0m;
			AssertEquals("Has Manual duty", 0m, ((ICMRDutyData)entryLine).ManualDutyAmount.Amount);

			AssertEquals("Total Invoice Quantity", 3m, entryLine.InvoiceQuantity);
		}

		public void TestTariffAndStatCodeFormatted()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.JI_Tariff = "0000.00.00 00";
			AssertEquals("EntryLine.TariffAndStatCodeFormatted", "0000.00.00 00", entryLine.TariffAndStatCodeFormatted);
		}

		public void TestIsLCTEAndWETE()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = deminimus + 1;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = deminimus + 1;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			line1.AddInfo.ZA_WETE = "1";
			line1.AddInfo.ZA_LCTI = "Y";
			line1.AddInfo.ZA_LCTE = "FEV";
			line2.AddInfo.ZA_LCTI = "Y";
			line2.AddInfo.ZA_LCTQ = "Y";
			line2.AddInfo.ZA_LCTE = "415";

			DutyDataFromInvoiceLine randomLineData1 = ((ICMRDutyData)entryLine1).RandomLineDutyData;
			DutyDataFromInvoiceLine randomLineData2 = ((ICMRDutyData)entryLine2).RandomLineDutyData;

			AssertEquals("Line1 is WET exempt", true, randomLineData1.IsWETExempt);
			AssertEquals("Line1 is not LCT exempt", false, randomLineData1.IsLCTExempt);
			AssertEquals("Line1 LCTE", "FEV", randomLineData1.LCTE);

			AssertEquals("Line2 is not WET exempt", false, randomLineData2.IsWETExempt);
			AssertEquals("Line2 is LCT exempt", true, randomLineData2.IsLCTExempt);
			AssertEquals("Line2 LCTE", "415", randomLineData2.LCTE);

			randomLineData1 = ((ICMRDutyData)entryLine1).RandomLineDutyData;
			randomLineData2 = ((ICMRDutyData)entryLine2).RandomLineDutyData;

			line2.AddInfo.ZA_LCTE = "";
			line1.AddInfo.ZA_WETE = "";

			line1.AddInfo.ZA_WETQ = "Y";
			line2.AddInfo.ZA_LCTQ = "";

			randomLineData1 = ((ICMRDutyData)entryLine1).RandomLineDutyData;
			randomLineData2 = ((ICMRDutyData)entryLine2).RandomLineDutyData;

			AssertEquals("Line1 is WET exempt", true, randomLineData1.IsWETExempt);
			AssertEquals("Line1 is not LCT exempt", false, randomLineData1.IsLCTExempt);
			AssertEquals("Line1 LCTE", "FEV", randomLineData1.LCTE);

			AssertEquals("Line2 is not WET exempt", false, randomLineData2.IsWETExempt);
			AssertEquals("Line2 is NOT LCT exempt", false, randomLineData2.IsLCTExempt);
			AssertEquals("Line2 LCTE", "", randomLineData2.LCTE);

			AssertEquals("IsSubjectToDutyAndTax", true, ((ICMRDutyData)entryLine1).IsSubjectToDutyAndTax);
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSubjectToDutyAndTax", false, ((ICMRDutyData)entryLine1).IsSubjectToDutyAndTax);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSubjectToDutyAndTax", true, ((ICMRDutyData)entryLine1).IsSubjectToDutyAndTax);

			testDec.JE_MessageSubType = "FRM";
			AssertEquals("IsSACWithoutLines", false, testDec.IsSACWithoutLines);
			line1.AddInfo.ZA_TreatmentCode_Hidden = "351";
			AssertEquals("IsSubjectToDutyAndTax", false, ((ICMRDutyData)entryLine1).IsSubjectToDutyAndTax);
			AssertEquals("IsSubjectToDutyAndTax", true, ((ICMRDutyData)entryLine2).IsSubjectToDutyAndTax);
		}

		public void TestAggregatedGSTEAndPSTUsedForDutyData()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_EFD = "010105";
			invoice.AddInfo.ZA_PST = "GEN";
			invoice.AddInfo.ZA_GSTE = "FOOD";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("RandomLineDutyData's GST Exempt", true, ((ICMRDutyData)entryLine).RandomLineDutyData.IsGSTExempt);
			AssertEquals("RandomLineDutyData's PST Exempt", "GEN", ((ICMRDutyData)entryLine).RandomLineDutyData.Preference);
		}

		public void TestICMRDutyData()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_EFD = "010105";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 200m, "AUD");
			invoiceLine.JI_Tariff = "0000.00.00 00";
			invoiceLine.JI_CustomsUnitQty = "NO";//set this before Customs Qty
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.AddInfo.ZA_CL2 = "11111111";
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			invoiceLine.AddInfo.ZA_QT2 = 30m;
			invoiceLine.AddInfo.ZA_ODF = 40m;
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "000";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "111";
			invoiceLine.AddInfo.ZA_PST = "XX";
			invoiceLine.AddInfo.ZA_RNO = "01";
			invoiceLine.AddInfo.ZA_GSTE = "FOOD";
			invoiceLine.AddInfo.ZA_TRN = "004";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.AddInfo.ZA_TILV = "100AUD";
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.JI_CustomsUnitQty = "NO";//set this before Customs Qty
			invoiceLine2.JI_CustomsQuantity = 25m;
			invoiceLine2.AddInfo.ZA_CL2 = "11111111";
			invoiceLine2.AddInfo.ZA_UQ2 = "LA";
			invoiceLine2.AddInfo.ZA_QT2 = 35m;
			invoiceLine2.AddInfo.ZA_ODF = 45m;
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "000";
			invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden = "111";
			invoiceLine2.AddInfo.ZA_PST = "YY";
			invoiceLine2.AddInfo.ZA_RNO = "01";
			invoiceLine2.AddInfo.ZA_GSTE = "FOOD";
			invoiceLine2.AddInfo.ZA_TRN = "004";

			entryLine.CL_CustomsValue = 2000m;

			AssertEquals("CustomsValue", 2000m, ((ICMRDutyData)entryLine).CustomsValue);
			AssertEquals("FirstQty", 45m, ((ICMRDutyData)entryLine).FirstQty);
			AssertEquals("SecondQty", 65m, ((ICMRDutyData)entryLine).SecondQty);
			AssertEquals("OtherDutyFactor", 85m, ((ICMRDutyData)entryLine).OtherDutyFactor);
			AssertEquals("TransportAndInsuranceInAUD", 300m, ((ICMRDutyData)entryLine).TransportAndInsuranceInAUD);
			entryLine.EntryLineAddInfo.ZA_TILV = "310AUD";
			AssertEquals("TransportAndInsuranceInAUD", 310m, ((ICMRDutyData)entryLine).TransportAndInsuranceInAUD);

			DutyDataFromInvoiceLine randomLineData = ((ICMRDutyData)entryLine).RandomLineDutyData;
			AssertEquals(new ZDateTime(2005, 1, 1), randomLineData.EffectiveDutyDate);
			AssertEquals("00000000", randomLineData.FirstTariffNumber);
			AssertEquals(invoiceLine.AddInfo.ZA_TreatmentCode_Hidden, randomLineData.FirstTreatmentCode);
			AssertEquals(invoiceLine.JI_CustomsUnitQty, randomLineData.FirstUQ);
			AssertEquals(invoiceLine.AddInfo.ZA_PST, randomLineData.Preference);
			AssertEquals(invoiceLine.AddInfo.ZA_RNO, randomLineData.RateNumber);
			AssertEquals(invoiceLine.AddInfo.ZA_CL2, randomLineData.SecondTariffNumber);
			AssertEquals(invoiceLine.AddInfo.ZA_TR2, randomLineData.SecondTreatmentCode);
			AssertEquals(invoiceLine.AddInfo.ZA_UQ2, randomLineData.SecondUQ);
			AssertEquals("GST is exempt", true, randomLineData.IsGSTExempt);
			AssertEquals("Treatment Number", "004", randomLineData.TreatmentRateNumber);
		}

		[TestDate(2005, 1, 1)]
		public void TestICPQALineAttachee()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryLine.Questions.AddNew();
			AssertEquals("FKColumnInCusEntryCPDecTable", CusEntryCPDecSchema.ON_CL, ((ICPQAAttachee)entryLine).FKColumnInCusEntryCPDecTable);
			AssertEquals("SelectionDate", new ZDateTime(2005, 1, 1), ((ICPQAAttachee)entryLine).SelectionDate);
			AssertEquals("Questions", entryLine.Questions[0], entryLine.Questions[0]);
			AssertEquals("TableCode", "CL", ((ICPQALineAttachee)entryLine).TableCode);

			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			invoiceLine.JI_Tariff = "0000.00.00 22";
			invoiceLine.AddInfo.ZA_ORG = "NZ";

			CPQuestionKeys key = ((ICPQALineAttachee)entryLine).CPQuestionKey;
			AssertEquals("Key.TariffNumber", "00000000", key.TariffNumber);
			AssertEquals("Key.StatCode", "22", key.StatCode);
			AssertEquals("Key.OriginCode", "NZ", key.OriginCode);
			AssertEquals("Key.Nature", "N10", key.Nature);
			AssertEquals("Key.ModeOfTransport", "S", key.ModeOfTransport);

			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			key = ((ICPQALineAttachee)entryLine).CPQuestionKey;
			AssertEquals("Key.ModeOfTransport", "A", key.ModeOfTransport);

			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			key = ((ICPQALineAttachee)entryLine).CPQuestionKey;
			AssertEquals("Key.Nature", "N20", key.Nature);

			invoiceLine.AddInfo.ZA_ORG = "";
			invoice.AddInfo.ZA_ORG = "US";
			key = ((ICPQALineAttachee)entryLine).CPQuestionKey;
			AssertEquals("Key.OriginCode", "US", key.OriginCode);

			AssertEquals("hasValidNatureOrMode", true, key.HasValidOriginOrNatureOrModeOfTransport);

			testDec.JE_TransportMode = Core.Constants.TransportModes.Other;
			key = ((ICPQALineAttachee)entryLine).CPQuestionKey;
			AssertEquals("Key.ModeOfTransport", "O", key.ModeOfTransport);
		}

		public void TestRefundReason()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.RefundReasonCode = "AA";
			AssertEquals("Refund reason persisted into AddInfo", "AA", entryLine.EntryLineAddInfo.ZA_RRC_Hidden);
			AssertEquals("Validation is fired", true, entryLine.RefundReasonCodeInfo.HasMessageErrors());
		}

		[TestDate(2005, 1, 10)]
		public void TestIAddInfo()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 2);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("AddInfo", entryLine.EntryLineAddInfo, ((IAddInfo)entryLine).AddInfo);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "US";
			invoice.AddInfo.ZA_PRF = "X";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("AggregatedORG", "US", ((IAggregatedAddInfo)entryLine).AggregatedZA_ORG);
			AssertEquals("AggregatedPRF", "X", ((IAggregatedAddInfo)entryLine).AggregatedZA_PRF);
			AssertEquals("DateOfValuation", testDec.JE_ExportDate, ((IAggregatedAddInfo)entryLine).DateOfValuation);
			AssertEquals("DateOfValuation", new ZDateTime(2005, 1, 10), ((IAggregatedAddInfo)entryLine).EffectiveDutyDate);
		}

		public void TestAddInfo()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("AddInfo", entryLine.EntryLineAddInfo, ((IAddInfo)entryLine).AddInfo);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "US";
			invoice.AddInfo.ZA_PRF = "X";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("ORG", "US", entryLine.ORG);
			AssertEquals("PRF", "X", entryLine.PRF);
		}

		public void TestBondedWarehouseTransactionLine()
		{
			var creator = new Customs.Business.Testing.MergedDeclarationCreator<JobDeclaration>(Factory);
			IWhsBondedWarehouseTransactionLine line = ((IBondedWarehouseTransactionLineProvider)creator.EntryLine1).TransactionLine;
			AssertEquals("Correct type", typeof(BondedWarehouseTransactionLine), line.GetType());
		}

		#region AQIS Package Count And Types

		public void TestAQISPackagesException()
		{
			CusEntryLine entryLine = GetEntryHeaderWithAQISPackages().MergedLines[0];
			AQISPackage line1Package11 = line1.AQISPackages.AddNew();
			line1Package11.Number = 256;
			line1Package11.Type = "TT";
			AssertEquals("Has 11 elements", 11, entryLine.OrderedAQISPackages.Count);

			ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("Expection was reported", "Added more than 10 items to the list", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAQISPackageNumbersAndTypes()
		{
			CusEntryLine entryLine = GetEntryHeaderWithAQISPackages().MergedLines[0];

			AssertEquals("Collection has 10 elements", 10, entryLine.OrderedAQISPackages.Count);

			foreach (AQISPackage currentPackage in entryLine.OrderedAQISPackages)
			{
				bool number = currentPackage.Number == 246 || currentPackage.Number == 912
					|| currentPackage.Number == 450 || currentPackage.Number == 1600
					|| currentPackage.Number == 1000 || currentPackage.Number == 1602
					|| currentPackage.Number == 802 || currentPackage.Number == 45
					|| currentPackage.Number == 25 || currentPackage.Number == 154;

				AssertEquals("Number is in list " + number.ToString(), true, number);

				bool type = currentPackage.Type == "KG" || currentPackage.Type == "BAG"
					|| currentPackage.Type == "BBK" || currentPackage.Type == "BOX"
					|| currentPackage.Type == "CF" || currentPackage.Type == "KEG"
					|| currentPackage.Type == "IN" || currentPackage.Type == "YD"
					|| currentPackage.Type == "M" || currentPackage.Type == "BOT";

				AssertEquals("Type is in list " + type, true, type);
			}
		}

		public void TestAQISPackageCountAndTypesForMergedLines()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			AQISPackage line1Package1 = line1.AQISPackages.AddNew();
			line1Package1.Number = 123;
			line1Package1.Type = "KG";
			AQISPackage line1Package2 = line1.AQISPackages.AddNew();
			line1Package2.Number = 456;
			line1Package2.Type = "BAG";

			JobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;
			AQISPackage line2Package1 = line2.AQISPackages.AddNew();
			line2Package1.Number = 456;
			line2Package1.Type = "BAG";

			JobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0206.29.00 26";
			line3.JI_LinePrice = 10000m;
			AQISPackage line3Package1 = line3.AQISPackages.AddNew();
			line3Package1.Number = 123;
			line3Package1.Type = "KG";

			JobComInvoiceLine line4 = invoice1.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "0206.29.00 26";
			line4.JI_LinePrice = 10000m;
			AQISPackage line4Package1 = line4.AQISPackages.AddNew();
			line4Package1.Number = 456;
			line4Package1.Type = "BAG";
			AQISPackage line4Package2 = line4.AQISPackages.AddNew();
			line4Package2.Number = 123;
			line4Package2.Type = "KG";

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			AssertEquals("PreCondition: One Entry Header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader header = declaration.CustomsEntryHeaders[0];

			AssertEquals("PreCondition: 3 entry lines", 3, header.MergedLines.Count);

			foreach (AQISPackage currentPackage in header.MergedLines[0].OrderedAQISPackages)
			{
				bool number = currentPackage.Number == 246 || currentPackage.Number == 912;
				AssertEquals("Number is in list " + number.ToString(), true, number);

				bool type = currentPackage.Type == "KG" || currentPackage.Type == "BAG";
				AssertEquals("Type is in list " + type, true, type);
			}

			AssertEquals("AQIS Package Count One For Merged Line Two", 456, header.MergedLines[1].OrderedAQISPackages[0].Number);
			AssertEquals("AQIS Package Type One For Merged Line Two", "BAG", header.MergedLines[1].OrderedAQISPackages[0].Type);

			AssertEquals("AQIS Package Count One For Merged Line Two", 123, header.MergedLines[2].OrderedAQISPackages[0].Number);
			AssertEquals("AQIS Package Type One For Merged Line Two", "KG", header.MergedLines[2].OrderedAQISPackages[0].Type);
		}

		public void TestAQISPackagesAreSortedBeforeReturned()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CL = entryLine.PK;

			AQISPackage linePackage1 = line.AQISPackages.AddNew();
			linePackage1.Number = 123;
			linePackage1.Type = "KG";

			AQISPackage linePackage2 = line.AQISPackages.AddNew();
			linePackage2.Number = 456;
			linePackage2.Type = "BAG";

			AQISPackageCollection ordered = entryLine.OrderedAQISPackages;
			AssertEquals("Items in the collection should be ordered to be used in the messages", "BAG", ordered[0].Type);
			AssertEquals("Items in the collection should be ordered to be used in the messages", "KG", ordered[1].Type);
		}

		CusEntryHeader GetEntryHeaderWithAQISPackages()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();

			#region Line 1

			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			AQISPackage line1Package1 = line1.AQISPackages.AddNew();
			line1Package1.Number = 123;
			line1Package1.Type = "KG";

			AQISPackage line1Package2 = line1.AQISPackages.AddNew();
			line1Package2.Number = 456;
			line1Package2.Type = "BAG";

			AQISPackage line1Package3 = line1.AQISPackages.AddNew();
			line1Package3.Number = 150;
			line1Package3.Type = "BBK";

			AQISPackage line1Package4 = line1.AQISPackages.AddNew();
			line1Package4.Number = 800;
			line1Package4.Type = "BOX";

			AQISPackage line1Package5 = line1.AQISPackages.AddNew();
			line1Package5.Number = 800;
			line1Package5.Type = "CF";

			AQISPackage line1Package6 = line1.AQISPackages.AddNew();
			line1Package6.Number = 900;
			line1Package6.Type = "KEG";

			AQISPackage line1Package7 = line1.AQISPackages.AddNew();
			line1Package7.Number = 2;
			line1Package7.Type = "IN";

			AQISPackage line1Package8 = line1.AQISPackages.AddNew();
			line1Package8.Number = 5;
			line1Package8.Type = "YD";

			AQISPackage line1Package9 = line1.AQISPackages.AddNew();
			line1Package9.Number = 2;
			line1Package9.Type = "M";

			AQISPackage line1Package10 = line1.AQISPackages.AddNew();
			line1Package10.Number = 4;
			line1Package10.Type = "BOT";

			#endregion

			#region Line 2

			line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			AQISPackage line2Package1 = line2.AQISPackages.AddNew();
			line2Package1.Number = 300;
			line2Package1.Type = "BBK";

			AQISPackage line2Package2 = line2.AQISPackages.AddNew();
			line2Package2.Number = 702;
			line2Package2.Type = "KEG";

			AQISPackage line2Package3 = line2.AQISPackages.AddNew();
			line2Package3.Number = 150;
			line2Package3.Type = "BOT";

			AQISPackage line2Package4 = line2.AQISPackages.AddNew();
			line2Package4.Number = 40;
			line2Package4.Type = "YD";

			AQISPackage line2Package5 = line2.AQISPackages.AddNew();
			line2Package5.Number = 123;
			line2Package5.Type = "KG";

			AQISPackage line2Package6 = line2.AQISPackages.AddNew();
			line2Package6.Number = 200;
			line2Package6.Type = "CF";

			AQISPackage line2Package7 = line2.AQISPackages.AddNew();
			line2Package7.Number = 800;
			line2Package7.Type = "IN";

			AQISPackage line2Package8 = line2.AQISPackages.AddNew();
			line2Package8.Number = 800;
			line2Package8.Type = "BOX";

			AQISPackage line2Package9 = line2.AQISPackages.AddNew();
			line2Package9.Number = 456;
			line2Package9.Type = "BAG";

			AQISPackage line2Package10 = line2.AQISPackages.AddNew();
			line2Package10.Number = 23;
			line2Package10.Type = "M";

			#endregion

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			AssertEquals("PreCondition: One Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition: One Entry Line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			return declaration.CustomsEntryHeaders[0];
		}

		JobComInvoiceLine line1;
		JobComInvoiceLine line2;

		#endregion

		public void TestIsExciseEquivalentGoods()
		{
			var tariffClassificationCharacteristic29 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic29.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029129";
			tariffClassificationCharacteristic29.TC_CharacteristicCode = 29;
			var tariffClassificationCharacteristic34 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic34.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029134";
			tariffClassificationCharacteristic34.TC_CharacteristicCode = 34;
			var tariffClassificationCharacteristic35 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic35.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029135";
			tariffClassificationCharacteristic35.TC_CharacteristicCode = 35;
			var tariffClassificationCharacteristic36 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic36.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029136";
			tariffClassificationCharacteristic36.TC_CharacteristicCode = 36;
			Factory.Save();

			var testDec = JobDeclaration.New(Factory);
			var line1 = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.AddRange(testDec.InvoiceLines);

			line1.JI_Tariff = "2402.91.29";
			AssertEquals(true, entryLine1.IsExciseEquivalentGoods);
			line1.JI_Tariff = "2402.91.34 02";
			AssertEquals(true, entryLine1.IsExciseEquivalentGoods);
			line1.JI_Tariff = "2402.91.35 03";
			AssertEquals(true, entryLine1.IsExciseEquivalentGoods);
			line1.JI_Tariff = "2402.91.36 04";
			AssertEquals(true, entryLine1.IsExciseEquivalentGoods);
			line1.JI_Tariff = "2402.91.66 00";
			AssertEquals(false, entryLine1.IsExciseEquivalentGoods);
			line1.JI_Tariff = "";
			AssertEquals(false, entryLine1.IsExciseEquivalentGoods);
		}

		public void TestAllEntryFees()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000m;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 4000m;

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 100m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 200m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 300m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 400m);
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 500m);
			AssertEquals("All Entry Fee for EntryLine1", 300m, entryLine1.AllEntryFees);
			AssertEquals("All Entry Fee for EntryLine2", 1200m, entryLine2.AllEntryFees);
		}

		public void TestAQISProcessingCharge()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000m;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 4000m;

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 500m);
			AssertEquals("AQISProcessingCharge Fee for EntryLine1", 100m, entryLine1.AQISProcessingCharge);
			AssertEquals("AQISProcessingCharge Fee for EntryLine2", 400m, entryLine2.AQISProcessingCharge);
		}

		public void TestAQISContainerCharges()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000m;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 4000m;

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 500m);
			AssertEquals("AQISContainerCharges Fee for EntryLine1", 100m, entryLine1.AQISContainerCharges);
			AssertEquals("AQISContainerCharges Fee for EntryLine2", 400m, entryLine2.AQISContainerCharges);
		}

		public void TestTradegateGST()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000m;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 4000m;

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 500m);
			AssertEquals("All TradegateGST Fee for EntryLine1", 100m, entryLine1.TradegateGST);
			AssertEquals("All TradegateGST Fee for EntryLine2", 400m, entryLine2.TradegateGST);
		}

		public void TestTotalCustomsValueAndFactorWithZeroAmountAndAdjustment()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 12);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 150m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 415.89m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 9.03m, "AUD");

			invoice.JZ_InvoiceAmount = 4175.89m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 2241.41m;
			line1.JI_CountryOfOrigin = "NZ";
			line1.JI_Tariff = "4814.90.00 36";
			line1.JI_CustomsQuantity = 223;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "505";
			line1.AddInfo.ZA_PRF = "S";
			line1.AddInfo.ZA_InstrumentType_Hidden = "TC1";
			line1.AddInfo.ZA_InstrumentCode_Hidden = "9007428";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 0m;
			line2.JI_CountryOfOrigin = "NZ";
			line2.JI_Tariff = "9999.32.33 04";
			line2.AddInfo.ZA_PRF = "S";
			line2.AddInfo.ZA_InstrumentType_Hidden = "BL";
			line2.AddInfo.ZA_InstrumentCode_Hidden = "9940008";
			line2.AddInfo.ZA_ADJ = "10AUD";

			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 1934.48m;
			line3.JI_CustomsQuantity = 132;
			line3.JI_CountryOfOrigin = "GB";
			line3.JI_Tariff = "4814.90.00 36";
			line3.AddInfo.ZA_TreatmentCode_Hidden = "505";
			line3.AddInfo.ZA_InstrumentType_Hidden = "TC1";
			line3.AddInfo.ZA_InstrumentCode_Hidden = "9007428";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Three entry line", 3, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			var entryLine1 = Factory.Load<CusEntryLine>(line1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(line2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(line3.JI_CL);

			AssertEquals("Customs value for line1", 1932.83m, entryLine1.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line2", 10m, entryLine2.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line3", 1668.15m, entryLine3.CL_CustomsValue, 0.01m);

			AssertEquals("Total customs value", 3610.98m, entryHeader.CustomsValueInAUD.Amount, 0.01m);
			AssertEquals("Customs factor", 0.86232396m, entryHeader.CustomsFactor);
		}

		public void TestTotalCustomsValueAndGSTAndFactorWithAdjustments()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2006, 3, 12);
			helper.SetExchangeRate(testDec.JE_ExportDate.AddDays(-1), testDec.JE_ExportDate.AddDays(1), 0.763700m, helper.USDCurrency);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 6190.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;

			GroupInvoiceCharge gC1 = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 500.00m, "AUD");
			gC1.J7_IsGSTApplicable = true;
			gC1.J7_DistributeBy = "VAL";
			gC1.J7_PrepaidCollect = "PPD";
			gC1.J7_FullOrPartialApportionment = "PAA";
			GroupInvoiceCharge gC2 = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 20.00m, "AUD");
			gC2.J7_IsGSTApplicable = true;
			gC2.J7_DistributeBy = "VAL";
			gC2.J7_PrepaidCollect = "PPD";
			gC2.J7_FullOrPartialApportionment = "PAA";
			GroupInvoiceCharge gC3 = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 150.00m, "AUD");
			gC3.J7_DistributeBy = "VAL";
			gC3.J7_PrepaidCollect = "PPD";
			gC3.J7_FullOrPartialApportionment = "PAA";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "4814.90.00 36";
			line1.JI_CustomsQuantity = 1;
			line1.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*DMP=1000";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 1010m;
			line2.JI_Tariff = "4814.90.00 36";
			line2.JI_CustomsQuantity = 1;
			line2.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*GSTE=FOOD";

			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 1020m;
			line3.JI_Tariff = "4814.90.00 36";
			line3.JI_CustomsQuantity = 1;
			line3.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*ADJ=100USD";

			JobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_LinePrice = 1030m;
			line4.JI_Tariff = "4814.90.00 36";
			line4.JI_CustomsQuantity = 1;
			line4.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*ADJ=100USD*GSTE=FOOD";

			JobComInvoiceLine line5 = invoice.JobComInvoiceLines.AddNew();
			line5.JI_LinePrice = 0m;
			line5.JI_Tariff = "4814.90.00 36";
			line5.JI_CustomsQuantity = 1;
			line5.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*ADJ=220USD";

			JobComInvoiceLine line6 = invoice.JobComInvoiceLines.AddNew();
			line6.JI_LinePrice = 0m;
			line6.JI_Tariff = "4814.90.00 36";
			line6.JI_CustomsQuantity = 1;
			line6.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*ADJ=200USD*GSTE=FOOD";

			JobComInvoiceLine line7 = invoice.JobComInvoiceLines.AddNew();
			line7.JI_LinePrice = 1060m;
			line7.JI_Tariff = "4814.90.00 36";
			line7.JI_CustomsQuantity = 1;
			line7.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*ADJ=-1060USD";

			JobComInvoiceLine line8 = invoice.JobComInvoiceLines.AddNew();
			line8.JI_LinePrice = 1070m;
			line8.JI_Tariff = "4814.90.00 36";
			line8.JI_CustomsQuantity = 1;
			line8.AddInfo.AddInfoLine = "ORG=US*PST=GEN*RNO=001*GSTE=FOOD";
			line8.AddInfo.ZA_ADJ = "-1070USD";//TO make apportionment dirty

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Eight entry line", 8, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			var entryLine1 = Factory.Load<CusEntryLine>(line1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(line2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(line3.JI_CL);
			var entryLine4 = Factory.Load<CusEntryLine>(line4.JI_CL);
			var entryLine5 = Factory.Load<CusEntryLine>(line5.JI_CL);
			var entryLine6 = Factory.Load<CusEntryLine>(line6.JI_CL);
			var entryLine7 = Factory.Load<CusEntryLine>(line7.JI_CL);
			var entryLine8 = Factory.Load<CusEntryLine>(line8.JI_CL);

			CombineAssertions(() =>
			{
				AssertEquals("Customs value for line1", 891.76m, entryLine1.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line2", 900.69m, entryLine2.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line3", 1040.53m, entryLine3.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line4", 1049.45m, entryLine4.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line5", 288.07m, entryLine5.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line6", 261.88m, entryLine6.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line7", 0m, entryLine7.CL_CustomsValue, 0.01m);
				AssertEquals("Customs value for line8", 0m, entryLine8.CL_CustomsValue, 0.01m);

				AssertEquals("Customs duty for line1", 44.58m, entryLine1.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line2", 45.03m, entryLine2.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line3", 52.02m, entryLine3.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line4", 52.47m, entryLine4.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line5", 14.40m, entryLine5.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line6", 13.09m, entryLine6.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line7", 0m, entryLine7.DutyAmount, 0.01m);
				AssertEquals("Customs duty for line8", 0m, entryLine8.DutyAmount, 0.01m);

				AssertEquals("TILV for line1", 104.62m, entryLine1.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line2", 105.67m, entryLine2.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line3", 122.07m, entryLine3.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line4", 123.12m, entryLine4.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line5", 33.80m, entryLine5.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line6", 30.72m, entryLine6.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line7", 0m, entryLine7.EntryLineAddInfo.TILVInAUD, 0.01m);
				AssertEquals("TILV for line8", 0m, entryLine8.EntryLineAddInfo.TILVInAUD, 0.01m);

				AssertEquals("VOTI for line1", 2040.96m, entryLine1.VOTI, 0.01m);
				AssertEquals("VOTI for line2", 1051.39m, entryLine2.VOTI, 0.01m);
				AssertEquals("VOTI for line3", 1214.62m, entryLine3.VOTI, 0.01m);
				AssertEquals("VOTI for line4", 1225.04m, entryLine4.VOTI, 0.01m);
				AssertEquals("VOTI for line5", 336.27m, entryLine5.VOTI, 0.01m);
				AssertEquals("VOTI for line6", 305.69m, entryLine6.VOTI, 0.01m);
				AssertEquals("VOTI for line7", 0m, entryLine7.VOTI, 0.01m);
				AssertEquals("VOTI for line8", 0m, entryLine8.VOTI, 0.01m);

				AssertEquals("GST for line1", 204.09m, entryLine1.GSTVATAmount, 0.01m);
				AssertEquals("GST for line2", 0m, entryLine2.GSTVATAmount, 0.01m);
				AssertEquals("GST for line3", 121.46m, entryLine3.GSTVATAmount, 0.01m);
				AssertEquals("GST for line4", 0m, entryLine4.GSTVATAmount, 0.01m);
				AssertEquals("GST for line5", 33.61m, entryLine5.GSTVATAmount, 0.01m);
				AssertEquals("GST for line6", 0m, entryLine6.GSTVATAmount, 0.01m);
				AssertEquals("GST for line7", 0m, entryLine7.GSTVATAmount, 0.01m);
				AssertEquals("GST for line8", 0m, entryLine8.GSTVATAmount, 0.01m);

				AssertEquals("Total customs value", 4432.38m, entryHeader.CustomsValueInAUD.Amount, 0.01m);
			});
		}

		public void TestNoChargesIfIncotermInclusiveCharges_UFB()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.UnpackedFreeOnBoard);

			AssertEquals("Packing Costs is OK to send", 10m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is inclusive in UFB", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_PAF()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.PackedAtFactory);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is OK to send", 50m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_UAF()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.UnpackedAtFactory);

			AssertEquals("Packing Costs is OK to send", 10m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 50m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_FOB()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.FreeOnBoard);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_CIP()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_CFR()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.CostAndFreight, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_UCI()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight);

			AssertEquals("Packing Costs is OK to send", 10m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_UCF()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.UnpackedCostAndFreight);

			AssertEquals("Packing Costs is OK to send", 10m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_CIF()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.CostInsuranceAndFreight);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_LIS()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.LandedIntoStore);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestNoChargesIfIncotermInclusiveCharges_DDU()
		{
			CusEntryLine entryLine = CreateEntryForMessageCharges(Core.Constants.IncoTerms.DeliveredAtPlace, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);

			AssertEquals("Packing Costs is included", 0m, entryLine.PackingCosts.Amount);
			AssertEquals("FIFT is included", 0m, entryLine.ForeignInlandFreight.Amount);
			AssertEquals("Overseas Freight is OK to send", 30m, entryLine.OverseasFreight.Amount);
			AssertEquals("Overseas Insurance is OK to send", 40m, entryLine.OverseasInsurance.Amount);
		}

		public void TestLoadedInvoiceLinesInvoiceHeaderHasApportionedCharges()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.CreateEntryFromDeliveranceN20S();

			JobDeclaration declaration = helper.Declaration;
			declaration.ResumeApportionment();
			Assert("Group Header charge exists", declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].GroupCharges.Count > 0);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			loadedDeclaration.DoMerge();
			CusEntryHeader entryHeader = loadedDeclaration.CustomsEntryHeaders[0];
			JobComInvoiceLine invoiceLineFromCusEntry = entryHeader.MergedLines[0].InvoiceLines[0];
			Assert("Invoice Header should have an apportioned charge", invoiceLineFromCusEntry.InvoiceHeader.GroupCharges.Count > 0);
		}

		public void TestMergeLineForeignKeySet()
		{
			JobDeclaration declaration = GetImportDec();
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceAmount = 3020.23m;
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_Description = "Test Description";
			declaration.DoMerge();

			ZGuid expected = declaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			AssertEquals("FK to CusEntryLine on JobComInvoiceLine", expected, line.JI_CL);
		}

		public void TestGettingIDPValueFromAddInfo()
		{
			testInvoiceLine.AddInfo.AddInfoLine = "IDP=1234";
			testCusLine.MergeInvoiceLine(testInvoiceLine);
			AssertEquals("IDP value", 1234m, testCusLine.InterimAntiDumpingDuty);
		}

		public void TestMergeChargesStandardDuty()
		{
			JobDeclaration declaration = GetImportDec();
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_STD = 1000;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_STD = 1000;

			declaration.DoMerge();
			CusEntryLine testCusLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Added STD", 2000m, testCusLine.StandardDuty.Amount);
			Factory.Save();
			BusinessObjectFactory f2 = new BusinessObjectFactory();
			CusEntryHeader entryHeader = f2.Load<CusEntryHeader>(declaration.CustomsEntryHeaders[0].PK);
			AssertEquals(testCusLine.StandardDuty.Amount, entryHeader.MergedLines[0].StandardDuty.Amount);
		}

		public void TestWarehouseRelatedLine()
		{
			JobDeclaration declaration = GetImportDec();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader header = declaration.Invoices[0];
			header.AddInfo.ZA_WRN = "123456";
			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_WRL = 23;
			declaration.DoMerge();

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Warehouse Related line", 23, entryLine.WarehouseRelatedLine);
		}

		public void TestCPDecQuestionsAnswersAvailableFromImporter()
		{
			ZDateTime currentDate = ZDateTime.Today;
			string sQLCommand = "DELETE FROM RefDbCmrAU_CMRLodgementQuestion";
			Db.Connection.ExecuteNonQuery(sQLCommand);// Need to delete data from a table in CMR Refernce files for testing purposes

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;
			org.OH_RL_NKClosestPort = "AUSYD";

			CMRCusEntryCPDec cPDec = Factory.New<CMRCusEntryCPDec>();
			cPDec.ON_CPDecNum = 400;
			cPDec.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec.ON_AnswerCode = "Y";
			cPDec.ON_ParentID = org.PK;
			cPDec.ON_Permit = "PERMIT";

			CMRLodgementQuestion lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 400;
			lodgementQuestion.CQ_LodgementQuestionType = "CPQ";
			lodgementQuestion.CQ_LodgementQuestionText = "Question to be asked";
			lodgementQuestion.CQ_LodgementQuestionStartDate = currentDate.AddDays(-1000);

			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "84716000";
			profile.CP_StatisticalClassificationCodefield = "55";
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			profile.CP_LineNatureTypefield = "N10";

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 500;
			risk.CK_StartDate = currentDate.AddDays(-1000);
			risk.CK_EndDate = currentDate.AddDays(1000);
			risk.CK_PermitApplicationIndicator = false;
			risk.CK_LodgementQuestionIdentifier = 400;

			JobDeclaration dec = GetImportDec();
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dec.JE_OH_Importer = org.PK;

			JobComInvoiceLine line1 = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8471.60.00 55";

			Factory.Save();

			dec.DoMerge();
			Assert("Precondition", dec.CustomsEntryHeaders.Count > 0 && dec.CustomsEntryHeaders[0].MergedLines.Count > 0);
			Assert("Precondition", dec.CustomsEntryHeaders[0].MergedLines[0].Questions.Count > 0);
			AssertEquals("Permit Number should have been defaulted", "PERMIT", dec.CustomsEntryHeaders[0].MergedLines[0].Questions[0].ON_Permit);
			AssertEquals("Answer Code should have been defaulted", CMRCusEntryCPDec.Answers.YES, dec.CustomsEntryHeaders[0].MergedLines[0].Questions[0].ON_AnswerCode);
		}

		public void TestIsRiskCalculatedFromTariff()
		{
			JobDeclaration dec = GetImportDec();
			CusEntryLine entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			AssertEquals("IsRiskCalculatedFromTariff", true, ((ICPQALineAttachee)entryLine).IsRiskCalculatedFromTariff);
		}

		public void TestIsRiskHistorySupported()
		{
			JobDeclaration dec = GetImportDec();
			CusEntryLine entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			AssertEquals("IsRiskHistorySupported", false, ((ICPQALineAttachee)entryLine).IsRiskHistorySupported);
		}

		public void TestCPDecQuestionsAnswerDefaulting()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ZDateTime currentDate = ZDateTime.Today;
			string sQLCommand = "DELETE FROM RefDbCmrAU_CMRLodgementQuestion";
			Db.Connection.ExecuteNonQuery(sQLCommand);// Need to delete data from a table in CMR Refernce files for testing purposes

			CMRLodgementQuestion lodgementQuestion = CMRLodgementQuestion.New(factory2);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 400;
			lodgementQuestion.CQ_LodgementQuestionType = "CPQ";
			lodgementQuestion.CQ_LodgementQuestionText = "Question to be asked";
			lodgementQuestion.CQ_LodgementQuestionStartDate = currentDate.AddDays(-1000);

			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(factory2);
			profile.CP_TariffClassificationNumberfield = "84716000";
			profile.CP_StatisticalClassificationCodefield = "55";
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			profile.CP_LineNatureTypefield = "N10";

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(factory2);
			risk.CK_Identifier = 500;
			risk.CK_StartDate = currentDate.AddDays(-1000);
			risk.CK_EndDate = currentDate.AddDays(1000);
			risk.CK_PermitApplicationIndicator = false;
			risk.CK_LodgementQuestionIdentifier = 400;

			var org = factory2.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;
			org.OH_RL_NKClosestPort = "AUSYD";
			ZGuid orgPK = org.PK;

			CMRCusEntryCPDec cPDec1 = factory2.New<CMRCusEntryCPDec>();
			cPDec1.ON_CPDecNum = 400;
			cPDec1.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec1.ON_ParentID = org.PK;
			cPDec1.ON_ParentTableCode = "OH";
			cPDec1.ON_CPDecVersion = 0;
			ZGuid cPDec1PK = cPDec1.PK;

			Classification classification = factory2.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_TariffNum = "8471.60.00 55";
			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			classification.CC_IsActive = true;
			classification.CC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			ZGuid classificationPK = classification.PK;

			CMRCusEntryCPDec cPDec2 = factory2.New<CMRCusEntryCPDec>();
			cPDec2.ON_CPDecNum = 400;
			cPDec2.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec2.ON_ParentID = classification.PK;
			cPDec2.ON_ParentTableCode = "CC";
			cPDec2.ON_CPDecVersion = 0;
			ZGuid cPDec2PK = cPDec2.PK;

			AUOrgSupplierPart product = factory2.New<AUOrgSupplierPart>();
			product.OP_PartNum = "PART";
			product.OP_Desc = "PART DESCRIPTION";
			product.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			product.AddNewImportPivotWithClassification(classification.PK);
			product.OP_IsActive = true;

			CMRCusEntryCPDec cPDec3 = factory2.New<CMRCusEntryCPDec>();
			cPDec3.ON_CPDecNum = 400;
			cPDec3.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec3.ON_ParentID = product.PivotsForBinding[0].PK;
			cPDec3.ON_ParentTableCode = "CI";
			cPDec3.ON_CPDecVersion = 0;
			ZGuid cPDec3PK = cPDec3.PK;

			factory2.Save();

			OrgHeader testOrg = Factory.Load<OrgHeader>(orgPK);
			Classification testClassification = Factory.Load<Classification>(classificationPK);
			CMRCusEntryCPDec testCPDec1 = Factory.Load<CMRCusEntryCPDec>(cPDec1PK);
			CMRCusEntryCPDec testCPDec2 = Factory.Load<CMRCusEntryCPDec>(cPDec2PK);
			CMRCusEntryCPDec testCPDec3 = Factory.Load<CMRCusEntryCPDec>(cPDec3PK);

			RunOneCPDefaultingTest("Y", "Y", "Y", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "Y", "Y", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "Y", "N", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "Y", "N", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "N", "Y", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "N", "Y", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "N", "N", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "N", "N", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "", "Y", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "", "Y", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "", "N", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "", "N", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "Y", "", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "Y", "", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "N", "", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "N", "", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "Y", "Y", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "Y", "N", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "N", "Y", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "N", "N", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "", "", "Y", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "", "", "N", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "", "Y", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "", "N", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("Y", "Y", "", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("N", "N", "", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
			RunOneCPDefaultingTest("", "", "", "", testCPDec1, testCPDec2, testCPDec3, testOrg, testClassification);
		}

		public void RunOneCPDefaultingTest(ZString expectedDefault, ZString cPDec1Default, ZString cPDec2Default, ZString cPDec3Default,
			CMRCusEntryCPDec cPDec1, CMRCusEntryCPDec cPDec2, CMRCusEntryCPDec cPDec3, OrgHeader org, Classification classification)
		{
			cPDec1.ON_AnswerCode = cPDec1Default;
			cPDec2.ON_AnswerCode = cPDec2Default;
			cPDec3.ON_AnswerCode = cPDec3Default;

			JobDeclaration dec = GetImportDec();
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dec.JE_OH_Importer = org.PK;

			JobComInvoiceLine line1 = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_PartNo = "PART";
			line1.JI_CC = classification.PK;
			line1.JI_Tariff = "8471.60.00 55";

			Factory.Save();
			dec.DoMerge();
			AssertEquals(expectedDefault, dec.CustomsEntryHeaders[0].MergedLines[0].Questions[0].ON_AnswerCode);
		}

		public void TestGST()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			helper.PopulateMergedDutiableDeclaration(new ZDateTime(2003, 11, 4), new ZDateTime(2003, 11, 10), "02032900", "30", 1000M);
			JobDeclaration declaration = helper.Declaration;
			//Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency = ZTestHelper.AUDCurrency.RX_Code;//can't set at this point
			//Declaration.DoMerge(); already merged //Do merge doesn;t throw away the previous merging result. It is up to actions that initiate merge
			AssertEquals("Precondition of test", "USD", declaration.CustomsEntryHeaders[0].InvoiceTotal.Currency.Code);
			AssertEquals("Precondition of test", 0M, declaration.CustomsEntryHeaders[0].MergedLines[0].DutyAmount);
			AssertEquals("GST Dollar Amount", 71.94M, declaration.CustomsEntryHeaders[0].MergedLines[0].GSTVATAmount);
			AssertEquals("GST Rate", 0.1m, declaration.CustomsEntryHeaders[0].MergedLines[0].GSTRate);
		}

		public void TestLCT()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			ZDateTime dTVA = new ZDateTime(2003, 11, 14);
			helper.PopulateDutiableDeclaration(dTVA, dTVA, "8703.33.19", "24", 80000);
			helper.Line1.JI_InvoiceQuantity = 1;
			helper.Line1.JI_InvoiceUQ = "NO";
			helper.Line1.AddInfo.ZA_LCT = 2000;
			helper.Declaration.DoMerge();
			AssertEquals("Luxury car tax on header", 2000m, helper.Declaration.CustomsEntryHeaders[0].MergedLines[0].LCTAmount);
		}

		public void TestEntryFromDeliveranceN10ACustomsFactor()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.CreateEntryFromDeliveranceN10A();

			helper.SetExchangeRate(new ZDateTime(2003, 10, 23), new ZDateTime(2003, 10, 28), 0.7010m);
			JobDeclaration declaration = helper.Declaration;
			declaration.DoMerge();
			AssertEquals("Factor", 1.42653352m, (decimal)declaration.CustomsEntryHeaders[0].CustomsFactor);
		}

		public void TestEntryFromDeliveranceN10A_2CustomsFactor()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.CreateEntryFromDeliveranceN10A_2();
			helper.SetExchangeRate(new ZDateTime(2000, 12, 29), new ZDateTime(2001, 01, 04), 0.5537m);
			JobDeclaration declaration = helper.Declaration;
			declaration.DoMerge();
			AssertEquals("Factor", 1.24973090m, (decimal)declaration.CustomsEntryHeaders[0].CustomsFactor);
		}

		public void TestCIFCustomsValue()
		{
			JobDeclaration declaration = GetImportDec();
			declaration.JE_ExportDate = new ZDateTime(2003, 11, 21);
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 3000, aUDCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 227.43m, aUDCurrency.RX_Code);
			oNS.J7_IsIncludedInITOT = true;

			JobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 94199.52m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			JobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 94199.52m;
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			//TODO : Remove Rounding and fix merge
			AssertEquals("Customs Value", 90972.09m, ZArchitecture.Core.Utilities.Round(entryLine.CL_CustomsValue, 2));
			AssertEquals("GST", 9419.95m, entryLine.GSTVATAmount);
		}

		public void TestExcisableLineWithout444()
		{
			JobDeclaration declaration = GetImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_ExportDate = new ZDateTime(2003, 11, 21);
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 411.34m, aUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 1.21m, aUDCurrency.RX_Code);

			JobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 487m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 40m;
			invoiceLine.JI_InvoiceUQ = "L";
			invoiceLine.JI_Tariff = "3403.99.90 37";
			invoiceLine.JI_LinePrice = 487m;
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals("(Precondition of test) Customs Value", 487m, entryLine.CL_CustomsValue);
		}

		public void TestSimpleCIF()
		{
			var declaration = GetImportDec();
			declaration.JE_ExportDate = new ZDateTime(2003, 11, 21);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			var oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 1627.50m, aUDCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			var oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 13.76m, aUDCurrency.RX_Code);
			oNS.J7_IsIncludedInITOT = true;

			var invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.AddInfo.ZA_PRF = "S";
			invoiceHeader1.AddInfo.ZA_ORG = "NZ";
			invoiceHeader1.JZ_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 12512.24m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			var invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 32.61m;
			invoiceLine.JI_InvoiceUQ = "M3";
			invoiceLine.JI_Tariff = "4407.10.99 11";
			invoiceLine.JI_LinePrice = 12512.24m;
			invoiceLine.AddInfo.ZA_PST = "NZ";
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals("(Precondition of test) Customs Value", 10870.98m, entryLine.CL_CustomsValue);
			AssertEquals("Duty at preference rate", 0m, entryLine.DutyAmount);
			AssertEquals("GST", 1251.22m, entryLine.GSTVATAmount);
		}

		public void TestFOBEntry()
		{
			JobDeclaration declaration = GetImportDec();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_ExportDate = new ZDateTime(2003, 11, 25);
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2003, 11, 24), new ZDateTime(2003, 11, 26), 0.7191m, uSDCurrency);

			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 210.40m, uSDCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = false;
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 1.86m, uSDCurrency.RX_Code);
			oNS.J7_IsIncludedInITOT = false;

			JobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 743.85m;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 45m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_Tariff = "3206.49.00 43";
			invoiceLine.JI_LinePrice = 743.85m;
			invoiceLine.AddInfo.ZA_TILV = "212.26AUD";

			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];

			AssertEquals(1034.41m, entryHeader.CustomsValueInAUD.Amount, 0.05m);//"Customs Value in Header"
			AssertEquals(1034.41m, entryLine.CL_CustomsValue, 0.05m);//"(Precondition of test) Customs Value"
		}

		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0106900069", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "OTHER LIVE ANIMALS EXCL MAMMALS,REPTILES,BIRDS", taxOrFeeCode: "GST");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var @class = Factory.New<Classification>();
				@class.CC_Description = "Classification Description";
				@class.CC_LookupCode = "12345";

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				var header = dec.CustomsEntryHeaders.AddNew();
				var entryLine = (TestHelperCusEntryLine)header.MergedLines.AddNew(typeof(TestHelperCusEntryLine));

				var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var invLine1 = invHeader.JobComInvoiceLines.AddNew();
				var invLine2 = invHeader.JobComInvoiceLines.AddNew();
				var invLine3 = invHeader.JobComInvoiceLines.AddNew();

				invLine1.JI_CC = @class.PK;
				invLine2.JI_CC = @class.PK;
				invLine3.JI_CC = @class.PK;

				invLine1.JI_CL = entryLine.PK;
				invLine2.JI_CL = entryLine.PK;
				invLine3.JI_CL = entryLine.PK;

				invLine1.JI_Description = "Overridden Description";
				invLine2.JI_Description = "Overridden Description";
				invLine3.JI_Description = "Overridden Description";

				AssertEquals("Description", "Overridden Description", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);
				invLine2.JI_Description = "Overridden Description Different from others";
				invLine1.JI_CC = @class.PK;
				invLine2.JI_CC = @class.PK;
				invLine3.JI_CC = @class.PK;

				AssertEquals("Description", "CLASSIFICATION DESCRIPTION", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);

				invLine3.JI_CC = ZGuid.Empty;
				invLine1.JI_Tariff = "0106.90.00 69";
				invLine2.JI_Tariff = "0106.90.00 69";
				invLine3.JI_Tariff = "0106.90.00 69";
				invLine1.JI_Description = "DESCRIPTION1";
				invLine2.JI_Description = "DESCRIPTION2";
				invLine3.JI_Description = "DESCRIPTIONn3";

				AssertEquals("Description", "OTHER LIVE ANIMALS EXCL MAMMALS,REPTILES,BIRDS", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);
			}
		}

		public void TestDescription_AUCClass()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var @class = Factory.New<Classification>();
				@class.CC_Description = "Classification Description";
				@class.CC_LookupCode = "12345";

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				var header = dec.CustomsEntryHeaders.AddNew();
				var entryLine = (TestHelperCusEntryLine)header.MergedLines.AddNew(typeof(TestHelperCusEntryLine));

				var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var invLine1 = invHeader.JobComInvoiceLines.AddNew();
				var invLine2 = invHeader.JobComInvoiceLines.AddNew();
				var invLine3 = invHeader.JobComInvoiceLines.AddNew();

				invLine1.JI_CC = @class.PK;
				invLine2.JI_CC = @class.PK;
				invLine3.JI_CC = @class.PK;

				invLine1.JI_CL = entryLine.PK;
				invLine2.JI_CL = entryLine.PK;
				invLine3.JI_CL = entryLine.PK;

				invLine1.JI_Description = "Overridden Description";
				invLine2.JI_Description = "Overridden Description";
				invLine3.JI_Description = "Overridden Description";

				AssertEquals("Description", "Overridden Description", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);
				invLine2.JI_Description = "Overridden Description Different from others";
				invLine1.JI_CC = @class.PK;
				invLine2.JI_CC = @class.PK;
				invLine3.JI_CC = @class.PK;

				AssertEquals("Description", "CLASSIFICATION DESCRIPTION", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);

				invLine3.JI_CC = ZGuid.Empty;
				invLine1.JI_Tariff = "0106.90.00 69";
				invLine2.JI_Tariff = "0106.90.00 69";
				invLine3.JI_Tariff = "0106.90.00 69";
				invLine1.JI_Description = "DESCRIPTION1";
				invLine2.JI_Description = "DESCRIPTION2";
				invLine3.JI_Description = "DESCRIPTIONn3";

				AssertEquals("Description", "OTHER LIVE ANIMALS EXCL MAMMALS,REPTILES,BIRDS", entryLine.Description);
				AssertDescriptionCalculatorUsed(entryLine);
			}
		}

		public void TestIfMergeByClassificationAlwaysUsingClassificationDescriptionWeUseClassificationDescriptionEvenIfOverriddenByUser()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_Description = "Classification Description";
			@class.CC_LookupCode = "12345";

			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader header = dec.CustomsEntryHeaders.AddNew();
			TestHelperCusEntryLine entryLine = (TestHelperCusEntryLine)header.MergedLines.AddNew(typeof(TestHelperCusEntryLine));

			JobComInvoiceHeader invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine1 = invHeader.JobComInvoiceLines.AddNew();

			invLine1.JI_CC = @class.PK;

			invLine1.JI_CL = entryLine.PK;

			invLine1.JI_Description = "Overridden Description";

			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			AssertEquals("Description", "Overridden Description", entryLine.Description);
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;
			AssertEquals("Description", "CLASSIFICATION DESCRIPTION", entryLine.Description);
		}

		void AssertDescriptionCalculatorUsed(TestHelperCusEntryLine entryLine)
		{
			AssertEquals(entryLine.Declaration, entryLine.LastDescriptionCalculatorUsed.Declaration);
			AssertEquals(entryLine.CommonPart, entryLine.LastDescriptionCalculatorUsed.Part);
			AssertEquals(entryLine.CommonClassification, entryLine.LastDescriptionCalculatorUsed.Class);
			AssertEquals(entryLine.RandomLine.TariffDescription, entryLine.LastDescriptionCalculatorUsed.TariffDescriptionDelegate.Item1());
		}

		#region TestHelper

		class TestHelperCusEntryLine : CusEntryLine
		{
			public TestHelperCusEntryLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override EntryLineDescriptionCalculator GetDescriptionCalculator()
			{
				LastDescriptionCalculatorUsed = base.GetDescriptionCalculator();
				return LastDescriptionCalculatorUsed;
			}

			public EntryLineDescriptionCalculator LastDescriptionCalculatorUsed;
		}

		#endregion

		public void TestZA_QT2()
		{
			testInvoiceLine.AddInfo.ZA_QT2 = 10m;
			AssertEquals("ZA_QT2", 10m, testCusLine.ZA_QT2);
		}

		public void TestLineDefaultsToNormal()
		{
			AssertEquals("CL_PArentTrailer", CusEntryLine.ParentTrailer.Normal, testCusLine.CL_ParentTrailer);
		}

		public void TestNatureTypeForCMR()
		{
			testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("Nature 10", CusEntryHeader.NatureTypesForImportCMR.Nature10, testCusLine.NatureTypeForCMR);
			AssertEquals("Is Nature 10", true, testCusLine.IsNature10);
			AssertEquals("Not Nature 20", false, testCusLine.IsNature20);
			AssertEquals("Not Nature 30", false, testCusLine.IsNature30);

			testInvoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("Nature 20", CusEntryHeader.NatureTypesForImportCMR.Nature20, testCusLine.NatureTypeForCMR);
			AssertEquals("Not Nature 10", false, testCusLine.IsNature10);
			AssertEquals("Is Nature 20", true, testCusLine.IsNature20);
			AssertEquals("Not Nature 30", false, testCusLine.IsNature30);

			testInvoiceLine.JI_IsPackToBondForLine = false;
			testCusLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Nature 30", CusEntryHeader.NatureTypesForImportCMR.Nature30, testCusLine.NatureTypeForCMR);
			AssertEquals("Not Nature 10", false, testCusLine.IsNature10);
			AssertEquals("Not Nature 20", false, testCusLine.IsNature20);
			AssertEquals("Is Nature 30", true, testCusLine.IsNature30);
		}

		public void TestDutyAndTax()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 20m);
			AssertEquals("TotalDutyAndTaxAdvisedInLastClearanceMessage", 100m, entryLine.TotalDutyTaxAdvisedInLastClearanceMessage);
			AssertEquals("Current Duty and tax", 30m, entryLine.CurrentTotalDutyTax);
			AssertEquals("IsLessDutyAndTax", true, entryLine.IsLessDutyAndTax);

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 70m);
			AssertEquals("Current Duty and tax", 100m, entryLine.CurrentTotalDutyTax);
			AssertEquals("IsLessDutyAndTax", false, entryLine.IsLessDutyAndTax);
		}

		public void TestValidation()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Validation type", typeof(CusEntryLineValidation), entryLine.Validation.GetType());
		}

		public void TestIDrawbackEntryLineMembers()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = (IDrawbackEntryLine)entryHeader.AllEntryLines.AddNew();
			AssertEquals(ZDateTime.Empty, entryLine.DeclarationDate);
			AssertEquals(ZString.Empty, entryLine.EntryNumber);
			var date1 = ZDateTime.Today.AddDays(-1);
			testDec.ManualClearanceDate = date1;
			entryHeader.EntryNumber = "1";
			AssertEquals("manual clearance date returned", date1, entryLine.DeclarationDate);
			AssertEquals("entry number returned", "1", entryLine.EntryNumber);
		}

		#region Implementation

		protected JobDeclaration GetImportDec()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			return declaration;
		}
		CusEntryLine testCusLine;
		JobComInvoiceLine testInvoiceLine;
		JobComInvoiceHeader testInvoiceHeader;

		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

			ZTestHelper helper = new ZTestHelper(Factory);
			helper.CreateMockInvoiceForCPDecQuestions();
			JobDeclaration declaration = helper.Declaration;

			declaration.CustomsEntryHeaders.AddNew();
			testCusLine = declaration.CustomsEntryHeaders[0].MergedLines.AddNew();
			testInvoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			testInvoiceLine = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testInvoiceLine.JI_CL = testCusLine.PK;
			TaxOrFeeTestHelper.SetUp();
		}

		protected CusEntryLine CreateEntryForMessageCharges(string incoTerm, string applicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (declaration.GetValidationSuspender())
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = applicationCode;
				JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();

				invoiceHeader.JZ_IncoTerm = incoTerm;
				invoiceHeader.JZ_InvoiceAmount = 1000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

				BaseJobComInvHeaderCharge pC = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 10, aUDCurrency.RX_Code);
				pC.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, pC.ChargeCode);

				BaseJobComInvHeaderCharge oFT = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 30, aUDCurrency.RX_Code);
				oFT.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, oFT.ChargeCode);

				BaseJobComInvHeaderCharge oNS = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 40, aUDCurrency.RX_Code);
				oNS.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, oNS.ChargeCode);

				BaseJobComInvHeaderCharge fIFT = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50, aUDCurrency.RX_Code);
				fIFT.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, fIFT.ChargeCode);

				BaseJobComInvHeaderCharge oTH = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 80, aUDCurrency.RX_Code);
				oTH.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, oTH.ChargeCode);

				BaseJobComInvHeaderCharge lCH = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 80, aUDCurrency.RX_Code);
				lCH.J7_IsIncludedInITOT = invoiceHeader.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoiceHeader.IncoTerm, lCH.ChargeCode);

				JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;

				LineMerger merger = new LineMerger(declaration);
				merger.DoMerge();
			}

			return declaration.CustomsEntryHeaders[0].MergedLines[0];
		}
		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);
		#endregion
	}

	class EntryLineComparisonBetweenCMRAndLegacyTest : TestCaseWithFactory
	{
		public void TestCharges()
		{
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, JobDeclaration.LocalCurrencyConstantCode);
			BaseJobComInvHeaderCharge lCH = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			lCH.J7_IsIncludedInITOT = true;

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.ResumeApportionment();
			AssertEquals("Discount", 100m, entryLine1.Discount.Amount);
			AssertEquals("ForeignInlandFreight", 100m, entryLine1.ForeignInlandFreight.Amount);
			AssertEquals("OTH + ADD", 200m, entryLine1.OtherCharge1.Amount);
			AssertEquals("PackingCost", 100m, entryLine1.PackingCosts.Amount);

			AssertEquals("OverseasFreight", 100m, entryLine2.OverseasFreight.Amount);
			AssertEquals("OverseasInsurance", 100m, entryLine2.OverseasInsurance.Amount);
			AssertEquals("Deduction Charge", 100m, entryLine2.OtherCharge2.Amount);
			AssertEquals("LandingCharges", 100m, entryLine2.LandingCharge.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			AssertEquals("Discount", 0m, entryLine1.Discount.Amount);
			AssertEquals("ForeignInlandFreight", 0m, entryLine1.ForeignInlandFreight.Amount);
			AssertEquals("OTH + ADD", 0m, entryLine1.OtherCharge1.Amount);
			AssertEquals("PackingCost", 0m, entryLine1.PackingCosts.Amount);

			AssertEquals("OverseasFreight", 100m, entryLine2.OverseasFreight.Amount);
			AssertEquals("OverseasInsurance", 100m, entryLine2.OverseasInsurance.Amount);
			AssertEquals("Deduction Charge", 0m, entryLine2.OtherCharge2.Amount);
			AssertEquals("LandingCharges", 0m, entryLine2.LandingCharge.Amount);
		}

		public void TestTransportAndInsurance()
		{
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.ResumeApportionment();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TransportAndInsurance", 200m, entryLine2.TransportAndInsuranceInLocalCurrency.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TransportAndInsurance", 200m, entryLine2.TransportAndInsuranceInLocalCurrency.Amount);
		}

		public void TestTransportAndInsuranceUnitValueInLocalCurrency_CMR()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			line1.JI_CustomsQuantity = 2m;
			line2.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_WRQ = 0m;
			line2.AddInfo.ZA_WRQ = 0m;
			testDec.ResumeApportionment();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceUnitValueInLocalCurrency);
			AssertEquals("TransportAndInsurance", 100m, entryLine2.TransportAndInsuranceUnitValueInLocalCurrency);

			line1.AddInfo.ZA_WRQ = 4m;
			line2.AddInfo.ZA_WRQ = 4m;
			testDec.ResumeApportionment();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceUnitValueInLocalCurrency);
			AssertEquals("TransportAndInsurance", 50m, entryLine2.TransportAndInsuranceUnitValueInLocalCurrency);
		}

		public void TestTransportAndInsuranceUnitValueInLocalCurrency_Legacy()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			line1.JI_CustomsQuantity = 2m;
			line2.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_WRQ = 0m;
			line2.AddInfo.ZA_WRQ = 0m;
			testDec.ResumeApportionment();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceUnitValueInLocalCurrency);
			AssertEquals("TransportAndInsurance", 100m, entryLine2.TransportAndInsuranceUnitValueInLocalCurrency);

			line1.AddInfo.ZA_WRQ = 4m;
			line2.AddInfo.ZA_WRQ = 4m;
			testDec.ResumeApportionment();
			AssertEquals("TransportAndInsurance", 0m, entryLine1.TransportAndInsuranceUnitValueInLocalCurrency);
			AssertEquals("TransportAndInsurance", 50m, entryLine2.TransportAndInsuranceUnitValueInLocalCurrency);
		}

		public void TestPrice()
		{
			InvoiceCharge lCH = invoice2.Charges.AddNew();
			lCH.J7_ChargeType = AUChargeCodeList.Codes.LandingCharges;
			lCH.J7_Amount = 200m;
			lCH.J7_RX_NKCurrency = "AUD";
			lCH.J7_IsIncludedInITOT = true;

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Invoice total", 10000m, entryLine1.Price.Amount);
			AssertEquals("Invoice total", 10000m, entryLine2.Price.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			testDec.ResumeApportionment();
			AssertEquals("Invoice Total", 10000m, entryLine1.Price.Amount);
			AssertEquals("Invoice Total", 9800m, entryLine2.Price.Amount);
		}

		#region Implementation

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine1;
		CusEntryLine entryLine2;

		JobComInvoiceHeader invoice1;
		JobComInvoiceLine line1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceLine line2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine2 = entryHeader.MergedLines.AddNew();

			invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;
			line1.JI_CL = entryLine1.PK;

			invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;
			line2.JI_CL = entryLine2.PK;

			TaxOrFeeTestHelper.SetUp();
		}

		#endregion
	}
}
