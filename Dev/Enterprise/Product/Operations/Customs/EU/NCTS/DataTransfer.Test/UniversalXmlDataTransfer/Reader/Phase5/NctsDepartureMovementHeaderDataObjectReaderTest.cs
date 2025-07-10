using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using CusSeal = Enterprise.Customs.EU.NCTS.Business.CusSeal;
using Seal = Enterprise.UniversalDataBuss.DataObjects.Universal.Seal;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5.Testing
{
	class NctsDepartureMovementHeaderDataObjectReaderTest : NctsHeaderCommonDataObjectReaderTest<NctsDepartureMovementHeaderDataObjectReader>
	{
		public void TestHandleNullInBondMoveHeaderCollection()
		{
			var headerData = new UniversalShipment();
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(NctsMovementType.Codes.Departure, headerBO.BH_HeaderType);
		}

		public void TestMovementType()
		{
			var header = GetNewHeader();
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(NctsMovementType.Codes.Departure, headerBO.BH_HeaderType);
		}

		public void TestRelatedDocuments()
		{
			var header = GetNewHeader();
			var supportingDocument1 = header.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Description = "sup1";
			supportingDocument1.CSI_Code = "C641";
			supportingDocument1.CSI_ReferenceNumber = "sup-ref1";
			supportingDocument1.CSI_ItemNumber = 1;
			supportingDocument1.CSI_ReferenceNumber2 = "sup-com1";
			var supportingDocument2 = header.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Description = "sup2";
			supportingDocument2.CSI_Code = "C642";
			supportingDocument2.CSI_ReferenceNumber = "sup-ref2";
			supportingDocument2.CSI_ItemNumber = 2;
			supportingDocument2.CSI_ReferenceNumber2 = "sup-com2";

			var additionalDocument1 = header.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = "INF";
			additionalDocument1.CSI_Description = "add1";
			additionalDocument1.CSI_Code = "C651";
			additionalDocument1.CSI_ReferenceNumber = "inf-ref";
			var additionalDocument2 = header.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = "TRA";
			additionalDocument2.CSI_Code = "C652";
			additionalDocument2.CSI_ReferenceNumber = "tra-ref";
			additionalDocument2.CSI_Description = "add2";

			var previousDocument1 = header.PreviousDocuments.AddNew();
			previousDocument1.CSI_Description = "pre1";
			previousDocument1.CSI_Code = "N830";
			previousDocument1.CSI_ReferenceNumber = "pre-ref1";
			previousDocument1.CSI_ReferenceNumber2 = "pre-com1";
			var previousDocument2 = header.PreviousDocuments.AddNew();
			previousDocument2.CSI_Description = "pre2";
			previousDocument2.CSI_Code = "N831";
			previousDocument2.CSI_ReferenceNumber = "pre-ref2";
			previousDocument2.CSI_ReferenceNumber2 = "pre-com2";

			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertCusSupportingInfo(headerBO.AdditionalDocuments[0], "add1", "C651", "inf-ref", "INF", ZString.Empty, 0);
				AssertCusSupportingInfo(headerBO.AdditionalDocuments[1], "add2", "C652", "tra-ref", "TRA", ZString.Empty, 0);
				AssertCusSupportingInfo(headerBO.PreviousDocuments[0], "pre1", "N830", "pre-ref1", ZString.Empty, "pre-com1", 0);
				AssertCusSupportingInfo(headerBO.PreviousDocuments[1], "pre2", "N831", "pre-ref2", ZString.Empty, "pre-com2", 0);
				headerBO.MovementHeader.SupportingDocuments.Load();
				AssertCusSupportingInfo(headerBO.MovementHeader.SupportingDocuments[0], "sup1", "C641", "sup-ref1", ZString.Empty, "sup-com1", 1);
				AssertCusSupportingInfo(headerBO.MovementHeader.SupportingDocuments[1], "sup2", "C642", "sup-ref2", ZString.Empty, "sup-com2", 2);
			});
		}

		void AssertCusSupportingInfo(CusSupportingInfo supportingInfo, ZString expectedDescription, ZString expectedCode, ZString expectedReferenceNumber, ZString expectedSubType, ZString expectedReferenceNumber2, ZInt expectedItemNumber)
		{
			AssertEquals("CSI_Description", expectedDescription, supportingInfo.CSI_Description);
			AssertEquals("CSI_Code", expectedCode, supportingInfo.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", expectedReferenceNumber, supportingInfo.CSI_ReferenceNumber);
			AssertEquals("CSI_ItemNumber", expectedItemNumber, supportingInfo.CSI_ItemNumber);
			AssertEquals("CSI_ReferenceNumber2", expectedReferenceNumber2, supportingInfo.CSI_ReferenceNumber2);
			AssertEquals("CSI_SubType", expectedSubType, supportingInfo.CSI_SubType);
		}

		public void TestCountriesOfRouting()
		{
			var countryOfRouting1 = new CustomsReference()
			{
				Order = 3,
				Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "COR" },
				SubType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair35Char() { Code = "CN" },
			};
			var countryOfRouting2 = new CustomsReference()
			{
				Order = 1,
				Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "COR" },
				SubType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair35Char() { Code = "IE" },
			};
			var countryOfRouting3 = new CustomsReference()
			{
				Order = 2,
				Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "COR" },
				SubType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair35Char() { Code = "AU" },
			};

			var universalMoveHeader = new InBondMoveHeader(UniversalDataBuss.DataObjects.DefaultDataObjectWriterStrategy.TestInstance);
			universalMoveHeader.SetCustomsReferenceCollection(() => new List<CustomsReference> { countryOfRouting1, countryOfRouting2, countryOfRouting3 });
			universalMoveHeader.SetTransportMeansCollection(() => new List<TransportMeans>());
			var universalShipment = new UniversalShipment(UniversalDataBuss.DataObjects.DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader> { universalMoveHeader });
			var dataObjectReader = GetNewReader(universalShipment, new TestErrorLogger());
			var readHeader = dataObjectReader.ReadIntoBusinessObject();

			var countriesOfRouting = readHeader.CountriesOfRouting.OrderBy(x => x.CY_Order).ToArray();
			AssertEquals("Count", 3, countriesOfRouting.Length);
			CombineAssertions(() =>
			{
				AssertCountryOfRouting(countriesOfRouting[0], 1, "IE");
				AssertCountryOfRouting(countriesOfRouting[1], 2, "AU");
				AssertCountryOfRouting(countriesOfRouting[2], 3, "CN");
			});

			void AssertCountryOfRouting(CountryOfRouting countryOfRouting, int order, string countryCode)
			{
				AssertEquals("CY_Order", order, countryOfRouting.CY_Order);
				AssertEquals("CY_Data", countryCode, countryOfRouting.CY_Data);
			}
		}

		public void TestUnmatchedCountriesOfRoutingAreDeleted()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			header.CountriesOfRouting.AddNew().CY_Data = "CN";
			header.CountriesOfRouting.AddNew().CY_Data = "IE";
			header.CountriesOfRouting.AddNew().CY_Data = "JP";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].CustomsReferenceCollection
			.Single(x => x.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.CountryOfRoutingCode && x.SubType.GetCodeAsUpperCase() == "JP")
			.SubType.Code = "AU";
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				var countriesOfRouting = headerBO.CountriesOfRouting.Cast<CountryOfRouting>().ToArray();
				AssertEquals("Count", 3, countriesOfRouting.Length);
				AssertEquals("CY_Data", "CN", countriesOfRouting[0].CY_Data);
				AssertEquals("CY_Data", "IE", countriesOfRouting[1].CY_Data);
				AssertEquals("CY_Data", "AU", countriesOfRouting[2].CY_Data);
			});
		}

		public void TestCusAuthorizationUsages()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var authorizationUsage1 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_OH_Owner = OrgINTHEMSYD.PK;
			authorizationUsage1.AGC_Number = "usage1";
			authorizationUsage1.AGC_Code = "ACR";
			var authorizationUsage2 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_OH_Owner = OrgWUFSHIJNB.PK;
			authorizationUsage2.AGC_Number = "usage2";
			authorizationUsage2.AGC_Code = "TRD";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				var authorizationUsages = headerBO.MovementHeader.CusAuthorizationUsages.Cast<Business.CusAuthorizationUsage>().ToArray();
				AssertEquals("Count", 2, authorizationUsages.Length);
				AssertCusAuthorizationUsage(authorizationUsages[0], "ACR", "usage1", OrgINTHEMSYD.PK);
				AssertCusAuthorizationUsage(authorizationUsages[1], "TRD", "usage2", OrgWUFSHIJNB.PK);
			});
		}

		public void TestUnmatchedCusAuthorizationUsagesAreDeleted()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			var movementHeader = header.MovementHeader;
			var authorizationUsage1 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_OH_Owner = OrgINTHEMSYD.PK;
			authorizationUsage1.AGC_Number = "usage1";
			authorizationUsage1.AGC_Code = "ACR";
			var authorizationUsage2 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_OH_Owner = OrgWUFSHIJNB.PK;
			authorizationUsage2.AGC_Number = "usage2";
			authorizationUsage2.AGC_Code = "TRD";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].CustomsReferenceCollection
			.Single(x => x.Type.GetCodeAsUpperCase() == CusReferenceTypeCodes.AUT && x.SubType.GetCodeAsUpperCase() == "ACR" && x.Reference.GetValueOrDefault() == "usage1")
			.SubType.Code = "SSE";
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				var authorizationUsages = headerBO.MovementHeader.CusAuthorizationUsages.Cast<Business.CusAuthorizationUsage>().ToArray();
				AssertEquals("Count", 2, authorizationUsages.Length);
				AssertCusAuthorizationUsage(authorizationUsages[0], "TRD", "usage2", OrgWUFSHIJNB.PK);
				AssertCusAuthorizationUsage(authorizationUsages[1], "SSE", "usage1", OrgINTHEMSYD.PK);
			});
		}

		void AssertCusAuthorizationUsage(Business.CusAuthorizationUsage usage, ZString exceptedAGC_Code, ZString exceptedAGC_Number, ZGuid exceptedAGC_OH_Owner)
		{
			AssertEquals("AGC_Code", exceptedAGC_Code, usage.AGC_Code);
			AssertEquals("AGC_Number", exceptedAGC_Number, usage.AGC_Number);
			AssertEquals("AGC_OH_Owner", exceptedAGC_OH_Owner, usage.AGC_OH_Owner);
		}

		public void TestAdditionalBillCollection()
		{
			var header = GetNewHeader();
			for (var i = 1; i <= 2; ++i)
			{
				var bill = header.Bills.AddNew();
				SetupBill(bill, i);
			}

			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();

			AssertBillFromAdditionalBill(headerBO.Bills[0], 1);
			AssertBillFromAdditionalBill(headerBO.Bills[1], 2);

			void SetupBill(NctsBill bill, int i)
			{
				bill.B0_ReferenceID = $"BILL{i}";
				bill.Consignor.E2_OA_Address = CreateAddressForTest($"COR{i}");
				bill.Consignee.E2_OA_Address = CreateAddressForTest($"CEE{i}");

				bill.B0_RX_NKLinePriceCurrency = "AUD";

				var supportingDocument1 = bill.SupportingDocuments.AddNew();
				supportingDocument1.CSI_Description = $"sup{i}1";
				supportingDocument1.CSI_Code = "C641";
				supportingDocument1.CSI_ReferenceNumber = $"sup-ref{i}1";
				supportingDocument1.CSI_ItemNumber = 1;
				supportingDocument1.CSI_ReferenceNumber2 = $"sup-com{i}1";
				var supportingDocument2 = bill.SupportingDocuments.AddNew();
				supportingDocument2.CSI_Description = $"sup{i}2";
				supportingDocument2.CSI_Code = "C642";
				supportingDocument2.CSI_ReferenceNumber = $"sup-ref{i}2";
				supportingDocument2.CSI_ItemNumber = 2;
				supportingDocument2.CSI_ReferenceNumber2 = $"sup-com{i}2";
				var additionalDocument1 = bill.AdditionalDocuments.AddNew();
				additionalDocument1.CSI_SubType = "INF";
				additionalDocument1.CSI_Description = $"add{i}1";
				additionalDocument1.CSI_Code = "C651";
				additionalDocument1.CSI_ReferenceNumber = $"inf-ref{i}";
				var additionalDocument2 = bill.AdditionalDocuments.AddNew();
				additionalDocument2.CSI_SubType = "REF";
				additionalDocument2.CSI_Code = "C653";
				additionalDocument2.CSI_ReferenceNumber = $"ref-ref{i}";
				additionalDocument2.CSI_Description = $"add{i}2";
				var additionalDocument3 = bill.AdditionalDocuments.AddNew();
				additionalDocument3.CSI_SubType = "TRA";
				additionalDocument3.CSI_Code = "C652";
				additionalDocument3.CSI_ReferenceNumber = $"tra-ref{i}";
				additionalDocument3.CSI_Description = $"add{i}3";
				var previousDocument1 = bill.PreviousDocuments.AddNew();
				previousDocument1.CSI_Description = $"pre{i}1";
				previousDocument1.CSI_Code = "N830";
				previousDocument1.CSI_ReferenceNumber = $"pre-ref{i}1";
				previousDocument1.CSI_ReferenceNumber2 = $"pre-com{i}1";
				var previousDocument2 = bill.PreviousDocuments.AddNew();
				previousDocument2.CSI_Description = $"pre{i}2";
				previousDocument2.CSI_Code = "N831";
				previousDocument2.CSI_ReferenceNumber = $"pre-ref{i}2";
				previousDocument2.CSI_ReferenceNumber2 = $"pre-com{i}2";
				var supplyChainActorReference1 = bill.CusSupplyChainActorReferences.AddNew();
				supplyChainActorReference1.CFR_Code = i == 1 ? SupplyChainActorRoleList.Codes.CS : SupplyChainActorRoleList.Codes.FW;
				supplyChainActorReference1.CFR_Reference = $"Reference{i}1";
				var supplyChainActorReference2 = bill.CusSupplyChainActorReferences.AddNew();
				supplyChainActorReference2.CFR_Code = i == 1 ? SupplyChainActorRoleList.Codes.WH : SupplyChainActorRoleList.Codes.MF;
				supplyChainActorReference2.CFR_Reference = $"Reference{i}2";
			}

			void AssertBillFromAdditionalBill(NctsBill bill, int i)
			{
				AssertEquals($"BILL{i}", bill.B0_ReferenceID);
				AssertEquals($"COR{i}", bill.Consignor.Organisation.OH_Code);
				AssertEquals($"CEE{i}", bill.Consignee.Organisation.OH_Code);
				AssertEquals("AUD", bill.LinePriceCurrency.Code);
				AssertCusSupportingInfo(bill.AdditionalDocuments[0], $"add{i}1", "C651", $"inf-ref{i}", "INF", ZString.Empty, 0);
				AssertCusSupportingInfo(bill.AdditionalDocuments[1], $"add{i}2", "C653", $"ref-ref{i}", "REF", ZString.Empty, 0);
				AssertCusSupportingInfo(bill.AdditionalDocuments[2], $"add{i}3", "C652", $"tra-ref{i}", "TRA", ZString.Empty, 0);
				AssertCusSupportingInfo(bill.PreviousDocuments[0], $"pre{i}1", "N830", $"pre-ref{i}1", ZString.Empty, $"pre-com{i}1", 0);
				AssertCusSupportingInfo(bill.PreviousDocuments[1], $"pre{i}2", "N831", $"pre-ref{i}2", ZString.Empty, $"pre-com{i}2", 0);
				AssertCusSupportingInfo(bill.SupportingDocuments[0], $"sup{i}1", "C641", $"sup-ref{i}1", ZString.Empty, $"sup-com{i}1", 1);
				AssertCusSupportingInfo(bill.SupportingDocuments[1], $"sup{i}2", "C642", $"sup-ref{i}2", ZString.Empty, $"sup-com{i}2", 2);
				AssertEquals($"Reference{i}1", bill.CusSupplyChainActorReferences[0].CFR_Reference);
				AssertEquals(i == 1 ? SupplyChainActorRoleList.Codes.CS : SupplyChainActorRoleList.Codes.FW, bill.CusSupplyChainActorReferences[0].CFR_Code);
				AssertEquals($"Reference{i}2", bill.CusSupplyChainActorReferences[1].CFR_Reference);
				AssertEquals(i == 1 ? SupplyChainActorRoleList.Codes.WH : SupplyChainActorRoleList.Codes.MF, bill.CusSupplyChainActorReferences[1].CFR_Code);
			}
		}

		public void TestInBondMoveDetail()
		{
			var headerBO = SetupInBondMoveDetailTest(road: false);
			AssertEquals("Pre-requisite: MovementHeader.BM_InlandTransportMode", ModeOfTransportList.Codes._1_SeaTransport, headerBO.MovementHeader.BM_InlandTransportMode);
			AssertBillFromInBondMoveDetail(headerBO.Bills[0], 1);
			AssertTransportMeansSea(headerBO.Bills[0]);
			AssertBillFromInBondMoveDetail(headerBO.Bills[1], 2);
			AssertTransportMeansSea(headerBO.Bills[1]);
		}

		public void TestInBondMoveDetail_Road()
		{
			var headerBO = SetupInBondMoveDetailTest(road: true);
			AssertEquals("Pre-requisite: MovementHeader.BM_InlandTransportMode", ModeOfTransportList.Codes._3_RoadTransport, headerBO.MovementHeader.BM_InlandTransportMode);
			AssertBillFromInBondMoveDetail(headerBO.Bills[0], 1);
			AssertTransportMeansRoad(headerBO.Bills[0]);
			AssertBillFromInBondMoveDetail(headerBO.Bills[1], 2);
			AssertTransportMeansRoad(headerBO.Bills[1]);
		}

		NctsHeader SetupInBondMoveDetailTest(bool road)
		{
			var header = GetNewHeader();
			header.MovementHeader.BM_InlandTransportMode = road ? ModeOfTransportList.Codes._3_RoadTransport : ModeOfTransportList.Codes._1_SeaTransport;
			for (var i = (short)1; i <= 2; ++i)
			{
				var bill = header.Bills.AddNew();
				bill.B0_TransportPaymentMethod = i == 1 ? TransportChargesModeOfPayment.Codes.Cash : TransportChargesModeOfPayment.Codes.CreditCard;
				bill.B0_RN_NKCountryOfExport = $"E{i}";
				bill.B0_RN_NKCountryOfDestination = $"D{i}";
				SetupMoveDetailTransportDeparture(bill, road ? NctsTransportTypeOfIdList.Codes._30 : NctsTransportTypeOfIdList.Codes._10);
				bill.B0_Weight = i * 1.23m;
				bill.B0_WeightUQ = $"W{i}";
			}

			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			return reader.ReadIntoBusinessObject();
		}

		static void SetupMoveDetailTransportDeparture(NctsBill bill, ZString transportType)
		{
			bill.TransportTypeAtDeparture = transportType;
			bill.TransportAtDeparture = "VS001";
			bill.TransportCountryAtDeparture = Constants.CountryCodes.Germany;

			bill.Trailer1IDAtDeparture = "Trailer1";
			bill.Trailer1NationalityAtDeparture = Constants.CountryCodes.Poland;
			bill.Trailer2IDAtDeparture = "Trailer2";
			bill.Trailer2NationalityAtDeparture = Constants.CountryCodes.Italy;
		}

		void AssertBillFromInBondMoveDetail(NctsBill bill, int i)
		{
			AssertEquals(i, bill.SequenceNumber);
			AssertEquals(i == 1 ? TransportChargesModeOfPayment.Codes.Cash : TransportChargesModeOfPayment.Codes.CreditCard, bill.B0_TransportPaymentMethod);
			AssertEquals($"E{i}", bill.B0_RN_NKCountryOfExport);
			AssertEquals($"D{i}", bill.B0_RN_NKCountryOfDestination);
			AssertEquals(i * 1.23m, bill.B0_Weight);
			AssertEquals($"W{i}", bill.B0_WeightUQ);
		}

		void AssertTransportMeansRoad(NctsBill bill)
		{
			AssertEquals("InlandTransportModeAtDeparture", ModeOfTransportList.Codes._3_RoadTransport, bill.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", NctsTransportTypeOfIdList.Codes._30, bill.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture, same as VesselNameAtDeparture", "VS001", bill.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", Constants.CountryCodes.Germany, bill.TransportCountryAtDeparture);
			AssertEquals("VesselNameAtDeparture, same as TransportAtDeparture", "VS001", bill.VesselNameAtDeparture);
			AssertEquals("VesselCountryAtDeparture", Constants.CountryCodes.Germany, bill.VesselCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "Trailer1", bill.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", Constants.CountryCodes.Poland, bill.Trailer1NationalityAtDeparture);
			AssertEquals("Trailer2IDAtDeparture", "Trailer2", bill.Trailer2IDAtDeparture);
			AssertEquals("Trailer2NationalityAtDeparture", Constants.CountryCodes.Italy, bill.Trailer2NationalityAtDeparture);
		}

		void AssertTransportMeansSea(NctsBill bill)
		{
			AssertEquals("InlandTransportModeAtDeparture", ModeOfTransportList.Codes._1_SeaTransport, bill.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", NctsTransportTypeOfIdList.Codes._10, bill.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture, same value as VesselNameAtDeparture.", "VS001", bill.TransportAtDeparture);
			AssertEquals("VesselNameAtDeparture", "VS001", bill.VesselNameAtDeparture);
			AssertEquals("VesselCountryAtDeparture", Core.Constants.CountryCodes.Germany, bill.VesselCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", ZString.Empty, bill.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", ZString.Empty, bill.Trailer1NationalityAtDeparture);
			AssertEquals("Trailer2IDAtDeparture", ZString.Empty, bill.Trailer2IDAtDeparture);
			AssertEquals("Trailer2NationalityAtDeparture", ZString.Empty, bill.Trailer2NationalityAtDeparture);
		}

		ZGuid CreateAddressForTest(string code)
		{
			var org = NCTSTestHelper.CreateOrgAddressForTest(Factory.BOFactory, code);
			org.Header.OH_Code = code;
			org.OA_Code = code;
			return org.PK;
		}

		public void TestLocationOfGoods()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = "Y";
			var address = goodsLocation.Address;
			address.AuthorisationNumber = "AUTH";
			address.E2_Postcode = "2180";
			address.E2_RN_NKCountryCode = "AU";
			address.E2_Contact = "ABC";
			address.E2_Phone = "123456";
			address.E2_Email = "abc@123.com";
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_AdditionalIdentifier = "Something";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].OrgAddress.AddressOverride = false;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Name = ZString.Empty;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.PhoneNumber = ZString.Empty;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Email = ZString.Empty;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var goodsLocationBO = reader.ReadIntoBusinessObject().MovementHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				AssertEquals("Qualifier.Code", "Y", goodsLocationBO.CGL_Qualifier);
				AssertEquals("AdditionalIdentifier", "Something", goodsLocationBO.CGL_AdditionalIdentifier);
				AssertEquals("SubType.Code", "A", goodsLocationBO.CGL_Type);
				var goodsLocationAddress = goodsLocationBO.Address;
				AssertEquals("OrgAddress.AuthorisationNumber", "AUTH", goodsLocationAddress.AuthorisationNumber);
				AssertEquals("OrgAddress.E2_Postcode", "2180", goodsLocationAddress.E2_Postcode);
				AssertEquals("OrgAddress.E2_RN_NKCountryCode", "AU", goodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("OrgAddress.E2_Contact", "ABC", goodsLocationAddress.E2_Contact);
				AssertEquals("OrgAddress.E2_Phone", "123456", goodsLocationAddress.E2_Phone);
				AssertEquals("OrgAddress.E2_Email", "abc@123.com", goodsLocationAddress.E2_Email);
			});
		}

		public void TestLocationOfGoods_OverrideAddress()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var address = goodsLocation.Address;
			address.E2_Postcode = "2180";
			address.E2_RN_NKCountryCode = "AU";
			address.E2_Contact = "ABC";
			address.E2_Phone = "123456";
			address.E2_Email = "abc@123.com";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].OrgAddress.AddressOverride = false;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Name = "DEF";
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.PhoneNumber = "987654";
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Email = "def@456.com";
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var goodsLocationBO = reader.ReadIntoBusinessObject().MovementHeader.GoodsLocation;
			CombineAssertions(() =>
			{
				var goodsLocationAddress = goodsLocationBO.Address;
				AssertEquals("OrgAddress.E2_Postcode", "2180", goodsLocationAddress.E2_Postcode);
				AssertEquals("OrgAddress.E2_RN_NKCountryCode", "AU", goodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("Contact.OC_ContactName instead of OrgAddress.E2_Contact", "DEF", goodsLocationAddress.E2_Contact);
				AssertEquals("Contact.OC_Phone instead of OrgAddress.E2_Phone", "987654", goodsLocationAddress.E2_Phone);
				AssertEquals("Contact.OC_Email instead of OrgAddress.E2_Email", "def@456.com", goodsLocationAddress.E2_Email);
			});
		}

		public void TestLocationOfGoods_WithoutContactNode()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var address = goodsLocation.Address;
			address.E2_Postcode = "2180";
			address.E2_RN_NKCountryCode = "AU";
			address.E2_Contact = "GHI";
			address.E2_Phone = "654321";
			address.E2_Email = "cba@123.com";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].OrgAddress.AddressOverride = false;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact = null;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var goodsLocationBO = reader.ReadIntoBusinessObject().MovementHeader.GoodsLocation;
			CombineAssertions(() =>
			{
				var goodsLocationAddress = goodsLocationBO.Address;
				AssertEquals("OrgAddress.E2_Postcode", "2180", goodsLocationAddress.E2_Postcode);
				AssertEquals("OrgAddress.E2_RN_NKCountryCode", "AU", goodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("Contact.OC_ContactName instead of OrgAddress.E2_Contact", "GHI", goodsLocationAddress.E2_Contact);
				AssertEquals("Contact.OC_Phone instead of OrgAddress.E2_Phone", "654321", goodsLocationAddress.E2_Phone);
				AssertEquals("Contact.OC_Email instead of OrgAddress.E2_Email", "cba@123.com", goodsLocationAddress.E2_Email);
			});
		}

		public void TestLocationOfGoods_WithoutNamePhoneEmailNodes()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var address = goodsLocation.Address;
			address.E2_Postcode = "2180";
			address.E2_RN_NKCountryCode = "AU";
			address.E2_Contact = "JKL";
			address.E2_Phone = "67890";
			address.E2_Email = "xyz@123.com";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].OrgAddress.AddressOverride = false;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Name = null;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.PhoneNumber = null;
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].Contact.Email = null;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var goodsLocationBO = reader.ReadIntoBusinessObject().MovementHeader.GoodsLocation;
			CombineAssertions(() =>
			{
				var goodsLocationAddress = goodsLocationBO.Address;
				AssertEquals("OrgAddress.E2_Postcode", "2180", goodsLocationAddress.E2_Postcode);
				AssertEquals("OrgAddress.E2_RN_NKCountryCode", "AU", goodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("Contact.OC_ContactName instead of OrgAddress.E2_Contact", "JKL", goodsLocationAddress.E2_Contact);
				AssertEquals("Contact.OC_Phone instead of OrgAddress.E2_Phone", "67890", goodsLocationAddress.E2_Phone);
				AssertEquals("Contact.OC_Email instead of OrgAddress.E2_Email", "xyz@123.com", goodsLocationAddress.E2_Email);
			});
		}

		public void TestLocationOfGoods_NoAddress()
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = "Y";
			var address = goodsLocation.Address;
			address.E2_Postcode = "2180";
			address.E2_RN_NKCountryCode = "AU";
			address.E2_Contact = "ABC";
			address.E2_Phone = "123456";
			address.E2_Email = "abc@123.com";
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_AdditionalIdentifier = "Something";
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].LocationOfGoodsCollection[0].OrgAddress = null;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var goodsLocationBO = reader.ReadIntoBusinessObject().MovementHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				AssertEquals("Qualifier.Code", "Y", goodsLocationBO.CGL_Qualifier);
				AssertEquals("AdditionalIdentifier", "Something", goodsLocationBO.CGL_AdditionalIdentifier);
				AssertEquals("SubType.Code", "A", goodsLocationBO.CGL_Type);
				var goodsLocationAddress = goodsLocationBO.Address;
				AssertEquals("OrgAddress.E2_Postcode", "", goodsLocationAddress.E2_Postcode);
				AssertEquals("OrgAddress.E2_RN_NKCountryCode", "", goodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("OrgAddress.E2_Contact", "", goodsLocationAddress.E2_Contact);
				AssertEquals("OrgAddress.E2_Phone", "", goodsLocationAddress.E2_Phone);
				AssertEquals("OrgAddress.E2_Email", "", goodsLocationAddress.E2_Email);
			});
		}

		public void TestGrossWeight()
		{
			var header = GetNewHeader();
			header.MovementHeader.BM_GrossWeight = 1;
			header.MovementHeader.BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("", (ZDecimal)1, headerBO.MovementHeader.BM_GrossWeight);

			var moveHeader = headerData.InBondMoveHeaderCollection[0];
			moveHeader.GrossWeightUnit.Code = Core.Constants.Weight.Tonnes;
			reader = GetNewReader(headerData, new TestErrorLogger());
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("", (ZDecimal)1000, headerBO.MovementHeader.BM_GrossWeight);

			moveHeader.GrossWeightUnit.Code = "XXX";
			reader = GetNewReader(headerData, new TestErrorLogger());
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("", (ZDecimal)0, headerBO.MovementHeader.BM_GrossWeight);

			moveHeader.GrossWeightUnit = null;
			reader = GetNewReader(headerData, new TestErrorLogger());
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("", (ZDecimal)1, headerBO.MovementHeader.BM_GrossWeight);
		}

		public void TestServices()
		{
			var header = GetNewHeader();
			var service1 = header.Services.AddNew();
			service1.ES_ServiceCode = "ABC";
			var service2 = header.Services.AddNew();
			service2.ES_ServiceCode = "CDE";

			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				var services = headerBO.Services;
				AssertEquals("There should be two AdditionalService(s)", 2, services.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC", "CDE" }, services.Select(x => x.ES_ServiceCode).ToList());
			});
		}

		public void TestImportContainers()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			header.DepartureHeaderContainers.RemoveAndDeleteAll();
			var container1 = NCTSTestHelper.AddContainerAndSealsForTest(header, "CONTAINER1", "SEAL1", "SEAL2", "SEAL3", "SEAL5");
			var container2 = NCTSTestHelper.AddContainerAndSealsForTest(header, "CONTAiNER2", "SEAL3", "", "", "");
			var container3 = NCTSTestHelper.AddContainerAndSealsForTest(header, "CoNTAINER3", "", "", "", "");
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var container1DataObject = headerData.ContainerCollection[0];
			var container2DataObject = headerData.ContainerCollection[1];
			var container3DataObject = headerData.ContainerCollection[2];
			container1DataObject.ContainerNumber = "CONTAINER1A";
			container1DataObject.FCL_LCL_AIR = new UniversalDataBuss.DataObjects.Universal.ContainerMode()
			{
				Code = Core.Constants.ContainerModes.Containerised
			};
			container1DataObject.SealCollection =
				new List<Seal>()
				{
					new Seal()
					{
						SealNumber = "SEAL3",
						Sequence = 3,
						StatusInformation = "NEW"
					},
					new Seal()
					{
						SealNumber = "SEAL4",
						Sequence = 4,
						StatusInformation = "NEW"
					}
		};

			container2DataObject.ContainerNumber = container2.BC_ContainerNum.ToUpperInvariant();
			container2DataObject.FCL_LCL_AIR = new UniversalDataBuss.DataObjects.Universal.ContainerMode()
			{
				Code = Core.Constants.ContainerModes.NonContainerised
			};
			container2DataObject.Seal = "SL2ABC";
			container3DataObject.SecondSeal = "SL3KHD";
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			AssertSame(header, headerBO);
			var sortedContainers = headerBO.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().OrderBy(x => x.BC_ContainerNum.ToUpperInvariant()).ToArray();
			AssertEquals("sortedContainers.Length", 3, sortedContainers.Length);
			AssertEquals("container1.IsDeleted", true, container1.IsDeleted);
			container1 = sortedContainers[0];
			AssertContainer("sortedContainers1", container1, "CONTAINER1A", "SEAL1", "SEAL2", Core.Constants.ContainerModes.Containerised);
			AssertSame(container2, sortedContainers[1]);
			AssertContainer("sortedContainers2", container2, "CONTAiNER2", "SL2ABC", ZString.Empty, Core.Constants.ContainerModes.NonContainerised);
			AssertSame(container3, sortedContainers[2]);
			AssertContainer("sortedContainers3", container3, "CoNTAINER3", ZString.Empty, "SL3KHD", ZString.Empty);

			headerData.ContainerCollection.Clear();
			container1DataObject.Seal = "SL1123";
			headerData.ContainerCollection.Add(container1DataObject);
			headerData.ContainerCollection.Content = CollectionContent.Partial;
			reader = GetNewReader(headerData, new TestErrorLogger());
			headerBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertSame(header, headerBO);
			sortedContainers = headerBO.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().OrderBy(x => x.BC_ContainerNum).ToArray();
			AssertEquals("sortedContainers.Length", 3, sortedContainers.Length);
			AssertSame(container1, sortedContainers[0]);

			AssertEquals(2, container1.AdditionalSeals.Count);

			var additionalSeal1 = container1.AdditionalSeals.Cast<CusSeal>().Single(x => x.BK_SealNumber == "SEAL3");
			AssertEquals((ZShort)3, additionalSeal1.BK_SequenceNumber);
			AssertEquals("NEW", additionalSeal1.BK_UnloadingState);

			var additionalSeal2 = container1.AdditionalSeals.Cast<CusSeal>().Single(x => x.BK_SealNumber == "SEAL4");
			AssertEquals((ZShort)4, additionalSeal2.BK_SequenceNumber);
			AssertEquals("NEW", additionalSeal2.BK_UnloadingState);

			AssertContainer("sortedContainers1", container1, "CONTAINER1A", "SL1123", "SEAL2", Core.Constants.ContainerModes.Containerised);
			AssertSame(container2, sortedContainers[1]);
			AssertContainer("sortedContainers2", container2, "CONTAiNER2", "SL2ABC", ZString.Empty, Core.Constants.ContainerModes.NonContainerised);
			AssertSame(container3, sortedContainers[2]);
			AssertContainer("sortedContainers3", container3, "CoNTAINER3", ZString.Empty, "SL3KHD", ZString.Empty);
		}

		void AssertContainer(string message, NctsDepartureHeaderContainer container, ZString containerNum, ZString seal1, ZString seal2, ZString mode)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("BC_ContainerNum", containerNum, container.BC_ContainerNum);
				AssertEquals("BC_Seal1", seal1, container.BC_Seal1);
				AssertEquals("BC_Seal2", seal2, container.BC_Seal2);
				AssertEquals("BC_Mode", mode, container.BC_Mode);
			});
		}

		public void TestImportContainer_DoesNotThrowForNullSealCollection()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			header.DepartureHeaderContainers.RemoveAndDeleteAll();
			_ = NCTSTestHelper.AddContainerAndSealsForTest(header, "CONTAINER1", "SEAL1", "SEAL2", "", "");

			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);

			var container1DataObject = headerData.ContainerCollection[0];
			container1DataObject.ContainerNumber = "CONTAINER1A";
			container1DataObject.FCL_LCL_AIR = new UniversalDataBuss.DataObjects.Universal.ContainerMode()
			{
				Code = Core.Constants.ContainerModes.Containerised
			};
			container1DataObject.SealCollection = null;

			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();

			var container1 = headerBO.DepartureHeaderContainers.Single();
			AssertContainer("container1", container1, "CONTAINER1A", "SEAL1", "SEAL2", Core.Constants.ContainerModes.Containerised);
		}

		public void TestGoodsItems()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			_ = NCTSTestHelper.AddContainerAndSealsForTest(header, "DANU654321", "", "", "", "");
			_ = NCTSTestHelper.AddContainerAndSealsForTest(header, "MSCU123456", "", "", "", "");
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(latviaCountryCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(latviaCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-1", "DESC1", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			var nctsBill = header.Bills.AddNew();

			var nctsDepartureCargoDesc = nctsBill.GoodsItems.AddNew();
			DGSubstanceTestHelper.CreateIfDoesntExist("0010", "a", "IMO");
			DGSubstanceTestHelper.CreateIfDoesntExist("0004", "b", "IMO");
			nctsDepartureCargoDesc.BY_FormattedHarmonisedTariff = NCTSTestHelper.TestTariffCode;
			nctsDepartureCargoDesc.BY_CusC4Number = "0010";
			nctsDepartureCargoDesc.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0010", "a", "IMO").First().PK;
			nctsDepartureCargoDesc.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0004", "b", "IMO").First().PK;
			Factory.SaveForTesting();

			var headerData = GetShipmentData(header);

			var addInfo1 = nctsDepartureCargoDesc.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "REF";
			addInfo1.CSI_Code = "C658";
			addInfo1.CSI_ReferenceNumber = "ADDINFO1 REF";
			addInfo1.CSI_Description = "DESCRIPTION";

			var suppDoc1 = nctsDepartureCargoDesc.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "C673";
			suppDoc1.CSI_ReferenceNumber = "SUPDOC1 REF";
			suppDoc1.CSI_ItemNumber = 2;
			suppDoc1.CSI_ReferenceNumber2 = "INFORMATION";

			var prevDoc1 = nctsDepartureCargoDesc.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "N821";
			prevDoc1.CSI_ReferenceNumber = "PREVDOC1 REF";
			prevDoc1.CSI_ItemNumber = 1;
			prevDoc1.CSI_PackQty = 5;
			prevDoc1.CSI_PackType = "PL";
			prevDoc1.CSI_Quantity = 2;
			prevDoc1.CSI_UnitOfQuantity = "BX";
			prevDoc1.CSI_ReferenceNumber2 = "COMPLEMENT OF INFORMATION";

			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection = new()
			{
				new InBondMoveDetail
				{
					Sequence = 1,
					InBondMoveLineItemCollection = new ()
					{
						new InBondMoveLineItem
						{
							LineNumber = 1,
							DescriptionAndQuantityOfMerchandise = "Item A",
							Weight = 10,
							WeightUnit = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = Constants.Weight.Kilograms },
							NetWeight = 1,
							NetWeightUnit = new UniversalDataBuss.DataObjects.Universal.UnitOfWeight { Code = Constants.Weight.Kilograms },
							DeclarationType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = NctsPhase5DeclarationTypeList.Codes.T2 },
							Link = 1,
							TariffCode = nctsDepartureCargoDesc.BY_FormattedHarmonisedTariff,
							CustomsSecondQuantity = 2.0,
							CustomsSecondQuantityUnit = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair4Char { Code = "UNT" },
							CountryOfDispatch = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = Core.Constants.CountryCodes.UnitedKingdom },
							CountryOfDestination = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = Core.Constants.CountryCodes.Latvia },
							CountryOfOrigin = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = Core.Constants.CountryCodes.Italy },
							ReferenceNumber = "REF A",
							TransportPaymentMethod = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = TransportChargesModeOfPayment.Codes.CreditCard },

							HazardousMaterial = new HazardousMaterial()
							{
								Code = nctsDepartureCargoDesc.BY_CusC4Number,
								CodeType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair5Char { Code = Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_ECICS },
								UNDGCollection = new ()
								{
									new UniversalDataBuss.DataObjects.Universal.UNDG
									{
										UNDGCode = "0010",
										Standard = "IMO",
									}
								}
							},
							CustomsFirstQuantity = 1,
							CustomsFirstQuantityUnit = new UniversalDataBuss.DataObjects.Universal.UnitOfWeight { Code = Constants.Weight.Kilograms },
							CustomsThirdQuantity = 3.0,
							CustomsThirdQuantityUnit = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair4Char { Code = "UNT" },
							CustomsFourthQuantity = 4.0,
							CustomsFourthQuantityUnit = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair4Char { Code = "DTNG" },
							MonetaryValue = 2.0,
							MonetaryValueCurrency = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "EUR" },
							TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "EUR" },
							LinePrice = 23.09m,
							LinePriceCurrency = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = "AUD" },
							CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
							{
								new CustomsSupportingInformation()
								{
									Category = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
									Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair6Char { Code = addInfo1.CSI_Code },
									ReferenceNumber = addInfo1.CSI_ReferenceNumber,
									Description = addInfo1.CSI_Description,
								},
								new CustomsSupportingInformation()
								{
									Category = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
									Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair6Char { Code = suppDoc1.CSI_Code },
									ReferenceNumber = suppDoc1.CSI_ReferenceNumber,
									Description = suppDoc1.CSI_Description,
									ItemNumber = suppDoc1.CSI_ItemNumber,
								},
								new CustomsSupportingInformation()
								{
									Category = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
									Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair6Char { Code = prevDoc1.CSI_Code },
									ReferenceNumber = prevDoc1.CSI_ReferenceNumber,
									Description = prevDoc1.CSI_Description,
									ItemNumber = prevDoc1.CSI_ItemNumber,
									PackQuantity = prevDoc1.CSI_PackQty,
									PackUnitOfQuantity = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = prevDoc1.CSI_PackType },
									Quantity = prevDoc1.CSI_Quantity,
									UnitOfQuantity = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair4Char { Code = prevDoc1.CSI_UnitOfQuantity },
								},
							},
							CustomsReferenceCollection = new List<CustomsReference>
							{
								new CustomsReference()
								{
									Order = 2,
									Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = NctsUxmlTypeList.Codes.SupplementaryCode },
									SubType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair35Char() { Code = "CN" },
									Reference = "SupplementaryCode Ref"
								},
								new CustomsReference()
								{
									Order = 1,
									Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = NctsUxmlTypeList.Codes.SupplyChainActor },
									SubType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair35Char() { Code = SupplyChainActorRoleList.Codes.CS },
									Owner = GetNewAddressData_WUFSHIJNB("SupplyChainActor Address"),
									Reference = "SupplyChainActor REF",
								},
							},
						}
					}
				}
			};
			var organizationAddresses = new List<UniversalDataBuss.DataObjects.Universal.OrganizationAddress>()
			{
				OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ConsigneeAddress)
			};
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0].SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0].SetOrganizationAddressCollection(() => organizationAddresses);

			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();

			AssertEquals(1, headerBO.Bills[0].GoodsItems.Count);
			var goodsItem1 = headerBO.Bills[0].GoodsItems[0];
			CheckDepartureGoodsItem(headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0], goodsItem1);
		}

		public void TestGoodsItemImportedWithoutException_OtherCountry()
		{
			NctsHeader header;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				header = GetNewHeader();
			}
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			var nctsBill = header.Bills.AddNew();
			nctsBill.GoodsItems.AddNew();

			Factory.SaveForTesting();

			var headerData = GetShipmentData(header);

			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection = new()
			{
				new InBondMoveDetail
				{
					Sequence = 1,
					InBondMoveLineItemCollection =
						new List<InBondMoveLineItem> { new InBondMoveLineItem { LineNumber = 1, DescriptionAndQuantityOfMerchandise = "Item A", } }
				}
			};
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0].SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();

			AssertEquals(1, headerBO.Bills[0].GoodsItems.Count);
		}

		void CheckDepartureGoodsItem(InBondMoveLineItem sourceGoodsItem, NctsDepartureCargoDesc targetGoodsItem)
		{
			AssertEquals(sourceGoodsItem.LineNumber, targetGoodsItem.BY_LineNo.ToZInt());
			AssertEquals(sourceGoodsItem.DescriptionAndQuantityOfMerchandise, targetGoodsItem.BY_Description);
			AssertEquals(sourceGoodsItem.Weight, targetGoodsItem.BY_GrossWeight);
			AssertEquals(sourceGoodsItem.WeightUnit.Code, targetGoodsItem.BY_GrossWeightUnit);
			AssertEquals(sourceGoodsItem.NetWeight, targetGoodsItem.BY_NetWeight);
			AssertEquals(sourceGoodsItem.NetWeightUnit.Code, targetGoodsItem.BY_NetWeightUnit);
			AssertEquals(sourceGoodsItem.TariffCode, targetGoodsItem.BY_FormattedHarmonisedTariff);
			AssertEquals(sourceGoodsItem.CountryOfOrigin.Code, targetGoodsItem.BY_RN_NKCountryOfOrigin);
			AssertEquals(sourceGoodsItem.CountryOfDispatch.Code, targetGoodsItem.BY_RN_NKCountryOfDispatch);
			AssertEquals(sourceGoodsItem.TaxType.Code, targetGoodsItem.BY_ZZF_NKTaxType);
			AssertEquals(sourceGoodsItem.DeclarationType.Code, targetGoodsItem.BY_Type);
			AssertEquals(sourceGoodsItem.ReferenceNumber, targetGoodsItem.BY_CommercialReferenceNumber);
			AssertEquals(sourceGoodsItem.TransportPaymentMethod.Code, targetGoodsItem.BY_TransportChargesMethodOfPayment);
			AssertEquals(sourceGoodsItem.CountryOfDestination.Code, targetGoodsItem.BY_RN_NKCountryOfDestination);
			AssertEquals(sourceGoodsItem.CustomsSecondQuantity, targetGoodsItem.BY_CustomsSecondQuantity);
			AssertEquals(sourceGoodsItem.CustomsSecondQuantityUnit.Code, targetGoodsItem.BY_CustomsSecondUnitQty);
			AssertEquals(sourceGoodsItem.CustomsFirstQuantity, targetGoodsItem.BY_CustomsQuantity);
			AssertEquals(sourceGoodsItem.CustomsFirstQuantityUnit.Code, targetGoodsItem.BY_CustomsUnitQty);
			AssertEquals(sourceGoodsItem.CustomsThirdQuantity, targetGoodsItem.BY_CustomsThirdQuantity);
			AssertEquals(sourceGoodsItem.CustomsThirdQuantityUnit.Code, targetGoodsItem.BY_CustomsThirdUnitQty);
			AssertEquals(sourceGoodsItem.CustomsThirdQuantity, targetGoodsItem.BY_CustomsThirdQuantity);
			AssertEquals(sourceGoodsItem.CustomsThirdQuantityUnit.Code, targetGoodsItem.BY_CustomsThirdUnitQty);
			AssertEquals(sourceGoodsItem.CustomsFourthQuantity, targetGoodsItem.BY_CustomsFourthQuantity);
			AssertEquals(sourceGoodsItem.CustomsFourthQuantityUnit.Code, targetGoodsItem.BY_CustomsFourthUnitQty);
			AssertEquals(sourceGoodsItem.MonetaryValue, targetGoodsItem.BY_MonetaryValue);
			AssertEquals(sourceGoodsItem.MonetaryValueCurrency.Code, targetGoodsItem.BY_RX_NKCurrency);
			AssertEquals(sourceGoodsItem.LinePrice, targetGoodsItem.BY_LinePrice);
			AssertEquals(sourceGoodsItem.LinePriceCurrency.Code, targetGoodsItem.BY_RX_NKLinePriceCurrency);

			foreach (var headerUndg in targetGoodsItem.UNDGs)
			{
				AssertNotNull(targetGoodsItem.UNDGs.Find(x => x?.DI_DG == headerUndg.DI_DG));
				AssertEquals(targetGoodsItem.UNDGs.Count, 1);
				AssertEquals(sourceGoodsItem.HazardousMaterial.UNDGCollection.Count, 1);
			}

			AssertNotNull(targetGoodsItem.Consignee);
			AssertEquals(sourceGoodsItem.OrganizationAddressCollection[0].CompanyName, targetGoodsItem.Consignee.E2_CompanyName);
			AssertEquals(sourceGoodsItem.OrganizationAddressCollection[0].Address1, targetGoodsItem.Consignee.E2_Address1);

			var supQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, targetGoodsItem.PK);
			supQuery.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, targetGoodsItem.TablePrefix);
			var queryResult = Factory.Load<SupplementaryCode>(supQuery);

			AssertEquals(queryResult.FirstOrDefault().CY_ParentID, targetGoodsItem.PK);
			AssertEquals(queryResult.FirstOrDefault().CY_Type, NctsUxmlTypeList.Codes.SupplementaryCode);

			var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, targetGoodsItem.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, targetGoodsItem.TablePrefix);
			var cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(1, cusSupportingInfoGroups.Length);
			AssertEquals("Code", cusSupportingInfoGroups[0].CSI_Code, "C658");
			AssertEquals("Reference", cusSupportingInfoGroups[0].CSI_ReferenceNumber, "ADDINFO1 REF");

			AssertEquals(1, targetGoodsItem.SupportingDocuments.Count);
			AssertCusSupportingInfo(targetGoodsItem.SupportingDocuments[0], "", "C673", "SUPDOC1 REF", "", "", 2);
			AssertEquals(1, targetGoodsItem.PreviousDocuments.Count);
			AssertCusSupportingInfo(targetGoodsItem.PreviousDocuments[0], "", "N821", "PREVDOC1 REF", "", "", 1);
		}

		public void TestFillHazardousMaterial()
		{
			var header = GetNewHeader();
			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection = new()
			{
				new InBondMoveDetail
				{
					Sequence = 1,
					InBondMoveLineItemCollection = new () { new InBondMoveLineItem { LineNumber = 1 } }
				}
			};
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0].SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);

			var reader = GetNewReader(headerData, new TestErrorLogger());
			var sourceGoodsItem = headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection[0];
			NctsHeader headerBO = null;
			AssertNoExceptionThrown(() => headerBO = reader.ReadIntoBusinessObject());
			AssertNull(sourceGoodsItem.HazardousMaterial);

			sourceGoodsItem.HazardousMaterial = new HazardousMaterial()
			{
				Code = "AR1",
				CodeType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair5Char { Code = Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_ECICS },
			};
			AssertNoExceptionThrown(() => headerBO = reader.ReadIntoBusinessObject());
			AssertNull(sourceGoodsItem.HazardousMaterial.UNDGCollection);

			sourceGoodsItem.HazardousMaterial.UNDGCollection = new()
			{
				new UniversalDataBuss.DataObjects.Universal.UNDG { UNDGCode = "0010", Standard = "IMO" }
			};
			AssertNoExceptionThrown(() => headerBO = reader.ReadIntoBusinessObject());
			AssertEquals(sourceGoodsItem.HazardousMaterial.UNDGCollection.Count, 1);
		}

		public void TestImportPackages()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			_ = NCTSTestHelper.AddContainerAndSealsForTest(header, "DANU654321", "", "", "", "");
			_ = NCTSTestHelper.AddContainerAndSealsForTest(header, "MSCU123456", "", "", "", "");
			_ = header.Bills.AddNew();
			Factory.SaveForTesting();

			var headerData = GetShipmentData(header);
			headerData.InBondMoveHeaderCollection[0].InBondMoveDetailCollection = new ()
			{
				new InBondMoveDetail
				{
					Sequence = 1,
					InBondMoveLineItemCollection = new ()
					{
						new InBondMoveLineItem
						{
							DescriptionAndQuantityOfMerchandise = "Item A",
							Link = 1
						},
						new InBondMoveLineItem
						{
							DescriptionAndQuantityOfMerchandise = "Item B",
							Link = 2
						}
					}
				}
			};
			var packingLine1 = CreatePackingLine("BLUE", "CT", 71, "DANU654321", new ()
			{
				new UniversalDataBuss.DataObjects.Universal.PackedItem
				{
					InBondMoveLineItemLink = 1,
					PackedQuantity = 69
				},
				new UniversalDataBuss.DataObjects.Universal.PackedItem
				{
					InBondMoveLineItemLink = 2,
					PackedQuantity = 2
				}
			});
			var packingLine2 = CreatePackingLine("RED", "PK", 1, "MSCU123456", new ()
			{
				new UniversalDataBuss.DataObjects.Universal.PackedItem
				{
					InBondMoveLineItemLink = 2,
					PackedQuantity = 1
				}
			});
			var packingLine3 = CreatePackingLine("BLUE", "CT", 2, "MSCU123456", new ()
			{
				new UniversalDataBuss.DataObjects.Universal.PackedItem
				{
					InBondMoveLineItemLink = 2,
					PackedQuantity = 2
				}
			});
			headerData.SetPackingLineCollection(() => new () { packingLine1, packingLine2, packingLine3 });
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals(2, headerBO.Bills[0].GoodsItems.Count);

				var goodsItem1 = headerBO.Bills[0].GoodsItems[0];
				AssertEquals(1, goodsItem1.Packages.Count);
				AssertPackage(goodsItem1.Packages[0], "BLUE", "CT", 69, "DANU654321");

				var goodsItem2 = headerBO.Bills[0].GoodsItems[1];
				AssertEquals(3, goodsItem2.Packages.Count);
				AssertPackage(goodsItem2.Packages[0], "BLUE", "CT", 2, "DANU654321");
				AssertPackage(goodsItem2.Packages[1], "RED", "PK", 1, "MSCU123456");
				AssertPackage(goodsItem2.Packages[2], "BLUE", "CT", 2, "MSCU123456");
			});
		}

		UniversalDataBuss.DataObjects.Universal.PackingLine CreatePackingLine(ZString marksAndNos, ZString packType, ZLong packQuantity, ZString containerNumber, List<UniversalDataBuss.DataObjects.Universal.PackedItem> packedItems)
		{
			var result = new UniversalDataBuss.DataObjects.Universal.PackingLine
			{
				MarksAndNos = marksAndNos,
				PackType = new UniversalDataBuss.DataObjects.Universal.PackageType { Code = packType },
				PackQty = packQuantity,
				ContainerNumber = containerNumber
			};
			result.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
			result.SetPackedItemCollection(() => packedItems);
			return result;
		}

		void AssertPackage(NctsPackage package, ZString marksAndNos, ZString packType, ZLong packQuantity, ZString containerNumber)
		{
			AssertEquals(marksAndNos, package.B5_MarksAndNumbers);
			AssertEquals(packType, package.B5_UnitType);
			AssertEquals(packQuantity, package.B5_UnitCount);
			AssertCollectionContains(containerNumber, package.ContainersSelected);
		}

		public void TestImportGuarantees()
		{
			var header = GetNewHeader();
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			header.MovementHeader.Guarantees.RemoveAndDeleteAll();
			var guarantee1 = NCTSTestHelper.CreateGuaranteeForTest(header, "9", "12346789", "AAAAAAAAAA", "ABCD", "FR");
			var guarantee2 = NCTSTestHelper.CreateGuaranteeForTest(header, "1", "987654321", "BBBBB", "WXYZ", "BE");
			guarantee1.PW_BondAmount = 150.15m;
			guarantee2.PW_BondAmount = 230.46m;
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var moveHeader = headerData.InBondMoveHeaderCollection[0];
			var guarantee1DataObject = moveHeader.GuaranteeCollection[0];
			var guarantee2DataObject = moveHeader.GuaranteeCollection[1];
			if (guarantee2DataObject.BondType.Code.Value == "1")
			{
				guarantee1DataObject = moveHeader.GuaranteeCollection[1];
				guarantee2DataObject = moveHeader.GuaranteeCollection[0];
			}
			guarantee1DataObject.AccessCode = ZString.Empty;
			guarantee1DataObject.BondAmount = 350.45m;
			guarantee2DataObject.BondNumber = "ABC1234";
			guarantee2DataObject.AccessCode = "EFGH";

			moveHeader.GuaranteeCollection.Add(new Guarantee()
			{
				BondType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair1Char() { Code = "2" },
				BondNumber = "DGE2342",
				BondNumber2 = "KEH8232",
				BondAmount = 546.23m,
				AccessCode = NctsDepartureMovementHeaderDataObjectWriter.GuaranteeAccessCodeMask,
				BondCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "USD" },
				SuretyCode = "SC2",
				BondFiledPort = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair8Char() { Code = "IT12211" },
			});
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertSame(header, headerBO);
			var sortedGuarantees = headerBO.MovementHeader.Guarantees.Cast<NctsGuarantee>().OrderBy(x => x.PW_BondType).ToArray();
			AssertEquals("sortedGuarantees.Length", 3, sortedGuarantees.Length);
			AssertEquals("guarantee1.IsDeleted", true, guarantee1.IsDeleted);
			AssertSame(guarantee2, sortedGuarantees[0]);
			AssertGuarantee("sortedGuarantee1", guarantee2, "1", "987654321", "BBBBB", 350.45m, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertGuarantee("sortedGuarantee2", sortedGuarantees[1], "2", "DGE2342", "KEH8232", 546.23m, ZString.Empty, "USD", "SC2", "IT12211");
			AssertGuarantee("sortedGuarantee3", sortedGuarantees[2], "9", "ABC1234", "AAAAAAAAAA", 150.15m, "EFGH", ZString.Empty, ZString.Empty, ZString.Empty);
		}

		void AssertGuarantee(string message,
			NctsGuarantee guarantee,
			ZString bondType,
			ZString bondNumber,
			ZString bondNumber2,
			ZDecimal bondAmount,
			ZString password,
			ZString currency,
			ZString suretyCode,
			ZString bondFilePort)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("PW_BondType", bondType, guarantee.PW_BondType);
				AssertEquals("PW_BondNumber", bondNumber, guarantee.PW_BondNumber);
				AssertEquals("PW_BondNumber2", bondNumber2, guarantee.PW_BondNumber2);
				AssertEquals("PW_BondAmount", bondAmount, guarantee.PW_BondAmount);
				AssertEquals("PW_Password", password, guarantee.PW_Password);
				AssertEquals("PW_Currency", currency, guarantee.PW_RX_NKCurrency);
				AssertEquals("PW_SuretyCode", suretyCode, guarantee.PW_SuretyCode);
				AssertEquals("PW_BondFiledPort", bondFilePort, guarantee.PW_BondFiledPort);
			});
		}

		public void TestUnmatchedSupplyChainActorsAreDeleted()
		{
			var header = GetNewHeader();
			var moveHeaderBO = header.MovementHeader;
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			var actor1 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor1.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor1.CFR_OA_Owner = OrgWUFSHIJNB.MainAddress.PK;
			actor1.CFR_Reference = "REF98761";
			actor1.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-15);
			var actor2 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor2.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor2.CFR_Reference = "REF9876A";
			actor2.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-14);
			var actor3 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor3.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor3.CFR_OA_Owner = OrgWUFSHIJNB.MainAddress.PK;
			actor3.CFR_Reference = "REF9876B";
			actor3.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-13);
			var actor4 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor4.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor4.CFR_Reference = "REF98761";
			actor4.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-12);
			var actor5 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor5.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor5.CFR_OA_Owner = OrgWUFSHIJNB.MainAddress.PK;
			actor5.CFR_Reference = "REF98761";
			actor5.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-11);
			var actor6 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor6.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor6.CFR_Reference = "REF9876C";
			actor6.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			var actor7 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor7.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor7.CFR_OA_Owner = OrgWUFSHIJNB.MainAddress.PK;
			actor7.CFR_Reference = "REF9876D";
			actor7.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-9);
			var actor8 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor8.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor8.CFR_Reference = "REF98761";
			actor8.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-8);
			var actor9 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor9.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			actor9.CFR_Reference = "REF9876E";
			actor9.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-7);
			var actor10 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor10.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor10.CFR_OA_Owner = OrgINTHEMSYD.MainAddress.PK;
			actor10.CFR_Reference = "REF9876F";
			actor10.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-6);
			var actor11 = moveHeaderBO.CusSupplyChainActors.AddNew();
			actor11.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor11.CFR_Reference = "REF9876H";
			actor11.CFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header);
			var moveHeader = headerData.InBondMoveHeaderCollection[0];
			moveHeader.CustomsReferenceCollection
				.Single(x => x.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor && x.SubType.GetCodeAsUpperCase() == SupplyChainActorRoleList.Codes.FW && x.Reference.GetValueOrDefault() == "REF9876E")
				.SubType.Code = SupplyChainActorRoleList.Codes.MF;
			moveHeader.CustomsReferenceCollection
				.Single(x => x.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor && (x.Owner?.OrganizationCode.GetValueOrDefault() ?? ZString.Empty) == OrgINTHEMSYD.OH_Code && x.Reference.GetValueOrDefault() == "REF9876F")
				.SubType.Code = SupplyChainActorRoleList.Codes.MF;
			moveHeader.CustomsReferenceCollection
				.Single(x => x.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor && x.Reference.GetValueOrDefault() == "REF9876H")
				.SubType.Code = SupplyChainActorRoleList.Codes.MF;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertSame(header, headerBO);
			var supplyChainActors = headerBO.MovementHeader.CusSupplyChainActors.Cast<CusSupplyChainActorReference>().OrderBy(x => x.CFR_SystemCreateTimeUtc).ThenBy(x => x.CFR_Reference).ThenBy(x => x.CFR_Code).ToArray();
			AssertEquals("headerBO.CusSupplyChainActors.Count", 11, supplyChainActors.Length);
			AssertEquals("actor9.IsDeleted", true, actor9.IsDeleted);
			AssertEquals("actor10.IsDeleted", true, actor10.IsDeleted);
			AssertEquals("actor11.IsDeleted", true, actor11.IsDeleted);
			AssertSupplyChainActor("actor1", supplyChainActors[0], SupplyChainActorRoleList.Codes.CS, "REF98761", OrgWUFSHIJNB.MainAddress.PK);
			AssertSupplyChainActor("actor2", supplyChainActors[1], SupplyChainActorRoleList.Codes.CS, "REF9876A", ZGuid.Empty);
			AssertSupplyChainActor("actor3", supplyChainActors[2], SupplyChainActorRoleList.Codes.WH, "REF9876B", OrgWUFSHIJNB.MainAddress.PK);
			AssertSupplyChainActor("actor4", supplyChainActors[3], SupplyChainActorRoleList.Codes.WH, "REF98761", ZGuid.Empty);
			AssertSupplyChainActor("actor5", supplyChainActors[4], SupplyChainActorRoleList.Codes.CS, "REF98761", OrgWUFSHIJNB.MainAddress.PK);
			AssertSupplyChainActor("actor6", supplyChainActors[5], SupplyChainActorRoleList.Codes.CS, "REF9876C", ZGuid.Empty);
			AssertSupplyChainActor("actor7", supplyChainActors[6], SupplyChainActorRoleList.Codes.WH, "REF9876D", OrgWUFSHIJNB.MainAddress.PK);
			AssertSupplyChainActor("actor8", supplyChainActors[7], SupplyChainActorRoleList.Codes.WH, "REF98761", ZGuid.Empty);
			AssertSupplyChainActor("actor9", supplyChainActors[8], SupplyChainActorRoleList.Codes.MF, "REF9876E", ZGuid.Empty);
			AssertSupplyChainActor("actor10", supplyChainActors[9], SupplyChainActorRoleList.Codes.MF, "REF9876F", OrgINTHEMSYD.MainAddress.PK);
			AssertSupplyChainActor("actor11", supplyChainActors[10], SupplyChainActorRoleList.Codes.MF, "REF9876H", ZGuid.Empty);
		}

		void AssertSupplyChainActor(string message, CusSupplyChainActorReference actor, ZString code, ZString reference, ZGuid ownerAddressPK)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("CFR_Code", code, actor.CFR_Code);
				AssertEquals("CFR_Reference", reference, actor.CFR_Reference);
				AssertEquals("CFR_OA_Owner", ownerAddressPK, actor.CFR_OA_Owner);
			});
		}

		public void TestImportMainData()
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var headerBO = SetupTestObject();
				var movementHeaderBO = headerBO.MovementHeader;
				AssertDepartureMovementHeaderData(movementHeaderBO);
			}
		}

		public void TestImport_WithManualDepartureCustomerReferenceEnabled_False()
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var headerBO = SetupTestObject();
				var movementHeaderBO = headerBO.MovementHeader;
				AssertEquals("LRN not set when registry not enabled", ZString.Empty, movementHeaderBO.BM_PaperlessInbondNum);
			}
		}

		void AssertDepartureMovementHeaderData(NctsDepartureMovementHeader movementHeaderBO)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Declaration Type", NctsPhase5DeclarationTypeList.Codes.T2, movementHeaderBO.BM_InBondEntryType);
				AssertEquals("Additional Declaration Type", NctsTypeOfAdditionalDeclarationList.Codes.D, movementHeaderBO.BM_AdditionalDeclarationType);
				AssertEquals("PaymentMethod", TransportChargesModeOfPayment.Codes.Cash, movementHeaderBO.BM_MethodOfPayment);
				AssertEquals("Country of Dispatch", "AU", movementHeaderBO.BM_RN_NKCountryOfDispatch);
				AssertEquals("Country of Destination", "AU", movementHeaderBO.BM_RL_NKDestinationPort);
				AssertEquals("Place of Loading", "AUSYD", movementHeaderBO.BM_PortOfPresentationCode);
				AssertEquals("Place of Unloading", "AUSYD", movementHeaderBO.BM_ForeignDestPortKCode);
				AssertEquals("UCR", "UCR213", movementHeaderBO.BM_UniqueConsignmentReference);
				AssertEquals("LRN", "LRN1234", movementHeaderBO.BM_PaperlessInbondNum);
				AssertEquals("Date Limit", new ZDateTime(2023, 3, 1), movementHeaderBO.BM_ExportDate);
				AssertEquals("Reduced Dataset Indicator", ZBool.True, movementHeaderBO.BM_ReducedDatasetIndicator);
				AssertEquals("Security", NctsTypeOfSecurityList.Codes.ENT, movementHeaderBO.BM_TypeOfSecurity);
				AssertEquals("Circumstances", NctsSpecificCircumstanceIndicatorList.Codes.XXX, movementHeaderBO.BM_SpecificCircumstance);
				AssertEquals("Representative.E2_OA_Address", OrgINTHEMSYD.MainAddress.PK, movementHeaderBO.Representative.E2_OA_Address);
				AssertEquals("Carrier.E2_OA_Address", orgWUFSHIJNB.MainAddress.PK, movementHeaderBO.Carrier.E2_OA_Address);

				AssertEquals("InlandTransportModeAtDeparture", ModeOfTransportList.Codes._1_SeaTransport, movementHeaderBO.InlandTransportModeAtDeparture);
				AssertEquals("TransportTypeAtDeparture", NctsTransportTypeOfIdList.Codes._10, movementHeaderBO.TransportTypeAtDeparture);
				AssertEquals("TransportAtDeparture, same value as VesselNameAtDeparture.", "VS001", movementHeaderBO.TransportAtDeparture);
				AssertEquals("VesselNameAtDeparture", "VS001", movementHeaderBO.VesselNameAtDeparture);
				AssertEquals("VesselCountryAtDeparture", Core.Constants.CountryCodes.Germany, movementHeaderBO.VesselCountryAtDeparture);
				AssertEquals("Trailer1IDAtDeparture", ZString.Empty, movementHeaderBO.Trailer1IDAtDeparture);
				AssertEquals("Trailer1NationalityAtDeparture", ZString.Empty, movementHeaderBO.Trailer1NationalityAtDeparture);
				AssertEquals("Trailer2IDAtDeparture", ZString.Empty, movementHeaderBO.Trailer2IDAtDeparture);
				AssertEquals("Trailer2NationalityAtDeparture", ZString.Empty, movementHeaderBO.Trailer2NationalityAtDeparture);

				AssertEquals("BM_ExportTransportMode", ModeOfTransportList.Codes._2_RailTransport, movementHeaderBO.BM_ExportTransportMode);
				AssertEquals("BM_ActiveBorderIdentificationType", NctsTransportTypeOfIdList.Codes._21, movementHeaderBO.BM_ActiveBorderIdentificationType);
				AssertEquals("BM_RN_NKTOLCarrierNationality", Core.Constants.CountryCodes.France, movementHeaderBO.BM_RN_NKTOLCarrierNationality);
				AssertEquals("BM_ConveyanceNumber", "A1A123F", movementHeaderBO.BM_ConveyanceNumber);
				AssertEquals("BM_CustomsOfficeAtBorder", "CUSOFF1", movementHeaderBO.BM_CustomsOfficeAtBorder);
			});
		}

		NctsHeader SetupTestObject(
			string transportMode = ModeOfTransportList.Codes._1_SeaTransport, string transportType = NctsTransportTypeOfIdList.Codes._10,
			Action<NctsHeader> additionalSetting = null,
			Action<UniversalShipment> additionalWriter = null
		)
		{
			var header = GetNewHeader();
			header.BH_JobReference = "NCT555";
			var movementHeader = header.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			movementHeader.BM_ReducedDatasetIndicator = true;
			movementHeader.BM_ExportDate = new ZDateTime(2023, 3, 1);
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			movementHeader.BM_RN_NKCountryOfDispatch = "AU";
			movementHeader.BM_RL_NKDestinationPort = "AU";
			movementHeader.BM_PortOfPresentationCode = "AUSYD";
			movementHeader.BM_ForeignDestPortKCode = "AUSYD";
			movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
			movementHeader.BM_UniqueConsignmentReference = "UCR213";
			movementHeader.BM_PaperlessInbondNum = "LRN1234";
			movementHeader.Representative.E2_OA_Address = OrgINTHEMSYD.MainAddress.PK;
			movementHeader.Carrier.E2_OA_Address = OrgWUFSHIJNB.MainAddress.PK;

			movementHeader.InlandTransportModeAtDeparture = transportMode;
			movementHeader.TransportTypeAtDeparture = transportType;
			movementHeader.BM_TransportAtDeparture = "VS001";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Germany;
			movementHeader.Trailer1IDAtDeparture = "Trailer1";
			movementHeader.Trailer1NationalityAtDeparture = Core.Constants.CountryCodes.Poland;
			movementHeader.Trailer2IDAtDeparture = "Trailer2";
			movementHeader.Trailer2NationalityAtDeparture = Core.Constants.CountryCodes.Italy;

			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.France;
			movementHeader.BM_ConveyanceNumber = "A1A123F";
			movementHeader.BM_CustomsOfficeAtBorder = "CUSOFF1";

			additionalSetting?.Invoke(header);

			var headerData = GetShipmentData(header);
			additionalWriter?.Invoke(headerData);

			header.Delete();
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var result = reader.ReadIntoBusinessObject();
			return result;
		}

		public void TestTransportDepartureFields_Road()
		{
			var headerBO = SetupTestObject(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30);
			var movementHeaderBO = headerBO.MovementHeader;

			CombineAssertions("Transport Departure Fields, with ROA, Vessel fields should be empty.", () =>
			{
				AssertEquals("InlandTransportModeAtDeparture", ModeOfTransportList.Codes._3_RoadTransport, movementHeaderBO.InlandTransportModeAtDeparture);
				AssertEquals("TransportTypeAtDeparture", NctsTransportTypeOfIdList.Codes._30, movementHeaderBO.TransportTypeAtDeparture);
				AssertEquals("TransportAtDeparture, same as VesselNameAtDeparture", "VS001", movementHeaderBO.TransportAtDeparture);
				AssertEquals("TransportCountryAtDeparture", Core.Constants.CountryCodes.Germany, movementHeaderBO.TransportCountryAtDeparture);
				AssertEquals("VesselNameAtDeparture, same as TransportAtDeparture", "VS001", movementHeaderBO.VesselNameAtDeparture);
				AssertEquals("VesselCountryAtDeparture", Core.Constants.CountryCodes.Germany, movementHeaderBO.VesselCountryAtDeparture);
				AssertEquals("Trailer1IDAtDeparture", "Trailer1", movementHeaderBO.Trailer1IDAtDeparture);
				AssertEquals("Trailer1NationalityAtDeparture", Core.Constants.CountryCodes.Poland, movementHeaderBO.Trailer1NationalityAtDeparture);
				AssertEquals("Trailer2IDAtDeparture", "Trailer2", movementHeaderBO.Trailer2IDAtDeparture);
				AssertEquals("Trailer2NationalityAtDeparture", Core.Constants.CountryCodes.Italy, movementHeaderBO.Trailer2NationalityAtDeparture);
			});
		}

		public void TestTransportDepartureFields_NullTypeOfIdentification()
		{
			var headerBO = SetupTestObject(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30,
				header =>
				{
					var movement = header.MovementHeader;
					movement.BM_TransportAtDepartureType = ZString.Empty;
					movement.BM_ActiveBorderIdentificationType = ZString.Empty;
				}
			);
			var movementHeaderBO = headerBO.MovementHeader;

			CombineAssertions(() =>
			{
				AssertEquals("BM_TransportAtDepartureType", ZString.Empty, movementHeaderBO.BM_TransportAtDepartureType);
				AssertEquals("BM_ActiveBorderIdentificationType", ZString.Empty, movementHeaderBO.BM_ActiveBorderIdentificationType);
			});
		}

		public void TestTransportDepartureFields_NullNationalities()
		{
			var headerBO = SetupTestObject(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30,
				header =>
				{
					var movement = header.MovementHeader;
					movement.BM_RN_NKTransportAtDepartureCountry = ZString.Empty;
					movement.BM_RN_NKTransportAtDepartureTrailer1Nationality = ZString.Empty;
					movement.BM_RN_NKTransportAtDepartureTrailer2Nationality = ZString.Empty;
					movement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
				}
			);
			var movementHeaderBO = headerBO.MovementHeader;

			CombineAssertions(() =>
			{
				AssertEquals("TransportCountryAtDeparture", ZString.Empty, movementHeaderBO.TransportCountryAtDeparture);
				AssertEquals("Trailer1NationalityAtDeparture", ZString.Empty, movementHeaderBO.Trailer1NationalityAtDeparture);
				AssertEquals("Trailer2NationalityAtDeparture", ZString.Empty, movementHeaderBO.Trailer2NationalityAtDeparture);
			});
		}

		public void TestTransportMeans_MissingNationality()
		{
			var strategy = new Mock<IDataObjectWriterStrategy>();
			strategy.Setup(x => x.IsAllowSet(It.IsAny<string>())).Returns(true);

			var shipment = new UniversalShipment(strategy.Object);
			var moveHeader = new InBondMoveHeader(strategy.Object);
			var departureMeans = new TransportMeans
			{
				TransportType = TransportTypeCode.Departure,
				IdentificationNumber = "DEP1",
				TypeOfIdentification = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle }
			};
			var departureTrailer1Means = new TransportMeans
			{
				TransportType = TransportTypeCode.Departure,
				IdentificationNumber = "TRAILER1",
				TypeOfIdentification = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer }
			};
			var departureTrailer2Means = new TransportMeans
			{
				TransportType = TransportTypeCode.Departure,
				IdentificationNumber = "TRAILER2",
				TypeOfIdentification = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer }
			};
			var borderMeans = new TransportMeans { TransportType = TransportTypeCode.Border, IdentificationNumber = "BRDR1" };
			moveHeader.SetTransportMeansCollection(() => new List<TransportMeans> { departureMeans, departureTrailer1Means, departureTrailer2Means, borderMeans });
			shipment.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader> { moveHeader });

			var reader = GetNewReader(shipment, new TestErrorLogger());
			NctsHeader headerBO = null;
			AssertNoExceptionThrown(() => headerBO = reader.ReadIntoBusinessObject());

			var moveHeaderBO = headerBO.MovementHeader;
			AssertEquals("DEP1", moveHeaderBO.BM_TransportAtDeparture);
			AssertEquals("TRAILER1", moveHeaderBO.BM_TransportAtDepartureTrailer1RegNo);
			AssertEquals("TRAILER2", moveHeaderBO.BM_TransportAtDepartureTrailer2RegNo);
			AssertEquals("BRDR1", moveHeaderBO.BM_TOLCarrierID);
			AssertEquals(ZString.Empty, moveHeaderBO.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals(ZString.Empty, moveHeaderBO.BM_RN_NKTransportAtDepartureTrailer1Nationality);
			AssertEquals(ZString.Empty, moveHeaderBO.BM_RN_NKTransportAtDepartureTrailer2Nationality);
			AssertEquals(ZString.Empty, moveHeaderBO.BM_RN_NKTOLCarrierNationality);
		}

		public void TestTransportMeansCollection_NullAndEmpty()
		{
			AssertNoExceptionThrown("Should not throw exception with null TransportMeansCollection", () => SetupTestObject(additionalWriter: dataObject => dataObject.SetTransportMeansCollection(() => null)));
			AssertNoExceptionThrown("Should not throw exception with Empty TransportMeansCollection", () => SetupTestObject(additionalWriter: dataObject => dataObject.SetTransportMeansCollection(() => new List<TransportMeans>())));
		}

		public void TestTirCarnetNumber()
		{
			var header = GetNewHeader();
			header.MovementHeader.TirCarnetNumber = "123456789012";
			Factory.SaveForTesting();

			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var headerBO = reader.ReadIntoBusinessObject();

			AssertEquals("Tir Carnet Number", "123456789012", headerBO.MovementHeader.TirCarnetNumber);
		}

		OrgHeader OrgINTHEMSYD => orgINTHEMSYD ?? (orgINTHEMSYD = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory));
		OrgHeader orgINTHEMSYD;

		OrgHeader OrgWUFSHIJNB => orgWUFSHIJNB ?? (orgWUFSHIJNB = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory));
		OrgHeader orgWUFSHIJNB;

		protected override NctsDepartureMovementHeaderDataObjectReader GetNewReader(UniversalShipment headerData, TestErrorLogger testErrorLogger) => new NctsDepartureMovementHeaderDataObjectReader(headerData, testErrorLogger, Factory);

		protected override NctsHeader GetNewHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}
	}
}
