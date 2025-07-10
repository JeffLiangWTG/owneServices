using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using CusEntryHeaderTest = Enterprise.Customs.AU.Declaration.Business.Testing.CusEntryHeaderTest;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		#region ZDecimal Fields

		public void TestFormattedBillNumbers()
		{
			SetJobDeclarationForATDTesting();
			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			EntryHeaderInternal.CH_JE = Declaration.PK;
			AssertEquals(ZString.Empty, newWrapper.FormattedBillNumbers);

			Bill bill1 = Declaration.Bills.AddNew();
			bill1.CU_BillType = "MB";
			bill1.CU_BillNum = "MBL1";

			Bill bill2 = Declaration.Bills.AddNew();
			bill2.CU_BillType = "MB";
			bill2.CU_BillNum = "MBL2";

			Bill bill3 = Declaration.Bills.AddNew();
			bill3.CU_BillType = "MB";
			bill3.CU_BillNum = "HBL1";
			bill3.CU_CU_ParentBill = bill2.PK;

			var billsCollection = new Enterprise.Customs.Business.BillCollectionForEntry(EntryHeaderInternal);
			billsCollection.PopulateBills();
			newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("HBL1,MBL1/M   ", newWrapper.FormattedBillNumbers);

			Bill bill4 = Declaration.Bills.AddNew();
			bill4.CU_BillType = "HB";
			bill4.CU_HouseBill = "HBL1";

			Bill bill5 = Declaration.Bills.AddNew();
			bill5.CU_BillType = "HB";
			bill5.CU_HouseBill = "HBL2";

			billsCollection = new Enterprise.Customs.Business.BillCollectionForEntry(EntryHeaderInternal);
			billsCollection.PopulateBills();
			newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("HBL1,MBL1/M   HBL1,HBL2/H", newWrapper.FormattedBillNumbers);
		}

		public void TestOtherEntryChargesOtherThanEntryFeeMessageFeeAndLevies()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 10m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 11m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 13m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 14m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 15m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 16m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 17m);

			AssertEquals("Other Entry Charges", 17m, EntryHeaderWrapperInternal.OtherChargesAmount);

			ZDecimal expected = 10m + 11m + 13m + 17m;
			AssertEquals("OtherEntryChargesOtherThanEntryFeeMessageFeeAndLevies", expected, EntryHeaderWrapperInternal.OtherEntryChargesOtherThanEntryFeeMessageFeeAndLevies);
		}

		public void TestCustomsFactor()
		{
			AssertEquals("CustomsFactor", EntryHeaderInternal.CustomsFactor, EntryHeaderWrapperInternal.CustomsFactor);
		}

		public void TestDutyAmount()
		{
			AssertEquals("DutyAmount", EntryHeaderInternal.DutyAmount, EntryHeaderWrapperInternal.DutyAmount);
		}

		public void TestLCTAmount()
		{
			AssertEquals("LCTAmount", EntryHeaderInternal.LCTAmount, EntryHeaderWrapperInternal.LCTAmount);
		}

		public void TestWETAmount()
		{
			AssertEquals("WETAmount", EntryHeaderInternal.WETAmount, EntryHeaderWrapperInternal.WETAmount);
		}

		public void TestTAndI()
		{
			AssertEquals("TAndI", EntryHeaderInternal.TAndI, EntryHeaderWrapperInternal.TAndI);
		}

		public void TestEntryFee_Legacy()
		{
			EntryHeaderInternal.Declaration.JE_MessageType = "IMP";
			EntryHeaderInternal.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 12.32m);
			AssertEquals("EntryFee", 12.32m, EntryHeaderWrapperInternal.EntryFeeAmount);
		}

		public void TestEntryFee_CMR()
		{
			EntryHeaderInternal.Declaration.JE_MessageType = "IMP";
			EntryHeaderInternal.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 12.32m);
			AssertEquals("EntryFee", 12.32m, EntryHeaderWrapperInternal.EntryFeeAmount);
		}

		public void TestMessageFee()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 12.32m);
			AssertEquals("MessageFee", EntryHeaderInternal.MessageFee, EntryHeaderWrapperInternal.MessageFeeAmount);
		}

		public void TestOtherCharges()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 12.32m);
			AssertEquals("OtherCharges", EntryHeaderInternal.OtherEntryCharge, EntryHeaderWrapperInternal.OtherChargesAmount);
		}

		public void TestScreenFreeCharge()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 12.32m);
			AssertEquals("ScreenFreeCharge", EntryHeaderInternal.ScreenFreeCharge, EntryHeaderWrapperInternal.ScreenFreeCharge);
		}

		public void TestTradegateGST()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 12.32m);
			AssertEquals("TradegateGST", EntryHeaderInternal.TradegateGST, EntryHeaderWrapperInternal.TradegateGST);
		}

		public void TestWoodLevy()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 12.32m);
			AssertEquals("WoodLevy", EntryHeaderInternal.WoodLevy, EntryHeaderWrapperInternal.WoodLevy);
		}

		public void TestEntryLeviesCore()
		{
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 10m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 50m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 100m);
			AssertEquals("Entry Levies", 160m, EntryHeaderWrapperInternal.EntryLeviesAmount);
		}

		[TestDate(2005, 12, 30)]
		public void TestAllEntryFees()
		{
			AssertEquals("IsImportCMR", true, EntryHeaderInternal.Declaration.IsImportCMR);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 20m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 10m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 50m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 100m);
			AssertEquals("AllEntryFees", 20m, EntryHeaderWrapperInternal.AllEntryFees);
		}

		public void TestTotalPayableFeesAndCharges()
		{
			CreateEntryHeaderCharges();
			CreateEntryLineFees();
			AssertEquals("Total charges and line fees", 390.5M, EntryHeaderWrapperInternal.TotalPayableFeesAndCharges);
		}

		public void TestAllAQISCharges()
		{
			CreateEntryHeaderCharges();
			CreateEntryLineFees();
			AssertEquals("Total charges and line fees", 200.5M, EntryHeaderWrapperInternal.AllAQISCharges);
		}

		public void TestTotalDeferredDuty()
		{
			var line = EntryHeaderInternal.MergedLines.AddNew();
			var dtyFee = line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 50m);
			AssertEquals("DutyAmount is 50m.", 50m, EntryHeaderWrapperInternal.DutyAmount);
			AssertEquals("It is not deferred yet.", false, EntryHeaderWrapperInternal.IsDutyDeferred);
			AssertEquals("TotalDeferredDuty is zero as it is not deferred.", 0m, EntryHeaderWrapperInternal.TotalDeferredDuty);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUI";
			org.AUIsDutyDeferred = true;
			EntryHeaderInternal.Declaration.JE_OH_Importer = org.PK;
			Factory.Save();

			AssertEquals("The duty is deferred as the consignee is set as IsDutyDeferred.", true, EntryHeaderWrapperInternal.IsDutyDeferred);
			AssertEquals(50m, EntryHeaderWrapperInternal.TotalDeferredDuty);
		}

		public void TestPayableFields()
		{
			var line = EntryHeaderInternal.MergedLines.AddNew();
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 50m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 40m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 30m);
			EntryHeaderInternal.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 60m);
			AssertEquals("It is not deferred yet.", false, EntryHeaderWrapperInternal.IsDutyDeferred);
			AssertEquals("TotalDeferredDuty is zero as it is not deferred.", 0m, EntryHeaderWrapperInternal.TotalDeferredDuty);
			AssertEquals("PayableDuty", 50m, EntryHeaderWrapperInternal.PayableDuty);
			AssertEquals("PayableWET", 40m, EntryHeaderWrapperInternal.PayableWET);
			AssertEquals("PayableLCT", 30m, EntryHeaderWrapperInternal.PayableLCT);
			AssertEquals("PayableOtherCMRCharges", 60m, EntryHeaderWrapperInternal.PayableOtherCMRCharges);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUI";
			org.AUIsDutyDeferred = true;
			EntryHeaderInternal.Declaration.JE_OH_Importer = org.PK;
			Factory.Save();

			AssertEquals("The duty is deferred as the consignee is set as IsDutyDeferred.", true, EntryHeaderWrapperInternal.IsDutyDeferred);
			AssertEquals("TotalDeferredDuty", 50m, EntryHeaderWrapperInternal.TotalDeferredDuty);
			AssertEquals("PayableDuty", 0m, EntryHeaderWrapperInternal.PayableDuty);
			AssertEquals("PayableWET", 0m, EntryHeaderWrapperInternal.PayableWET);
			AssertEquals("PayableLCT", 0m, EntryHeaderWrapperInternal.PayableLCT);
			AssertEquals("PayableOtherCMRCharges", 0m, EntryHeaderWrapperInternal.PayableOtherCMRCharges);
		}

		#endregion

		#region ZBool Fields

		public void TestIsPrimeEntry()
		{
			AssertEquals("IsPrimeEntry", EntryHeaderInternal.IsPrimeEntry, EntryHeaderWrapperInternal.IsPrimeEntry);
		}

		public void TestIsEnclosureEntry()
		{
			AssertEquals("IsEnclosureEntry", EntryHeaderInternal.IsEnclosureEntry, EntryHeaderWrapperInternal.IsEnclosureEntry);
		}

		public void TestIncludeAQISServicePaymentsInTotalPayableOnEntryPrint()
		{
			AUCustomsDataRegistry.Instance.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SetJobDeclarationForATDTesting();
			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("IsNature10", "Y", newWrapper.IncludeAQISServicePaymentsInTotalPayable.ToString());
			AUCustomsDataRegistry.Instance.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("IsNature10", "N", newWrapper.IncludeAQISServicePaymentsInTotalPayable.ToString());
		}

		#endregion

		#region ZString Fields

		public void TestPortDetails()
		{
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockEntry.Setup(m => m.Nature).Returns(new ZString("N10"));
			CusEntryHeader entry = mockEntry.Object;
			entry.CH_JE = Declaration.PK;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;

			RefUNLOCO aUSYD = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			aUSYD.RL_PortName = "Loading Port";
			Declaration.JE_RL_NKPortOfLoading = aUSYD.RL_Code;
			Declaration.JE_DateOfArrival = new ZDateTime(2016, 12, 20);
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2016, 12, 21);

			RefUNLOCO aUBNE = new RefUNLOCO.Loader(Factory).Load("AUBNE");
			aUBNE.RL_PortName = "Arrival Port ";
			Declaration.JE_RL_NKPortOfArrival = aUBNE.RL_Code;

			RefUNLOCO aUHBA = new RefUNLOCO.Loader(Factory).Load("AUHBA");
			aUHBA.RL_PortName = "Destination Port ";
			Declaration.JE_RL_NKFinalDestination = aUHBA.RL_Code;

			RefUNLOCO aUMEL = new RefUNLOCO.Loader(Factory).Load("AUMEL");
			aUMEL.RL_PortName = "First Arrival Port ";
			Declaration.JE_RL_NKPortOfFirstArrival = aUMEL.RL_Code;

			DocCusEntryHeader entryHeaderWrapper = DocCusEntryHeader.New(entry, Factory);
			AssertEquals("LOADING PORT", entryHeaderWrapper.LoadPort);
			AssertEquals("QLD PARCELS POST   20DEC16", entryHeaderWrapper.ArrivalPort);
			AssertEquals("DESTINATION PORT", entryHeaderWrapper.DestlPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper.DschlPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper.FirstPort);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			entryHeaderWrapper = DocCusEntryHeader.New(entry, Factory);

			AssertEquals("LOADING PORT", entryHeaderWrapper.LoadPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper.ArrivalPort);
			AssertEquals("DESTINATION PORT", entryHeaderWrapper.DestlPort);
			AssertEquals("ARRIVAL PORT   20DEC16", entryHeaderWrapper.DschlPort);
			AssertEquals("FIRST ARRIVAL PORT   20DEC16", entryHeaderWrapper.FirstPort);

			var mockEntry2 = Factory.NewMoq<CusEntryHeader>();
			mockEntry2.Setup(m => m.Nature).Returns(new ZString("10"));
			var entry2 = mockEntry2.Object;
			entry2.CH_JE = Declaration.PK;
			var entryHeaderWrapper2 = DocCusEntryHeader.New(entry2, Factory);
			AssertEquals("LOADING PORT", entryHeaderWrapper2.LoadPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper2.ArrivalPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper2.DestlPort);
			AssertEquals("ARRIVAL PORT   20DEC16", entryHeaderWrapper2.DschlPort);
			AssertEquals("FIRST ARRIVAL PORT   20DEC16", entryHeaderWrapper2.FirstPort);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			entryHeaderWrapper2 = DocCusEntryHeader.New(entry2, Factory);
			AssertEquals("LOADING PORT", entryHeaderWrapper2.LoadPort);
			AssertEquals("QLD PARCELS POST   20DEC16", entryHeaderWrapper2.ArrivalPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper2.DestlPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper2.DschlPort);
			AssertEquals(ZString.Empty, entryHeaderWrapper2.FirstPort);
		}

		public void TestTotalNoOfPacks()
		{
			SetJobDeclarationForATDTesting();
			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("IsCMREntry", "Y", newWrapper.IsCMREntry.ToString());
			Declaration.JE_TotalNoOfPacks = 10;
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_MessageSubType = "SAC";
			Assert("Is SAC", EntryHeaderInternal.IsSAC);

			AssertEquals("      10   (ONE ZERO)", newWrapper.FormattedTotalNoOfPacks);
			AssertEquals("TOTAL NUMBER OF PACKAGES", newWrapper.TotalNoOfPacksTitle);

			Header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("IsNature1020", "Y", newWrapper.IsCMRNature1020.ToString());

			AssertEquals("      10   (ONE ZERO)", newWrapper.FormattedTotalNoOfPacks);
			AssertEquals("TOTAL NUMBER OF PACKAGES", newWrapper.TotalNoOfPacksTitle);

			Declaration.JE_MessageSubType = "FRM";
			Declaration.JE_TransportMode = "OTH";
			Assert("Transport mode is Other", EntryHeaderInternal.IsTransportModeOther);
			AssertEquals("      10   (ONE ZERO)", newWrapper.FormattedTotalNoOfPacks);
		}

		public void TestImportEntryAdvice()
		{
			AssertEquals("ImportEntryAdvice", EntryHeaderInternal.ImportEntryAdvice, EntryHeaderWrapperInternal.ImportEntryAdvice);
		}

		public void TestStatusDescription()
		{
			EntryHeaderInternal.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("StatusDescription", EntryHeaderInternal.MessageStatusDescription, EntryHeaderWrapperInternal.StatusDescription);
		}

		public void TestBranchID()
		{
			AssertEquals("Returns registry value", Env.Registry.AUCustoms.LocalCustomsBranchIdentifier, EntryHeaderWrapperInternal.BranchID);

			var orgMessage = Factory.New<CMRIMDMessage>();
			orgMessage.EM_MessageText = CMRImportDeclarationTestData.IMD; //IMD+B00122382/1/NAD+VT+AA33HF
			orgMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			orgMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			EntryHeaderInternal.Messages.Add(orgMessage);
			EntryHeaderInternal.EntryNumber = "AAAA7GW6R";
			AssertEquals("BranchID comes from original message", "AA33HF", EntryHeaderWrapperInternal.BranchID);
		}

		public void TestGetEntryPrintLines()
		{
			AssertEquals("Expected entry print lines", true, EntryHeaderWrapperInternal.GetEntryPrintLines(true).Length > 0);
			AssertEquals("Expected entry print lines", true, EntryHeaderWrapperInternal.GetEntryPrintLines(false).Length > 0);
		}

		#region Total Number Of Pacakges

		public void TestTotalNumberOfPacakges()
		{
			SetJobDeclarationForATDTesting();
			Declaration.DisableDefaultPackingInformation = false;
			Declaration.JE_HouseBill = "1";
			Bill houseBill1 = Declaration.Bills[0];
			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "2";

			Package pack1 = Declaration.Packages[0];
			Package pack2 = Declaration.Packages[1];
			pack1.CW_PackQty = 10;
			pack2.CW_PackQty = 35;
			pack2.CW_HouseBill = houseBill2.CU_BillUniqueCode;

			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("HouseBillContainersForEntry total pack count", "45", newWrapper.TotalNumberOfPacakges);
		}

		public void TestTotalNumberOfPacakgesForNature30()
		{
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockEntry.Setup(m => m.Nature).Returns(new ZString("30"));
			CusEntryHeader entry = mockEntry.Object;
			entry.CH_JE = Declaration.PK;
			DocCusEntryHeader entryHeaderWrapper = DocCusEntryHeader.New(entry, Factory);

			AssertEquals(true, entry.IsNature30);
			Declaration.JE_TotalNoOfPacks = 150;
			AssertEquals("Total Number Of Packages", "150", entryHeaderWrapper.TotalNumberOfPacakges);

			entry.WarehouseNumberOfPacks = 250;
			AssertEquals("Total Number Of Packages", "250", entryHeaderWrapper.TotalNumberOfPacakges);
		}

		#endregion

		#endregion

		#region Wrapper Fields

		public void TestPrimeEntry()
		{
			AssertNull("PrimeEntry", EntryHeaderInternal.PrimeEntry);

			EntryHeaderInternal.CH_CH_PrimeEntry = Factory.New(typeof(CusEntryHeader)).PK;
			AssertNotNull("PrimeEntry", EntryHeaderInternal.PrimeEntry);
			AssertEquals("PrimeEntry is of type DocCusEntryHeader", typeof(DocCusEntryHeader), EntryHeaderWrapperInternal.PrimeEntry.GetType());
		}

		public void TestDeclaration()
		{
			AssertNotNull("Declaration", EntryHeaderInternal.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), EntryHeaderWrapperInternal.Declaration.GetType());
		}

		#endregion

		#region Collection

		public void TestPayableCharges()
		{
			CreateEntryHeaderCharges();
			AssertEquals("Only 3 charges are payable", 3, EntryHeaderWrapperInternal.PayableCharges.Count);
		}

		public void TestPayableFees()
		{
			CreateEntryLineFees();
			AssertEquals("Only 2 line fees are payable", 2, EntryHeaderWrapperInternal.PayableFees.Count);
		}

		#endregion

		#region ATD Properties

		public void TestN30AuthorityText()
		{
			EntryHeaderInternal.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			EntryHeaderInternal.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("IsNature30", "Y", EntryHeaderWrapperInternal.IsCMRNature30.ToString());
			AssertEquals("Nature 30 - use HomeConsumptionAuthorityText", DocCusEntryHeader.HomeConsumptionAuthorityText, EntryHeaderWrapperInternal.AuthorityText);
		}

		public void TestN20AuthorityText()
		{
			SetJobDeclarationForATDTesting();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			Header2.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";

			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();

			AssertEquals("IsNature20", "Y", EntryHeaderWrapperInternal.IsCMRNature20.ToString());
			AssertEquals("Nature 20 - use WarehousingGoodsAuthorityText", DocCusEntryHeader.WarehousingGoodsAuthorityText, EntryHeaderWrapperInternal.AuthorityText);
		}

		public void TestN1020AuthorityText()
		{
			SetJobDeclarationForATDTesting();
			Header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";

			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();

			AssertEquals("IsNature1020", "Y", newWrapper.IsCMRNature1020.ToString());
			AssertEquals("Nature 1020 - use HomeConsumptionAndWarehouseAuthorityText", DocCusEntryHeader.HomeConsumptionAndWarehouseAuthorityText, newWrapper.AuthorityText);
		}

		public void TestN10AuthorityText()
		{
			SetJobDeclarationForATDTesting();
			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();

			AssertEquals("IsNature10", "Y", newWrapper.IsCMRNature10.ToString());
			AssertEquals("Nature 10 - use Home Consumption authority text", DocCusEntryHeader.HomeConsumptionAuthorityText, newWrapper.AuthorityText);
		}

		public void TestHouseBillContainersForEntry()
		{
			SetJobDeclarationForATDTesting();
			Declaration.DisableDefaultPackingInformation = false;
			Declaration.JE_HouseBill = "1";
			Bill houseBill1 = Declaration.Bills[0];
			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "2";

			Package pack1 = Declaration.Packages[0];
			Package pack2 = Declaration.Packages[1];
			pack1.CW_PackQty = 10;
			pack2.CW_PackQty = 35;
			pack2.CW_HouseBill = houseBill2.CU_BillUniqueCode;

			DocCusEntryHeader newWrapper = MergeDeclarationAndGetEntryHeaderWrapper();
			AssertEquals("HouseBillContainersForEntry count", 2, newWrapper.HouseBillContainersForEntry.Count);
			AssertEquals("HouseBillContainersForEntry count", 2, newWrapper.HouseBillContainersForEntryLineCount);
			AssertEquals("HouseBillContainersForEntry total pack count", 45, newWrapper.HouseBillContainersForEntryTotalNumberOfPackages);
		}

		public void TestAuthorityToDeal()
		{
			AssertNull("ATD null", EntryHeaderWrapperInternal.AuthorityToDealMessage);
			AssertNotNull("Message lines collection should not be null", EntryHeaderWrapperInternal.MessageLines);
			AssertEquals("Message lines collection empty", 0, EntryHeaderWrapperInternal.MessageLines.Count);

			SetJobDeclarationForATDTesting();

			var aTDMessage = EntryHeaderInternal.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CusEntryHeaderTest.ATDMessageText;

			AssertNotNull("ATD not null", EntryHeaderWrapperInternal.AuthorityToDealMessage);
		}

		#endregion

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.DisableDefaultPackingInformation = true;
					fDeclaration.JE_DeclarationReference = "B00148999";
					fDeclaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceHeader Header2;
		JobComInvoiceHeader Header1;

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(EntryHeaderInternal, Factory);
		}

		protected override void TearDown()
		{
			fDeclaration = null;
			base.TearDown();
		}

		protected override CusEntryHeader GetNewEntryHeader()
		{
			return Declaration.CustomsEntryHeaders.AddNew();
		}

		void CreateEntryHeaderCharges()
		{
			CusEntryHeaderCharges charges1 = EntryHeaderInternal.Charges.AddNew();
			charges1.C1_ChargeAmount = 100M;
			charges1.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;

			CusEntryHeaderCharges charges2 = EntryHeaderInternal.Charges.AddNew();
			charges2.C1_ChargeAmount = 100M;
			charges2.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;

			CusEntryHeaderCharges charges3 = EntryHeaderInternal.Charges.AddNew();
			charges3.C1_ChargeAmount = 100.50M;
			charges3.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISContainerCharges;

			CusEntryHeaderCharges charges4 = EntryHeaderInternal.Charges.AddNew();
			charges4.C1_ChargeAmount = 300.45M;
			charges4.C1_ChargeType = CusEntryChargeTypeList.Codes.FlatDutyPortion;
		}

		void CreateEntryLineFees()
		{
			CusEntryLine line1 = EntryHeaderInternal.MergedLines.AddNew();
			CusEntryLineFee fee1 = line1.Fees.AddNew();
			CusEntryLineFee fee2 = line1.Fees.AddNew();
			fee1.CF_ChargeAmount = 50M;
			fee1.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			fee2.CF_ChargeAmount = 30M;
			fee2.CF_ChargeType = CusEntryChargeTypeList.Codes.EntryFee;

			CusEntryLine line2 = EntryHeaderInternal.MergedLines.AddNew();
			CusEntryLineFee fee3 = line1.Fees.AddNew();
			fee3.CF_ChargeAmount = 40M;
			fee3.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
		}

		void SetJobDeclarationForATDTesting()
		{
			Declaration.JE_ExportDate = ZDateTime.Today;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = "CMR";

			Header1 = Declaration.Invoices.AddNew();
			Header1.JobComInvoiceLines.AddNew();

			Header2 = Declaration.Invoices.AddNew();
			Header2.JobComInvoiceLines.AddNew();

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			Declaration.MessageInitiator = sender;
		}

		DocCusEntryHeader MergeDeclarationAndGetEntryHeaderWrapper()
		{
			Declaration.DoMerge();
			EntryHeaderInternal = Declaration.CustomsEntryHeaders[0];
			DocCusEntryHeader newWrapper = DocCusEntryHeader.New(EntryHeaderInternal, Factory);
			return newWrapper;
		}
		#endregion
	}
}
