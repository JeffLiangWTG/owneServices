using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using NUnit.Framework;
using CusCodeDataTypeList = Enterprise.Customs.EU.Business.CusCodeDataTypeList;
using UniversalDataObjects = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	public sealed class NctsHeaderDataObjectWriterTest : NctsHeaderCommonDataObjectWriterTest<NctsHeaderDataObjectWriter>
	{
		[TestDate(1998, 10, 31)]
		public void TestNctsHeaderDataObjectWriter_Departure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.Latvia, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DTNE", "Hectokilogram net of drained weight", ZDateTime.Today, ZDateTime.Today.AddYears(1));

			helper.CreateRefCusTaxOrFeeType("VAT");
			var othFee = helper.CreateTaxOrFee("ABCD", 5, Constants.CountryCodes.Latvia, description: "ABCD Description");
			othFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			Factory.Save();

			var nctsHeaderBO = Factory.New<NctsHeader>();
			nctsHeaderBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderBO.SetMovementType(NctsMovementType.Codes.Departure);

			SetupNctsHeaderForDeparture(Factory, nctsHeaderBO);
			SetupNctsHeaderForCancellation(nctsHeaderBO);
			Factory.Save();

			var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeaderBO)));
			var xmlShipmentData = writer.GetDataObject(nctsHeaderBO);

			AssertDepartureXmlDataGenerated(xmlShipmentData);
			AssertCancellationXmlDataGenerated(xmlShipmentData);
		}

		public void TestNctsHeaderDataObjectWriter_Arrival()
		{
			var nctsHeaderBO = Factory.New<NctsHeader>();
			nctsHeaderBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderBO.SetMovementType(NctsMovementType.Codes.Arrival);

			SetupNctsHeaderForArrival(nctsHeaderBO);
			SetupNctsHeaderForUnloading(nctsHeaderBO);
			Factory.Save();

			var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeaderBO)));
			var xmlShipmentData = writer.GetDataObject(nctsHeaderBO);

			AssertArrivalXmlDataGenerated(xmlShipmentData);
			AssertUnloadingXmlDataGenerated(xmlShipmentData);
		}

		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, header),
				writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))));
			var shipment = writer.GetDataObject(header);
			var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

			writer = writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, header)));
			shipment = writer.GetDataObject(header);
			commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
		}

		public void TestPopulateNoteCollection()
		{
			var nctsHeaderBO = Factory.New<NctsHeader>();
			nctsHeaderBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			var note1 = nctsHeaderBO.Notes.AddNew();
			note1.ST_Description = "Description 1";
			note1.ST_NoteDataAsText = "Line 1";

			var note2 = nctsHeaderBO.Notes.AddNew();
			note2.ST_Description = "Description 2";
			note2.ST_NoteDataAsText = "Line 2";

			var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeaderBO)));
			var xmlShipmentData = writer.GetDataObject(nctsHeaderBO);

			CombineAssertions(() =>
			{
				var noteCollection = xmlShipmentData.NoteCollection;
				AssertNotNull("Should Populate Note Collection", noteCollection);
				AssertEquals("First note description", "Description 1", noteCollection[0].Description);
				AssertEquals("First note text", "Line 1", noteCollection[0].NoteText);
				AssertEquals("Second note description", "Description 2", noteCollection[1].Description);
				AssertEquals("Second note text", "Line 2", noteCollection[1].NoteText);
			});
		}

		static void AssertDepartureXmlDataGenerated(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertNotNull(xmlShipmentData);
			AssertMessagingApplicationCode(xmlShipmentData);
			AssertEquals("Departure", 1, xmlShipmentData.CommercialInfo.CommercialInvoiceCollection.Count);

			var departureXmlData = xmlShipmentData.CommercialInfo.CommercialInvoiceCollection.Single(x => x.RelatedIndicator.Code.ToString() == NctsMovementType.Codes.Departure);

			AssertEquals("Document/reference number (MRN)", "MRN123", GetEntryNumberCollectionValue(xmlShipmentData.EntryNumberCollection, CusEntryNumberTypes.Standard.MovementReferenceNumber));
			AssertEquals("Reference number (LRN)", "NCT00000001", xmlShipmentData.OwnerRef);
			AssertEquals("Type of declaration", "T-", xmlShipmentData.MessageType.Code);
			AssertEquals("Country of destination Code", "RU", xmlShipmentData.PortOfDestination.Code);
			AssertEquals("Agreed location of goods code / Authorised location of goods code / Agreed location of goods / Customs sub place", "PRE-LODGED", xmlShipmentData.LocationAtClearance.Code);
			AssertEquals("Place of loading code", "CNKWE", xmlShipmentData.PortOfLoading.Code);
			AssertEquals("Country of dispatch/export code", "SG", xmlShipmentData.PortOfOrigin.Code);

			AssertEquals("Inland transport mode", "1", xmlShipmentData.TransportMode.Code);
			AssertEquals("Tranport mode at border", "2", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.TransportModeAtBorder));
			AssertEquals("Identity of means of transport at departure (exp/trans)", "REG DEP1", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportID));
			AssertEquals("Nationality of means of transport at departure", "NZ", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportNationality));
			AssertEquals("Identity of means of transport crossing border", "REG DEP2", xmlShipmentData.VesselName);
			AssertEquals("Nationality of means of transport crossing border", "JP", xmlShipmentData.VesselCountryOfRegistration.Code);
			//AssertEquals("Type of means of transport crossing border", "", departureSubShipmentXml.???); //TODO Missing from BO and GUI - fix it

			AssertEquals("Containerised indicator", "Y", GetContainerisedValue(xmlShipmentData, "D"));
			AssertEquals("Total number of items", 2, departureXmlData.CommercialInvoiceLineCollection.Count);
			AssertEquals("Total number of packages", 60, xmlShipmentData.TotalNoOfPacks);
			AssertEquals("Total gross mass", 20m, xmlShipmentData.TotalWeight);
			AssertEquals("Declaration date", ZDate.BrettsBirthday.ToShortDateString(), GetDateCollectionValue(xmlShipmentData.DateCollection, UniversalDataObjects.DateType.Departure).Substring(0, 9));
			AssertEquals("Declaration place", "Brisbane", xmlShipmentData.CustomsOffice.Code);
			AssertEquals("Specific Circumstance Indicator", "E", xmlShipmentData.DeliveryMode.Code);
			AssertEquals("Transport charges/ Method of Payment", "Y", xmlShipmentData.PaymentMethod.Code);
			AssertEquals("Commercial Reference Number", "COMM-REF1", GetCustomsReference(xmlShipmentData.CustomsReferenceCollection, DataObjectWriterConstants.Header.AddInfo.CustomReferenceType).Reference);

			AssertEquals("Security", "Y", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.SecurityIndicator));
			AssertEquals("Conveyance reference number", "CONV-REF1", xmlShipmentData.VoyageFlightNo);
			AssertEquals("Place of unloading code", "UAODS", xmlShipmentData.PortOfDischarge.Code);

			AssertOrganizationAddress("(PRINCIPAL) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "Principal"), "1", tir: "GBR/022/1234567");
			AssertOrganizationAddress("(CONSIGNOR)TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "ConsignorDocumentaryAddress"), "2");
			AssertOrganizationAddress("(CONSIGNEE) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "ConsigneeAddress"), "3");
			//AssertEquals("AUTHORISED CONSIGNEE) TRADER EORI-TIN (Turn)", "", xmlShipmentData.); // TODO Used to signify simplified arrival at consignee - not implemented - fix it

			AssertOffice("DEPARTURE) CUSTOMS OFFICE Reference number", xmlShipmentData.CustomsReferenceCollection, "AA123456", "DEP", ZDateTime.Empty);
			AssertOffice("(TRANSIT) CUSTOMS OFFICE 1", xmlShipmentData.CustomsReferenceCollection, "NN123456", "TRA", new ZDateTime(2012, 10, 12, 6, 6, 0));
			AssertOffice("(TRANSIT) CUSTOMS OFFICE 2", xmlShipmentData.CustomsReferenceCollection, "TT123456", "TRA", new ZDateTime(2012, 10, 13, 7, 7, 0));
			AssertOffice("(DESTINATION) CUSTOMS OFFICE Reference number", xmlShipmentData.CustomsReferenceCollection, "ZZ123456", "DES", ZDateTime.Empty);

			AssertEquals("CONTROL RESULT Control result code", "A3", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultCode));
			AssertEquals("CONTROL RESULT Date limit", "19981107", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultDateLimit));
			AssertEquals("REPRESENTATIVE Name", "NWG", xmlShipmentData.CustomsBroker.Code);
			//AssertEquals("REPRESENTATIVE Representative capacity", "", xmlShipmentData.); // TODO not captured or generated in message - fix it
			AssertHeaderContainersAndSeals(xmlShipmentData);

			AssertGuarantee("GUARANTEE 1", xmlShipmentData.GuaranteeCollection, "9", "12346789", "AAAAAAAAAA", "ABCD", "FR");
			AssertGuarantee("GUARANTEE 2", xmlShipmentData.GuaranteeCollection, "1", "987654321", "BBBBB", "WXYZ", "BE");

			AssertDepartureGoodsItems(xmlShipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0], 1);
			AssertDepartureGoodsItems(xmlShipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1], 2);

			AssertItinerary("ITINERARY 1", xmlShipmentData.TransportLegCollection, "1", "DE");
			AssertItinerary("ITINERARY 2", xmlShipmentData.TransportLegCollection, "2", "CH");

			AssertOrganizationAddress("(CONSIGNOR-SECURITY) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "NotifyParty2"), "4");
			AssertOrganizationAddress("(CONSIGNEE-SECURITY) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "NotifyParty3"), "5");
			AssertCarrier("(CARRIER) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "Carrier"));
			AssertOrganizationAddress("(REPRESENTATIVE) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "Representative"), "6");
			AssertOrganizationAddress("(WAREHOUSE2) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "Warehouse2"), "7");

			AssertPackageLinkExists("Departure Item 1 package count", xmlShipmentData.PackingLineCollection, "D", 1, 2);
			AssertPackageLinkExists("Departure Item 2 package count", xmlShipmentData.PackingLineCollection, "D", 2, 2);

			AssertEquals("CustomsProfileIdentifier Type", "UserName", xmlShipmentData.CustomsProfileIdentifier.Type);
			AssertEquals("CustomsProfileIdentifier Value", "999A", xmlShipmentData.CustomsProfileIdentifier.Value);

			AssertSealInfo(xmlShipmentData.SealInfo);
		}

		static void AssertSealInfo(UniversalDataObjects.SealInfo sealInfo)
		{
			AssertNotNull("SealInfo", sealInfo);
			AssertNotNull("SealInfo.Type", sealInfo.Type);
			AssertEquals("SealInfo.Type.Code", "CON", sealInfo.Type.Code);
			AssertEquals("SealInfo.Type.Description", "Container Seal", sealInfo.Type.Description);
			AssertEquals("SealInfo.Quantity", 2, sealInfo.Quantity);
		}

		static void AssertPackageLinkExists(string message, IEnumerable<UniversalDataObjects.PackingLine> packingLineCollectionData, string packageParentType, ZShort? itemNo, int packageCountExpected)
		{
			var goodsItemPackagesCount = packingLineCollectionData
				.Count(o => (string)o.EntryType == packageParentType && string.IsNullOrEmpty(o.ContainerNumber) && o.ItemNo == itemNo);
			AssertEquals(message, packageCountExpected, goodsItemPackagesCount);
		}

		static ZString GetContainerisedValue(UniversalDataObjects.Shipment departureXmlData, ZString movementType)
		{
			var containersSelectedCount = departureXmlData.PackingLineCollection
				.Count(n => !string.IsNullOrEmpty(n.ContainerNumber) && n.EntryType.GetValueOrDefault() == movementType);
			return containersSelectedCount > 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No;
		}

		static List<ZString> GetSealsUsed(UniversalDataObjects.Shipment departureXmlData)
		{
			var sealsUsed = new List<ZString>();

			foreach (var containerData in departureXmlData.PackingLineCollection
				.Where(n => !string.IsNullOrEmpty(n.ContainerNumber) && n.EntryType.GetValueOrDefault() == "D")
				.Select(c => departureXmlData.ContainerCollection.FirstOrDefault(n => n.ContainerNumber == c.ContainerNumber))
				.Where(containerData => containerData != null))
			{
				AddSealToList(containerData.Seal.GetValueOrDefault(), sealsUsed);
				AddSealToList(containerData.SecondSeal.GetValueOrDefault(), sealsUsed);
			}
			return sealsUsed;
		}

		static void AddSealToList(ZString seal, List<ZString> list)
		{
			if (!seal.IsEmpty)
			{
				list.Add(seal);
			}
		}

		static void AssertOffice(string message, IEnumerable<CustomsReference> customsReferenceCollection, string officeCode, string officePurpose, ZDateTime submittedDateToCustoms)
		{
			var customsReferences = customsReferenceCollection.Where(o => o.Type.Code.ToString() == CusCodeDataTypeList.Codes.OfficeCode);
			if (submittedDateToCustoms.IsEmpty)
			{
				var addInfosExist = customsReferences.Any(r => r.Reference.GetValueOrDefault() == officeCode && r.SubType.Code.GetValueOrDefault() == officePurpose);
				AssertEquals(message, true, addInfosExist);
			}
			else
			{
				var addInfosExist = customsReferences
					.Any(r => r.Reference.GetValueOrDefault() == officeCode
									&& r.SubType.Code.GetValueOrDefault() == officePurpose
									&& (r.DateCollection?.FirstOrDefault(x => x.Type.GetValueOrDefault() == UniversalDataObjects.DateType.DateAtOffice)?.Value.GetValueOrDefault() ?? ZDateTime.Empty) == submittedDateToCustoms);
				AssertEquals(message, true, addInfosExist);
			}
		}

		static void AssertGuarantee(string message, IEnumerable<Guarantee> guaranteeCollectionData, string type, string refNo, string otherRefNo, string accessCode, string validNonEC)
		{
			var guaranteesExist = guaranteeCollectionData.Any(x => (x.BondType?.GetNullableCodeAsUpperCase() ?? ZString.Empty) == type
																&& x.BondNumber.GetValueOrDefault() == refNo
																&& x.AccessCode.GetValueOrDefault() == accessCode
																&& x.BondNumber2.GetValueOrDefault() == otherRefNo
																&& x.ValidityLimitation.GetValueOrDefault() == validNonEC);
			AssertEquals(message, true, guaranteesExist);
		}

		static void AssertItinerary(string message, IEnumerable<UniversalDataObjects.TransportLeg> transportLegCollectionData, string seq, ZString country)
		{
			var transportLeg = transportLegCollectionData.FirstOrDefault(o => o.DepartureReference.ToString() == country && o.LegOrder.ToString() == seq);
			AssertNotNull(message, transportLeg);
		}

		static string GetDateCollectionValue(IEnumerable<UniversalDataObjects.Date> dateCollection, UniversalDataObjects.DateType dateType)
		{
			return dateCollection.FirstOrDefault(d => d.Type == dateType)?.Value.ToString() ?? string.Empty;
		}

		static UniversalDataObjects.OrganizationAddress GetOrganizationAddress(IEnumerable<UniversalDataObjects.OrganizationAddress> organizationAddressCollection, string addressType)
		{
			return organizationAddressCollection.FirstOrDefault(oa => oa.AddressType.ToString() == addressType);
		}

		static string GetAddInfoCollectionValue(IEnumerable<UniversalDataObjects.AddInfo> addInfoCollection, string key)
		{
			return addInfoCollection.FirstOrDefault(ai => ai.Key.GetValueOrDefault() == key)?.Value.ToString() ?? string.Empty;
		}

		static void AssertCarrier(ZString traderType, UniversalDataObjects.OrganizationAddress xmlOrganizationAddressData)
		{
			AssertEquals(traderType + " Name", "CARRIER1", xmlOrganizationAddressData.CompanyName);
			AssertEquals(traderType + " Street and number", "CARRIER STREET", xmlOrganizationAddressData.Address1);
			AssertEquals(traderType + " Postal code", "CMK1", xmlOrganizationAddressData.Postcode);
			AssertEquals(traderType + " City", "CITY", xmlOrganizationAddressData.City);
			AssertEquals(traderType + " Country code", "BR", xmlOrganizationAddressData.Country.Code);
			AssertEquals(traderType + " EORI-TIN (Turn)", "GB954131533000", GetRegistrationNumberCollectionValue(xmlOrganizationAddressData.RegistrationNumberCollection, "EOR"));
			AssertEquals(traderType + " Holder ID TIR", "GBR/ABC1234", GetRegistrationNumberCollectionValue(xmlOrganizationAddressData.RegistrationNumberCollection, "TIR"));
		}

		static string GetEntryNumberCollectionValue(IEnumerable<UniversalDataObjects.EntryNumber> entryNumberCollection, ZString? entryType)
		{
			return entryNumberCollection.FirstOrDefault(e => e.Type.Code == entryType)?.Number ?? ZString.Empty;
		}

		static string GetRegistrationNumberCollectionValue(IEnumerable<UniversalDataObjects.RegistrationNumber> registrationNumberCollection, string code)
		{
			return registrationNumberCollection.FirstOrDefault(r => r.Type.Code.ToString() == code)?.Value.ToString() ?? string.Empty;
		}

		static void AssertDepartureGoodsItems(CommercialInvoiceLine goodsItemXmlData, ZInt itemNo)
		{
			AssertEquals("GOODS ITEM Item number", itemNo, goodsItemXmlData.LineNo);
			AssertEquals("GOODS ITEM Commodity code (taric code)", "12345678" + itemNo, goodsItemXmlData.HarmonisedCode);
			AssertEquals("GOODS ITEM Procedure", "PRO" + itemNo, goodsItemXmlData.Procedure);
			AssertEquals("GOODS ITEM Type of Declaration", "T" + itemNo, GetAddInfoCollectionValue(goodsItemXmlData.AddInfoCollection, DataObjectWriterConstants.GoodsItem.AddInfo.DeclarationType));
			AssertEquals("GOODS ITEM Goods description", "DESC" + itemNo, goodsItemXmlData.Description);
			AssertEquals("GOODS ITEM Gross mass", 10m, goodsItemXmlData.Weight);
			AssertEquals("GOODS ITEM Gross mass units", "KG", goodsItemXmlData.WeightUnit.Code);
			AssertEquals("GOODS ITEM Net mass", 1m, goodsItemXmlData.NetWeight);
			AssertEquals("GOODS ITEM Net mass units", "KG", goodsItemXmlData.NetWeightUnit.Code);
			AssertEquals("GOODS ITEM Country of Origin code", "Z" + itemNo, goodsItemXmlData.CountryOfOrigin.Code);
			AssertEquals("GOODS ITEM State of Origin Code", "O" + itemNo, goodsItemXmlData.StateOfOrigin.Code);
			AssertEquals("GOODS ITEM State of Origin Name", "O DESC" + itemNo, goodsItemXmlData.StateOfOrigin.Name);
			AssertEquals("GOODS ITEM Country of dispatch/export code", "A" + itemNo, goodsItemXmlData.CountryOfExport.Code);
			AssertEquals("GOODS ITEM Country of destination code", "K" + itemNo, GetAddInfoCollectionValue(goodsItemXmlData.AddInfoCollection, DataObjectWriterConstants.GoodsItem.AddInfo.CountryOfDestination));
			AssertEquals("GOODS ITEM Transport charges/ Method of Payment", "A", GetAddInfoCollectionValue(goodsItemXmlData.AddInfoCollection, DataObjectWriterConstants.GoodsItem.AddInfo.TransportChargesMoP));
			AssertEquals("GOODS ITEM Commercial Reference Number", "C" + itemNo, GetAddInfoCollectionValue(goodsItemXmlData.AddInfoCollection, DataObjectWriterConstants.GoodsItem.AddInfo.CommercialReferenceNumber));
			AssertEquals("GOODS ITEM UN dangerous goods code", "U" + itemNo, GetAddInfoCollectionValue(goodsItemXmlData.AddInfoCollection, DataObjectWriterConstants.GoodsItem.AddInfo.UNDangerousGoodsCode));
			AssertEquals("GOODS ITEM Customs Second Quantity", 1.0m, goodsItemXmlData.CustomsSecondQuantity);
			AssertEquals("GOODS ITEM Customs Second Quantity Unit", "DTNE", goodsItemXmlData.CustomsSecondQuantityUnit.Code);
			AssertEquals("GOODS ITEM Customs Second Quantity Unit description", "Hectokilogram net of drained weight", goodsItemXmlData.CustomsSecondQuantityUnit.Description);
			AssertEquals("GOODS ITEM Customs Value", 2.0m, goodsItemXmlData.CustomsValue);
			AssertEquals("GOODS ITEM Tax Type", "ABCD", goodsItemXmlData.TaxType.Code);
			AssertEquals("GOODS ITEM Tax Type Description", "ABCD Description", goodsItemXmlData.TaxType.Description);

			AssertPreviousAdminReference(goodsItemXmlData.CustomsSupportingInformationCollection, "T1", "P", "PD1");
			AssertPreviousAdminReference(goodsItemXmlData.CustomsSupportingInformationCollection, "T2", "P", "PD2");
			AssertSupportingDocument(goodsItemXmlData.CustomsSupportingInformationCollection, "380", "REF1", "SD1");
			AssertSupportingDocument(goodsItemXmlData.CustomsSupportingInformationCollection, "18", "REF2", "SD2");
			AssertAdditionalInformation(goodsItemXmlData.CustomsSupportingInformationCollection, "00100", "AI1", "AD", "Y");
			AssertAdditionalInformation(goodsItemXmlData.CustomsSupportingInformationCollection, "00200", "AI2", "IS", "");
			AssertSupplementaryCode(goodsItemXmlData.CustomsReferenceCollection, "A" + itemNo);

			AssertOrganizationAddress("GOODS ITEM (CONSIGNOR) TRADER.", GetOrganizationAddress(goodsItemXmlData.OrganizationAddressCollection, "ConsignorDocumentaryAddress"), "6");
			AssertOrganizationAddress("GOODS ITEM (CONSIGNEE) TRADER.", GetOrganizationAddress(goodsItemXmlData.OrganizationAddressCollection, "ConsigneeAddress"), "7");
			AssertOrganizationAddress("GOODS ITEM (CONSIGNOR-SECURITY) TRADER", GetOrganizationAddress(goodsItemXmlData.OrganizationAddressCollection, "NotifyParty2"), "8");
			AssertOrganizationAddress("GOODS ITEM (CONSIGNEE-SECURITY) TRADER", GetOrganizationAddress(goodsItemXmlData.OrganizationAddressCollection, "NotifyParty3"), "9");
		}

		static void AssertPreviousAdminReference(List<CustomsSupportingInformation> customsSupportingInformationData, string pdType, string pdClass, string pdRef)
		{
			var pdExists = PreviousAdminReferenceExists(customsSupportingInformationData, pdType, pdClass, pdRef);
			AssertEquals("PREV ADMIN REF " + pdRef + " missing", true, pdExists);
		}

		static bool PreviousAdminReferenceExists(List<CustomsSupportingInformation> customsSupportingInformationData, string pdType, string pdClass, string pdRef)
		{
			return customsSupportingInformationData
				.Any(csi => csi.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.PreviousDocument
							&& csi.Type.Code.GetValueOrDefault() == pdType
							&& csi.SubType.Code.GetValueOrDefault() == pdClass
							&& csi.ReferenceNumber.GetValueOrDefault() == pdRef);
		}

		static void AssertAdditionalInformation(List<CustomsSupportingInformation> customsSupportingInformationData, string aiCoded, string aiInformation, string aiExportFromCountry, string aiExportFromEC)
		{
			var aiExists = AdditionalInformationExists(customsSupportingInformationData, aiCoded, aiInformation, aiExportFromCountry, aiExportFromEC);
			AssertEquals("SPECIAL MENTIONS " + aiCoded + " missing", true, aiExists);
		}

		static bool AdditionalInformationExists(List<CustomsSupportingInformation> customsSupportingInformationData, string aiCoded, string aiInformation, string aiExportFromCountry, string aiExportFromEC)
		{
			return customsSupportingInformationData
				.Any(csi => csi.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.AdditionalInfo
							&& csi.Type.Code.GetValueOrDefault() == aiCoded
							&& csi.Description.GetValueOrDefault() == aiInformation
							&& csi.Country.Code.GetValueOrDefault() == aiExportFromCountry
							&& csi.SubType.Code.GetValueOrDefault() == aiExportFromEC);
		}

		static void AssertSupportingDocument(List<CustomsSupportingInformation> customsSupportingInformationData, string sdType, string sdReason, string sdRef)
		{
			var sdExists = SupportingDocumentExists(customsSupportingInformationData, sdType, sdReason, sdRef);
			AssertEquals("PRODUCED DOC/CERT " + sdRef + " missing", true, sdExists);
		}

		static bool SupportingDocumentExists(List<CustomsSupportingInformation> customsSupportingInformationData, string sdType, string sdReason, string sdRef)
		{
			return customsSupportingInformationData
				.Any(csi => csi.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.SupportingDocument
							&& csi.Type.Code.GetValueOrDefault() == sdType
							&& csi.Description.GetValueOrDefault() == sdReason
							&& csi.ReferenceNumber.GetValueOrDefault() == sdRef);
		}

		static void AssertHeaderContainersAndSeals(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertEquals("SEALS INFO Seals Number", 3, GetSealsUsed(xmlShipmentData).Count);
			AssertEquals("CONTAINERS.Container number", "CONTAINER1", xmlShipmentData.ContainerCollection[0].ContainerNumber);
			AssertEquals("SEALS ID1.Seals identity 1", "SEAL1", xmlShipmentData.ContainerCollection[0].Seal);
			AssertEquals("SEALS ID1.Seals identity 2", "SEAL2", xmlShipmentData.ContainerCollection[0].SecondSeal);
			AssertEquals("CONTAINERS.Container number", "CONTAINER2", xmlShipmentData.ContainerCollection[1].ContainerNumber);
			AssertEquals("SEALS ID2.Seals identity 1", "SEAL3", xmlShipmentData.ContainerCollection[1].Seal);
			AssertEquals("SEALS ID2.Seals identity 2", "", xmlShipmentData.ContainerCollection[1].SecondSeal);
			AssertEquals("CONTAINERS.Container number", "CONTAINER3", xmlShipmentData.ContainerCollection[2].ContainerNumber);
		}

		static void AssertSupplementaryCode(List<CustomsReference> customsReferenceCollection, string expectedSupplementaryCode)
		{
			var universalShipmentSupplementaryCodes = customsReferenceCollection?.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.SupplementaryCode).ToArray() ?? Array.Empty<CustomsReference>();
			AssertEquals("GOODS ITEM SupplementaryCodes Count", 1, universalShipmentSupplementaryCodes.Length);
			AssertEquals("GOODS ITEM SupplementaryCodes[0] Code", expectedSupplementaryCode, universalShipmentSupplementaryCodes[0].SubType?.Code);
		}

		public static void SetupNctsHeaderForDeparture(BusinessObjectFactory factory, NctsHeader nctsHeaderBO)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "PC1", nctsHeaderBO.Principal, "1", traderTir: "GBR/022/1234567");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "CO1", nctsHeaderBO.Consignor, "2");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "CE1", nctsHeaderBO.Consignee, "3");

			NCTSTestHelper.SetMrnForTest(nctsHeaderBO, "MRN123");
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = "REPRESENTATIVE";
			staff.GS_Code = "A35";
			nctsHeaderBO.MovementHeader.BM_GS_NKCusAgent = "NWG";
			nctsHeaderBO.MovementHeader.SetMixedConsignment();
			nctsHeaderBO.MovementHeader.BM_RL_NKDestinationPort = Constants.CountryCodes.Russia;
			nctsHeaderBO.MovementHeader.BM_LocationOfGoodsCode = "PRE-LODGED"; // also represents AuthorisedLocationOfGoodsCode, eg. "954131533-GB60DEP"
			nctsHeaderBO.MovementHeader.BM_LocationOfGoods = "LOC";
			nctsHeaderBO.MovementHeader.BM_RL_NKForeignDestPort = "CNKWE";
			nctsHeaderBO.MovementHeader.BM_CustomsSubPlace = "DOVER ERTS";
			nctsHeaderBO.PlaceOfUnloadingCode = "UAODS";
			nctsHeaderBO.BH_RL_NKImportLoadPort = Constants.CountryCodes.Singapore;
			nctsHeaderBO.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			nctsHeaderBO.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			nctsHeaderBO.MovementHeader.BM_TransportAtDeparture = "REG DEP1";
			nctsHeaderBO.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "NZ";
			nctsHeaderBO.MovementHeader.BM_TOLCarrierID = "REG DEP2";
			nctsHeaderBO.MovementHeader.BM_TOLCarrierCode = "JP";
			nctsHeaderBO.MovementHeader.BM_EntryDate = ZDate.BrettsBirthday;
			nctsHeaderBO.MovementHeader.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
			nctsHeaderBO.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.AccountHolderWithCarrier;
			nctsHeaderBO.MovementHeader.BM_AdditionalText = "COMM-REF1";
			nctsHeaderBO.MovementHeader.BM_ConveyanceNumber = "CONV-REF1";
			nctsHeaderBO.MovementHeader.IsSimplifiedNctsProcedure = true;
			nctsHeaderBO.BH_FTZMove = true;
			SetupCustomsOfficesForTest(nctsHeaderBO);
			nctsHeaderBO.MovementHeader.BM_GONumber = "A3";
			nctsHeaderBO.MovementHeader.BM_ExportDate = ZDate.Today.AddDays(7);

			NCTSTestHelper.SetupContainersAndSealsForTest(nctsHeaderBO);
			NCTSTestHelper.SetupGuaranteesForTest(nctsHeaderBO);
			SetupItineraryForTest(nctsHeaderBO);
			NCTSTestHelper.SetupCarrierForTest(nctsHeaderBO);
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "COS", nctsHeaderBO.SecurityConsignor, "4");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "TSC", nctsHeaderBO.SecurityConsignee, "5");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "REP", nctsHeaderBO.MovementHeader.Representative, "6");
			nctsHeaderBO.MovementHeader.BM_OA_WarehouseAddress = NCTSTestHelper.CreateOrgAddressForTest(factory, "WAR", "7").PK;

			SetupDepartureGoodsItemsForTest(factory, nctsHeaderBO.MovementHeader.GoodsItems.AddNew(), false, "1");
			SetupDepartureGoodsItemsForTest(factory, nctsHeaderBO.MovementHeader.GoodsItems.AddNew(), true, "2");
			nctsHeaderBO.BH_CustomsProfile = "999A";
			nctsHeaderBO.MovementHeader.BM_SealType = "CON";
			nctsHeaderBO.MovementHeader.BM_SealQty = 2;
		}

		static void SetupDepartureGoodsItemsForTest(BusinessObjectFactory factory, NctsDepartureCargoDesc goodsItem, bool addContainerPivot, string suffix)
		{
			var refCountryOfOrigin = factory.New<RefCountry>();
			refCountryOfOrigin.Code = "Z" + suffix;
			var refCountryState = refCountryOfOrigin.States.AddNew();
			refCountryState.RW_Code = "O" + suffix;
			refCountryState.RW_Description = "O DESC" + suffix;
			refCountryState.RW_RN_NKCountryCode = "AU";

			goodsItem.BY_HarmonisedTariff = "12345678" + suffix;
			goodsItem.BY_Type = "T" + suffix;
			goodsItem.BY_Description = "DESC" + suffix;
			goodsItem.BY_GrossWeight = 10;
			goodsItem.BY_GrossWeightUnit = Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = 1;
			goodsItem.BY_NetWeightUnit = Constants.Weight.Kilograms;
			goodsItem.BY_RN_NKCountryOfDispatch = "A" + suffix;
			goodsItem.BY_RN_NKCountryOfOrigin = refCountryOfOrigin.Code;
			goodsItem.BY_RW_NKOriginState = refCountryState.RW_Code;
			goodsItem.BY_RN_NKCountryOfDestination = "K" + suffix;
			goodsItem.BY_TransportChargesMethodOfPayment = "A";
			goodsItem.BY_CommercialReferenceNumber = "C" + suffix;
			goodsItem.BY_CustomsSecondQuantity = 1.0;
			goodsItem.BY_CustomsSecondUnitQty = "DTNE";
			goodsItem.BY_MonetaryValue = 2.0;
			goodsItem.BY_ZZF_NKTaxType = "ABCD";
			goodsItem.BY_Procedure = "PRO" + suffix;

			var subCode = "U" + suffix;

			DGSubstanceTestHelper.CreateIfDoesntExist(subCode, "", "IMO");

			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, subCode, "", "IMO").First().PK;

			AddPreviousDocumentForTest(goodsItem, "T1", "P", "PD1");
			AddPreviousDocumentForTest(goodsItem, "T2", "P", "PD2");
			AddSupportingDocumentForTest(goodsItem, "380", "REF1", "SD1");
			AddSupportingDocumentForTest(goodsItem, "18", "REF2", "SD2");
			AddAdditionalInfosForTest(goodsItem, "00100", "AI1", "AD", true);
			AddAdditionalInfosForTest(goodsItem, "00200", "AI2", "IS", false);
			goodsItem.AdditionalSupplementaryCodes.AddNew().CY_Code = "A" + suffix;

			NCTSTestHelper.CreateJobDocAddressForTest(factory, "CO1", goodsItem.Consignor, "6");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "CE1", goodsItem.Consignee, "7");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "COS", goodsItem.SecurityConsignor, "8");
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "TSC", goodsItem.SecurityConsignee, "9");

			if (addContainerPivot)
			{
				NCTSTestHelper.SetContainerPivotForTest(goodsItem, 0);
				NCTSTestHelper.SetContainerPivotForTest(goodsItem, 1);
			}

			NCTSTestHelper.SetupPackageForTest(goodsItem, "MARKA", "BX", 10);
			NCTSTestHelper.SetupPackageForTest(goodsItem, "MARKB", "CT", 20);
		}

		static void SetupNonDepartureGoodsItemsForTest(NctsCommonCargoDesc goodsItem, string suffix)
		{
			goodsItem.BY_HarmonisedTariff = "12345678" + suffix;
			goodsItem.BY_Type = "T" + suffix;
			goodsItem.BY_Description = "DESC" + suffix;
			goodsItem.BY_GrossWeight = 10;
			goodsItem.BY_GrossWeightUnit = Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = 1;
			goodsItem.BY_NetWeightUnit = Constants.Weight.Kilograms;

			AddSupportingDocumentForTest(goodsItem, "380", "REF1", "SD1");
			AddSupportingDocumentForTest(goodsItem, "18", "REF2", "SD2");

			NCTSTestHelper.SetupPackageForTest(goodsItem, "MARKA", "BX", 10);
			NCTSTestHelper.SetupPackageForTest(goodsItem, "MARKB", "CT", 20);
		}

		static void AddPreviousDocumentForTest(NctsDepartureCargoDesc goodsItem, string pdType, string pdClass, string pdRef)
		{
			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_Code = pdType;            // Document type
			pd.CSI_SubType = pdClass;        // Document reference
			pd.CSI_ReferenceNumber = pdRef;  // Complement of information
		}

		static void AddSupportingDocumentForTest(NctsCommonCargoDesc goodsItem, string sdType, string sdReason, string sdRef)
		{
			var sd = goodsItem.SupportingDocuments.AddNew();
			sd.CSI_Code = sdType;             // Document type
			sd.CSI_Description = sdReason;    // Document reference
			sd.CSI_ReferenceNumber = sdRef;   // Complement of information
		}

		static void AddAdditionalInfosForTest(NctsCommonCargoDesc goodsItem, string aiType, string aiDescription, string aiExportFromCountry, bool aiExportFromEC)
		{
			var ai = goodsItem.AdditionalInfos.AddNew();
			ai.CSI_Code = aiType;                               // Additional information coded
			ai.CSI_Description = aiDescription;                 // Additional Information
			ai.CSI_RN_NKCountryCode = aiExportFromCountry;      // Export from country
			ai.CSI_NctsExportFromEC = aiExportFromEC;           // Export from EC
		}

		static void SetupCustomsOfficesForTest(NctsHeader nctsHeaderBO)
		{
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeaderBO, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AA123456", ZDateTime.Empty, true);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeaderBO, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeaderBO, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeaderBO, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "ZZ123456", ZDateTime.Empty);
		}

		static void SetupItineraryForTest(NctsHeader nctsHeaderBO)
		{
			NCTSTestHelper.AddItineraryCountryForTest(nctsHeaderBO, "DE");
			NCTSTestHelper.AddItineraryCountryForTest(nctsHeaderBO, "CH");
		}

		void SetupNctsHeaderForArrival(NctsHeader nctsHeaderBO)
		{
			nctsHeaderBO.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
			nctsHeaderBO.ArrivalMovementHeader.BM_GrossWeight = 123.45m;
			nctsHeaderBO.ArrivalMovementHeader.BM_GrossWeightUQ = Constants.Weight.Kilograms;
			NCTSTestHelper.SetMrnForTest(nctsHeaderBO, "MRN123");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CPD", nctsHeaderBO.DestinationTrader, "9", traderTir: "GBR/022/999999");
			nctsHeaderBO.ArrivalMovementHeader.BM_PlaceOfUnloading = "UAODS";
			nctsHeaderBO.ArrivalMovementHeader.BM_CustomsSubPlace = "A-SUBPLACE";
			nctsHeaderBO.ArrivalMovementHeader.BM_LocationOfGoodsCode = "A-LOCOFGOODS";
			nctsHeaderBO.ArrivalMovementHeader.BM_EntryDate = ZDate.BrettsBirthday.AddDays(1);
			SetupCustomsOfficesForTest(nctsHeaderBO);

			NCTSTestHelper.CreateEventsAndIncidents(nctsHeaderBO);

			SetupNonDepartureGoodsItemsForTest(nctsHeaderBO.ArrivalMovementHeader.GoodsItems.AddNew(), "1");
			SetupNonDepartureGoodsItemsForTest(nctsHeaderBO.ArrivalMovementHeader.GoodsItems.AddNew(), "2");
		}

		static void AssertMessagingApplicationCode(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertEquals("MessagingApplicationCode.Code", CusInBondApplicationCodeList.Codes.NCTS4, xmlShipmentData.MessagingApplicationCode.Code);
			AssertEquals("MessagingApplicationCode.Description", CusInBondApplicationCodeList.Descriptions.NCTS4, xmlShipmentData.MessagingApplicationCode.Description);
		}

		static void AssertArrivalXmlDataGenerated(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertMessagingApplicationCode(xmlShipmentData);
			AssertEquals("Document/reference number (MRN)", "MRN123", GetEntryNumberCollectionValue(xmlShipmentData.EntryNumberCollection, CusEntryNumberTypes.Standard.MovementReferenceNumber));
			AssertEquals("Arrival Agreed location of goods code / Arrival Authorised location of goods code / Arrival Agreed location of goods / Customs sub place", "A-LOCOFGOO", xmlShipmentData.SubLocationAtClearance.Code);
			AssertEquals("Arrival Agreed location of goods code / Arrival Authorised location of goods code / Arrival Agreed location of goods / Customs sub place", "A-LOCOFGOODS : Authorised Location of Goods Code", xmlShipmentData.SubLocationAtClearance.Description);
			AssertEquals("Arrival Simplified procedure flag", "1", GetAddInfoCollectionValue(xmlShipmentData.AddInfoCollection, DataObjectWriterConstants.ArrivalMovementHeader.AddInfo.SimplifiedArrivalProcedureFlag));
			AssertEquals("Arrival notification date", ZDate.BrettsBirthday.AddDays(1).ToString(), GetDateCollectionValue(xmlShipmentData.DateCollection, UniversalDataObjects.DateType.Arrival).Substring(0, 9));
			AssertEquals("Arrival notification place", "Brisbane", xmlShipmentData.CustomsOffice.Code);

			AssertOrganizationAddress("(DESTINATION) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "ImporterDocumentaryAddress"), "9", tir: "GBR/022/999999");

			AssertOffice("(PRESENTATION OFFICE) CUSTOMS OFFICE Reference number", xmlShipmentData.CustomsReferenceCollection, "ZZ123456", "DES", ZDateTime.Empty);

			AssertPackageLinkExists("Arrival Item 1 package count", xmlShipmentData.PackingLineCollection, "A", 1, 2);
			AssertPackageLinkExists("Arrival Item 2 package count", xmlShipmentData.PackingLineCollection, "A", 2, 2);
		}

		static void SetupNctsHeaderForUnloading(NctsHeader nctsHeaderBO)
		{
			nctsHeaderBO.ResetUnloadedValues();

			nctsHeaderBO.UnloadingMovementHeader.BM_GrossWeightUQ = Constants.Weight.Kilograms;
			nctsHeaderBO.UnloadingRemark.G9_StateOfSealsOk = "A";
			nctsHeaderBO.UnloadingRemark.G9_UnloadingRemark = "B";
			nctsHeaderBO.UnloadingRemark.G9_Conform = "C";
			nctsHeaderBO.UnloadingRemark.G9_UnloadingCompletion = "D";
			nctsHeaderBO.UnloadingRemark.G9_UnloadingDate = ZDate.BrettsBirthday.AddDays(2);

			nctsHeaderBO.UnloadedMeansOfTransportAtDepartureNationality = "DC";

			var resultsOfControl = nctsHeaderBO.ResultsOfControlCollection.AddNew();
			resultsOfControl.Data.G9_ControlIndicator = "DI";
			resultsOfControl.Data.G9_PointerToTheAttribute = "35";
			resultsOfControl.Data.G9_Description = "wrong trousers";
			resultsOfControl.Data.G9_CorrectedValue = "1010";

			var unloadedGoodsItem = nctsHeaderBO.UnloadingMovementHeader.GoodsItems[0];
			var itemResultsOfControl = unloadedGoodsItem.ResultsOfControlCollection.AddNew();
			itemResultsOfControl.Data.G9_ControlIndicator = "DI";
			itemResultsOfControl.Data.G9_PointerToTheAttribute = "55";
			itemResultsOfControl.Data.G9_Description = "wrong shirt";
		}

		static void AssertAddInfoGroupExists(string message, IEnumerable<AddInfoGroup> addInfoGroupCollectionData, string addInfoGroupType, string keyValue)
		{
			var addInfoGroupExists = addInfoGroupCollectionData.Where(o => o.AddInfoCollection != null).Any(o => o.Type.Code.GetValueOrDefault() == addInfoGroupType && o.AddInfoCollection.Any(a => a.Key.GetValueOrDefault() == keyValue));
			AssertEquals(message, true, addInfoGroupExists);
		}

		static void AssertUnloadingXmlDataGenerated(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertAddInfoGroupExists("UNLOADING REMARK Conform exists", xmlShipmentData.AddInfoGroupCollection, "ULR", "Conform");
			AssertAddInfoGroupExists("RESULTS OF CONTROL Control indicator exists", xmlShipmentData.AddInfoGroupCollection, "ROC", "ControlIndicator");
			var unloadingMovementHeader = xmlShipmentData.CommercialInfo.CommercialInvoiceCollection[1];
			AssertEquals("UNLOADING goods item count", 2, unloadingMovementHeader.CommercialInvoiceLineCollection.Count);
			AssertAddInfoGroupExists("UNLOADING GODDS ITEM RESULTS OF CONTROL exists", unloadingMovementHeader.CommercialInvoiceLineCollection[0].AddInfoGroupCollection, "ROC", "PointerToTheAttribute");

			AssertPackageLinkExists("Arrival Item 1 package count", xmlShipmentData.PackingLineCollection, "U", 1, 2);
			AssertPackageLinkExists("Arrival Item 2 package count", xmlShipmentData.PackingLineCollection, "U", 2, 2);
		}

		static void SetupNctsHeaderForCancellation(NctsHeader nctsHeaderBO)
		{
			nctsHeaderBO.ExplanationToCustomsForWhyCancelling = "CANCELLATION REASON";
		}

		static void AssertCancellationXmlDataGenerated(UniversalDataObjects.Shipment xmlShipmentData)
		{
			AssertEquals("Document/reference number (MRN)", "MRN123", GetEntryNumberCollectionValue(xmlShipmentData.EntryNumberCollection, CusEntryNumberTypes.Standard.MovementReferenceNumber));
			AssertEquals("Cancellation reason", "CANCELLATION REASON", xmlShipmentData.AdditionalTerms);
			AssertOrganizationAddress("(PRINCIPAL) TRADER", GetOrganizationAddress(xmlShipmentData.OrganizationAddressCollection, "Principal"), "1", tir: "GBR/022/1234567");
			AssertOffice("DEPARTURE) CUSTOMS OFFICE Reference number", xmlShipmentData.CustomsReferenceCollection, "AA123456", "DEP", ZDateTime.Empty);
		}

		protected override NctsHeader GetNewHeader()
		{
			var nctsHeaderBO = Factory.New<NctsHeader>();
			nctsHeaderBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderBO.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeaderBO;
		}

		protected override NctsHeaderDataObjectWriter GetNewWriter(IDataWritingManager manager) => new NctsHeaderDataObjectWriter(manager);
	}
}
