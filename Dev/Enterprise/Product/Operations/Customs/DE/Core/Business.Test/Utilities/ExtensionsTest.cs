using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestHasPreviousDocumentsOfType_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				var invoice = Factory.New<JobComInvoiceHeader>();
				AssertEquals("No previousDocs", false, invoice.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.PreviousDocuments.AddNew();
				AssertEquals("Added previousDoc with no type", false, invoice.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.PreviousDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertEquals("Added previousDoc N830", false, invoice.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.PreviousDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DAB;
				AssertEquals("Added previousDoc 9DAB", true, invoice.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));
			});
		}

		public void TestHasPreviousDocumentsOfType_JobComInvoiceLine()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				AssertEquals("No previousDocs", false, invoiceLine.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.PreviousDocuments.AddNew();
				AssertEquals("Added previousDoc with no type", false, invoiceLine.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.PreviousDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertEquals("Added previousDoc N830", false, invoiceLine.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.PreviousDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DAB;
				AssertEquals("Added previousDoc 9DAB", true, invoiceLine.HasPreviousDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));
			});
		}

		public void TestHasSupportingDocumentsOfType_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				var invoice = Factory.New<JobComInvoiceHeader>();
				AssertEquals("No supportingDocs", false, invoice.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.SupportingDocuments.AddNew();
				AssertEquals("Added supportingDoc with no type", false, invoice.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertEquals("Added supportingDoc N830", false, invoice.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoice.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DAB;
				AssertEquals("Added supportingDoc 9DAB", true, invoice.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));
			});
		}

		public void TestHasSupportingDocumentsOfType_JobComInvoiceLine()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				AssertEquals("No supportingDocs", false, invoiceLine.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.SupportingDocuments.AddNew();
				AssertEquals("Added supportingDoc with no type", false, invoiceLine.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertEquals("Added supportingDoc N830", false, invoiceLine.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));

				invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._9DAB;
				AssertEquals("Added supportingDoc 9DAB", true, invoiceLine.HasSupportingDocumentsOfType(UniversalReferenceConstants.SupportingDocumentTypes._9DAB));
			});
		}

		public void TestHasConcessionOfType()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				AssertEquals("No concession", false, invoiceLine.HasConcessionOfType(UniversalReferenceConstants.Concessions._8E3));

				invoiceLine.JI_Procedure = $"{CustomsProcedureCodeList.Import.ProcedureCode._42}{CustomsProcedureCodeList.Import.PreviousProcedureCode._21}{UniversalReferenceConstants.Concessions._6F0}";
				AssertEquals("Concession <> 8E3", false, invoiceLine.HasConcessionOfType(UniversalReferenceConstants.Concessions._8E3));

				invoiceLine.JI_Procedure = $"{CustomsProcedureCodeList.Import.ProcedureCode._42}{CustomsProcedureCodeList.Import.PreviousProcedureCode._21}{UniversalReferenceConstants.Concessions._8E3}";
				AssertEquals("Concession 8E3", true, invoiceLine.HasConcessionOfType(UniversalReferenceConstants.Concessions._8E3));
			});
		}

		public void TestGetDefermentAccounts()
		{
			OrgHeader org = null;
			AssertNotNull(org.GetDefermentAccounts());

			org = Factory.New<OrgHeader>();
			org.AddDefermentAccountNumber("10", "1111");
			org.AddDefermentAccountNumber("10", "1111", "AU");

			AssertEquals(1, org.GetDefermentAccounts().Count());
			Assert(org.GetDefermentAccounts().All(x => x.CZ_RN_NKCountryCode == Core.Constants.CountryCodes.Germany));
		}

		public void TestXmlEnumToString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Returns Enum Name", "M", Gender.M.XmlEnumToString());
				AssertEquals("Returns Enum Attribute", "Female", Gender.F.XmlEnumToString());
				AssertEquals("Invalid Value", ZString.Empty, ((Gender)3).XmlEnumToString());
			});
		}

		public void TestGetCustomsDeclarationValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
			Factory.Save();
			helper.CreateTaxOrFee("DV1", 20000.1m, "EUN", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			helper.CreateTaxOrFee("DV1", 20000.2m, "EUN", ZDate.Today.AddDays(1), ZDate.Today.AddDays(7));
			helper.CreateTaxOrFee("DV1", 20000.3m, "IT", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			Factory.Save();

			AssertEquals(20000.1m, Factory.GetCustomsDeclarationValue());
		}

		public void TestGetATLASParticipantIdentificationNumber()
		{
			AssertEquals(ZString.Empty, Extensions.GetATLASParticipantIdentificationNumber(null));
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			AssertEquals(ZString.Empty, orgAddress.GetATLASParticipantIdentificationNumber());
			var customsCode = orgAddress.CustomsCodes.AddNew();
			customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			customsCode.OK_CustomsRegNo = "ATLAS123456";
			AssertEquals(ZString.Empty, orgAddress.GetATLASParticipantIdentificationNumber());
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			AssertEquals("ATLAS123456", orgAddress.GetATLASParticipantIdentificationNumber());
			customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode;
			AssertEquals(ZString.Empty, orgAddress.GetATLASParticipantIdentificationNumber());
		}

		public void TestGetCustomsRegNoOrgHeader()
		{
			AssertEquals(ZString.Empty, Extensions.GetCustomsRegNo((OrgHeader)null, ZString.Empty));
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(ZString.Empty, orgHeader.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			customsCode.OK_CustomsRegNo = "BERTH1";
			AssertEquals(ZString.Empty, orgHeader.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			AssertEquals("BERTH1", orgHeader.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
		}

		public void TestGetCustomsRegNoOrgAddress()
		{
			AssertEquals(ZString.Empty, Extensions.GetCustomsRegNo((OrgAddress)null, ZString.Empty));
			var orgAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			AssertEquals(ZString.Empty, orgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
			var customsCode = orgAddress.CustomsCodes.AddNew();
			customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			customsCode.OK_CustomsRegNo = "BERTH2";
			AssertEquals(ZString.Empty, orgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			AssertEquals("BERTH2", orgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode));
		}

		public void TestAddPrefixToNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Prefixed", "GR123456", new ZString("123456").AddPrefixToNumber("GR"));
				AssertEquals("Empty number", ZString.Empty, ZString.Empty.AddPrefixToNumber("GR"));
			});
		}

		public void TestGetOfficeReferenceNumber_JobDeclaration()
		{
			AssertEquals(ZString.Empty, Extensions.GetOfficeReferenceNumber(null, ZString.Empty));
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice));
			var customsOffice = declaration.CustomsOffices.Cast<DEOfficeCode>()
										.FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice)
								?? declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			customsOffice.CY_Data = "OFFICE1";
			AssertEquals("OFFICE1", declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice));
		}

		public void TestGetAllSameValue()
		{
			CusEntryHeader entry = null;
			AssertEquals(ZString.Empty, entry.GetAllSameValue<ZString>(null));
			AssertEquals(ZDecimal.Zero, entry.GetAllSameValue<ZDecimal>(null));
			var declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(ZString.Empty, entry.GetAllSameValue(x => x.JZ_ValuationCode));
			AssertEquals(ZDecimal.Zero, entry.GetAllSameValue(x => x.JZ_InvoiceAmount));
			var entryLine = entry.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = "3";
			invoice1.JZ_InvoiceAmount = 2.22m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "3";
			invoice2.JZ_InvoiceAmount = 2.22m;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entry.ResetInvoiceHeadersAndLines();
			AssertEquals("3", entry.GetAllSameValue(x => x.JZ_ValuationCode));
			AssertEquals(2.22m, entry.GetAllSameValue(x => x.JZ_InvoiceAmount));
			invoice1.JZ_ValuationCode = "3";
			invoice1.JZ_InvoiceAmount = 2.22m;
			invoice2.JZ_ValuationCode = "4";
			invoice2.JZ_InvoiceAmount = 4.22m;
			AssertEquals(ZString.Empty, entry.GetAllSameValue(x => x.JZ_ValuationCode));
			AssertEquals(ZDecimal.Zero, entry.GetAllSameValue(x => x.JZ_InvoiceAmount));
		}

		public void TestGetRegLine()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "ATB150000960420195875";

			CombineAssertions(() =>
			{
				var result = regHeader.GetRegLine(ZString.Empty);
				AssertNull("CusTempStorageRegLine exists", result);
				result = regHeader.GetRegLine("1");
				AssertNull("CusTempStorageRegLine exists", result);

				var regLine = regHeader.CusTempStorageRegLines.AddNew();
				regLine.SRL_LineNumber = 1;
				regLine.SRL_LimitDate = ZDate.Today;
				regLine.SRL_LocationOfGoods = "SYD";
				result = regHeader.GetRegLine("1");
				AssertNotNull("CusTempStorageRegLine not found", result);
			});
		}

		public void TestGetCountriesInC0063()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0063", "C0063");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "C0063", "AD", "Andorra", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals(true, Factory.GetCountriesInC0063().ContainsCode("AD"));
		}

		public void TestCalculatePackageQtySumFromTransactions()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			AssertEquals(0, regLine.CalculatePackageQtySumFromTransactions());

			var transaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_PackageQty = 5;
			AssertEquals(5, regLine.CalculatePackageQtySumFromTransactions());

			var transaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -6;
			AssertEquals(-1, regLine.CalculatePackageQtySumFromTransactions());
		}

		public void TestHasEoriRegNo()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);

				AssertEquals("OrgHeader Null", ZBool.False, Extensions.HasEUEoriRegNo(null));

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				AssertEquals("No Eori", ZBool.False, orgHeader.HasEUEoriRegNo());

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Netherlands);
				AssertEquals("Single Eori", ZBool.True, orgHeader.HasEUEoriRegNo());

				var eori2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "94567", Core.Constants.CountryCodes.France);
				AssertEquals("Multiple Eori", ZBool.False, orgHeader.HasEUEoriRegNo());

				eori2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("Single EU CL010 Eori", ZBool.True, orgHeader.HasEUEoriRegNo());
			});
		}

		public void TestHasEoriOrTcu_Eori()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgHeader Null", ZBool.False, Extensions.HasEoriOrTcu(null));
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				AssertEquals("No Eori or Tcu", ZBool.False, orgHeader.HasEoriOrTcu());
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Netherlands);
				AssertEquals("Eori", ZBool.True, orgHeader.HasEoriOrTcu());
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "94567", Core.Constants.CountryCodes.France);
				AssertEquals("Tcu and Eori", ZBool.True, orgHeader.HasEoriOrTcu());
			});
		}

		public void TestHasEoriOrTcu_Tcu()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Netherlands);
			AssertEquals("Tcu", ZBool.True, orgHeader.HasEoriOrTcu());
		}

		public void TestGetOrgCusCode()
		{
			var expectedCodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			var expectedRegNum = "0000";
			CombineAssertions(() =>
			{
				AssertNull("OrgHeader is null", Extensions.GetOrgCusCode(null, expectedCodeType, expectedRegNum));
				var orgHeader = Factory.New<OrgHeader>();
				Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, expectedCodeType
					, Core.Constants.CountryCodes.Germany
					, expectedRegNum);
				AssertNull("OrgCusCode has different code type", orgHeader.GetOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, expectedRegNum));
				AssertNull("OrgCusCode has different regnum", orgHeader.GetOrgCusCode(expectedCodeType, "987654321"));
				var cusCode = orgHeader.GetOrgCusCode(expectedCodeType, expectedRegNum);
				AssertNotNull("OrgCusCode successfully obtained", cusCode);
				AssertEquals("Obtained OrgCusCode has correct code type", expectedCodeType, cusCode.OK_CodeType);
				AssertEquals("Obtained OrgCusCode has correct regnum", expectedRegNum, cusCode.OK_CustomsRegNo);
			});
		}

		public void TestGetOrgHeaderByCustomsRegNo_Factory()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001");
			Factory.Save();
			AssertNull(Extensions.GetOrgHeaderByCustomsRegNo(null, Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgHeaderByCustomsRegNo_CountryCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, ZString.Empty
					, "TE001");
			Factory.Save();
			AssertNull(Factory.GetOrgHeaderByCustomsRegNo(ZString.Empty, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgHeaderByCustomsRegNo_CodeType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, ZString.Empty);
			Factory.Save();
			AssertNull(Factory.GetOrgHeaderByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgHeaderByCustomsRegNo_RegistrationNumber()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader1.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001");

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader2.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE002");
			Factory.Save();
			CombineAssertions(() =>
			{
				var orgHeader3 = Factory.GetOrgHeaderByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001");
				AssertEquals("Get OrgHeader with RegNo TE001", orgHeader1.PK, orgHeader3.PK);

				var orgHeader4 = Factory.GetOrgHeaderByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE003");
				AssertEquals("Cannot find match orgHeader", null, orgHeader4);
			});
		}

		public void TestGetOrgHeaderByCustomsRegNo_OrderBy()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader1.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001");
			Factory.Save();

			orgHeader1.OH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader2.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001");
			Factory.Save();

			var oldestOrgHeader = Factory.GetOrgHeaderByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001");
			AssertEquals("Correct Order", orgHeader1.PK, oldestOrgHeader.PK);
		}

		public void TestGetOrgAddressByCustomsRegNo_Factory()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001"
					, orgAddress.PK);
			Factory.Save();
			AssertNull(Extensions.GetOrgAddressByCustomsRegNo(null, Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgAddressByCustomsRegNo_CountryCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, ZString.Empty
					, "TE001"
					, orgAddress.PK);
			Factory.Save();
			AssertNull(Factory.GetOrgAddressByCustomsRegNo(ZString.Empty, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgAddressByCustomsRegNo_CodeType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, ZString.Empty
					, orgAddress.PK);
			Factory.Save();
			AssertNull(Factory.GetOrgAddressByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001"));
		}

		public void TestGetOrgAddressByCustomsRegNo_RegistrationNumber()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "A1";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader1.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001"
					, orgAddress1.PK);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "A2";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader2.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE002"
					, orgAddress2.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				var orgAddress3 = Factory.GetOrgAddressByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001");
				AssertEquals("Get orgAddress with RegNo TE001", orgAddress1.PK, orgAddress3.PK);

				var orgAddress4 = Factory.GetOrgAddressByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE003");
				AssertEquals("Cannot find match orgAddress", null, orgAddress4);
			});
		}

		public void TestGetOrgAddressByCustomsRegNo_OrderBy()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "A1";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader1.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001"
					, orgAddress1.PK);
			Factory.Save();

			orgAddress1.OA_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "A2";
			Factory.New<OrgCusCode>().ModifyOrgCusCode(orgHeader2.PK
					, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber
					, Core.Constants.CountryCodes.Germany
					, "TE001"
					, orgAddress2.PK);
			Factory.Save();

			var oldestOrgAddress = Factory.GetOrgAddressByCustomsRegNo(Core.Constants.CountryCodes.Germany, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TE001");
			AssertEquals("Correct Order", orgAddress1.PK, oldestOrgAddress.PK);
		}

		public void TestGetCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
			AssertEquals(charge, invoiceLine.GetCharge(ImportChargeCodeList.Codes.TCE));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_NAR()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "CIG", 0.301491m);
			AssertEquals(0.301491m, Factory.GetTobaccoRetailSellingPriceOrZero("NAR"));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_KGM()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "TAB", 152.2906m);
			AssertEquals(152.2906m, Factory.GetTobaccoRetailSellingPriceOrZero("KGM"));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_InvalidUq()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "TAB", 152.2906m);
			AssertEquals(0m, Factory.GetTobaccoRetailSellingPriceOrZero("GRM"));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_NonPresentTaxOrFeeCode()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "XXX", 0.301491m);
			AssertEquals(0m, Factory.GetTobaccoRetailSellingPriceOrZero("NAR"));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_NonPresentTaxOrFeeTypeTSP()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "XXX", "Miscellaneous Type", "CIG", 0.301491m);
			AssertEquals(0m, Factory.GetTobaccoRetailSellingPriceOrZero("NAR"));
		}

		public void TestGetTobaccoRetailSellingPriceOrFallbackToZero_FactoryNull()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "CIG", 0.301491m);
			AssertEquals(0m, Extensions.GetTobaccoRetailSellingPriceOrZero(null, "NAR"));
		}

		public void TestGetCusReconEntry()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var cusReconEntry = Factory.New<CusReconEntry>();
			CombineAssertions(() =>
			{
				AssertNull("Parameter entryHeader is null", ((CusEntryHeader)null).GetCusReconEntry());
				AssertNull("No CusReconEntry with matching CRE_CH_OriginalEntry", entryHeader.GetCusReconEntry());
				cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
				AssertEquals("Has CusReconEntry with matching CRE_CH_OriginalEntry", cusReconEntry, entryHeader.GetCusReconEntry());
			});
		}

		public void TestGetCusReconEntryFromDB()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_EntryType = "AA";
			cusReconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			CombineAssertions(() =>
			{
				AssertNull("Not in DB", entryHeader.GetCusReconEntryFromDB());

				Factory.Save();
				AssertEquals("In DB", cusReconEntry, entryHeader.GetCusReconEntryFromDB());
			});
		}

		public void TestGetCusReconEntryLineByOriginalEntryLineNumber()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			CombineAssertions(() =>
			{
				AssertNull("Parameter cusReconEntry is null", ((CusReconEntry)null).GetCusReconEntryLineByOriginalEntryLineNumber("1"));
				AssertNull("No CusReconEntryLines", cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("1"));
				var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
				cusReconEntryLine1.CRL_OriginalEntryLineNumber = 1;
				var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
				cusReconEntryLine2.CRL_OriginalEntryLineNumber = 2;
				AssertNull("No CusReconEntryLine with matching CRL_OriginalEntryLineNumber", cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("3"));
				AssertNull("Parameter originalEntryLineNumber is not a number", cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("1b"));
				AssertEquals("CusReconEntryLine with matching CRL_OriginalEntryLineNumber = 1", cusReconEntryLine1, cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("1"));
				AssertEquals("CusReconEntryLine with matching CRL_OriginalEntryLineNumber = 2", cusReconEntryLine2, cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("2"));
			});
		}

		public void TestsDuplicateCusReconEntryAndUnlinkFromDeclaration_Null()
		{
			AssertNull(Extensions.DuplicateCusReconEntryAndUnlinkFromDeclaration(null));
		}

		public void TestsDuplicateCusReconEntryAndUnlinkFromDeclaration()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var entry = declaration.CusReconEntries.AddNew();
			entry.CRE_OriginalEntryNumber = "12345";
			entry.CRE_EntryDate = new ZDate(2022, 01, 12);
			entry.CRE_EntryType = "X";
			entry.CRE_OA_DeclarantAddress = ZGuid.NewZGuid();
			entry.CRE_OA_ImporterAddress = ZGuid.NewZGuid();
			entry.CRE_OA_RepresentativeAddress = ZGuid.NewZGuid();
			entry.CRE_OA_BuyingAgentAddress = ZGuid.NewZGuid();
			entry.CRE_CH_OriginalEntry = ZGuid.NewZGuid();
			entry.CusReconEntryLines.AddNew();

			var entrySnapshot1 = entry.CusReconSnapshots.AddNew();
			entrySnapshot1.CRS_Type = CusReconConstants.Lodged;
			entrySnapshot1.CRS_SnapshotXml = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""></DEMonthlyClosingEntrySnapshot>";
			var entrySnapshot2 = entry.CusReconSnapshots.AddNew();
			entrySnapshot2.CRS_Type = CusReconConstants.Current;
			entrySnapshot2.CRS_SnapshotXml = "Snaphot2 XML";

			var result = entry.DuplicateCusReconEntryAndUnlinkFromDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("OriginalEntry CRE_CRD not updated", entry.CRE_CRD, declaration.PK);
				AssertSequencesEqual("OriginalEntrySnapshots not changed", new[] { entrySnapshot1, entrySnapshot2 }, entry.CusReconSnapshots);

				AssertNotEquals("CRE_PK", entry.PK, result.PK);
				AssertEquals("CRE_CRD", ZGuid.Empty, result.CRE_CRD);
				AssertEquals("CRE_OriginalEntryNumber", entry.CRE_OriginalEntryNumber, result.CRE_OriginalEntryNumber);
				AssertEquals("CRE_EntryDate", entry.CRE_EntryDate, result.CRE_EntryDate);
				AssertEquals("CRE_EntryType", entry.CRE_EntryType, result.CRE_EntryType);
				AssertEquals("CRE_GB_Branch", entry.CRE_GB_Branch, result.CRE_GB_Branch);
				AssertEquals("CRE_OA_DeclarantAddress", entry.CRE_OA_DeclarantAddress, result.CRE_OA_DeclarantAddress);
				AssertEquals("CRE_OA_ImporterAddress", entry.CRE_OA_ImporterAddress, result.CRE_OA_ImporterAddress);
				AssertEquals("CRE_OA_RepresentativeAddress", entry.CRE_OA_RepresentativeAddress, result.CRE_OA_RepresentativeAddress);
				AssertEquals("CRE_OA_BuyingAgentAddress", entry.CRE_OA_BuyingAgentAddress, result.CRE_OA_BuyingAgentAddress);
				AssertEquals("CRE_CH_OriginalEntry", entry.CRE_CH_OriginalEntry, result.CRE_CH_OriginalEntry);
				AssertEquals("CusReconEntryLines not copied", false, result.CusReconEntryLines.Any());

				var copiedEntrySnapshots = result.CusReconSnapshots;
				AssertEquals("CopiedEntrySnapshots Count", 2, copiedEntrySnapshots.Count);
				var copiedEntrySnapshot1 = copiedEntrySnapshots[0];
				AssertEquals("copiedEntrySnapshot1 CRS_Type", entrySnapshot1.CRS_Type, copiedEntrySnapshot1.CRS_Type);
				AssertEquals("copiedEntrySnapshot1 CRS_SnapshotXml", entrySnapshot1.CRS_SnapshotXml, copiedEntrySnapshot1.CRS_SnapshotXml);
				var copiedEntrySnapshot2 = copiedEntrySnapshots[1];
				AssertEquals("copiedEntrySnapshot2 CRS_Type", entrySnapshot2.CRS_Type, copiedEntrySnapshot2.CRS_Type);
				AssertEquals("copiedEntrySnapshot2 CRS_SnapshotXml", entrySnapshot2.CRS_SnapshotXml, copiedEntrySnapshot2.CRS_SnapshotXml);
			});
		}

		public void TestGetUnlinkedCusReconEntryByOriginalEntryNumber()
		{
			var entry1 = Factory.New<CusReconEntry>();
			var entry2 = Factory.New<CusReconEntry>();
			entry2.CRE_OriginalEntryNumber = "2";
			var entry3 = Factory.New<CusReconEntry>();
			entry3.CRE_CRD = Factory.New<CusReconDeclaration>().PK;
			entry3.CRE_OriginalEntryNumber = "2";
			var entry4 = Factory.New<CusReconEntry>();
			entry4.CRE_GB_Branch = Factory.New<GlbBranch>().PK;
			entry4.CRE_OriginalEntryNumber = "2";
			var entry5 = Factory.New<CusReconEntry>();
			entry5.CRE_OriginalEntryNumber = "3";
			var entry6 = Factory.New<CusReconEntry>();
			entry6.CRE_OriginalEntryNumber = "3";

			var originalEntry = Factory.New<CusReconEntry>();
			originalEntry.CRE_CRD = Factory.New<CusReconDeclaration>().PK;

			CombineAssertions(() =>
			{
				AssertNull("Parameter CusReconEntry is null", Extensions.GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber(null));

				originalEntry.CRE_OriginalEntryNumber = ZString.Empty;
				AssertNull("OriginalEntryNumber is empty", originalEntry.GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber());

				originalEntry.CRE_OriginalEntryNumber = "2";
				AssertEquals("OriginalEntryNumber = '2'", entry2, originalEntry.GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber());

				originalEntry.CRE_OriginalEntryNumber = "3";
				AssertExceptionThrown<InvalidOperationException>("OriginalEntryNumber = '3', multiple results", () => originalEntry.GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber());
			});
		}

		public void TestCreateOrUpdateNote_Create_CommaSeparated()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", new ZString[] { "123", "456" }, ", ");
			AssertEquals("123, 456", message.Notes.FindByDescription("Test").SingleOrDefault().ST_NoteText);
		}

		public void TestCreateOrUpdateNote_Create_PipeSeparated()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", new ZString[] { "123", "456" }, "|");
			AssertEquals("123|456", message.Notes.FindByDescription("Test").SingleOrDefault().ST_NoteText);
		}

		public void TestCreateOrUpdateNote_CreateWithSingleValue()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", "123");
			AssertEquals("123", message.Notes.FindByDescription("Test").SingleOrDefault().ST_NoteText);
		}

		public void TestCreateOrUpdateNote_EmptySingleValue()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", ZString.Empty);
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Description, "Test")).Length);
		}

		public void TestCreateOrUpdateNote_Update()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", new ZString[] { "123", "456" }, ", ");
			Extensions.CreateOrUpdateNote(message, "Test", new ZString[] { "456", "123" }, ", ");
			AssertEquals("456, 123", message.Notes.FindByDescription("Test").SingleOrDefault().ST_NoteText);
		}

		public void TestCreateOrUpdateNote_NullMessage()
		{
			Extensions.CreateOrUpdateNote(null, "Test", "123");
			Extensions.CreateOrUpdateNote(null, "Test", new ZString[] { "123", "456" }, ", ");
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Description, "Test")).Length);
		}

		public void TestCreateOrUpdateNote_EmptyDescription()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, string.Empty, new ZString[] { "123", "456" }, ", ");
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_NoteText, "12, 456")).Length);
		}

		public void TestCreateOrUpdateNote_EmptyValues()
		{
			var message = Factory.New<EDIMessage>();
			Extensions.CreateOrUpdateNote(message, "Test", Array.Empty<ZString>(), ", ");
			AssertEquals(0, message.Notes.FindByDescription("Test").Length);
		}

		public void TestGetNote_NullBizo()
		{
			AssertNull(Extensions.GetNote(null, "Test"));
		}

		public void TestGetNote_NoNotes()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			AssertNull(declaration.GetNote("Description"));
		}

		public void TestGetNote_NoMatch()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote = declaration.Notes.AddNew();
			stmNote.ST_Description = "Description";
			stmNote.ST_NoteDataAsText = "Value";
			AssertNull(declaration.GetNote("Description2"));
		}

		public void TestGetNote_MultipleMatches()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote1 = declaration.Notes.AddNew();
			stmNote1.ST_Description = "Description";
			stmNote1.ST_NoteDataAsText = "Value1";
			var stmNote2 = declaration.Notes.AddNew();
			stmNote2.ST_Description = "Description";
			stmNote2.ST_NoteDataAsText = "Value2";
			AssertExceptionThrown<InvalidOperationException>(() => declaration.GetNote("Description"));
		}

		public void TestGetNote_SingleMatch()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote = declaration.Notes.AddNew();
			stmNote.ST_Description = "Description";
			stmNote.ST_NoteDataAsText = "Value";
			AssertEquals("Value", declaration.GetNote("Description"));
		}

		public void TestGetCusAuthorizationUsageNumber()
		{
			var entryInstruction = PrepareCusAuthorizationUsages();
			AssertEquals("AUTHEIR", entryInstruction.GetCusAuthorizationUsageNumber(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords));
		}

		public void TestGetCusAuthorizationUsageNumber_EntryInstructionNull()
		{
			_ = PrepareCusAuthorizationUsages();
			AssertNullOrEmpty(Extensions.GetCusAuthorizationUsageNumber(null, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords));
		}

		public void TestGetCusAuthorizationUsageNumber_InvalidCode()
		{
			var entryInstruction = PrepareCusAuthorizationUsages();
			AssertNullOrEmpty(entryInstruction.GetCusAuthorizationUsageNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
		}

		[TestDate(2022, 12, 28, 17, 30, 00)]
		public void TestAddCustomsEntryStatusLog()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.AddCustomsEntryStatusLog(UniversalReferenceConstants.EntryStatus.ERR);

			var stmLog = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.CustomsEntryStatus).Single();
			CombineAssertions(() =>
			{
				AssertEquals("SL_Reference", UniversalReferenceConstants.EntryStatus.ERR, stmLog.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2022, 12, 28, 17, 30, 00), stmLog.SL_EventTime);
			});
		}

		public void TestGetEUEoriNumber()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);

				var header = Factory.New<OrgHeader>();
				Assert("No customs codes", header.GetEUEoriNumber(Core.Constants.CountryCodes.Germany).IsEmpty);

				var cusCode = header.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				cusCode.OK_CustomsRegNo = "EU1234567890";
				Assert("Customs code exist, but type is not EORI, EU country", header.GetEUEoriNumber(Core.Constants.CountryCodes.Germany).IsEmpty);

				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				AssertEquals("Customs code of type EORI for EU country exist", "EU1234567890", header.GetEUEoriNumber(Core.Constants.CountryCodes.Germany));

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				cusCode.OK_CustomsRegNo = "EU1234567890";
				Assert("Customs code of type EORI exist, but country is not EU", header.GetEUEoriNumber(Core.Constants.CountryCodes.UnitedKingdom).IsEmpty);
			});
		}

		public void TestHasEUEoriNumber()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);

				var header = Factory.New<OrgHeader>();
				Assert("OrgHeader has no EORI", header.GetEUEoriDetails(true).IsEmpty);

				var cusCode = header.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				cusCode.OK_CustomsRegNo = "1234567890";
				AssertEquals("OrgHeader has EORI for NOT EU country", false, header.HasEUEoriNumber());

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				Assert("OrgHeader has EORI for EU country", header.HasEUEoriNumber());

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.UnitedKingdom);
				Assert("OrgHeader has multiple EORI, but single for EU country", header.HasEUEoriNumber());
			});
		}

		public void TestEUGetEoriDetails()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);

				var header = Factory.New<OrgHeader>();
				Assert("OrgHeader has no EORI", header.GetEUEoriDetails(true).IsEmpty);

				var cusCode = header.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				cusCode.OK_CustomsRegNo = "1234567890";
				Assert("OrgHeader has EORI for NOT EU country", header.GetEUEoriDetails(true).IsEmpty);

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				AssertEquals("OrgHeader has single EORI for EU country", "FR1234567890", header.GetEUEoriDetails(true));

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.Germany);
				AssertEquals("OrgHeader has multiple EORI", "* multiple EOR *", header.GetEUEoriDetails(true));

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("OrgHeader has multiple EORI, but single for EU country", "DE123456", header.GetEUEoriDetails(true));
			});
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);

				var header = Factory.New<OrgHeader>();
				Assert("OrgHeader has no EORI", header.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, true).IsEmpty);

				var cusCode = header.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				cusCode.OK_CustomsRegNo = "1234567890";
				Assert("OrgHeader has EORI for NOT EU country", header.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, true).IsEmpty);

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				AssertEquals("OrgHeader has single EORI for EU country", "FR1234567890", header.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, true));

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.Germany);
				AssertEquals("OrgHeader has multiple EORI", "* multiple EOR *", header.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, true));

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("OrgHeader has multiple EORI, but single for EU country", "DE123456", header.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, true));
			});
		}

		public void GetOrgAddressFromOrgHeaderCodeAndAddressCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Code = "ADDRESS";
			AssertEquals(orgAddress, Factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode("ORG", "ADDRESS"));
		}

		public void GetOrgAddressFromOrgHeaderCodeAndAddressCode_EmptyParameters()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Code = "ADDRESS";
			CombineAssertions(() =>
			{
				AssertNull("Empty orgHeader Code", Factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode(ZString.Empty, "ADDRESS"));
				AssertNull("Empty orgAddress Code", Factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode("ORG", ZString.Empty));
			});
		}

		public void GetOrgAddressFromOrgHeaderCodeAndAddressCode_InvalidParameters()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Code = "ADDRESS";
			CombineAssertions(() =>
			{
				AssertNull("Invalid orgHeader Code", Factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode("ORG1", "ADDRESS"));
				AssertNull("Invalid orgAddress Code", Factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode("ORG", "ADDRESS1"));
			});
		}

		CusEntryInstruction PrepareCusAuthorizationUsages()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var usageEIR = entryInstruction.CusAuthorizationUsages.AddNew();
			usageEIR.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			usageEIR.AGC_ParentID = entryInstruction.PK;
			usageEIR.AGC_ParentTableCode = "CEI";
			usageEIR.AGC_Number = "AUTHEIR";

			return entryInstruction;
		}

		enum Gender
		{
			M,
			[XmlEnum("Female")]
			F
		}
	}
}
