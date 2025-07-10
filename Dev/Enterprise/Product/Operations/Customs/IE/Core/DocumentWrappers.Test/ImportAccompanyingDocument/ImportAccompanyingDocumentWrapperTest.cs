using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[TestedType(typeof(ImportAccompanyingDocumentWrapper))]
	public class ImportAccompanyingDocumentWrapperTest : DocumentWrapperTestCase
	{
		public void TestNoExceptionThrownWhenEmptyEntryHeader()
		{
			AssertNoExceptionThrown(() => {
				var wrapper = new ImportAccompanyingDocumentWrapper(Factory.New<CusEntryHeader>(), Factory);
				var natureOfTransaction = wrapper.NatureOfTransaction;
			});
		}

		public void TestImportAccompanyingDocumentWrapperProperties()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "EXPer";
			exporter.MainAddress.Address1 = "Exporter Street and Number";
			exporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EXPREG001", Core.Constants.CountryCodes.Ireland);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPer";
			importer.MainAddress.Address1 = "Importer Street and Number";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMPREG002", Core.Constants.CountryCodes.Ireland);

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "DCLer";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DCLREG003", Core.Constants.CountryCodes.Ireland);

			var declarantAddr = declarant.Addresses.AddNew();
			declarantAddr.Address1 = "Declarant Street and Number";

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller";
			seller.MainAddress.Address1 = "Seller Street and Number";
			seller.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "SELLER004", Core.Constants.CountryCodes.Ireland);

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.MainAddress.Address1 = "Buyer Street and Number";
			buyer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BUYER005", Core.Constants.CountryCodes.Ireland);

			var representative = Factory.New<OrgHeader>();
			representative.OH_FullName = "Representative";
			representative.MainAddress.Address1 = "Representative Street and Number";
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REPRESENTATIVE006", Core.Constants.CountryCodes.Ireland);

			var supervising = Factory.New<OrgHeader>();
			supervising.OH_FullName = "Supervising";
			supervising.MainAddress.Address1 = "Representative Street and Number";
			supervising.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "SUPERVISING007", Core.Constants.CountryCodes.Ireland);

			declaration.JE_ApplicationCode = "IMP";
			entryInstruction.CEI_Style = "EDF";
			entryInstruction.CEI_SubStyle = "J";

			entryHeader.EntryNumber = "ENT123456";
			declaration.JE_EntryStyle = "02";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = exporter.MainAddress.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddr.PK;
			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			declaration.JE_OH_Buyer = buyer.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.SupervisingOfficeDocAddress.OrganisationPK = supervising.PK;
			declaration.CustomsEntryInstructions[0].CEI_Style = "EDF";
			declaration.JE_CustomsOffice = "IE012345";
			declaration.JE_TotalWeight = 55.55m;

			declaration.JE_UCR = "UCR";
			invoiceHeader.JZ_ValuationCode = "11";

			AssertEquals("ENT123456", wrapper.FormattedEntryNumber);
			AssertEquals("EDF", wrapper.EntryStyle);
			AssertEquals("EXPer", wrapper.ExporterFullName);
			AssertEquals("IEEXPREG001", wrapper.ExporterEORI);
			AssertEquals("Exporter Street and Number", wrapper.ExporterStreetAndNumber);
			AssertEquals("IMPer", wrapper.ImporterFullName);
			AssertEquals("IEIMPREG002", wrapper.ImporterEORI);
			AssertEquals("Importer Street and Number", wrapper.ImporterStreetAndNumber);
			AssertEquals("DCLer", wrapper.DeclarantFullName);
			AssertEquals("IEDCLREG003", wrapper.DeclarantEORI);
			AssertEquals("Declarant Street and Number", wrapper.DeclarantStreetAndNumber);

			AssertEquals("Seller", wrapper.SellerFullName);
			AssertEquals("IESELLER004", wrapper.SellerEORI);
			AssertEquals("Seller Street and Number", wrapper.SellerStreetAndNumber);
			AssertEquals("Buyer", wrapper.BuyerFullName);
			AssertEquals("IEBUYER005", wrapper.BuyerEORI);
			AssertEquals("Buyer Street and Number", wrapper.BuyerStreetAndNumber);
			AssertEquals("Representative", wrapper.RepresentativeFullName);
			AssertEquals("IEREPRESENTATIVE006", wrapper.RepresentativeEORI);
			AssertEquals("Representative Street and Number", wrapper.RepresentativeStreetAndNumber);

			AssertEquals("IE012345", wrapper.Box44OfficeOfPresentation);
			AssertEquals("55.55", wrapper.Box35GrossMass);
			AssertEquals("SUPERVISING007", wrapper.Box44SupervisingOffice);

			AssertEquals("UCR", wrapper.DeclarationUCR);
			AssertEquals("11", wrapper.NatureOfTransaction);

			AssertNotNull(wrapper.Items);
			AssertType<ImportAccompanyingDocumentEntryLineWrapperCollection>(wrapper.Items);
		}

		public void TestBox14RepresentativeStatus()
		{
			declaration.JE_ApplicationCode = "IMP";
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("JE_DeclarantType = SEL", "1", wrapper.Box14RepresenativeStatus);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("JE_DeclarantType = DIR", "2", wrapper.Box14RepresenativeStatus);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("JE_DeclarantType = IND", "3", wrapper.Box14RepresenativeStatus);
		}

		public void TestDeclarationUCR()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_UCR = "UCR1234567890";
			invoiceHeader.JZ_UCR = "UCR";
			AssertEquals("UCR1234567890", wrapper.DeclarationUCR);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			declaration.JE_UCR = "UCR1234567890";
			AssertEquals("UCR1234567890", wrapper.DeclarationUCR);

			declaration.JE_UCR = ZString.Empty;
			AssertEquals("JE_UCR empty, fallback to JZ_UCR", "UCR", wrapper.DeclarationUCR);

			invoiceHeader.JZ_UCR = ZString.Empty;
			AssertEquals("Both JE_UCR and JZ_UCR empty", ZString.Empty, wrapper.DeclarationUCR);
		}

		public void TestInvoiceTotal()
		{
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);

			declaration.JE_ApplicationCode = "IMP";
			entryInstruction.CEI_Style = "EDF";
			entryInstruction.CEI_SubStyle = "J";

			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine1.JI_LinePrice = 100.00m;

			AssertEquals(100.00m, wrapper.Box22InvoiceTotal);
			AssertEquals("USD", wrapper.Box22InvoiceTotalCurrency);
			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine2.JI_LinePrice = 200.00m;
			AssertEquals(300.00m, wrapper.Box22InvoiceTotal);
		}

		public void TestBox22ExchangeRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = "IMP";
				var cei = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = cei.PK;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceCurrExRate = 1.612345;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var wrapper = new ImportAccompanyingDocumentWrapper(entryHeader, Factory);
				AssertEquals("5 Decimal Places ", "1.61235", wrapper.Box22ExchangeRate);
			}
		}

		public void TestContainers()
		{
			declaration.CusContainers.AddNew();
			declaration.CusContainers[0].CO_ContainerNumber = "CONT1";
			declaration.CusContainers[0].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.CusContainers.AddNew();
			declaration.CusContainers[1].CO_ContainerNumber = "CONT2";
			declaration.CusContainers[1].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Add(declaration.CusContainers[0]);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Add(declaration.CusContainers[1]);

			var wrapper = new ImportAccompanyingDocumentWrapper(entryHeader, Factory);
			AssertEquals("CONT1\r\nCONT2", wrapper.Box31Containers);
		}

		public void TestPreviousDocuments()
		{
			declaration.PreviousDocuments.AddNew();
			declaration.PreviousDocuments[0].CSI_SubType = "Z";
			declaration.PreviousDocuments[0].CSI_Code = "MCR";
			declaration.PreviousDocuments[0].CSI_ReferenceNumber = "HBAC12578834930";

			var previousDocument1 = entryInstruction.PreviousDocuments.AddNew();
			previousDocument1.CSI_SubType = "Z";
			previousDocument1.CSI_Code = "MCR";
			previousDocument1.CSI_ReferenceNumber = "HBAC12578834930";

			var previousDocument2 = invoiceHeader.PreviousDocuments.AddNew();
			previousDocument2.CSI_SubType = "Z";
			previousDocument2.CSI_Code = "DCR";
			previousDocument2.CSI_ReferenceNumber = "HBAC12578834930-1234";

			AssertEquals("MCR | HBAC12578834930 | 0\r\nDCR | HBAC12578834930-1234 | 0", wrapper.Box40SummaryDeclarationAndPreviousDocsCombined);
		}

		public void TestAuthorisationHolders()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure4071 = helper.CreateRefCusProcedure("IE", "A", "40", "71", "000", "whatever", "IMP", group: "C21", outOfWarehouse: true);
			var procedure7100 = helper.CreateRefCusProcedure("IE", "A", "71", "00", "000", "whatever", "IMP", group: "C21", intoWarehouse: true);
			Factory.Save();

			var warehouseINTO = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseINTO.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOUTOF.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseINTO.MainAddress.OA_RN_NKCountryCode = "IE";
			warehouseOUTOF.MainAddress.OA_RN_NKCountryCode = "IE";
			warehouseINTO.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U1234567INN", "IE");
			warehouseOUTOF.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U7654321OUT", "IE");

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_LocationQualifier = "CC";

			entryInstruction.CEI_Style = "EDF";
			entryInstruction.CEI_SubStyle = "J";
			entryInstruction.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;

			var inLine4071 = invoiceHeader.InvoiceLines.AddNew();
			var inLine7100 = invoiceHeader.InvoiceLines.AddNew();
			inLine4071.JI_CEI = entryInstruction.PK;
			inLine7100.JI_CEI = entryInstruction.PK;
			inLine4071.JI_Procedure = procedure4071.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7100.JI_Procedure = procedure7100.FullCodeCurrentPlusPreviousPlusConcession;

			AssertEquals("pre-req IsOutOfWarehouseWarehousing", true, entryInstruction.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req IsIntoWarehouseWarehousing", true, entryInstruction.HasIntoWarehouseProcedure);

			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			owner3.CustomsCodes.AddNew("EOR", "EOR1234567890");
			owner3.OH_Code = "123";

			var auth1 = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "CGU";
			auth1.AGC_Number = "IE945390992000";
			auth1.AGC_OH_Owner = owner1.PK;

			var auth2 = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "OLD";
			auth2.AGC_Number = "OLDOWNER";
			auth2.AGC_OH_Owner = owner2.PK;

			var auth3 = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth3.AGC_Code = "ZZZ";
			auth3.AGC_Number = "";
			auth3.AGC_OH_Owner = owner3.PK;

			entryHeader.Declaration.JE_OH_Importer = owner2.PK;
			entryInstruction.CEI_OH_Owner = owner2.PK;
			AssertNotEquals("pre-req owner exists", ZGuid.Empty, entryInstruction.CEI_OH_Owner);

			AssertEquals("CGU | IE945390992000\r\nZZZ | ", wrapper.Box44AuthorisationHolders);
		}

		public void TestBox18TransportReference()
		{
			declaration.JE_TransportMeans = "10";
			declaration.JE_TransportIDInland = "ABC123";

			AssertEquals("Box18TransportReference", "10 ABC123", wrapper.Box18TransportReference);
		}

		public void TestBox49WarehouseID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("IE", "A", "71", "00", "000", "", MessageTypeList.Codes.Import,
				group: "H2", intoWarehouse: true);
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U123456A", Core.Constants.CountryCodes.Ireland);

			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			entryInstruction.CEI_Style = "H2";
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "7100000";

			AssertEquals("U123456A", wrapper.Box49WarehouseID);
		}

		public void TestTransportModeInlandConverted()
		{
			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
			AssertEquals("Inland transport mode AIR", "4", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("Inland transport mode FIX", "7", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("Inland transport mode IWT", "8", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.OwnPropulsion;
			AssertEquals("Inland transport mode OWN", "9", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.Mail;
			AssertEquals("Inland transport mode MAI", "5", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.Rail;
			AssertEquals("Inland transport mode RAI", "2", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("Inland transport mode ROA", "3", wrapper.TransportModeInlandConverted);

			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			AssertEquals("Inland transport mode SEA", "1", wrapper.TransportModeInlandConverted);
		}

		public void TestBox30LocationOfGoods_UCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = "IMP";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = cei.GoodsLocation;
				entryHeader.CH_CEI_Instruction = cei.PK;
				goodsLocation.CGL_Qualifier = "U";
				goodsLocation.CGL_Type = "A";
				goodsLocation.CGL_AdditionalIdentifier = "AT000000";
				var wrapper = new ImportAccompanyingDocumentWrapper(entryHeader, Factory);

				AssertEquals("UNLOCODE when no qualifiers is filled.", "U A AT000000", wrapper.Box30LocationOfGoods);
			}
		}

		public void TestBox30LocationOfGoods_NonUCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, false))
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = cei.GoodsLocation;
				entryHeader.CH_CEI_Instruction = cei.PK;
				var wrapper = new ImportAccompanyingDocumentWrapper(entryHeader, Factory);

				AssertEquals("UNLOCODE when no qualifiers is filled.", "IE", wrapper.Box30LocationOfGoods);

				goodsLocation.CGL_Type = "BE";
				AssertEquals("UNLOCODE when CGL_Type is filled.", "BE", wrapper.Box30LocationOfGoods);

				goodsLocation.CGL_AdditionalIdentifier = "Dublin";
				AssertEquals("UNLOCODE when CGL_AdditionalIdentifier is filled.", "Dublin", wrapper.Box30LocationOfGoods);
			}
		}

		public void TestPresentationOffice()
		{
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice.CY_Data = "CUSTOMSOFF";

			AssertEquals("Presentation office", "CUSTOMSOFF", wrapper.PresentationOffice);
		}

		public void TestDateOfAcceptance()
		{
			entryHeader.MovementReferenceNumberSetter("123456789", new ZDateTime(2024, 04, 01));
			AssertEquals(new ZDateTime(2024, 04, 01).ToBestReadableDateString(), wrapper.DateOfAcceptance);
		}

		public void TestSummarisedTaxes()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var fee11 = entryLine1.Fees.AddNew();
			fee11.NationalFeeTypeCode = "TAX1";
			fee11.CF_BaseValue = 1000;
			fee11.CF_ChargeAmount = 230;
			fee11.CF_Rate = 23;
			fee11.CF_MethodOfPayment = "P";
			var fee12 = entryLine1.Fees.AddNew();
			fee12.NationalFeeTypeCode = "TAX2";
			fee12.CF_BaseValue = 2000;
			fee12.CF_ChargeAmount = 1000;
			fee12.CF_Rate = 50;
			fee12.CF_MethodOfPayment = "A";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var fee21 = entryLine2.Fees.AddNew();
			fee21.NationalFeeTypeCode = "TAX1";
			fee21.CF_BaseValue = 3000;
			fee21.CF_ChargeAmount = 460;
			fee21.CF_Rate = 23;
			fee21.CF_MethodOfPayment = "D";
			var fee22 = entryLine2.Fees.AddNew();
			fee22.NationalFeeTypeCode = "TAX2";
			fee22.CF_BaseValue = 4000;
			fee22.CF_ChargeAmount = 1000;
			fee22.CF_Rate = 25;
			fee22.CF_MethodOfPayment = "C";
			var fee23 = entryLine2.Fees.AddNew();
			fee23.NationalFeeTypeCode = "TAX3";
			fee23.CF_BaseValue = 5000;
			fee23.CF_ChargeAmount = 500;
			fee23.CF_Rate = 10;
			fee23.CF_MethodOfPayment = "A";

			CombineAssertions("Summarised Taxes", () =>
			{
				AssertEquals("SummarisedTaxType", "TAX1\r\nTAX2\r\nTAX3", wrapper.SummarisedTaxType);
				AssertEquals("SummarisedTaxBase", "4000.00\r\n6000.00\r\n5000.00", wrapper.SummarisedTaxBase);
				AssertEquals("SummarisedPayableAmount", "690.00\r\n2000.00\r\n500.00", wrapper.SummarisedPayableAmount);
				AssertEquals("SummarisedPaymentMethod", "D,P\r\nA,C\r\nA", wrapper.SummarisedPaymentMethod);
				AssertEquals("TotalTaxAmount", "3190.00", wrapper.TotalTaxAmount);

				AssertEquals(
					"SummarisedTaxRate: show the rate when items in group have same rates; show empty otherwise.",
					"23.00\r\nN/A\r\n10.00",
					wrapper.SummarisedTaxRate
				);
			});
		}

		public void TestSummarisedTaxes_ConfirmedFees()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var fee11 = entryLine1.ConfirmedFees.AddNew();
			fee11.CF_ChargeType = "TAX1";
			fee11.CF_BaseValue = 1000;
			fee11.CF_ChargeAmount = 200;
			fee11.CF_Rate = 20;
			fee11.CF_MethodOfPayment = "P";
			var fee12 = entryLine1.ConfirmedFees.AddNew();
			fee12.CF_ChargeType = "TAX2";
			fee12.CF_BaseValue = 2000;
			fee12.CF_ChargeAmount = 100;
			fee12.CF_Rate = 5;
			fee12.CF_MethodOfPayment = "A";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var fee21 = entryLine2.ConfirmedFees.AddNew();
			fee21.CF_ChargeType = "TAX1";
			fee21.CF_BaseValue = 3000;
			fee21.CF_ChargeAmount = 100;
			fee21.CF_Rate = 3.33;
			fee21.CF_MethodOfPayment = "D";
			var fee22 = entryLine2.ConfirmedFees.AddNew();
			fee22.CF_ChargeType = "TAX2";
			fee22.CF_BaseValue = 4000;
			fee22.CF_ChargeAmount = 100;
			fee22.CF_Rate = 2.5;
			fee22.CF_MethodOfPayment = "C";
			var fee23 = entryLine2.ConfirmedFees.AddNew();
			fee23.CF_ChargeType = "TAX3";
			fee23.CF_BaseValue = 5000;
			fee23.CF_ChargeAmount = 500;
			fee23.CF_Rate = 0.1;
			fee23.CF_MethodOfPayment = "A";

			CombineAssertions("Summarised Taxes Confirmed Fees", () =>
			{
				AssertEquals("SummarisedTaxType", "TAX1\r\nTAX2\r\nTAX3", wrapper.SummarisedTaxType);
				AssertEquals("SummarisedTaxBase", "4000.00\r\n6000.00\r\n5000.00", wrapper.SummarisedTaxBase);
				AssertEquals("SummarisedTaxRate", "N/A\r\nN/A\r\n0.10", wrapper.SummarisedTaxRate);
				AssertEquals("SummarisedPayableAmount", "300.00\r\n200.00\r\n500.00", wrapper.SummarisedPayableAmount);
				AssertEquals("SummarisedPaymentMethod", "D,P\r\nA,C\r\nA", wrapper.SummarisedPaymentMethod);
				AssertEquals("TotalTaxAmount", "1000.00", wrapper.TotalTaxAmount);
			});
		}

		public void TestSummarisedTaxes_1D6()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			entryInstruction.AdditionalInfos.AddNew("00100", "").CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			invoiceLine.JI_Procedure = "44";
			var entryLine = invoiceLine.CusEntryLine;

			var fee11 = entryLine.Fees.AddNew();
			fee11.CF_ChargeType = "A00";
			fee11.NationalFeeTypeCode = "1D6";
			fee11.CF_BaseValue = 1000;
			fee11.CF_ChargeAmount = 200;
			fee11.CF_Rate = 20;
			fee11.CF_MethodOfPayment = "P";
			var fee12 = entryLine.Fees.AddNew();
			fee12.CF_ChargeType = "B00";
			fee12.NationalFeeTypeCode = "1D6";
			fee12.CF_BaseValue = 2000;
			fee12.CF_ChargeAmount = 400;
			fee12.CF_Rate = 20;
			fee12.CF_MethodOfPayment = "A";

			CombineAssertions("Summarised Taxes 1D6", () =>
			{
				AssertEquals("SummarisedTaxType", "A00\r\nB00", wrapper.SummarisedTaxType);
				AssertEquals("SummarisedTaxBase", "1000.00\r\n2000.00", wrapper.SummarisedTaxBase);
				AssertEquals("SummarisedTaxRate", "20.00\r\n20.00", wrapper.SummarisedTaxRate);
				AssertEquals("SummarisedPayableAmount", "200.00\r\n400.00", wrapper.SummarisedPayableAmount);
				AssertEquals("SummarisedPaymentMethod", "P\r\nA", wrapper.SummarisedPaymentMethod);
				AssertEquals("TotalTaxAmount", "600.00", wrapper.TotalTaxAmount);
			});
		}

		public void TestEntryStatus()
		{
			entryHeader.MovementReferenceNumberSetter("123456789", new ZDateTime(2024, 04, 01));
			entryHeader.CH_EntryStatus = "CAN";

			AssertEquals("Canceled", wrapper.EntryStatus);
		}

		public void TestAdditionalFiscalReferences()
		{
			var addInfo1 = entryInstruction.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo1.CSI_Code = "1";
			addInfo1.CSI_Description = "Description 1";

			var addInfo2 = entryInstruction.AdditionalInfos.AddNew();
			addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo2.CSI_Code = "2";
			addInfo2.CSI_Description = "Description 2";

			var addInfo3 = entryInstruction.AdditionalInfos.AddNew();
			addInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfo3.CSI_Code = "3";
			addInfo3.CSI_Description = "Description 3";

			AssertContains("Additional References", "1 Description 1", wrapper.AdditionalFiscalReferences);
			AssertContains("Additional References", "2 Description 2", wrapper.AdditionalFiscalReferences);
		}

		public void TestBox45SupportingDocuments()
		{
			declaration.SupportingDocuments.AddNew();
			declaration.SupportingDocuments[0].CSI_SubType = "Z";
			declaration.SupportingDocuments[0].CSI_Code = "MCR";
			declaration.SupportingDocuments[0].CSI_ReferenceNumber = "HBAC12578834930";

			var supportingDocument1 = entryInstruction.SupportingDocuments.AddNew();
			supportingDocument1.CSI_SubType = "Z";
			supportingDocument1.CSI_Code = "MCR";
			supportingDocument1.CSI_ReferenceNumber = "HBAC12578834930";

			var supportingDocument2 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_SubType = "Z";
			supportingDocument2.CSI_Code = "DCR";
			supportingDocument2.CSI_ReferenceNumber = "HBAC12578834930-1234";

			AssertEquals("MCR | HBAC12578834930\r\nDCR | HBAC12578834930-1234", wrapper.Box45SupportingDocuments);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var supplyChainActorReference1 = entryInstruction.CusSupplyChainActorReferences.AddNew();
			supplyChainActorReference1.CFR_Code = "1";
			supplyChainActorReference1.CFR_Reference = "REF 1";

			var supplyChainActorReference2 = entryInstruction.CusSupplyChainActorReferences.AddNew();
			supplyChainActorReference2.CFR_Code = "2";
			supplyChainActorReference2.CFR_Reference = "REF 2"
;
			AssertEquals("1 REF 1\r\n2 REF 2", wrapper.AdditionalSupplyChainActors);
		}

		public void TestDeliveryTerms()
		{
			invoiceHeader.ZG_AgreedPlaceCode = "IEDUB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			AssertEquals("FOB IEDUB", wrapper.DeliveryTerms);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			wrapper = new ImportAccompanyingDocumentWrapper(entryHeader, Factory);
			return new DocumentWrapper[] { wrapper };
		}

		public void TestRouting_IsOrangeForIM460()
		{
			AssertEquals("Routing, no message.", string.Empty, wrapper.Routing);

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

			AssertEquals("Routing, message containing ControlTypeCoded", "ORANGE", wrapper.Routing);
		}

		public void TestRouting_IsGreenWhenIM429IsPresent()
		{
			AssertEquals("Routing, no message.", string.Empty, wrapper.Routing);

			var im429Message = Factory.New<AESInboundEDIMessage>();
			entryHeader.Messages.Add(im429Message);
			im429Message.EM_Status = EDIMessage.Status.ProcessedOK;
			im429Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im429Message.EM_MessageType = "429";

			AssertEquals("When a IM429 is present.", "GREEN", wrapper.Routing);

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

			AssertEquals("Still Green even if there is a newer IM460.", "GREEN", wrapper.Routing);
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Ireland;

		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		ImportAccompanyingDocumentWrapper wrapper;
	}
}
