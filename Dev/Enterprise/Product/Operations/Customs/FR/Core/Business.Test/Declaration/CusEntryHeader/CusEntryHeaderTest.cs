using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
	{
		public override void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<CusEntryHeader>(), "FRCusEntryHeader");
		}

		public void TestCountriesOfRoutingForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
			AssertEquals("Prerequisite: CountriesOfRouting should be empty.", 0, entryHeader.CountriesOfRouting.Count);

			officeOfExit.CY_Data = "FRXXXXX";
			AssertEquals("FR should not be considered as valid for itinerary and thus not added to the list.", 0, entryHeader.CountriesOfRouting.Count);

			officeOfExit.CY_Data = "TRXXXXX";
			AssertEquals("TR is not member of EU Customs union, thus not added to the list.", 0, entryHeader.CountriesOfRouting.Count);

			officeOfExit.CY_Data = "GBXXXXX";
			AssertEquals("GB is not member of EU Customs union, thus not added to the list.", 0, entryHeader.CountriesOfRouting.Count);

			officeOfExit.CY_Data = "USXXXXX";
			AssertEquals("US is not member of EU Customs union, thus not added to the list.", 0, entryHeader.CountriesOfRouting.Count);

			officeOfExit.CY_Data = "ITXXXXX";
			AssertEquals("IT is a member of EU Customs union, thus added to the list when it is empty.", 1, entryHeader.CountriesOfRouting.Count);
		}

		public void TestValidationType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CusEntryHeaderValidation>(entryHeader.Validation);
		}

		public void TestIsPromotionalProductToDROM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			Assert("Prerequisite: first entry line is not promotional product to DROM.", !entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is not promotional product to DROM.", !entryLine2.IsPromotionalProductToDROM);
			Assert("The entry should not be considered as promotional product to DROM when no entry line is.", !entry.IsPromotionalProductToDROM);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is not promotional product to DROM.", !entryLine2.IsPromotionalProductToDROM);
			Assert("The entry should not be considered as promotional product to DROM when not all entry line are.", !entry.IsPromotionalProductToDROM);

			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is promotional product to DROM.", entryLine2.IsPromotionalProductToDROM);
			Assert("The entry should be considered as promotional product to DROM when all entry line are.", entry.IsPromotionalProductToDROM);
		}

		public void TestIsProductOfNegligibleValueToDROM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			Assert("Prerequisite: first entry line is not product of negligible value to DROM.", !entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value to DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			Assert("The entry should not be considered as product of negligible value to DROM when no entry line is.", !entry.IsProductOfNegligibleValueToDROM);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value to DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			Assert("The entry should not be considered as product of negligible value to DROM when not all entry line are.", !entry.IsProductOfNegligibleValueToDROM);

			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is product of negligible value to DROM.", entryLine2.IsProductOfNegligibleValueToDROM);
			Assert("The entry should be considered as product of negligible value to DROM when all entry line are.", entry.IsProductOfNegligibleValueToDROM);
		}

		public void TestCH_CalcTotalInvoicedAmountInLocalCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine11 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 1000m;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var invoiceLine21 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine21.JI_LinePrice = 1000m;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 1000m;
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invoiceLine31 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine31.JI_LinePrice = 1000m;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			invoiceLine11.JI_CL = entryLine1.PK;
			var entryLine2 = entry.MergedLines.AddNew();
			invoiceLine21.JI_CL = entryLine2.PK;
			invoiceLine31.JI_CL = entryLine2.PK;

			AssertEquals("Prerequisite: entryLine1 CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency.", 719.42m, entryLine1.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
			AssertEquals("Prerequisite: entryLine2 CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency.", 1490.20m, entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
			AssertEquals("Entry CH_CalcTotalInvoicedAmountInLocalCurrency should be the sum of entry lines CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency", 2209.62m, entry.CH_CalcTotalInvoicedAmountInLocalCurrency);
		}

		public void TestCanBeRevertedToLastBAE()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();

			CombineAssertions("Delta IE: ", () =>
			{
				var declaration_DeltaIE = Factory.New<JobDeclaration>();
				declaration_DeltaIE.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				entryHeader.CH_JE = declaration_DeltaIE.PK;
				Assert("entryHeader.CanBeRevertedToLastBAE should be false for DeltaIE for this moment.", !entryHeader.CanBeRevertedToLastBAE);
			});

			CombineAssertions("Delta G: ", () =>
			{
				var declaration_DeltaG = Factory.New<JobDeclaration>();
				declaration_DeltaG.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				entryHeader.CH_JE = declaration_DeltaG.PK;
				Assert("When DeltaG entryHeader is not RectificationError, its CanBeRevertedToLastBAE should be false.", !entryHeader.CanBeRevertedToLastBAE);
				var outgoingMessage = AddOutgoingMessage(entryHeader);
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
				var incomingMessage = AddIncomingMessage(entryHeader, "Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
				Assert("Prerequisite: entry is in status of RectificationError", new DeltaGStatusResolver(entryHeader, incomingMessage).CheckIsRectificationError());
				Assert(entryHeader.CanBeRevertedToLastBAE);
			});

			FREDIMessage AddOutgoingMessage(CusEntryHeader entry)
			{
				var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				outgoingInterchange.EI_InterchangeNum = "123";
				var outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
				outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
				entry.Messages.Add(outgoingMessage);
				outgoingMessage.EM_EI = outgoingInterchange.PK;
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				Factory.Save();
				return outgoingMessage;
			}

			FREDIMessage AddIncomingMessage(CusEntryHeader entry, string messageFile)
			{
				var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				incomingInterchange.EI_InterchangeNum = "123.";
				var incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
				entry.Messages.Add(incomingMessage);
				incomingMessage.EM_EI = incomingInterchange.PK;
				incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				incomingMessage.EM_MessageText = resourceRetriever.Value.GetString(messageFile);
				return incomingMessage;
			}
		}
		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		public void TestLookups()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
		}

		#region T2LxApplicableEntryLines

		public void TestT2LApplicableEntryLines()
		{
			var typeOfDocument = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument;
			Func<CusEntryHeader, IEnumerable<CusEntryLine>> getT2LEntryLines = entry => entry.T2LApplicableEntryLines;
			AssertT2LApplicableEntryLines(new[] { "1111", "2222", "4444", "3333", "5555" }, "Declaration", typeOfDocument, getT2LEntryLines, "There should be 5 elements in entryHeader.T2LApplicableEntryLines as N825 in declaration level is supportive for all inner lines.");
			AssertT2LApplicableEntryLines(new[] { "2222" }, "InvoiceLine", typeOfDocument, getT2LEntryLines, "There should be 1 element in entryHeader.T2LApplicableEntryLines as N825 in invoice line level is supportive for the only one related entry line.");
			AssertT2LApplicableEntryLines(new[] { "1111", "2222", "3333" }, "InvoiceHeader", typeOfDocument, getT2LEntryLines, "There should be 3 elements in entryHeader.T2LApplicableEntryLines as N825 in invoice level is supportive for children lines only.");
		}

		public void TestT2LFApplicableEntryLines()
		{
			var typeOfDocument = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument;
			Func<CusEntryHeader, IEnumerable<CusEntryLine>> getT2LFEntryLines = entry => entry.T2LFApplicableEntryLines;
			AssertT2LApplicableEntryLines(new[] { "1111", "2222", "4444", "3333", "5555" }, "Declaration", typeOfDocument, getT2LFEntryLines, "There should be 5 elements in entryHeader.T2LFApplicableEntryLines as C620 in declaration level is supportive for all inner lines.");
			AssertT2LApplicableEntryLines(new[] { "2222" }, "InvoiceLine", typeOfDocument, getT2LFEntryLines, "There should be 1 element in entryHeader.T2LFApplicableEntryLines as C620 in invoice line level is supportive for the only one related entry line.");
			AssertT2LApplicableEntryLines(new[] { "1111", "2222", "3333" }, "InvoiceHeader", typeOfDocument, getT2LFEntryLines, "There should be 3 elements in entryHeader.T2LFApplicableEntryLines as C620 in invoice level is supportive for children lines only.");
		}

		void AssertT2LApplicableEntryLines(string[] listOfDocument, string lvlAtWhichTheDocIsAdded, string typeOfDocument, Func<CusEntryHeader, IEnumerable<CusEntryLine>> getT2LxEntryLine, string message)
		{
			var entryHeader = GetTestDataForT2LApplicableEntryLines(typeOfDocument);

			if (lvlAtWhichTheDocIsAdded == "Declaration")
			{
				entryHeader.Declaration.SupportingDocuments.AddNew().CSI_Code = typeOfDocument;
			}
			else if (lvlAtWhichTheDocIsAdded == "InvoiceHeader")
			{
				entryHeader.Declaration.Invoices[0].SupportingDocuments.AddNew().CSI_Code = typeOfDocument;
			}
			else
			{
				entryHeader.Declaration.Invoices[0].InvoiceLines[1].SupportingDocuments.AddNew().CSI_Code = typeOfDocument;
			}

			AssertContainsExactElementsInAnyOrder(message, listOfDocument, getT2LxEntryLine(entryHeader).Select(x => x.Tariff.ToString()).ToList());
		}

		CusEntryHeader GetTestDataForT2LApplicableEntryLines(string documentType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = documentType switch
			{
				UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument => Core.Constants.TransportModes.Sea,
				UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument => ZString.Empty,
				_ => throw new ArgumentOutOfRangeException(nameof(documentType), "documentType should be T2L or T2LF")
			};

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Tariff = "1111";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.FillWithValidTestData();
			invoiceLine2.JI_Tariff = "2222";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.FillWithValidTestData();
			invoiceLine3.JI_Tariff = "3333";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.FillWithValidTestData();
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.FillWithValidTestData();
			invoiceLine4.JI_Tariff = "4444";
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.FrenchGuyana;
			var invoiceLine5 = invoice2.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "5555";
			invoiceLine5.JI_CountryOfOrigin = ZString.Empty;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			return declaration.CustomsEntryHeaders[0];
		}

		#endregion

		public void TestIsChangingToClearStatusForAccIntegration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			entryHeader.CH_JE = declaration.PK;

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			AssertEquals("ES050 status should never trigger any billing.", false, entryHeader.IsChangingToClearStatusForAccIntegration);

			Factory.Save();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			AssertEquals("Change of status from unclear to clear status should trigger a billing.", true, entryHeader.IsChangingToClearStatusForAccIntegration);

			Factory.Save();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			AssertEquals("Change of status from clear status to same status should not trigger a billing.", false, entryHeader.IsChangingToClearStatusForAccIntegration);

			Factory.Save();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			AssertEquals("Change of status from clear to clear and complete status should trigger a billing.", true, entryHeader.IsChangingToClearStatusForAccIntegration);
		}

		public void TestIsStatusChangingToCleared()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES010, EntryStatusDescriptionCodeList.Codes.ES050));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES010, EntryStatusDescriptionCodeList.Codes.ES100));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES010, EntryStatusDescriptionCodeList.Codes.ES101));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES010, EntryStatusDescriptionCodeList.Codes.ES130));
			AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES100));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES130));
			AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES101));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES130));
			AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusDescriptionCodeList.Codes.ES130, EntryStatusDescriptionCodeList.Codes.ES130));
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			entryHeader.CH_JE = declaration.PK;

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.Constants.JDD_ParentID] = entryHeader.PK;
			documentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = "CH";

			AssertEquals(1, entryHeader.BusinessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { documentData }, entryHeader.BusinessObjectsWithRelatedEvents);
		}

		public void TestITRCDetailsMembers()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000002";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "FRXXX";
			var portOfArrival = CreateNewOrGetExistingRefUNLOCO("FRXXX");
			var portofLoading = CreateNewOrGetExistingRefUNLOCO("AUSYD");
			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			entryNumber.CE_EntryNum = "BKG001";

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_MasterBill = "MAWB123";

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.JE_DateAtOrigin = new ZDateTime(2023, 05, 10);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2023, 05, 15);

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var iTRCDetails = entryHeader.TRCDetailsProvider as ITRCDetails;
			AssertEquals(entryHeader, iTRCDetails.BusinessObject);
			AssertEquals("CustomsDeclaration", iTRCDetails.SourceType);
			AssertEquals("B00000002", iTRCDetails.SourceID);
			AssertContainsExactElementsInAnyOrder(new CommonContainer[] { container1.JobContainer, container2.JobContainer }, iTRCDetails.Containers);
			AssertEquals("AUSYD", iTRCDetails.PortOfOrigin);
			AssertEquals("FRXXX", iTRCDetails.PortOfDestination);
			AssertEquals(portOfArrival, iTRCDetails.OperationalPortImport);
			AssertEquals(portofLoading, iTRCDetails.OperationalPortExport);
			AssertEquals("BKG001", iTRCDetails.BookingConfirmationReference);
			AssertEquals(declaration.Lookups.CargoIdTypeList, iTRCDetails.ContainerModeList);
			AssertEquals("FCL", iTRCDetails.ContainerMode);
			AssertEquals(new CodeDescriptionPairList(), iTRCDetails.ShipmentTypeList);
			AssertEquals("", iTRCDetails.ShipmentType);
			AssertEquals("MAWB123", iTRCDetails.WaybillNumber);
			AssertEquals(forwarder.MainAddress, iTRCDetails.ReceivingForwarder);
			AssertEquals(forwarder.MainAddress, iTRCDetails.SendingForwarder);
			AssertEquals(shippingLine.MainAddress, iTRCDetails.Carrier);
			AssertEquals(new ZDateTime(2023, 05, 10), iTRCDetails.ETD);
			AssertEquals(new ZDateTime(2023, 05, 15), iTRCDetails.ETA);
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		public void TestMRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, nameof(TemporaryStorageHeader.MRN));
		}

		public void TestDescriptionPropertyAttribute()
		{
			var attr = typeof(CusEntryHeader).GetCustomAttribute<DescriptionPropertyAttribute>();
			AssertEquals(nameof(CusEntryHeader.CH_BGMReference), attr.PropertyName);
		}

		#region TriggeringPointForValidation

		public void TestArrivalDetailsChangedEvent_ShouldBeAddedAsADC_WhenTheEntryHeaderNewlyCreated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders.First();
			Assert("Prerequisite: entry should no be saved yet.", !entryHeader.IsInDatabase);

			entryHeader.OnSaving();
			CombineAssertions(() =>
			{
				AssertEquals("Prerequisite: When no registry specified, the TriggeringPointForValidation of entry is NUL by default.", TriggerPointsCodeList.Codes.NUL, entryHeader.CH_TriggeringPointForValidation);

				var mostRecentLog = entryHeader.Logs.MostRecentLog;
				AssertEquals("ADC event should be added for newly created entry header.", Events.ArrivalDetailsChanged.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ADC event reference should be set as the TriggeringPointForValidation value.", "VAA Trigger = NUL", mostRecentLog.SL_Reference);
			});
		}

		public void TestArrivalDetailsChangedEvent_ShouldBeAddedAsADC_WhenTriggeringPointForValidationIsModified()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			CombineAssertions(() =>
			{
				Assert("Prerequisite: entry should be saved already.", entryHeader.IsInDatabase);
				AssertEquals("Prerequisite: When no registry specified, the TriggeringPointForValidation of entry is NUL by default.", TriggerPointsCodeList.Codes.NUL, entryHeader.CH_TriggeringPointForValidation);
				var mostRecentLog = entryHeader.Logs.MostRecentLog;
				AssertEquals("Prerequisite: ADC event should be added for newly created entry header.", Events.ArrivalDetailsChanged.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("Prerequisite: Only one ADC event is added as NUL in the entry.", "VAA Trigger = NUL", mostRecentLog.SL_Reference);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.PAB;
				Factory.Save();

				mostRecentLog = entryHeader.Logs.MostRecentLog;
				AssertEquals("ADC event should be added when TriggeringPointForValidation is modified.", Events.ArrivalDetailsChanged.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ADC event reference should be set as the new TriggeringPointForValidation value.", "VAA Trigger = PAB", mostRecentLog.SL_Reference);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.PAB;
				Factory.Save();
				AssertArrayEqualsByElements("There're still only two ADC events after setter of CH_TriggeringPointForValidation and Factory.Save() are both called again, because the value of CH_TriggeringPointForValidation is not actually updated.", new[] { "ADC:VAA Trigger = NUL", "ADC:VAA Trigger = PAB" }, entryHeader.Logs.GetAllLogs().Select(l => $"{l.SL_SE_NKEvent}:{l.SL_Reference}").ToArray());

				entryHeader.CH_SequenceNumber = 1;
				Factory.Save();
				AssertArrayEqualsByElements("There're still only two ADC events after setter of another AddInfo and Factory.Save() are both called again, because the value of CH_TriggeringPointForValidation is not updated.", new[] { "ADC:VAA Trigger = NUL", "ADC:VAA Trigger = PAB" }, entryHeader.Logs.GetAllLogs().Select(l => $"{l.SL_SE_NKEvent}:{l.SL_Reference}").ToArray());
			});
		}

		#endregion

		public void TestCusEntryLinesConfirmedValueHasBeenPopulated()
		{
			var entryheader1 = Factory.New<CusEntryHeader>();
			var entryheader2 = Factory.New<CusEntryHeader>();

			var sql = $@"
IF EXISTS(SELECT NULL FROM sys.tables WHERE name = 'ClientFRCusEntryLineWithResponseMessage')
BEGIN
	DROP TABLE ClientFRCusEntryLineWithResponseMessage
END
CREATE TABLE ClientFRCusEntryLineWithResponseMessage
(
	FR2_CH_PK UNIQUEIDENTIFIER NOT NULL
)
INSERT INTO ClientFRCusEntryLineWithResponseMessage VALUES ('{entryheader1.PK}')
INSERT INTO ClientFRCusEntryLineWithResponseMessage VALUES ('{entryheader2.PK}')";

			try
			{
				Db.Connection.Command(sql).ExecuteNonQuery();
				Assert(!entryheader1.CusEntryLinesConfirmedValueHasBeenPopulated);
			}
			finally
			{
				sql = $@"
IF EXISTS(SELECT NULL FROM sys.tables WHERE name = 'ClientFRCusEntryLineWithResponseMessage')
BEGIN
	DROP TABLE ClientFRCusEntryLineWithResponseMessage
END";
				Db.Connection.Command(sql).ExecuteNonQuery();
				Assert(entryheader2.CusEntryLinesConfirmedValueHasBeenPopulated);
			}
		}

		public void TestCH_Calc_GuaranteeAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
			Factory.Save();
			var euDtyRateType = helper.CreateNewOrGetExistingRateType(euGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "";
			usage.AGC_OH_Owner = ZGuid.Empty;
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault() ?? declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12);
			entryLine1.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29);
			entryLine1.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";
			CombineAssertions("To make sure EntryLine fields return expected result.", () =>
			{
				AssertEquals(12m, entryLine1.DutyAmount);
				AssertEquals(29m, entryLine1.GSTVATAmount);
				AssertEquals(43m, entryLine1.ParaFiscal);
			});
			AssertEquals("(DutyAmount 12 x 100%) + (GSTVATAmount 29 x 5%) + (ParaFiscal 43 x 15%)", 19.9m, entryLine1.SpecificRegimeGuaranteeAmount);
			AssertEquals("CL_Calc_GuaranteeAmount should be equal to the rounding of SpecificRegimeGuaranteeAmount", 20m, entryLine1.CL_Calc_GuaranteeAmount);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 10);
			entryLine2.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29);
			entryLine2.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";
			CombineAssertions("To make sure EntryLine fields return expected result.", () =>
			{
				AssertEquals(10m, entryLine2.DutyAmount);
				AssertEquals(29m, entryLine2.GSTVATAmount);
				AssertEquals(43m, entryLine2.ParaFiscal);
			});
			AssertEquals("(DutyAmount 10 x 100%) + (GSTVATAmount 29 x 5%) + (ParaFiscal 43 x 15%)", 17.9m, entryLine2.SpecificRegimeGuaranteeAmount);
			AssertEquals("CL_Calc_GuaranteeAmount should be equal to the rounding of SpecificRegimeGuaranteeAmount", 18m, entryLine2.CL_Calc_GuaranteeAmount);

			AssertEquals("CH_Calc_GuaranteeAmount Should be equal to the sum of CL_Calc_GuaranteeAmount of each entryLine.", 38m, entryHeader.CH_Calc_GuaranteeAmount);
		}
		internal static CusAuthorisationRule CreateCusAuthorisationRule(CusAuthorisationHeader authHeader, string code, string value)
		{
			var rule = authHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = code;
			rule.CPR_ValueFrom = value;
			return rule;
		}

		public void TestAssessmentDate()
		{
			var entryheader = Factory.New<CusEntryHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals("AssessmentDate should be empty because no entry instruction has setup against the entry header yet.", ZDateTime.Empty, entryheader.AssessmentDate);

			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("AssessmentDate should be empty because entry instruction CEI_DateForDuty has not be setup yet.", ZDateTime.Empty, entryheader.AssessmentDate);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			AssertEquals("AssessmentDate should map entry instruction CEI_DateForDuty.", ZDateTime.Today, entryheader.AssessmentDate);
		}

		public void TestGetTemporaryStorageRegisterTransactionData_DeltaG()
		{
			var register1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT1";

			var register2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT2";

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.DDTNumber = "DDT1";

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			ist1.DDTNumber = "DDT2";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 69;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.FillWithValidTestData();
			var doc1 = invoiceLine1.PreviousDocuments.AddNew();
			doc1.CSI_LineNo = 1;
			doc1.CSI_Code = "IST";
			doc1.CSI_ReferenceNumber = "FRJ_IST1";
			invoiceLine1.JI_Weight = 10m;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.FillWithValidTestData();
			var doc2 = invoiceLine2.PreviousDocuments.AddNew();
			doc2.CSI_LineNo = 1;
			doc2.CSI_Code = "IST";
			doc2.CSI_ReferenceNumber = "FRJ_IST1";
			invoiceLine2.JI_Weight = 20m;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.FillWithValidTestData();
			var doc3 = invoiceLine3.PreviousDocuments.AddNew();
			doc3.CSI_LineNo = 1;
			doc3.CSI_Code = "IST";
			doc3.CSI_ReferenceNumber = "FRJ_IST2";
			invoiceLine3.JI_Weight = 50m;

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.FillWithValidTestData();
			var doc4 = invoiceLine4.PreviousDocuments.AddNew();
			doc4.CSI_LineNo = 2;
			doc4.CSI_Code = "IST";
			doc4.CSI_ReferenceNumber = "FRJ_IST1";
			invoiceLine4.JI_Weight = 80m;

			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine5.FillWithValidTestData();
			var doc5 = invoiceLine5.PreviousDocuments.AddNew();
			doc5.CSI_LineNo = 1;
			doc5.CSI_Code = FRConstants.PreviousDocuments.N337;
			doc5.CSI_ReferenceNumber = "FRJ_IST1";
			invoiceLine5.JI_Weight = 100m;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 1;

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 2;

			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var packing3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0];
			packing3.IsLinked = true;
			packing3.PackQty = 5;

			var package4 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var packing4 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[0];
			packing4.IsLinked = true;
			packing4.PackQty = 8;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine1 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine1.PK;
			var cusEntryLine2 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine2.PK;
			var cusEntryLine3 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = cusEntryLine3.PK;
			var cusEntryLine4 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine4.JI_CL = cusEntryLine4.PK;
			var cusEntryLine5 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine5.JI_CL = cusEntryLine5.PK;

			cusEntryHeader.CH_BGMReference = "InternalReference";
			cusEntryHeader.EntryNumber = "CustomsReference";

			var transactionLines = cusEntryHeader.GetTemporaryStorageRegisterTransactionData().ToList();
			AssertEquals(@"First and second entry line should create a unique transaction as their previous IST document share same Reference Number and Line No.
Third entry line should create its own transaction because its previous IST document Reference Number is unique.
Fourth entry line should create its own transaction because its its previous IST document Line No is unique.
Fifth entry line should not create any transaction because it has no IST type of previous document.", 3, transactionLines.Count);

			CombineAssertions("Transaction values", () =>
			{
				AssertEquals("First transaction line RegisterHeader", ist1.RegisterHeader, transactionLines[0].PreviousRegisterHeader);
				AssertEquals("First transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[0].CustomsReferenceNumber);
				AssertEquals("First transaction line InternalReferenceNumber", "InternalReference", transactionLines[0].InternalReferenceNumber);
				AssertEquals("First transaction line PackageQuantity", 3, transactionLines[0].PackageQuantity);
				AssertEquals("First transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[0].ReferenceType);
				AssertEquals("First transaction line GrossMass", 30m, transactionLines[0].GrossMass);
				AssertEquals("First transaction line Comment", "Entry line 0", transactionLines[0].Comments);
				AssertEquals("First transaction line LineNo", 1, transactionLines[0].RegisterLineNo);

				AssertEquals("Second transaction line RegisterHeader", ist2.RegisterHeader, transactionLines[1].PreviousRegisterHeader);
				AssertEquals("Second transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[1].CustomsReferenceNumber);
				AssertEquals("Second transaction line InternalReferenceNumber", "InternalReference", transactionLines[1].InternalReferenceNumber);
				AssertEquals("Second transaction line PackageQuantity", 5, transactionLines[1].PackageQuantity);
				AssertEquals("Second transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[1].ReferenceType);
				AssertEquals("Second transaction line GrossMass", 50m, transactionLines[1].GrossMass);
				AssertEquals("Second transaction line Comment", "Entry line 0", transactionLines[1].Comments);
				AssertEquals("Second transaction line LineNo", 1, transactionLines[1].RegisterLineNo);

				AssertEquals("Third transaction line RegisterHeader", ist1.RegisterHeader, transactionLines[2].PreviousRegisterHeader);
				AssertEquals("Third transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[2].CustomsReferenceNumber);
				AssertEquals("Third transaction line InternalReferenceNumber", "InternalReference", transactionLines[2].InternalReferenceNumber);
				AssertEquals("Third transaction line PackageQuantity", 8, transactionLines[2].PackageQuantity);
				AssertEquals("Third transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[2].ReferenceType);
				AssertEquals("Third transaction line GrossMass", 80m, transactionLines[2].GrossMass);
				AssertEquals("Third transaction line Comment", "Entry line 0", transactionLines[2].Comments);
				AssertEquals("Third transaction line LineNo", 2, transactionLines[2].RegisterLineNo);
			});
		}

		public void TestGetTemporaryStorageRegisterTransactionData_DeltaIE()
		{
			var register1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT1";

			var register2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT2";

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.DDTNumber = "DDT1";

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			ist1.DDTNumber = "DDT2";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Weight = 10m;

			var doc1 = invoiceLine1.PreviousDocuments.AddNew();
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_Code = FRConstants.PreviousDocuments.N337;
			doc1.CSI_ReferenceNumber = "FRJ_IST1";
			doc1.CSI_PackQty = 1;
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "KGM";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.FillWithValidTestData();
			invoiceLine2.JI_Weight = 20m;

			var doc2 = invoiceLine2.PreviousDocuments.AddNew();
			doc2.CSI_ItemNumber = 1;
			doc2.CSI_Code = FRConstants.PreviousDocuments.N337;
			doc2.CSI_ReferenceNumber = "FRJ_IST1";
			doc2.CSI_PackQty = 2;
			doc2.CSI_Quantity = 20m;
			doc2.CSI_UnitOfQuantity = "KGM";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.FillWithValidTestData();
			invoiceLine3.JI_Weight = 50m;

			var doc3 = invoiceLine3.PreviousDocuments.AddNew();
			doc3.CSI_ItemNumber = 1;
			doc3.CSI_Code = FRConstants.PreviousDocuments.N337;
			doc3.CSI_ReferenceNumber = "FRJ_IST2";
			doc3.CSI_PackQty = 5;
			doc3.CSI_Quantity = 50m;
			doc3.CSI_UnitOfQuantity = "KGM";

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.FillWithValidTestData();
			invoiceLine4.JI_Weight = 80m;

			var doc4 = invoiceLine4.PreviousDocuments.AddNew();
			doc4.CSI_ItemNumber = 2;
			doc4.CSI_Code = FRConstants.PreviousDocuments.N337;
			doc4.CSI_ReferenceNumber = "FRJ_IST1";
			doc4.CSI_PackQty = 8;
			doc4.CSI_Quantity = 80m;
			doc4.CSI_UnitOfQuantity = "KGM";

			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine5.FillWithValidTestData();
			invoiceLine5.JI_Weight = 100m;

			var doc5 = invoiceLine5.PreviousDocuments.AddNew();
			doc5.CSI_ItemNumber = 1;
			doc5.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc5.CSI_ReferenceNumber = "FRJ_IST1";
			doc5.CSI_PackQty = 10;
			doc5.CSI_Quantity = 100m;
			doc5.CSI_UnitOfQuantity = "KGM";

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine1 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine1.PK;
			var cusEntryLine2 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine2.PK;
			var cusEntryLine3 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = cusEntryLine3.PK;
			var cusEntryLine4 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine4.JI_CL = cusEntryLine4.PK;
			var cusEntryLine5 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine5.JI_CL = cusEntryLine5.PK;

			cusEntryHeader.CH_BGMReference = "InternalReference";
			cusEntryHeader.EntryNumber = "CustomsReference";

			var transactionLines = cusEntryHeader.GetTemporaryStorageRegisterTransactionData().ToList();
			AssertEquals(@"First and second entry line should create a unique transaction as their previous IST document share same Reference Number and Line No.
Third entry line should create its own transaction because its previous IST document Reference Number is unique.
Fourth entry line should create its own transaction because its its previous IST document Line No is unique.
Fifth entry line should not create any transaction because it has no IST type of previous document.", 3, transactionLines.Count);

			CombineAssertions("Transaction values", () =>
			{
				AssertEquals("First transaction line RegisterHeader", ist1.RegisterHeader, transactionLines[0].PreviousRegisterHeader);
				AssertEquals("First transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[0].CustomsReferenceNumber);
				AssertEquals("First transaction line InternalReferenceNumber", "InternalReference", transactionLines[0].InternalReferenceNumber);
				AssertEquals("First transaction line PackageQuantity", 3, transactionLines[0].PackageQuantity);
				AssertEquals("First transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[0].ReferenceType);
				AssertEquals("First transaction line GrossMass", 30m, transactionLines[0].GrossMass);
				AssertEquals("First transaction line Comment", "Entry line 0", transactionLines[0].Comments);
				AssertEquals("First transaction line LineNo", 1, transactionLines[0].RegisterLineNo);

				AssertEquals("Second transaction line RegisterHeader", ist2.RegisterHeader, transactionLines[1].PreviousRegisterHeader);
				AssertEquals("Second transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[1].CustomsReferenceNumber);
				AssertEquals("Second transaction line InternalReferenceNumber", "InternalReference", transactionLines[1].InternalReferenceNumber);
				AssertEquals("Second transaction line PackageQuantity", 5, transactionLines[1].PackageQuantity);
				AssertEquals("Second transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[1].ReferenceType);
				AssertEquals("Second transaction line GrossMass", 50m, transactionLines[1].GrossMass);
				AssertEquals("Second transaction line Comment", "Entry line 0", transactionLines[1].Comments);
				AssertEquals("Second transaction line LineNo", 1, transactionLines[1].RegisterLineNo);

				AssertEquals("Third transaction line RegisterHeader", ist1.RegisterHeader, transactionLines[2].PreviousRegisterHeader);
				AssertEquals("Third transaction line CustomsReferenceNumber", "CustomsReference", transactionLines[2].CustomsReferenceNumber);
				AssertEquals("Third transaction line InternalReferenceNumber", "InternalReference", transactionLines[2].InternalReferenceNumber);
				AssertEquals("Third transaction line PackageQuantity", 8, transactionLines[2].PackageQuantity);
				AssertEquals("Third transaction line ReferenceType", TempStorageTransactionRefTypeList.Codes.EntryHeader, transactionLines[2].ReferenceType);
				AssertEquals("Third transaction line GrossMass", 80m, transactionLines[2].GrossMass);
				AssertEquals("Third transaction line Comment", "Entry line 0", transactionLines[2].Comments);
				AssertEquals("Third transaction line LineNo", 2, transactionLines[2].RegisterLineNo);
			});
		}

		public void TestGetTemporaryStorageRegisterTransactionDataPackageIsNotLinked()
		{
			var register = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			register.SRH_Reference = "DDT1";

			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ_IST1";
			ist.DDTNumber = "DDT1";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 69;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			var doc = invoiceLine.PreviousDocuments.AddNew();
			doc.CSI_LineNo = 1;
			doc.CSI_Code = "IST";
			doc.CSI_ReferenceNumber = "FRJ_IST1";
			invoiceLine.JI_Weight = 10m;

			var package = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var packing = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing.IsLinked = false;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine1 = cusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine1.PK;

			AssertNoExceptionThrown(() => { _ = cusEntryHeader.GetTemporaryStorageRegisterTransactionData().ToList(); });
		}

		public void TestConfirmedCusEntryHeaderChargesCollectionType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>>(entryHeader.ConfirmedCharges);
		}

		public override void TestGetTaxBoxSupporterList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = "5";
			var entry = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var charge1 = entry.ConfirmedCharges.AddNew();
			charge1.C1_ChargeType = "A1";
			charge1.C1_MethodOfPayment = "1";
			charge1.C1_ChargeAmount = 10.10m;
			var charge2 = entry.ConfirmedCharges.AddNew();
			charge2.C1_ChargeType = "A2";
			charge2.C1_MethodOfPayment = "2";
			charge2.C1_ChargeAmount = 20.20m;

			var taxBoxSupporters = entry.GetTaxBoxSupporterList().OrderBy(x => x.Type).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("2 taxes", 2, taxBoxSupporters.Length);
				AssertTaxBoxSupporter(taxBoxSupporters[0], "A1", "10", "1", "5");
				AssertTaxBoxSupporter(taxBoxSupporters[1], "A2", "20", "2", "5");
			});
		}

		void AssertTaxBoxSupporter(IDocSADHLineTaxBoxSupporter taxBoxSupporter, ZString type, ZString amount, ZString methodOfPayment, ZString declarationMethodOfPayment)
		{
			AssertEquals("Type", type, taxBoxSupporter.Type);
			AssertEquals("AmountInDeclarationCurrency", amount, taxBoxSupporter.AmountInDeclarationCurrency);
			AssertEquals("MethodOfPayment", methodOfPayment, taxBoxSupporter.MethodOfPayment);
			AssertEquals("DeclarationMethodOfPayment", declarationMethodOfPayment, taxBoxSupporter.DeclarationMethodOfPayment);
			AssertEquals("TaxBase", ZString.Empty, taxBoxSupporter.TaxBase);
			AssertEquals("Rate", ZString.Empty, taxBoxSupporter.Rate);
			AssertEquals("RateDuty", ZString.Empty, taxBoxSupporter.RateDuty);
			AssertEquals("RateOverride", ZString.Empty, taxBoxSupporter.RateOverride);
			AssertEquals("NationalFeeTypeCode", ZString.Empty, taxBoxSupporter.NationalFeeTypeCode);
		}

		public void TestGetTotalChargeValueFor_WithEntryHeaderCharge_CW1Fees()
		{
			AssertGetTotalChargeValueForWithEntryHeaderCharge("CW1 Charges", false, 111m, 111m, 222m);
		}

		public void TestGetTotalChargeValueFor_WithEntryHeaderCharge_CUSFees()
		{
			AssertGetTotalChargeValueForWithEntryHeaderCharge("CUS ComfirmedCharges", true, 111m, 111m, 222m);
		}

		public void TestGetTotalChargeValueFor_WithEntryHeaderCharge_MixedCW1AndCUSFees()
		{
			AssertGetTotalChargeValueForWithEntryHeaderCharge("CW1 Charges and CUS ComfirmedCharges", true, 111m, 111m, 222m, true);
		}

		public void TestGetTotalChargeValueFor_WithEntryHeaderCharge_MixedCW1AndCUSFees_IncludeEntryLineFee()
		{
			AssertGetTotalChargeValueForWithEntryHeaderCharge("CW1 Charges and CUS ComfirmedCharges", false, 10m, 10m, 0m, true);
		}

		void AssertGetTotalChargeValueForWithEntryHeaderCharge(ZString message, bool includeComfirmedCharges, ZDecimal expectedCharageforA00X, ZDecimal expectedCharageforA00, ZDecimal expectedCharageforB00, bool isMixedCharges = false)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var lvCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(lvCode, "Latvia");
			Factory.Save();

			var a00 = helper.CreateNewOrGetExistingRateType(lvCode, UniversalReferenceConstants.RefCusRateCodes.A00);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.A00, a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(lvCode, "B00");
			helper.LoadOrCreateNewCusRateCode(Factory, "B00", b00.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			if (isMixedCharges && !includeComfirmedCharges) // No entry confirmed charges but has entryLine ConfirmedFees
			{
				AddNewEntryHeaderCharge(entry, UniversalReferenceConstants.RefCusRateCodes.A00, "X", 111m, false);
				AddNewEntryHeaderCharge(entry, "B00", "DEF", 222m, false);
				AddNewEntryHeaderCharge(entry, "C00", "X", 555m, true);

				var entryLine = entry.MergedLines.AddNew();
				var fee = entryLine.ConfirmedFees.AddNew();
				fee.CF_MethodOfPayment = "X";
				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
				fee.CF_ChargeAmount = 10m;
			}
			else
			{
				AddNewEntryHeaderCharge(entry, UniversalReferenceConstants.RefCusRateCodes.A00, "X", 111m, false, includeComfirmedCharges);
				AddNewEntryHeaderCharge(entry, "B00", "DEF", 222m, false, includeComfirmedCharges);
				AddNewEntryHeaderCharge(entry, "C00", "X", 555m, true, includeComfirmedCharges);

				if (isMixedCharges)  // has entry confirmed charges with CW1 charge together
				{
					AddNewEntryHeaderCharge(entry, UniversalReferenceConstants.RefCusRateCodes.A00, "X", 111m, false);
					AddNewEntryHeaderCharge(entry, "B00", "DEF", 222m, false);
					AddNewEntryHeaderCharge(entry, "C00", "X", 555m, true);
				}
			}

			var a00Charge = new EntryChargeType(null, UniversalReferenceConstants.RefCusRateCodes.A00, "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");
			var c00Charge = new EntryChargeType(null, "C00", "", true, "");

			var customsChargeEntry = (ICustomsChargeEntry)entry;

			CombineAssertions(() =>
			{
				AssertEquals(message + "for A00 and methodPfPayment = X", expectedCharageforA00X, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "X"));
				AssertEquals(message + "for A00 and methodPfPayment = empty)", expectedCharageforA00, customsChargeEntry.GetTotalChargeValueFor(a00Charge, ""));
				AssertEquals(message + "for A00 and methodPfPayment = T)", 0m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "T"));
				AssertEquals(message + "for B00 DEF", expectedCharageforB00, customsChargeEntry.GetTotalChargeValueFor(b00Charge, "DEF"));
				AssertEquals(message + "for C00 X and landedCostOnly not included)", 0m, customsChargeEntry.GetTotalChargeValueFor(c00Charge, "X"));
			});
		}

		void AddNewEntryHeaderCharge(CusEntryHeader entryHeader, string feeType, string mop, decimal amount, bool isLandedCostOnly, bool confirmed = false)
		{
			var charge = confirmed ? entryHeader.ConfirmedCharges.AddNew() : entryHeader.Charges.AddNew();
			charge.C1_MethodOfPayment = mop;
			charge.C1_ChargeType = feeType;
			charge.C1_ChargeAmount = amount;
			charge.C1_IsLandedCostOnly = isLandedCostOnly;
		}

		public void TestSupplierEoriOfMainOffice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertEquals("Supplier not added yet.", ZString.Empty, entryHeader.SupplierEoriOfMainOffice);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.FillWithValidTestData();

			supplier.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, Factory.Load<RefCountry>(Core.Constants.CountryGuids.France), "123456789123");
			var suffix = supplier.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "1234", Enterprise.Core.Constants.CountryCodes.France);
			suffix.OK_OA_PremisesAddress = supplierAddress.PK;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;

			Factory.Save();
			AssertEquals("When EORI is not added.", ZString.Empty, entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567890", Enterprise.Core.Constants.CountryCodes.France);
			AssertEquals("Eori number should be supplier's suffixed depending address.", "FR12345678901234", entryHeader.SupplierEoriOfMainOffice);

			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR3";
			fiscalReference.CFR_Reference = "FR33562024100133";
			var euAddInfo = EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();
			AssertEquals(true, entryHeader.SupplierUsesFiscalRepresentative);
			AssertEquals("Eori number should come from fiscal reference of type FR3 set in entry instruction.", "FR33562024100133", entryHeader.SupplierEoriOfMainOffice);
		}

		public void TestImporterEoriOfMainOffice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertEquals("Importer not added yet.", ZString.Empty, entryHeader.ImporterEoriOfMainOffice);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var importerAddress = importer.Addresses.AddNew();
			importerAddress.FillWithValidTestData();

			importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, Factory.Load<RefCountry>(Core.Constants.CountryGuids.France), "123456789123");
			var suffix = importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "1234", Enterprise.Core.Constants.CountryCodes.France);
			suffix.OK_OA_PremisesAddress = importerAddress.PK;

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			Factory.Save();
			AssertEquals("When EORI is not added.", ZString.Empty, entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567890", Enterprise.Core.Constants.CountryCodes.France);
			AssertEquals("Eori number should be importer's suffixed depending address.", "FR12345678901234", entryHeader.ImporterEoriOfMainOffice);

			var euAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();
			AssertEquals(true, entryHeader.ImporterUsesFiscalRepresentative);
			AssertEquals("Eori number should be importer's.", "FR12345678901234", entryHeader.ImporterEoriOfMainOffice);
		}

		public override void TestRepresentativeOrDeclarantEoriOfMainOffice()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();

			var declarant = Factory.New<OrgHeader>();
			var importerAddress = declarant.Addresses.AddNew();
			importerAddress.FillWithValidTestData();

			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567890", Enterprise.Core.Constants.CountryCodes.France);
			var suffix = declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "1234", Enterprise.Core.Constants.CountryCodes.France);
			suffix.OK_OA_PremisesAddress = importerAddress.PK;

			declaration.JE_OA_DeclarantAddress = importerAddress.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Eori number should be declarant's suffixed per address", "FR12345678901234", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
		}

		public void TestSupplierUsesFiscalRepresentative()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals(false, entryHeader.SupplierUsesFiscalRepresentative);

			var euAddInfo = EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();
			AssertEquals(true, entryHeader.SupplierUsesFiscalRepresentative);
		}

		public void TestImporterUsesFiscalRepresentative()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals(false, entryHeader.ImporterUsesFiscalRepresentative);

			var euAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();
			AssertEquals(true, entryHeader.ImporterUsesFiscalRepresentative);
		}

		public void TestGetEntrySnapshot()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();

			var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
			invoiceSupportingDocument.CSI_Code = "HDOC";
			invoiceSupportingDocument.CSI_ReferenceNumber = "INVOICEHEADER_DOCUMENT";
			invoiceSupportingDocument.CSI_DateOfIssue = ZDate.Today;

			var invoiceLineSupportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSupportingDocument1.CSI_Code = "LDOC";
			invoiceLineSupportingDocument1.CSI_ReferenceNumber = "INVOICELINE_DOCUMENT";
			invoiceLineSupportingDocument1.CSI_DateOfIssue = ZDate.Today;

			var invoiceLineDTP = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineDTP.CSI_Code = "LDTP";
			invoiceLineDTP.CSI_ReferenceNumber = "";
			invoiceLineDTP.CSI_IsDTP = true;

			invoiceLine.JI_SupplementaryCode1 = "CAC1";
			invoiceLine.JI_SupplementaryCode2 = "CAC2";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V910";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V911";

			Factory.Save();

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var expectedSnapshotXml = @"<CusEntryLine><LineNumber>1</LineNumber><ChildData Type=""DOC""><Code>HDOC</Code><Reference>INVOICEHEADER_DOCUMENT</Reference></ChildData><ChildData Type=""DOC""><Code>LDOC</Code><Reference>INVOICELINE_DOCUMENT</Reference></ChildData><ChildData Type=""DOC""><Code>LDTP</Code></ChildData><ChildData Type=""CAN""><Code>V910</Code></ChildData><ChildData Type=""CAN""><Code>V911</Code></ChildData><ChildData Type=""CAC""><Code>CAC1</Code></ChildData><ChildData Type=""CAC""><Code>CAC2</Code></ChildData></CusEntryLine></FrenchEntryLineChildSnapshot>";
			AssertContains(expectedSnapshotXml, entryHeader.GetEntrySnapshotData());
		}

		public override void TestOfficeOfEntry()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfEntry);
			AssertEquals("LV002000", entry.OfficeOfEntry);
		}

		[TestDate(2023, 12, 24)]
		public void TestLastNonIntermediateEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals(ZString.Empty, entryHeader.LastNonIntermediateEntryStatus);

			var logEntryForEAV = entryHeader.Logs.AddNew();
			using (logEntryForEAV.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForEAV.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForEAV.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES055;
				logEntryForEAV.SL_EventTime = ZDate.Today;
			}

			var logEntryForBAE = entryHeader.Logs.AddNew();
			using (logEntryForBAE.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForBAE.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForBAE.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES100;
				logEntryForBAE.SL_EventTime = ZDate.Today.AddDays(1);
			}

			var logEntryVerboseStatus = entryHeader.Logs.AddNew();
			using (logEntryVerboseStatus.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryVerboseStatus.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryVerboseStatus.SL_Reference = "Updating held entry";
				logEntryVerboseStatus.SL_EventTime = ZDate.Today.AddDays(2);
			}

			var logEntryForDEA = entryHeader.Logs.AddNew();
			using (logEntryForDEA.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForDEA.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForDEA.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES114;
				logEntryForDEA.SL_EventTime = ZDate.Today.AddDays(3);
			}

			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES100, entryHeader.LastNonIntermediateEntryStatus);
		}

		public void TestShouldLogEntryStatus()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			Assert(entryHeader.ShouldLogEntryStatus);
		}

		public void TestTestIsCustomsNumberEnteredEventSupported()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert(entryHeader.IsCustomsNumberEnteredEventSupported_Exposed);
		}

		public void TestIsCustomsReleaseNumberEnteredEventSupported()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert(entryHeader.IsCustomsReleaseNumberEnteredEventSupported_Exposed);
		}

		public void TestShouldLogCustomsReleaseNumberEnteredEvent()
		{
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			Assert("False as CH_EntryStatus is empty.", !entryHeader.ShouldLogCustomsReleaseNumberEnteredEvent_Exposed);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			Assert("True as CH_EntryStatus is 100 and no CRN event exists.", entryHeader.ShouldLogCustomsReleaseNumberEnteredEvent_Exposed);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES101;
			Assert("True as CH_EntryStatus is 101 and no CRN event exists.", entryHeader.ShouldLogCustomsReleaseNumberEnteredEvent_Exposed);
		}

		public void TestCustomsPackageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals(10m, entryHeader.CustomsPackageCount);
		}

		public void TestCH_Status_ReCalculateStatusDetails_CH_EntryStatus_AwaitingResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES010, entryHeader.CH_EntryStatus);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = MessageStatusList.Codes.AwaitingResponse;
			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingResponse;

			CombineAssertions(() =>
			{
				AssertEquals("An UCC6 Entry Status should be empty when the entry is waiting for Customs response.", "", entryHeader2.CH_EntryStatus);
			});
		}

		public void TestSupportModificationState()
		{
			AssertEquals(true, ((Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter)Factory.New<CusEntryHeader>()).SupportModificationState);
		}

		public void TestGetPermitRecords()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			helper.CreateTaxOrFee("ZZ2", 20.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.VAT, toDay.AddDays(-1), toDay.AddDays(1), "Test VAT");
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeHeader1.CPH_Number = "0000001";
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "0000001";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 100m;

			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.NationalFeeTypeCode = "ZZ2";
			fee2.CF_ChargeAmount = 200m;

			var permitRecords = cusEntryHeader.GetPermitRecords();
			AssertEquals(1, permitRecords.Count);

			var record = permitRecords.FirstOrDefault();
			AssertEquals(declaration.Ai2Permit, record.PermitHeader);
			AssertEquals(300m, record.Value);
			AssertEquals(ZDecimal.Zero, record.Quantity);
		}

		public void TestAddFallbackSpecialMention()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fallbackSetting = new FallbackSettings();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader2.CH_BGMReference = "19212081312";

			var cusEntryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine2.PK;
			cusEntryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.AddFallbackSpecialMention();
			Factory.Save();
			Assert("Declaration should have a special mention 50000.", declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours));

			cusEntryHeader.AddFallbackSpecialMention();
			Factory.Save();
			AssertEquals("Declaration should have only one special mention 50000.", 1, declaration.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours));

			cusEntryHeader2.RollbackFallbackSpecialMention(false);
			Factory.Save();
			Assert("Declaration should still have a special mention 50000.", declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours));

			cusEntryHeader2.RollbackFallbackSpecialMention(true);
			Factory.Save();
			Assert("Declaration should have its special mention 50000 removed.", !declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours));
		}

		public void TestInitFRCustomsFallbackEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fallbackSetting = new FallbackSettings();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader2.CH_BGMReference = "19212081312";

			var cusEntryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine2.PK;
			cusEntryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.InitFRCustomsFallbackEntryNumber();
			Factory.Save();

			Assert("cusEntryHeader should have a fallback entry number.", cusEntryHeader.FRCustomsFallbackNumber == "0000000001");
			Assert("cusEntryHeader2 should not have a fallback entry number.", cusEntryHeader2.FRCustomsFallbackNumber.IsEmpty);

			cusEntryHeader2.InitFRCustomsFallbackEntryNumber();
			Factory.Save();

			Assert("cusEntryHeader2 should now have a fallback entry number.", cusEntryHeader2.FRCustomsFallbackNumber == "0000000002");
			Assert("cusEntryHeader and cusEntryHeader2 should have distinct fallback entry numbers.", cusEntryHeader.FRCustomsFallbackNumber != cusEntryHeader2.FRCustomsFallbackNumber);
		}

		public void TestCorrelationID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			Assert(cusEntryHeader1.CorrelationID != cusEntryHeader2.CorrelationID);
			AssertType<ZString>(cusEntryHeader1.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "WTL"))
			{
				Env.Registry.PhysicalServerID = "FRM";
				GlbCompany.CurrentCompany.GC_Code = "DFR";

				var deltaGDeclaration = Factory.New<JobDeclaration>();
				deltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				Assert("Prerequisite: deltaGDeclaration is DeltaG", deltaGDeclaration.IsDeltaG);
				var deltaGEntry = deltaGDeclaration.CustomsEntryHeaders.AddNew();
				AssertEquals("DeltaG entries should have CorrelationIDPrefix empty.", ZString.Empty, deltaGEntry.CorrelationIDPrefix);

				var nonDeltaGDeclaration = Factory.New<JobDeclaration>();
				nonDeltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				Assert("Prerequisite: nonDeltaGDeclaration is not DeltaG", !nonDeltaGDeclaration.IsDeltaG);
				var nonDeltaGEntry = nonDeltaGDeclaration.CustomsEntryHeaders.AddNew();
				AssertEquals("Non DeltaG entries CorrelationIDPrefix should be made of Enterprise code + Company code + PhysicalServerID.", "WTLDFRFRM", nonDeltaGEntry.CorrelationIDPrefix);
			}
		}

		public void TestCIN745()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 12;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 12;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "TEST BGM";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			declaration.CustomsOffices.RemoveAndDeleteAll();

			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = "EXT";
			customsOfficeExit.CY_Data = "OfficeExit";

			declaration.JE_TotalNoOfPacks = 12;

			OrgHeader testSupplier = Factory.NewWithValidTestData<OrgHeader>();

			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";

			OrgHeader testCarrier = Factory.NewWithValidTestData<OrgHeader>();

			testCarrier.OH_FullName = "London Test Supplier DHL Ltd.";
			testCarrier.MainAddress.OA_Address1 = "Heathrow Airport";
			testCarrier.MainAddress.OA_Address2 = "Building 1B";

			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_ShippingLine = testCarrier.PK;

			testSupplier.CustomsCodes.AddNew(CusEntryHeader.Schema.CINOACICode, "TESTOAC SUPP");
			testCarrier.CustomsCodes.AddNew(CusEntryHeader.Schema.CINOACICode, "TESTOAC CARR");

			ICINMessage745 iCusEntryHeader = cusEntryHeader;
			var testMR = (CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France)).CE_EntryNum;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(12, iCusEntryHeader.MrnQuantity);
			AssertEquals(24m, iCusEntryHeader.MrnWeight);
			AssertEquals("TEST BGM", iCusEntryHeader.Name);
			AssertEquals("REF", iCusEntryHeader.Level);
			AssertEquals("OfficeExit", iCusEntryHeader.ExitOfOffice);
			AssertEquals(testMR, iCusEntryHeader.MrnNumber);
			AssertEquals("TESTOAC SUPP", iCusEntryHeader.OACI_Shipper);
			AssertEquals("TESTOAC CARR", iCusEntryHeader.OACI_Carrier);

			declaration.JE_HouseBill = "TEST HOUSE";
			AssertEquals("TEST HOUSE", iCusEntryHeader.Name);
			AssertEquals("HWB", iCusEntryHeader.Level);

			declaration.JE_MasterBill = "TEST MASTER";
			AssertEquals("TEST MASTER", iCusEntryHeader.Name);
			AssertEquals("AWB", iCusEntryHeader.Level);
		}

		[TestDate(2020, 04, 24)]
		public void TestCIN755()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "88883013825";
			declaration.JE_HouseBill = "AFCDG2019F128796";

			declaration.CustomsOffices.RemoveAndDeleteAll();
			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = "EXT";
			customsOfficeExit.CY_Data = "OfficeExit";

			OrgHeader testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";

			OrgHeader testCarrier = Factory.NewWithValidTestData<OrgHeader>();
			testCarrier.OH_FullName = "London Test Supplier DHL Ltd.";
			testCarrier.MainAddress.OA_Address1 = "Heathrow Airport";
			testCarrier.MainAddress.OA_Address2 = "Building 1B";

			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_ShippingLine = testCarrier.PK;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "PK";

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "19212081311";
			CusEntryNumber cusEntryNumber = CusEntryNumber.New(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber.CE_EntryNum = "123";
			cusEntryNumber.CE_IssueDate = ZDateTime.Now;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 5;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 15;

			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT");
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "X1Z";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";

			ICINMessage755 iCusEntryHeader = cusEntryHeader;
			declaration.JE_SubLocationOfGoods = "SUB";
			AssertEquals("200424#ID_MESSAGE#", iCusEntryHeader.UniqueMessageNumber);
			AssertContains("115#ID_MESSAGE#", iCusEntryHeader.BGMReference);
			AssertContains("20200424120000", iCusEntryHeader.Date);
			AssertEquals("88883013825", iCusEntryHeader.NumLTA);
			AssertEquals("CIN-SENDER-UT", iCusEntryHeader.OACI_Shipper);
			AssertEquals("888", iCusEntryHeader.OACI_Carrier);
			AssertEquals("19212081311", iCusEntryHeader.RefDos);
			AssertEquals("AFCDG2019F128796", iCusEntryHeader.Hwb);
			AssertEquals("SUB", iCusEntryHeader.StoreCode);
			AssertEquals("20", iCusEntryHeader.PackagesCount);
			AssertEquals("10", iCusEntryHeader.GrossWeight);
			AssertEquals("123", iCusEntryHeader.MrnNumber);
			AssertEquals("OfficeExit", iCusEntryHeader.CustomOffice);
			AssertEquals("20200424", iCusEntryHeader.BAEDate);

			declaration.JE_MasterBill = "X1Z83013825";
			AssertEquals("X1Z83013825", iCusEntryHeader.NumLTA);
			AssertEquals("AAA", iCusEntryHeader.OACI_Carrier);
		}

		public void TestStatusDescriptionUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			var entry = Factory.New<CusEntryHeader>();
			entry.SetDeclarationForTesting(declaration);
			entry.CH_Status = MessageStatusCodeList.Codes.NACK;
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			CombineAssertions(() =>
			{
				AssertEquals("Entry status description should be human readable (BAE in this case)", EntryStatusDescriptionCodeList.Descriptions.ES100, entry.EntryHeaderStatusDescription);
				AssertEquals("Message status should be human readable (Not acknowledged)", MessageStatusCodeList.Descriptions.NACK, entry.MessageStatusDescription);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Entry status description should be empty", "", entry.EntryHeaderStatusDescription);

			entry.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated;
			AssertEquals("Entry status description should be Declaration Accepted (MRN Allocated)", DeltaIEImportCusEntryStatusList.Descriptions.DeclarationAcceptedMrnAllocated, entry.EntryHeaderStatusDescription);
		}

		public void TestEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertType<CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
		}

		public override void TestAdditionalInfos()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var instruction = dec.CustomsEntryInstructions.AddNew();
			invLine1.JI_CEI = instruction.PK;

			var addInfo1 = inv.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "9002";
			addInfo1.CSI_Description = "9002 Desc";

			var addInfo2 = inv.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "9002";
			addInfo2.CSI_Description = "9002 Desc";

			var addInfo3 = inv.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "9003";
			addInfo3.CSI_Description = "9003 Desc";

			var addinfo4 = instruction.AdditionalInfos.AddNew();
			addinfo4.CSI_Code = "9004";
			addinfo4.CSI_Description = "9004 Desc";

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("ActiveEntryHeaders", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("AdditionalInfos", 3, ((CusEntryHeader)dec.ActiveEntryHeaders[0]).AdditionalInfos.Count());
		}

		public void TestSupportingDocument()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var instruction = dec.CustomsEntryInstructions.AddNew();
			invLine1.JI_CEI = instruction.PK;

			var supportingDocument1 = inv.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "9001";
			supportingDocument1.CSI_Description = "9001 Desc";

			var supportingDocument2 = inv.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "9002";
			supportingDocument2.CSI_Description = "9002 Desc";

			var supportingDocument3 = inv.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "9003";
			supportingDocument3.CSI_Description = "9003 Desc";

			var supportingDocument4 = instruction.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "9004";
			supportingDocument4.CSI_Description = "9004 Desc";

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("ActiveEntryHeaders", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("SupportingDocument", 4, ((CusEntryHeader)dec.ActiveEntryHeaders[0]).SupportingDocuments.Count());
		}

		public void TestPreviousDocument()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var instruction = dec.CustomsEntryInstructions.AddNew();
			invLine1.JI_CEI = instruction.PK;

			var previousDocuments1 = inv.PreviousDocuments.AddNew();
			previousDocuments1.CSI_Code = "9001";
			previousDocuments1.CSI_Description = "9001 Desc";

			var previousDocuments2 = inv.PreviousDocuments.AddNew();
			previousDocuments2.CSI_Code = "9002";
			previousDocuments2.CSI_Description = "9002 Desc";

			var previousDocuments3 = inv.PreviousDocuments.AddNew();
			previousDocuments3.CSI_Code = "9003";
			previousDocuments3.CSI_Description = "9003 Desc";

			var previousDocuments4 = instruction.PreviousDocuments.AddNew();
			previousDocuments4.CSI_Code = "9004";
			previousDocuments4.CSI_Description = "9004 Desc";

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("ActiveEntryHeaders", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("PreviousDocuments", 4, ((CusEntryHeader)dec.ActiveEntryHeaders[0]).PreviousDocuments.Count());
		}

		public void TestAdditionalInfos_WhenDeclarationIsNonUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			SetUpDataForAdditionalInfosTest(declaration);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertContainsExactElementsInAnyOrder(new ZString[] { "MIS1", "MIS2", "MIS3" }, entryHeader.AdditionalInfos.Select(x => x.CSI_Code));
		}

		public void TestAdditionalInfos_WhenDeclarationIsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			SetUpDataForAdditionalInfosTest(declaration);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertContainsExactElementsInAnyOrder(new ZString[] { "MIS1", "MIS2", "MIS3", "INV1", "INV2", "INV3" }, entryHeader.AdditionalInfos.Select(x => x.CSI_Code));
		}

		internal static void SetUpDataForAdditionalInfosTest(JobDeclaration declaration)
		{
			var miscAddInfoINF = declaration.AdditionalInfos.AddNew();
			miscAddInfoINF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			miscAddInfoINF.CSI_Code = "MIS1";
			miscAddInfoINF.CSI_ReferenceNumber = "MiscINFReference";
			var miscAddInfoREF = declaration.AdditionalInfos.AddNew();
			miscAddInfoREF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			miscAddInfoREF.CSI_Code = "MIS2";
			miscAddInfoREF.CSI_ReferenceNumber = "REFReference";
			var miscAddInfoTRA = declaration.AdditionalInfos.AddNew();
			miscAddInfoTRA.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			miscAddInfoTRA.CSI_Code = "MIS3";
			miscAddInfoTRA.CSI_ReferenceNumber = "TRAReference";

			var invoice = declaration.Invoices.AddNew();
			var invoiceAddInfoINF = invoice.AdditionalInfos.AddNew();
			invoiceAddInfoINF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			invoiceAddInfoINF.CSI_Code = "INV1";
			invoiceAddInfoINF.CSI_ReferenceNumber = "INFReference";
			var invoiceAddInfoREF = invoice.AdditionalInfos.AddNew();
			invoiceAddInfoREF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			invoiceAddInfoREF.CSI_Code = "INV2";
			invoiceAddInfoREF.CSI_ReferenceNumber = "REFReference";
			var invoiceAddInfoTRA = invoice.AdditionalInfos.AddNew();
			invoiceAddInfoTRA.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceAddInfoTRA.CSI_Code = "INV3";
			invoiceAddInfoTRA.CSI_ReferenceNumber = "TRAReference";

			var line = invoice.InvoiceLines.AddNew();
			var lineAddInfoINF = line.AdditionalInfos.AddNew();
			lineAddInfoINF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			lineAddInfoINF.CSI_Code = "LIN1";
			lineAddInfoINF.CSI_ReferenceNumber = "INFReference";
			var lineAddInfoREF = line.AdditionalInfos.AddNew();
			lineAddInfoREF.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			lineAddInfoREF.CSI_Code = "LIN2";
			lineAddInfoREF.CSI_ReferenceNumber = "REFReference";
			var lineAddInfoTRA = line.AdditionalInfos.AddNew();
			lineAddInfoTRA.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			lineAddInfoTRA.CSI_Code = "LIN3";
			lineAddInfoTRA.CSI_ReferenceNumber = "TRAReference";

			declaration.Factory.Save();
		}

		public override void TestDuty()
		{
			var entryHeader = SetupEntryHeader();
			AssertEquals("IntegerFeeRounder(20.5)+IntegerFeeRounder(2.5)", 24m, entryHeader.Duty);
		}

		public override void TestVAT()
		{
			var entryHeader = SetupEntryHeader();
			AssertEquals("IntegerFeeRounder(10.5)+IntegerFeeRounder(1.5)", 13m, entryHeader.VAT);
		}

		CusEntryHeader SetupEntryHeader()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(entryHeader.TaxCode, 10.5m);
			entryLine.Fees.AddOrUpdate(entryHeader.DutyCode, 20.5m);
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(entryHeader.TaxCode, 1.5m);
			entryLine2.Fees.AddOrUpdate(entryHeader.DutyCode, 2.5m);

			return entryHeader;
		}

		public void TestChargesAsTaxes()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "ABC";
			charge1.C1_ChargeAmount = 2.5;
			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeType = "DEF";
			charge2.C1_ChargeAmount = 6.7;

			var taxes = entryHeader.ChargesAsTaxes();
			CombineAssertions(() =>
			{
				AssertEquals(2, taxes.Count);
				AssertEquals("ABC", taxes[0].G4_Type);
				AssertEquals("2.00", taxes[0].G4_Amount_InDeclarationCurrency);
				AssertEquals("DEF", taxes[1].G4_Type);
				AssertEquals("7.00", taxes[1].G4_Amount_InDeclarationCurrency);
			});
		}

		public void TestIsDeltaDStepOneSentOK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			var entry = Factory.New<CusEntryHeader>();
			entry.SetDeclarationForTesting(declaration);
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			CombineAssertions(() =>
			{
				AssertEquals("G2 and ES100", true, entry.IsDeltaDStepOneSentOK);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertEquals("G1 and ES100", false, entry.IsDeltaDStepOneSentOK);

				entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("G2 and ES120", false, entry.IsDeltaDStepOneSentOK);
			});
		}

		public void TestIsDeltaDStepTwoSentOK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			var entry = Factory.New<CusEntryHeader>();
			entry.SetDeclarationForTesting(declaration);
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			CombineAssertions(() =>
			{
				AssertEquals("G2 and ES120", true, entry.IsDeltaDStepTwoSentOK);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertEquals("G1 and ES120", false, entry.IsDeltaDStepTwoSentOK);

				entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("G2 and ES100", false, entry.IsDeltaDStepTwoSentOK);
			});
		}

		public void TestIsDeltaDStepTwoSentOKButZeroLiquidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var entry1 = Factory.New<CusEntryHeader>();
			entry1.SetDeclarationForTesting(declaration);
			entry1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;

			var entry2 = Factory.New<CusEntryHeader>();
			entry2.SetDeclarationForTesting(declaration);
			entry2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var cusEntryLine2 = entry2.MergedLines.AddNew();
			var fee1 = cusEntryLine2.ConfirmedFees.AddNew();
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			fee1.CF_ChargeAmount = 0m;

			var entry3 = Factory.New<CusEntryHeader>();
			entry3.SetDeclarationForTesting(declaration);
			entry3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var cusEntryLine3 = entry3.MergedLines.AddNew();
			var fee2 = cusEntryLine3.ConfirmedFees.AddNew();
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			fee2.CF_ChargeAmount = 50m;

			var entry4 = Factory.New<CusEntryHeader>();
			entry4.SetDeclarationForTesting(declaration);
			entry4.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var cusEntryLine4 = entry4.MergedLines.AddNew();
			var fee3 = cusEntryLine4.Fees.AddNew();
			fee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			fee3.CF_ChargeAmount = 50m;

			CombineAssertions(() =>
			{
				AssertEquals("Valid Step 2 and Zero Liquidation", true, entry1.IsDeltaDStepTwoSentOKButZeroLiquidation);
				AssertEquals("Valid Step 2 and Zero Liquidation, because €0 will NOT be rounded up to amount €1.", true, entry2.IsDeltaDStepTwoSentOKButZeroLiquidation);
				AssertEquals("Valid Step 2 and €50 ConfirmedFees Liquidation", false, entry3.IsDeltaDStepTwoSentOKButZeroLiquidation);
				AssertEquals("Valid Step 2 and €50 Fees Liquidation", false, entry4.IsDeltaDStepTwoSentOKButZeroLiquidation);
			});
		}

		public void TestIsDeltaGFallbackManualRegularisation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = Factory.New<CusEntryHeader>();
			entry.SetDeclarationForTesting(declaration);

			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);

			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			entry.DeltaGFallbackStatus = "PDS";
			Assert(entry.IsDeltaGFallbackInactiveAndNotRegularised);

			entry.DeltaGFallbackStatus = "RGA";
			Assert(!entry.IsDeltaGFallbackInactiveAndNotRegularised);

			entry.DeltaGFallbackStatus = "";
			Assert(!entry.IsDeltaGFallbackInactiveAndNotRegularised);

			entry.DeltaGFallbackStatus = "PDS";
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			Assert(!entry.IsDeltaGFallbackInactiveAndNotRegularised);
		}

		public void TestIsAwaitingResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.SetDeclarationForTesting(declaration);

			entryHeader.CH_Status = MessageStatusCodeList.Codes.AWR;
			Assert(entryHeader.IsAwaitingResponse);
		}

		public void TestSadBoxAText()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR004000", "bureau", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR003000", "desc", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cauOffice = declaration.CustomsOffices.Cast<EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "CAU") ?? declaration.CustomsOffices.AddNew();
			cauOffice.CY_Type = "EUO";
			cauOffice.CY_Code = "CAU";
			cauOffice.CY_Data = "FR003000";
			AssertEquals("desc", cauOffice.CY_OfficeDescription);
			Factory.Save();

			declaration.JE_CustomsOffice = "FR004000";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			var fallbackSetting = new FallbackSettings();

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.FRCustomsFallbackNumber = "0000000001";
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			Factory.Save();

			AssertEquals(@"Etat de la declaration: BAE
Bureau de présentation: FR004000 - bureau
Bureau de declaration: FR003000 - desc
No douane: 0000000001", cusEntryHeader.SadBoxAText);

			cauOffice.CY_Code = "OTH";
			Factory.Save();

			AssertEquals(@"Etat de la declaration: BAE
Bureau de présentation: FR004000 - bureau
No douane: 0000000001", cusEntryHeader.SadBoxAText);

			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(+2);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(@"Etat de la declaration: BAE
Bureau de présentation: FR004000 - bureau", cusEntryHeader.SadBoxAText);

			cusEntryHeader.CH_ExitedStatus = ExportControlStatusList.Codes.EBL;
			AssertContains("Exit status should show in description.", "Etat de la declaration: BAE/EBL", cusEntryHeader.SadBoxAText);

			cusEntryHeader.CH_ExitedStatus = ExportControlStatusList.Codes.APE;
			AssertContains("APE exit status should show in description as ESO-PA.", "Etat de la declaration: BAE/ESO-PA", cusEntryHeader.SadBoxAText);
		}

		public void TestAi2Amount()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			helper.CreateTaxOrFee("ZZ2", 20.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.VAT, toDay.AddDays(-1), toDay.AddDays(1), "Test VAT");
			helper.CreateTaxOrFee("ZZ3", 30.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, toDay.AddDays(-1), toDay.AddDays(1), "Test DTY");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 100m;

			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.NationalFeeTypeCode = "ZZ2";
			fee2.CF_ChargeAmount = 200m;

			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.NationalFeeTypeCode = "ZZ3";
			fee3.CF_ChargeAmount = 400m;

			var fee4 = cusEntryLine.Fees.AddNew();
			fee4.NationalFeeTypeCode = "";
			fee4.CF_ChargeAmount = 1m;

			AssertEquals(0m, cusEntryHeader.Ai2Amount);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			AssertEquals(300m, cusEntryHeader.Ai2Amount);
		}

		public void TestTotalD48Amount()
		{
			var entry = SetupForTotalD48Amount();

			Factory.Save();

			AssertEquals(40m, entry.TotalD48Amount);
		}

		public void TestTotalD48AmountAtHeaderLevel()
		{
			var entry = SetupForTotalD48AmountAtHeaderLevel();

			Factory.Save();

			AssertEquals(50m, entry.TotalD48Amount);
		}

		public void TestTotalD48AmountAtAllLevels()
		{
			var entry = SetupForTotalD48AmountWithDocumentsSetAtAllLevels();

			Factory.Save();

			AssertEquals(70m, entry.TotalD48Amount);
		}

		public override void TestAmountAndTypeToBeGuaranteeds()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Core.Constants.CountryCodes.France;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			Factory.Save();

			var entry = SetupForTotalD48Amount();
			AssertEquals(true, entry.HasConsumingGuaranteeProcedure);

			var entryLine1 = entry.AllEntryLines[0];
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			var entryLine2 = entry.AllEntryLines[1];
			var fee3 = entryLine2.Fees.AddNew();
			fee3.CF_ChargeAmount = 30;

			AssertEquals(2, entry.AmountAndTypeToBeGuaranteeds.Count());
			AssertEquals(GuaranteeDebitType.NORMAL, entry.AmountAndTypeToBeGuaranteeds.ToArray()[0].DebitType);
			AssertEquals(30m, entry.AmountAndTypeToBeGuaranteeds.ToArray()[0].AmountInDeclarationCurrency);
			AssertEquals(GuaranteeDebitType.SUSPENDED, entry.AmountAndTypeToBeGuaranteeds.ToArray()[1].DebitType);
			AssertEquals(40m, entry.AmountAndTypeToBeGuaranteeds.ToArray()[1].AmountInDeclarationCurrency);
		}

		public void TestIsGuaranteeConsumedCore()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";
			var entry = SetupForTotalD48Amount();
			Assert(entry.IsGuaranteeConsumed);
			AssertEquals(entry.IsGuaranteeConsumed, entry.HasConsumingGuaranteeProcedure);

			foreach (JobComInvoiceLine invLine in entry.Declaration.InvoiceLines)
			{
				invLine.JI_Procedure = "Ye12344";
			}
			Assert(!entry.IsGuaranteeConsumed);
			AssertEquals(entry.IsGuaranteeConsumed, entry.HasConsumingGuaranteeProcedure);
		}

		public void TestIsBAE()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = "100";
			Assert(entryHeader.IsBAE);

			entryHeader.CH_EntryStatus = "130";
			Assert(!entryHeader.IsBAE);
		}

		public void TestIsVALOrBAEOrComplete()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			Assert(entryHeader.IsVALOrBAEOrComplete);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			Assert(entryHeader.IsVALOrBAEOrComplete);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			Assert(entryHeader.IsVALOrBAEOrComplete);

			entryHeader.CH_EntryStatus = "081";
			Assert(!entryHeader.IsVALOrBAEOrComplete);
		}

		public void TestGetMethodsOfPaymentThatCanInfluenceAutoRatingCore()
		{
			var ceh = (ICustomsChargeEntry)Factory.New<CusEntryHeader>();
			var codes = ceh.GetMethodsOfPaymentThatCanInfluenceAutoRating();
			AssertEquals("When no FR data in RefZZ, use default string", 3, codes.Length);
			AssertEquals("1", codes[0]);
			AssertEquals("2", codes[1]);
			AssertEquals("6", codes[2]);

			var newFactory = new BusinessObjectFactory();
			CreateRatingRefZZRecords(newFactory);

			ceh = newFactory.New<CusEntryHeader>();
			codes = ceh.GetMethodsOfPaymentThatCanInfluenceAutoRating();
			AssertEquals("When has FR data in RefZZ, still use default string", 3, codes.Length);
			AssertEquals("1", codes[0]);
			AssertEquals("2", codes[1]);
			AssertEquals("6", codes[2]);
		}

		public class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public new ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPaymentCode) => base.GetTotalChargeValueFor(chargeTypeElement, methodOfPaymentCode);

			public new ZBool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => base.IsStatusChangingToCleared(originalStatus, newStatus);

			public new ZBool IsChangingToClearStatusForAccIntegration => base.IsChangingToClearStatusForAccIntegration;

			public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

			public bool IsCustomsNumberEnteredEventSupported_Exposed => this.IsCustomsNumberEnteredEventSupported;

			public bool IsCustomsReleaseNumberEnteredEventSupported_Exposed => this.IsCustomsReleaseNumberEnteredEventSupported;

			public bool ShouldLogCustomsReleaseNumberEnteredEvent_Exposed => this.ShouldLogCustomsReleaseNumberEnteredEvent;
		}

		public void TestGetAmountsForDifferentMethodsOfPayment()
		{
			CreateRatingRefZZRecords(Factory);

			var ceh = Factory.New<CusEntryHeaderForTest>();
			var entryLine1 = ceh.MergedLines.AddNew();
			var entryLine2 = ceh.MergedLines.AddNew();

			var fee1A00X = AddNewFee(entryLine1, UniversalReferenceConstants.RefCusRateCodes.A00, "X", 111m);
			var fee1A00Y = AddNewFee(entryLine1, UniversalReferenceConstants.RefCusRateCodes.A00, "Y", 222m);
			var fee1B00X = AddNewFee(entryLine1, "B00", "X", 333m);
			var fee1B00Y = AddNewFee(entryLine1, "B00", "Y", 444m);
			var fee2A00X = AddNewFee(entryLine2, UniversalReferenceConstants.RefCusRateCodes.A00, "X", 1000m);
			var fee2A00Y = AddNewFee(entryLine2, UniversalReferenceConstants.RefCusRateCodes.A00, "Y", 2000m);
			var fee2B00X = AddNewFee(entryLine2, "B00", "X", 3000m);
			var fee2B00Y = AddNewFee(entryLine2, "B00", "Y", 4000m);

			var a00Charge = new EntryChargeType(null, UniversalReferenceConstants.RefCusRateCodes.A00, "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");
			AssertEquals(111m + 1000m, ceh.GetTotalChargeValueFor(a00Charge, "X"));
			AssertEquals(222m + 2000m, ceh.GetTotalChargeValueFor(a00Charge, "Y"));
			AssertEquals(333m + 3000m, ceh.GetTotalChargeValueFor(b00Charge, "X"));
			AssertEquals(444m + 4000m, ceh.GetTotalChargeValueFor(b00Charge, "Y"));

			AssertEquals(111m + 1000m + 222m + 2000m, ceh.GetTotalChargeValueFor(a00Charge, ""));
			AssertEquals(333m + 3000m + 444m + 4000m, ceh.GetTotalChargeValueFor(b00Charge, ""));
		}

		public void TestVisualizableDocumentsSupportableAttribute()
		{
			var visualizableDocumentsSupportableAttributes = typeof(CusEntryHeader).GetCustomAttributes(typeof(VisualizableDocumentsSupportableAttribute), false);
			AssertEquals("VisualizableDocumentsSupportableAttribute", 1, visualizableDocumentsSupportableAttributes.Length);

			var visualizableDocumentsSupportableAttribute = (VisualizableDocumentsSupportableAttribute)visualizableDocumentsSupportableAttributes.Single();
			AssertEquals("SupporterType", typeof(CusEntryHeaderFRVisualizableDocumentSupporter), visualizableDocumentsSupportableAttribute.SupporterType);
		}

		[TestDate(2020, 01, 01, 12, 0, 0)]
		public void TestFallbackStampTitleAndText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			Factory.Save();

			AssertEquals((ZString)"PROCÉDURE DE SECOURS / FALLBACK PROCEDURE", cusEntryHeader.FallbackStampTitle);
			AssertEquals((ZString)"AUCUNE DONNÉE DISPONIBLE DANS LE SYSTÈME\r\nENGAGÉE LE 31/12/2019 A 00:00", cusEntryHeader.FallbackStampText);

			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(+2);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(ZString.Empty, cusEntryHeader.FallbackStampTitle);
			AssertEquals(ZString.Empty, cusEntryHeader.FallbackStampText);
		}

		public void TestCH_EntryReleaseDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.CH_EntryReleaseDateInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("First BAE Date", resStringData.Caption);
		}

		public void TestCH_ExitedStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.CH_ExitedStatusInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("ECS Status", resStringData.ShortCaption);
				AssertEquals("Export Control Status", resStringData.Caption);
			});
		}

		public void TestTotalPaid()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.CH_TotalPaid), false, x
				=> x.Caption == "Total Paid");
		}

		[TestDate(2022, 9, 1, 1, 1, 1)]
		public void TestLogExitedStatusIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_ExitedStatus = "XXX";
			Factory.Save();
			var mostRecentLog = entry.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals(Events.ArrivalDetailsChanged.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("VAA Trigger = NUL", mostRecentLog.SL_Reference);
				AssertEquals("01-Sep-22 01:01:01 +00:00", mostRecentLog.EventTimeOffset.ToString());

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				entry.CH_ExitedStatus = "XX1";
				Factory.Save();
				mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals(Events.CustomsEntryStatus.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ECS=XX1", mostRecentLog.SL_Reference);
				AssertEquals("01-Sep-22 01:01:02 +00:00", mostRecentLog.EventTimeOffset.ToString());

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				entry.CH_ExitedStatus = "YYY";
				Factory.Save();
				mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals("ECS=YYY", mostRecentLog.SL_Reference);

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				entry.CH_ExitedStatus = "ZZZ";
				var addedLog = entry.Logs.AddNew(Events.CustomsEntryStatus, "ECS=ZZZ", ZDateTimeOffset.Now);

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				Factory.Save();
				mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals("should not add duplicate log", addedLog.PK, mostRecentLog.PK);
			});
		}

		public void TestGetNewAmendmentSnapshotManager()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<AmendmentSnapshotManager>(entryHeader.GetNewAmendmentSnapshotManager());
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public override void TestFOBAndCIFFigures()
		{
			var declaration = ImportJobDeclaration;
			var setup = GetChargesCurrencyTestSetup();
			setup.SetupJobDecWithOFTAndCIFCharges(declaration, declaration.LocalCurrencyCode);
			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasFreight.Amount);
				AssertEquals("FOB for the entry", 10500.00m, entryHeader.FOB.Amount);
				AssertEquals("CIF for the entry", 11000.00m, entryHeader.CIF.Amount);
			});
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public override void TestFOBAndCIFInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			ZDateTime from = new ZDateTime(2007, 6, 1);
			ZDateTime to = new ZDateTime(2007, 12, 30);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			var setup = GetChargesCurrencyTestSetup();
			setup.SetupJobDecWithOFTAndCIFCharges(declaration, newCurrency.RX_Code);
			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("FOB in local currency", 21000.0m, entryHeader.FOBInLocalCurrency.Amount);
				AssertEquals("CIF in local currency", 22000.0m, entryHeader.CIFInLocalCurrency.Amount);
			});
		}

		public override void TestRemergeDoesNotClobberCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			invHeader.InvoiceLines.AddNew();
			DoMerge(dec);
			var ceh = dec.CustomsEntryHeaders[0];
			ceh.Charges.AddNew("DAN", 69m);
			ceh.MergedLines[0].Fees.AddOrUpdate("JAM", 79m);
			ceh.MergedLines[0].Fees[0].CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			DoMerge(dec);
			AssertEquals(0, ceh.Charges.Count);
		}

		public void TestPreviousECSStatusToAPE()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.APE));
			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.EAN));
			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.APE));
			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.APE));
			AssertEquals(ExportControlStatusList.Codes.EAN, entryHeader.PreviousECSStatusToAPE);

			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.EBL));
			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.APE));
			entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ExportControlStatusList.Codes.APE));
			AssertEquals(ExportControlStatusList.Codes.EBL, entryHeader.PreviousECSStatusToAPE);
		}

		public void TestCustomsLastEntryStatusDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryheader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryheader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;

			var msg = Factory.New<DeltaCImportFREDIMessage>();
			msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			msg.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 11, DateTimeKind.Utc);
			entryheader.Messages.Add(msg);

			AssertEquals("CustomsLastEntryStatusDate should be the date of the latest received message containing entry status", new ZDateTime(2024, 03, 11, DateTimeKind.Utc).ToLocalBranchTime(), entryheader.CustomsLastEntryStatusDate);

			var msg2 = Factory.New<DeltaCImportFREDIMessage>();
			msg2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg2.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.ErrorResponseMessageWithoutNodeEtat.xml");
			msg2.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 12, DateTimeKind.Utc);
			entryheader.Messages.Add(msg2);

			AssertEquals("CustomsLastEntryStatusDate should be the date of the latest received message containing entry status", new ZDateTime(2024, 03, 11, DateTimeKind.Utc).ToLocalBranchTime(), entryheader.CustomsLastEntryStatusDate);

			var msg3 = Factory.New<DeltaCImportFREDIMessage>();
			msg3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg3.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			msg3.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 13, DateTimeKind.Utc);
			entryheader.Messages.Add(msg3);

			AssertEquals("CustomsLastEntryStatusDate should be the date of the latest received message containing entry status", new ZDateTime(2024, 03, 13, DateTimeKind.Utc).ToLocalBranchTime(), entryheader.CustomsLastEntryStatusDate);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var firstCESLogDate = new ZDateTime(2024, 08, 29, DateTimeKind.Utc);
			var lastCESLogDate = new ZDateTime(2024, 08, 30, DateTimeKind.Utc);

			AssertEquals("CustomsLastEntryStatusDate for DeltaIE when there is no CES event", ZDateTime.Empty, entryheader.CustomsLastEntryStatusDate);

			entryheader.Logs.AddNew(Events.CustomsEntryStatus, firstCESLogDate.ToOffset());
			AssertEquals("CustomsLastEntryStatusDate for DeltaIE should be the latest CES event's SL_EventTime", firstCESLogDate, entryheader.CustomsLastEntryStatusDate);

			entryheader.Logs.AddNew(Events.CustomsEntryStatus, lastCESLogDate.ToOffset());
			AssertEquals("CustomsLastEntryStatusDate for DeltaIE should be the latest CES event's SL_EventTime", lastCESLogDate, entryheader.CustomsLastEntryStatusDate);
		}

		public void TestIDDMessageAndMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			AssertIDDMessageAndMessageType(EDIMessageStatusList.Codes.ProcessedOK, DeltaIEResponseMessageSubTypeList.Codes.InvalidationApprovalNotificationFeedback, new ZDateTime(2024, 03, 11, DateTimeKind.Utc));
			AssertIDDMessageAndMessageType(EDIMessageStatusList.Codes.Pending, DeltaIEResponseMessageSubTypeList.Codes.InvalidationApprovalNotificationFeedback, new ZDateTime(2024, 03, 11, DateTimeKind.Utc));
			AssertIDDMessageAndMessageType(EDIMessageStatusList.Codes.ProcessedOK, DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance, new ZDateTime(2024, 03, 12, DateTimeKind.Utc));
			AssertIDDMessageAndMessageType(EDIMessageStatusList.Codes.ProcessedOK, DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance, new ZDateTime(2024, 03, 13, DateTimeKind.Utc));
			AssertIDDMessageAndMessageType(EDIMessageStatusList.Codes.ProcessedOK, DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification, new ZDateTime(2024, 03, 14, DateTimeKind.Utc));

			void AssertIDDMessageAndMessageType(ZString messageStatus, ZString messageSubType, ZDateTime createdDateTime)
			{
				var msg = AddMessage(messageStatus, messageSubType, createdDateTime);
				entryHeader.Messages.Add(msg);

				if (!messageSubType.EqualsAny(new ZString[] { "426", "428", "429" }))
				{
					AssertEquals("IDDMessageType should be equal to empty when EM_MessageSubType is neither 426 nor 428 nor 429", ZString.Empty, entryHeader.IDDMessageType);
				}

				if (messageStatus != EDIMessageStatusList.Codes.ProcessedOK || !messageSubType.EqualsAny(new ZString[] { "426", "428", "429" }))
				{
					AssertNull("IDDMessage should be equal to the last received and processed message where EM_MessageSubType is either 426 or 428 or 429", entryHeader.IDDMessage);
				}
				else
				{
					AssertEquals("IDDMessage should be equal to the last received and processed message where EM_MessageSubType is either 426 or 428 or 429", msg, entryHeader.IDDMessage);
					AssertEquals("IDDMessageType should be equal to the EM_MessageSubType of IDDMessage", messageSubType, entryHeader.IDDMessageType);
				}
			}

			DeltaIEFREDIMessage AddMessage(ZString messageStatus, ZString messageSubType, ZDateTime createdDateTime)
			{
				var msg = Factory.New<DeltaIEFREDIMessage>();
				msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				msg.EM_SystemCreateTimeUtc = createdDateTime;
				msg.EM_Status = messageStatus;
				msg.EM_MessageSubType = messageSubType;
				return msg;
			}
		}

		public void TestCOEEventPublishedOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EAN, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EBL, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EEC, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EEX, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.ENF, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.ESD, false);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.HEC, false);

			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.APE, true, ExportControlStatusList.Codes.APE);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.SOR, true, ExportControlStatusList.Codes.APE);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.ESO, true, ExportControlStatusList.Codes.APE);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EAN, true, ExportControlStatusList.Codes.APE);

			declaration = Factory.New<JobDeclaration>();
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.SOR, true, ExportControlStatusList.Codes.SOR);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.APE, true, ExportControlStatusList.Codes.SOR);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.ESO, true, ExportControlStatusList.Codes.SOR);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EAN, true, ExportControlStatusList.Codes.SOR);

			declaration = Factory.New<JobDeclaration>();
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.ESO, true, ExportControlStatusList.Codes.ESO);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.SOR, true, ExportControlStatusList.Codes.ESO);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.APE, true, ExportControlStatusList.Codes.ESO);
			RunCOEScenario(entryHeader, ExportControlStatusList.Codes.EAN, true, ExportControlStatusList.Codes.ESO);

			void RunCOEScenario(CusEntryHeader entryHeader, ZString exitedStatus, bool expectEvent, string expectedReference = null)
			{
				entryHeader.CH_ExitedStatus = exitedStatus;
				Factory.Save();

				var events = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.ConfirmationOfExit);

				if (expectEvent)
				{
					AssertEquals("It is expected that a COE event is generated", 1, events.Count());
					var coeEvent = events.First();
					AssertEquals("Event Reference must be equal to ECS Status", expectedReference, coeEvent.SL_Reference);
				}
				else
				{
					AssertEquals("It is expected that no COE events is generated", 0, events.Count());
				}

				entryHeader.Logs.ClearAllNotifications();
			}
		}

		public void TestSADDocumentPrintingOnECSStatus()
		{
			using var tempEnabled = FRCustomsDataRegistry.Instance.SADGenerationOnConfirmedExitEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			RunScenario(ExportControlStatusList.Codes.SOR, true);
			RunScenario(ExportControlStatusList.Codes.ESO, true);
			RunScenario(ExportControlStatusList.Codes.APE, true);
			RunScenario(ExportControlStatusList.Codes.ESD, true);
			RunScenario(ExportControlStatusList.Codes.EAN, false);
			RunScenario(ExportControlStatusList.Codes.EBL, false);
			RunScenario(ExportControlStatusList.Codes.EEC, false);
			RunScenario(ExportControlStatusList.Codes.EEX, false);
			RunScenario(ExportControlStatusList.Codes.ENF, false);
			RunScenario(ExportControlStatusList.Codes.HEC, false);

			void RunScenario(string ecsCode, bool shouldBePrinted)
			{
				const string expectedDocumentType = "EPR";
				const string expectedDocumentName = "SADH C88 - BGMReference";
				const string expectedAttachmentName = "SAD/H for FR  BGMReference  .XLSX";
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "BGMReference";

				entryHeader.Logs.Add(GetNewStmALogWithCESAndGivenECSCode(ecsCode));
				entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "60", ZDateTimeOffset.Now);

				Factory.Save();
				var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader.PK));

				if (shouldBePrinted)
				{
					AssertEquals("There should be 1 print job created if all conditions are satisfied for the entry.", 1, printJobs.Length);

					var printJob = printJobs[0];
					AssertEquals("Printed document should be enqueued to eDocs.", "DDS", printJob.SP_JobType);
					AssertEquals("Printed document should have the document type as EPR.", expectedDocumentType, printJob.SP_DocumentType);
					AssertContains("There should be a print job with the expected document name.", expectedDocumentName, printJob.SP_DocumentName);
					AssertEquals("There should be a print job with the expected attachment name.", expectedAttachmentName, printJob.SP_EmailAttachments);
				}
				else
				{
					var printJobFound = printJobs.Any(
						s => s.SP_DocumentType == expectedDocumentType
							&& s.SP_DocumentName.Contains(expectedDocumentName)
							&& s.SP_EmailAttachments == expectedAttachmentName);
					Assert("SAD document print job should not be created", !printJobFound);
				}
			}
		}

		public void TestSADDocumentPrintingOnCustomsClearance()
		{
			EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string expectedDocumentType = "EPR";
			const string expectedDocumentName = "SADH C88 - BGMReference";
			const string expectedAttachmentName = "SAD/H for FR  BGMReference  .XLSX";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001114";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGMReference";

			Factory.Save();
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader.PK));
			var printJobFound = printJobs.Any(s => s.SP_DocumentType == expectedDocumentType && s.SP_DocumentName.Contains(expectedDocumentName) && s.SP_EmailAttachments == expectedAttachmentName);
			Assert("SAD document print job should not be created", !printJobFound);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "100", ZDateTimeOffset.Now);
			Factory.Save();
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader.PK));
			AssertEquals("There should be 1 print job created if all conditions are satisfied for the entry.", 1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("Printed document should be enqueued to eDocs.", "DDS", printJob.SP_JobType);
			AssertEquals("Printed document should have the document type as EPR.", expectedDocumentType, printJob.SP_DocumentType);
			AssertContains("There should be a print job with the expected document name.", expectedDocumentName, printJob.SP_DocumentName);
			AssertEquals("There should be a print job with the expected attachment name.", expectedAttachmentName, printJob.SP_EmailAttachments);

			EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001115";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "BGMReference";
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "100", ZDateTimeOffset.Now);

			Factory.Save();
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entryHeader2.PK));

			printJobFound = printJobs.Any(s => s.SP_DocumentType == expectedDocumentType && s.SP_DocumentName.Contains(expectedDocumentName) && s.SP_EmailAttachments == expectedAttachmentName);
			Assert("SAD document print job should not be created", !printJobFound);
		}

		public override void TestEntryStatusChangingToClearRecordsLog()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			Factory.Save();
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			entryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
			AssertEquals(true, entryHeader.ClearanceDate.IsEmpty);
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, entryHeader.ClearanceDate);
		}

		public void TestIsBondedWarehousingDisabled()
		{
			var testCase = new MultiFactorTestCase<CusEntryHeader>(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;

				var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				authorisationHeader.CusAuthorisationRules.AddNew();
				var authorizationUsage = (entryInstruction as CusEntryInstruction).CusAuthorizationUsages.AddNew();
				authorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;

				return entryHeader;
			});

			var typeIsIPO = new FieldPreq<CusEntryHeader>(header => (header.EntryInstruction.CusAuthorizationUsages[0].AuthorisationHeader as CusAuthorisationHeader).CPH_TypeInfo)
				.Values(Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing).NotValues(Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			var ruleCodeIsCON = new FieldPreq<CusEntryHeader>(header => (header.EntryInstruction.CusAuthorizationUsages[0].AuthorisationHeader as CusAuthorisationHeader).CusAuthorisationRules[0].CPR_RuleCodeInfo)
				.Values(CusAuthorisationRuleTypeList.Codes.CON).NotValues(CusAuthorisationRuleTypeList.Codes.AUT);
			var valueFromIsSUC = new FieldPreq<CusEntryHeader>(header => (header.EntryInstruction.CusAuthorizationUsages[0].AuthorisationHeader as CusAuthorisationHeader).CusAuthorisationRules[0].CPR_ValueFromInfo)
				.Values(RuleCodeCONValueFromList.Codes.SUC).NotValues(RuleCodeCONValueFromList.Codes.CNA);

			testCase.SetUpCondition(typeIsIPO && ruleCodeIsCON && valueFromIsSUC);
			testCase.RunAssertion(header => Assert("IsBondedWarehousingDisabled should be true when the entryHeader has an IPO Authorization containing a CON rule with ValueFrom as SUC.", header.IsBondedWarehousingDisabled), header => Assert(!header.IsBondedWarehousingDisabled));
		}

		public void TestIsEntryStatusCleared()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			entry.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
			AssertEquals("Prerequisite", true, declaration.ApplicationExtender.IsEntryStatusCleared(entry));
			AssertEquals("entry.IsEntryStatusCleared should be true when ApplicationExtender.IsEntryStatusCleared is true.", true, entry.IsEntryStatusCleared);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Prerequisite", false, declaration.ApplicationExtender.IsEntryStatusCleared(entry));
			AssertEquals("entry.IsEntryStatusCleared should be false when both ApplicationExtender.IsEntryStatusCleared and base.IsEntryStatusCleared are false.", false, entry.IsEntryStatusCleared);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status", "FR");
			var entryStatusCodeWithCustomsClearedAttribute = helper.CreateNewOrGetExistingCusCodeList("FR", "CSTA", "TES", ZDateTime.Now.AddYears(-1), ZDateTime.Now.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("CustomsCleared", "Status represents Customs Cleared", "CSTA", "FR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(entryStatusCodeWithCustomsClearedAttribute.PK, "CustomsCleared", "true");
			Factory.Save();

			entry.CH_EntryStatus = "TES";
			AssertEquals("entry.IsEntryStatusCleared should be true when base.IsEntryStatusCleared is true.", true, entry.IsEntryStatusCleared);
		}

		protected override Type ExpectedChargeCollectionType => typeof(EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = base.ImportJobDeclaration;
				declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				return declaration;
			}
		}

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocument GetPreviousDocumentForComplementaryJob(EU.Business.Declaration.JobComInvoiceHeader invoice)
		{
			var prevDoc = invoice.PreviousDocuments.AddNew();
			prevDoc.CSI_ReferenceNumber = "456";
			prevDoc.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			return prevDoc;
		}

		protected override CusEntryNumber GetEntryNumForComplementaryJob(ZString entryNum, EU.Business.Declaration.JobDeclaration declaration)
		{
			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_RN_NKCountryCode = declaration.CountryCode;
			cusEntryNum.CE_EntryNum = entryNum;
			cusEntryNum.CE_EntryType = declaration.IsImport ? Customs.Business.JobMessageTypeList.Codes.Import : Customs.Business.JobMessageTypeList.Codes.Export;
			cusEntryNum.CE_ParentTable = "JobDeclaration";

			return cusEntryNum;
		}

		protected override void SetInvoicesToResultInTwoEntries(BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2)
		{
			((JobComInvoiceLine)line1).InvoiceHeader.ZG_AgreedPlaceCode = "1";
			((JobComInvoiceLine)line2).InvoiceHeader.ZG_AgreedPlaceCode = "2";
		}

		protected override void OverrideValuationDate(BaseJobComInvoiceHeader invoice, ZDateTime date)
		{
			base.OverrideValuationDate(invoice, date);
			foreach (CusEntryInstruction instruction in invoice.CusEntryInstructions)
			{
				instruction.CEI_DateForDuty = date;
			}
		}
		CusEntryHeader SetupForTotalD48AmountAtHeaderLevel()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.FillWithValidTestData();
			entryInstruction.CEI_Style = "Ye12367";

			var suppDoc1 = invoiceHeader.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "0001";
			suppDoc1.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc1.CSI_Status = "AN";
			suppDoc1.CSI_Quantity3 = 5;
			suppDoc1.CSI_Value = 30;
			suppDoc1.CSI_ReferenceNumber = "Header";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();

			var suppDoc2 = invoiceLine1.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "0001";
			suppDoc2.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc2.CSI_Status = "AN";
			suppDoc2.CSI_Quantity3 = 30;
			suppDoc2.CSI_Value = 20;
			suppDoc2.CSI_ReferenceNumber = "invoiceline";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));

			entry.CH_CEI_Instruction = entryInstruction.PK;

			// After merge CSI_Code = "0001" so IsD48 = true, CL_LineNumber = 1 So invoiceHeader.SupportingDocuments counted(suppDoc1.CSI_Value = 30), suppDoc2 is IsUnderInvoiceLine so counted(CSI_Value = 20)
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Procedure = "Ye12367";

			// After merge IsD48 = true but CL_LineNumber <> 1 so invoiceHeader.SupportingDocuments(suppDoc1) not counted and no SupportingDocuments of invoiceLine2
			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "Ye12367";

			// After merge IsD48 = true but CL_LineNumber <> 1 so invoiceHeader.SupportingDocuments(suppDoc1) not counted and no SupportingDocuments of invoiceLine3
			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "Ye12367";

			// After merge IsD48 = true but CL_LineNumber <> 1 so invoiceHeader.SupportingDocuments(suppDoc1) not counted and no SupportingDocuments of invoiceLine4
			var entryLine4 = entry.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = "Ye12367";

			return entry;
		}

		CusEntryHeader SetupForTotalD48Amount()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.FillWithValidTestData();
			entryInstruction.CEI_Style = "Ye12367";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc3 = invoiceLine1.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "0001";
			suppDoc3.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc3.CSI_Status = "AN";
			suppDoc3.CSI_Quantity3 = 30;
			suppDoc3.CSI_Value = 20;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "DEF";
			suppDoc4.CSI_Status = "AN";
			suppDoc4.CSI_Quantity3 = 0;
			suppDoc4.CSI_Value = 10;
			var suppDoc5 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "GHI";
			suppDoc5.CSI_Status = "AN";
			suppDoc5.CSI_Quantity3 = 0;
			suppDoc5.CSI_Value = 30;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc6 = invoiceLine3.SupportingDocuments.AddNew();
			suppDoc6.CSI_Code = "0003";
			suppDoc6.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc6.CSI_Status = "AN";
			suppDoc6.CSI_Quantity3 = 5;
			suppDoc6.CSI_Value = 10;

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc7 = invoiceLine4.SupportingDocuments.AddNew();
			suppDoc7.CSI_Code = "0003";
			suppDoc7.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc7.CSI_Status = "AY";
			suppDoc7.CSI_Quantity3 = 5;
			suppDoc7.CSI_Value = 10;

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));

			entry.CH_CEI_Instruction = entryInstruction.PK;

			// After merge CSI_Code = "0001" so IsD48 = true and CSI_Value = 10
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "DEF" | "GHI" so IsD48 = false and CSI_Value = 40 so value will not be counted
			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10
			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10, however CSI_Status = 'Y' so therefore IsCompletee = false so value will not be counted
			var entryLine4 = entry.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = "Ye12367";

			return entry;
		}

		CusEntryHeader SetupForTotalD48AmountWithDocumentsSetAtAllLevels()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var suppDoc1 = invoiceHeader.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "0001";
			suppDoc1.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc1.CSI_Status = "AN";
			suppDoc1.CSI_Quantity3 = 5;
			suppDoc1.CSI_Value = 30;
			suppDoc1.CSI_ReferenceNumber = "Header";

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.FillWithValidTestData();
			entryInstruction.CEI_Style = "Ye12367";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc3 = invoiceLine1.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "0001";
			suppDoc3.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc3.CSI_Status = "AN";
			suppDoc3.CSI_Quantity3 = 30;
			suppDoc3.CSI_Value = 20;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "DEF";
			suppDoc4.CSI_Status = "AN";
			suppDoc4.CSI_Quantity3 = 0;
			suppDoc4.CSI_Value = 10;
			var suppDoc5 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "GHI";
			suppDoc5.CSI_Status = "AN";
			suppDoc5.CSI_Quantity3 = 0;
			suppDoc5.CSI_Value = 30;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc6 = invoiceLine3.SupportingDocuments.AddNew();
			suppDoc6.CSI_Code = "0003";
			suppDoc6.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc6.CSI_Status = "AN";
			suppDoc6.CSI_Quantity3 = 5;
			suppDoc6.CSI_Value = 10;

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc7 = invoiceLine4.SupportingDocuments.AddNew();
			suppDoc7.CSI_Code = "0003";
			suppDoc7.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc7.CSI_Status = "AY";
			suppDoc7.CSI_Quantity3 = 5;
			suppDoc7.CSI_Value = 10;

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));

			entry.CH_CEI_Instruction = entryInstruction.PK;

			// After merge CSI_Code = "0001" so IsD48 = true and CSI_Value = 10
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "DEF" | "GHI" so IsD48 = false and CSI_Value = 40 so value will not be counted
			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10
			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10, however CSI_Status = 'Y' so therefore IsCompletee = false so value will not be counted
			var entryLine4 = entry.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = "Ye12367";

			return entry;
		}

		EU.Business.Declaration.CusEntryLineFee AddNewFee(EU.Business.Declaration.CusEntryLine entryLine, string feeType, string mop, decimal amount)
		{
			var fee = entryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = mop;
			fee.CF_ChargeType = feeType;
			fee.CF_ChargeAmount = amount;
			return fee;
		}

		void CreateRatingRefZZRecords(BusinessObjectFactory newFactory)
		{
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			newFactory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Anything one", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Doesn't matter");
			var mop2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "1", "Anything two", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Still doesn't matter");
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "");

			var a00 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusRateCodes.A00);
			helper.LoadOrCreateNewCusRateCode(newFactory, UniversalReferenceConstants.RefCusRateCodes.A00, a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, "B00");
			helper.LoadOrCreateNewCusRateCode(newFactory, "B00", b00.PK);
			newFactory.Save();
		}

		StmALog GetNewStmALogWithCESAndGivenECSCode(string code)
		{
			Thread.Sleep(1);
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.CustomsEntryStatusCode;
				log.SL_Reference = "ECS=" + code;
				log.SL_EventTime = ZDateTime.Now.AddMinutes(-1);
			}
			return log;
		}
	}
}

