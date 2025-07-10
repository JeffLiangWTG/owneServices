using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	sealed class NctsHeaderDataObjectReaderTest : NctsHeaderCommonDataObjectReaderTest<NctsHeaderDataObjectReader>
	{
		public void TestsOnlyNctsCusInBondHeaderIsMatched()
		{
			var header1 = Factory.BOFactory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header1.BH_GB = GlbBranch.CurrentBranch.PK;
			CusEntryNumber.LoadOrCreate((BusinessObject)header1, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).CE_EntryNum = "ENT123456";
			var header2 = Factory.BOFactory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header2.BH_GB = GlbBranch.CurrentBranch.PK;
			CusEntryNumber.LoadOrCreate((BusinessObject)header2, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).CE_EntryNum = "ENT123456";
			var header3 = Factory.New<NctsHeader>();
			header3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header3.SetMovementType(NctsMovementType.Codes.Departure);
			header3.BH_GB = GlbBranch.CurrentBranch.PK;
			CusEntryNumber.LoadOrCreate(header3, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).CE_EntryNum = "ENT123456";
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, header3));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(header3);

			var reader = new NctsHeaderDataObjectReader(headerData, new TestErrorLogger(), Factory);
			AssertEquals(header3, reader.ReadIntoBusinessObject());
		}

		public void TestNctsHeaderDataObjectReaderForDeparture()
		{
			var headerBO = SetupHeader();
			headerBO.BH_HeaderType = NctsMovementType.Codes.Departure;

			SetupDepartureBo(headerBO);

			var writeManager = new DataWritingManager(new ActionInfo(null, headerBO));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(headerBO);

			var reader = new NctsHeaderDataObjectReader(headerData, new TestErrorLogger(), Factory);
			var readerBo = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CheckHeaderBo(headerBO, readerBo);

			CheckDepartureBo(headerBO, readerBo);
		}

		public void TestNctsHeaderDataObjectReaderForArrival()
		{
			var headerBO = SetupHeader();
			headerBO.SetMovementType(NctsMovementType.Codes.Arrival);

			SetupArrivalBo(headerBO);

			var writeManager = new DataWritingManager(new ActionInfo(null, headerBO));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(headerBO);

			var logger = new TestErrorLogger();
			var reader = new NctsHeaderDataObjectReader(headerData, logger, Factory);
			var readerBo = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CheckHeaderBo(headerBO, readerBo);

			CheckArrivalBo(headerBO, readerBo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNctsHeaderDataObjectReaderFromArrival()
		{
			var headerBO = SetupNCTSHeaderFromFile("UniversalShipmentNCTSHeaderArrival.xml");
			AssertResultFromFileForNctsHeader(headerBO, NctsMovementType.Codes.Arrival);
			AssertArrivalData(headerBO);
		}

		public void TestNctsHeaderDataObjectReaderForUnloading()
		{
			var headerBO = SetupHeader();
			headerBO.SetMovementType(NctsMovementType.Codes.Arrival);

			SetupUnloadingBo(headerBO);

			var writeManager = new DataWritingManager(new ActionInfo(null, headerBO));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(headerBO);

			var logger = new TestErrorLogger();
			var reader = new NctsHeaderDataObjectReader(headerData, logger, Factory);
			var readerBo = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CheckHeaderBo(headerBO, readerBo);

			CheckUnloadingBo(headerBO, readerBo);
		}

		public void TestNctsHeaderDataObjectReaderForCombinedDepartureAndArrival()
		{
			var headerBO = SetupHeader();
			headerBO.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			SetupDepartureBo(headerBO);
			SetupArrivalBo(headerBO);
			SetupUnloadingBo(headerBO);

			var writeManager = new DataWritingManager(new ActionInfo(null, headerBO));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var headerData = writer.GetDataObject(headerBO);

			var reader = new NctsHeaderDataObjectReader(headerData, new TestErrorLogger(), Factory);
			var readerBo = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CheckHeaderBo(headerBO, readerBo);

			CheckDepartureBo(headerBO, readerBo);
			CheckArrivalBo(headerBO, readerBo);
			CheckUnloadingBo(headerBO, readerBo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNctsHeaderDataObjectReaderFromDeparture()
		{
			var headerBO = SetupNCTSHeaderFromFile("UniversalShipmentNCTSHeaderDeparture.xml");
			AssertResultFromFileForNctsHeader(headerBO, "D");
			AssertDepartureData(headerBO);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNctsHeaderDataObjectReaderFromUnloading()
		{
			var headerBO = SetupNCTSHeaderFromFile("UniversalShipmentNCTSHeaderUnloading.xml");
			AssertResultFromFileForNctsHeader(headerBO, NctsMovementType.Codes.Arrival);
			AssertUnloadingData(headerBO);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNctsHeaderDataObjectReaderFromCombinedDepartureAndArrival()
		{
			var headerBO = SetupNCTSHeaderFromFile("UniversalShipmentNCTSHeaderDepartureAndArrival.xml");
			AssertResultFromFileForNctsHeader(headerBO, "DA");
			AssertDepartureData(headerBO);
			AssertArrivalData(headerBO);
			AssertUnloadingData(headerBO);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUxmlReaderGeneratesCorrectRowCount()
		{
			var headerCount = Factory.Load<CusInBondHeader>(new ZQuery()).Length;
			var moveHeaderCount = Factory.Load<CusInBondMoveHeader>(new ZQuery()).Length;
			var goodsItemCount = Factory.Load<CusInBondCargoDesc>(new ZQuery()).Length;
			var headerBO = SetupNCTSHeaderFromFile("UniversalShipmentNCTSHeaderDepartureAndArrival.xml");
			CombineAssertions(delegate
			{
				AssertEquals("Incorrect CusInBondHeader row count", headerCount + 1, Factory.Load<CusInBondHeader>(new ZQuery()).Length);
				AssertEquals("Incorrect CusInBondMoveHeader row count", moveHeaderCount + 3, Factory.Load<CusInBondMoveHeader>(new ZQuery()).Length);
				AssertEquals("Incorrect CusInBondCargoDesc row count", goodsItemCount + 9, Factory.Load<CusInBondCargoDesc>(new ZQuery()).Length);
			});

			var factory = new BusinessObjectFactory();
			headerBO.BH_RL_NKImportLoadPort = "XX";
			headerBO = ProcessQueuedUniversalShipmentMessage("UniversalShipmentNCTSHeaderDepartureAndArrival.xml");
			AssertEquals("BH_RL_NKImportLoadPort should be updated", headerBO.BH_RL_NKImportLoadPort, Core.Constants.CountryCodes.Canada);
			CombineAssertions(delegate
			{
				AssertEquals("Incorrect CusInBondHeader row count after reprocessing uXml", headerCount + 1, factory.Load<CusInBondHeader>(new ZQuery()).Length);
				AssertEquals("Incorrect CusInBondMoveHeader row count after reprocessing uXml", moveHeaderCount + 3, factory.Load<CusInBondMoveHeader>(new ZQuery()).Length);
				AssertEquals("Incorrect CusInBondCargoDesc row count after reprocessing uXml", goodsItemCount + 9, factory.Load<CusInBondCargoDesc>(new ZQuery()).Length);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUxmlReaderDoesntUpdateIfMessagingExists()
		{
			var headerBO = Factory.New<NctsHeader>();
			headerBO.SetMovementType(NctsMovementType.Codes.Departure);
			NCTSTestHelper.SetMrnForTest(headerBO, "16GB000060100CCCCC");
			headerBO.BH_RL_NKImportLoadPort = "XX";

			var message = headerBO.Messages.AddNew();
			message.EM_ApplicationCode = "NCT";
			var inboundMessage = Factory.Load<NctsEdiMessage>(message.PK);
			inboundMessage.EM_MessageType = "GB";
			inboundMessage.EM_MessageSubType = "015";
			inboundMessage.EM_MessageText = "XXX";
			inboundMessage.EM_ReceiveTransmit = "RCV";
			Factory.SaveForTesting();
			inboundMessage.EM_ReceiveTransmit = "TRX";
			Factory.SaveForTesting();

			headerBO = ProcessQueuedUniversalShipmentMessage("UniversalShipmentNCTSHeaderDepartureAndArrival.xml");
			AssertEquals("uXml should not be processed because there are messages", headerBO.BH_RL_NKImportLoadPort, "XX");
		}

		public void TestNctsHeaderDataObjectReaderWithNullCustomsReferenceCollection()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			var dataObjectWriter = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeader)));
			var universalShipmentDataObject = dataObjectWriter.GetDataObject(nctsHeader);
			universalShipmentDataObject.SetCustomsReferenceCollection(() => null);
			AssertNull("PRE-CONDITION: CustomsReferenceCollection", universalShipmentDataObject.CustomsReferenceCollection);

			var dataObjectReader = new NctsHeaderDataObjectReader(universalShipmentDataObject, new TestErrorLogger(), Factory);
			AssertNoExceptionThrown(() => dataObjectReader.ReadIntoBusinessObject());
		}

		NctsHeader SetupHeader()
		{
			var headerBO = Factory.New<NctsHeader>();
			headerBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			var euOfficeCode = headerBO.CustomsOffices.AddNew();
			euOfficeCode.CY_Type = "CYT";
			euOfficeCode.CY_Code = "DEP";
			euOfficeCode.CY_Data = "CY Data";
			euOfficeCode.CY_IsOverridden = true;
			euOfficeCode.CY_Order = 1;
			euOfficeCode.CY_Date = ZDateTime.Today.AddMonths(1);

			euOfficeCode = headerBO.CustomsOffices.AddNew();
			euOfficeCode.CY_Type = "CYT";
			euOfficeCode.CY_Code = "DES";
			euOfficeCode.CY_Data = "CY Data2";
			euOfficeCode.CY_IsOverridden = true;
			euOfficeCode.CY_Order = 2;
			euOfficeCode.CY_Date = ZDateTime.Today.AddMonths(2);

			CusEntryNumber.LoadOrCreate(headerBO, CusEntryNumberTypes.Standard.MovementReferenceNumber, headerBO.CountryCode).CE_EntryNum = "CE EntryNum";

			var principalJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Principal, ContactType.NotifyParty);
			var principal = headerBO.DocAddresses.FindOrCreateWithRequirement(principalJobDocAddressRequirement);
			principal.E2_OA_Address = ZGuid.NewZGuid();
			principal.E2_AddressOverride = true;
			principal.E2_CompanyName = "DE";

			var consignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
			var consignor = headerBO.DocAddresses.FindOrCreateWithRequirement(consignorJobDocAddressRequirement);
			consignor.E2_OA_Address = ZGuid.NewZGuid();
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "GB";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "carrier Code";
			headerBO.BH_OH_Carrier = carrier.PK;

			headerBO.CarrierOrgAddress.Address1 = "carrier address1";
			headerBO.CarrierOrgAddress.City = "carrier city";
			headerBO.CarrierOrgAddress.CompanyName = "carrier companyname";
			headerBO.CarrierOrgAddress.OA_RN_NKCountryCode = "DE";
			headerBO.CarrierOrgAddress.OA_Code = "carrier Code";
			headerBO.CarrierOrgAddress.Postcode = "Postcode";

			var orgHeader = headerBO.CarrierOrgAddress.Header;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR", Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.CustomsCodes.AddNew("TIR", "TIR", Core.Constants.CountryCodes.UnitedKingdom);

			headerBO.MessageFunctionCode = "IE044";

			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_ParentID = headerBO.PK;
			guarantee.PW_ParentTableCode = headerBO.TablePrefix;
			guarantee.PW_BondType = "1";

			return headerBO;
		}

		void SetupDepartureGoodsItem(NctsHeader headerBO, NctsDepartureMovementHeader nctsMoveHeader, ZDecimal itemNumber, ZString temp)
		{
			var goodsItemBO = nctsMoveHeader.GoodsItems.AddNew();
			goodsItemBO.BY_Description = "BI Description" + temp;
			goodsItemBO.BY_GrossWeight = 10 + itemNumber;
			goodsItemBO.BY_GrossWeightUnit = "Kg";
			goodsItemBO.BY_NetWeight = 20 + itemNumber;
			goodsItemBO.BY_NetWeightUnit = "Kg";
			goodsItemBO.BY_HarmonisedTariff = "Commodity Code" + temp;
			goodsItemBO.BY_RN_NKCountryOfOrigin = "CN";
			goodsItemBO.BY_RN_NKCountryOfDispatch = "DE";
			goodsItemBO.BY_RN_NKCountryOfDestination = "AU";
			goodsItemBO.BY_CustomsSecondQuantity = 1.0;
			goodsItemBO.BY_CustomsSecondUnitQty = "DTNE";
			goodsItemBO.BY_MonetaryValue = 2.0;
			goodsItemBO.BY_ZZF_NKTaxType = "6501";
			goodsItemBO.BY_Procedure = "1000";
			goodsItemBO.BY_RW_NKOriginState = "PD";

			if (headerBO.DepartureHeaderContainers.Count > 0)
			{
				var nonPersistentContainerPivot = goodsItemBO.ContainersPivots.AddNew();
				var container = headerBO.DepartureHeaderContainers[0];
				nonPersistentContainerPivot.Container = container;
				nonPersistentContainerPivot.ContainerSelected = true;
				nonPersistentContainerPivot.ContainerNumber = "5";
			}

			var packageBO = goodsItemBO.Packages.AddNew();
			packageBO.B5_UnitType = "PAC";
			packageBO.B5_UnitCount = 1 + itemNumber.ToZInt();
			packageBO.B5_MarksAndNumbers = "Mark and Numbers" + temp;
			var dataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(headerBO.Factory);
			dataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType);
			dataHelper.CreateNewOrGetExistingCusCodeList(headerBO.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "Decl", "Declaration Type", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			headerBO.Factory.InvalidateCachedProperties();
			goodsItemBO.BY_Type = "Dec";
			goodsItemBO.BY_CommercialReferenceNumber = "Commercial ReferenceNumber" + temp;
			goodsItemBO.BY_TransportChargesMethodOfPayment = "A";

			var undgs = goodsItemBO.UNDGs.AddNew();

			var query = new ZQuery();
			query.AddToFilter(UNDGSubstanceSchema.DG_UNNO, "1234");
			query.AddToFilter(UNDGSubstanceSchema.DG_Variant, temp);
			query.AddToFilter(UNDGSubstanceSchema.DG_Standard, "IMO");

			var loaded = Factory.Load<UNDGSubstance>(query).FirstOrDefault();
			if (loaded == null)
			{
				loaded = Factory.New<UNDGSubstance>();
				loaded.DG_UNNO = "1234";
				loaded.DG_Variant = temp;
				loaded.DG_Standard = "IMO";
			}

			undgs.LinkDefault(loaded);

			var newConsignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
			var newConsignor = goodsItemBO.DocAddresses.FindOrCreateWithRequirement(newConsignorJobDocAddressRequirement);
			newConsignor.E2_OA_Address = ZGuid.NewZGuid();
			newConsignor.E2_AddressOverride = true;
			newConsignor.E2_CompanyName = "DE";
			var consigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.Consignee);
			var consignee = goodsItemBO.DocAddresses.FindOrCreateWithRequirement(consigneeJobDocAddressRequirement);
			consignee.E2_OA_Address = ZGuid.NewZGuid();
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "GB";

			goodsItemBO.AdditionalInfos.AddNew();

			goodsItemBO.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP";
			goodsItemBO.AdditionalSupplementaryCodes.AddNew().CY_Code = "PUS";
		}

		void SetupNonDepartureGoodsItem(NctsCommonMovementHeader nctsMoveHeader, ZDecimal itemNumber, ZString temp)
		{
			var goodsItemBO = nctsMoveHeader.GoodsItems.AddNew();
			goodsItemBO.BY_Description = "BI Description" + temp;
			goodsItemBO.BY_GrossWeight = 10 + itemNumber;
			goodsItemBO.BY_GrossWeightUnit = "Kg";
			goodsItemBO.BY_NetWeight = 20 + itemNumber;
			goodsItemBO.BY_NetWeightUnit = "Kg";
			goodsItemBO.BY_HarmonisedTariff = "Commodity Code" + temp;

			var packageBO = goodsItemBO.Packages.AddNew();
			packageBO.B5_UnitType = "PAC";
			packageBO.B5_UnitCount = 1 + itemNumber.ToZInt();
			packageBO.B5_MarksAndNumbers = "Mark and Numbers" + temp;

			var resultsOfControl = Factory.New<CusAddInfo<ResultsOfControlAddInfo>>();
			resultsOfControl.B7_ParentID = goodsItemBO.PK;
			resultsOfControl.B7_ParentTableCode = goodsItemBO.TablePrefix;
			resultsOfControl.B7_AddInfoData = "B7 AddInfoData = BYE" + temp;

			goodsItemBO.AdditionalInfos.AddNew();
		}

		void CheckHeaderBo(NctsHeader headerBO, NctsHeader readerBo)
		{
			AssertEquals(headerBO.BH_HeaderType, readerBo.BH_HeaderType);
			if (headerBO.IsDepartureMovement)
			{
				AssertEquals(headerBO.MovementHeader.BM_AdditionalText, readerBo.MovementHeader.BM_AdditionalText);
			}
			AssertEquals(2, readerBo.CustomsOffices.Count);
			foreach (NctsEuOfficeCode customOffice in headerBO.CustomsOffices)
			{
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_Type == customOffice.CY_Type));
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_Code == customOffice.CY_Code));
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_Data == customOffice.CY_Data));
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_IsOverridden == customOffice.CY_IsOverridden));
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_Order == customOffice.CY_Order));
				AssertNotNull(readerBo.CustomsOffices.Find(x => x.CY_Date == customOffice.CY_Date));
			}
			AssertEquals(headerBO.MovementReferenceNumber, readerBo.MovementReferenceNumber);
			AssertNotNull(readerBo.Principal);
			AssertNotNull(readerBo.Consignor);
			AssertEquals(headerBO.Principal.E2_CompanyName, readerBo.Principal.E2_CompanyName);
			AssertEquals(headerBO.Principal.E2_AddressOverride, readerBo.Principal.E2_AddressOverride);
			AssertEquals(headerBO.Consignor.E2_CompanyName, readerBo.Consignor.E2_CompanyName);
			AssertEquals(headerBO.Consignor.E2_AddressOverride, readerBo.Consignor.E2_AddressOverride);

			AssertNotNull(readerBo.CarrierOrgAddress);
			AssertEquals(headerBO.CarrierOrgAddress.Address1, readerBo.CarrierOrgAddress.Address1);
			AssertEquals(headerBO.CarrierOrgAddress.City, readerBo.CarrierOrgAddress.City);
			AssertEquals(headerBO.CarrierOrgAddress.CompanyName, readerBo.CarrierOrgAddress.CompanyName);
			AssertEquals(headerBO.CarrierOrgAddress.OA_RN_NKCountryCode, readerBo.CarrierOrgAddress.OA_RN_NKCountryCode);
			AssertEquals(headerBO.CarrierOrgAddress.OA_Code, readerBo.CarrierOrgAddress.OA_Code);
			AssertEquals(headerBO.CarrierOrgAddress.Postcode, readerBo.CarrierOrgAddress.Postcode);

			var sourceCarrierHeader = headerBO.CarrierOrgAddress.Header;
			var targetCarrierHeader = readerBo.CarrierOrgAddress.Header;
			AssertEquals(sourceCarrierHeader.GetEuIdentificationNumber(), targetCarrierHeader.GetEuIdentificationNumber());
			AssertEquals(EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(sourceCarrierHeader, "TIR"), EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(targetCarrierHeader, "TIR"));

			AssertEquals(headerBO.UnloadingRemark.G9_UnloadingDate, readerBo.UnloadingRemark.G9_UnloadingDate);

			AssertNotNull(readerBo.Guarantees);
			AssertEquals(1, readerBo.Guarantees.Count);
			AssertEquals(headerBO.Guarantees, readerBo.Guarantees);
		}

		void CheckDepartureGoodsItem(NctsDepartureCargoDesc sourceGoodsItem, NctsDepartureCargoDesc targetGoodsItem)
		{
			AssertEquals(sourceGoodsItem.BY_LineNo, targetGoodsItem.BY_LineNo);
			AssertEquals(sourceGoodsItem.BY_Description, targetGoodsItem.BY_Description);
			AssertEquals(sourceGoodsItem.BY_GrossWeight, targetGoodsItem.BY_GrossWeight);
			AssertEquals(sourceGoodsItem.BY_GrossWeightUnit, targetGoodsItem.BY_GrossWeightUnit);
			AssertEquals(sourceGoodsItem.BY_NetWeight, targetGoodsItem.BY_NetWeight);
			AssertEquals(sourceGoodsItem.BY_NetWeightUnit, targetGoodsItem.BY_NetWeightUnit);
			AssertEquals(sourceGoodsItem.BY_HarmonisedTariff, targetGoodsItem.BY_HarmonisedTariff);
			AssertEquals(sourceGoodsItem.BY_RN_NKCountryOfOrigin, targetGoodsItem.BY_RN_NKCountryOfOrigin);
			AssertEquals(sourceGoodsItem.BY_RN_NKCountryOfDispatch, targetGoodsItem.BY_RN_NKCountryOfDispatch);
			AssertEquals(sourceGoodsItem.BY_ZZF_NKTaxType, targetGoodsItem.BY_ZZF_NKTaxType);
			AssertEquals(sourceGoodsItem.BY_Procedure, targetGoodsItem.BY_Procedure);
			AssertEquals(sourceGoodsItem.BY_RW_NKOriginState, targetGoodsItem.BY_RW_NKOriginState);

			AssertEquals(sourceGoodsItem.ContainersSelected.Count, targetGoodsItem.ContainersSelected.Count);
			foreach (var headerContainer in sourceGoodsItem.ContainersSelected)
			{
				AssertNotNull(targetGoodsItem.ContainersSelected.Contains(headerContainer));
			}

			foreach (NctsPackage headerPackage in sourceGoodsItem.Packages)
			{
				var packages = targetGoodsItem.Packages;
				AssertNotNull(packages.ToList().Find(x => x.B5_UnitType == headerPackage.B5_UnitType));
				AssertNotNull(packages.ToList().Find(x => x.B5_UnitCount == headerPackage.B5_UnitCount));
				AssertNotNull(packages.ToList().Find(x => x.B5_MarksAndNumbers == headerPackage.B5_MarksAndNumbers));
			}

			AssertEquals(sourceGoodsItem.BY_Type, targetGoodsItem.BY_Type);
			AssertEquals(sourceGoodsItem.BY_CommercialReferenceNumber, targetGoodsItem.BY_CommercialReferenceNumber);
			AssertEquals(sourceGoodsItem.BY_TransportChargesMethodOfPayment, targetGoodsItem.BY_TransportChargesMethodOfPayment);
			AssertEquals(sourceGoodsItem.CountryOfDestination, targetGoodsItem.CountryOfDestination);

			foreach (UNDGDataItem headerUndg in sourceGoodsItem.UNDGs)
			{
				AssertNotNull(targetGoodsItem.UNDGs.Find(x => x.DI_DG == headerUndg.DI_DG));
			}

			AssertNotNull(targetGoodsItem.Consignee);
			AssertNotNull(targetGoodsItem.Consignor);
			AssertEquals(sourceGoodsItem.Consignee.E2_CompanyName, targetGoodsItem.Consignee.E2_CompanyName);
			AssertEquals(sourceGoodsItem.Consignee.E2_AddressOverride, targetGoodsItem.Consignee.E2_AddressOverride);
			AssertEquals(sourceGoodsItem.Consignor.E2_CompanyName, targetGoodsItem.Consignor.E2_CompanyName);
			AssertEquals(sourceGoodsItem.Consignor.E2_AddressOverride, targetGoodsItem.Consignor.E2_AddressOverride);

			var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, targetGoodsItem.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, targetGoodsItem.TablePrefix);
			var cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(1, cusSupportingInfoGroups.Length);

			AssertEquals("AdditionalSupplementaryCodes AsString", "SUP,PUS", targetGoodsItem.AdditionalSupplementaryCodes.AsString);
		}

		void CheckNonDepartureGoodsItem(NctsCommonCargoDesc sourceGoodsItem, NctsCommonCargoDesc targetGoodsItem)
		{
			AssertEquals(sourceGoodsItem.BY_LineNo, targetGoodsItem.BY_LineNo);
			AssertEquals(sourceGoodsItem.BY_Description, targetGoodsItem.BY_Description);
			AssertEquals(sourceGoodsItem.BY_GrossWeight, targetGoodsItem.BY_GrossWeight);
			AssertEquals(sourceGoodsItem.BY_GrossWeightUnit, targetGoodsItem.BY_GrossWeightUnit);
			AssertEquals(sourceGoodsItem.BY_NetWeight, targetGoodsItem.BY_NetWeight);
			AssertEquals(sourceGoodsItem.BY_NetWeightUnit, targetGoodsItem.BY_NetWeightUnit);
			AssertEquals(sourceGoodsItem.BY_HarmonisedTariff, targetGoodsItem.BY_HarmonisedTariff);

			foreach (NctsPackage headerPackage in sourceGoodsItem.Packages)
			{
				AssertNotNull(targetGoodsItem.Packages.ToList().Find(x => x.B5_UnitType == headerPackage.B5_UnitType));
				AssertNotNull(targetGoodsItem.Packages.ToList().Find(x => x.B5_UnitCount == headerPackage.B5_UnitCount));
				AssertNotNull(targetGoodsItem.Packages.ToList().Find(x => x.B5_MarksAndNumbers == headerPackage.B5_MarksAndNumbers));
			}

			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, targetGoodsItem.PK);
			query.AddToFilter(CusAddInfoSchema.B7_Type, "ROC");
			var addInfoGroups = Factory.Load<CusAddInfo>(query);
			AssertNotNull(addInfoGroups);
			AssertEquals(targetGoodsItem.MoveHeader.BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Unloading ? 2 : 1, addInfoGroups.Length);

			query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, targetGoodsItem.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, targetGoodsItem.TablePrefix);
			var cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(1, cusSupportingInfoGroups.Length);
		}

		void CheckDepartureBo(NctsHeader headerBO, NctsHeader readerBo)
		{
			var sourceMoveHeader = headerBO.MovementHeader;
			var targetMoveHeader = readerBo.MovementHeader;

			AssertEquals(sourceMoveHeader.BM_SubApplicationCode, targetMoveHeader.BM_SubApplicationCode);
			AssertEquals(headerBO.LocalReferenceNumber, readerBo.LocalReferenceNumber);
			AssertEquals(sourceMoveHeader.BM_InBondEntryType, targetMoveHeader.BM_InBondEntryType);
			AssertEquals(headerBO.MovementHeader.BM_GS_NKCusAgent, readerBo.MovementHeader.BM_GS_NKCusAgent);
			AssertEquals(headerBO.MovementHeader.BM_RL_NKForeignDestPort, readerBo.MovementHeader.BM_RL_NKForeignDestPort);
			AssertEquals(headerBO.BH_RL_NKImportLoadPort, readerBo.BH_RL_NKImportLoadPort);
			AssertEquals(headerBO.MovementHeader.BM_RL_NKDestinationPort, readerBo.MovementHeader.BM_RL_NKDestinationPort);
			AssertEquals(headerBO.MovementHeader.BM_InlandTransportMode, readerBo.MovementHeader.BM_InlandTransportMode);
			AssertEquals(headerBO.MovementHeader.BM_TOLCarrierCode, readerBo.MovementHeader.BM_TOLCarrierCode);
			AssertEquals(headerBO.MovementHeader.BM_TOLCarrierID, readerBo.MovementHeader.BM_TOLCarrierID);
			AssertEquals(headerBO.MovementHeader.BM_BTAIndicator, readerBo.MovementHeader.BM_BTAIndicator);
			AssertEquals(headerBO.MovementHeader.BM_MethodOfPayment, readerBo.MovementHeader.BM_MethodOfPayment);
			AssertEquals(headerBO.PlaceOfUnloadingCode, readerBo.PlaceOfUnloadingCode);
			AssertEquals(headerBO.MovementHeader.BM_ConveyanceNumber, readerBo.MovementHeader.BM_ConveyanceNumber);
			AssertEquals(headerBO.MovementHeader.IsSimplifiedNctsProcedure, readerBo.MovementHeader.IsSimplifiedNctsProcedure);
			AssertEquals(headerBO.MovementHeader.BM_LocationOfGoodsCode, readerBo.MovementHeader.BM_LocationOfGoodsCode);
			AssertEquals(headerBO.MovementHeader.BM_ExportDate, readerBo.MovementHeader.BM_ExportDate);
			AssertEquals(headerBO.BH_FTZMove, readerBo.BH_FTZMove);
			AssertEquals(headerBO.MovementHeader.BM_ExportTransportMode, readerBo.MovementHeader.BM_ExportTransportMode);
			AssertEquals(headerBO.MovementHeader.BM_TransportAtDeparture, readerBo.MovementHeader.BM_TransportAtDeparture);
			AssertEquals(headerBO.MovementHeader.BM_RN_NKTransportAtDepartureCountry, readerBo.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals(headerBO.MovementHeader.BM_SealType, readerBo.MovementHeader.BM_SealType);
			AssertEquals(headerBO.MovementHeader.BM_SealQty, readerBo.MovementHeader.BM_SealQty);

			foreach (NctsDepartureHeaderContainer headerContainer in headerBO.DepartureHeaderContainers)
			{
				AssertNotNull(readerBo.DepartureHeaderContainers?.FirstOrDefault(x => x.BC_ContainerNum == headerContainer.BC_ContainerNum));
			}

			foreach (NonPersistentItineraryCountry readerItinerary in readerBo.Itinerary)
			{
				if (readerItinerary.CountryCode != "DE" && readerItinerary.CountryCode != "GB")
				{
					Assert(false);
				}
			}

			AssertEquals(3, targetMoveHeader.GoodsItems.Count);
			var sourceGoodsItem = sourceMoveHeader.GoodsItems[0];
			var targetGoodsItem = targetMoveHeader.GoodsItems[0];
			CheckDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[1];
			targetGoodsItem = targetMoveHeader.GoodsItems[1];
			CheckDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[2];
			targetGoodsItem = targetMoveHeader.GoodsItems[2];
			CheckDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);
		}

		void SetupDepartureBo(NctsHeader headerBO)
		{
			var nctsMoveHeader = headerBO.MovementHeader;
			nctsMoveHeader.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;

			headerBO.LocalReferenceNumber = "MRN00123456";
			nctsMoveHeader.BM_InBondEntryType = "T1";
			headerBO.MovementHeader.BM_GS_NKCusAgent = "PRC";
			headerBO.MovementHeader.BM_RL_NKForeignDestPort = "ADALV";
			headerBO.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Germany;
			headerBO.MovementHeader.BM_RL_NKDestinationPort = "DE";
			headerBO.MovementHeader.BM_InlandTransportMode = "1";
			headerBO.MovementHeader.BM_TOLCarrierCode = "DE";
			headerBO.MovementHeader.BM_TOLCarrierID = "Identity";
			headerBO.MovementHeader.BM_BTAIndicator = "E";
			headerBO.MovementHeader.BM_MethodOfPayment = "Y";
			headerBO.PlaceOfUnloadingCode = "PUC";
			headerBO.MovementHeader.BM_ConveyanceNumber = "Conveyance Reference Number";
			headerBO.MovementHeader.IsSimplifiedNctsProcedure = true;
			headerBO.MovementHeader.BM_SealType = "PAC";
			headerBO.MovementHeader.BM_SealQty = 12;
			nctsMoveHeader.BM_LocationOfGoodsCode = "Location Of Goods";

			headerBO.MovementHeader.BM_ExportDate = ZDateTime.Today.AddMonths(3);
			headerBO.BH_FTZMove = true;
			headerBO.MovementHeader.BM_ExportTransportMode = "SE";
			headerBO.MovementHeader.BM_TransportAtDeparture = "Means Of Transport At ";
			headerBO.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "DE";

			var nctsHeaderContainer = headerBO.DepartureHeaderContainers.AddNew();
			nctsHeaderContainer.BC_ContainerNum = "container num 1";

			nctsHeaderContainer = headerBO.DepartureHeaderContainers.AddNew();
			nctsHeaderContainer.BC_ContainerNum = "container num 2";

			var nonPersistentItineraryCountry = new NonPersistentItineraryCountry();
			nonPersistentItineraryCountry.CountryCode = "DE";
			headerBO.Itinerary.Add(nonPersistentItineraryCountry);

			nonPersistentItineraryCountry = new NonPersistentItineraryCountry();
			nonPersistentItineraryCountry.CountryCode = "GB";
			headerBO.Itinerary.Add(nonPersistentItineraryCountry);

			SetupDepartureGoodsItem(headerBO, nctsMoveHeader, 1, "1");
			SetupDepartureGoodsItem(headerBO, nctsMoveHeader, 2, "2");
			SetupDepartureGoodsItem(headerBO, nctsMoveHeader, 3, "3");

			Factory.SaveForTesting();
		}

		void CheckArrivalBo(NctsHeader headerBO, NctsHeader readerBo)
		{
			var sourceMoveHeader = headerBO.ArrivalMovementHeader;
			var targetMoveHeader = readerBo.ArrivalMovementHeader;
			AssertEquals(headerBO.ArrivalMovementHeader.IsSimplifiedNctsProcedure, readerBo.ArrivalMovementHeader.IsSimplifiedNctsProcedure);
			AssertEquals(sourceMoveHeader.BM_LocationOfGoodsCode, targetMoveHeader.BM_LocationOfGoodsCode);

			AssertEquals(3, targetMoveHeader.GoodsItems.Count);
			var sourceGoodsItem = sourceMoveHeader.GoodsItems[0];
			var targetGoodsItem = targetMoveHeader.GoodsItems[0];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[1];
			targetGoodsItem = targetMoveHeader.GoodsItems[1];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[2];
			targetGoodsItem = targetMoveHeader.GoodsItems[2];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);
		}

		void SetupArrivalBo(NctsHeader headerBO)
		{
			var nctsMoveHeader = headerBO.ArrivalMovementHeader;
			headerBO.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
			nctsMoveHeader.BM_LocationOfGoodsCode = "Location";

			SetupNonDepartureGoodsItem(nctsMoveHeader, 1, "1");
			SetupNonDepartureGoodsItem(nctsMoveHeader, 2, "2");
			SetupNonDepartureGoodsItem(nctsMoveHeader, 3, "3");

			Factory.SaveForTesting();
		}

		void CheckUnloadingBo(NctsHeader headerBO, NctsHeader readerBo)
		{
			var sourceMoveHeader = headerBO.UnloadingMovementHeader;
			var targetMoveHeader = readerBo.UnloadingMovementHeader;

			AssertEquals(3, targetMoveHeader.GoodsItems.Count);
			var sourceGoodsItem = sourceMoveHeader.GoodsItems[0];
			var targetGoodsItem = targetMoveHeader.GoodsItems[0];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[1];
			targetGoodsItem = targetMoveHeader.GoodsItems[1];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);

			sourceGoodsItem = sourceMoveHeader.GoodsItems[2];
			targetGoodsItem = targetMoveHeader.GoodsItems[2];
			CheckNonDepartureGoodsItem(sourceGoodsItem, targetGoodsItem);
		}

		void SetupUnloadingBo(NctsHeader headerBO)
		{
			var nctsMoveHeader = headerBO.UnloadingMovementHeader;
			nctsMoveHeader.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Unloading;

			SetupNonDepartureGoodsItem(nctsMoveHeader, 1, "1");
			SetupNonDepartureGoodsItem(nctsMoveHeader, 2, "2");
			SetupNonDepartureGoodsItem(nctsMoveHeader, 3, "3");

			Factory.SaveForTesting();
		}

		NctsHeader SetupNCTSHeaderFromFile(ZString filename)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "NCTUKTSEN";
			orgHeader.MainAddress.OA_Address1 = "11TH FLOOR, ALEX HOUSE, VICTORIA AV";
			orgHeader.MainAddress.OA_City = "SOUTHEND-ON-SEA, ESSEX";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "GB";
			orgHeader.MainAddress.OA_PostCode = "SS99 1AA";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB954131533000", Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.CustomsCodes.AddNew("TIR", "GBR/022/1234567", Core.Constants.CountryCodes.UnitedKingdom);
			Factory.SaveForTesting();

			return ProcessQueuedUniversalShipmentMessage(filename);
		}

		NctsHeader ProcessQueuedUniversalShipmentMessage(ZString filename)
		{
			var testShipmentMessage = BaseSourcePath + @"Enterprise\Product\Operations\Customs\EU\NCTS\DataTransfer.Test\TestFiles\" + filename;
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(testShipmentMessage));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_JobReference, "NCT00000001");
			var headerBO = Factory.LoadTop1<NctsHeader>(query);
			headerBO.Reload();

			return headerBO;
		}

		void AssertResultFromFileForNctsHeader(NctsHeader headerBO, string expectedNctsHeaderType)
		{
			AssertNotNull(headerBO);
			AssertEquals(expectedNctsHeaderType, headerBO.BH_HeaderType);
			if (headerBO.IsDepartureMovement)
			{
				AssertEquals("7896", headerBO.MovementHeader.BM_AdditionalText);
			}

			var customOffice = headerBO.CustomsOffices.GetFirstElementHaving("DEP");
			AssertNotNull(customOffice);
			AssertEquals("EUO", customOffice.CY_Type);
			AssertEquals("DEP", customOffice.CY_Code);
			AssertEquals("DE002101", customOffice.CY_Data);
			AssertEquals(false, customOffice.CY_IsOverridden);
			AssertEquals(ZShort.Zero, customOffice.CY_Order);
			AssertEquals("29-Mar-18 10:28:00", customOffice.CY_Date.ToString());

			customOffice = headerBO.CustomsOffices.GetFirstElementHaving("DES");
			AssertNotNull(customOffice);
			AssertEquals("EUO", customOffice.CY_Type);
			AssertEquals("DES", customOffice.CY_Code);
			AssertEquals("BE101000", customOffice.CY_Data);
			AssertEquals(false, customOffice.CY_IsOverridden);
			AssertEquals(ZShort.Zero, customOffice.CY_Order);
			AssertEquals("30-Mar-18 10:28:00", customOffice.CY_Date.ToString());

			var docAddress = headerBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
			AssertNotNull(docAddress);
			AssertEquals("200 UNIVERSITY AVE", docAddress.Address1);
			AssertEquals("B3 QUOTEA CLIENT1", docAddress.CompanyName);

			docAddress = headerBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			AssertNotNull(docAddress);
			AssertEquals("1341 LONG DRIVE", docAddress.Address1);
			AssertEquals("CA 1 IMPORTER/EXPORTER/OWNER", docAddress.CompanyName);

			docAddress = headerBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty2);
			AssertNotNull(docAddress);
			AssertEquals("LOC. FABBRICA 46 S.S. 10", docAddress.Address1);
			AssertEquals("S.4M. SRL", docAddress.CompanyName);

			docAddress = headerBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty3);
			AssertNotNull(docAddress);
			AssertEquals("UNIT 2/ 65 YARRA STREET", docAddress.Address1);
			AssertEquals("S2DIO PTY LTD", docAddress.CompanyName);

			docAddress = headerBO.DocAddresses.FindByDocAddressType(DocAddressType.Principal);
			AssertNotNull(docAddress);
			AssertEquals("PIAZZA DEL POPOLO", docAddress.Address1);
			AssertEquals("NCTS ITALY TEST", docAddress.CompanyName);

			var carrierAddress = headerBO.CarrierOrgAddress;
			AssertNotNull(carrierAddress);
			AssertEquals("11TH FLOOR, ALEX HOUSE, VICTORIA AV", carrierAddress.Address1);
			AssertEquals("SOUTHEND-ON-SEA, ESSEX", carrierAddress.City);
			AssertEquals("", carrierAddress.CompanyName);
			AssertEquals("GB", carrierAddress.Country.Code);
			AssertEquals("NCTUKTSEN", carrierAddress.Header.OH_Code);
			AssertEquals("SS99 1AA", carrierAddress.Postcode);
			AssertEquals("GB954131533000", carrierAddress.Header.GetEuIdentificationNumber());
			AssertEquals("GBR/022/1234567", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(carrierAddress.Header, "TIR"));

			if (headerBO.IsDepartureMovement)
			{
				AssertEquals("30-Mar-18 04:38:00", headerBO.MovementHeader.BM_EntryDate.ToString());
			}

			AssertNotNull(headerBO.Guarantees);
			AssertEquals(2, headerBO.Guarantees.Count);
		}

		void AssertResultFromFileForDepartureGoodsItem(NctsDepartureMovementHeader moveHeader)
		{
			AssertEquals(3, moveHeader.GoodsItems.Count);
			var goodsItem = moveHeader.GoodsItems[0];

			AssertEquals("description2", goodsItem.BY_Description);
			AssertEquals(20, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_GrossWeightUnit);
			AssertEquals(20, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_NetWeightUnit);
			AssertEquals("20", goodsItem.BY_HarmonisedTariff);
			AssertEquals("BE", goodsItem.BY_RN_NKCountryOfOrigin);
			AssertEquals("AU", goodsItem.BY_RN_NKCountryOfDispatch);
			AssertEquals("CN", goodsItem.BY_RN_NKCountryOfDestination);
			AssertEquals("", goodsItem.BY_Procedure);
			AssertEquals("", goodsItem.BY_RW_NKOriginState);

			AssertEquals(1, goodsItem.ContainersSelected.Count);
			AssertEquals("container", goodsItem.ContainersSelected.First());
			AssertEquals(2, goodsItem.Packages.Count);
			AssertEquals("1", goodsItem.Packages[0].B5_UnitType);
			AssertEquals(1, goodsItem.Packages[0].B5_UnitCount);
			AssertEquals("1", goodsItem.Packages[0].B5_MarksAndNumbers);
			AssertEquals("2", goodsItem.Packages[1].B5_UnitType);
			AssertEquals(2, goodsItem.Packages[1].B5_UnitCount);
			AssertEquals("2", goodsItem.Packages[1].B5_MarksAndNumbers);

			AssertEquals("T2", goodsItem.BY_Type);
			AssertEquals("JJHJH", goodsItem.BY_CommercialReferenceNumber);
			AssertEquals("A", goodsItem.BY_TransportChargesMethodOfPayment);

			var existingSubsPK = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0004", "a", "IMO").First().PK;
			AssertNotNull(goodsItem.UNDGs?.Find(x => x.DI_DG == existingSubsPK));

			AssertNotNull(goodsItem.Consignee);
			AssertNotNull(goodsItem.Consignor);
			AssertEquals("19 HOMEDALE ROAD", goodsItem.Consignee.Address1);
			AssertEquals("UNIT 39 CONEGRE IND. ESTATE", goodsItem.Consignor.Address1);

			var query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			var cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "PRE");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);
			AssertEquals("AdditionalSupplementaryCodes AsString", "", goodsItem.AdditionalSupplementaryCodes.AsString);

			goodsItem = moveHeader.GoodsItems[1];
			AssertEquals("description1", goodsItem.BY_Description);
			AssertEquals(10, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("DT", goodsItem.BY_GrossWeightUnit);
			AssertEquals(10, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("DT", goodsItem.BY_NetWeightUnit);
			AssertEquals("10", goodsItem.BY_HarmonisedTariff);
			AssertEquals("AX", goodsItem.BY_RN_NKCountryOfOrigin);
			AssertEquals("KR", goodsItem.BY_RN_NKCountryOfDispatch);
			AssertEquals("FR", goodsItem.BY_RN_NKCountryOfDestination);
			AssertEquals("1000", goodsItem.BY_Procedure);
			AssertEquals("PD", goodsItem.BY_RW_NKOriginState);

			AssertEquals(1, goodsItem.ContainersSelected.Count);
			AssertEquals("container1", goodsItem.ContainersSelected.First());
			AssertEquals(2, goodsItem.Packages.Count);
			AssertEquals("3", goodsItem.Packages[0].B5_UnitType);
			AssertEquals(3, goodsItem.Packages[0].B5_UnitCount);
			AssertEquals("3", goodsItem.Packages[0].B5_MarksAndNumbers);
			AssertEquals("4", goodsItem.Packages[1].B5_UnitType);
			AssertEquals(4, goodsItem.Packages[1].B5_UnitCount);
			AssertEquals("4", goodsItem.Packages[1].B5_MarksAndNumbers);

			AssertEquals("T1", goodsItem.BY_Type);
			AssertEquals("QWQWQ", goodsItem.BY_CommercialReferenceNumber);
			AssertEquals("Y", goodsItem.BY_TransportChargesMethodOfPayment);
			AssertNotNull(goodsItem.UNDGs?.Find(x => x.DI_DG == existingSubsPK));

			AssertNotNull(goodsItem.Consignee);
			AssertNotNull(goodsItem.Consignor);
			AssertEquals("4/F. NO.218 GUANGYUAN ZHONG 343", goodsItem.Consignee.Address1);
			AssertEquals("1341 LONG DRIVE", goodsItem.Consignor.Address1);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "PRE");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);
			AssertEquals("AdditionalSupplementaryCodes AsString", "SUP1,SUP2", goodsItem.AdditionalSupplementaryCodes.AsString);

			goodsItem = moveHeader.GoodsItems[2];
			AssertEquals("description2", goodsItem.BY_Description);
			AssertEquals(20, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_GrossWeightUnit);
			AssertEquals(20, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_NetWeightUnit);
			AssertEquals("20", goodsItem.BY_HarmonisedTariff);
			AssertEquals("IT", goodsItem.BY_RN_NKCountryOfOrigin);
			AssertEquals("BE", goodsItem.BY_RN_NKCountryOfDispatch);
			AssertEquals("JP", goodsItem.BY_RN_NKCountryOfDestination);
			AssertEquals("9999", goodsItem.BY_Procedure);
			AssertEquals("BY", goodsItem.BY_RW_NKOriginState);

			AssertEquals(0, goodsItem.ContainersSelected?.Count);
			AssertEquals(0, goodsItem.Packages.Count);

			AssertEquals("T2", goodsItem.BY_Type);
			AssertEquals("JJHJH", goodsItem.BY_CommercialReferenceNumber);
			AssertEquals("A", goodsItem.BY_TransportChargesMethodOfPayment);
			AssertNotNull(goodsItem.UNDGs?.Find(x => x.DI_DG == existingSubsPK));

			AssertNotNull(goodsItem.Consignee);
			AssertNotNull(goodsItem.Consignor);
			AssertEquals("19 HOMEDALE ROAD", goodsItem.Consignee.Address1);
			AssertEquals("UNIT 39 CONEGRE IND. ESTATE", goodsItem.Consignor.Address1);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "PRE");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);
			AssertEquals("AdditionalSupplementaryCodes AsString", "SUP3,SUP4", goodsItem.AdditionalSupplementaryCodes.AsString);
		}

		void AssertResultFromFileForNonDepartureGoodsItem(NctsCommonMovementHeader moveHeader)
		{
			AssertEquals(3, moveHeader.GoodsItems.Count);
			var goodsItem = moveHeader.GoodsItems[0];

			AssertEquals("description2", goodsItem.BY_Description);
			AssertEquals(20, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_GrossWeightUnit);
			AssertEquals(20, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_NetWeightUnit);
			AssertEquals("20", goodsItem.BY_HarmonisedTariff);

			AssertEquals(2, goodsItem.Packages.Count);
			AssertEquals("1", goodsItem.Packages[0].B5_UnitType);
			AssertEquals(1, goodsItem.Packages[0].B5_UnitCount);
			AssertEquals("1", goodsItem.Packages[0].B5_MarksAndNumbers);
			AssertEquals("2", goodsItem.Packages[1].B5_UnitType);
			AssertEquals(2, goodsItem.Packages[1].B5_UnitCount);
			AssertEquals("2", goodsItem.Packages[1].B5_MarksAndNumbers);

			var query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			var cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			goodsItem = moveHeader.GoodsItems[1];
			AssertEquals("description1", goodsItem.BY_Description);
			AssertEquals(10, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("DT", goodsItem.BY_GrossWeightUnit);
			AssertEquals(10, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("DT", goodsItem.BY_NetWeightUnit);
			AssertEquals("10", goodsItem.BY_HarmonisedTariff);

			AssertEquals(2, goodsItem.Packages.Count);
			AssertEquals("3", goodsItem.Packages[0].B5_UnitType);
			AssertEquals(3, goodsItem.Packages[0].B5_UnitCount);
			AssertEquals("3", goodsItem.Packages[0].B5_MarksAndNumbers);
			AssertEquals("4", goodsItem.Packages[1].B5_UnitType);
			AssertEquals(4, goodsItem.Packages[1].B5_UnitCount);
			AssertEquals("4", goodsItem.Packages[1].B5_MarksAndNumbers);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			goodsItem = moveHeader.GoodsItems[2];
			AssertEquals("description2", goodsItem.BY_Description);
			AssertEquals(20, goodsItem.BY_GrossWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_GrossWeightUnit);
			AssertEquals(20, goodsItem.BY_NetWeight.ToZInt());
			AssertEquals("G", goodsItem.BY_NetWeightUnit);
			AssertEquals("20", goodsItem.BY_HarmonisedTariff);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);

			query1 = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK);
			query1.AddToFilter(CusSupportingInfoSchema.CSI_Type, "SUP");
			query1.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix);
			cusSupportingInfoGroups = Factory.Load<CusSupportingInfo>(query1);
			AssertNotNull(cusSupportingInfoGroups);
			AssertEquals(2, cusSupportingInfoGroups.Length);
		}

		void AssertDepartureData(NctsHeader headerBO)
		{
			var moveHeader = headerBO.MovementHeader;
			AssertEquals("D", moveHeader.BM_SubApplicationCode);
			AssertEquals("123456798", headerBO.LocalReferenceNumber);
			AssertEquals("T2F", moveHeader.BM_InBondEntryType);
			AssertEquals("", headerBO.MovementHeader.BM_GS_NKCusAgent);
			AssertEquals("PAABA", headerBO.MovementHeader.BM_RL_NKForeignDestPort);
			AssertEquals(Core.Constants.CountryCodes.Canada, headerBO.BH_RL_NKImportLoadPort);
			AssertEquals("CA", headerBO.MovementHeader.BM_RL_NKDestinationPort);
			AssertEquals("1", headerBO.MovementHeader.BM_InlandTransportMode);
			AssertEquals("DE", headerBO.MovementHeader.BM_TOLCarrierCode);
			AssertEquals("TRANSPORT ID", headerBO.MovementHeader.BM_TOLCarrierID);
			AssertEquals("E", headerBO.MovementHeader.BM_BTAIndicator);
			AssertEquals("B", headerBO.MovementHeader.BM_MethodOfPayment);
			AssertEquals("ADALV", headerBO.MovementHeader.BM_PlaceOfUnloading);
			AssertEquals("6666", headerBO.MovementHeader.BM_ConveyanceNumber);
			AssertEquals(true, headerBO.MovementHeader.IsSimplifiedNctsProcedure);
			AssertEquals("AGREED LOC", headerBO.MovementHeader.BM_LocationOfGoodsCode);
			AssertEquals("04-Apr-18 00:00:00", headerBO.MovementHeader.BM_ExportDate.ToString());
			AssertEquals(true, headerBO.BH_FTZMove);
			AssertEquals("1", headerBO.MovementHeader.BM_ExportTransportMode);
			AssertEquals("TRANSPORT ID", headerBO.MovementHeader.BM_TransportAtDeparture);
			AssertEquals("DE", headerBO.MovementHeader.BM_RN_NKTransportAtDepartureCountry);

			AssertEquals(2, headerBO.DepartureHeaderContainers.Count);
			AssertEquals("container", headerBO.DepartureHeaderContainers[0].BC_ContainerNum);
			AssertEquals("seal1", headerBO.DepartureHeaderContainers[0].BC_Seal1);
			AssertEquals("seal2", headerBO.DepartureHeaderContainers[0].BC_Seal2);
			AssertEquals("container1", headerBO.DepartureHeaderContainers[1].BC_ContainerNum);
			AssertEquals("seal11", headerBO.DepartureHeaderContainers[1].BC_Seal1);
			AssertEquals("seal21", headerBO.DepartureHeaderContainers[1].BC_Seal2);

			AssertEquals(2, headerBO.Itinerary.Count);
			AssertEquals("CU", headerBO.Itinerary[0].CountryCode);
			AssertEquals("ES", headerBO.Itinerary[1].CountryCode);

			AssertEquals("MovementHeader.BM_SealType", "CON", headerBO.MovementHeader.BM_SealType);
			AssertEquals("MovementHeader.BM_SealQty", (ZShort)999, headerBO.MovementHeader.BM_SealQty);

			AssertResultFromFileForDepartureGoodsItem(moveHeader);
		}

		void AssertArrivalData(NctsHeader headerBO)
		{
			var moveHeader = headerBO.ArrivalMovementHeader;
			AssertEquals(Common.EU.NctsMoveHeaderType.Codes.Arrival, moveHeader.BM_SubApplicationCode);
			AssertEquals(true, moveHeader.IsSimplifiedNctsProcedure);
			AssertResultFromFileForNonDepartureGoodsItem(moveHeader);
		}

		void AssertUnloadingData(NctsHeader headerBO)
		{
			var moveHeader = headerBO.UnloadingMovementHeader;
			AssertEquals(Common.EU.NctsMoveHeaderType.Codes.Unloading, moveHeader.BM_SubApplicationCode);

			AssertResultFromFileForNonDepartureGoodsItem(moveHeader);
		}

		protected override NctsHeaderDataObjectReader GetNewReader(Shipment headerData, TestErrorLogger testErrorLogger) => new NctsHeaderDataObjectReader(headerData, testErrorLogger, Factory);

		protected override NctsHeader GetNewHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}
	}
}
