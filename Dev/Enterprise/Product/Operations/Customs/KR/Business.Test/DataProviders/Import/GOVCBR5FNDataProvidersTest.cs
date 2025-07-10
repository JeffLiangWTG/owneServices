using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FNDataProvidersTest : XMLMessageTestHelper<GOVCBR5FNDataProvidersTest>
	{
		void SetUpTariffData()
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.IsDutyExempt, "Y", tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();
			#endregion
		}

		public void TestSerialisationAndDeserialisationRealData()
		{
			SetUpTariffData();

			#region Branch
			var glbOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "Branch", "Branch 상호");
			TestOrgDataSetUpHelper.AddOrgAddress(glbOrgHeader.MainAddress, "기본주소", "상세주소");
			glbOrgHeader.MainAddress.OA_Mobile = "999-9999-9999";
			glbOrgHeader.MainAddress.OA_Fax = "99999999999";
			glbOrgHeader.MainAddress.OA_Email = "Brocker@wise.com";
			glbOrgHeader.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			#endregion

			#region Header
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "040";
			declaration.JE_CustomsDivision = "12";
			declaration.JE_PaidBy = ZString.Empty;
			var countryCode = declaration.CountryCode;

			declaration.Branch.GB_OH_OrgProxy = glbOrgHeader.PK;
			declaration.Branch.GB_OA_AddressProxy = glbOrgHeader.MainAddress.PK;
			glbOrgHeader.MainAddress.OA_RL_NKRelatedPortCode = declaration.Branch.GB_RL_NKHomePort;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();

			entry.EntryNumber = "2292620002733M";
			entryNumber.CE_EntryNum = "2292620002733M";
			entryNumber.CE_EntryLineReference = "1";
			#endregion

			#region IOrganization
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST", "크리스챤디올꾸뛰르코리아(주)");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "1208174197" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			declaration.JE_OH_DutyPayer = payer.PK;

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "2", "상호1");
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "수입자주소", "", "");
			importer.MainAddress.OA_Phone = "02-541-1834";
			var organisationWrapper = OrgHeaderWrapper.New(importer);
			organisationWrapper.ZO_TypeOfBusiness = "업태";

			var buyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "2", "상호2");
			TestOrgDataSetUpHelper.AddOrgAddress(buyer.MainAddress, "수입자주소", "", "");

			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			#endregion

			#region Line
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_BuyerAddress = buyer.MainAddress.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "4203400000";
			invoiceLine.JI_Model = "";
			invoiceLine.JI_SerialNumber = "";
			invoiceLine.JI_SpecificUseCodeDescription = "주문수집 후 재수출 예정";
			invoiceLine.JI_SpecificUseProductType = "99";
			invoiceLine.JI_DutyReductionRateRegulationCode = "01:12:45";
			invoiceLine.JI_ScheduledReExportCustomsOffice = "040";
			invoiceLine.JI_RN_NKReExportDestinationCountry = "FR";
			invoiceLine.JI_ScheduledReExportDate = new ZDateTime(2020, 10, 31);
			invoiceLine.JI_PostClearanceProcedureGA1 = "";
			invoiceLine.JI_PostClearanceProcedureGA2 = "";
			invoiceLine.JI_PostClearanceProcedureGA3 = "";
			invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = ZString.Empty;
			invoiceLine.JI_InstallmentCode = "A093000004";
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = countryCode;
			invoiceLine.AdditionalInformationContent = "감세 감면 처음 신청 합니다. 빠른 처리 부탁 드립니다.";
			invoiceLine.JI_JurisdictionalCusOffice = "";
			invoiceLine.JI_PCProcedure = "N";

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var goodsLocation = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "1", "설치장소");
			TestOrgDataSetUpHelper.AddOrgAddress(goodsLocation.MainAddress, "서울시 강남구 도산대로 458 (청담동,리츠타워 701호,801호)", "리츠타워 701호,801호)", "06062", "도로명", "빌딩번호");
			goodsLocation.MainAddress.OA_Phone = "02-513-3220";

			invoiceLine.JI_OA_ConsigneeAddress = goodsLocation.MainAddress.PK;
			#endregion

			var import5FNLine = new Import5FNLineCreator().Create(entryLine);

			var result = new GOVCBR5FNMessageBuilder(import5FNLine).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5FNDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
			AssertEquals(1, import5FNLine.EntryLineNo);
			AssertEquals("4203400000", import5FNLine.HSCode);
			AssertEquals("", import5FNLine.ModelName);

			AssertEquals("", import5FNLine.SerialNumber);
			AssertEquals("주문수집 후 재수출 예정", import5FNLine.UseCodeDescription);
			AssertEquals("99", import5FNLine.ProductTypeCode);
			AssertEquals("N", import5FNLine.PostClearanceProcedureYN);

			AssertEquals("01", import5FNLine.ReductionRateRegulationGroupNumber);
			AssertEquals("12", import5FNLine.ReductionRateRegulationSeqNumber);
			AssertEquals("45", import5FNLine.ReductionRateRegulationItemNumber);

			AssertEquals("040", import5FNLine.ScheduledReExportCustomsOffice);
			AssertEquals("FR", import5FNLine.ReExportDestinationCountryCode);
			AssertEquals("20201031", import5FNLine.ScheduledReExportDate.ToString("yyyyMMdd"));

			AssertEquals("2292620002733M", import5FNLine.Header.ImportDeclarationNumber);
			AssertEquals("040", import5FNLine.Header.DeclarationCustomsOffice);
			AssertEquals("12", import5FNLine.Header.DeclarationCustomsDivision);
			AssertEquals("", import5FNLine.JurisdictionalCustomsOffice);
			AssertEquals("업태", import5FNLine.Header.TypeOfBusiness);

			AssertEquals("크리스챤디올꾸뛰르코리아(주)", import5FNLine.Header.Payer.CompanyName);
			AssertEquals("", import5FNLine.Header.Payer.FaxNumber);
			AssertEquals("", import5FNLine.Header.Payer.MobileNumber);
			AssertEquals("", import5FNLine.Header.Payer.Email);
			AssertEquals("1208174197", import5FNLine.Header.Payer.ForeignCompanyID);

			AssertEquals("02-513-3220", import5FNLine.GoodsLocation.PhoneNumber);
			AssertEquals("06062", import5FNLine.GoodsLocation.Postcode);
			AssertEquals("도로명", import5FNLine.GoodsLocation.RoadNameCode);
			AssertEquals("빌딩번호", import5FNLine.GoodsLocation.BuildingNumber);
			AssertEquals("서울시 강남구 도산대로 458 (청담동,리츠타워 701호,801호)", import5FNLine.GoodsLocation.AddressLine1);
			AssertEquals("리츠타워 701호,801호)", import5FNLine.GoodsLocation.AddressLine2);

			AssertEquals("Branch 상호", import5FNLine.Header.CustomsBroker.CompanyName);
			AssertEquals("99999999999", import5FNLine.Header.CustomsBroker.FaxNumber);
			AssertEquals("999-9999-9999", import5FNLine.Header.CustomsBroker.MobileNumber);
			AssertEquals("Brocker@wise.com", import5FNLine.Header.CustomsBroker.Email);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		JobDeclaration CreateEmptyDeclaration()
		{
			SetUpTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsOffice = "040";
			declaration.JE_CustomsDivision = "12";

			#region Line
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "4203400000";
			invoiceLine.JI_Model = "";
			invoiceLine.JI_SerialNumber = "";
			invoiceLine.JI_SpecificUseCodeDescription = "주문수집 후 재수출 예정";
			invoiceLine.JI_ProductTypeCode = "99";
			invoiceLine.JI_DutyReductionRateRegulationCode = "01:12:45";
			invoiceLine.JI_ScheduledReExportCustomsOffice = "040";
			invoiceLine.JI_RN_NKReExportDestinationCountry = "FR";
			invoiceLine.JI_ScheduledReExportDate = new ZDateTime(2020, 10, 31);
			invoiceLine.JI_PostClearanceProcedureGA1 = "";
			invoiceLine.JI_PostClearanceProcedureGA2 = "";
			invoiceLine.JI_PostClearanceProcedureGA3 = "";
			invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = ZString.Empty;
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_CountryOfOrigin = declaration.CountryCode;
			invoiceLine.AdditionalInformationContent = "재수출 하기위한 감면 신청입니다.";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			#endregion

			return declaration;
		}

		public void TestPayerBussiness()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST", "크리스챤디올꾸뛰르코리아(주)");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1208174197" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			declaration.JE_OH_DutyPayer = payer.PK;
			var import5FNLine = new Import5FNLineCreator().Create(entry.MergedLines[0]);
			AssertEquals("1208174197", import5FNLine.Header.Payer.BusinessRegNo);
		}

		public void TestPayerNaturalPerson()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, OrgConstants.Category.NaturalPersonIndividual, "KRTEST", "상호1");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "9999999999999" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			declaration.JE_OH_DutyPayer = payer.PK;
			var import5FNLine = new Import5FNLineCreator().Create(entry.MergedLines[0]);
			AssertEquals("9999999999999", import5FNLine.Header.Payer.KoreanRegNoForResident);
		}

		void AssertDutyReductionClassification(ZString preference, ZString installmentCode, bool isSpecificUseCode, ZString expectedValue)
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine = entry.MergedLines[0].RandomLine;
			invoiceLine.JI_SecondaryPreference = preference;
			invoiceLine.JI_InstallmentCode = installmentCode;
			invoiceLine.JI_IsSpecificUseCode = isSpecificUseCode;

			var import5FNLine = new Import5FNLineCreator().Create(entry.MergedLines[0]);

			AssertEquals(expectedValue, import5FNLine.DutyReductionClassification);
		}

		public void TestDutyReductionClassificationCodeA()
		{
			AssertDutyReductionClassification("A093000004", "", false, ImportDutyReductionClassificationList.Codes.DutyExemption);
		}

		public void TestDutyReductionClassificationCodeB()
		{
			AssertDutyReductionClassification("A1070001", "", false, ImportDutyReductionClassificationList.Codes.DutyReduction);
		}

		public void TestDutyReductionClassificationCodeC()
		{
			AssertDutyReductionClassification("", "A1070001", false, ImportDutyReductionClassificationList.Codes.InstallmentPayment);
		}

		public void TestDutyReductionClassificationCodeD()
		{
			AssertDutyReductionClassification("", "", true, ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly);
		}

		public void TestDutyReductionClassificationCodeT()
		{
			AssertDutyReductionClassification("", "A093000004", true, ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment);
		}
	}
}
