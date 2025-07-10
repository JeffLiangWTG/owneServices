using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class NctsIE29CusdecParserTest : TestCaseWithFactory
	{
		public void TestParse()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m);
			Factory.Save();

			var query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals(1, transactionRequested.Length);

			cusGuaranteeHeaderList.Add(guaranteeHeader1);
			var header = helper.GetProcessReceivedMessageHeader("ParserIE029_Message.xml", cusGuaranteeHeaderList, org1);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("029", header.Messages.LastIncomingMessage.EM_MessageType);

			var parser = header.GetIE29CusdecParser(Factory.Load<EU.NCTS.Business.NctsEdiMessage>(header.Messages.LastIncomingMessage.PK));

			var responseData = parser.Parse();
			CombineAssertions(() =>
			{
				AssertEquals("local reference number", "LRNScenario1b", responseData.LocalReferenceNumber);
				AssertEquals("MovementReferenceNumber", "18FR00400000000273", responseData.MovementReferenceNumber);
				AssertEquals("DeclarationType", "T1", responseData.DeclarationType);
				AssertEquals("CountryOfDestination", "FR", responseData.CountryOfDestination);
				AssertEquals("AgreedLocationOfGoodsCode", "", responseData.AgreedLocationOfGoodsCode);
				AssertEquals("AgreedLocationOfGoods", "", responseData.AgreedLocationOfGoods);
				AssertEquals("AuthorisedLocationOfGoodsCode", "", responseData.AuthorisedLocationOfGoodsCode);
				AssertEquals("PlaceOfLoadingCode", "quai 10", responseData.PlaceOfLoadingCode);
				AssertEquals("CountryOfDispatch", "FR", responseData.CountryOfDispatch);
				AssertEquals("CustomsSubPlace", "", responseData.CustomsSubPlace);
				AssertEquals("InlandTransportMode", "3", responseData.InlandTransportMode);
				AssertEquals("TransportModeAtBorder", "3", responseData.TransportModeAtBorder);
				AssertEquals("MeansOfTransportCrossingBorderIdentity", "BB15 VF", responseData.MeansOfTransportCrossingBorderIdentity);
				AssertEquals("MeansOfTransportCrossingBorderNationality", "IT", responseData.MeansOfTransportCrossingBorderNationality);
				AssertEquals("MeansOfTransportCrossingBorderType", "", responseData.MeansOfTransportCrossingBorderType);
				AssertEquals("MeansOfTransportAtDepartureIdentity", "11 TT 75", responseData.MeansOfTransportAtDepartureIdentity);
				AssertEquals("MeansOfTransportAtDepartureNationality", "FR", responseData.MeansOfTransportAtDepartureNationality);
				AssertEquals("IsContainerised", true, responseData.IsContainerised);
				AssertEquals("NctsReturnCopy", "0", responseData.NctsReturnCopy);
				AssertEquals("AcceptanceDate", "20180419", responseData.AcceptanceDate);
				AssertEquals("IssuingDate", "20180419", responseData.IssuingDate);
				AssertEquals("DialogLanguageIndicatorAtDeparture", "FR", responseData.DialogLanguageIndicatorAtDeparture);
				AssertEquals("NctsAccompanyingDocumentLanguageCode", "FR", responseData.NctsAccompanyingDocumentLanguageCode);
				AssertEquals("TotalNumberOfItems", "2", responseData.TotalNumberOfItems);
				AssertEquals("TotalNumberOfPackages", "20", responseData.TotalNumberOfPackages);
				AssertEquals("TotalGrossMassInKilograms", "120", responseData.TotalGrossMassInKilograms);
				AssertEquals("TotalNettMassInKilograms", "116", responseData.TotalNettMassInKilograms);
				AssertEquals("BindingItinerary", "0", responseData.BindingItinerary);
				AssertEquals("AuthorisationId", "00000006", responseData.AuthorisationId);
				AssertEquals("DeclarationDate", "20180419", responseData.DeclarationDate);
				AssertEquals("DeclarationPlace", "PARIS", responseData.DeclarationPlace);
				AssertEquals("SpecificCircumstanceIndicator", "", responseData.SpecificCircumstanceIndicator);
				AssertEquals("TransportChargesMoP", "", responseData.TransportChargesMoP);
				AssertEquals("CommercialReferenceNumber", "", responseData.CommercialReferenceNumber);
				AssertEquals("IsSecurity", true, responseData.IsSecurity);
				AssertEquals("ConveyanceReferenceNumber", "", responseData.ConveyanceReferenceNumber);
				AssertEquals("PlaceOfUnloadingCode", "", responseData.PlaceOfUnloadingCode);

				AssertEquals("AuthorisedConsigneeEori", "", responseData.AuthorisedConsigneeEori);
				AssertEquals("DepartureCustomsOfficeCode", "FR000040", responseData.DepartureCustomsOfficeCode);
				AssertEquals("DestinationCustomsOfficeCode", "FR002300", responseData.DestinationCustomsOfficeCode);

				AssertEquals("ControlResultControlDate", "20180419", responseData.ControlResultControlDate);
				AssertEquals("ControlResultControlResultCode", "A2", responseData.ControlResultControlResultCode);
				AssertEquals("ControlResultControlledBy", "", responseData.ControlResultControlledBy);
				AssertEquals("ControlResultTimeLimit", "20180429", responseData.ControlResultTimeLimit);
				AssertEquals("RepresentativeName", "", responseData.RepresentativeName);
				AssertEquals("RepresentativeCapacity", "", responseData.RepresentativeCapacity);
				AssertEquals("SealsNumber", "1", responseData.SealsNumber);

				AssertEquals("Itinerary", "ES", responseData.Itinerary);
			});

			CombineAssertions(() =>
			{
				var principal = responseData.Principal;
				AssertEquals("Principal company", "GO SPORT FRANCE", principal.CompanyName);
				AssertEquals("Principal CountryCode", "FR", principal.CountryCode);
				AssertEquals("Principal City", "ANNECY", principal.City);
				AssertEquals("Principal GovRegNum", "FR42856003100257", principal.GovRegNum);
				AssertEquals("Principal Address1", "1, RUE JEAN JAURES", principal.Address1);
				AssertEquals("Principal Postcode", "74000", principal.Postcode);
				AssertEquals("Principal AuthorisedNumber", "", principal.AuthorisedNumber);

				var consignor = responseData.Consignor;
				AssertEquals("consignor company", "SI Lesaffre", consignor.CompanyName);
				AssertEquals("consignor CountryCode", "FR", consignor.CountryCode);
				AssertEquals("consignor City", "Marcq-en-Baroeul", consignor.City);
				AssertEquals("consignor GovRegNum", "", consignor.GovRegNum);
				AssertEquals("consignor Address1", "137 rue Gabriel Peri", consignor.Address1);
				AssertEquals("consignor Postcode", "59703", consignor.Postcode);

				var consignee = responseData.Consignee;
				AssertEquals("consignee company", "Lantmannen Unibake Denmark AS", consignee.CompanyName);
				AssertEquals("consignee CountryCode", "DK", consignee.CountryCode);
				AssertEquals("consignee City", "Berghagan 2", consignee.City);
				AssertEquals("consignee GovRegNum", "", consignee.GovRegNum);
				AssertEquals("consignee Address1", "5 Statoil Street", consignee.Address1);
				AssertEquals("consignee Postcode", "N-1405", consignee.Postcode);

				var returnCopiesCustomsOffice = responseData.ReturnCopiesCustomsOffice;
				AssertEquals("returnCopiesCustomsOffice company", "FR000040", returnCopiesCustomsOffice.CompanyName);
				AssertEquals("returnCopiesCustomsOffice CountryCode", "FR", returnCopiesCustomsOffice.CountryCode);
				AssertEquals("returnCopiesCustomsOffice City", "AJACCIO CEDEX", returnCopiesCustomsOffice.City);
				AssertEquals("returnCopiesCustomsOffice GovRegNum", "", returnCopiesCustomsOffice.GovRegNum);
				AssertEquals("returnCopiesCustomsOffice Address1", "BLD SAMPIERO BP 99", returnCopiesCustomsOffice.Address1);
				AssertEquals("returnCopiesCustomsOffice Postcode", "20177", returnCopiesCustomsOffice.Postcode);
			});

			CombineAssertions(() =>
			{
				AssertEquals("GoodsItems count", 2, responseData.GoodsItems.Count);
				var goodsItem = responseData.GoodsItems.ElementAt(0);
				AssertEquals("GoodsItems ItemNumber", "1", goodsItem.ItemNumber);
				AssertEquals("GoodsItems CommodityCode", "", goodsItem.CommodityCode);
				AssertEquals("GoodsItems DeclarationType", "", goodsItem.DeclarationType);
				AssertEquals("GoodsItems DescriptionOfGoods", "souliers rouges", goodsItem.DescriptionOfGoods);
				AssertEquals("GoodsItems GrossMassInKilograms", "60", goodsItem.GrossMassInKilograms);

				AssertEquals("GoodsItems PreviousDocuments", 1, goodsItem.PreviousDocuments.Count);
				AssertEquals("GoodsItems PreviousDocuments", "T2", goodsItem.PreviousDocuments.ElementAt(0).TypeCode);

				AssertEquals("GoodsItems SupportingDocuments", 1, goodsItem.SupportingDocuments.Count);
				AssertEquals("GoodsItems SupportingDocuments", "714", goodsItem.SupportingDocuments.ElementAt(0).TypeCode);

				AssertEquals("GoodsItems SpecialMentions", 0, goodsItem.SpecialMentions.Count);

				AssertEquals("GoodsItems Packages", 1, goodsItem.Packages.Count);
				AssertEquals("GoodsItems Packages", "BX", goodsItem.Packages.ElementAt(0).PackageType);
			});

			CombineAssertions(() =>
			{
				var guarantees = responseData.Guarantees;
				AssertEquals("Guarantees", 1, responseData.Guarantees.Count);

				var guarantee = guarantees.ElementAt(0);
				AssertEquals("Guarantees GuaranteeReferenceNumber", "17FR0000000000021", guarantee.GuaranteeReferenceNumber);
				AssertEquals("Guarantees GuaranteeType", "1", guarantee.GuaranteeType);
				AssertEquals("Guarantees ValidityLimitationOther", "", guarantee.ValidityLimitationOther);
				AssertEquals("Guarantees ValidityLimitationEC", false, guarantee.ValidityLimitationEC);
			});

			CombineAssertions(() =>
			{
				var seals = responseData.Seals;
				AssertEquals("Seals", 1, seals.Count);

				AssertEquals("Seal ", "Commerciaux", seals.ElementAt(0));
			});

			CombineAssertions(() =>
			{
				var carrier = responseData.Carrier;
				AssertEquals("carrier company", "BRASSERIE ARTHUR", carrier.CompanyName);
				AssertEquals("carrier CountryCode", "UK", carrier.CountryCode);
				AssertEquals("carrier City", "VAL D'OUSTE", carrier.City);
				AssertEquals("carrier GovRegNum", "FR95047117700029", carrier.GovRegNum);
				AssertEquals("carrier Address1", "SITE DE LA MINE D'OR", carrier.Address1);
				AssertEquals("carrier Postcode", "56461", carrier.Postcode);

				var securityconsignor = responseData.SecurityConsignor;
				AssertEquals("securityconsignor company", "DACHSER FRANCE", securityconsignor.CompanyName);
				AssertEquals("securityconsignor CountryCode", "FR", securityconsignor.CountryCode);
				AssertEquals("securityconsignor City", "CHANVERRIE", securityconsignor.City);
				AssertEquals("securityconsignor GovRegNum", "FR54665033400156", securityconsignor.GovRegNum);
				AssertEquals("securityconsignor Address1", "1, AV DE L'EUROPE", securityconsignor.Address1);
				AssertEquals("securityconsignor Postcode", "85130", securityconsignor.Postcode);

				var securityConsignee = responseData.SecurityConsignee;
				AssertEquals("securityConsignee company", "BRASSERIE LANCELOT", securityConsignee.CompanyName);
				AssertEquals("securityConsignee CountryCode", "UA", securityConsignee.CountryCode);
				AssertEquals("securityConsignee City", "VAL D'OUST", securityConsignee.City);
				AssertEquals("securityConsignee GovRegNum", "FR95047117700028", securityConsignee.GovRegNum);
				AssertEquals("securityConsignee Address1", ",  SITE DE LA MINE D'OR", securityConsignee.Address1);
				AssertEquals("securityConsignee Postcode", "56460", securityConsignee.Postcode);
			});
		}

		CusGuaranteeHeader GenerateGuaranteeHeader(OrgHeader org1, ZString pW_BondNumber, ZString transactionReference, ZString transactionAppId, ZDecimal valueTransaction)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = pW_BondNumber;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.AddTransaction(transactionReference, "CMT-TO-CONF", transactionAppId, "", valueTransaction, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader.AddTransaction(transactionReference, "CMT-CON", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			AssertEquals(valueTransaction, guaranteeHeader.CPH_Calc_OpeningBalance);
			return guaranteeHeader;
		}
	}
}
