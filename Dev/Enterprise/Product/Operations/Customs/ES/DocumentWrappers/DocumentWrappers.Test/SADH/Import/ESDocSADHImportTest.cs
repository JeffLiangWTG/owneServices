using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHImportTest : ESDocSADHTest
	{
		public void TestLines()
		{
			var wrapper = GetNewDocumentWrapper() as ESDocSADHImport;
			AssertEquals(typeof(ESDocSADHLineCollectionImport), wrapper.Lines.GetType());
		}

		public void TestPages()
		{
			var wrapper = GetNewDocumentWrapper() as ESDocSADHImport;
			AssertEquals(typeof(ESDocSADHPageCollectionImport), wrapper.Pages.GetType());
		}

		public void TestEPUandENOImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				entryHeader.EntryNumber = "ES0000001";
				AssertEquals("wrapper.EPU", ZString.Empty, wrapper.EPU);
				AssertEquals("wrapper.ENO", ZString.Empty, wrapper.ENO);
			});
		}

		public void TestDOEImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			CusEntryNumber num = CusEntryNumber.New(entryHeader, "IMP", declaration.CountryCode);
			num.CE_EntryNum = "ES0000002";
			num.CE_IssueDate = ZDateTime.BrettsBirthday;
			AssertEquals("wrapper.DOE - from CE_IssueDate", ZDateTime.Empty, wrapper.DOE);
		}

		public void TestLayoutStyle()
		{
			var wrapper = (ESDocSADHImport)GetNewDocumentWrapper();

			CombineAssertions("Test all LayoutStyle properties for ES", () =>
			{
				AssertEquals("LayoutStyle2", "8", wrapper.LayoutStyle2);
				AssertEquals("LayoutStyle2Description", "Copy for consignee", wrapper.LayoutStyle2Description);
			});
		}

		public void TestAgentsReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			CusEntryNumber num = CusEntryNumber.New(entryHeader, "IMP", declaration.CountryCode);
			num.CE_EntryNum = "ES0000003";
			AssertEquals(entryHeader.ReferenceNumber, wrapper.AgentsReference);
		}

		public void TestBox2Address()
		{
			var address1 = Factory.New<OrgAddress>();
			address1.City = "BCN";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "EXPORTER";
			org.MainAddress.City = "MAD";
			org.Addresses.Add(address1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				invoice.JZ_OH_Supplier = org.PK;
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				var cusEntryHeader = declaration.CustomsEntryHeaders[0];
				var wrapper = ESDocSADHImport.New(cusEntryHeader, Factory);
				AssertNotNull("Declaration not null", wrapper);

				AssertEquals("Expected Supplier MainAddres city", "MAD", wrapper.Box2Supplier.MainAddress.City);

				invoice.JZ_OA_SupplierAddress = address1.PK;
				wrapper = ESDocSADHImport.New(cusEntryHeader, Factory);
				AssertEquals("Expected OA_SupplierAddress city", "BCN", wrapper.Box2Supplier.MainAddress.City);
			});
		}

		public void TestBox2SupplierWhenMultipleSuppliers()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.FillWithValidTestData();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

				var invoice = declaration.Invoices.AddNew();
				invoice.InvoiceLines.AddNew();
				var org = Factory.New<OrgHeader>();
				var address1 = Factory.New<OrgAddress>();
				address1.City = "BCN";
				org.OH_Code = "EXPORTER";
				org.MainAddress.City = "MAD";
				org.Addresses.Add(address1);
				invoice.JZ_OH_Supplier = org.PK;

				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var orgHeader2 = Factory.New<OrgHeader>();
				orgHeader2.OH_Code = "EXPORTER2";
				orgHeader2.Addresses.AddNew();
				invoice2.JZ_OH_Supplier = orgHeader2.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				var cusEntryHeader = declaration.CustomsEntryHeaders[0];

				var wrapper = ESDocSADHImport.New(cusEntryHeader, Factory);
				AssertEquals("Expected Name 00200 because there are 2 different suppliers in the entryheader", "00200", wrapper.Box2Supplier.CompanyNameAndAddress);

				invoice2.JZ_OH_Supplier = org.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				cusEntryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = ESDocSADHImport.New(cusEntryHeader, Factory);
				AssertEquals("Expected Name correct because there are 2 suppliers but they are the same", "MAD\nSPAIN", wrapper.Box2Supplier.CompanyNameAndAddress);
			});
		}

		public void TestBox29ExitOfficeImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber num = CusEntryNumber.New(entryHeader, "IMP", declaration.CountryCode);
			num.CE_EntryNum = "ES0000004";
			var officeOfExit = declaration.CustomsOffices.AddNew();
			officeOfExit.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
			officeOfExit.CY_Data = "ES009998";

			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			AssertEquals("Box29ExitOffice", "ES009998", wrapper.Box29ExitOffice);
		}

		public void TestBoxDControlResult()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber num = CusEntryNumber.New(entryHeader, "IMP", declaration.CountryCode);
			num.CE_EntryNum = "ES0000004";

			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Expected Empty if Entry is not accepted", ZString.Empty, wrapper.BoxDControlResult);

				entryHeader.MovementReferenceNumber = "21ES00999912345678";
				AssertEquals("BoxD expected Empty if not ReleaseDate", ZString.Empty, wrapper.BoxDControlResult);

				entryHeader.CH_EntryReleaseDate = new ZDateTime(1995, 2, 16);
				AssertEquals("BoxD expected ReleaseDate if Entry is accepted and has ReleaseDate", "Levante: 16-02-1995", wrapper.BoxDControlResult);
			});
		}

		public void TestBoxDContainerSealsAffixedES()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber num = CusEntryNumber.New(entryHeader, "IMP", declaration.CountryCode);
			num.CE_EntryNum = "ES0000004";

			ESDocSADHImport wrapper = ESDocSADHImport.New(entryHeader, Factory);

			ZString expectedResult = "\r\n\r\n\r\n";

			CombineAssertions(() =>
			{
				AssertEquals("Expected Empty if Entry is not accepted", expectedResult, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "21ES00999912345678";
				entryHeader.MovementReferenceNumberIssueDate = new ZDateTime(1995, 2, 16);
				entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				expectedResult = "Admitido: 16-02-1995\r\n\r\n\r\nC.S.V.: ABCDEFGHIJKLMNOP";
				AssertEquals("BoxD expected AcceptanceDate and CsvClearance", expectedResult, wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestShowEpuEnoDoeLabelsTextES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("Print EPU, ENO, DOE Labels", false, wrapper.ShowEpuEnoDoeLabelsText);
		}

		public void TestBoxABarcodeES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals($"{nameof(ESDocSADHImport.BoxABarcode)} is empty because there is no MRN", ZString.Empty, wrapper.BoxABarcode);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals($"When MRN is set, {nameof(ESDocSADHImport.BoxABarcode)}", "*MRN-TEST*", wrapper.BoxABarcode);

				entryHeader.EntryNumber = "ENUMBER";

				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals($"When EntryNumber is set, {nameof(ESDocSADHImport.BoxABarcode)}", "*ENUMBER*", wrapper.BoxABarcode);
			});
		}

		public void TestBox14DetailsFooterES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_AuthPerDeclaration = false;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals($"{nameof(ESDocSADHImport.Box14DetailsFooter)} is empty because Misc/[14] Authorization by operation is not ticketed", ZString.Empty, wrapper.Box14DetailsFooter);

				declaration.ZG_AuthPerDeclaration = true;

				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals($"When Misc/[14] Authorization by operation is ticketed, {nameof(ESDocSADHImport.Box14DetailsFooter)}", "Authorization: O", wrapper.Box14DetailsFooter);
			});
		}

		public void TestBox14DeclarantRepresentativeES()
		{
			var declarantOrgHeader = Factory.New<OrgHeader>();
			declarantOrgHeader.OH_FullName = "TARIC S.A.";
			var es = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Spain);
			declarantOrgHeader.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, es, "A12345678");

			var declarantAddress = declarantOrgHeader.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Boix y Morer 6 Madrid";

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress - rep 2", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 2", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
			AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 2", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress - rep 3", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 3", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
			AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 3", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress for self rep but Importer not same as Declarant ", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 1", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
			AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 1", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

			declaration.JE_OH_Importer = declarantOrgHeader.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress for self rep and Importer same as Declarant ", false, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("Box 14 should be null for self rep and Importer same as Declarant", null, wrapper.Box14DeclarantRepresentative);

			AssertEquals("Box14AlternativeText should be Consignee", "CONSIGNEE", wrapper.Box14AlternativeText);
		}

		public void TestBox17CountryOfDestinationCodeES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("Test Box17 Country Code data", ZString.Empty, wrapper.Box17CountryOfDestinationCode);
		}

		public void TestBox17ImporterStateES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_DestinationState = "64";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("Test Box17 Importer State", "64", wrapper.Box17ImporterState);
		}

		public void TestShowBox18LabelTextES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("Print Box18 Label always visible", true, wrapper.ShowBox18LabelText);
		}

		public void TestBox20IncoTermCodeES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);

			declaration.ZG_AgreedPlaceCode = "3";
			AssertEquals("Box20AgreedPlaceCode", ZString.Empty, wrapper.Box20AgreedPlaceCode);
			AssertEquals("Box20AgreedPlaceCode2 last box", "3", wrapper.Box20AgreedPlaceCode2);
		}

		public void TestBox27PortOfLoadingES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfLoading = "PORT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);

			AssertEquals("Box27PortOfLoading", ZString.Empty, wrapper.Box27PortOfLoading);
		}

		public void TestBox28FinancialAndBankingDataLine2ES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var line1 = entryHeader.AllEntryLines.AddNew();
			var line2 = entryHeader.AllEntryLines.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Test Box28FinancialAndBankingDataLine2 when there is no Statistical Value", ZString.Empty, wrapper.Box28FinancialAndBankingDataLine2);

				line1.CL_StatisticalValue = 2000.20m;
				line2.CL_StatisticalValue = 3000m;
				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				AssertEquals("Test Box28FinancialAndBankingDataLine2 when there is Statistical Value", "5,000.20", wrapper.Box28FinancialAndBankingDataLine2);
			});
		}

		public void TestBoxBAccountingDetailsES()
		{
			CanaryIslandSetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.ZG_MethodOfPayment2 = "A";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine2.JI_CEI = cei2.PK;

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var wrapper = ESDocSADHImport.New(entryHeader, Factory);

				AssertEquals("Test BoxBAccountingDetails when there is no Guarantees", ZString.Empty, wrapper.BoxBAccountingDetails);

				var guarantee1 = declaration.Guarantees.AddNew();
				guarantee1.PW_BondNumber = "16ESAGL9990000096";
				guarantee1.PW_BondAmount = 1m;
				guarantee1.EntryInstructionID = cei.PK;
				var guarantee2 = declaration.Guarantees.AddNew();
				guarantee2.PW_BondNumber = "16ESAGL9990000097";
				guarantee2.PW_BondAmount = 0m;
				guarantee2.EntryInstructionID = cei.PK;
				var guarantee3 = declaration.Guarantees.AddNew();
				guarantee3.PW_BondNumber = "16ESAGL9990000098";
				guarantee3.PW_BondAmount = 2m;
				guarantee3.EntryInstructionID = cei.PK;
				var guarantee4 = declaration.Guarantees.AddNew();
				guarantee4.PW_BondNumber = "16ESAGL9990000099";
				guarantee4.PW_BondAmount = 1m;
				guarantee4.EntryInstructionID = cei2.PK;
				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				var expectedResult = "16ESAGL9990000096\r\n16ESAGL9990000098";
				AssertEquals("Test BoxBAccountingDetails when there are guarantees", expectedResult, wrapper.BoxBAccountingDetails);

				declaration.ZG_DestinationState = Enterprise.Customs.ES.Business.CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
				wrapper = ESDocSADHImport.New(entryHeader, Factory);
				expectedResult = "MP A.T.C.: A\r\n16ESAGL9990000096\r\n16ESAGL9990000098";
				AssertEquals("Test BoxBAccountingDetails when is a Canary Island Declaration", expectedResult, wrapper.BoxBAccountingDetails);
			});
		}

		public void TestShowBoxDLabelsTextES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			AssertEquals("Print BoxD Labels", false, wrapper.ShowBoxDLabelsText);
		}

		public void TestLabelsCaptionsES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ESDocSADHImport.New(entryHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("DispatchOfficeTitleCaption Test", "A OFFICE OF DESTINATION", wrapper.DispatchOfficeTitleCaption);
				AssertEquals("DispatchOfficeTitleBisPageCaption Test", "OFFICE OF DESTINATION", wrapper.DispatchOfficeTitleBisPageCaption);
				AssertEquals("Box10LabelPart1Caption Test", "Last Country/Region", wrapper.Box10LabelPart1Caption);
				AssertEquals("Box10LabelPart2Caption Test", "Proc.", wrapper.Box10LabelPart2Caption);
				AssertEquals("Box11LabelPart1Caption Test", "Trad. Country/Region/", wrapper.Box11LabelPart1Caption);
				AssertEquals("Box11LabelPart2Caption Test", "Prod.", wrapper.Box11LabelPart2Caption);
				AssertEquals("Box18LabelCaption Test", "18 Identity and nationality of means of transport on arrival", wrapper.Box18LabelCaption);
				AssertEquals("Box27LabelCaption Test", "27 Place of unloading", wrapper.Box27LabelCaption);
				AssertEquals("Box29LabelCaption Test", "29 Office of entry", wrapper.Box29ExitOfficeLabel);
				AssertEquals("BoxDLetterLabelCaption Test", "J", wrapper.BoxDLetterLabelCaption);
				AssertEquals("BoxDLabelCaption Test", "CONTROL BY OFFICE OF DESTINATION", wrapper.BoxDLabelCaption);
				AssertEquals("Box43LabelCaption Test", "Cod M.E.", wrapper.Box43LabelCaption);
				AssertEquals("Box31PackagesAndDescriptionOfGoodsCaption Test", "Marks and numeration - Container(s) number(s) – Number and class", wrapper.Box31PackagesAndDescriptionOfGoodsCaption);
			});
		}

		void CanaryIslandSetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain + "C", parent: grouping);
			helper.CreateCusCodeList("ESC", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "64", "Test 61", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		protected override DocSADH GetNewDocSADH(ESCusEntryHeader entryHeader)
		{
			return ESDocSADHImport.New(entryHeader, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return ESDocSADHImport.New(entryHeader, Factory);
		}

		protected override ZString ExpectedBox20AgreedPlaceCode => ZString.Empty;

		protected override ZString ExpectedBox20AgreedPlaceCode2 => "3";

		protected override ZString GetDeclarationType => MessageTypeList.Codes.Import;
	}
}
