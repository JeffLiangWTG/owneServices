using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using AddInfoConst = Enterprise.Customs.EU.NCTS.DataTransfer.DataObjectWriterConstants.DepartureMovementHeader.AddInfo;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5.Testing
{
	class NctsDepartureMovementHeaderDataObjectWriterTest : NctsMovementHeaderDataObjectWriter<NctsDepartureMovementHeaderDataObjectWriter>
	{
		public void TestCommercialInfo()
		{
			var headerBO = GetNewHeader();
			var bill = headerBO.Bills.AddNew();

			SetupDepartureGoodsItemsForTest(Factory, bill.GoodsItems.AddNew(), true, 1);
			SetupDepartureGoodsItemsForTest(Factory, bill.GoodsItems.AddNew(), false, 2);
			var giWithoutPart = bill.GoodsItems.AddNew();
			SetupDepartureGoodsItemsForTest(Factory, giWithoutPart, true, 3);
			giWithoutPart.BY_OP_Part = ZGuid.Empty;
			AssertNull(giWithoutPart.Part);
			Factory.Save();
			Shipment data = null;
			AssertNoExceptionThrown(() =>
			{
				data = GetDataObject(headerBO);
			});

			var line1 = data.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Single(e => e.LineNo == 1);
			var line2 = data.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Single(e => e.LineNo == 2);
			var line3 = data.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Single(e => e.LineNo == 3);

			AssertDepartureGoodsItems(line1, 1, true);
			AssertDepartureGoodsItems(line2, 2, false);
			AssertNull("When NctsDepartureCargoDesc.Part is null", line3.PartNo);
		}

		public void TestInBondMoveLineItemCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("RID", 0.1m, Core.Constants.CountryCodes.Latvia, 0.1m, 0.1m, "VAT");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "12345678");
			var headerBO = GetNewHeader();
			var bill = headerBO.Bills.AddNew();

			SetupDepartureGoodsItemsForTest(Factory, bill.GoodsItems.AddNew(), false, 1);
			SetupDepartureGoodsItemsForTest(Factory, bill.GoodsItems.AddNew(), false, 2);
			Factory.Save();
			bill.GoodsItems[0].Lookups.CusCodeList.Load();

			var data = GetDataObject(headerBO);
			foreach (var line in data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].InBondMoveLineItemCollection)
			{
				AssertDepartureGoodsItems(line, line.LineNumber.Value);
			}
		}

		public void TestAdditionalBillCollection()
		{
			var headerBO = GetNewHeader();
			for (var i = 1; i <= 2; ++i)
			{
				var bill = headerBO.Bills.AddNew();
				SetupBill(bill, i);
			}
			Factory.Save();

			var data = GetDataObject(headerBO);
			AssertNotNull("AdditionalBillCollection", data.AdditionalBillCollection);
			AssertEquals("AdditionalBillCollection.Count", 2, data.AdditionalBillCollection.Count);

			AssertAdditionalBill(data.AdditionalBillCollection[0], 1);
			AssertAdditionalBill(data.AdditionalBillCollection[1], 2);

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

			void AssertAdditionalBill(AdditionalBill bill, int i)
			{
				AssertEquals($"BILL{i}", bill.BillNumber);
				AssertEquals($"COR{i}", GetAddressOfType(bill.OrganizationAddressCollection, DocAddressType.ConsignorDocumentaryAddress)?.OrganizationCode);
				AssertEquals($"CEE{i}", GetAddressOfType(bill.OrganizationAddressCollection, DocAddressType.ConsigneeDocumentaryAddress)?.OrganizationCode);
				AssertEquals("AUD", bill.LinePriceCurrency.Code);
				AssertEquals("CustomsSupportingInformationCollection.Count", 7, bill.CustomsSupportingInformationCollection?.Count);
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[0], $"add{i}1", "C651", $"inf-ref{i}", "INF", null, null);
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[1], $"add{i}2", "C653", $"ref-ref{i}", "REF", null, null);
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[2], $"add{i}3", "C652", $"tra-ref{i}", "TRA", null, null);
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[3], $"pre{i}1", "N830", $"pre-ref{i}1", ZString.Empty, $"pre-com{i}1");
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[4], $"pre{i}2", "N831", $"pre-ref{i}2", ZString.Empty, $"pre-com{i}2");
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[5], $"sup{i}1", "C641", $"sup-ref{i}1", ZString.Empty, $"sup-com{i}1", 1);
				AssertCustomsSupportingInformation(bill.CustomsSupportingInformationCollection[6], $"sup{i}2", "C642", $"sup-ref{i}2", ZString.Empty, $"sup-com{i}2", 2);
				AssertEquals("CustomsReferenceCollection.Count", 2, bill.CustomsReferenceCollection.Count);
				AssertEquals($"Reference{i}1", bill.CustomsReferenceCollection[0].Reference);
				AssertEquals(NctsUxmlTypeList.Codes.SupplyChainActor, bill.CustomsReferenceCollection[0].Type.Code);
				AssertEquals(i == 1 ? SupplyChainActorRoleList.Codes.CS : SupplyChainActorRoleList.Codes.FW, bill.CustomsReferenceCollection[0].SubType.Code);
				AssertEquals($"Reference{i}2", bill.CustomsReferenceCollection[1].Reference);
				AssertEquals(NctsUxmlTypeList.Codes.SupplyChainActor, bill.CustomsReferenceCollection[1].Type.Code);
				AssertEquals(i == 1 ? SupplyChainActorRoleList.Codes.WH : SupplyChainActorRoleList.Codes.MF, bill.CustomsReferenceCollection[1].SubType.Code);
			}
		}

		static OrganizationAddress GetAddressOfType(IEnumerable<OrganizationAddress> addresses, DocAddressType type) =>
				addresses.SingleOrDefault(e =>
					e.AddressType.GetValueOrDefault() == type.ToString());

		public void TestInBondMoveDetail()
		{
			var data = SetupInBondMoveDetailTest(road: false);
			AssertInBondMoveDetail(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0], 1);
			AssertTransportMeansSea(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].TransportMeansCollection);
			AssertInBondMoveDetail(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[1], 2);
			AssertTransportMeansSea(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[1].TransportMeansCollection);
		}

		public void TestInBondMoveDetail_Road()
		{
			var data = SetupInBondMoveDetailTest(road: true);
			AssertInBondMoveDetail(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0], 1);
			AssertTransportMeansRoad(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[0].TransportMeansCollection);
			AssertInBondMoveDetail(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[1], 2);
			AssertTransportMeansRoad(data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection[1].TransportMeansCollection);
		}

		Shipment SetupInBondMoveDetailTest(bool road)
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_InlandTransportMode = road ? ModeOfTransportList.Codes._3_RoadTransport : ModeOfTransportList.Codes._1_SeaTransport;
			for (var i = (short)1; i <= 2; ++i)
			{
				var bill = headerBO.Bills.AddNew();
				bill.B0_TransportPaymentMethod = i == 1 ? TransportChargesModeOfPayment.Codes.Cash : TransportChargesModeOfPayment.Codes.CreditCard;
				bill.B0_RN_NKCountryOfExport = $"E{i}";
				bill.B0_RN_NKCountryOfDestination = $"D{i}";
				SetupMoveDetailTransportDeparture(bill, road ? NctsTransportTypeOfIdList.Codes._30 : NctsTransportTypeOfIdList.Codes._10);
				bill.B0_Weight = i * 1.23m;
				bill.B0_WeightUQ = $"W{i}";
			}

			var data = GetDataObject(headerBO);
			AssertNotNull("InBondMoveHeaderCollection", data.InBondMoveHeaderCollection);
			AssertEquals("InBondMoveHeaderCollection.Count", 1, data.InBondMoveHeaderCollection.Count);
			AssertNotNull("InBondMoveHeaderCollection[0].InBondMoveDetailCollection", data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection);
			AssertEquals("InBondMoveHeaderCollection[0].InBondMoveDetailCollection.Count", 2, data.InBondMoveHeaderCollection[0].InBondMoveDetailCollection.Count);

			return data;
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

		static void AssertInBondMoveDetail(InBondMoveDetail detail, int i)
		{
			AssertEquals(i, detail.Sequence);
			AssertEquals(i == 1 ? TransportChargesModeOfPayment.Codes.Cash : TransportChargesModeOfPayment.Codes.CreditCard, detail.TransportPaymentMethod.Code);
			AssertEquals(i == 1 ? TransportChargesModeOfPayment.Descriptions.Cash : TransportChargesModeOfPayment.Descriptions.CreditCard, detail.TransportPaymentMethod.Description);
			AssertEquals($"E{i}", detail.DispatchCountry.Code);
			AssertEquals($"D{i}", detail.DestinationCountry.Code);
			AssertEquals(i * 1.23m, detail.Weight);
			AssertEquals($"W{i}", detail.WeightUnit.Code);
		}

		static void AssertTransportMeansRoad(List<TransportMeans> means)
		{
			AssertEquals("TransportMeansCollection.Count", means.Count, 3);
			AssertEquals("TransportMeansCollection[0].TransportType", TransportTypeCode.Departure, means[0].TransportType);
			AssertEquals("TransportMeansCollection[0].Order", 0, means[0].Order);
			AssertEquals("TransportMeansCollection[0].IdentificationNumber", "VS001", means[0].IdentificationNumber);
			AssertEquals("TransportMeansCollection[0].Nationality", Core.Constants.CountryCodes.Germany, means[0].Nationality.Code);
			AssertEquals("TransportMeansCollection[0].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, means[0].TypeOfIdentification.Code);
			AssertEquals("TransportMeansCollection[0].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle, means[0].TypeOfIdentification.Description);

			AssertEquals("TransportMeansCollection[1].TransportType", TransportTypeCode.Departure, means[1].TransportType);
			AssertEquals("TransportMeansCollection[1].Order", 1, means[1].Order);
			AssertEquals("TransportMeansCollection[1].IdentificationNumber", "Trailer1", means[1].IdentificationNumber);
			AssertEquals("TransportMeansCollection[1].Nationality", Core.Constants.CountryCodes.Poland, means[1].Nationality.Code);
			AssertEquals("TransportMeansCollection[1].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, means[1].TypeOfIdentification.Code);
			AssertEquals("TransportMeansCollection[1].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, means[1].TypeOfIdentification.Description);

			AssertEquals("TransportMeansCollection[2].TransportType", TransportTypeCode.Departure, means[2].TransportType);
			AssertEquals("TransportMeansCollection[2].Order", 2, means[2].Order);
			AssertEquals("TransportMeansCollection[2].IdentificationNumber", "Trailer2", means[2].IdentificationNumber);
			AssertEquals("TransportMeansCollection[2].Nationality", Core.Constants.CountryCodes.Italy, means[2].Nationality.Code);
			AssertEquals("TransportMeansCollection[2].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, means[2].TypeOfIdentification.Code);
			AssertEquals("TransportMeansCollection[2].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, means[2].TypeOfIdentification.Description);
		}

		static void AssertTransportMeansSea(List<TransportMeans> means)
		{
			AssertEquals("TransportMeansCollection.Count", means.Count, 1);
			AssertEquals("TransportMeansCollection[0].TransportType", TransportTypeCode.Departure, means[0].TransportType);
			AssertEquals("TransportMeansCollection[0].Order", 0, means[0].Order);
			AssertEquals("TransportMeansCollection[0].IdentificationNumber", "VS001", means[0].IdentificationNumber);
			AssertEquals("TransportMeansCollection[0].Nationality", Constants.CountryCodes.Germany, means[0].Nationality.Code);
			AssertEquals("TransportMeansCollection[0].TransportType", TransportMeansList.Codes.ImoShipIdentificationNumber, means[0].TypeOfIdentification.Code);
			AssertEquals("TransportMeansCollection[0].TransportType.Description", TransportMeansList.Descriptions.ImoShipIdentificationNumber, means[0].TypeOfIdentification.Description);
		}

		public void TestWarehouseAddresses()
		{
			var headerBO = GetNewHeader();
			headerBO.Consignee.E2_OA_Address = CreateAddressForTest("CEE");
			headerBO.Consignor.E2_OA_Address = CreateAddressForTest("COR");
			headerBO.MovementHeader.BM_OA_WarehouseAddress = CreateAddressForTest("WHA");

			var data = GetDataObject(headerBO);
			var warehouseAddress = GetAddressOfType(data.OrganizationAddressCollection, DocAddressType.CustomsWarehouseAddress);
			var warehouseClientAddress = GetAddressOfType(data.OrganizationAddressCollection, DocAddressType.WarehouseClient);
			var importDocumentary = GetAddressOfType(data.OrganizationAddressCollection, DocAddressType.ImporterDocumentaryAddress);

			AssertEquals("warehouseAddress", "WHA", warehouseAddress.AddressShortCode);
			AssertEquals("warehouseClientAddress", "COR", warehouseClientAddress.AddressShortCode);
			AssertEquals("importDocumentary", "CEE", importDocumentary.AddressShortCode);
		}

		ZGuid CreateAddressForTest(string code)
		{
			var org = NCTSTestHelper.CreateOrgAddressForTest(Factory, code);
			org.Header.OH_Code = code;
			org.OA_Code = code;
			org.Header.OH_IsWarehouseClient = true;
			return org.PK;
		}

		public void TestBookingConfirmationReference()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_PaperlessInbondNum = "REF1";
			AssertEquals("BookingConfirmationReference is also set to the LRN when there is only one MovementHeader", "REF1", GetDataObject(headerBO).BookingConfirmationReference);
		}

		public void TestNoContainersOnBWR()
		{
			var headerBO = GetNewHeader();
			headerBO.DepartureHeaderContainers.AddNew();
			AssertNull(GetNewWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWR, headerBO))).GetDataObject(headerBO).ContainerCollection);
			AssertNotNull(GetNewWriter(new DataWritingManager(new ActionInfo(null, headerBO))).GetDataObject(headerBO).ContainerCollection);
		}

		public void TestMessageType()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_InBondEntryType = "T";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("T", moveHeader.EntryType.Code);
			AssertEquals("Mixed consignments comprising both goods to be placed under external Union transit procedure and goods which are to be placed under the internal Union transit procedure", moveHeader.EntryType.Description);
		}

		public void TestMessageSubType()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_AdditionalDeclarationType = "D";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("D", moveHeader.AdditionalEntryType.Code);
			AssertEquals("Pre-lodged Declaration", moveHeader.AdditionalEntryType.Description);
		}

		public void TestPorts()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_RN_NKCountryOfDispatch = "IE";
			headerBO.MovementHeader.BM_RL_NKDestinationPort = "BE";
			headerBO.MovementHeader.BM_PortOfPresentationCode = "IEROS";
			headerBO.MovementHeader.BM_ForeignDestPortKCode = "BEZEE";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];

			CombineAssertions(() =>
			{
				AssertEquals("Port of Origin Code", "IE", moveHeader.PortOfOrigin.Code);
				AssertEquals("Port of Destination Code", "BE", moveHeader.PortOfDestination.Code);
				AssertEquals("Port of Origin Code", "IEROS", moveHeader.PortOfLoading.Code);
				AssertEquals("Port of Discharge Code", "BEZEE", moveHeader.PortOfDischarge.Code);
			});
		}

		public void TestTotalWeight()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_GrossWeight = 222;
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("Gross Weight", 222m, moveHeader.GrossWeight);
			AssertEquals("Gross Weight Unit Code", "KG", moveHeader.GrossWeightUnit.Code);
		}

		public void TestAdditionalInfoCollection()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.IsSimplifiedNctsProcedure = true;
			headerBO.MovementHeader.BM_ReducedDatasetIndicator = true;
			headerBO.MovementHeader.BM_TypeOfSecurity = "NON";
			headerBO.MovementHeader.BM_SpecificCircumstance = "1";
			headerBO.MovementHeader.TirCarnetNumber = "12345678901";

			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var addInfos = moveHeader.AddInfoCollection;
			AssertEquals("count", 7, addInfos.Count);
			CombineAssertions(() =>
			{
				AssertEquals("IsSimplifiedNctsProcedure Key", "IsSimplifiedProcedure", addInfos[0].Key);
				AssertEquals("IsSimplifiedNctsProcedure Value", "Y", addInfos[0].Value);

				AssertEquals("BM_ReducedDatasetIndicator Key", "ReducedDatasetIndicator", addInfos[1].Key);
				AssertEquals("BM_ReducedDatasetIndicator Value", "Y", addInfos[1].Value);

				AssertEquals("BM_TypeOfSecurity Key", "SecurityIndicator", addInfos[2].Key);
				AssertEquals("BM_TypeOfSecurity Value", "NON", addInfos[2].Value);

				AssertEquals("BM_SpecificCircumstance Key", "SpecificCircumstance", addInfos[3].Key);
				AssertEquals("BM_SpecificCircumstance Value", "1", addInfos[3].Value);

				AssertEquals("TirCarnetNumber Key", "TirCarnetNumber", addInfos[6].Key);
				AssertEquals("TirCarnetNumber Value", "12345678901", addInfos[6].Value);
			});
		}

		public void TestCustomsReferenceCollection_Empty()
		{
			var headerBO = GetNewHeader();
			var movementHeader = headerBO.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			CombineAssertions(() =>
			{
				var supplyChainActorCollection = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor);
				AssertEquals("Empty SupplyChainActors should export empty record", 1, supplyChainActorCollection.Count());
				var supplyChainActor = supplyChainActorCollection.First();
				AssertNull("Empty SupplyChainActors should export empty record", supplyChainActor.Reference);
				AssertNull("Empty SupplyChainActors should export empty record", supplyChainActor.Owner);
				AssertNull("Empty SupplyChainActors should export empty record", supplyChainActor.SubType);

				var authorizationUsageCollection = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == "AUT");
				AssertEquals("Empty AuthorizationUsages should export empty record", 1, authorizationUsageCollection.Count());
				var authorizationUsage = authorizationUsageCollection.First();
				AssertNull("Empty AuthorizationUsages should export empty record", authorizationUsage.Reference);
				AssertNull("Empty AuthorizationUsages should export empty record", authorizationUsage.Owner);
				AssertNull("Empty AuthorizationUsages should export empty record", authorizationUsage.SubType);

				var customsOfficeCollection = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.OfficeCode);
				AssertEquals("Empty CustomsOffices should export empty record", 1, customsOfficeCollection.Count());
				var customsOffice = customsOfficeCollection.First();
				AssertNull("Empty CustomsOffices should export empty record", customsOffice.Reference);
				AssertNull("Empty CustomsOffices should export empty record", customsOffice.DateCollection);
				AssertNull("Empty CustomsOffices should export empty record", customsOffice.SubType);
				AssertNull("Empty CustomsOffices should export empty record", customsOffice.ReferencedEntityDescription);

				var countryOfRoutingCollection = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.CountryOfRoutingCode);
				AssertEquals("Empty CountryOfRoutings should export empty record", 1, countryOfRoutingCollection.Count());
				var countryOfRouting = countryOfRoutingCollection.First();
				AssertNull("Empty CountryOfRoutings should export empty record", countryOfRouting.SubType);
			});
		}

		public void TestCustomsReferenceCollection_SupplyChainActors()
		{
			var headerBO = GetNewHeader();
			var actor1 = headerBO.MovementHeader.CusSupplyChainActors.AddNew();
			actor1.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor1.CFR_SystemCreateTimeUtc = new ZDateTime(2023, 1, 10);
			var actor2 = headerBO.MovementHeader.CusSupplyChainActors.AddNew();
			actor2.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			actor2.CFR_SystemCreateTimeUtc = new ZDateTime(2023, 1, 9);
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var supplyChainActorDataObjects = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor).ToArray();
			AssertEquals("supplyChainActorDataObjects.Length", 2, supplyChainActorDataObjects.Length);
			AssertEquals("supplyChainActorDataObjects[0].SubType.Code", SupplyChainActorRoleList.Codes.WH, supplyChainActorDataObjects[0].SubType.Code);
			AssertEquals("supplyChainActorDataObjects[1].SubType.Code", SupplyChainActorRoleList.Codes.CS, supplyChainActorDataObjects[1].SubType.Code);
		}

		public void TestCustomsReferenceCollection_CustomsAuthorizations()
		{
			var headerBO = GetNewHeader();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "Nanjing";
			var movementHeader = headerBO.MovementHeader;
			var authorizationUsage1 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_OH_Owner = orgHeader.PK;
			authorizationUsage1.AGC_Number = "usage1";
			authorizationUsage1.AGC_Code = "ACR";
			var authorizationUsage2 = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_OH_Owner = orgHeader.PK;
			authorizationUsage2.AGC_Number = "usage2";
			authorizationUsage2.AGC_Code = "TRD";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var cusAuthorisations = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == "AUT").ToList();
			AssertEquals("count", 2, cusAuthorisations.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Reference", "usage1", cusAuthorisations[0].Reference);
				AssertEquals("Code", "ACR", cusAuthorisations[0].SubType.Code);
				AssertEquals("Address1", "Address1", cusAuthorisations[0].Owner.Address1);
				AssertEquals("Address2", "Address2", cusAuthorisations[0].Owner.Address2);
				AssertEquals("City", "Nanjing", cusAuthorisations[0].Owner.City);
				AssertEquals("Reference", "usage2", cusAuthorisations[1].Reference);
				AssertEquals("Code", "TRD", cusAuthorisations[1].SubType.Code);
				AssertEquals("Address1", "Address1", cusAuthorisations[1].Owner.Address1);
				AssertEquals("Address2", "Address2", cusAuthorisations[1].Owner.Address2);
				AssertEquals("City", "Nanjing", cusAuthorisations[1].Owner.City);
			});
		}

		public void TestCustomsReferenceCollection_CustomsOffices()
		{
			var headerBO = GetNewHeader();
			var departureMovement = headerBO.MovementHeader;
			SetupCustomsOffice();
			departureMovement.CustomsOfficesForDeparture[0].CY_Data = "IEROS100";
			departureMovement.CustomsOfficesForDeparture[0].CY_Date = ZDateTime.BrettsBirthday;
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var cusOffices = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.OfficeCode).ToList();
			AssertEquals("count", 2, cusOffices.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Type Code", "EUO", cusOffices[0].Type.Code);
				AssertEquals("SubType Code", "DES", cusOffices[0].SubType.Code);
				AssertEquals("Reference", "IEROS100", cusOffices[0].Reference);
				AssertEquals("ReferencedEntityDescription", "Rosslare Europort", cusOffices[0].ReferencedEntityDescription);
				AssertEquals("Date Collection Type", DateType.DateAtOffice, cusOffices[0].DateCollection[0].Type);
				AssertEquals("Date Collection Value", ZDateTime.BrettsBirthday, cusOffices[0].DateCollection[0].Value);

				AssertEquals("Type Code", "EUO", cusOffices[1].Type.Code);
				AssertEquals("Sub Type Code", "DEP", cusOffices[1].SubType.Code);
			});
		}

		void SetupCustomsOffice()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("IE", parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE");
			helper.CreateNewOrGetExistingCusCodeList("IEROS100", "IE", "Rosslare Europort", new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination });
			Factory.Save();
		}

		public void TestCustomsReferenceCollection_CountriesOfRouting()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008;
			var eurpoeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.China, "China", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Ireland, "Ireland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australia", startDate, endDate);
			Factory.Save();

			var headerBO = GetNewHeader();
			var country1 = headerBO.CountriesOfRouting.AddNew();
			country1.CY_Order = 2;
			country1.CY_Data = "IE";
			var country2 = headerBO.CountriesOfRouting.AddNew();
			country2.CY_Order = 1;
			country2.CY_Data = "CN";
			var country3 = headerBO.CountriesOfRouting.AddNew();
			country3.CY_Order = 3;
			country3.CY_Data = "AU";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var countriesOfRouting = moveHeader.CustomsReferenceCollection.Where(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.CountryOfRoutingCode).ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, countriesOfRouting.Count);
				AssertCountryOfRoutingCustomsReference(countriesOfRouting[0], 1, "CN", "China");
				AssertCountryOfRoutingCustomsReference(countriesOfRouting[1], 2, "IE", "Ireland");
				AssertCountryOfRoutingCustomsReference(countriesOfRouting[2], 3, "AU", "Australia");
			});

			void AssertCountryOfRoutingCustomsReference(CustomsReference customsReference, int order, string countryCode, ZString countryDescription)
			{
				AssertEquals("Type Code", "COR", customsReference.Type.Code);
				AssertEquals("Type Description", "Country Of Routing Code", customsReference.Type.Description);
				AssertEquals("Order", order, customsReference.Order);
				AssertEquals("SubType Code", countryCode, customsReference.SubType.Code);
				AssertEquals("SubType Description", countryDescription, customsReference.SubType.Description);
			}
		}

		public void TestCustomsSupportingInformationCollection()
		{
			var headerBO = GetNewHeader();
			var supportingDocument1 = headerBO.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "C641";
			supportingDocument1.CSI_Description = "sup1";
			supportingDocument1.CSI_ReferenceNumber = "sup-ref1";
			supportingDocument1.CSI_ItemNumber = 1;
			supportingDocument1.CSI_ReferenceNumber2 = "sup-com1";
			var supportingDocument2 = headerBO.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "C642";
			supportingDocument2.CSI_Description = "sup2";
			supportingDocument2.CSI_ReferenceNumber = "sup-ref2";
			supportingDocument2.CSI_ItemNumber = 2;
			supportingDocument2.CSI_ReferenceNumber2 = "sup-com2";

			var additionalDocument1 = headerBO.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_SubType = "INF";
			additionalDocument1.CSI_Description = "add1";
			additionalDocument1.CSI_Code = "C651";
			additionalDocument1.CSI_ReferenceNumber = "inf-ref";
			var additionalDocument2 = headerBO.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_SubType = "TRA";
			additionalDocument2.CSI_Code = "C652";
			additionalDocument2.CSI_ReferenceNumber = "tra-ref";
			additionalDocument2.CSI_Description = "add2";

			var previousDocument1 = headerBO.PreviousDocuments.AddNew();
			previousDocument1.CSI_Description = "pre1";
			previousDocument1.CSI_Code = "N830";
			previousDocument1.CSI_ReferenceNumber = "pre-ref1";
			previousDocument1.CSI_ReferenceNumber2 = "pre-com1";
			var previousDocument2 = headerBO.PreviousDocuments.AddNew();
			previousDocument2.CSI_Description = "pre2";
			previousDocument2.CSI_Code = "N831";
			previousDocument2.CSI_ReferenceNumber = "pre-ref2";
			previousDocument2.CSI_ReferenceNumber2 = "pre-com2";

			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var cusSupportingInfos = moveHeader.CustomsSupportingInformationCollection;
			AssertEquals("count", 6, cusSupportingInfos.Count);
			CombineAssertions(() =>
			{
				AssertCustomsSupportingInformation(cusSupportingInfos[0], "add1", "C651", "inf-ref", "INF", null, null);
				AssertCustomsSupportingInformation(cusSupportingInfos[1], "add2", "C652", "tra-ref", "TRA", null, null);
				AssertCustomsSupportingInformation(cusSupportingInfos[2], "pre1", "N830", "pre-ref1", ZString.Empty, "pre-com1");
				AssertCustomsSupportingInformation(cusSupportingInfos[3], "pre2", "N831", "pre-ref2", ZString.Empty, "pre-com2");
				AssertCustomsSupportingInformation(cusSupportingInfos[4], "sup1", "C641", "sup-ref1", ZString.Empty, "sup-com1", 1);
				AssertCustomsSupportingInformation(cusSupportingInfos[5], "sup2", "C642", "sup-ref2", ZString.Empty, "sup-com2", 2);
			});
		}

		public void TestPopulateLocalProcessingFromServices()
		{
			var headerBO = GetNewHeader();
			var dataObject = GetDataObject(headerBO);

			AssertNull("LocalProcessing should be null if no services", dataObject.LocalProcessing);

			var service1 = headerBO.Services.AddNew();
			service1.ES_ServiceCode = "ABC";

			var service2 = headerBO.Services.AddNew();
			service2.ES_ServiceCode = "CDE";

			dataObject = GetDataObject(headerBO);

			CombineAssertions(() =>
			{
				var localProcessing = dataObject.LocalProcessing;
				AssertNotNull("Should Populate LocalProcessing", localProcessing);
				var additionalServiceCollection = localProcessing.AdditionalServiceCollection;
				AssertNotNull("Should Populate LocalProcessing.AdditionalServiceCollection", additionalServiceCollection);

				AssertEquals("There should be two AdditionalService(s)", 2, additionalServiceCollection.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC", "CDE" }, additionalServiceCollection.Select(x => x.ServiceCode.Code).ToList());
			});
		}

		void AssertCustomsSupportingInformation(CustomsSupportingInformation supportingInformation, ZString expectedDescription, ZString expectedCode, ZString expectedReferenceNumber, ZString expectedSubType, ZString? expectedReferenceNumber2 = null, ZInt? expectedItemNumber = null)
		{
			AssertEquals("Description", expectedDescription, supportingInformation.Description);
			AssertEquals("Code", expectedCode, supportingInformation.Type.Code);
			AssertEquals("ReferenceNumber", expectedReferenceNumber, supportingInformation.ReferenceNumber);
			AssertEquals("ItemNumber", expectedItemNumber, supportingInformation.ItemNumber);
			if (expectedReferenceNumber2 is null)
			{
				AssertNull("ReferenceNumber2", supportingInformation.ReferenceNumberCollection);
			}
			else
			{
				AssertEquals("ReferenceNumber2", expectedReferenceNumber2, supportingInformation.ReferenceNumberCollection[0].ReferenceNumber);
			}
			AssertEquals("SubType", expectedSubType, supportingInformation.SubType.Code);
		}

		public void TestDateLimit()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_ExportDate = ZDateTime.BrettsBirthday;
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("Date Limit Type", DateType.DateLimit, moveHeader.DateCollection[0].Type);
			AssertEquals("Date Limit Value", ZDateTime.BrettsBirthday, moveHeader.DateCollection[0].Value);
		}

		public void TestGuaranteesCollection()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Constants.CountryCodes.Italy);
			helper.CreateNewOrGetExistingCusCodeList("IT304199", Constants.CountryCodes.Italy, "PESCARA", new ZString[] { "CGU" });
			Factory.Save();

			var headerBO = GetNewHeader();
			var guarantees = NCTSTestHelper.SetupGuaranteesForTest(headerBO);
			guarantees[0].PW_BondAmount = 150.15m;
			guarantees[0].PW_Password = "ABCD";
			guarantees[0].PW_RX_NKCurrency = "USD";
			guarantees[1].PW_BondAmount = 230.46m;
			guarantees[1].PW_Password = ZString.Empty;
			guarantees[1].PW_SuretyCode = "SC2";
			guarantees[1].PW_BondFiledPort = "IT304199";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var guaranteeDataObjects = moveHeader.GuaranteeCollection.OrderBy(x => x.BondType.GetCodeAsUpperCase()).ToArray();
			AssertEquals("guaranteeDataObjects.Length", 2, guaranteeDataObjects.Length);
			AssertGuarantee("GUARANTEE 1",
				guaranteeDataObjects[0],
				bondType: "1",
				bondNumber: "987654321",
				accessCode: ZString.Empty,
				bondNumber2: "BBBBB",
				bondAmount: 230.46m,
				bondCurrency: (ZString.Empty, ZString.Empty),
				suretyCode: "SC2",
				bondFilePort: ("IT304199", "PESCARA"));

			AssertGuarantee("GUARANTEE 2", guaranteeDataObjects[1],
				bondType: "9",
				bondNumber: "12346789",
				accessCode: NctsDepartureMovementHeaderDataObjectWriter.GuaranteeAccessCodeMask,
				bondNumber2: "AAAAAAAAAA",
				bondAmount: 150.15m,
				bondCurrency: ("USD", "United States Dollar"),
				suretyCode: ZString.Empty,
				bondFilePort: (ZString.Empty, ZString.Empty));
		}

		void AssertGuarantee(string message,
			Guarantee guarantee,
			ZString bondType,
			ZString bondNumber,
			ZString accessCode,
			ZString bondNumber2,
			ZDecimal bondAmount,
			(ZString Code, ZString Description) bondCurrency,
			ZString suretyCode,
			(ZString Code, ZString Description) bondFilePort)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("BondType", bondType, guarantee.BondType.GetNullableCodeAsUpperCase());
				AssertEquals("BondNumber", bondNumber, guarantee.BondNumber);
				AssertEquals("AccessCode", accessCode, guarantee.AccessCode);
				AssertEquals("BondNumber2", bondNumber2, guarantee.BondNumber2);
				AssertEquals("BondAmount", bondAmount, guarantee.BondAmount);
				AssertEquals("Currency Code", bondCurrency.Code, guarantee.BondCurrency?.Code ?? ZString.Empty);
				AssertEquals("Currency Description", bondCurrency.Description, guarantee.BondCurrency?.Description ?? ZString.Empty);
				AssertEquals("SuretyCode", suretyCode, guarantee.SuretyCode);
				AssertEquals("BondFilePort Code", bondFilePort.Code, guarantee.BondFiledPort?.Code ?? ZString.Empty);
				AssertEquals("BondFilePort Description", bondFilePort.Description, guarantee.BondFiledPort?.Description ?? ZString.Empty);
			});
		}

		public void TestLocationOfGoods_Empty()
		{
			var headerBO = GetNewHeader();
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("No Goods Location will output empty collection", 0, moveHeader.LocationOfGoodsCollection.Count);
		}

		public void TestLocationOfGoods()
		{
			var headerBO = GetNewHeader();
			var goodsLocation = headerBO.MovementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = "Y";
			var address = goodsLocation.Address;
			address.AuthorisationNumber = "AUTH";
			address.E2_RN_NKCountryCode = "CN";
			address.Postcode = "210001";
			address.E2_Contact = "TestName";
			address.E2_Phone = "123456";
			address.E2_Email = "abc@123.com";
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_AdditionalIdentifier = "Something";

			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("Count", 1, moveHeader.LocationOfGoodsCollection.Count);
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalIdentifier", "Something", moveHeader.LocationOfGoodsCollection[0].AdditionalIdentifier);
				AssertEquals("AuthorisationNumber", "AUTH", moveHeader.LocationOfGoodsCollection[0].AuthorizationNumber);
				AssertEquals("OrgAddress.Code", "CN", moveHeader.LocationOfGoodsCollection[0].OrgAddress.Country.Code);
				AssertEquals("OrgAddress.Postcode", "210001", moveHeader.LocationOfGoodsCollection[0].OrgAddress.Postcode);
				AssertEquals("OrgAddress.Contact", "TestName", moveHeader.LocationOfGoodsCollection[0].OrgAddress.Contact);
				AssertEquals("OrgAddress.Phone", "123456", moveHeader.LocationOfGoodsCollection[0].OrgAddress.Phone);
				AssertEquals("OrgAddress.Email", "abc@123.com", moveHeader.LocationOfGoodsCollection[0].OrgAddress.Email);
				AssertEquals("Contact.Name", "TestName", moveHeader.LocationOfGoodsCollection[0].Contact.Name);
				AssertEquals("Contact.PhoneNumber", "123456", moveHeader.LocationOfGoodsCollection[0].Contact.PhoneNumber);
				AssertEquals("Contact.Email", "abc@123.com", moveHeader.LocationOfGoodsCollection[0].Contact.Email);
				AssertEquals("Qualifier.Code", "Y", moveHeader.LocationOfGoodsCollection[0].Qualifier.Code);
				AssertEquals("Qualifier.Description", "Authorization Number", moveHeader.LocationOfGoodsCollection[0].Qualifier.Description);
				AssertEquals("SubType.Code", "A", moveHeader.LocationOfGoodsCollection[0].SubType.Code);
				AssertEquals("SubType.Description", "Designated Location", moveHeader.LocationOfGoodsCollection[0].SubType.Description);
			});
		}

		public void TestCarrier()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.Carrier.E2_OA_Address = Factory.New<JobDocAddress>().PK;
			headerBO.MovementHeader.Carrier.CompanyName = "Test Company";
			headerBO.MovementHeader.Carrier.E2_Address1 = "10 Main Street";
			headerBO.MovementHeader.Carrier.E2_City = "Limerick";
			headerBO.MovementHeader.Carrier.E2_Postcode = "L01 453";
			headerBO.MovementHeader.Carrier.E2_RN_NKCountryCode = "IE";
			var moveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			AssertEquals("Count", 1, moveHeader.OrganizationAddressCollection.Count);
			var address = moveHeader.OrganizationAddressCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("Company Name", "Test Company", address.CompanyName);
				AssertEquals("Address1", "10 Main Street", address.Address1);
				AssertEquals("City", "Limerick", address.City);
				AssertEquals("Country Code", "IE", address.Country.Code);
				AssertEquals("Postcode", "L01 453", address.Postcode);
			});
		}

		public void TestRepresentative()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.Representative.E2_OA_Address = Factory.New<JobDocAddress>().PK;
			headerBO.MovementHeader.Representative.CompanyName = "Test Rep";
			headerBO.MovementHeader.Representative.E2_Address1 = "12 Main Street";
			headerBO.MovementHeader.Representative.E2_City = "Ennis";
			headerBO.MovementHeader.Representative.E2_Postcode = "CXE 453";
			headerBO.MovementHeader.Representative.E2_RN_NKCountryCode = "IE";
			var dataObject = GetDataObject(headerBO);
			AssertEquals("Count", 1, dataObject.OrganizationAddressCollection.Count);
			var address = dataObject.OrganizationAddressCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("Company Name", "Test Rep", address.CompanyName);
				AssertEquals("Address1", "12 Main Street", address.Address1);
				AssertEquals("City", "Ennis", address.City);
				AssertEquals("Country Code", "IE", address.Country.Code);
				AssertEquals("Postcode", "CXE 453", address.Postcode);
			});
		}

		public void TestContainerCollection()
		{
			var headerBO = GetNewHeader();
			var containers = NCTSTestHelper.SetupContainersAndSealsForTest(headerBO);
			containers[0].BC_Mode = Core.Constants.ContainerModes.Containerised;
			containers[1].BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			containers[2].BC_Mode = ZString.Empty;
			var dataObject = GetDataObject(headerBO);
			var containerDataObjects = dataObject.ContainerCollection.OrderBy(x => x.ContainerNumber.GetValueOrDefault()).ToArray();
			AssertEquals("containerDataObjects.Length", 3, containerDataObjects.Length);
			AssertContainers("Container 1", containerDataObjects[0], Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised, "CONTAINER1", "SEAL1", "SEAL2", "SEAL3", "SEAL4");
			AssertContainers("Container 2", containerDataObjects[1], Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised, "CONTAINER2", "SEAL3", "");
			AssertContainers("Container 3", containerDataObjects[2], string.Empty, null, "CONTAINER3", string.Empty, string.Empty);
		}

		void AssertContainers(string message, Container container, string modeCode, string modeDescription, string containerNumber, string seal, string secondSeal, string thirdSeal = null, string fourthSeal = null)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("FCL_LCL_AIR.Code", modeCode, container.FCL_LCL_AIR.Code);
				AssertEquals("FCL_LCL_AIR.Description", modeDescription, container.FCL_LCL_AIR.Description);
				AssertEquals("ContainerNumber", containerNumber, container.ContainerNumber);
				AssertEquals("Seal", seal, container.Seal);
				AssertEquals("SecondSeal", secondSeal, container.SecondSeal);

				if (thirdSeal != null)
				{
					var seal3 = container.SealCollection.Single(s => s.SealNumber.Value == thirdSeal);
					AssertEquals(thirdSeal, seal3.SealNumber);
					AssertEquals(3, seal3.Sequence);
					AssertEquals("NEW", seal3.StatusInformation);
				}

				if (fourthSeal != null)
				{
					var seal4 = container.SealCollection.Single(s => s.SealNumber.Value == fourthSeal);
					AssertEquals(fourthSeal, seal4.SealNumber);
					AssertEquals(4, seal4.Sequence);
					AssertEquals("NEW", seal4.StatusInformation);
				}
			});
		}

		public void TestTransportMeansCollection_Sea()
		{
			var dataObject = GetDataObject(SetupTransDepartureFields(ModeOfTransportList.Codes._1_SeaTransport, NctsTransportTypeOfIdList.Codes._10));
			var transportMeansCollection = dataObject.InBondMoveHeaderCollection[0].TransportMeansCollection.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("TransportMeansCollection should contain 2 items for SEA-10", 2, transportMeansCollection.Length);

				AssertEquals("TransportMeansCollection[0].TransportType", TransportTypeCode.Departure, transportMeansCollection[0].TransportType);
				AssertEquals("TransportMeansCollection[0].Order", 0, transportMeansCollection[0].Order);
				AssertEquals("TransportMeansCollection[0].IdentificationNumber", "VS001", transportMeansCollection[0].IdentificationNumber);
				AssertEquals("TransportMeansCollection[0].Nationality", Core.Constants.CountryCodes.Germany, transportMeansCollection[0].Nationality.Code);
				AssertEquals("TransportMeansCollection[0].TransportType", TransportMeansList.Codes.ImoShipIdentificationNumber, transportMeansCollection[0].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[0].TransportType.Description", TransportMeansList.Descriptions.ImoShipIdentificationNumber, transportMeansCollection[0].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[0].ModeOfTransport.Code", "1", transportMeansCollection[0].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[0].ModeOfTransport.Description", ModeOfTransportList.Descriptions._1_SeaTransport, transportMeansCollection[0].ModeOfTransport.Description);

				AssertEquals("TransportMeansCollection[1].TransportType", TransportTypeCode.Border, transportMeansCollection[1].TransportType);
				AssertEquals("TransportMeansCollection[1].Order", 1, transportMeansCollection[1].Order);
				AssertEquals("TransportMeansCollection[1].IdentificationNumber", "NC15REG", transportMeansCollection[1].IdentificationNumber);
				AssertEquals("TransportMeansCollection[1].Nationality", Core.Constants.CountryCodes.France, transportMeansCollection[1].Nationality.Code);
				AssertEquals("TransportMeansCollection[1].TransportType", TransportMeansList.Codes.TrainNumber, transportMeansCollection[1].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[1].TransportType.Description", TransportMeansList.Descriptions.TrainNumber, transportMeansCollection[1].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Code", "2", transportMeansCollection[1].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Description", ModeOfTransportList.Descriptions._2_RailTransport, transportMeansCollection[1].ModeOfTransport.Description);
			});
		}

		public void TestTransportMeansCollection_Road()
		{
			var dataObject = GetDataObject(SetupTransDepartureFields(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30));
			var transportMeansCollection = dataObject.InBondMoveHeaderCollection[0].TransportMeansCollection.ToArray();

			CombineAssertions("Transport Departure Fields transport mode ROA.", () =>
			{
				AssertEquals("TransportMeansCollection should contain 4 items for ROA-30", 4, transportMeansCollection.Length);

				AssertEquals("TransportMeansCollection[0].TransportType", TransportTypeCode.Departure, transportMeansCollection[0].TransportType);
				AssertEquals("TransportMeansCollection[0].Order", 0, transportMeansCollection[0].Order);
				AssertEquals("TransportMeansCollection[0].IdentificationNumber", "VS001", transportMeansCollection[0].IdentificationNumber);
				AssertEquals("TransportMeansCollection[0].Nationality", Core.Constants.CountryCodes.Germany, transportMeansCollection[0].Nationality.Code);
				AssertEquals("TransportMeansCollection[0].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, transportMeansCollection[0].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[0].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle, transportMeansCollection[0].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[0].ModeOfTransport.Code", "3", transportMeansCollection[0].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[0].ModeOfTransport.Description", ModeOfTransportList.Descriptions._3_RoadTransport, transportMeansCollection[0].ModeOfTransport.Description);

				AssertEquals("TransportMeansCollection[1].TransportType", TransportTypeCode.Departure, transportMeansCollection[1].TransportType);
				AssertEquals("TransportMeansCollection[1].Order", 1, transportMeansCollection[1].Order);
				AssertEquals("TransportMeansCollection[1].IdentificationNumber", "Trailer1", transportMeansCollection[1].IdentificationNumber);
				AssertEquals("TransportMeansCollection[1].Nationality", Core.Constants.CountryCodes.Poland, transportMeansCollection[1].Nationality.Code);
				AssertEquals("TransportMeansCollection[1].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeansCollection[1].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[1].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, transportMeansCollection[1].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Code", "3", transportMeansCollection[1].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Description", ModeOfTransportList.Descriptions._3_RoadTransport, transportMeansCollection[1].ModeOfTransport.Description);

				AssertEquals("TransportMeansCollection[2].TransportType", TransportTypeCode.Departure, transportMeansCollection[2].TransportType);
				AssertEquals("TransportMeansCollection[2].Order", 2, transportMeansCollection[2].Order);
				AssertEquals("TransportMeansCollection[2].IdentificationNumber", "Trailer2", transportMeansCollection[2].IdentificationNumber);
				AssertEquals("TransportMeansCollection[2].Nationality", Core.Constants.CountryCodes.Italy, transportMeansCollection[2].Nationality.Code);
				AssertEquals("TransportMeansCollection[2].TransportType", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeansCollection[2].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[2].TransportType.Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, transportMeansCollection[2].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[2].ModeOfTransport.Code", "3", transportMeansCollection[2].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[2].ModeOfTransport.Description", ModeOfTransportList.Descriptions._3_RoadTransport, transportMeansCollection[2].ModeOfTransport.Description);
			});
		}

		public void TestTransportMeansCollection_Border()
		{
			var dataObject = GetDataObject(SetupTransDepartureFields(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30));
			var transportMeansCollection = dataObject.InBondMoveHeaderCollection[0].TransportMeansCollection.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("TransportMeansCollection[3].TransportType", TransportTypeCode.Border, transportMeansCollection[3].TransportType);
				AssertEquals("TransportMeansCollection[3].Order", 3, transportMeansCollection[3].Order);
				AssertEquals("TransportMeansCollection[3].IdentificationNumber", "NC15REG", transportMeansCollection[3].IdentificationNumber);
				AssertEquals("TransportMeansCollection[3].Nationality", Core.Constants.CountryCodes.France, transportMeansCollection[3].Nationality.Code);
				AssertEquals("TransportMeansCollection[3].TransportType", TransportMeansList.Codes.TrainNumber, transportMeansCollection[3].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[3].TransportType.Description", TransportMeansList.Descriptions.TrainNumber, transportMeansCollection[3].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[3].ModeOfTransport.Code", "2", transportMeansCollection[3].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[3].ModeOfTransport.Description", ModeOfTransportList.Descriptions._2_RailTransport, transportMeansCollection[3].ModeOfTransport.Description);
			});

			dataObject = GetDataObject(SetupTransDepartureFields(ModeOfTransportList.Codes._1_SeaTransport, NctsTransportTypeOfIdList.Codes._10));
			transportMeansCollection = dataObject.InBondMoveHeaderCollection[0].TransportMeansCollection.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("TransportMeansCollection[1].TransportType", TransportTypeCode.Border, transportMeansCollection[1].TransportType);
				AssertEquals("TransportMeansCollection[1].Order", 1, transportMeansCollection[1].Order);
				AssertEquals("TransportMeansCollection[1].IdentificationNumber", "NC15REG", transportMeansCollection[1].IdentificationNumber);
				AssertEquals("TransportMeansCollection[1].Nationality", Core.Constants.CountryCodes.France, transportMeansCollection[1].Nationality.Code);
				AssertEquals("TransportMeansCollection[1].TransportType", TransportMeansList.Codes.TrainNumber, transportMeansCollection[1].TypeOfIdentification.Code);
				AssertEquals("TransportMeansCollection[1].TransportType.Description", TransportMeansList.Descriptions.TrainNumber, transportMeansCollection[1].TypeOfIdentification.Description);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Code", "2", transportMeansCollection[1].ModeOfTransport.Code);
				AssertEquals("TransportMeansCollection[1].ModeOfTransport.Description", ModeOfTransportList.Descriptions._2_RailTransport, transportMeansCollection[1].ModeOfTransport.Description);
			});
		}

		public void TestReleaseDate()
		{
			var headerBO = GetNewHeader();
			var universalShipment = GetDataObject(headerBO);
			var universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			AssertNull("Release Date Record", universalInBondMoveHeader.DateCollection.SingleOrDefault(d => d.Type.GetValueOrDefault() == DateType.Release));

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(headerBO, CusEntryNumberTypes.Standard.MovementReferenceNumber, headerBO.CountryCode);
			mrnEntryNumber.CE_IssueDate = new ZDateTime(2024, 01, 12);
			universalShipment = GetDataObject(headerBO);
			universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			AssertEquals("Release Date", new ZDateTime(2024, 01, 12), universalInBondMoveHeader.DateCollection.Single(d => d.Type.Equals(DateType.Release)).Value);
		}

		public void TestAcceptanceDate()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			var universalShipment = GetDataObject(headerBO);
			var universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			AssertNull("Acceptance Date Record", universalInBondMoveHeader.DateCollection.SingleOrDefault(d => d.Type.GetValueOrDefault() == DateType.Acceptance));

			headerBO.MovementHeader.BM_EntryDate = new ZDateTime(2024, 01, 13);
			universalShipment = GetDataObject(headerBO);
			universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			AssertEquals("Acceptance Date", new ZDateTime(2024, 01, 13), universalInBondMoveHeader.DateCollection.Single(d => d.Type.Equals(DateType.Acceptance)).Value);
		}

		public void TestMessagingStatus()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_MessageStatus = "ACC";
			var universalShipment = GetDataObject(headerBO);
			var universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			var messagingStatusRecord = universalInBondMoveHeader.MessagingStatus;
			AssertNotNull("MessagingStatus Record", messagingStatusRecord);
			AssertEquals("MessagingStatus Code", "ACC", messagingStatusRecord.Code);
			AssertEquals("MessagingStatus Description", "Accepted", messagingStatusRecord.Description);
		}

		public void TestPhaseStatus()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_Phase = "013";
			var universalShipment = GetDataObject(headerBO);
			var universalInBondMoveHeader = universalShipment.InBondMoveHeaderCollection[0];
			var phaseStatusRecord = universalInBondMoveHeader.PhaseStatus;
			AssertNotNull("PhaseStatus Record", phaseStatusRecord);
			AssertEquals("PhaseStatus Code", "013", phaseStatusRecord.Code);
			AssertEquals("PhaseStatus Description", "Amendment", phaseStatusRecord.Description);
		}

		public void TestCustomsStatus()
		{
			var headerBO = GetNewHeader();
			headerBO.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem;
			var universalInBondMoveHeader = GetDataObject(headerBO).InBondMoveHeaderCollection[0];
			var customsStatusRecord = universalInBondMoveHeader.CustomsStatus;
			AssertNotNull("CustomsStatus Record", customsStatusRecord);
			AssertEquals("CustomsStatus Code", "ACS", customsStatusRecord.Code);
			AssertEquals("CustomsStatus Description", "Accepted by System", customsStatusRecord.Description);
		}

		public void TestPackingLineCollection()
		{
			var headerBO = GetNewHeader();
			var container1 = NCTSTestHelper.AddContainerAndSealsForTest(headerBO, "DANU654321", "", "", "", "");
			var container2 = NCTSTestHelper.AddContainerAndSealsForTest(headerBO, "MSCU123456", "", "", "", "");
			var bill = headerBO.Bills.AddNew();

			var goodsItem1 = bill.GoodsItems.AddNew();
			goodsItem1.BY_Description = "ITEM 1";
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 69;
			package1.B5_MarksAndNumbers = "BLUE";
			package1.ContainersPivot.AddPivotFor(container1);

			var goodsItem2 = bill.GoodsItems.AddNew();
			goodsItem2.BY_Description = "ITEM 2 (without package)";

			var goodsItem3 = bill.GoodsItems.AddNew();
			goodsItem3.BY_Description = "ITEM 3";
			var package2 = goodsItem3.Packages.AddNew();
			package2.B5_UnitType = "PK";
			package2.B5_UnitCount = 1;
			package2.B5_MarksAndNumbers = "RED";
			package2.ContainersPivot.AddPivotFor(container2);
			var package3 = goodsItem3.Packages.AddNew();
			package3.B5_UnitType = "CT";
			package3.B5_UnitCount = 2;
			package3.B5_MarksAndNumbers = "BLUE";
			package3.ContainersPivot.AddPivotFor(container1);
			package3.ContainersPivot.AddPivotFor(container2);

			CombineAssertions(() =>
			{
				var universalShipment = GetDataObject(headerBO);
				var packingLines = universalShipment.PackingLineCollection;
				AssertEquals(3, packingLines.Count);

				var packingLine1 = packingLines[0];
				AssertEquals(2, packingLine1.PackedItemCollection.Count);
				AssertEquals("CT", packingLine1.PackType.Code);
				AssertEquals((long)71, packingLine1.PackQty);
				AssertEquals("BLUE", packingLine1.MarksAndNos);
				AssertEquals("DANU654321", packingLine1.ContainerNumber);
				AssertEquals(69m, packingLine1.PackedItemCollection[0].PackedQuantity);
				AssertEquals(1, packingLine1.PackedItemCollection[0].InBondMoveLineItemLink);
				AssertEquals(2m, packingLine1.PackedItemCollection[1].PackedQuantity);
				AssertEquals(3, packingLine1.PackedItemCollection[1].InBondMoveLineItemLink);

				var packingLine2 = packingLines[1];
				AssertEquals(1, packingLine2.PackedItemCollection.Count);
				AssertEquals("PK", packingLine2.PackType.Code);
				AssertEquals((long)1, packingLine2.PackQty);
				AssertEquals("RED", packingLine2.MarksAndNos);
				AssertEquals("MSCU123456", packingLine2.ContainerNumber);
				AssertEquals(1m, packingLine2.PackedItemCollection[0].PackedQuantity);
				AssertEquals(3, packingLine2.PackedItemCollection[0].InBondMoveLineItemLink);

				var packingLine3 = packingLines[2];
				AssertEquals(1, packingLine3.PackedItemCollection.Count);
				AssertEquals("CT", packingLine3.PackType.Code);
				AssertEquals((long)2, packingLine3.PackQty);
				AssertEquals("BLUE", packingLine3.MarksAndNos);
				AssertEquals("MSCU123456", packingLine3.ContainerNumber);
				AssertEquals(2m, packingLine3.PackedItemCollection[0].PackedQuantity);
				AssertEquals(3, packingLine3.PackedItemCollection[0].InBondMoveLineItemLink);
			});
		}

		NctsHeader SetupTransDepartureFields(ZString transportMode, ZString transportType)
		{
			var header = GetNewHeader();
			var movementHeader = header.MovementHeader;

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
			movementHeader.BM_TOLCarrierID = "NC15REG";
			movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.France;
			movementHeader.BM_ConveyanceNumber = "A1A123F";
			movementHeader.BM_CustomsOfficeAtBorder = "CUSOFF1";

			return header;
		}

		protected override void SetupAdditionalDataCore(NctsHeader header)
		{
			var movementHeader = header.MovementHeader;
			movementHeader.Representative.E2_OA_Address = OrgWUFSHIJNB.MainAddress.PK;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_ReducedDatasetIndicator = ZBool.True;
			movementHeader.BM_ExportDate = new ZDateTime(2023, 3, 1);
			movementHeader.BM_PresentationDateTime = new ZDateTimeOffset(2023, 12, 11);
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Australia;
			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.UnitedStates;
			movementHeader.BM_PortOfPresentationCode = "AUSYD";
			movementHeader.BM_ForeignDestPortKCode = "USLAX";
			movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
			movementHeader.BM_UniqueConsignmentReference = "UCR213";
			movementHeader.BM_PaperlessInbondNum = "LRN1234";

			var actor = movementHeader.CusSupplyChainActors.AddNew();
			actor.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			actor.CFR_Reference = "REF12345";
			actor.CFR_OA_Owner = OrgINTHEMSYD.MainAddress.PK;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("AA", parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping("NN", parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping("JY", parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AA");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NN");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JY");
			helper.CreateNewOrGetExistingCusCodeList("AA123456", "AA", "AA123456 Office Description", new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture });
			helper.CreateNewOrGetExistingCusCodeList("NN123456", "NN", "NN123456 Office Description", new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination });
			helper.CreateNewOrGetExistingCusCodeList("JY123456", "JY", "JY123456 Office Description", new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival });
			Factory.Save();
			movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AA123456");
			movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "NN123456").CY_Date = new ZDateTime(2012, 10, 12, 6, 6, 0);
			movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "JY123456").CY_Date = new ZDateTime(2012, 10, 12, 6, 6, 0);
		}

		protected override void AssertAdditionalDataCore(Shipment dataObject)
		{
			var moveHeaderData = dataObject.InBondMoveHeaderCollection[0];
			CombineAssertions("Main Departure data", () =>
			{
				AssertEquals("dataObject.MessageType.Code", NctsPhase5DeclarationTypeList.Codes.T2, moveHeaderData.EntryType.Code);
				AssertEquals("dataObject.MessageType.Description", NctsPhase5DeclarationTypeList.Descriptions.T2, moveHeaderData.EntryType.Description);
				AssertEquals("dataObject.MessageSubType.Code", NctsTypeOfAdditionalDeclarationList.Codes.D, moveHeaderData.AdditionalEntryType.Code);
				AssertEquals("dataObject.MessageSubType.Description", NctsTypeOfAdditionalDeclarationList.Descriptions.D, moveHeaderData.AdditionalEntryType.Description);
				AssertEquals("dataObject.PaymentMethod.Code", TransportChargesModeOfPayment.Codes.Cash, moveHeaderData.PaymentMethod.Code);
				AssertEquals("dataObject.PaymentMethod.Description", TransportChargesModeOfPayment.Descriptions.Cash, moveHeaderData.PaymentMethod.Description);
				AssertEquals("AddInfoCollection - ReducedDateSetIndicator", ZBool.True, moveHeaderData.AddInfoCollection.GetZBoolValue(AddInfoConst.ReducedDateSetIndicator));
				AssertEquals("Date Limit", new ZDateTime(2023, 3, 1), moveHeaderData.DateCollection.Single(d => d.Type.Equals(DateType.DateLimit)).Value);
				AssertEquals("Date Presentation", new ZDateTimeOffset(2023, 12, 11).ToUtcDateTime(), moveHeaderData.DateCollection.Single(d => d.Type.Equals(DateType.Presentation)).Value);
				AssertEquals("AddInfoCollection - SecurityIndicator", NctsTypeOfSecurityList.Codes.ENT, moveHeaderData.AddInfoCollection.GetZStringValue(AddInfoConst.SecurityIndicator));
				AssertEquals("dataObject.PortOfOrigin.Code", "AU", moveHeaderData.PortOfOrigin.Code);
				AssertEquals("dataObject.PortOfOrigin.Name", "Australia", moveHeaderData.PortOfOrigin.Name);
				AssertEquals("dataObject.PortOfDestination.Code", "US", moveHeaderData.PortOfDestination.Code);
				AssertEquals("dataObject.PortOfDestination.Name", "United States", moveHeaderData.PortOfDestination.Name);
				AssertEquals("dataObject.PortOfLoading.Code", "AUSYD", moveHeaderData.PortOfLoading.Code);
				AssertEquals("dataObject.PortOfLoading.Name", "Sydney", moveHeaderData.PortOfLoading.Name);
				AssertEquals("dataObject.PortOfDischarge.Code", "USLAX", moveHeaderData.PortOfDischarge.Code);
				AssertEquals("dataObject.PortOfDischarge.Name", "Los Angeles", moveHeaderData.PortOfDischarge.Name);
				AssertEquals("AddInfoCollection - SpecificCircumstance", NctsSpecificCircumstanceIndicatorList.Codes.XXX, moveHeaderData.AddInfoCollection.GetZStringValue(AddInfoConst.SpecificCircumstance));
				var uniqueConsignmentReferenceDataObject = moveHeaderData.CustomsReferenceCollection.Single(c => c.Type.Code.Equals(NctsUxmlTypeList.Codes.UniqueConsignmentReference));
				AssertEquals("uniqueConsignmentReferenceDataObject.Type.Description", NctsUxmlTypeList.Descriptions.UniqueConsignmentReference, uniqueConsignmentReferenceDataObject.Type.Description);
				AssertEquals("uniqueConsignmentReferenceDataObject.Reference", "UCR213", uniqueConsignmentReferenceDataObject.Reference);
				var localReferenceNumberDataObject = moveHeaderData.CustomsReferenceCollection.Single(c => c.Type.Code.Equals(NctsUxmlTypeList.Codes.LocalReferenceNumber));
				AssertEquals("localReferenceNumberDataObject.Type.Description", NctsUxmlTypeList.Descriptions.LocalReferenceNumber, localReferenceNumberDataObject.Type.Description);
				AssertEquals("localReferenceNumberDataObject.Reference", "LRN1234", localReferenceNumberDataObject.Reference);
			});
			var representativeAddressType = nameof(DocAddressType.Representative);
			OrganizationAddressTestHelper.AssertOrganizationBO_WUFSHIJNB("Representative", GetOrganizationAddress(dataObject.OrganizationAddressCollection, representativeAddressType), representativeAddressType);
			var supplyChainActorDataObject = moveHeaderData.CustomsReferenceCollection.Single(c => c.Type.GetCodeAsUpperCase() == NctsUxmlTypeList.Codes.SupplyChainActor);
			AssertEquals("supplyChainActorDataObject.Type.Description", NctsUxmlTypeList.Descriptions.SupplyChainActor, supplyChainActorDataObject.Type.Description);
			AssertEquals("supplyChainActorDataObject.Reference", "REF12345", supplyChainActorDataObject.Reference);
			supplyChainActorDataObject.SubType.AssertEquals(new CodeDescriptionPair() { Code = SupplyChainActorRoleList.Codes.CS, Description = SupplyChainActorRoleList.Descriptions.CS });
			OrganizationAddressTestHelper.AssertOrganizationBO_INTHEMSYD("Owner", supplyChainActorDataObject.Owner, AddressTypes.Owner);

			var officeCodes = GetCustomsReferences(moveHeaderData.CustomsReferenceCollection, NctsUxmlTypeList.Codes.OfficeCode).ToArray();
			Assert("Should not include NCTSOfficeOfDestinationForArrival", officeCodes.All(p => !p.SubType.Code.Equals(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival)));
			AssertOffice(OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDeparture, officeCodes[0], "AA123456", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDeparture, ZDateTime.Empty, "AA123456 Office Description");
			AssertOffice(OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDestination, officeCodes[1], "NN123456", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDestination, new ZDateTime(2012, 10, 12, 6, 6, 0), "NN123456 Office Description");
		}

		protected override ZString MovementType => NctsMovementType.Codes.Departure;

		protected override NctsDepartureMovementHeaderDataObjectWriter GetNewWriter(IDataWritingManager manager) => new NctsDepartureMovementHeaderDataObjectWriter(manager);

		protected override NctsHeader GetNewHeader()
		{
			var header = base.GetNewHeader();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			return header;
		}

		static void SetupDepartureGoodsItemsForTest(BusinessObjectFactory factory, NctsDepartureCargoDesc goodsItem, bool addWarehouseData, short suffix)
		{
			var refCountryOfOrigin = factory.New<RefCountry>();
			refCountryOfOrigin.Code = "Z" + suffix;
			var refCountryState = refCountryOfOrigin.States.AddNew();
			refCountryState.RW_Code = "O" + suffix;
			refCountryState.RW_Description = "O DESC" + suffix;
			refCountryState.RW_RN_NKCountryCode = "AU";

			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;
			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "b", "IMO").First().PK;
			goodsItem.BY_LineNo = suffix;
			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
			goodsItem.BY_HarmonisedTariff = "12345678" + suffix;
			goodsItem.BY_CusC4Number = "0018137";
			goodsItem.BY_Description = "DESC" + suffix;
			goodsItem.BY_GrossWeight = 10;
			goodsItem.BY_GrossWeightUnit = Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = 1;
			goodsItem.BY_NetWeightUnit = Constants.Weight.Kilograms;
			goodsItem.BY_RN_NKCountryOfDispatch = "A" + suffix;
			goodsItem.BY_ZZF_NKTaxType = "RID";
			goodsItem.BY_RN_NKCountryOfOrigin = refCountryOfOrigin.Code;
			goodsItem.BY_RN_NKCountryOfDestination = "K" + suffix;
			goodsItem.BY_CommercialReferenceNumber = "C" + suffix;
			goodsItem.BY_CustomsQuantity = 1.0;
			goodsItem.BY_CustomsUnitQty = "KG";
			goodsItem.BY_CustomsSecondQuantity = 2.0;
			goodsItem.BY_CustomsSecondUnitQty = "UNT";
			goodsItem.BY_CustomsThirdQuantity = 3.0;
			goodsItem.BY_CustomsThirdUnitQty = "K3";
			goodsItem.BY_CustomsFourthQuantity = 4.0;
			goodsItem.BY_CustomsFourthUnitQty = "DTNG";
			goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			goodsItem.BY_OP_Part = OrgSupplierPart.New(factory).PK;
			goodsItem.Part.OP_PartNum = "P" + suffix;
			goodsItem.BY_LineNo = suffix;
			goodsItem.BY_LinePrice = 23.45m;
			goodsItem.BY_RX_NKLinePriceCurrency = "AUD";
			goodsItem.BY_MonetaryValue = 2.0;
			if (addWarehouseData)
			{
				goodsItem.BY_BondedWhsQuantity = 15;
				goodsItem.BY_BondedWhsUnitQty = "UNT";
				goodsItem.BY_WarehouseEntryNumber = "WEN" + suffix;
				goodsItem.BY_WarehouseEntryLineNo = 1;
			}
			var addInfo1 = goodsItem.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "REF";
			addInfo1.CSI_Code = "C658";
			addInfo1.CSI_ReferenceNumber = "ADDINFO1 REF" + suffix;
			addInfo1.CSI_Description = "DESCRIPTION";

			var suppDoc1 = goodsItem.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "C673";
			suppDoc1.CSI_ReferenceNumber = "SUPDOC1 REF" + suffix;
			suppDoc1.CSI_ItemNumber = 2;
			suppDoc1.CSI_ReferenceNumber2 = "INFORMATION";

			var prevDoc1 = goodsItem.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "N821";
			prevDoc1.CSI_ReferenceNumber = "PREVDOC1 REF" + suffix;
			prevDoc1.CSI_ItemNumber = 1;
			prevDoc1.CSI_PackQty = 5;
			prevDoc1.CSI_PackType = "PL";
			prevDoc1.CSI_Quantity = 2;
			prevDoc1.CSI_UnitOfQuantity = "BX";
			prevDoc1.CSI_ReferenceNumber2 = "COMPLEMENT OF INFORMATION";

			NCTSTestHelper.CreateJobDocAddressForTest(factory, "CE2", goodsItem.Consignee, suffix.ToString());

			var supCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supCode.CY_Code = "ABCD";
			supCode.CY_Data = "SupplementaryCode" + suffix;

			var supplyChainActor1 = goodsItem.CusSupplyChainActorReferences.AddNew();
			supplyChainActor1.CFR_Code = "FW";
			supplyChainActor1.CFR_Reference = "SUPPLYCHAINACTOR" + suffix;
			var org = NCTSTestHelper.CreateOrgAddressForTest(factory, "AAA" + suffix);
			org.Header.OH_Code = "AAA " + suffix;
			org.OA_Code = "AAA " + suffix;
			org.Header.OH_IsWarehouseClient = true;
			org.OA_RN_NKCountryCode = "FR";
			supplyChainActor1.CFR_OA_Owner = org.PK;
		}

		static void AssertDepartureGoodsItems(InBondMoveLineItem goodsItemXmlData, ZInt suffix)
		{
			AssertEquals("Item number", suffix, goodsItemXmlData.LineNumber);
			AssertEquals("Commodity code (TariffCode)", "1234.56.78 " + suffix, goodsItemXmlData.TariffCode);
			AssertEquals("Goods description", "DESC" + suffix, goodsItemXmlData.DescriptionAndQuantityOfMerchandise);
			AssertEquals("Gross mass", 10m, goodsItemXmlData.Weight);
			AssertEquals("Gross mass units", "KG", goodsItemXmlData.WeightUnit.Code);
			AssertEquals("Net mass", 1m, goodsItemXmlData.NetWeight);
			AssertEquals("Net mass units", "KG", goodsItemXmlData.NetWeightUnit.Code);
			AssertEquals("Country of Origin code", "Z" + suffix, goodsItemXmlData.CountryOfOrigin.Code);
			AssertEquals("Country of dispatch/export code", "A" + suffix, goodsItemXmlData.CountryOfDispatch.Code);
			AssertEquals("Country of Destination code", "K" + suffix, goodsItemXmlData.CountryOfDestination.Code);
			AssertEquals("Commercial ReferenceNumber", "C" + suffix, goodsItemXmlData.ReferenceNumber);
			AssertEquals("Customs Value", 2.0m, goodsItemXmlData.MonetaryValue);
			AssertEquals("Customs Quantity", 1.0m, goodsItemXmlData.CustomsFirstQuantity);
			AssertEquals("Customs Quantity Unit Code", "KG", goodsItemXmlData.CustomsFirstQuantityUnit.Code);
			AssertEquals("Customs Quantity Unit Description", "Kilograms", goodsItemXmlData.CustomsFirstQuantityUnit.Description);
			AssertEquals("Customs Second Quantity", 2.0m, goodsItemXmlData.CustomsSecondQuantity);
			AssertEquals("Customs Second Quantity Unit", "UNT", goodsItemXmlData.CustomsSecondQuantityUnit.Code);
			AssertEquals("Customs Third Quantity", 3.0m, goodsItemXmlData.CustomsThirdQuantity);
			AssertEquals("Customs Third Quantity Unit", "K3", goodsItemXmlData.CustomsThirdQuantityUnit.Code);
			AssertEquals("Customs Fourth Quantity", 4.0m, goodsItemXmlData.CustomsFourthQuantity);
			AssertEquals("Customs Fourth Quantity Unit", "DTNG", goodsItemXmlData.CustomsFourthQuantityUnit.Code);
			AssertEquals("TaxType", "RID", goodsItemXmlData.TaxType.Code);
			AssertEquals("TaxType Description", "RID DESC", goodsItemXmlData.TaxType.Description);
			AssertEquals("Declaration Type", NctsPhase5DeclarationTypeList.Codes.T2, goodsItemXmlData.DeclarationType.Code);
			AssertEquals("Transport Charges Method Of Payment", TransportChargesModeOfPayment.Codes.CreditCard, goodsItemXmlData.TransportPaymentMethod.Code);
			AssertEquals("Line Price", 23.45m, goodsItemXmlData.LinePrice);
			AssertEquals("Line Price Currency", "AUD", goodsItemXmlData.LinePriceCurrency.Code);
			AssertNotNull(goodsItemXmlData.OrganizationAddressCollection);
			var docAddress = goodsItemXmlData.OrganizationAddressCollection.Find(x => (string)x.AddressType == nameof(DocAddressType.ConsigneeAddress));
			AssertEquals("Oscorp Industries" + suffix, docAddress?.CompanyName);
			AssertNotNull(goodsItemXmlData.CustomsSupportingInformationCollection);
			Assert("Customs Supporting Collection Has Reference", goodsItemXmlData.CustomsSupportingInformationCollection.Exists(x => (string)x.ReferenceNumber == "ADDINFO1 REF" + suffix));
			Assert("Customs Supporting Collection Has Reference", goodsItemXmlData.CustomsSupportingInformationCollection.Exists(x => (string)x.ReferenceNumber == "SUPDOC1 REF" + suffix));
			Assert("Customs Supporting Collection Has Reference", goodsItemXmlData.CustomsSupportingInformationCollection.Exists(x => (string)x.ReferenceNumber == "PREVDOC1 REF" + suffix));
			AssertNotNull(goodsItemXmlData.CustomsReferenceCollection);
			Assert("Customs Reference Collection Has Reference", goodsItemXmlData.CustomsReferenceCollection.Exists(x => (string)x.Reference == "SupplementaryCode" + suffix));
			Assert("Customs Reference Collection Has Reference", goodsItemXmlData.CustomsReferenceCollection.Exists(x => (string)x.Reference == "SUPPLYCHAINACTOR" + suffix));
			AssertNotNull(goodsItemXmlData.HazardousMaterial);
			AssertEquals("HazardousMaterial>Code", "0018137", goodsItemXmlData.HazardousMaterial.Code);
			AssertEquals("HazardousMaterial>CodeType>Code", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, goodsItemXmlData.HazardousMaterial.CodeType.Code);
			AssertEquals("UNDGCollection Count", 2, goodsItemXmlData.HazardousMaterial.UNDGCollection.Count);
			Assert("UNDGCollection Has Reference", goodsItemXmlData.HazardousMaterial.UNDGCollection.Exists(x => (string)x.UNDGCode == "0004a"));
			Assert("UNDGCollection Has Reference", goodsItemXmlData.HazardousMaterial.UNDGCollection.Exists(x => (string)x.UNDGCode == "0004b"));
		}

		static void AssertDepartureGoodsItems(CommercialInvoiceLine goodsItemXmlData, ZInt suffix, bool shouldHaveWarehouseData)
		{
			AssertEquals("Item number", suffix, goodsItemXmlData.LineNo);
			AssertEquals("Commodity code (taric code)", "12345678" + suffix, goodsItemXmlData.HarmonisedCode);
			AssertEquals("Goods description", "DESC" + suffix, goodsItemXmlData.Description);
			AssertEquals("Gross mass", 10m, goodsItemXmlData.Weight);
			AssertEquals("Gross mass units", "KG", goodsItemXmlData.WeightUnit.Code);
			AssertEquals("Net mass", 1m, goodsItemXmlData.NetWeight);
			AssertEquals("Net mass units", "KG", goodsItemXmlData.NetWeightUnit.Code);
			AssertEquals("Country of Origin code", "Z" + suffix, goodsItemXmlData.CountryOfOrigin.Code);
			AssertEquals("Country of dispatch/export code", "A" + suffix, goodsItemXmlData.CountryOfExport.Code);
			AssertEquals("Customs Second Quantity", 2.0m, goodsItemXmlData.CustomsSecondQuantity);
			AssertEquals("Customs Second Quantity Unit", "UNT", goodsItemXmlData.CustomsSecondQuantityUnit.Code);
			AssertEquals("Customs Value", 2.0m, goodsItemXmlData.CustomsValue);

			if (shouldHaveWarehouseData)
			{
				AssertEquals("BondedWarehouseQuantity", 15m, goodsItemXmlData.BondedWarehouseQuantity);
				AssertEquals("BondedWarehouseQuantityUnit", "UNT", goodsItemXmlData.BondedWarehouseQuantityUnit.Code);
				AssertEquals("PartNo", "P" + suffix, goodsItemXmlData.PartNo);
				AssertEquals("PreviousEntryNumber", "WEN" + suffix, goodsItemXmlData.PreviousEntryNumber);
				AssertEquals("PreviousEntryLineNumber", (ZShort)1, goodsItemXmlData.PreviousEntryLineNumber);
				AssertEquals("EntryLineNumber", (ZShort)1, goodsItemXmlData.EntryLineNumber);
				AssertEquals("EntryNumber", "C" + suffix, goodsItemXmlData.EntryNumber);
			}
			else
			{
				AssertEquals("BondedWarehouseQuantity", null, goodsItemXmlData.BondedWarehouseQuantity);
				AssertEquals("BondedWarehouseQuantityUnit", null, goodsItemXmlData.BondedWarehouseQuantityUnit);
				AssertEquals("PartNo", null, goodsItemXmlData.PartNo);
				AssertEquals("PreviousEntryNumber", null, goodsItemXmlData.PreviousEntryNumber);
				AssertEquals("PreviousEntryLineNumber", null, goodsItemXmlData.PreviousEntryLineNumber);
				AssertEquals("EntryLineNumber", null, goodsItemXmlData.EntryLineNumber);
				AssertEquals("EntryNumber", null, goodsItemXmlData.EntryNumber);
			}
		}
	}
}
