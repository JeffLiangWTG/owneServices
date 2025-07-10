using System.Linq;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public abstract class NT043BaseResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Arrival;

	public void TestSingleMRN()
	{
		NctsHeader initialNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;

		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber().Replace("<TIRHolderIdentificationNumber>!</TIRHolderIdentificationNumber>", ""),
			(ediMessage) =>
			{
				AssertMovementUpdated(initialNctsHeader, sampleMovementReferenceData);
				AssertHeaderUpdated(initialNctsHeader, false);
				AssertMessageIdentification(ediMessage);
				AssertMessageLinked(initialNctsHeader, ediMessage);
				AssertCESEventAdded(initialNctsHeader);
				AssertNull(LoadRelatedNctsHeader(initialNctsHeader));
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestSingleMRNFormerlyMultiple()
	{
		NctsHeader existingNctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => existingNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, mrn: SampleMRN, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				AssertMessageLinked(existingNctsHeader, ediMessage);
				AssertMessageIdentification(ediMessage);
				AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, existingNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("BM_Phase", NCTS5ArrivalPhaseList.Codes.Arrival, existingNctsHeader.ArrivalMovementHeader.BM_Phase);
				AssertCESEventAdded(existingNctsHeader);
				AssertNull(LoadRelatedNctsHeader(existingNctsHeader));
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
		}
	}

	public void TestMultipleMRN()
	{
		var department = Factory.New<GlbDepartment>();

		NctsHeader initialNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;

		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				var newNctsHeader = LoadRelatedNctsHeader(initialNctsHeader);
				Factory.Save();
				AssertNotNull("New movement created", newNctsHeader);
				AssertNotSame(initialNctsHeader, newNctsHeader);
				AssertNewMovement(newNctsHeader, sampleMovementReferenceData);
				AssertMovementUpdated(newNctsHeader, sampleMovementReferenceData);
				AssertHeaderUpdated(newNctsHeader);
				AssertMessageIdentification(ediMessage);
				AssertMessageLinked(newNctsHeader, ediMessage);
				AssertCESEventAdded(newNctsHeader);
				AssertEquals("BH_SystemCreateUser", "US1", newNctsHeader.BH_SystemCreateUser);
				var job = new JobHeader.Loader(newNctsHeader).TryLoadOrCreate();
				AssertEquals("Department", department.PK, job.JH_GE);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK,
			assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header);
			header.BH_SystemCreateUser = "US1";
			var job = new JobHeader.Loader(header).TryCreate();
			job.JH_GE = department.PK;
		}
	}

	public void TestMultipleMRNWithExistingMultipleMRNIndicatorN()
	{
		NctsHeader initialNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;
		NctsHeader existingNctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				var relatedNctsHeader = LoadRelatedNctsHeader(initialNctsHeader);
				AssertNotNull("Existing movement not found", relatedNctsHeader);
				AssertSame("Existing movement expected", existingNctsHeader, relatedNctsHeader);
				AssertMovementUpdated(relatedNctsHeader, sampleMovementReferenceData);
				AssertHeaderUpdated(relatedNctsHeader);
				AssertMessageIdentification(ediMessage);
				AssertMessageLinked(relatedNctsHeader, ediMessage);
				AssertCESEventAdded(relatedNctsHeader);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK,
			assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header);
			existingNctsHeader = Factory.New<NctsHeader>();
			existingNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			existingNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
			existingNctsHeader.MovementReferenceNumberSetter(SampleMRN);
		}
	}

	public void TestMultipleMRNWithExistingMultipleMRNIndicatorY()
	{
		NctsHeader initialNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;
		NctsHeader existingNctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				var newRelatedNctsHeader = LoadRelatedNctsHeader(initialNctsHeader);
				AssertNotNull("New movement created", newRelatedNctsHeader);
				AssertNotSame(initialNctsHeader, newRelatedNctsHeader);
				AssertNewMovement(newRelatedNctsHeader, sampleMovementReferenceData);
				AssertMovementUpdated(newRelatedNctsHeader, sampleMovementReferenceData);
				AssertHeaderUpdated(newRelatedNctsHeader);
				AssertMessageIdentification(ediMessage);
				AssertMessageLinked(newRelatedNctsHeader, ediMessage);
				AssertCESEventAdded(newRelatedNctsHeader);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK,
			assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header);
			existingNctsHeader = Factory.New<NctsHeader>();
			existingNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			existingNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
			existingNctsHeader.MovementReferenceNumberSetter(SampleMRN);
		}
	}

	public void TestMasterCustomsStatusUpdated()
	{
		NctsHeader masterNctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => masterNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header);
			CreateChildMovement(header, "MRN1", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);
			CreateChildMovement(header, "MRN2", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
			CreateChildMovement(header, "MRN3", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);
		}
	}

	public void TestMasterCustomsStatusNotUpdated()
	{
		NctsHeader masterNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;

		AssertProcessMessage(
			(correlationIdentifier) => masterNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", ZString.Empty, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header);
			CreateChildMovement(header, "MRN1", ZString.Empty);
		}
	}

	public void TestDangerousGoodsSubstance()
	{
		NctsHeader initialNctsHeader = null;
		SampleMovementReferenceData sampleMovementReferenceData = null;

		var undgSubstance = Factory.New<UNDGSubstance>();
		undgSubstance.DG_UNNO = "0004";

		var undgSubstance2 = Factory.New<UNDGSubstance>();
		undgSubstance2.DG_UNNO = "0001";
		Factory.Save();

		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber().Replace("<TIRHolderIdentificationNumber>!</TIRHolderIdentificationNumber>", ""),
			(ediMessage) =>
			{
				var undgs = initialNctsHeader.Bills.FirstOrDefault().ArrivalGoodsItems.FirstOrDefault().UNDGs;
				AssertEquals("GoodsItem Commodity DangerousGoods UNNumber", undgSubstance.PK, undgs.FirstOrDefault().Substance.PK);
				AssertEquals("Dangerous Goods collection doesn't contain errors", false, undgs.HasErrors());
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			sampleMovementReferenceData = PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestJobDocAddressEmptyAddressAndCompanyName()
	{
		NctsHeader initialNctsHeader = null;
		var xmlNT043 = XDocument.Parse(GetResponseMessageWithMrnAndIdentificationNumber().Replace("<TIRHolderIdentificationNumber>!</TIRHolderIdentificationNumber>", ""));
		xmlNT043.Root.Element("Consignment").Element("Consignee").Remove();
		xmlNT043.Root.Element("Consignment").Element("Consignor").Remove();
		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => xmlNT043.ToString(),
			(ediMessage) =>
			{
				AssertJobDocAddressDoesNotGetFilledWhenNotRequired(initialNctsHeader);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestGoodsMeasureGrossMassOptional()
	{
		NctsHeader initialNctsHeader = null;
		var xmlNT043 = XDocument.Parse(GetResponseMessageWithMrnAndIdentificationNumber());
		xmlNT043.Root.Element("Consignment").Element("HouseConsignment").Element("ConsignmentItem").Element("Commodity").Element("GoodsMeasure").Element("grossMass").Remove();
		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => xmlNT043.ToString(),
			(ediMessage) =>
			{
				var nctsBill1 = initialNctsHeader.Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == "1");
				var goodsItem = nctsBill1.ArrivalGoodsItems.FirstOrDefault();

				AssertEquals("When GrossMass is not defined, GrossWeight should be: ", ZDecimal.Zero, goodsItem.BY_GrossWeight);
				AssertEquals("When GrossMass is not defined, GrossWeightUnit should be by default KG: ", Core.Constants.Weight.Kilograms, goodsItem.BY_GrossWeightUnit);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestGoodsMeasureNetMassOptional()
	{
		NctsHeader initialNctsHeader = null;
		var xmlNT043 = XDocument.Parse(GetResponseMessageWithMrnAndIdentificationNumber());
		xmlNT043.Root.Element("Consignment").Element("HouseConsignment").Element("ConsignmentItem").Element("Commodity").Element("GoodsMeasure").Element("netMass").Remove();
		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => xmlNT043.ToString(),
			(ediMessage) =>
			{
				var nctsBill1 = initialNctsHeader.Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == "1");
				var goodsItem = nctsBill1.ArrivalGoodsItems.FirstOrDefault();

				AssertEquals("When NetMass is not defined, NetWeight should be: ", ZDecimal.Zero, goodsItem.BY_NetWeight);
				AssertEquals("When NetMass is not defined, NetWeightUnit should be by default KG: ", Core.Constants.Weight.Kilograms, goodsItem.BY_NetWeightUnit);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestGoodsMeasureOptional()
	{
		NctsHeader initialNctsHeader = null;
		var xmlNT043 = XDocument.Parse(GetResponseMessageWithMrnAndIdentificationNumber());
		xmlNT043.Root.Element("Consignment").Element("HouseConsignment").Element("ConsignmentItem").Element("Commodity").Element("GoodsMeasure").Remove();
		AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => xmlNT043.ToString(),
			(ediMessage) =>
			{
				var nctsBill1 = initialNctsHeader.Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == "1");
				var goodsItem = nctsBill1.ArrivalGoodsItems.FirstOrDefault();

				AssertEquals("When NetMass is not defined, NetWeight should be: ", ZDecimal.Zero, goodsItem.BY_NetWeight);
				AssertEquals("When NetMass is not defined, NetWeightUnit should be by default KG: ", Core.Constants.Weight.Kilograms, goodsItem.BY_NetWeightUnit);
				AssertEquals("When GrossMass is not defined, GrossWeight should be: ", ZDecimal.Zero, goodsItem.BY_GrossWeight);
				AssertEquals("When GrossMass is not defined, GrossWeightUnit should be by default KG: ", Core.Constants.Weight.Kilograms, goodsItem.BY_GrossWeightUnit);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, additionalMovementReference: false);
			header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		}
	}

	public void TestMultiMRN_NoChangesToReport()
	{
		var department = Factory.New<GlbDepartment>();

		NctsHeader initialNctsHeader = null;
		var mrnValue1 = "22CHVL2525YYN7IZJ7";
		var mrnValue2 = "22CHVL2525YYN7IZJ8";
		var mrnValue3 = "22CHVL2525YYN7IZJ9";

		RunAssertions(mrnValue1, true);
		RunAssertions(mrnValue2, false);
		RunAssertions(mrnValue3, true);

		void RunAssertions(ZString mrnValue, bool expectedNoChangesToReport)
		{
			AssertProcessMessage(
			(correlationIdentifier) => initialNctsHeader ??= Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
			(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(mrnValue),
			(ediMessage) =>
			{
				var newNctsHeader = (NctsHeader)ediMessage.EM_LinkedObject;
				Factory.Save();
				AssertNotNull("New movement created", newNctsHeader);
				AssertMovementNoChangesToReport(newNctsHeader.ArrivalMovementHeader, expectedNoChangesToReport);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK,
			assertEDIMessageLinked: false);
		}

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovementWithMultiMRNAndCSIStatus(header, mrnValue1, string.Empty, mrnValue2, YesNoList.Codes.No, mrnValue3, YesNoList.Codes.Yes);
		}
	}

	public void TestSingleMRNWithEmtpyBM_StateOfSeals_NoChangesToReport()
	{
		NctsHeader initialNctsHeader = null;

		AssertProcessMessage(
		(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
		(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
		(ediMessage) =>
		{
			Factory.Save();
			AssertMovementNoChangesToReport(initialNctsHeader.ArrivalMovementHeader, true);
		},
		expectedMessageStatus: EDIMessage.Status.ProcessedOK,
		assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, status: string.Empty, additionalMovementReference: false);
		}
	}

	public void TestSingleMRNWithNoBM_StateOfSeals_NoChangesToReport()
	{
		NctsHeader initialNctsHeader = null;

		AssertProcessMessage(
		(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
		(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
		(ediMessage) =>
		{
			Factory.Save();
			AssertMovementNoChangesToReport(initialNctsHeader.ArrivalMovementHeader, false);
		},
		expectedMessageStatus: EDIMessage.Status.ProcessedOK,
		assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, status: YesNoList.Codes.No, additionalMovementReference: false);
		}
	}

	public void TestSingleMRNWithYesBM_StateOfSeals_NoChangesToReport()
	{
		NctsHeader initialNctsHeader = null;

		AssertProcessMessage(
		(correlationIdentifier) => initialNctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, prepareNctsHeader: PrepareMovement).nctsHeader,
		(correlationIdentifier) => GetResponseMessageWithMrnAndIdentificationNumber(),
		(ediMessage) =>
		{
			Factory.Save();
			AssertMovementNoChangesToReport(initialNctsHeader.ArrivalMovementHeader, true);
		},
		expectedMessageStatus: EDIMessage.Status.ProcessedOK,
		assertEDIMessageLinked: false);

		void PrepareMovement(NctsHeader header)
		{
			PrepareInitialMovement(header, status: YesNoList.Codes.Yes, additionalMovementReference: false);
		}
	}

	public void AssertMovementNoChangesToReport(NctsArrivalMovementHeader arrivalMovementHeader, bool expectedValue) => AssertEquals("BM_NoChangesToReport", expectedValue, arrivalMovementHeader.BM_NoChangesToReport);

	void CreateChildMovement(NctsHeader masterNctsHeader, string mrn, string customsStatus)
	{
		var mr = masterNctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mr.CSI_ReferenceNumber = mrn;
		var childNctsHeader = Factory.New<NctsHeader>();
		childNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		childNctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
		childNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = false;
		masterNctsHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childNctsHeader.ArrivalMovementHeader);
		childNctsHeader.MovementReferenceNumberSetter(mrn);
	}

	NctsHeader LoadRelatedNctsHeader(NctsHeader initialNctsHeader)
	{
		NctsHeader relatedNctsHeader = null;
		var pivot = GenPivot.LoadRelation1Pivot(initialNctsHeader.ArrivalMovementHeader, GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot);
		if (pivot != null)
		{
			var newMovement = (NctsArrivalMovementHeader)pivot.Relation2Object;
			relatedNctsHeader = newMovement.Header;
		}
		return relatedNctsHeader;
	}

	SampleMovementReferenceData PrepareInitialMovement(NctsHeader header, bool additionalMovementReference = true, string status = YesNoList.Codes.Yes)
	{
		BasePrepareInitialMovement(header);
		if (additionalMovementReference)
		{
			header.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = "22CHVL2525YYN7IZJ7";
		}
		var movementReferenceNumber = header.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		movementReferenceNumber.CSI_ReferenceNumber = SampleMRN;
		movementReferenceNumber.CSI_Status = status;
		movementReferenceNumber.CSI_Description = "add.info";
		return new SampleMovementReferenceData(movementReferenceNumber);
	}

	void BasePrepareInitialMovement(NctsHeader header)
	{
		header.BH_CommunicationLanguage = SwissCustomsLanguageList.Codes.Italian;
		header.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		header.ArrivalMovementHeader.BM_PaperlessInbondNum = "IB100";
		header.ArrivalMovementHeader.BM_ArrivalDate = new ZDateTime(2023, 5, 18);
		header.ArrivalMovementHeader.BM_TransportAtArrivalType = "TT";
		header.ArrivalMovementHeader.BM_TransportAtArrivalID = "TI";
		header.ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality = "TN";
		header.ArrivalMovementHeader.GoodsLocation.CGL_Qualifier = "Y";
		header.ArrivalMovementHeader.GoodsLocation.CGL_Type = "C";
		header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "add.id";
		header.ArrivalMovementHeader.GoodsLocation.Address.IdentificationHolderPK = ZGuid.NewZGuid();
		header.ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = "goodsloc.auth";
		header.DestinationTrader.E2_OA_Address = CreateAddress("DEST").PK;
		header.Principal.E2_AddressOverride = true;
		header.Principal.DocAddressNumbers.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Core.Constants.CountryCodes.Switzerland).E2N_Number = "1";
		header.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "1");
		header.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "2");
		header.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "3");
		header.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "4");
		header.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "5");
	}

	void PrepareInitialMovementWithMultiMRNAndCSIStatus(NctsHeader header, string mrnNumber1, string mrnStatus1, string mrnNumber2, string mrnStatus2, string mrnNumber3, string mrnStatus3)
	{
		BasePrepareInitialMovement(header);
		var mrn1 = header.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn1.CSI_ReferenceNumber = mrnNumber1;
		mrn1.CSI_Status = mrnStatus1;

		var mrn2 = header.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn2.CSI_ReferenceNumber = mrnNumber2;
		mrn2.CSI_Status = mrnStatus2;

		var mrn3 = header.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn3.CSI_ReferenceNumber = mrnNumber3;
		mrn3.CSI_Status = mrnStatus3;
}

	void AssertNewMovement(NctsHeader newNctsHeader, SampleMovementReferenceData sampleMovementReferenceData)
	{
		var initialNctsHeader = sampleMovementReferenceData.Header;
		AssertEquals("BH_HeaderType", NctsMovementType.Codes.Arrival, newNctsHeader.BH_HeaderType);
		AssertEquals("BH_CommunicationLanguage", initialNctsHeader.BH_CommunicationLanguage, newNctsHeader.BH_CommunicationLanguage);
		AssertEquals("BM_PaperlessInbondNum", initialNctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum, newNctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		AssertEquals("BM_ArrivalDate", initialNctsHeader.ArrivalMovementHeader.BM_ArrivalDate, newNctsHeader.ArrivalMovementHeader.BM_ArrivalDate);
		AssertEquals("BM_TransportAtArrivalType", initialNctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalType, newNctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalType);
		AssertEquals("BM_TransportAtArrivalID", initialNctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalID, newNctsHeader.ArrivalMovementHeader.BM_TransportAtArrivalID);
		AssertEquals("BM_RN_NKTransportAtArrivalIDNationality", initialNctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality, newNctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality);
		AssertEquals("CGL_Qualifier", initialNctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_Qualifier, newNctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_Qualifier);
		AssertEquals("CGL_Type", initialNctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_Type, newNctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_Type);
		AssertEquals("GoodsLocation.AdditionalIdentifier", initialNctsHeader.ArrivalMovementHeader.GoodsLocation.AdditionalIdentifier, newNctsHeader.ArrivalMovementHeader.GoodsLocation.AdditionalIdentifier);
		AssertEquals("GoodsLocation.IdentificationHolderPK", initialNctsHeader.ArrivalMovementHeader.GoodsLocation.Address.IdentificationHolderPK, newNctsHeader.ArrivalMovementHeader.GoodsLocation.Address.IdentificationHolderPK);
		AssertEquals("GoodsLocation.AuthorisationNumber", initialNctsHeader.ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber, newNctsHeader.ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber);
		AssertEquals("DestinationTrader.E2_OA_Address", initialNctsHeader.DestinationTrader.E2_OA_Address, newNctsHeader.DestinationTrader.E2_OA_Address);
		Helper.AssertEvent("JobOpen event", newNctsHeader.Logs, expectedEvent: Events.JobOpen, expectedReference: sampleMovementReferenceData.MRN);
	}

	void AssertMovementUpdated(NctsHeader nctsHeader, SampleMovementReferenceData sampleMovementReferenceData)
	{
		AssertEquals("MultipleMRNIndicator", ZBool.False, nctsHeader.ArrivalMovementHeader.MultipleMRNIndicator);
		AssertEquals("CE_EntryNum", sampleMovementReferenceData.MRN, nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
		AssertEquals("BM_StateOfSeals", sampleMovementReferenceData.StateOfSeals, nctsHeader.ArrivalMovementHeader.BM_StateOfSeals);
		AssertEquals("BM_AdditionalText", sampleMovementReferenceData.AdditionalText, nctsHeader.ArrivalMovementHeader.BM_AdditionalText);
		AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
		AssertEquals("BM_Phase", NCTS5ArrivalPhaseList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
	}

	void AssertHeaderUpdated(NctsHeader nctsHeader, bool hasTIRHolderIdentificationNumber = true)
	{
		AssertEquals("Transit Operation - Declaration Type", "T2", nctsHeader.ArrivalMovementHeader.BM_InBondEntryType);
		AssertEquals("Transit Operation - Reduced Dataset Indicator", true, nctsHeader.ArrivalMovementHeader.BM_ReducedDatasetIndicator);
		AssertEquals("Transit Operation - Security Code", NctsTypeOfSecurityList.Codes.EXI, nctsHeader.ArrivalMovementHeader.BM_TypeOfSecurity);
		AssertEquals("Transit Operation - Specific Circumstance Indicator", "A20", nctsHeader.ArrivalMovementHeader.BM_SpecificCircumstance);

		AssertEquals("Customs Office - Count", 5, nctsHeader.ArrivalMovementHeader.CustomsOffices.Count);

		var customsOfficeOfDeparture = nctsHeader.ArrivalMovementHeader.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		AssertEquals("Customs Office of Departure", "CH001253", customsOfficeOfDeparture.CY_Data);

		var customsOfficeOfDestinationDeclared = nctsHeader.ArrivalMovementHeader.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		AssertEquals("Customs Office of Destination Declared", "IT044104", customsOfficeOfDestinationDeclared.CY_Data);

		var customsOfficeOfDestinationActual = nctsHeader.ArrivalMovementHeader.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		AssertEquals("Customs Office of Destination Actual", "IT044104", customsOfficeOfDestinationActual.CY_Data);

		var customsOfficeOfTransitDeclareds = nctsHeader.ArrivalMovementHeader.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).OrderBy(x => x.CY_Order).ToArray();
		AssertEquals("Customs Office of Transit Declared - Sequence Number", new ZShort(1), customsOfficeOfTransitDeclareds[0].CY_Order);
		AssertEquals("Customs Office of Transit Declared - Arrival Date and Time", new ZDateTime(2022, 11, 15, 17, 52, 58), customsOfficeOfTransitDeclareds[0].CY_Date);
		AssertEquals("Customs Office of Transit Declared", "CH001253", customsOfficeOfTransitDeclareds[0].CY_Data);
		AssertEquals("Customs Office of Transit Declared - Sequence Number", new ZShort(2), customsOfficeOfTransitDeclareds[1].CY_Order);
		AssertEquals("Customs Office of Transit Declared - Arrival Date and Time", ZDateTime.Empty, customsOfficeOfTransitDeclareds[1].CY_Date);
		AssertEquals("Customs Office of Transit Declared", "CH001254", customsOfficeOfTransitDeclareds[1].CY_Data);

		var holderOfTransitProcedure = nctsHeader.Principal;
		AssertEquals("Address Validation Manually Verified", AddressValidationStatus.ManuallyVerified, holderOfTransitProcedure.E2_ValidationStatus);
		AssertEquals("Holder of Transit Procedure Identification Number", "1000037844", holderOfTransitProcedure.E2_GovRegNum);
		AssertEquals("Holder of Transit Procedure - Name", "Hans Meyer", holderOfTransitProcedure.E2_CompanyName);

		AssertEquals("Holder of Transit Procedure Address - C/O", "T.Meyer", holderOfTransitProcedure.E2_AdditionalAddressInformation);
		AssertEquals("Holder of Transit Procedure Address - City", "Sitten", holderOfTransitProcedure.E2_City);
		AssertEquals("Holder of Transit Procedure Address - Country", "CH", holderOfTransitProcedure.E2_RN_NKCountryCode);
		AssertEquals("Holder of Transit Procedure Address - Postcode", "1950", holderOfTransitProcedure.E2_Postcode);
		AssertEquals("Holder of Transit Procedure Address - Street and Number", "Bergstrasse", holderOfTransitProcedure.E2_Address1);

		if (hasTIRHolderIdentificationNumber)
		{
			AssertEquals("DocAddressNumber - Count", 1, holderOfTransitProcedure.DocAddressNumbers.Count);
			var docAddressNumber = holderOfTransitProcedure.DocAddressNumbers.FindFirstByNumberType("TIR");
			AssertEquals("DocAddressNumber - Country Code", Core.Constants.CountryCodes.Switzerland, docAddressNumber.E2N_RN_NKCountryCode);
			AssertEquals("DocAddressNumber - Number", "!", docAddressNumber.E2N_Number);
		}
		else
		{
			AssertEquals("DocAddressNumber - Count", 0, holderOfTransitProcedure.DocAddressNumbers.Count);
		}

		AssertConsignmentUpdated(nctsHeader);
		AssertIncidentsAdded(nctsHeader);
	}

	void AssertConsignmentUpdated(NctsHeader nctsHeader)
	{
		var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		AssertEquals("Country of Destination", "CH", arrivalMovementHeader.BM_RL_NKDestinationPort);
		AssertEquals("Country of Dispatch", "DE", arrivalMovementHeader.BM_RN_NKCountryOfDispatch);
		AssertEquals("Gross Mass (total)", 0m, arrivalMovementHeader.BM_GrossWeight);
		AssertEquals("Mode of Transport at the Border", "1", arrivalMovementHeader.BM_ExportTransportMode);
		AssertEquals("Mode of Transport Inland", "t", arrivalMovementHeader.BM_InlandTransportMode);
		AssertEquals("Reference Number UCR", "UCR/ 4561231234-5", arrivalMovementHeader.BM_UniqueConsignmentReference);

		var carrier = arrivalMovementHeader.Carrier;
		AssertEquals("Carrier Address Override", true, carrier.E2_AddressOverride);
		AssertEquals("Carrier Validation Status", AddressValidationStatus.ManuallyVerified, carrier.E2_ValidationStatus);
		AssertEquals("Carrier Identifiation Number", "1000037842", carrier.E2_GovRegNum);

		AssertJobDocAddress(nctsHeader.Consignor, "1000037841", "H. Muster", "T.Meyer", "Bergstrasse", "1950", "Sitten", "CH");
		AssertJobDocAddress(nctsHeader.Consignee, "1000037843", "Hans Meyer", "T.Meyer", "Bergstrasse", "1950", "Sitten", "CH");

		var supplyChainActors = nctsHeader.CusSupplyChainActors[0];
		AssertEquals("SupplyChainActor - Identification Number", "ASCA-25621459", supplyChainActors.CFR_Reference);
		AssertEquals("SupplyChainActor - Role", "WH", supplyChainActors.CFR_Code);

		var headerContainer = nctsHeader.ArrivalHeaderContainers[0];
		AssertEquals("HeaderContainer - SequenceNumber", new ZShort(1), headerContainer.BC_SequenceNumber);
		AssertEquals("HeaderContainer - UnloadingState", NctsUnloadedStateList.Codes.DEC, headerContainer.BC_UnloadedState);
		AssertEquals("HeaderContainer - ContainerNum", "CSQU3054383", headerContainer.BC_ContainerNum);
		AssertEquals("HeaderContainer - BC_Mode", Core.Constants.ContainerModes.Containerised, headerContainer.BC_Mode);
		var headerContainerSeal = headerContainer.Seals[0];
		AssertEquals("HeaderContainerSeal - UloadingState", NctsUnloadedStateList.Codes.DEC, headerContainerSeal.BK_UnloadingState);
		AssertEquals("HeaderContainerSeal - ContainerNum", new ZShort(1), headerContainerSeal.BK_SequenceNumber);
		AssertEquals("HeaderContainerSeal - SequenceNumber", "F742", headerContainerSeal.BK_SealNumber);

		AssertNotNull("ArrivalTransportMeans", arrivalMovementHeader.ArrivalTransportInfos.FirstOrDefault(x => x.TPM_SequenceNumber == new ZShort(1)
			&& x.TPM_TransportState == NctsUnloadedStateList.Codes.DEC
			&& x.TPM_RN_NKTransportNationality == "DE"
			&& x.TPM_IdentificationNumber == "MK A 1234"
			&& x.TPM_TypeOfIdentification == "30"));

		AssertNotNull("ActiveBorderTransportMeans", arrivalMovementHeader.ArrivalTransportInfos.FirstOrDefault(x => x.TPM_SequenceNumber == new ZShort(2)
			&& x.TPM_TransportState == NctsUnloadedStateList.Codes.DEC
			&& x.TPM_RN_NKTransportNationality == "CH"
			&& x.TPM_IdentificationNumber == "BE-123457"
			&& x.TPM_TypeOfIdentification == "30"
			&& x.TPM_ReferenceNumber == "XX291224"
			&& x.TPM_CustomsOffice == "CH003081"));

		var countryOfRouting = nctsHeader.CountriesOfRouting.FirstOrDefault();
		AssertEquals("CountryOfRouting - Sequence Number", new ZShort(1), countryOfRouting.CY_Order);
		AssertEquals("CountryOfRouting - Country", "AT", countryOfRouting.CY_Data);

		AssertEquals("PlaceOfLoading - Location", "Altena", arrivalMovementHeader.BM_PlaceOfLoading);
		AssertEquals("PlaceOfLoading - UNLOCODE", "ARGEO", arrivalMovementHeader.BM_PortOfPresentationCode);

		AssertEquals("PlaceOfUnloading - Location", "Bern", arrivalMovementHeader.BM_PlaceOfUnloading);
		AssertEquals("PlaceOfUnloading - UNLOCODE", "ARGEO", arrivalMovementHeader.BM_ForeignDestPortKCode);

		var previousDocument = nctsHeader.PreviousDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(previousDocument, 1, 0, "N825", "99IT9876AB889012A9", "Internal Community TD");

		var supportingDocument = arrivalMovementHeader.SupportingDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(supportingDocument, 1, 0, "N825", "56788", "example information");

		var transportDocument = arrivalMovementHeader.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault();
		AssertNctsAdditionalInfo(transportDocument, 1, "N825", "99IT9876AB889012A9", true);

		var additionalReference = arrivalMovementHeader.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalReference, 1, "Y001", "AREF:124987-4", true);

		var additionalInformation = arrivalMovementHeader.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalInformation, 1, "20100", "Export from one EFTA country...", false);

		AssertEquals("TransportCharges - Method of Payment", "A", arrivalMovementHeader.BM_MethodOfPayment);

		AssertHouseConsignmentUpdated(nctsHeader.Bills);
	}

	public void AssertHouseConsignmentUpdated(NctsBillCollection nctsBills)
	{
		var nctsBill1 = nctsBills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == "1");
		AssertNotNull("Bill with MovementDetail.B9_SeqNo = '1' not found", nctsBill1);
		AssertEquals("Bill1.MovementDetail - B9_UnloadedState", NctsUnloadedStateList.Codes.DEC, nctsBill1.MovementDetail.B9_UnloadedState);
		AssertEquals("Bill1 - B0_RN_NKCountryOfExport", "DE", nctsBill1.B0_RN_NKCountryOfExport);
		AssertHouseConsignmentCountryOfDestination(nctsBill1);
		AssertEquals("Bill1 - B0_Weight", 1000.0m, nctsBill1.B0_Weight);
		AssertEquals("Bill1 - B0_WeightUQ", Core.Constants.Weight.Kilograms, nctsBill1.B0_WeightUQ);
		AssertEquals("Bill1 - B0_SecurityIndicatorFromExport", true, nctsBill1.B0_SecurityIndicatorFromExport);

		AssertJobDocAddress(nctsBill1.MovementDetail.ConsignorDocAddress, "1000037844", "Hans Muster", "T.Meyer", "Bergstrasse 7", "1950", "Sitten", "CH");
		AssertJobDocAddress(nctsBill1.MovementDetail.ConsigneeDocAddress, "1000037845", "Hans Meyer", "T.Meyer", "Bergstrasse", "1950", "Sitten", "CH");

		var additionalSupplyChainActor = nctsBill1.CusSupplyChainActorReferences[0];
		AssertEquals("CusSupplyChainActorReferences - CFR_Code", "WH", additionalSupplyChainActor.CFR_Code);
		AssertEquals("CusSupplyChainActorReferences - CFR_Reference", "ASCA-25621459", additionalSupplyChainActor.CFR_Reference);

		var departureTransportMeans = nctsBill1.DepartureTransportInfos.FirstOrDefault();
		AssertEquals("DepartureTransportInfos - TPM_SequenceNumber", new ZShort(1), departureTransportMeans.TPM_SequenceNumber);
		AssertEquals("DepartureTransportInfos - TPM_IdentificationNumber", "MK A 1234", departureTransportMeans.TPM_IdentificationNumber);
		AssertEquals("DepartureTransportInfos - TPM_TypeOfIdentification", "30", departureTransportMeans.TPM_TypeOfIdentification);
		AssertEquals("DepartureTransportInfos - TPM_RN_NKTransportNationality", "DE", departureTransportMeans.TPM_RN_NKTransportNationality);
		AssertEquals("DepartureTransportInfos - TPM_TransportState", NctsUnloadedStateList.Codes.DEC, departureTransportMeans.TPM_TransportState);

		var previousDocument = nctsBill1.PreviousDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(previousDocument, 1, 0, "N825", "99IT9876AB889012A9", "Internal Community TD");

		var supportingDocument = nctsBill1.SupportingDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(supportingDocument, 1, 42, "N825", "56788", "example information");

		var transportDocument = nctsBill1.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault();
		AssertNctsAdditionalInfo(transportDocument, 1, "N825", "99IT9876AB889012A9", true);

		var additionalReference = nctsBill1.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalReference, 1, "Y001", "AREF:124987-4", true);

		var additionalInformation = nctsBill1.AdditionalDocuments?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalInformation, 1, "20100", "Export from one EFTA country...", false);

		var nctsBill2 = nctsBills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == "2");
		AssertNotNull("Bill with MovementDetail.B9_SeqNo = '2' not found", nctsBill2);
		AssertEquals("Bill2 - B0_Weight", 0.02m, nctsBill2.B0_Weight);
		AssertEquals("Bill2 - B0_WeightUQ", Core.Constants.Weight.Grams, nctsBill2.B0_WeightUQ);
		AssertEquals("Bill2 - B0_SecurityIndicatorFromExport", false, nctsBill2.B0_SecurityIndicatorFromExport);

		var consignmentItem = nctsBill1.ArrivalGoodsItems?.FirstOrDefault();
		AssertConsignmentItem(consignmentItem);
	}

	protected abstract void AssertHouseConsignmentCountryOfDestination(NctsBill bill);

	public void AssertConsignmentItem(NctsArrivalCargoDesc goodsItem)
	{
		var goodsItem2 = goodsItem.Bill.ArrivalGoodsItems[1];

		AssertEquals("GoodsItem GoodsItemNumber", (short)1, goodsItem.BY_LineNo);
		AssertEquals("GoodsItem UnloadedState", NctsUnloadedStateList.Codes.DEC, goodsItem.BY_UnloadedState);
		AssertEquals("GoodsItem DeclarationGoodsItemNumber", 1, goodsItem.BY_DeclarationGoodsItemNumber);
		AssertEquals("GoodsItem Description", "Excavator Spare Parts", goodsItem.BY_Description);
		AssertEquals("GoodsItem Type", "T2", goodsItem.BY_Type);
		AssertEquals("GoodsItem CommercialReferenceNumber UCR", "UCR/ 4561231234-5", goodsItem.BY_CommercialReferenceNumber);
		AssertEquals("GoodsItem CountryOfDestination", "CH", goodsItem.BY_RN_NKCountryOfDestination);
		AssertEquals("GoodsItem CountryOfDispatch", "DE", goodsItem.BY_RN_NKCountryOfDispatch);
		AssertEquals("GoodsItem TransportChargesMethodOfPayment", "A", goodsItem.BY_TransportChargesMethodOfPayment);

		AssertJobDocAddress(goodsItem.ConsigneeDocAddress, "1000037846", "Hans Meyer", "T.Meyer", "Bergstrasse", "1950", "Sitten", "CH");

		AssertEquals("GoodsItem AdditionalSupplyChainActor IdentificationNumber", "ASCA-25621459", goodsItem.Bill.CusSupplyChainActorReferences?.Where(r => r.CFR_ParentID == goodsItem.PK).FirstOrDefault().CFR_Reference);
		AssertEquals("GoodsItem AdditionalSupplyChainActor Role", "WH", goodsItem.Bill.CusSupplyChainActorReferences?.Where(r => r.CFR_ParentID == goodsItem.PK).FirstOrDefault().CFR_Code);

		AssertEquals("GoodsItem Commodity CUSCode", "0018113-5", goodsItem.BY_CusC4Number);
		AssertEquals("GoodsItem Commodity DescriptionOfGoods", "Excavator Spare Parts", goodsItem.BY_Description);
		AssertEquals("GoodsItem Commodity TariffNumber Control Code 123", "12341212123", goodsItem.BY_HarmonisedTariff);
		AssertEquals("GoodsItem Commodity TariffNumber Control Code missing", "12341212000", goodsItem2.BY_HarmonisedTariff);

		AssertEquals("GoodsItem Commodity DangerousGoods UNNumber", "0004", goodsItem.UNDGs.FirstOrDefault().Substance.DG_UNNO);

		AssertEquals("GoodsItem Commodity GoodsMeasure GrossWeight KG", 1000m, goodsItem.BY_GrossWeight);
		AssertEquals("GoodsItem Commodity GoodsMeasure GrossWeightUnit KG", "KG", goodsItem.BY_GrossWeightUnit);
		AssertEquals("GoodsItem Commodity GoodsMeasure NetWeight KG", 1000m, goodsItem.BY_NetWeight);
		AssertEquals("GoodsItem Commodity GoodsMeasure NetWeightUnit KG", "KG", goodsItem.BY_NetWeightUnit);

		AssertEquals("GoodsItem Commodity GoodsMeasure GrossWeight G", 0.08m, goodsItem2.BY_GrossWeight);
		AssertEquals("GoodsItem Commodity GoodsMeasure GrossWeightUnit G", "G", goodsItem2.BY_GrossWeightUnit);
		AssertEquals("GoodsItem Commodity GoodsMeasure NetWeight G", 0.08m, goodsItem2.BY_NetWeight);
		AssertEquals("GoodsItem Commodity GoodsMeasure NetWeightUnit G", "G", goodsItem2.BY_NetWeightUnit);

		var package = goodsItem.Packages[0];
		AssertEquals("GoodsItem Package SequenceNumber", (short)1, package.B5_SequenceNumber);
		AssertEquals("GoodsItem Package UnloadedState", NctsUnloadedStateList.Codes.DEC, package.B5_TypeOfDifference);
		AssertEquals("GoodsItem Package UnitCount", 42, package.B5_UnitCount);
		AssertEquals("GoodsItem Package ShippingMarks", "AMC", package.B5_MarksAndNumbers);
		AssertEquals("GoodsItem Package UnitType", "CT", package.B5_UnitType);

		var containerPivot = (NonPersistentContainerPivotPhase5)package.ContainersPivotsForBindingOnly.FirstOrDefault();
		AssertEquals("GoodsItem Package linked to Container Selected", true, containerPivot.ContainerSelected);
		AssertEquals("GoodsItem Package linked to Container Number", "CSQU3054383", containerPivot.Container.BC_ContainerNum);

		var previousDocument = goodsItem.PreviousDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(previousDocument, 1, 0, "N825", "99IT9876AB889012A9", "Internal Community TD");

		var supportingDocument = goodsItem.SupportingDocuments.FirstOrDefault();
		AssertImportExportAwareSupportingInfo(supportingDocument, 1, 42, "N825", "56788", "example information");

		var transportDocument = goodsItem.AdditionalInfos?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).FirstOrDefault();
		AssertNctsAdditionalInfo(transportDocument, 1, "N825", "99IT9876AB889012A9", true);

		var additionalReference = goodsItem.AdditionalInfos?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalReference, 1, "Y001", "AREF:124987-4", true);

		var additionalInformation = goodsItem.AdditionalInfos?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).FirstOrDefault();
		AssertNctsAdditionalInfo(additionalInformation, 1, "20100", "Export from one EFTA country...", false);
	}

	void AssertJobDocAddress(JobDocAddress address, ZString id, ZString name, ZString careOf, ZString streetAndNumber, ZString postcode, ZString city, ZString country)
	{
		AssertEquals($"{address.E2_AddressType} Identification number", id, address.E2_GovRegNum);
		AssertEquals($"{address.E2_AddressType} Name", name, address.E2_CompanyName);
		AssertEquals($"{address.E2_AddressType} Address - C/O", careOf, address.E2_AdditionalAddressInformation);
		AssertEquals($"{address.E2_AddressType} Address - Street and Number", streetAndNumber, address.E2_Address1);
		AssertEquals($"{address.E2_AddressType} Address - Postcode", postcode, address.E2_Postcode);
		AssertEquals($"{address.E2_AddressType} Address - City", city, address.E2_City);
		AssertEquals($"{address.E2_AddressType} Address - Country", country, address.E2_RN_NKCountryCode);
	}

	void AssertNctsAdditionalInfo(AdditionalInfo additionalInfo, ZInt lineNumber, ZString code, ZString referenceNumber, bool useReferenceNumber)
	{
		AssertEquals($"{additionalInfo.CSI_SubType} - Line Number", lineNumber, additionalInfo.CSI_LineNo);
		AssertEquals($"{additionalInfo.CSI_SubType} - Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
		AssertEquals($"{additionalInfo.CSI_SubType} - Code", code, additionalInfo.CSI_Code);
		AssertEquals($"{additionalInfo.CSI_SubType} - Reference Number", referenceNumber, useReferenceNumber ? additionalInfo.CSI_ReferenceNumber : additionalInfo.CSI_Description);
	}

	void AssertImportExportAwareSupportingInfo(ImportExportAwareSupportingInfo document, ZInt lineNumber, ZInt documentLineNumber, ZString code, ZString referenceNumber, ZString referenceNumber2)
	{
		AssertEquals($"{document.CSI_Type} - Line Number", lineNumber, document.CSI_LineNo);
		AssertEquals($"{document.CSI_Type} - Status", NctsUnloadedStateList.Codes.DEC, document.CSI_Status);
		AssertEquals($"{document.CSI_Type} - Document Line Number", documentLineNumber, document.CSI_ItemNumber);
		AssertEquals($"{document.CSI_Type} - Code", code, document.CSI_Code);
		AssertEquals($"{document.CSI_Type} - Reference Number", referenceNumber, document.CSI_ReferenceNumber);
		AssertEquals($"{document.CSI_Type} - Reference Number 2", referenceNumber2, document.CSI_ReferenceNumber2);
	}

	void AssertIncidentsAdded(NctsHeader nctsHeader)
	{
		AssertEquals("BH_ExportFlag", EventFlagList.Codes.Yes, nctsHeader.BH_ExportFlag);

		AssertEquals("EnRouteIncidents - Count", 4, nctsHeader.EnRouteIncidents.Count);

		var incident = nctsHeader.EnRouteIncidents[0];
		AssertEquals("EnRouteIncidents[0].BN_Information", "example incident 1 descr", incident.BN_Information);
		AssertEquals("EnRouteIncidents[0].BN_IncidentCode", "3", incident.BN_IncidentCode);
		AssertEquals("EnRouteIncidents[0].BN_EndorsementAuthority", "Police", incident.BN_EndorsementAuthority);
		AssertEquals("EnRouteIncidents[0].BN_EndorsementCountryCode", "CH", incident.BN_EndorsementCountryCode);
		AssertEquals("EnRouteIncidents[0].BN_EndorsementDate", new ZDateTime(2022, 8, 9), incident.BN_EndorsementDate);
		AssertEquals("EnRouteIncidents[0].BN_EndorsementPlace", "Bern", incident.BN_EndorsementPlace);
		AssertEquals("EnRouteIncidents[0].BN_EventCountryCode", "CH", incident.BN_EventCountryCode);

		AssertNull("GoodsLocation not created for invalid Qualifier", Factory.LoadTop1<CusGoodsLocation>(new ZQuery(CusGoodsLocationSchema.CGL_ParentID, incident.PK)));

		AssertEquals("EnRouteIncidents[0].IncidentContainers.Count", 2, incident.IncidentContainers.Count);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].BC_Mode", "NCT", incident.IncidentContainers[0].BC_Mode);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].BC_ContainerNum", "CSQU3054383", incident.IncidentContainers[0].BC_ContainerNum);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].BC_SequenceNumber", new ZShort(1), incident.IncidentContainers[0].BC_SequenceNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].BC_Seal1", "F742", incident.IncidentContainers[0].BC_Seal1);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].BC_Seal2", "F743", incident.IncidentContainers[0].BC_Seal2);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals.Count", 3, incident.IncidentContainers[0].Seals.Count);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[0].BK_SequenceNumber", new ZShort(3), incident.IncidentContainers[0].Seals[0].BK_SequenceNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[0].BK_SealNumber", "F744", incident.IncidentContainers[0].Seals[0].BK_SealNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[1].BK_SequenceNumber", new ZShort(4), incident.IncidentContainers[0].Seals[1].BK_SequenceNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[1].BK_SealNumber", "F745", incident.IncidentContainers[0].Seals[1].BK_SealNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[2].BK_SequenceNumber", new ZShort(5), incident.IncidentContainers[0].Seals[2].BK_SequenceNumber);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].Seals[2].BK_SealNumber", "F746", incident.IncidentContainers[0].Seals[2].BK_SealNumber);
		var goodsReferences = incident.IncidentContainers[0].ItemNumbers.Where(x => x.CY_Code == "ITM").ToArray();
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].ItemNumbers.Length", 2, goodsReferences.Length);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].ItemNumbers[0].CY_Data", "3", goodsReferences[0].CY_Data);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].ItemNumbers[0].CY_Order", new ZShort(1), goodsReferences[0].CY_Order);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].ItemNumbers[1].CY_Data", "4", goodsReferences[1].CY_Data);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[0].ItemNumbers[1].CY_Order", new ZShort(2), goodsReferences[1].CY_Order);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[1].BC_Mode", "NCT", incident.IncidentContainers[0].BC_Mode);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[1].BC_ContainerNum", "CSQU3054386", incident.IncidentContainers[1].BC_ContainerNum);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[1].Seals.Count", 0, incident.IncidentContainers[1].Seals.Count);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[1].BC_Seal1", "F745", incident.IncidentContainers[1].BC_Seal1);
		AssertEquals("EnRouteIncidents[0].IncidentContainers[1].BC_Seal2", "F746", incident.IncidentContainers[1].BC_Seal2);
		AssertEquals("EnRouteIncidents[0].BN_RN_NKTransportAtDepartureIDNationality", "DE", incident.BN_RN_NKTransportAtDepartureIDNationality);
		AssertEquals("EnRouteIncidents[0].BN_TransportAtDepartureID", "MK A 1234", incident.BN_TransportAtDepartureID);
		AssertEquals("EnRouteIncidents[0].BN_TransportAtDepartureType", "30", incident.BN_TransportAtDepartureType);

		incident = nctsHeader.EnRouteIncidents[1];
		AssertEquals("EnRouteIncidents[1].GoodsLocation.CGL_Qualifier", CusGoodsLocationQualifierList.Codes.GnssCoordinates, incident.GoodsLocation.CGL_Qualifier);
		AssertEquals("EnRouteIncidents[1].GoodsLocation.CGL_LocationUse", CusGoodsLocationUseList.Codes.Arrival, incident.GoodsLocation.CGL_LocationUse);
		AssertEquals("EnRouteIncidents[1].GoodsLocation.Address.E2_GeoLocation.Longitude", 46.8510827, incident.GoodsLocation.Address.E2_GeoLocation.Longitude);
		AssertEquals("EnRouteIncidents[1].GoodsLocation.Address.E2_GeoLocation.Latitude", 7.5386324, incident.GoodsLocation.Address.E2_GeoLocation.Latitude);

		AssertEquals("EnRouteIncidents[1].IncidentContainers.Count", 1, incident.IncidentContainers.Count);
		AssertEquals("EnRouteIncidents[1].IncidentContainers[0].BC_Mode", "CNT", incident.IncidentContainers[0].BC_Mode);
		AssertEquals("EnRouteIncidents[1].IncidentContainers[0].BC_ContainerNum", "CSQU3054384", incident.IncidentContainers[0].BC_ContainerNum);

		incident = nctsHeader.EnRouteIncidents[2];
		AssertEquals("EnRouteIncidents[2].GoodsLocation.CGL_Qualifier", CusGoodsLocationQualifierList.Codes.Address, incident.GoodsLocation.CGL_Qualifier);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.CGL_LocationUse", CusGoodsLocationUseList.Codes.Arrival, incident.GoodsLocation.CGL_LocationUse);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.Address.E2_Address1", "Bergstrasse 17", incident.GoodsLocation.Address.E2_Address1);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.Address.E2_Postcode", "1950", incident.GoodsLocation.Address.E2_Postcode);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.Address.E2_City", "Sitten", incident.GoodsLocation.Address.E2_City);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.Address.E2_RN_NKCountryCode", "CH", incident.GoodsLocation.Address.E2_RN_NKCountryCode);

		AssertEquals("EnRouteIncidents[2].IncidentContainers.Count", 1, incident.IncidentContainers.Count);
		AssertEquals("EnRouteIncidents[2].IncidentContainers[0].BC_Mode", "CNT", incident.IncidentContainers[0].BC_Mode);
		AssertEquals("EnRouteIncidents[2].IncidentContainers[0].BC_ContainerNum", "CSQU3054385", incident.IncidentContainers[0].BC_ContainerNum);

		incident = nctsHeader.EnRouteIncidents[3];
		AssertEquals("EnRouteIncidents[2].GoodsLocation.CGL_Qualifier", CusGoodsLocationQualifierList.Codes.UnLocode, incident.GoodsLocation.CGL_Qualifier);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.CGL_LocationUse", CusGoodsLocationUseList.Codes.Arrival, incident.GoodsLocation.CGL_LocationUse);
		AssertEquals("EnRouteIncidents[2].GoodsLocation.CGL_AdditionalIdentifier", "ARGEO", incident.GoodsLocation.CGL_AdditionalIdentifier);
		AssertEquals("EnRouteIncidents[3].IncidentContainers.Count", 1, incident.IncidentContainers.Count);
		AssertEquals("EnRouteIncidents[3].IncidentContainers[0].", "NCT", incident.IncidentContainers[0].BC_Mode);
	}

	void AssertJobDocAddressDoesNotGetFilledWhenNotRequired(NctsHeader nctsHeader)
	{
		AssertEquals("Consingees E2_AddressOverride does not get to be set to true", false, nctsHeader.Consignee.E2_AddressOverride);
		AssertEquals("Consingees E2_GovRegNum stays empty", ZString.Empty, nctsHeader.Consignee.E2_GovRegNum);
		AssertEquals("Consingees E2_ValidationStatus stays empty", ZString.Empty, nctsHeader.Consignee.E2_GovRegNum);

		AssertEquals("Consignors E2_AddressOverride does not get to be set to true", false, nctsHeader.Consignor.E2_AddressOverride);
		AssertEquals("Consignors E2_GovRegNum stays empty", ZString.Empty, nctsHeader.Consignor.E2_GovRegNum);
		AssertEquals("Consignors E2_ValidationStatus stays empty", ZString.Empty, nctsHeader.Consignor.E2_GovRegNum);
	}

	void AssertMessageLinked(NctsHeader nctsHeader, EDIMessage ediMessage)
	{
		AssertSame(ediMessage.EM_LinkedObject, nctsHeader);
	}

	void AssertMessageIdentification(EDIMessage ediMessage)
	{
		AssertEquals(SampleMessageIdentification, ediMessage.EM_ApplicationReference);
	}

	void AssertCESEventAdded(NctsHeader nctsHeader)
	{
		Helper.AssertEvent("CES Event", nctsHeader.ArrivalMovementHeader.Logs, Events.CustomsEntryStatus, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);
	}

	OrgAddress CreateAddress(ZString code)
	{
		var org = Factory.New<OrgHeader>();
		org.OH_Code = code;
		return org.MainAddress;
	}

	protected override string ExpectedFriendlyName => "NT043 - Arrival Intentory Request Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarInventoryRequest;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT043ResponseMessageProcessor(Logger);

	protected abstract string GetResponseMessageWithMrnAndIdentificationNumber(string mrn = SampleMRN);

	protected const string SampleMRN = "23CH126525YYN9IZN6";
	protected const string SampleMessageIdentification = "dNGMtGospbQtRtCxZyRsHWIBJafPOIXDniN";

	class SampleMovementReferenceData
	{
		internal SampleMovementReferenceData(MovementReferenceNumberSupportingInfo movementReferenceNumber)
		{
			Header = movementReferenceNumber.Parent.Header;
			LineNo = movementReferenceNumber.CSI_LineNo;
			MRN = movementReferenceNumber.CSI_ReferenceNumber;
			StateOfSeals = movementReferenceNumber.CSI_Status;
			AdditionalText = movementReferenceNumber.CSI_Description;
		}
		internal NctsHeader Header { get; }
		internal ZInt LineNo { get; }
		internal ZString MRN { get; }
		internal ZString StateOfSeals { get; }
		internal ZString AdditionalText { get; }
	}
}
