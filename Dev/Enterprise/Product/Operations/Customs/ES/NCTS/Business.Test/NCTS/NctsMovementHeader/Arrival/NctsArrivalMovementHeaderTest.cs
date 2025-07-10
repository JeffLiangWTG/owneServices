using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
class NctsArrivalMovementHeaderTest : NctsArrivalMovementHeaderAbstractTest
{
	public void TestBM_InBondEntryType_MaxLegnth()
	{
		AssertEquals(3, arrivalMovement.BM_InBondEntryTypeInfo.MaxLength);
	}

	public void TestLookups()
	{
		AssertType<NctsArrivalMovementHeaderLookups>(arrivalMovement.Lookups);
	}

	public void TestValidation()
	{
		AssertType<NctsArrivalMovementHeaderValidation>(arrivalMovement.Validation);
	}

	public void TestGuaranteesForArrival()
	{
		CombineAssertions(() =>
		{
			var guarantee = arrivalMovement.GuaranteesForArrival.AddNew();
			AssertType<NctsGuarantee>("Now it returns guarantee ES type", guarantee);
		});
	}

	public void TestG4PreviousDocuments()
	{
		AssertType<G4PreviousDocumentCollection>(arrivalMovement.G4PreviousDocuments);
	}

	public void TestGetCusSupportingInfoTypes_G4PreviousDocument()
	{
		AssertEquals(typeof(G4PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)arrivalMovement).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestSupportingDocuments()
	{
		var supportingDocumentCollection = arrivalMovement.SupportingDocuments;
		var supportingDocument = supportingDocumentCollection.AddNew();

		CombineAssertions(() =>
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("Type", supportingDocumentCollection);
			AssertEquals("Collection IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(supportingDocumentCollection));
			AssertEquals("Collection IsLoaded", true, supportingDocumentCollection.IsLoaded);
			AssertEquals("Collection ReadOnly", false, supportingDocumentCollection.ReadOnly);
			AssertEquals("Document CSI_Type", Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, supportingDocument.CSI_Type);
			AssertEquals("Document Parent", arrivalMovement, supportingDocument.Parent);
		});
	}

	public void TestGetCusSupportingInfoTypes_SupportingDocuments()
	{
		AssertEquals(typeof(NctsSupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)arrivalMovement).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestAdditionalDocuments()
	{
		var additionalDocumentCollection = arrivalMovement.AdditionalDocuments;
		var additionalDocument = additionalDocumentCollection.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Collection IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(additionalDocumentCollection));
			AssertEquals("Collection IsLoaded", true, additionalDocumentCollection.IsLoaded);
			AssertEquals("Collection ReadOnly", false, additionalDocumentCollection.ReadOnly);
			AssertEquals("Document CSI_Type", Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalDocument.CSI_Type);
			AssertEquals("Document Parent", arrivalMovement, additionalDocument.Parent);
		});
	}

	public void TestGetCusSupportingInfoTypes_AdditionalDocuments()
	{
		AssertEquals(typeof(NctsAdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)arrivalMovement).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestIsUnloadingRemarksReadOnlySpain() => CombineAssertions(() =>
	{
		arrivalMovement.BM_CustomsStatus = ZString.Empty;
		AssertEquals("BM_CustomsStatus is Empty", expected: false, arrivalMovement.IsUnloadingRemarksReadOnlySpain);

		arrivalMovement.BM_CustomsStatus = "CL1";
		AssertEquals("BM_CustomsStatus is CL1", expected: true, arrivalMovement.IsUnloadingRemarksReadOnlySpain);

		arrivalMovement.BM_CustomsStatus = "CD4";
		AssertEquals("BM_CustomsStatus is CD4", expected: true, arrivalMovement.IsUnloadingRemarksReadOnlySpain);

		arrivalMovement.BM_CustomsStatus = ZString.Empty;
		arrivalMovement.Header.EffectiveMessageStatus = ZString.Empty;
		AssertEquals("BM_CustomsStatus is empty and MessageStatus empty", expected: false, arrivalMovement.IsUnloadingRemarksReadOnlySpain);

		arrivalMovement.Header.EffectiveMessageStatus = "SNT";
		AssertEquals("BM_CustomsStatus is empty and MessageStatus SNT", expected: true, arrivalMovement.IsUnloadingRemarksReadOnlySpain);
	});

	public void TestRepresentativeChangedWithTNNCopyFieldToRepresentative()
	{
		var representative1 = Factory.New<OrgHeader>();
		representative1.OH_Code = "REP1";
		var representative2 = Factory.New<OrgHeader>();
		representative2.OH_Code = "REP2";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalMovement = nctsHeader.ArrivalMovementHeader;
		nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = representative1.PK;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.ArrivalMrnFromUser = "ES123456";
		nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

		var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
		tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
		tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);

		arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Representative in arrivalMovement is representative.PK", representative1.PK, nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK);
			AssertEquals("Representative in arrivalMovement HeaderTNN is representative.PK (GenerateTNNDeparture)", representative1.PK, arrivalMovement.HeaderTNN.MovementHeader.Representative.OrganisationPK);

			nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = representative2.PK;
			AssertEquals("Representative in arrivalMovement is representative2.PK", representative2.PK, nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK);
			AssertEquals("Representative in arrivalMovement HeaderTNN representative2.PK", representative2.PK, arrivalMovement.HeaderTNN.MovementHeader.Representative.OrganisationPK);

			nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = ZGuid.Empty;
			AssertEquals("Representative in arrivalMovement is empty (Change with value to empty)", ZGuid.Empty, nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK);
			AssertEquals("Representative in arrivalMovement HeaderTNN is empty (Change with value to empty)", ZGuid.Empty, arrivalMovement.HeaderTNN.MovementHeader.Representative.OrganisationPK);
		});
	}

	public void TestBH_CustomsProfileDefaulted()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		CombineAssertions(() =>
		{
			arrivalMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			arrivalMovement.BM_GS_NKCusAgent = staff2.GS_Code;
			AssertEquals("BH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", nctsHeader.BH_CustomsProfile);

			arrivalMovement.BM_GS_NKCusAgent = staff3.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			arrivalMovement.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			AssertEquals("BH_CustomsProfile has value when broker not empty", "TestCert2", nctsHeader.BH_CustomsProfile);

			arrivalMovement.BM_GS_NKCusAgent = ZString.Empty;
			AssertEquals("BH_CustomsProfile has been cleared when broker is empty", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsHeader.BH_CustomsProfile = "TESTCERT1";
			arrivalMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", nctsHeader.BH_CustomsProfile);

			arrivalMovement.BM_GS_NKCusAgent = ZString.Empty;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			arrivalMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, nctsHeader.BH_CustomsProfile);
		});
	}

	public void TestBM_PaperlessInbondNum()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_JobReference = "Reference1";
		arrivalMovement.BM_PaperlessInbondNum = ZString.Empty;

		CombineAssertions(() =>
		{
			arrivalMovement.OnSaving();
			AssertEquals("Not populated OnSaving because is not Phase5", ZString.Empty, arrivalMovement.BM_PaperlessInbondNum);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalMovement.OnSaving();
			AssertEquals("Populated OnSaving when BM_PaperlessInbondNum is empty", "Reference1", arrivalMovement.BM_PaperlessInbondNum);

			arrivalMovement.BM_PaperlessInbondNum = "ManualReference1";
			arrivalMovement.OnSaving();
			AssertEquals("Not populated OnSaving because the value is manually changed", "ManualReference1", arrivalMovement.BM_PaperlessInbondNum);
		});
	}

	public void TestPopulateArrivalGoodsItemsFromDepartureCopyDocuments()
	{
		var headerDeparture = Factory.New<NctsHeader>();
		headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsItems = headerDeparture.MovementHeader.GoodsItems;
		var goodsItem = goodsItems.AddNew();

		goodsItem.SupportingDocuments.AddNew();
		goodsItem.SupportingDocuments.AddNew();
		goodsItem.SupportingDocuments.AddNew();

		var headerArrival = Factory.New<NctsHeader>();
		headerArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: 1 departure goods item", 1, goodsItems.Count);

			headerArrival.ArrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(headerDeparture.MovementHeader);
			var arrivalGoodsItems = headerArrival.ArrivalMovementHeader.GoodsItems;
			AssertEquals("expected 1 arrival goods item", 1, arrivalGoodsItems.Count);
			AssertEquals("expected 3 Documents", 3, arrivalGoodsItems[0].SupportingDocuments.Count);
			AssertEquals("expected all 3 Documents have line no not 0", true, arrivalGoodsItems[0].SupportingDocuments.Cast<NctsSupportingDocument>().All(x => !x.CSI_LineNo.IsEmpty));
		});
	}

	public void TestIsSimplifiedNctsProcedure_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(arrivalMovement.IsSimplifiedNctsProcedureInfo, ((ISupportMultipleResourceStringData)arrivalMovement).MultipleKeysToUse);
			AssertEquals("NCTS4 Caption", "Simplified Arrival?", DataBoundResourceStrings.GetDataForProperty(arrivalMovement.IsSimplifiedNctsProcedureInfo).Caption);
			AssertEquals("NCTS4 FullDescription", "Simplified Arrival Notification?", DataBoundResourceStrings.GetDataForProperty(arrivalMovement.IsSimplifiedNctsProcedureInfo).FullDescription);
		});
	}

	public void TestIsSimplifiedNctsProcedure_Phase5()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(arrivalMovement.IsSimplifiedNctsProcedureInfo, ((ISupportMultipleResourceStringData)arrivalMovement).MultipleKeysToUse);
			AssertEquals($"NCTS5 Caption {string.Join(",", nctsHeader.MultipleKeysToUse)}", "Simplified Procedure", data.Caption);

			arrivalMovement.IsSimplifiedNctsProcedure = ZBool.True;
			AssertEquals("IsSimplifiedNctsProcedure=true", NctsControlResult.Codes.AuthorizedTrader, arrivalMovement.BM_GONumber);
			arrivalMovement.IsSimplifiedNctsProcedure = ZBool.False;
			AssertEquals("IsSimplifiedNctsProcedure=false", ZString.Empty, arrivalMovement.BM_GONumber);
		});
	}

	public void TestGenerateTNNDeparture()
	{
		var destinationTrader = Factory.New<OrgHeader>();
		destinationTrader.OH_Code = "TRA1";
		var representative = Factory.New<OrgHeader>();
		representative.OH_Code = "REP1";
		var location = "aaaa";

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMrnFromUser = "AH3";
			var esNctsHeader = nctsHeader.ESNctsHeader;
			esNctsHeader.CEN_TNNArrival = true;

			arrivalMovement.Representative.OrganisationPK = representative.PK;
			nctsHeader.DestinationTrader.OrganisationPK = destinationTrader.PK;
			var goodsLocation = arrivalMovement.GoodsLocation;
			goodsLocation.CGL_AdditionalIdentifier = location;

			var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
			tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
			tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);

			arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
			Factory.Save();

			var (_, headerFound) = nctsHeader.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
			AssertNull("Action not performed when TNNArrival = true", headerFound);

			esNctsHeader.CEN_TNNArrival = false;

			arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
			Factory.Save();

			(_, headerFound) = nctsHeader.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
			var departureHeader = (NctsHeader)headerFound;
			var tnnMovement = departureHeader.MovementHeader;

			AssertEquals("TNNArrival set to true for Arrival", expected: true, esNctsHeader.CEN_TNNArrival);
			AssertEquals("BM_BM_DepartureMovement is set", tnnMovement.PK, arrivalMovement.BM_BM_DepartureMovement);

			AssertNotNull("Departure is available", departureHeader);
			AssertEquals("TNNArrival set to true", expected: true, departureHeader.ESNctsHeader.CEN_TNNArrival);
			AssertEquals("Declaration is Departure", "D", departureHeader.BH_HeaderType);
			AssertEquals("Same MRN", "AH3", departureHeader.MovementReferenceNumber);
			AssertEquals("Phase 5", "NC5", departureHeader.BH_ApplicationCode);
			AssertEquals("Acceptance date in EntryNumber", tnnDataCodeInfo.AcceptanceDate, departureHeader.AcceptanceDate);
			AssertEquals("Clearance date in EntryNumber", tnnDataCodeInfo.ClearanceDate, departureHeader.ClearanceDate);

			AssertEquals("DestinationTrader's values in TNN Departure values", destinationTrader.PK, departureHeader.Consignee.OrganisationPK);

			AssertEquals("Phase set to TNN", "TNN", tnnMovement.BM_Phase);
			AssertEquals("Representative's values in TNN Departure values", representative.PK, tnnMovement.Representative.OrganisationPK);
			AssertEquals("GoodsLocation's values in TNN Departure values", location, tnnMovement.GoodsLocation.CGL_AdditionalIdentifier);
		});
	}

	public void TestHeaderTNN()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.ArrivalMrnFromUser = "1234567890";

		Factory.Save();

		CombineAssertions(() =>
		{
			var arrivalMovement = arrivalHeader.ArrivalMovementHeader;
			var matchingTNN = arrivalMovement.HeaderTNN;
			AssertNull("It hasn't a tnn with Arrival MRN", matchingTNN);

			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";
			var tnnMovement = departure.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			matchingTNN = arrivalMovement.HeaderTNN;
			AssertNull("It has a tnn with Arrival MRN but Arrival is not marked as CEN_TNNArrival", matchingTNN);

			arrivalHeader.ESNctsHeader.CEN_TNNArrival = true;
			matchingTNN = arrivalMovement.HeaderTNN;
			AssertNull("It has a tnn with Arrival MRN + CEN_TNNArrival but Arrival has no BM_BM_DepartureMovement", matchingTNN);

			arrivalMovement.BM_BM_DepartureMovement = tnnMovement.PK;
			matchingTNN = arrivalMovement.HeaderTNN;
			AssertNotNull("It has a departure with Arrival BM_BM_DepartureMovement", matchingTNN);
			AssertEquals("HeaderTNN should be marked as ChildEditable ", expected: true, arrivalMovement.IsRegisteredEditableChildObject(matchingTNN));
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_All_FinalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AddUnloadingDataToArrival(arrivalHeader);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		AddDataToDepartureForUnloading(departure);

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				AssertEquals("Prereq: departure cont1.BC_SequenceNumber", (ZShort)1, departure.DepartureHeaderContainers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1").BC_SequenceNumber);
				AssertEquals("Prereq: departure cont2.BC_SequenceNumber", (ZShort)2, departure.DepartureHeaderContainers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2").BC_SequenceNumber);
				AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departure.Bills.First().SequenceNumber);

				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals("BM_InlandTransportMode is copied from departure", ModeOfTransportList.Codes._2_RailTransport, arrivalHeader.ArrivalMovementHeader.BM_InlandTransportMode);
				AssertEquals("BM_GrossWeight is copied from departure", (ZDecimal)12.345, arrivalHeader.ArrivalMovementHeader.BM_GrossWeight);

				AssertContainsExactElementsInAnyOrder(
					"Header transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "20", "wagon", "GB"),
						(2, "NEW", "10", "transport", "FR"),
						(3, "NEW", "11", "transport2", "DE")
					},
					GetArrivalHeaderTransportInfos(arrivalHeader));

				AssertEquals(
					"Bill transportInfos - No departure bill transportInfos to copy",
					0,
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]).Count());

				var containers = arrivalHeader.ArrivalHeaderContainers;
				AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
															new (ZShort, ZString, ZString, ZString)[]
																{
																(1, "DEC", "CONT1", "CNT"),
																(2, "DEC", "CONT2", "CNT"),
																(3, "NEW", "CONTAINER3", "CNT")
																}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));

				var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
				var container1Seal = containerWithSeq1.Seals[0];
				AssertEquals("Container with seq 1, only seal's BK_SequenceNumber", (ZShort)1, container1Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 1, only seal's BK_UnloadingState", "DEC", container1Seal.BK_UnloadingState);
				AssertEquals("Container with seq 1, only seal's BK_SealNumber", "Seal1", container1Seal.BK_SealNumber);

				var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
				var container2Seals = containerWithSeq2.Seals;
				AssertEquals("Container with seq 2, has 3 seals", 3, container2Seals.Count);
				AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
														new (ZShort, ZString, ZString)[]
														{
															(1, "DEC", "Seal2"),
															(2, "DEC", "Seal3"),
															(3, "NEW", "SEAL12")
														}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

				var containerWithSeq3 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
				var container3Seal = containerWithSeq3.Seals[0];
				AssertEquals("Container with seq 3, only seal's BK_SequenceNumber", (ZShort)1, container3Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 3, only seal's BK_UnloadingState", "NEW", container3Seal.BK_UnloadingState);
				AssertEquals("Container with seq 3, only seal's BK_SealNumber", "SEAL22", container3Seal.BK_SealNumber);

				AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
															new (ZString, ZString, ZDecimal, ZString)[]
															{
															("1", "DEC", 20m, "KG"),
															("2", "NEW", 15m, "T")
															}, arrivalHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));

				var bill1GoodsItems = arrivalHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1").ArrivalGoodsItems;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(1, 1, "DEC", "QQQ", "88888888", 20, "KG", 10, "DG", "departuredescription"),
															(2, 2, "DEC", "WWW", "99999999", 15, "KL", 11, "L", "departuredescription2"),
															(3, 6, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

				var bill1GoodsItem1 = bill1GoodsItems.First(x => x.BY_LineNo == 1);
				var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

				var bill1GoodsItem1Package1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package1ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package1Containers);
				var bill1GoodsItem1Package2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package2ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package2Containers);

				var bill1GoodsItem1SupportingDocuments = bill1GoodsItem1.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("billGoodsItemSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "A001", "departuredoc1"),
															(2, "DEC", "A002", "departuredoc2")
														}, bill1GoodsItem1SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem1TransportDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "B001", "departuretra1"),
															(2, "DEC", "B002", "departuretra2")
														}, bill1GoodsItem1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var header1TransportDocuments = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("header1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(4, "DEC", "B003", "departuretraHeader1")
														}, header1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1TransportDocuments = arrivalHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(5, "DEC", "B004", "departuretraBill1")
														}, bill1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem1ReferenceDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "C001", "departureref1"),
															(2, "DEC", "C002", "departureref2")
														}, bill1GoodsItem1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var headerReferenceDocuments = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("headerReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(4, "DEC", "C003", "departurerefHeader1")
														}, headerReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1ReferenceDocuments = arrivalHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(5, "DEC", "C004", "departurerefBill1")
														}, bill1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2 = bill1GoodsItems.First(x => x.BY_LineNo == 2);
				var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

				var bill1GoodsItem2Package1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package1Containers);
				var bill1GoodsItem2Package2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package2Containers);
				var bill1GoodsItem2Package3Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package3ContainersPivots.Container", new[] { containerWithSeq3 }, bill1GoodsItem2Package3Containers);

				var bill1GoodsItem2SupportingDocuments = bill1GoodsItem2.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "A001", "departuredoc1"),
															(2, "DEC", "A002", "departuredoc2"),
															(3, "NEW", "4321", "REFERENCE2")
														}, bill1GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2TransportDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "B001", "departuretra1"),
															(2, "DEC", "B002", "departuretra2"),
															(3, "NEW", "A001", "TRA2")
														}, bill1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var header2TransportDocuments = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("header2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(4, "DEC", "B003", "departuretraHeader1")
														}, header2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1_1GoodsItem2TransportDocuments = arrivalHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1_1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(5, "DEC", "B004", "departuretraBill1")
														}, bill1_1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2ReferenceDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "C001", "departureref1"),
															(2, "DEC", "C002", "departureref2"),
															(3, "NEW", "A003", "REF2")
														}, bill1GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var header3ReferenceDocuments = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("header3ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(4, "DEC", "C003", "departurerefHeader1")
														}, header3ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1_2GoodsItem2ReferenceDocuments = arrivalHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1_2GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(5, "DEC", "C004", "departurerefBill1")
														}, bill1_2GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem3 = bill1GoodsItems.First(x => x.BY_LineNo == 3);
				AssertPackagesWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3Packages", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3SupportingDocuments");
				AssertTransportDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3TransportDocuments");
				AssertReferenceDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3ReferenceDocuments");

				var bill2GoodsItems = arrivalHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "2").ArrivalGoodsItems;
				AssertContainsExactElementsInAnyOrder("bill2GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(2, 5, "NEW", "AAA", "11111111", 4, "HG", 6, "G", "somedescription"),
															(3, 6, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, bill2GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

				var bill2GoodsItem2 = bill2GoodsItems.First(x => x.BY_LineNo == 2);
				AssertPackagesWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2SupportingDocuments");
				AssertTransportDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2TransportDocuments");
				AssertReferenceDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2ReferenceDocuments");

				var bill2GoodsItem3 = bill2GoodsItems.First(x => x.BY_LineNo == 3);
				AssertPackagesWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3SupportingDocuments");
				AssertTransportDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3TransportDocuments");
				AssertReferenceDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3ReferenceDocuments");
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_All_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AddUnloadingDataToArrival(arrivalHeader);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		AddDataToDepartureForUnloading(departure);

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				AssertEquals("Prereq: departure cont1.BC_SequenceNumber", (ZShort)1, departure.DepartureHeaderContainers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1").BC_SequenceNumber);
				AssertEquals("Prereq: departure cont2.BC_SequenceNumber", (ZShort)2, departure.DepartureHeaderContainers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2").BC_SequenceNumber);
				AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departure.Bills.First().SequenceNumber);

				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals("BM_InlandTransportMode is copied from departure", ModeOfTransportList.Codes._2_RailTransport, arrivalHeader.ArrivalMovementHeader.BM_InlandTransportMode);
				AssertEquals("BM_GrossWeight is copied from departure", (ZDecimal)12.345, arrivalHeader.ArrivalMovementHeader.BM_GrossWeight);

				AssertContainsExactElementsInAnyOrder(
					"transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "20", "wagon", "GB"),
						(2, "NEW", "10", "transport", "FR"),
						(3, "NEW", "11", "transport2", "DE")
					},
					GetArrivalHeaderTransportInfos(arrivalHeader));

				AssertEquals(
					"No departure bill transportInfos to copy",
					0,
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]).Count());

				var containers = arrivalHeader.ArrivalHeaderContainers;
				AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
															new (ZShort, ZString, ZString, ZString)[]
																{
																(1, "DEC", "CONT1", "CNT"),
																(2, "DEC", "CONT2", "CNT"),
																(3, "NEW", "CONTAINER3", "CNT")
																}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));

				var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
				var container1Seal = containerWithSeq1.Seals[0];
				AssertEquals("Container with seq 1, only seal's BK_SequenceNumber", (ZShort)1, container1Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 1, only seal's BK_UnloadingState", "DEC", container1Seal.BK_UnloadingState);
				AssertEquals("Container with seq 1, only seal's BK_SealNumber", "Seal1", container1Seal.BK_SealNumber);

				var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
				var container2Seals = containerWithSeq2.Seals;
				AssertEquals("Container with seq 2, has 3 seals", 3, container2Seals.Count);
				AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
														new (ZShort, ZString, ZString)[]
														{
															(1, "DEC", "Seal2"),
															(2, "DEC", "Seal3"),
															(3, "NEW", "SEAL12")
														}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

				var containerWithSeq3 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
				var container3Seal = containerWithSeq3.Seals[0];
				AssertEquals("Container with seq 3, only seal's BK_SequenceNumber", (ZShort)1, container3Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 3, only seal's BK_UnloadingState", "NEW", container3Seal.BK_UnloadingState);
				AssertEquals("Container with seq 3, only seal's BK_SealNumber", "SEAL22", container3Seal.BK_SealNumber);

				AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
															new (ZString, ZString, ZDecimal, ZString)[]
															{
															("1", "DEC", 20m, "KG"),
															("2", "NEW", 15m, "T")
															}, arrivalHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));

				var bill1GoodsItems = arrivalHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1").ArrivalGoodsItems;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(1, 1, "DEC", "QQQ", "88888888", 20, "KG", 10, "DG", "departuredescription"),
															(2, 2, "DEC", "WWW", "99999999", 15, "KL", 11, "L", "departuredescription2"),
															(3, 6, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

				var bill1GoodsItem1 = bill1GoodsItems.First(x => x.BY_LineNo == 1);
				var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

				var bill1GoodsItem1Package1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package1ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package1Containers);
				var bill1GoodsItem1Package2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package2ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package2Containers);

				var bill1GoodsItem1SupportingDocuments = bill1GoodsItem1.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("billGoodsItemSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "A001", "departuredoc1"),
															(2, "DEC", "A002", "departuredoc2")
														}, bill1GoodsItem1SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem1TransportDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "B001", "departuretra1"),
															(2, "DEC", "B002", "departuretra2"),
															(4, "DEC", "B003", "departuretraHeader1"),
															(5, "DEC", "B004", "departuretraBill1")
														}, bill1GoodsItem1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem1ReferenceDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "C001", "departureref1"),
															(2, "DEC", "C002", "departureref2"),
															(4, "DEC", "C003", "departurerefHeader1"),
															(5, "DEC", "C004", "departurerefBill1")
														}, bill1GoodsItem1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2 = bill1GoodsItems.First(x => x.BY_LineNo == 2);
				var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

				var bill1GoodsItem2Package1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package1Containers);
				var bill1GoodsItem2Package2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package2Containers);
				var bill1GoodsItem2Package3Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package3ContainersPivots.Container", new[] { containerWithSeq3 }, bill1GoodsItem2Package3Containers);

				var bill1GoodsItem2SupportingDocuments = bill1GoodsItem2.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "A001", "departuredoc1"),
															(2, "DEC", "A002", "departuredoc2"),
															(3, "NEW", "4321", "REFERENCE2")
														}, bill1GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2TransportDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "B001", "departuretra1"),
															(2, "DEC", "B002", "departuretra2"),
															(3, "NEW", "A001", "TRA2"),
															(4, "DEC", "B003", "departuretraHeader1"),
															(5, "DEC", "B004", "departuretraBill1")
														}, bill1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2ReferenceDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "C001", "departureref1"),
															(2, "DEC", "C002", "departureref2"),
															(3, "NEW", "A003", "REF2"),
															(4, "DEC", "C003", "departurerefHeader1"),
															(5, "DEC", "C004", "departurerefBill1")
														}, bill1GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem3 = bill1GoodsItems.First(x => x.BY_LineNo == 3);
				AssertPackagesWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3Packages", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3SupportingDocuments");
				AssertTransportDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3TransportDocuments");
				AssertReferenceDocWithoutChanges(bill1GoodsItem3, "bill1GoodsItem3ReferenceDocuments");

				var bill2GoodsItems = arrivalHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "2").ArrivalGoodsItems;
				AssertContainsExactElementsInAnyOrder("bill2GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(2, 5, "NEW", "AAA", "11111111", 4, "HG", 6, "G", "somedescription"),
															(3, 6, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, bill2GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

				var bill2GoodsItem2 = bill2GoodsItems.First(x => x.BY_LineNo == 2);
				AssertPackagesWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2SupportingDocuments");
				AssertTransportDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2TransportDocuments");
				AssertReferenceDocWithoutChanges(bill2GoodsItem2, "bill2GoodsItem2ReferenceDocuments");

				var bill2GoodsItem3 = bill2GoodsItems.First(x => x.BY_LineNo == 3);
				AssertPackagesWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3", containerWithSeq3);
				AssertSupportingDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3SupportingDocuments");
				AssertTransportDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3TransportDocuments");
				AssertReferenceDocWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3ReferenceDocuments");
			}
		});
	}

	void AssertPackagesWithoutChanges(NctsArrivalCargoDesc goodsItem, ZString goodsItemName, NctsArrivalHeaderContainer containerWithSeq3)
	{
		var packages = goodsItem.Packages;
		AssertContainsExactElementsInAnyOrder(goodsItemName + " (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
													new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(2, "NEW", "CT", 3, "marks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
													}, packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

		var package1Containers = packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
		var package2Containers = packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;

		AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package1ContainersPivots.Container", new[] { containerWithSeq3 }, package1Containers);
		AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package2ContainersPivots.Container", new[] { containerWithSeq3 }, package2Containers);
	}

	void AssertSupportingDocWithoutChanges(NctsArrivalCargoDesc goodsItem, ZString goodsItemName)
	{
		var supportingDocuments = goodsItem.SupportingDocuments;
		AssertContainsExactElementsInAnyOrder(goodsItemName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "NEW", "1234", "REFERENCE1"),
															(3, "NEW", "4321", "REFERENCE2")
													}, supportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
	}

	void AssertTransportDocWithoutChanges(NctsArrivalCargoDesc goodsItem, ZString goodsItemName)
	{
		var transportDocuments = goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
		AssertContainsExactElementsInAnyOrder(goodsItemName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
												new (ZInt, ZString, ZString, ZString)[]
												{
															(2, "NEW", "A000", "TRA1"),
															(3, "NEW", "A001", "TRA2")
												}, transportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
	}

	void AssertReferenceDocWithoutChanges(NctsArrivalCargoDesc goodsItem, ZString goodsItemName)
	{
		var referenceDocuments = goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
		AssertContainsExactElementsInAnyOrder(goodsItemName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
												new (ZInt, ZString, ZString, ZString)[]
												{
															(2, "NEW", "A002", "REF1"),
															(3, "NEW", "A003", "REF2")
												}, referenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOTnot3_WithTransportAtDeparture_FinalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._1_SeaTransport, "10", "Vessel", "ES");

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertContainsExactElementsInAnyOrder(
					"Header TransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "10", "Vessel", "ES")
					},
					GetArrivalHeaderTransportInfos(arrivalHeader));

				AssertEquals(
					"No Bill TransportInfos to copy",
					0,
					arrivalHeader.Bills.Count);
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOTnot3_WithTransportAtDeparture_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._1_SeaTransport, "10", "Vessel", "ES");

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
		{
			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			AssertContainsExactElementsInAnyOrder(
				"Header TransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
				new (ZShort, ZString, ZString, ZString, ZString)[]
				{
					(1, "DEC", "10", "Vessel", "ES")
				},
				GetArrivalHeaderTransportInfos(arrivalHeader));

			AssertEquals(
				"No Bill TransportInfos to copy",
				0,
				arrivalHeader.Bills.Count);
		}
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOT3_WithAllData_FinalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transportID", "GB", "trailer1", "FR", "trailer2", "ES");

		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "TR1", "PL", "TR1-11", "IT", "TR1-12", "GB");
		AddDataToDepartureBillAndTransportForUnloading(departure, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "TR2", "GR", "TR2-11", "FR", "TR2-12", "IT");

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals(
					"No Header TransportInfos copied when Bill TransportInfos specified",
					0,
					GetArrivalHeaderTransportInfos(arrivalHeader).Count());

				AssertContainsExactElementsInAnyOrder(
					"Bill 1 TransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "TR1", "PL"),
						(2, "DEC", "31", "TR1-11", "IT"),
						(3, "DEC", "31", "TR1-12", "GB")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]));

				AssertContainsExactElementsInAnyOrder(
					"Bill 2 TransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID", "GB"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[1]));

				AssertContainsExactElementsInAnyOrder(
					"Bill 3 TransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "TR2", "GR"),
						(2, "DEC", "31", "TR2-11", "FR"),
						(3, "DEC", "31", "TR2-12", "IT")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[2]));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOT3_WithAllData_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transportID", "GB", "trailer1", "FR", "trailer2", "ES");

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
		{
			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			AssertContainsExactElementsInAnyOrder(
				"Header TransportInfo (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
				new (ZShort, ZString, ZString, ZString, ZString)[]
				{
					(1, "DEC", "30", "transportID", "GB"),
					(2, "DEC", "31", "trailer1", "FR"),
					(3, "DEC", "31", "trailer2", "ES"),
				},
				GetArrivalHeaderTransportInfos(arrivalHeader));

			AssertEquals(
				"No Bill TransportInfos to copy",
				0,
				arrivalHeader.Bills.Count);
		}
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOT3_WithPartialData()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");

		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID", "DE", "trailer1", "FR", "trailer2", "ES");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, ZString.Empty, "transportID-1", "DE", "trailer1", "FR", "trailer2", "ES");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", ZString.Empty, "DE", "trailer1", "FR", "trailer2", "ES", createWithEmptyId: true);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID-3", ZString.Empty, "trailer1", "FR", "trailer2", "ES");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID-4", "DE", ZString.Empty, "FR", "trailer2", "ES", createWithEmptyId: true);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID-5", "DE", "trailer1", ZString.Empty, "trailer2", "ES");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID-6", "DE", "trailer1", "FR", ZString.Empty, "ES", createWithEmptyId: true);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID-7", "DE", "trailer1", "FR", "trailer2", ZString.Empty);

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals(
					"Arrival Header TransportInfos not populated when Departure Bills TransportInfos specified",
					0,
					GetArrivalHeaderTransportInfos(arrivalHeader).Count());

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - all data copied from Departure Bill",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TypeID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-1", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[1]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TransportID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[2]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TransportNationality copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-3", "GB"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[3]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - Trailer1ID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-4", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[4]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - Trailer1Nationality copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-5", "DE"),
						(2, "DEC", "31", "trailer1", "IT"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[5]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - Trailer2ID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-6", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "ES")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[6]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - Trailer2Nationality copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID-7", "DE"),
						(2, "DEC", "31", "trailer1", "FR"),
						(3, "DEC", "31", "trailer2", "PT")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[7]));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_MOTnot3_WithPartialData()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._1_SeaTransport, "10", "vessel", "GB");

		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "10", "vessel1", "DE");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, ZString.Empty, "vessel1", "DE");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "10", ZString.Empty, "FR", createWithEmptyId: true);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "10", "vessel1", ZString.Empty);

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals(
					"Arrival Header TransportInfos not populated when Departure Bills TransportInfos specified",
					0,
					GetArrivalHeaderTransportInfos(arrivalHeader).Count());

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - all data copied from Departure Bill",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "10", "vessel1", "DE")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TypeID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "10", "vessel1", "DE")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[1]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TransportID copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "10", "vessel", "FR")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[2]));

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill TransportInfos - TransportNationality copied from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "10", "vessel1", "GB")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[3]));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_WithBillTransportInfoSameAsHeader()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");

		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertContainsExactElementsInAnyOrder(
					"Arrival Header TransportInfos populated from Departure Header",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transportID", "GB"),
						(2, "DEC", "31", "trailer1", "IT"),
						(3, "DEC", "31", "trailer2", "PT")
					},
					GetArrivalHeaderTransportInfos(arrivalHeader));

				AssertEquals(
					"Arrival Bill 1 TransportInfos - not copied from Departure Bill because all Bills TransportInfos are the same as in Header",
					0,
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]).Count());

				AssertEquals(
					"Arrival Bill 2 TransportInfos - not copied from Departure Bill because all Bills TransportInfos are the same as in Header",
					0,
					GetArrivalBillTransportInfos(arrivalHeader.Bills[1]).Count());
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_ClearNewInArrivalHeader_RetainNewInArrivalBill()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transport", "GB", String.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AddDataToDepartureBillAndTransportForUnloading(departure, departureMovement.BM_InlandTransportMode, "30", "transport", "GB", "trailerA", "GB", ZString.Empty, ZString.Empty);

		AddNewTransportInfosToArrivalHeader(arrivalHeader);
		AddArrivalBillWithNewTransportInfo(arrivalHeader, departureMovement.BM_InlandTransportMode, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertEquals(
					"Arrival Header NEW TransportInfos cleared",
					0,
					GetArrivalHeaderTransportInfos(arrivalHeader).Count());

				AssertContainsExactElementsInAnyOrder(
					"Arrival Bill NEW TransportInfo: overwriting one set in Departure Bill, retaining one not mentioned in Departure Bill",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transport", "GB"),
						(2, "DEC", "31", "trailerA", "GB"),
						(3, "NEW", "31", "trailer2", "PT")
					},
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportInfos_RetainNewInArrivalHeader_ClearNewInArrivalBill()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = departure.MovementHeader;

		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._3_RoadTransport, "30", "transport", "GB", String.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

		AddNewTransportInfosToArrivalHeader(arrivalHeader);
		AddArrivalBillWithNewTransportInfo(arrivalHeader, departureMovement.BM_InlandTransportMode, "30", "transportID", "GB", "trailer1", "IT", "trailer2", "PT");

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				AssertContainsExactElementsInAnyOrder(
					"Arrival Header NEW TransportInfo cleared and Departure Header TransportInfo copied",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "30", "transport", "GB"),
						(2, "NEW", "10", "transport", "FR"),
						(3, "NEW", "11", "transport2", "DE")
					},
					GetArrivalHeaderTransportInfos(arrivalHeader));

				AssertEquals(
					"Arrival Bill TransportInfos cleared",
					0,
					GetArrivalBillTransportInfos(arrivalHeader.Bills[0]).Count());
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_Containers()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AddContainersToArrival(arrivalHeader);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var cont1 = departure.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";
		cont1.Seal1 = "Seal1";
		cont1.Seal2 = "Seal2";
		var seal3 = cont1.AdditionalSeals.AddNew();
		seal3.BK_SealNumber = "Seal3";
		var seal4 = cont1.AdditionalSeals.AddNew();
		seal4.BK_SealNumber = "Seal4";

		var cont2 = departure.DepartureHeaderContainers.AddNew();
		cont2.BC_Mode = "CNT";
		cont2.BC_ContainerNum = "CONT2";
		cont2.Seal1 = "Seal5";
		cont2.Seal2 = "Seal6";

		var cont3 = departure.DepartureHeaderContainers.AddNew();
		cont3.BC_Mode = "NCT";
		cont3.BC_ContainerNum = "CONT3";

		var cont4 = departure.DepartureHeaderContainers.AddNew();
		cont4.BC_Mode = "NCT";
		cont4.BC_ContainerNum = "CONT4";
		cont4.Seal1 = "Seal7";
		cont4.Seal2 = "Seal8";
		var seal9 = cont4.AdditionalSeals.AddNew();
		seal9.BK_SealNumber = "Seal9";

		var cont5 = departure.DepartureHeaderContainers.AddNew();
		cont5.BC_Mode = "NCT";
		cont5.BC_ContainerNum = "CONT5";

		var nctsBill = departure.Bills.AddNew();
		var goodsItem = nctsBill.GoodsItems.AddNew();
		var package = goodsItem.Packages.AddNew();
		package.ContainersPivot.AddPivotFor(cont1);
		package.ContainersPivot.AddPivotFor(cont2);
		package.ContainersPivot.AddPivotFor(cont4);
		package.ContainersPivot.AddPivotFor(cont5);

		cont1.BC_SequenceNumber = 1;
		cont2.BC_SequenceNumber = 2;
		cont3.BC_SequenceNumber = 3;
		cont4.BC_SequenceNumber = 4;
		cont5.BC_SequenceNumber = 5;

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departure cont1.BC_SequenceNumber", (ZShort)1, cont1.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont2.BC_SequenceNumber", (ZShort)2, cont2.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont3.BC_SequenceNumber", (ZShort)3, cont3.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont4.BC_SequenceNumber", (ZShort)4, cont4.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont5.BC_SequenceNumber", (ZShort)5, cont5.BC_SequenceNumber);

			AssertEquals("Prereq: departure seal3.BK_SequenceNumber", (ZShort)3, seal3.BK_SequenceNumber);
			AssertEquals("Prereq: departure seal4.BK_SequenceNumber", (ZShort)4, seal4.BK_SequenceNumber);
			AssertEquals("Prereq: departure seal9.BK_SequenceNumber", (ZShort)3, seal9.BK_SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var containers = arrivalHeader.ArrivalHeaderContainers;
			AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
														new (ZShort, ZString, ZString, ZString)[]
															{
																(1, "DEC", "CONT1", "CNT"),
																(2, "DEC", "CONT2", "CNT"),
																(3, "NEW", "CONTAINER3", "CNT"),
																(4, "DEC", "CONT4", "NCT"),
																(5, "DEC", "CONT5", "NCT")
															}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));

			var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			var container1Seals = containerWithSeq1.Seals;
			AssertEquals("Container CNT with seq 1, has 4 seals", 4, container1Seals.Count);
			AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
													new (ZShort, ZString, ZString)[]
													{
															(1, "DEC", "Seal1"),
															(2, "DEC", "Seal2"),
															(3, "DEC", "Seal3"),
															(4, "DEC", "Seal4")
													}, container1Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

			var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
			var container2Seals = containerWithSeq2.Seals;
			AssertEquals("Container CNT with seq 2, has 3 seals", 3, container2Seals.Count);
			AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
													new (ZShort, ZString, ZString)[]
													{
															(1, "DEC", "Seal5"),
															(2, "DEC", "Seal6"),
															(3, "NEW", "SEAL12")
													}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

			var containerWithSeq3 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
			var container3Seal = containerWithSeq3.Seals[0];
			AssertEquals("Container with seq 3, only seal's BK_SequenceNumber", (ZShort)1, container3Seal.BK_SequenceNumber);
			AssertEquals("Container with seq 3, only seal's BK_UnloadingState", "NEW", container3Seal.BK_UnloadingState);
			AssertEquals("Container with seq 3, only seal's BK_SealNumber", "SEAL22", container3Seal.BK_SealNumber);

			var containerWithSeq4 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 4);
			var container4Seals = containerWithSeq4.Seals;
			AssertEquals("Container with seq 4, has 3 seals", 3, container4Seals.Count);
			AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
													new (ZShort, ZString, ZString)[]
													{
															(1, "DEC", "Seal7"),
															(2, "DEC", "Seal8"),
															(3, "DEC", "Seal9")
													}, container4Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

			var containerWithSeq5 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 5);
			AssertEquals("Container with seq 5, has no seals", 0, containerWithSeq5.Seals.Count);
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_HouseConsignments()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill1 = arrivalHeader.Bills.AddNew();
		bill1.MovementDetail.B9_SeqNo = "1";
		bill1.MovementDetail.B9_UnloadedState = "NEW";
		bill1.B0_Weight = 10m;
		bill1.B0_WeightUQ = "LT";

		var bill2 = arrivalHeader.Bills.AddNew();
		bill2.MovementDetail.B9_SeqNo = "2";
		bill2.MovementDetail.B9_UnloadedState = "NEW";
		bill2.B0_Weight = 15m;
		bill2.B0_WeightUQ = "T";

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();
		departureBill.B0_Weight = 20m;
		departureBill.B0_WeightUQ = "KG";

		//TODO: future WI, when SequenceNumber for Departure Bill is saved we will load with multiple bills

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
														new (ZString, ZString, ZDecimal, ZString)[]
														{
															("1", "DEC", 20m, "KG"),
															("2", "NEW", 15m, "T")
														}, arrivalHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_GoodsItems()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 2;
		goodsItem1.BY_DeclarationGoodsItemNumber = 5;
		goodsItem1.BY_UnloadedState = "NEW";
		goodsItem1.BY_Description = "somedescription";
		goodsItem1.BY_CusC4Number = "AAA";
		goodsItem1.BY_HarmonisedTariff = "11111111";
		goodsItem1.BY_GrossWeight = 4;
		goodsItem1.BY_GrossWeightUnit = "HG";
		goodsItem1.BY_NetWeight = 6;
		goodsItem1.BY_NetWeightUnit = "G";

		var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_LineNo = 3;
		goodsItem2.BY_DeclarationGoodsItemNumber = 6;
		goodsItem2.BY_UnloadedState = "NEW";
		goodsItem2.BY_Description = "somedescription2";
		goodsItem2.BY_CusC4Number = "BBB";
		goodsItem2.BY_HarmonisedTariff = "22222222";
		goodsItem2.BY_GrossWeight = 3;
		goodsItem2.BY_GrossWeightUnit = "T";
		goodsItem2.BY_NetWeight = 7;
		goodsItem2.BY_NetWeightUnit = "LT";

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		departureGoodsItem1.BY_DeclarationGoodsItemNumber = 1;
		departureGoodsItem1.BY_Description = "departuredescription";
		departureGoodsItem1.BY_CusC4Number = "QQQ";
		departureGoodsItem1.BY_HarmonisedTariff = "88888888";
		departureGoodsItem1.BY_GrossWeight = 20;
		departureGoodsItem1.BY_GrossWeightUnit = "KG";
		departureGoodsItem1.BY_NetWeight = 10;
		departureGoodsItem1.BY_NetWeightUnit = "DG";

		var departureGoodsItem2 = departureBill.GoodsItems.AddNew();
		departureGoodsItem2.BY_LineNo = 2;
		departureGoodsItem2.BY_DeclarationGoodsItemNumber = 2;
		departureGoodsItem2.BY_Description = "departuredescription2";
		departureGoodsItem2.BY_CusC4Number = "WWW";
		departureGoodsItem2.BY_HarmonisedTariff = "99999999";
		departureGoodsItem2.BY_GrossWeight = 15;
		departureGoodsItem2.BY_GrossWeightUnit = "KL";
		departureGoodsItem2.BY_NetWeight = 11;
		departureGoodsItem2.BY_NetWeightUnit = "L";

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var billGoodsItems = arrivalHeader.Bills.First().ArrivalGoodsItems;
			AssertContainsExactElementsInAnyOrder("billGoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
													{
															(1, 1, "DEC", "QQQ", "88888888", 20, "KG", 10, "DG", "departuredescription"),
															(2, 2, "DEC", "WWW", "99999999", 15, "KL", 11, "L", "departuredescription2"),
															(3, 6, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
													}, billGoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_Packages_IsVehiclesFalse()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var container1 = arrivalHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_SequenceNumber = 2;
		container1.BC_ContainerNum = "CONTAINER2";

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddPackagesToGoodsItemArrival(goodsItem1, container1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var cont1 = departure.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";

		var cont2 = departure.DepartureHeaderContainers.AddNew();
		cont2.BC_Mode = "CNT";
		cont2.BC_ContainerNum = "CONT2";

		cont1.BC_SequenceNumber = 1;
		cont2.BC_SequenceNumber = 2;

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		AddPackagesToGoodsItemDeparture(departureGoodsItem1, cont1);

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);
			AssertEquals("Prereq: departure cont1.BC_SequenceNumber", (ZShort)1, cont1.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont2.BC_SequenceNumber", (ZShort)2, cont2.BC_SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var billGoodsItem = arrivalHeader.Bills.First().ArrivalGoodsItems.First();
			var bill1GoodsItemPackages = billGoodsItem.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
													new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
													}, bill1GoodsItemPackages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var containers = arrivalHeader.ArrivalHeaderContainers;
			var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);

			var billGoodsItemPackage1Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq1 }, billGoodsItemPackage1Containers);
			var billGoodsItemPackage2Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq1 }, billGoodsItemPackage2Containers);
			var billGoodsItemPackage3Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, billGoodsItemPackage3Containers);
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_Packages_IsVehiclesTrue()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var container1 = arrivalHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_SequenceNumber = 2;
		container1.BC_ContainerNum = "CONTAINER2";

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddPackagesToGoodsItemArrival(goodsItem1, container1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var cont1 = departure.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";

		var cont2 = departure.DepartureHeaderContainers.AddNew();
		cont2.BC_Mode = "CNT";
		cont2.BC_ContainerNum = "CONT2";

		cont1.BC_SequenceNumber = 1;
		cont2.BC_SequenceNumber = 2;

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		departureGoodsItem1.IsVehicles = true;

		var pack1 = departureGoodsItem1.Packages.AddNew();
		pack1.B5_SequenceNumber = 1;
		pack1.B5_PackageID = "VINCODE1";
		pack1.B5_Brand = "Brand1";
		pack1.B5_Model = "Model1";
		pack1.ContainersPivot.AddPivotFor(cont1);

		var pack2 = departureGoodsItem1.Packages.AddNew();
		pack2.B5_SequenceNumber = 2;
		pack2.B5_PackageID = "VINCODE2";
		pack2.B5_Brand = "Brand2";
		pack2.B5_Model = "Model2";
		pack2.ContainersPivot.AddPivotFor(cont1);

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);
			AssertEquals("Prereq: departure cont1.BC_SequenceNumber", (ZShort)1, cont1.BC_SequenceNumber);
			AssertEquals("Prereq: departure cont2.BC_SequenceNumber", (ZShort)2, cont2.BC_SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var billGoodsItem = arrivalHeader.Bills.First().ArrivalGoodsItems.First();
			var bill1GoodsItemPackages = billGoodsItem.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItemPackages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
													new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "DEC", "FR", 1, ZString.Empty, "VINCODE1", "Brand1", "Model1"),
															(2, "DEC", "FR", 1, ZString.Empty, "VINCODE2", "Brand2", "Model2"),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
													}, bill1GoodsItemPackages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var containers = arrivalHeader.ArrivalHeaderContainers;
			var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);

			var billGoodsItemPackage1Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItemPackage1ContainersPivots.Container", new[] { containerWithSeq1 }, billGoodsItemPackage1Containers);
			var billGoodsItemPackage2Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItemPackage2ContainersPivots.Container", new[] { containerWithSeq1 }, billGoodsItemPackage2Containers);
			var billGoodsItemPackage3Containers = bill1GoodsItemPackages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItemPackage3ContainersPivots.Container", new[] { containerWithSeq2 }, billGoodsItemPackage3Containers);
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_SupportingDocuments_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddSupportingDocumentsToGoodsItemArrival(goodsItem1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		AddSupportingDocumentsToGoodsItemDeparture(departureGoodsItem1);

		var supDoc1 = departure.MovementHeader.SupportingDocuments.AddNew();
		supDoc1.CSI_Code = "A003";
		supDoc1.CSI_ReferenceNumber = "departuredocHeader1";

		var supDoc2 = departureBill.SupportingDocuments.AddNew();
		supDoc2.CSI_Code = "A004";
		supDoc2.CSI_ReferenceNumber = "departuredocBill1";

		supDoc1.CSI_LineNo = 4;
		supDoc2.CSI_LineNo = 5;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				var billGoodsItem = arrivalHeader.Bills.First().ArrivalGoodsItems.First();
				var billGoodsItemSupportingDocuments = billGoodsItem.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("billGoodsItemSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "A001", "departuredoc1"),
															(2, "DEC", "A002", "departuredoc2"),
															(3, "NEW", "4321", "REFERENCE2")
														}, billGoodsItemSupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_TransportDocuments_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddTransportDocumentsToGoodsItemArrival(goodsItem1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		AddTransportDocumentsToGoodsItemDeparture(departureGoodsItem1);

		var transpDoc1 = departure.AdditionalDocuments.AddNew();
		transpDoc1.CSI_Code = "B003";
		transpDoc1.CSI_SubType = "TRA";
		transpDoc1.CSI_ReferenceNumber = "departuretraHeader1";

		var transpDoc2 = departureBill.AdditionalDocuments.AddNew();
		transpDoc2.CSI_Code = "B004";
		transpDoc2.CSI_SubType = "TRA";
		transpDoc2.CSI_ReferenceNumber = "departuretraBill1";

		transpDoc1.CSI_LineNo = 4;
		transpDoc2.CSI_LineNo = 5;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				var billGoodsItem = arrivalHeader.Bills.First().ArrivalGoodsItems.First();
				var billGoodsItemTransportDocuments = billGoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("billGoodsItemTransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "B001", "departuretra1"),
															(2, "DEC", "B002", "departuretra2"),
															(3, "NEW", "A001", "TRA2"),
															(4, "DEC", "B003", "departuretraHeader1"),
															(5, "DEC", "B004", "departuretraBill1")
														}, billGoodsItemTransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_AdditionalReferences_TransitionalPeriod()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddReferenceDocumentsToGoodsItemArrival(goodsItem1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		AddReferenceDocumentsToGoodsItemDeparture(departureGoodsItem1);

		var refDoc1 = departure.AdditionalDocuments.AddNew();
		refDoc1.CSI_Code = "C003";
		refDoc1.CSI_SubType = "REF";
		refDoc1.CSI_ReferenceNumber = "departurerefHeader1";

		var refDoc2 = departureBill.AdditionalDocuments.AddNew();
		refDoc2.CSI_Code = "C004";
		refDoc2.CSI_SubType = "REF";
		refDoc2.CSI_ReferenceNumber = "departurerefBill1";

		refDoc1.CSI_LineNo = 4;
		refDoc2.CSI_LineNo = 5;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

				var billGoodsItem = arrivalHeader.Bills.First().ArrivalGoodsItems.First();
				var billGoodsItemReferenceDocuments = billGoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("billGoodsItemReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "C001", "departureref1"),
															(2, "DEC", "C002", "departureref2"),
															(3, "NEW", "A003", "REF2"),
															(4, "DEC", "C003", "departurerefHeader1"),
															(5, "DEC", "C004", "departurerefBill1")
														}, billGoodsItemReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
			}
		});
	}

	public void TestLoadDataForUnloadingFromDeparture_LiabilityCalculation()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var bill = nctsHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		AddLiabilityCalculationToGoodsItemArrival(goodsItem1);

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		AddLiabilityCalculationToGoodsItemDeparture(departureGoodsItem1);

		var registryRegisterEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled;
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_RN_NKCountryOfOrigin", "CA", goodsItem1.BY_RN_NKCountryOfOrigin);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_HarmonisedTariff", "11110", goodsItem1.BY_HarmonisedTariff);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsSecondQuantity", 10m, goodsItem1.BY_CustomsSecondQuantity);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsSecondUnitQty", "KA", goodsItem1.BY_CustomsSecondUnitQty);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsThirdQuantity", 20m, goodsItem1.BY_CustomsThirdQuantity);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsThirdUnitQty", "GA", goodsItem1.BY_CustomsThirdUnitQty);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsFourthQuantity", 30m, goodsItem1.BY_CustomsFourthQuantity);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_CustomsFourthUnitQty", "TA", goodsItem1.BY_CustomsFourthUnitQty);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.BY_MonetaryValue", 100m, goodsItem1.BY_MonetaryValue);
					AssertEquals("Final Period - Prereq: arrivalBillGoodsItem.AdditionalSupplementaryCodes", true, goodsItem1.AdditionalSupplementaryCodes.ContainsCode("V999"));

					nctsHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

					AssertEquals("Final Period - arrivalBillGoodsItem.BY_RN_NKCountryOfOrigin", "CD", goodsItem1.BY_RN_NKCountryOfOrigin);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_HarmonisedTariff", "11111", goodsItem1.BY_HarmonisedTariff);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsSecondQuantity", 1m, goodsItem1.BY_CustomsSecondQuantity);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsSecondUnitQty", "KD", goodsItem1.BY_CustomsSecondUnitQty);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsThirdQuantity", 2m, goodsItem1.BY_CustomsThirdQuantity);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsThirdUnitQty", "GD", goodsItem1.BY_CustomsThirdUnitQty);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsFourthQuantity", 3m, goodsItem1.BY_CustomsFourthQuantity);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_CustomsFourthUnitQty", "TD", goodsItem1.BY_CustomsFourthUnitQty);
					AssertEquals("Final Period - arrivalBillGoodsItem.BY_MonetaryValue", 10m, goodsItem1.BY_MonetaryValue);
					AssertEquals("Final Period - arrivalBillGoodsItem.AdditionalSupplementaryCodes", true, goodsItem1.AdditionalSupplementaryCodes.ContainsCode("V901"));
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AddLiabilityCalculationToGoodsItemArrival(goodsItem1);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_RN_NKCountryOfOrigin", "CA", goodsItem1.BY_RN_NKCountryOfOrigin);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_HarmonisedTariff", "11110", goodsItem1.BY_HarmonisedTariff);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsSecondQuantity", 10m, goodsItem1.BY_CustomsSecondQuantity);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsSecondUnitQty", "KA", goodsItem1.BY_CustomsSecondUnitQty);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsThirdQuantity", 20m, goodsItem1.BY_CustomsThirdQuantity);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsThirdUnitQty", "GA", goodsItem1.BY_CustomsThirdUnitQty);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsFourthQuantity", 30m, goodsItem1.BY_CustomsFourthQuantity);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_CustomsFourthUnitQty", "TA", goodsItem1.BY_CustomsFourthUnitQty);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.BY_MonetaryValue", 100m, goodsItem1.BY_MonetaryValue);
					AssertEquals("No final Period - Prereq: arrivalBillGoodsItem.AdditionalSupplementaryCodes", true, goodsItem1.AdditionalSupplementaryCodes.ContainsCode("V999"));

					nctsHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

					AssertEquals("No final Period - arrivalBillGoodsItem.BY_RN_NKCountryOfOrigin", "CA", goodsItem1.BY_RN_NKCountryOfOrigin);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_HarmonisedTariff", "11111", goodsItem1.BY_HarmonisedTariff);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsSecondQuantity", 10m, goodsItem1.BY_CustomsSecondQuantity);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsSecondUnitQty", ZString.Empty, goodsItem1.BY_CustomsSecondUnitQty);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsThirdQuantity", 20m, goodsItem1.BY_CustomsThirdQuantity);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsThirdUnitQty", "GA", goodsItem1.BY_CustomsThirdUnitQty);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsFourthQuantity", 30m, goodsItem1.BY_CustomsFourthQuantity);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_CustomsFourthUnitQty", "TA", goodsItem1.BY_CustomsFourthUnitQty);
					AssertEquals("No final Period - arrivalBillGoodsItem.BY_MonetaryValue", 100m, goodsItem1.BY_MonetaryValue);
					AssertEquals("No final Period - arrivalBillGoodsItem.AdditionalSupplementaryCodes", true, goodsItem1.AdditionalSupplementaryCodes.ContainsCode("V999"));
				}
			});
		}
	}

	public void TestLoadDataForUnloadingFromDeparture_GrossWeightMrnES()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var newEntryNumber = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "24ES00999830001277";
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		goodsItem1.BY_UnloadedState = "NEW";

		var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;
		goodsItem2.BY_UnloadedState = "NEW";

		var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
		goodsItem3.BY_LineNo = 3;
		goodsItem3.BY_UnloadedState = "NEW";

		var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_LineNo = 4;
		goodsItem4.BY_UnloadedState = "NEW";

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		departureGoodsItem1.BY_GrossWeight = 4;
		departureGoodsItem1.BY_GrossWeightUnit = "KG";
		AddPreviousDocumentToDepartureItem(departureGoodsItem1, 44, code: "ZZZ");

		var departureGoodsItem2 = departureBill.GoodsItems.AddNew();
		departureGoodsItem2.BY_LineNo = 2;
		departureGoodsItem2.BY_GrossWeight = 3;
		departureGoodsItem2.BY_GrossWeightUnit = "KG";
		AddPreviousDocumentToDepartureItem(departureGoodsItem2, 33, unit: "NAR");

		var departureGoodsItem3 = departureBill.GoodsItems.AddNew();
		departureGoodsItem3.BY_LineNo = 3;
		departureGoodsItem3.BY_GrossWeight = 2;
		departureGoodsItem3.BY_GrossWeightUnit = "LT";
		AddPreviousDocumentToDepartureItem(departureGoodsItem3, 0);

		var departureGoodsItem4 = departureBill.GoodsItems.AddNew();
		departureGoodsItem4.BY_LineNo = 4;
		departureGoodsItem4.BY_GrossWeight = 1;
		departureGoodsItem4.BY_GrossWeightUnit = "LT";
		AddPreviousDocumentToDepartureItem(departureGoodsItem4, 11);

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var billGoodsItems = arrivalHeader.Bills.First().ArrivalGoodsItems;
			AssertContainsExactElementsInAnyOrder("billGoodsItems (BY_LineNo, BY_GrossWeight, BY_GrossWeightUnit)",
													new (ZShort, ZDecimal, ZString)[]
													{
															(1, 4, "KG"),
															(2, 3, "KG"),
															(3, 2, "LT"),
															(4, 11, "KG")
													}, billGoodsItems.Select(x => (x.BY_LineNo, x.BY_GrossWeight, x.BY_GrossWeightUnit)));
		});

		void AddPreviousDocumentToDepartureItem(NctsDepartureCargoDesc departureGoodsItem, ZDecimal quantity, string code = "N337", string unit = "KGM")
		{
			var prevDoc = departureGoodsItem.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = code;
			prevDoc.CSI_Quantity = quantity;
			prevDoc.CSI_UnitOfQuantity = unit;
		}
	}

	public void TestLoadDataForUnloadingFromDeparture_GrossWeightMrnNoES()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var newEntryNumber = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "24FR00999830001277";
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		goodsItem1.BY_UnloadedState = "NEW";

		var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;
		goodsItem2.BY_UnloadedState = "NEW";

		var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
		goodsItem3.BY_LineNo = 3;
		goodsItem3.BY_UnloadedState = "NEW";

		var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_LineNo = 4;
		goodsItem4.BY_UnloadedState = "NEW";

		var departure = Factory.New<NctsHeader>();
		departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departure.SetMovementType(NctsMovementType.Codes.Departure);

		var departureBill = departure.Bills.AddNew();

		var departureGoodsItem1 = departureBill.GoodsItems.AddNew();
		departureGoodsItem1.BY_LineNo = 1;
		departureGoodsItem1.BY_GrossWeight = 4;
		departureGoodsItem1.BY_GrossWeightUnit = "KG";
		AddPreviousDocumentToDepartureItem(departureGoodsItem1, 44, code: "ZZZ");

		var departureGoodsItem2 = departureBill.GoodsItems.AddNew();
		departureGoodsItem2.BY_LineNo = 2;
		departureGoodsItem2.BY_GrossWeight = 3;
		departureGoodsItem2.BY_GrossWeightUnit = "KG";
		AddPreviousDocumentToDepartureItem(departureGoodsItem2, 33, unit: "NAR");

		var departureGoodsItem3 = departureBill.GoodsItems.AddNew();
		departureGoodsItem3.BY_LineNo = 3;
		departureGoodsItem3.BY_GrossWeight = 2;
		departureGoodsItem3.BY_GrossWeightUnit = "LT";
		AddPreviousDocumentToDepartureItem(departureGoodsItem3, 0);

		var departureGoodsItem4 = departureBill.GoodsItems.AddNew();
		departureGoodsItem4.BY_LineNo = 4;
		departureGoodsItem4.BY_GrossWeight = 1;
		departureGoodsItem4.BY_GrossWeightUnit = "LT";
		AddPreviousDocumentToDepartureItem(departureGoodsItem4, 11);

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: departureBill.SequenceNumber", (ZShort)1, departureBill.SequenceNumber);

			arrivalHeader.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture(departure);

			var billGoodsItems = arrivalHeader.Bills.First().ArrivalGoodsItems;
			AssertContainsExactElementsInAnyOrder("billGoodsItems (BY_LineNo, BY_GrossWeight, BY_GrossWeightUnit)",
													new (ZShort, ZDecimal, ZString)[]
													{
															(1, 4, "KG"),
															(2, 3, "KG"),
															(3, 2, "LT"),
															(4, 1, "LT")
													}, billGoodsItems.Select(x => (x.BY_LineNo, x.BY_GrossWeight, x.BY_GrossWeightUnit)));
		});

		void AddPreviousDocumentToDepartureItem(NctsDepartureCargoDesc departureGoodsItem, ZDecimal quantity, string code = "N337", string unit = "KGM")
		{
			var prevDoc = departureGoodsItem.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = code;
			prevDoc.CSI_Quantity = quantity;
			prevDoc.CSI_UnitOfQuantity = unit;
		}
	}

	public void TestTotalUnloadedNumberOfPackages()
	{
		var bulkType = Factory.SetupBulkCusCode();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = header.Bills.AddNew();

		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		package1.B5_UnitCount = 10;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package2.B5_UnitType = bulkType;

		var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		var package3 = goodsItem2.Packages.AddNew();
		package3.B5_UnitCount = 20;

		var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
		var package4 = goodsItem3.Packages.AddNew();
		package4.B5_UnitCount = 30;

		var bill2 = header.Bills.AddNew();

		var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		var package5 = goodsItem4.Packages.AddNew();
		package5.B5_UnitCount = 60;

		var goodsItem5 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem5.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var package6 = goodsItem5.Packages.AddNew();
		package6.B5_UnitCount = 10;

		AssertEquals(111, header.ArrivalMovementHeader.TotalUnloadedNumberOfPackages);
	}

	public void TestShouldGuaranteeForArrivalBeVisible_ES()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var registryRegisterEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled;
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			CombineAssertions(() =>
			{
				using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = false, ShouldGuaranteeForArrivalBeVisible is false", false, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);
				}

				using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = false, ShouldGuaranteeForArrivalBeVisible is true", true, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);
				}

				using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = true, ShouldGuaranteeForArrivalBeVisible is true", true, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);
				}

				using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					Factory.ClearCachedValue<bool>($"ArrivalGoodsLocationIsInPremises_ArrivalLoc_Y");
					AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is a premises with type ADT and location = ArrivalGoodsLocation ShouldGuaranteeForArrivalBeVisible is true", true, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);

					premises.SRP_Type = "AAA";
					Factory.ClearCachedValue<bool>($"ArrivalGoodsLocationIsInPremises_ArrivalLoc_Y");
					AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is a premises with location = ArrivalGoodsLocation but type is not ADT ShouldGuaranteeForArrivalBeVisible is false", false, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);

					premises.SRP_Type = "ADT";
					premises.SRP_CustomsLocation = "AAA";
					Factory.ClearCachedValue<bool>($"ArrivalGoodsLocationIsInPremises_ArrivalLoc_Y");
					AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is a premises with type ADT but location != ArrivalGoodsLocation ShouldGuaranteeForArrivalBeVisible is false", false, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);

					premises.Delete();
					Factory.ClearCachedValue<bool>($"ArrivalGoodsLocationIsInPremises_ArrivalLoc_Y");
					AssertEquals("When PRegisterEnabled = true and RegisterEnabledDeveloperOnly = true but there is no premises ShouldGuaranteeForArrivalBeVisible is false", false, arrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);
				}
			});
		}
	}

	public void TestCreateTemporaryStorageData_WithAllData_BillDEC()
	{
		AssertTemporaryStorageDataCreated("DEC");
	}

	public void TestCreateTemporaryStorageData_WithAllData_BillNEW()
	{
		AssertTemporaryStorageDataCreated("NEW");
	}

	public void TestCreateTemporaryStorageData_WithAllData_BillDIF()
	{
		AssertTemporaryStorageDataCreated("DIF");
	}

	public void TestCreateTemporaryStorageData_WithAllData_BillMIS()
	{
		var (arrivalMovementHeader, premisesPK, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage("MIS");

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is false", false, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders not created", 0, regHeaders.Length);

			var regLines = Factory.Load<CusTempStorageRegLine>(new ZQuery());
			AssertEquals("regLines not created", 0, regLines.Length);

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions not created", 0, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots not created", 0, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems not created", 0, regLineItems.Length);
		});
	}

	public void TestCreateTemporaryStorageData_WithAllData_MultipleBills()
	{
		SetUpTariffAndRates();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_RX_NKCurrency = "USD";

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var bill1 = arrivalHeader.Bills.AddNew();
		bill1.MovementDetail.B9_SeqNo = "1";
		bill1.MovementDetail.B9_UnloadedState = "NEW";

		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem1.SetValues("NEW", 1, 1, "0304798000", "CUSCODE1", "Description1", 2);

		var pack1 = goodsItem1.Packages.AddNew();
		pack1.SetValues("DEC", "BX", "marks1", 5, 2);

		var bill2 = arrivalHeader.Bills.AddNew();
		bill2.MovementDetail.B9_SeqNo = "2";
		bill2.MovementDetail.B9_UnloadedState = "DIF";

		var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem2.SetValues("DIF", 2, 4, "222222", "CUSCODE2", "Description2", 5, "0304798000", "CUSCODE2DIF", "Description2DIF", 10);

		var pack2 = goodsItem2.Packages.AddNew();
		pack2.SetValues("NEW", "FR", "marks4", 5, 0, vin: "VINCODE4", brand: "BRAND4", model: "MODEL4");

		var bill3 = arrivalHeader.Bills.AddNew();
		bill3.MovementDetail.B9_SeqNo = "3";
		bill3.MovementDetail.B9_UnloadedState = "MIS";

		var goodsItem3 = bill3.ArrivalGoodsItems.AddNew();
		goodsItem3.SetValues("NEW", 5, 7, "555555", "CUSCODE5", "Description5", 2);

		var pack3 = goodsItem3.Packages.AddNew();
		pack3.SetValues("NEW", "FR", "marks9", 5, 2, vin: "VINCODE9", model: "MODEL9");

		var bill4 = arrivalHeader.Bills.AddNew();
		bill4.MovementDetail.B9_SeqNo = "4";
		bill4.MovementDetail.B9_UnloadedState = "DEC";

		var goodsItem4 = bill4.ArrivalGoodsItems.AddNew();
		goodsItem4.SetValues("DEC", 1, 2, "444444", "CUSCODE4", "Description4", 0);

		var pack4 = goodsItem4.Packages.AddNew();
		pack4.SetValues("DEC", "CT", "marks4", 4, 3);

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 297.20m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "FR22222222", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE4:BRAND4:MODEL4", "FR22222222", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "FR22222222", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", 2, 5, 148.6m, 99001, "0304798000", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "VINCODE4:BRAND4:MODEL4", 10, 5, 148.6m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF");

			AssertRegLineWithOnePivot(regLines, "marks4", 3, 4, ZDecimal.Zero, 2, "444444", "CUSCODE4", "Description4");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 3, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 3, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 3, regLineItems.Length);
		});
	}

	public void TestCreateTemporaryStorageData_WithAllData_LiabilityAmountFromGuaranteeZeroAndPW_OverrideFalse()
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = ZDecimal.Zero;
		guarantee.PW_RX_NKCurrency = "USD";
		guarantee.PW_Override = true;

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		SetUpBillAndGoodsItemsToCreateTemporaryStorage(arrivalHeader, "NEW");

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", ZDecimal.Zero, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks2dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE5:BRAND5:MODEL5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE9::MODEL9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("AA", "marks11", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks12", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks13dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks15", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks16", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks17", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithTwoPivots(regLines, "marks1", 5, 5, ZDecimal.Zero,
										2, 99001, "0304798000", "CUSCODE1", "Description1",
										3, 99001, "0304798000", "CUSCODE4", "Description4");

			AssertRegLineWithOnePivot(regLines, "marks2dif", 4, 7, ZDecimal.Zero, 99001, "0304798000", "CUSCODE1", "Description1");

			AssertRegLineWithTwoPivots(regLines, "marks4", 7.2m, 2, ZDecimal.Zero,
										2.2m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF",
										5, 4, "66666612", "CUSCODE6DIF", "Description6DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE5:BRAND5:MODEL5", 8.8m, 8, ZDecimal.Zero, 4, "0304798000", "CUSCODE2DIF", "Description2DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE9::MODEL9", 2, 5, ZDecimal.Zero, 99002, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks11", 6, 0, ZDecimal.Zero, 10, "88888812", "CUSCODE8DIF", "Description8DIF");

			AssertRegLineWithOnePivot(regLines, "marks12", 15.48847m, 5, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks13dif", 21.68384m, 7, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks15", 3.09769m, 0, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks16", 5.00000, 7, ZDecimal.Zero, 11, "123123", "CUSCODE10", "Description10");

			AssertRegLineWithOnePivot(regLines, "marks17", 5, 7, ZDecimal.Zero, 12, "12312444", "CUSCODE11DIFF", "Description11DIF");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 11, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 13, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 9, regLineItems.Length);
		});
	}

	public void TestCreateTemporaryStorageData_WithAllData_LiabilityAmount()
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000m;
		guarantee.PW_RX_NKCurrency = "USD";
		guarantee.PW_Override = true;

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		SetUpBillAndGoodsItemsToCreateTemporaryStorage(arrivalHeader, "NEW");

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks2dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE5:BRAND5:MODEL5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE9::MODEL9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("AA", "marks11", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks12", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks13dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks15", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks16", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks17", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithTwoPivots(regLines, "marks1", 5, 5, 120.10m,
										2, 99001, "0304798000", "CUSCODE1", "Description1",
										3, 99001, "0304798000", "CUSCODE4", "Description4");

			AssertRegLineWithOnePivot(regLines, "marks2dif", 4, 7, 96.07m, 99001, "0304798000", "CUSCODE1", "Description1");

			AssertRegLineWithTwoPivots(regLines, "marks4", 7.2m, 2, 172.94m,
										2.2m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF",
										5, 4, "66666612", "CUSCODE6DIF", "Description6DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE5:BRAND5:MODEL5", 8.8m, 8, 211.36m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE9::MODEL9", 2, 5, 48.04m, 99002, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks11", 6, 0, 144.1m, 10, "88888812", "CUSCODE8DIF", "Description8DIF");

			AssertRegLineWithOnePivot(regLines, "marks12", 15.48847m, 5, 372.01m, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks13dif", 21.68384m, 7, 520.8m, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks15", 3.09769m, 0, 74.4m, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks16", 5.00000, 7, 120.09m, 11, "123123", "CUSCODE10", "Description10");

			AssertRegLineWithOnePivot(regLines, "marks17", 5, 7, 120.09m, 12, "12312444", "CUSCODE11DIFF", "Description11DIF");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 11, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 13, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 9, regLineItems.Length);
		});
	}

	public void TestCreateTemporaryStorageData_WithAllData_LiabilityAmountFromGuarantee_WithLiabilityNotDivided()
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000m;
		guarantee.PW_RX_NKCurrency = "USD";
		guarantee.PW_Override = true;

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";
		bill.MovementDetail.B9_UnloadedState = "NEW";

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.SetValues("NEW", 1, 1, "0304798000", "CUSCODE1", "Description1", 6);

		var pack1 = goodsItem1.Packages.AddNew();
		pack1.SetValues("DEC", "BX", "marks1", 5, 2);

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", 2.00000m, 5, 2000m, 99001, "0304798000", "CUSCODE1", "Description1");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 1, regLineItems.Length);
		});
	}

	public void TestCreateTemporaryStorageData_CorrectGrossWeightInItemPivots()
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000m;
		guarantee.PW_RX_NKCurrency = "USD";
		guarantee.PW_Override = true;

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var bill = arrivalHeader.Bills.AddNew();
		bill.UnloadedStatus = "DEC";
		bill.B0_Weight = 2;
		bill.B0_WeightUQ = "KG";

		var goods1 = bill.ArrivalGoodsItems.AddNew();
		goods1.BY_UnloadedState = "DEC";
		goods1.BY_GrossWeight = 1;
		goods1.BY_GrossWeightUnit = "KG";
		goods1.BY_NetWeight = 1;
		goods1.BY_NetWeightUnit = "KG";
		goods1.BY_FormattedHarmonisedTariff = "7318.29.00";

		var pack1_1 = goods1.Packages.AddNew();
		pack1_1.UnloadedStatus = "DEC";
		pack1_1.B5_UnitType = "BX";
		pack1_1.B5_UnitCount = 15;
		pack1_1.B5_GrossWeight = 0.74;
		pack1_1.B5_GrossWeightUQ = "KG";
		pack1_1.B5_MarksAndNumbers = "rtdas";

		var goods2 = bill.ArrivalGoodsItems.AddNew();
		goods2.BY_UnloadedState = "DEC";
		goods2.BY_GrossWeight = 0;
		goods2.BY_GrossWeightUnit = "KG";
		goods2.BY_NetWeight = 12;
		goods2.BY_NetWeightUnit = "KG";
		goods2.BY_FormattedHarmonisedTariff = "1801.00.00 00";

		var pack2_1 = goods2.Packages.AddNew();
		pack2_1.UnloadedStatus = "DEC";
		pack2_1.B5_UnitType = "BX";
		pack2_1.B5_UnitCount = 15;
		pack2_1.B5_GrossWeight = 0.25;
		pack2_1.B5_GrossWeightUQ = "KG";
		pack2_1.B5_MarksAndNumbers = "rtdas";

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var line = regHeaders[0].CusTempStorageRegLines.Single();
			var pivot = line.RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			Assert("Correct Gross Weight First item 0.74000", pivot.Any(x => x.SRV_GrossWeight == 0.74000m));
			Assert("Correct Gross Weight Second item 0.25000", pivot.Any(x => x.SRV_GrossWeight == 0.25000m));
		});
	}

	public void TestCreateTemporaryStorageData_CorrectGrossWeightInItemPivotsWhenPackQtyInFirstItemAndZeroInRest()
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000m;
		guarantee.PW_RX_NKCurrency = "USD";
		guarantee.PW_Override = true;

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";

		var bill = arrivalHeader.Bills.AddNew();
		bill.UnloadedStatus = "DEC";
		bill.B0_Weight = 2;
		bill.B0_WeightUQ = "KG";

		var goods1 = bill.ArrivalGoodsItems.AddNew();
		goods1.BY_UnloadedState = "DEC";
		goods1.BY_GrossWeight = 3;
		goods1.BY_GrossWeightUnit = "KG";
		goods1.BY_NetWeight = 1;
		goods1.BY_NetWeightUnit = "KG";
		goods1.BY_FormattedHarmonisedTariff = "7318.29.00";

		var pack1_1 = goods1.Packages.AddNew();
		pack1_1.UnloadedStatus = "DEC";
		pack1_1.B5_UnitType = "BX";
		pack1_1.B5_UnitCount = 15;
		pack1_1.B5_GrossWeight = ZDecimal.Zero;
		pack1_1.B5_GrossWeightUQ = ZString.Empty;
		pack1_1.B5_MarksAndNumbers = "rtdas";

		var goods2 = bill.ArrivalGoodsItems.AddNew();
		goods2.BY_UnloadedState = "DEC";
		goods2.BY_GrossWeight = 2;
		goods2.BY_GrossWeightUnit = "KG";
		goods2.BY_NetWeight = 12;
		goods2.BY_NetWeightUnit = "KG";
		goods2.BY_FormattedHarmonisedTariff = "1801.00.00 00";

		var pack2_1 = goods2.Packages.AddNew();
		pack2_1.UnloadedStatus = "DEC";
		pack2_1.B5_UnitType = "BX";
		pack2_1.B5_UnitCount = ZLong.Zero;
		pack2_1.B5_GrossWeight = ZDecimal.Zero;
		pack2_1.B5_GrossWeightUQ = ZString.Empty;
		pack2_1.B5_MarksAndNumbers = "rtdas";

		var goods3 = bill.ArrivalGoodsItems.AddNew();
		goods3.BY_UnloadedState = "DEC";
		goods3.BY_GrossWeight = 1;
		goods3.BY_GrossWeightUnit = "KG";
		goods3.BY_NetWeight = 12;
		goods3.BY_NetWeightUnit = "KG";
		goods3.BY_FormattedHarmonisedTariff = "1805.00.00 00";

		var pack3_1 = goods3.Packages.AddNew();
		pack3_1.UnloadedStatus = "DEC";
		pack3_1.B5_UnitType = "VG";
		pack3_1.B5_UnitCount = ZLong.Zero;
		pack3_1.B5_GrossWeight = ZDecimal.Zero;
		pack3_1.B5_GrossWeightUQ = ZString.Empty;
		pack3_1.B5_MarksAndNumbers = "rtdas";

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var line1 = regHeaders[0].CusTempStorageRegLines.Single(x => x.SRL_LineNumber == 1);
			var line2 = regHeaders[0].CusTempStorageRegLines.Single(x => x.SRL_LineNumber == 2);
			var line3 = regHeaders[0].CusTempStorageRegLines.Single(x => x.SRL_LineNumber == 3);
			var pivot1 = line1.RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			var pivot2 = line2.RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			var pivot3 = line3.RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
			Assert("Correct Gross Weight First item 3", pivot1.Any(x => x.SRV_GrossWeight == 3.0m));
			Assert("Correct Gross Weight Second item 2", pivot2.Any(x => x.SRV_GrossWeight == 2.0m));
			Assert("Correct Gross Weight Third item 1", pivot3.Any(x => x.SRV_GrossWeight == 1.0m));
		});
	}

	public void TestPopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage()
	{
		var org1 = Factory.New<OrgHeader>();
		org1.OH_Code = "REP";
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.ArrivalMovementHeader.Representative.OrganisationPK = org1.PK;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises.SRP_Code = "SRPC";
		premises.SRP_Description = "SRPD";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.Address1 = "Address1";
		orgAddress.OA_OH = org1.PK;
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		ZString customsLocation1 = "ArrivalLoc";
		premises.SRP_CustomsLocation = customsLocation1;

		const string guaranteeNumber1 = "Guarantee1";
		const string guaranteeNumber2 = "Guarantee2";

		var registryPNTSEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		CombineAssertions(() =>
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var guarantee1 = Factory.New<CusGuaranteeHeader>();
				guarantee1.CPH_Number = guaranteeNumber1;
				guarantee1.CPH_OH_PermitHolder = org1.PK;
				guarantee1.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee1.CPH_StartDate = ZDate.BrettsBirthday;
				var rule1 = guarantee1.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule1.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = customsLocation1;
				AssertEquals("PNTS Enabled, 1 Guarantee, PW_Bond empty", guaranteeNumber1, arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber);

				arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber = guaranteeNumber2;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = customsLocation1;
				AssertEquals("PNTS Enabled, 1 Guarantee, PW_Bond not empty", guaranteeNumber2, arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber);

				var guarantee2 = Factory.New<CusGuaranteeHeader>();
				guarantee2.CPH_Number = guaranteeNumber2;
				guarantee2.CPH_OH_PermitHolder = org1.PK;
				guarantee2.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee2.CPH_StartDate = ZDate.BrettsBirthday;
				var rule2 = guarantee2.CusGuaranteeRules.AddNew();
				rule2.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule2.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber = ZString.Empty;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = customsLocation1;
				AssertNullOrEmpty("PNTS Enabled, 2 Guarantee, PW_Bond empty", arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber);

				guarantee1.Delete();
				guarantee2.Delete();
				Factory.Save();
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = customsLocation1;
				AssertNullOrEmpty("PNTS Enabled, no Guarantee, PW_Bond empty", arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var guarantee1 = Factory.New<CusGuaranteeHeader>();
				guarantee1.CPH_Number = guaranteeNumber1;
				guarantee1.CPH_OH_PermitHolder = org1.PK;
				guarantee1.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee1.CPH_StartDate = ZDate.BrettsBirthday;
				var rule1 = guarantee1.CusGuaranteeRules.AddNew();
				rule1.CPR_ValueFrom = customsLocation1;
				rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule1.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				arrivalHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = customsLocation1;
				AssertNullOrEmpty("PNTS disabled", arrivalHeader.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber);
			}
		});
	}

	void AssertTemporaryStorageDataCreated(ZString billState)
	{
		var (arrivalMovementHeader, premisesPK, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(billState);

		CombineAssertions(() =>
		{
			var response = arrivalMovementHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 445.8m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeaderPK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks2dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE5:BRAND5:MODEL5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE9::MODEL9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("AA", "marks11", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks12", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks13dif", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks15", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks16", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks17", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithTwoPivots(regLines, "marks1", 5, 5, 198.14m,
										2, 99001, "0304798000", "CUSCODE1", "Description1",
										3, 99001, "0304798000", "CUSCODE4", "Description4");

			AssertRegLineWithOnePivot(regLines, "marks2dif", 4, 7, 99.06m, 99001, "0304798000", "CUSCODE1", "Description1");

			AssertRegLineWithTwoPivots(regLines, "marks4", 7.2m, 2, 29.72m,
										2.2m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF",
										5, 4, "66666612", "CUSCODE6DIF", "Description6DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE5:BRAND5:MODEL5", 8.8m, 8, 118.88m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF");

			AssertRegLineWithOnePivot(regLines, "VINCODE9::MODEL9", 2, 5, ZDecimal.Zero, 99002, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks11", 6, 0, ZDecimal.Zero, 10, "88888812", "CUSCODE8DIF", "Description8DIF");

			AssertRegLineWithOnePivot(regLines, "marks12", 15.48847m, 5, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks13dif", 21.68384m, 7, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			AssertRegLineWithOnePivot(regLines, "marks15", 3.09769m, 0, ZDecimal.Zero, 1, "999999", "CUSCODE9", "Description9");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());

			AssertRegLineWithOnePivot(regLines, "marks17", 5, 7, 0, 12, "12312444", "CUSCODE11DIFF", "Description11DIF");

			AssertEquals("regLineTransactions created", 11, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 13, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 9, regLineItems.Length);
		});
	}

	void AssertRegLineWithOnePivot(CusTempStorageRegLineCollection regLines, ZString marks, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], grossWeight, packageQty, bondAmount);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 1, regLinePivots.Length);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivots[0], grossWeight, itemNumber, tariff, cusCode, description);
	}

	void AssertRegLineWithTwoPivots(CusTempStorageRegLineCollection regLines, ZString marks, ZDecimal transactionGrossWeight, ZInt packageQty, ZDecimal transactionBondAmount,
									ZDecimal grossWeightPivot1, ZInt itemNumberPivot1, ZString tariffPivot1, ZString cusCodePivot1, ZString descriptionPivot1,
									ZDecimal grossWeightPivot2, ZInt itemNumberPivot2, ZString tariffPivot2, ZString cusCodePivot2, ZString descriptionPivot2)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], transactionGrossWeight, packageQty, transactionBondAmount);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 2, regLinePivots.Length);
		var regLinePivot1 = regLinePivots.First(x => x.SRV_GrossWeight == grossWeightPivot1);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivot1, grossWeightPivot1, itemNumberPivot1, tariffPivot1, cusCodePivot1, descriptionPivot1);
		var regLinePivot2 = regLinePivots.First(x => x.SRV_GrossWeight == grossWeightPivot2);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot2", regLinePivot2, grossWeightPivot2, itemNumberPivot2, tariffPivot2, cusCodePivot2, descriptionPivot2);
	}

	void AssertRegLineTransaction(ZString assertMessage, CusTempStorageRegLineTransaction transaction, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount)
	{
		AssertEquals(assertMessage + ".SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals(assertMessage + ".SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals(assertMessage + ".SRT_TransactionType", "OBL", transaction.SRT_TransactionType);
		AssertEquals(assertMessage + ".SRT_InternalReferenceNumber", "NCT00000001", transaction.SRT_InternalReferenceNumber);
		AssertEquals(assertMessage + ".SRT_InternalReferenceType", "TRA", transaction.SRT_InternalReferenceType);
		AssertEquals(assertMessage + ".SRT_TransactionDate", new ZDateTimeOffset(2023, 01, 01, 02, 01, 00), transaction.SRT_TransactionDate);
		AssertEquals(assertMessage + ".SRT_PhysicalInOutDate", new ZDateTimeOffset(2022, 02, 15, 16, 43, 27), transaction.SRT_PhysicalInOutDate);
		AssertEquals(assertMessage + ".SRT_BondAmount", bondAmount, transaction.SRT_BondAmount);
	}

	void AssertRegLineItemPivotAndItem(ZString assertMessage, EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot pivot, ZDecimal grossWeight, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		AssertEquals(assertMessage + ".SRV_GrossWeight", grossWeight, pivot.SRV_GrossWeight);
		var regLineItem = pivot.RegLineItem;
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsItemNumber", itemNumber, regLineItem.SRI_GoodsItemNumber);
		AssertEquals(assertMessage + ".RegLineItem.SRI_Tariff", tariff, regLineItem.SRI_Tariff);
		AssertEquals(assertMessage + ".RegLineItem.SRI_CusC4Number", cusCode, regLineItem.SRI_CusC4Number);
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsDescription", description, regLineItem.SRI_GoodsDescription);
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForTemporaryStorage()
	{
		EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "ArrivalTest";

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.SummaryEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovement = arrivalHeader.ArrivalMovementHeader;

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var nctsGuarantee = arrivalMovement.SingleGuaranteeForArrival;
		nctsGuarantee.PW_BondNumber = "Test1";
		nctsGuarantee.PW_BondAmount = 5.0m;

		var arrivalGuarantee = arrivalMovement.GuaranteesForArrival[0];

		CombineAssertions(() =>
		{
			AssertEquals("[PRE-CONDITION] SingleGuaranteeForArrival are loaded from same PK", nctsGuarantee.PK, arrivalGuarantee.PK);
			var cusGuaranteeHeader = arrivalGuarantee.CusGuarantee;
			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			arrivalMovement.AddNewGuaranteeTransactionForTemporaryStorage();

			var transactions = nctsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transaction = transactions.FirstOrDefault();
			AssertEquals("With Liability Amount not 0, transaction is created", 1, transactions.Count());
			AssertEquals("Transaction Date", new ZDateTime(2023, 01, 01, 02, 01, 00), transaction.CPL_TransactionDate);
			AssertEquals("Transaction Type", Customs.Business.PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
			AssertEquals("Reference", "99982000174", transaction.CPL_Reference);
			AssertEquals("Value", -5.0m, transaction.CPL_TranValue);
			AssertEquals("Comment", "NCTS Arrival ArrivalTest. MRN: 1234567890", transaction.CPL_Comment);
			AssertEquals("Status", "CON", transaction.CPL_TransactionStatus);

			arrivalGuarantee.PW_BondAmount = 0.0m;
			arrivalMovement.AddNewGuaranteeTransactionForTemporaryStorage();
			AssertEquals("With Liability Amount 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			arrivalGuarantee.PW_BondAmount = 5.0m;
			arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000175";
			arrivalHeader.SummaryEntryNumber.CE_IssueDate = ZDateTime.Empty;
			arrivalMovement.AddNewGuaranteeTransactionForTemporaryStorage();
			transactions = nctsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99982000175");
			transaction = transactions.FirstOrDefault();
			AssertEquals("Transaction Date is Today if Summary Date is empty", new ZDateTime(2024, 01, 01, 10, 11, 12), transaction.CPL_TransactionDate);
		});
	}

	(NctsArrivalMovementHeader arrivalMovementHeader, ZGuid premisesPK, ZGuid guaranteePK) SetUpDataToCreateTemporaryStorage(ZString billState)
	{
		SetUpTariffAndRates();
		Factory.SetBulkTypeHelper();

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalHeader.BH_JobReference = "NCT00000001";

		var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		arrivalHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;

		arrivalHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		arrivalHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		arrivalHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader1 = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader1.CPH_Number = "GUARANTEEREF";
		cusPermitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader1.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader1.CPH_Type = "TST";
		cusPermitHeader1.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader1.CPH_EndDate = ZDate.Today.AddDays(1);

		var cusPermitHeader2 = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader2.CPH_Number = "GUARANTEEREF";
		cusPermitHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader2.CPH_OH_PermitHolder = orgHeaderDestinationTraderArrival.PK;
		cusPermitHeader2.CPH_Type = "TRA";
		cusPermitHeader2.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader2.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_RX_NKCurrency = "USD";

		arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Type = "ADT";
		premises1.SRP_CustomsLocation = "ArrivalLoc";
		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Type = "TST";
		premises2.SRP_CustomsLocation = "ArrivalLoc";
		var premises3 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises3.SRP_Type = "ADT";
		premises3.SRP_CustomsLocation = "ArrivalLocExtra";

		SetUpBillAndGoodsItemsToCreateTemporaryStorage(arrivalHeader, billState);

		return (arrivalMovementHeader, premises1.PK, cusPermitHeader1.PK);
	}

	void SetUpBillAndGoodsItemsToCreateTemporaryStorage(NctsHeader arrivalHeader, ZString billState)
	{
		var bill = arrivalHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";
		bill.MovementDetail.B9_UnloadedState = billState;

		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.SetValues("NEW", 1, 1, "0304798000", "CUSCODE1", "Description1", 6);

		var pack1 = goodsItem1.Packages.AddNew();
		pack1.SetValues("DEC", "BX", "marks1", 5, 2);

		var pack2 = goodsItem1.Packages.AddNew();
		pack2.SetValues("DIF", "BX", "marks2", 6, 4, typeDif: "CT", marksDif: "marks2dif", qtyDif: 7);

		var pack3 = goodsItem1.Packages.AddNew();
		pack3.SetValues("MIS", "VG", "marks3", 4, 5);

		var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
		goodsItem2.SetValues("DIF", 2, 4, "222222", "CUSCODE2", "Description2", 5, "0304798000", "CUSCODE2DIF", "Description2DIF", 11);

		var pack4 = goodsItem2.Packages.AddNew();
		pack4.SetValues("NEW", "CT", "marks4", 2, 0);

		var pack5 = goodsItem2.Packages.AddNew();
		pack5.SetValues("DIF", "BX", "marks5", 6, 0, typeDif: "FR", marksDif: "marks5dif", vinDif: "VINCODE5", brandDif: "BRAND5", modelDif: "MODEL5", qtyDif: 8);

		var pack6 = goodsItem2.Packages.AddNew();
		pack6.SetValues("MIS", "VG", "marks6", 4, 5);

		var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
		goodsItem3.SetValues("MIS", 3, 5, "333333", "CUSCODE3", "Description3", 3);

		var pack7 = goodsItem3.Packages.AddNew();
		pack7.SetValues("NEW", "BX", "marks7", 5, 2);

		var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
		goodsItem4.SetValues("NEW", 4, 6, "0304798000", "CUSCODE4", "Description4", 0);

		var pack8 = goodsItem4.Packages.AddNew();
		pack8.SetValues("NEW", "BX", "marks1", 0, 3);

		var goodsItem5 = bill.ArrivalGoodsItems.AddNew();
		goodsItem5.SetValues("NEW", 5, 7, "555555", "CUSCODE5", "Description5", 2);

		var pack9 = goodsItem5.Packages.AddNew();
		pack9.SetValues("NEW", "FR", "marks9", 5, 2, vin: "VINCODE9", model: "MODEL9");

		var goodsItem6 = bill.ArrivalGoodsItems.AddNew();
		goodsItem6.SetValues("DIF", 6, 8, "666666", "CUSCODE6", "Description6", 5, "66666612", "CUSCODE6DIF", "Description6DIF", 0);

		var pack10 = goodsItem6.Packages.AddNew();
		pack10.SetValues("NEW", "CT", "marks4", 0, 5);

		var goodsItem7 = bill.ArrivalGoodsItems.AddNew();
		goodsItem7.SetValues("NEW", 8, 9, "777777", "CUSCODE7", "Description7", 10);

		var goodsItem8 = bill.ArrivalGoodsItems.AddNew();
		goodsItem8.SetValues("DIF", 9, 10, "888888", "CUSCODE8", "Description8", 5, "88888812", "CUSCODE8DIF", "Description8DIF", 0);

		var pack11 = goodsItem8.Packages.AddNew();
		pack11.SetValues("NEW", "AA", "marks11", 0, 6);

		var goodsItem9 = bill.ArrivalGoodsItems.AddNew();
		goodsItem9.SetValues("DEC", 1, 1, "999999", "CUSCODE9", "Description9", 40.27m);

		var pack12 = goodsItem9.Packages.AddNew();
		pack12.SetValues("NEW", "BX", "marks12", 5, 0);

		var pack13 = goodsItem9.Packages.AddNew();
		pack13.SetValues("DIF", "BX", "marks13", 6, 0, typeDif: "CT", marksDif: "marks13dif", qtyDif: 7);

		var pack14 = goodsItem9.Packages.AddNew();
		pack14.SetValues("MIS", "VG", "marks14", 4, 0);

		var pack15 = goodsItem9.Packages.AddNew();
		pack15.SetValues("DEC", "VG", "marks15", 0, 0);

		var goodsItem10 = bill.ArrivalGoodsItems.AddNew();
		goodsItem10.SetValues("DEC", 10, 11, "100000", "CUSCODE10", "Description10", 40.30m);
		goodsItem10.LiabilityTariff = "123123";

		var pack16 = goodsItem10.Packages.AddNew();
		pack16.SetValues("NEW", "BX", "marks16", 7, 5);

		var goodsItem11 = bill.ArrivalGoodsItems.AddNew();
		goodsItem11.SetValues("DIF", 11, 12, "100001", "CUSCODE11", "Description11", 40.30m, "10000112", "CUSCODE11DIFF", "Description11DIF", 0);
		goodsItem11.LiabilityTariff = "12312444";

		var pack17 = goodsItem11.Packages.AddNew();
		pack17.SetValues("NEW", "BX", "marks17", 7, 5);
	}

	void AddUnloadingDataToArrival(NctsHeader nctsHeader)
	{
		AddNewTransportInfosToArrivalHeader(nctsHeader);

		AddContainersToArrival(nctsHeader);
		var container2 = nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().FirstOrDefault(x => x.BC_ContainerNum == "CONTAINER3");

		var bill1 = nctsHeader.Bills.AddNew();
		bill1.MovementDetail.B9_SeqNo = "1";
		bill1.MovementDetail.B9_UnloadedState = "NEW";
		bill1.B0_Weight = 10m;
		bill1.B0_WeightUQ = "LT";
		AddGoodsItemsToBill(bill1, container2);

		var bill2 = nctsHeader.Bills.AddNew();
		bill2.MovementDetail.B9_SeqNo = "2";
		bill2.MovementDetail.B9_UnloadedState = "NEW";
		bill2.B0_Weight = 15m;
		bill2.B0_WeightUQ = "T";
		AddGoodsItemsToBill(bill2, container2);

		void AddGoodsItemsToBill(NctsBill bill, NctsArrivalHeaderContainer container)
		{
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_LineNo = 2;
			goodsItem1.BY_DeclarationGoodsItemNumber = 5;
			goodsItem1.BY_UnloadedState = "NEW";
			goodsItem1.BY_Description = "somedescription";
			goodsItem1.BY_CusC4Number = "AAA";
			goodsItem1.BY_HarmonisedTariff = "11111111";
			goodsItem1.BY_GrossWeight = 4;
			goodsItem1.BY_GrossWeightUnit = "HG";
			goodsItem1.BY_NetWeight = 6;
			goodsItem1.BY_NetWeightUnit = "G";

			AddPackagesToGoodsItemArrival(goodsItem1, container);
			AddSupportingDocumentsToGoodsItemArrival(goodsItem1);
			AddTransportDocumentsToGoodsItemArrival(goodsItem1);
			AddReferenceDocumentsToGoodsItemArrival(goodsItem1);

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_LineNo = 3;
			goodsItem2.BY_DeclarationGoodsItemNumber = 6;
			goodsItem2.BY_UnloadedState = "NEW";
			goodsItem2.BY_Description = "somedescription2";
			goodsItem2.BY_CusC4Number = "BBB";
			goodsItem2.BY_HarmonisedTariff = "22222222";
			goodsItem2.BY_GrossWeight = 3;
			goodsItem2.BY_GrossWeightUnit = "T";
			goodsItem2.BY_NetWeight = 7;
			goodsItem2.BY_NetWeightUnit = "LT";

			AddPackagesToGoodsItemArrival(goodsItem2, container);
			AddSupportingDocumentsToGoodsItemArrival(goodsItem2);
			AddTransportDocumentsToGoodsItemArrival(goodsItem2);
			AddReferenceDocumentsToGoodsItemArrival(goodsItem2);
		}
	}

	public void TestB5_GrossWeightIsNotReadOnlyRegardlessOfCustomsStatus()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.BM_Phase = NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		var package1 = goodsItem.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package1.B5_GrossWeight = 10;
		var package2 = goodsItem.Packages.AddNew();
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		package2.B5_GrossWeight = 20;
		var package3 = goodsItem.Packages.AddNew();
		package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package3.B5_GrossWeight = 20;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Package 1 when BM_NoChangesToReport = false, type of difference MIS and without BM_CustomsStatus, is readOnly", true, package1.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 2 when BM_NoChangesToReport = false, type of difference DEC and without BM_CustomsStatus, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 3 when BM_NoChangesToReport = false, type of difference DIF and without BM_CustomsStatus, is Editable", false, package3.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			AssertEquals("Package 1 when BM_NoChangesToReport = false, type of difference MIS and BM_CustomsStatus = UAP, is readOnly", true, package1.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 2 when BM_NoChangesToReport = false, type of difference DEC and BM_CustomsStatus = UAP, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 3 when BM_NoChangesToReport = false, type of difference DIF and BM_CustomsStatus = UAP, is Editable", false, package3.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			AssertEquals("Package 1 when BM_NoChangesToReport = false, type of difference MIS and BM_CustomsStatus = CL1, is readOnly", true, package1.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 2 when BM_NoChangesToReport = false, type of difference DEC and BM_CustomsStatus = CL1, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 3 when BM_NoChangesToReport = false, type of difference DIF and BM_CustomsStatus = CL1, is Editable", false, package3.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
			AssertEquals("Package 1 when BM_NoChangesToReport = false, type of difference MIS and BM_CustomsStatus = CD4, is readOnly", true, package1.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 2 when BM_NoChangesToReport = false, type of difference DEC and BM_CustomsStatus = CD4, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);
			AssertEquals("Package 3 when BM_NoChangesToReport = false, type of difference DIF and BM_CustomsStatus = CD4, is Editable", false, package3.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			AssertEquals("Package 2 when BM_NoChangesToReport = true (Only DEC), type of difference DEC and without BM_CustomsStatus, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			AssertEquals("Package 2 when BM_NoChangesToReport = true (Only DEC), type of difference DEC and BM_CustomsStatus = UAP, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			AssertEquals("Package 2 when BM_NoChangesToReport = true (Only DEC), type of difference DEC and BM_CustomsStatus = CL1, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
			AssertEquals("Package 2 when BM_NoChangesToReport = true (Only DEC), type of difference DEC and BM_CustomsStatus = CD4, is Editable", false, package2.B5_GrossWeightInfo.ReadOnly);
		});
	}

	void AddNewTransportInfosToArrivalHeader(NctsHeader nctsHeader)
	{
		var transpInfo1 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
		transpInfo1.TPM_SequenceNumber = 2;
		transpInfo1.TPM_TransportState = "NEW";
		transpInfo1.TPM_TypeOfIdentification = "10";
		transpInfo1.TPM_IdentificationNumber = "transport";
		transpInfo1.TPM_RN_NKTransportNationality = "FR";

		var transpInfo2 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
		transpInfo2.TPM_SequenceNumber = 3;
		transpInfo2.TPM_TransportState = "NEW";
		transpInfo2.TPM_TypeOfIdentification = "11";
		transpInfo2.TPM_IdentificationNumber = "transport2";
		transpInfo2.TPM_RN_NKTransportNationality = "DE";
	}

	void AddContainersToArrival(NctsHeader nctsHeader)
	{
		var container1 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_SequenceNumber = 2;
		container1.BC_UnloadedState = "NEW";
		container1.BC_ContainerNum = "CONTAINER2";
		container1.BC_Mode = "CNT";
		var seal1 = container1.Seals.AddNew();
		seal1.BK_SequenceNumber = 2;
		seal1.BK_UnloadingState = "NEW";
		seal1.BK_SealNumber = "SEAL11";
		var seal2 = container1.Seals.AddNew();
		seal2.BK_SequenceNumber = 3;
		seal2.BK_UnloadingState = "NEW";
		seal2.BK_SealNumber = "SEAL12";

		var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container2.BC_SequenceNumber = 3;
		container2.BC_UnloadedState = "NEW";
		container2.BC_ContainerNum = "CONTAINER3";
		container2.BC_Mode = "CNT";
		var seal3 = container2.Seals.AddNew();
		seal3.BK_SequenceNumber = 1;
		seal3.BK_UnloadingState = "NEW";
		seal3.BK_SealNumber = "SEAL22";
	}

	void AddPackagesToGoodsItemArrival(NctsArrivalCargoDesc goodsItem, NctsArrivalHeaderContainer container)
	{
		var pack1 = goodsItem.Packages.AddNew();
		pack1.B5_SequenceNumber = 2;
		pack1.B5_TypeOfDifference = "NEW";
		pack1.B5_UnitType = "CT";
		pack1.B5_UnitCount = 3;
		pack1.B5_MarksAndNumbers = "marks1";
		pack1.ContainersPivot.AddPivotFor(container);

		var pack2 = goodsItem.Packages.AddNew();
		pack2.B5_SequenceNumber = 3;
		pack2.B5_TypeOfDifference = "NEW";
		pack2.B5_UnitType = "BX";
		pack2.B5_UnitCount = 2;
		pack2.B5_MarksAndNumbers = "marks2";
		pack2.ContainersPivot.AddPivotFor(container);
	}

	void AddSupportingDocumentsToGoodsItemArrival(NctsArrivalCargoDesc goodsItem)
	{
		var supDoc1 = goodsItem.SupportingDocuments.AddNew();
		supDoc1.CSI_LineNo = 2;
		supDoc1.CSI_Status = "NEW";
		supDoc1.CSI_Code = "1234";
		supDoc1.CSI_ReferenceNumber = "REFERENCE1";

		var supDoc2 = goodsItem.SupportingDocuments.AddNew();
		supDoc2.CSI_LineNo = 3;
		supDoc2.CSI_Status = "NEW";
		supDoc2.CSI_Code = "4321";
		supDoc2.CSI_ReferenceNumber = "REFERENCE2";
	}

	void AddTransportDocumentsToGoodsItemArrival(NctsArrivalCargoDesc goodsItem)
	{
		var transpDoc1 = goodsItem.AdditionalInfos.AddNew();
		transpDoc1.CSI_Code = "A000";
		transpDoc1.CSI_SubType = "TRA";
		transpDoc1.CSI_Status = "NEW";
		transpDoc1.CSI_ReferenceNumber = "TRA1";
		transpDoc1.CSI_LineNo = 2;

		var transpDoc2 = goodsItem.AdditionalInfos.AddNew();
		transpDoc2.CSI_Code = "A001";
		transpDoc2.CSI_SubType = "TRA";
		transpDoc2.CSI_Status = "NEW";
		transpDoc2.CSI_ReferenceNumber = "TRA2";
		transpDoc2.CSI_LineNo = 3;
	}

	void AddReferenceDocumentsToGoodsItemArrival(NctsArrivalCargoDesc goodsItem)
	{
		var refDoc1 = goodsItem.AdditionalInfos.AddNew();
		refDoc1.CSI_Code = "A002";
		refDoc1.CSI_SubType = "REF";
		refDoc1.CSI_Status = "NEW";
		refDoc1.CSI_ReferenceNumber = "REF1";
		refDoc1.CSI_LineNo = 2;

		var refDoc2 = goodsItem.AdditionalInfos.AddNew();
		refDoc2.CSI_Code = "A003";
		refDoc2.CSI_SubType = "REF";
		refDoc2.CSI_Status = "NEW";
		refDoc2.CSI_ReferenceNumber = "REF2";
		refDoc2.CSI_LineNo = 3;
	}

	void AddLiabilityCalculationToGoodsItemArrival(NctsArrivalCargoDesc liabilityCalculation)
	{
		liabilityCalculation.BY_RN_NKCountryOfOrigin = "CA";
		liabilityCalculation.BY_HarmonisedTariff = "11110";
		liabilityCalculation.BY_CustomsSecondQuantity = 10;
		liabilityCalculation.BY_CustomsSecondUnitQty = "KA";
		liabilityCalculation.BY_CustomsThirdQuantity = 20;
		liabilityCalculation.BY_CustomsThirdUnitQty = "GA";
		liabilityCalculation.BY_CustomsFourthQuantity = 30;
		liabilityCalculation.BY_CustomsFourthUnitQty = "TA";
		liabilityCalculation.BY_MonetaryValue = 100;
		var additionalCode1 = Factory.New<SupplementaryCode>();
		additionalCode1.CY_Code = "V999";
		liabilityCalculation.AdditionalSupplementaryCodes.Add(additionalCode1);
	}

	void AddDataToDepartureBillAndTransportForUnloading(NctsHeader nctsHeader, ZString transportMode, ZString typeId, ZString transportId, ZString transportNationality, bool createWithEmptyId = false) =>
		AddDataToDepartureBillAndTransportForUnloading(nctsHeader, transportMode, typeId, transportId, transportNationality, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, createWithEmptyId);

	void AddDataToDepartureBillAndTransportForUnloading(NctsHeader nctsHeader, ZString transportMode, ZString typeId, ZString transportId, ZString transportNationality, ZString trailer1Id, ZString trailer1Nationality, ZString trailer2Id, ZString trailer2Nationality, bool createWithEmptyId = false)
	{
		var bill = nctsHeader.Bills.AddNew();

		if (!transportId.IsEmpty || createWithEmptyId)
		{
			var transportInfo1 = bill.DepartureTransportInfos.AddNew();
			transportInfo1.TPM_SequenceNumber = 1;
			transportInfo1.TPM_TypeOfIdentification = typeId;
			transportInfo1.TPM_IdentificationNumber = transportId;
			transportInfo1.TPM_RN_NKTransportNationality = transportNationality;
		}

		if (transportMode == ModeOfTransportList.Codes._3_RoadTransport)
		{
			var trailerMode = EU.NCTS.Business.NctsTransportTypeOfIdList.Codes._31;

			if (!trailer1Id.IsEmpty || createWithEmptyId)
			{
				var transportInfo2 = bill.DepartureTransportInfos.AddNew();
				transportInfo2.TPM_SequenceNumber = 2;
				transportInfo2.TPM_TypeOfIdentification = trailerMode;
				transportInfo2.TPM_IdentificationNumber = trailer1Id;
				transportInfo2.TPM_RN_NKTransportNationality = trailer1Nationality;
			}
			if (!trailer2Id.IsEmpty || createWithEmptyId)
			{
				var transportInfo3 = bill.DepartureTransportInfos.AddNew();
				transportInfo3.TPM_SequenceNumber = 3;
				transportInfo3.TPM_TypeOfIdentification = trailerMode;
				transportInfo3.TPM_IdentificationNumber = trailer2Id;
				transportInfo3.TPM_RN_NKTransportNationality = trailer2Nationality;
			}
		}
	}

	void AddArrivalBillWithNewTransportInfo(NctsHeader nctsHeader, ZString transportMode, ZString typeId, ZString transportId, ZString transportNationality, ZString trailer1Id, ZString trailer1Nationality, ZString trailer2Id, ZString trailer2Nationality, bool createWithEmptyId = false)
	{
		var sequenceNumber = nctsHeader.Bills.LastOrDefault()?.SequenceNumber ?? 0;

		sequenceNumber++;

		var bill = nctsHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = sequenceNumber.ToString();

		if (!transportId.IsEmpty || createWithEmptyId)
		{
			var transportInfo1 = bill.ArrivalTransportInfos.AddNew();
			transportInfo1.TPM_SequenceNumber = 1;
			transportInfo1.TPM_TypeOfIdentification = typeId;
			transportInfo1.TPM_IdentificationNumber = transportId;
			transportInfo1.TPM_RN_NKTransportNationality = transportNationality;
			transportInfo1.TPM_TransportState = "NEW";
		}

		if (transportMode == ModeOfTransportList.Codes._3_RoadTransport)
		{
			var trailerMode = EU.NCTS.Business.NctsTransportTypeOfIdList.Codes._31;

			if (!trailer1Id.IsEmpty || createWithEmptyId)
			{
				var transportInfo2 = bill.ArrivalTransportInfos.AddNew();
				transportInfo2.TPM_SequenceNumber = 2;
				transportInfo2.TPM_TypeOfIdentification = trailerMode;
				transportInfo2.TPM_IdentificationNumber = trailer1Id;
				transportInfo2.TPM_RN_NKTransportNationality = trailer1Nationality;
				transportInfo2.TPM_TransportState = "NEW";
			}
			if (!trailer2Id.IsEmpty || createWithEmptyId)
			{
				var transportInfo3 = bill.ArrivalTransportInfos.AddNew();
				transportInfo3.TPM_SequenceNumber = 3;
				transportInfo3.TPM_TypeOfIdentification = trailerMode;
				transportInfo3.TPM_IdentificationNumber = trailer2Id;
				transportInfo3.TPM_RN_NKTransportNationality = trailer2Nationality;
				transportInfo3.TPM_TransportState = "NEW";
			}
		}
	}

	void AddDataToDepartureForUnloading(NctsHeader nctsHeader)
	{
		var departureMovement = nctsHeader.MovementHeader;
		SetDepartureMovementTransportInfo(departureMovement, ModeOfTransportList.Codes._2_RailTransport, "20", "wagon", "GB");
		departureMovement.BM_GrossWeight = 12.345;

		var cont1 = nctsHeader.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";
		cont1.Seal1 = "Seal1";
		var cont2 = nctsHeader.DepartureHeaderContainers.AddNew();
		cont2.BC_Mode = "CNT";
		cont2.BC_ContainerNum = "CONT2";
		cont2.Seal1 = "Seal2";
		cont2.Seal2 = "Seal3";

		var bill = nctsHeader.Bills.AddNew();
		bill.B0_Weight = 20m;
		bill.B0_WeightUQ = "KG";

		var goodsItem1 = bill.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		goodsItem1.BY_DeclarationGoodsItemNumber = 1;
		goodsItem1.BY_Description = "departuredescription";
		goodsItem1.BY_CusC4Number = "QQQ";
		goodsItem1.BY_HarmonisedTariff = "88888888";
		goodsItem1.BY_GrossWeight = 20;
		goodsItem1.BY_GrossWeightUnit = "KG";
		goodsItem1.BY_NetWeight = 10;
		goodsItem1.BY_NetWeightUnit = "DG";
		AddPackagesToGoodsItemDeparture(goodsItem1, cont1);
		AddSupportingDocumentsToGoodsItemDeparture(goodsItem1);
		AddTransportDocumentsToGoodsItemDeparture(goodsItem1);
		AddReferenceDocumentsToGoodsItemDeparture(goodsItem1);

		var goodsItem2 = bill.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;
		goodsItem2.BY_DeclarationGoodsItemNumber = 2;
		goodsItem2.BY_Description = "departuredescription2";
		goodsItem2.BY_CusC4Number = "WWW";
		goodsItem2.BY_HarmonisedTariff = "99999999";
		goodsItem2.BY_GrossWeight = 15;
		goodsItem2.BY_GrossWeightUnit = "KL";
		goodsItem2.BY_NetWeight = 11;
		goodsItem2.BY_NetWeightUnit = "L";
		AddPackagesToGoodsItemDeparture(goodsItem2, cont2);
		AddSupportingDocumentsToGoodsItemDeparture(goodsItem2);
		AddTransportDocumentsToGoodsItemDeparture(goodsItem2);
		AddReferenceDocumentsToGoodsItemDeparture(goodsItem2);

		var supDoc1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		supDoc1.CSI_Code = "A003";
		supDoc1.CSI_ReferenceNumber = "departuredocHeader1";

		var supDoc2 = bill.SupportingDocuments.AddNew();
		supDoc2.CSI_Code = "A004";
		supDoc2.CSI_ReferenceNumber = "departuredocBill1";

		supDoc1.CSI_LineNo = 4;
		supDoc2.CSI_LineNo = 5;

		var transpDoc1 = nctsHeader.AdditionalDocuments.AddNew();
		transpDoc1.CSI_Code = "B003";
		transpDoc1.CSI_SubType = "TRA";
		transpDoc1.CSI_ReferenceNumber = "departuretraHeader1";

		var transpDoc2 = bill.AdditionalDocuments.AddNew();
		transpDoc2.CSI_Code = "B004";
		transpDoc2.CSI_SubType = "TRA";
		transpDoc2.CSI_ReferenceNumber = "departuretraBill1";

		transpDoc1.CSI_LineNo = 4;
		transpDoc2.CSI_LineNo = 5;

		var refDoc1 = nctsHeader.AdditionalDocuments.AddNew();
		refDoc1.CSI_Code = "C003";
		refDoc1.CSI_SubType = "REF";
		refDoc1.CSI_ReferenceNumber = "departurerefHeader1";

		var refDoc2 = bill.AdditionalDocuments.AddNew();
		refDoc2.CSI_Code = "C004";
		refDoc2.CSI_SubType = "REF";
		refDoc2.CSI_ReferenceNumber = "departurerefBill1";

		refDoc1.CSI_LineNo = 4;
		refDoc2.CSI_LineNo = 5;
	}

	void AddPackagesToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem, NctsDepartureHeaderContainer container)
	{
		goodsItem.IsVehicles = false;

		var pack1 = goodsItem.Packages.AddNew();
		pack1.B5_SequenceNumber = 1;
		pack1.B5_UnitType = "AA";
		pack1.B5_UnitCount = 5;
		pack1.B5_MarksAndNumbers = "departuremarks1";
		pack1.ContainersPivot.AddPivotFor(container);

		var pack2 = goodsItem.Packages.AddNew();
		pack2.B5_SequenceNumber = 2;
		pack2.B5_UnitType = "BB";
		pack2.B5_UnitCount = 20;
		pack2.B5_MarksAndNumbers = "departuremarks2";
		pack2.ContainersPivot.AddPivotFor(container);
	}

	void AddSupportingDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
	{
		var supDoc1 = goodsItem.SupportingDocuments.AddNew();
		supDoc1.CSI_LineNo = 1;
		supDoc1.CSI_Code = "A001";
		supDoc1.CSI_ReferenceNumber = "departuredoc1";

		var supDoc2 = goodsItem.SupportingDocuments.AddNew();
		supDoc2.CSI_LineNo = 2;
		supDoc2.CSI_Code = "A002";
		supDoc2.CSI_ReferenceNumber = "departuredoc2";
	}

	void AddTransportDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
	{
		var transpDoc1 = goodsItem.AdditionalInfos.AddNew();
		transpDoc1.CSI_Code = "B001";
		transpDoc1.CSI_SubType = "TRA";
		transpDoc1.CSI_ReferenceNumber = "departuretra1";
		transpDoc1.CSI_LineNo = 1;

		var transpDoc2 = goodsItem.AdditionalInfos.AddNew();
		transpDoc2.CSI_Code = "B002";
		transpDoc2.CSI_SubType = "TRA";
		transpDoc2.CSI_ReferenceNumber = "departuretra2";
		transpDoc2.CSI_LineNo = 2;
	}

	void AddReferenceDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
	{
		var refDoc1 = goodsItem.AdditionalInfos.AddNew();
		refDoc1.CSI_Code = "C001";
		refDoc1.CSI_SubType = "REF";
		refDoc1.CSI_ReferenceNumber = "departureref1";

		var refDoc2 = goodsItem.AdditionalInfos.AddNew();
		refDoc2.CSI_Code = "C002";
		refDoc2.CSI_SubType = "REF";
		refDoc2.CSI_ReferenceNumber = "departureref2";

		refDoc1.CSI_LineNo = 1;
		refDoc2.CSI_LineNo = 2;
	}

	void AddLiabilityCalculationToGoodsItemDeparture(NctsDepartureCargoDesc liabilityCalculation)
	{
		liabilityCalculation.BY_RN_NKCountryOfOrigin = "CD";
		liabilityCalculation.BY_HarmonisedTariff = "11111";
		liabilityCalculation.BY_CustomsSecondQuantity = 1;
		liabilityCalculation.BY_CustomsSecondUnitQty = "KD";
		liabilityCalculation.BY_CustomsThirdQuantity = 2;
		liabilityCalculation.BY_CustomsThirdUnitQty = "GD";
		liabilityCalculation.BY_CustomsFourthQuantity = 3;
		liabilityCalculation.BY_CustomsFourthUnitQty = "TD";
		liabilityCalculation.BY_MonetaryValue = 10;
		var additionalCode1 = Factory.New<SupplementaryCode>();
		additionalCode1.CY_Code = "V901";
		liabilityCalculation.AdditionalSupplementaryCodes.Add(additionalCode1);
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
			&& info.Name != "DestinationCustomsOfficeCodeForArrival")
		{
			base.TestBizObjectField(info);
		}
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}

	protected override BusinessObject GetNewBusinessObject() => arrivalMovement;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => arrivalMovement;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovement = nctsHeader.ArrivalMovementHeader;
		arrivalMovement.BM_NoChangesToReport = false;
	}
	NctsArrivalMovementHeader arrivalMovement;
	NctsHeader nctsHeader;

	void SetUpTariffAndRates()
	{
		var countrycode = Core.Constants.CountryCodes.Spain;

		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateRefCusTaxOrFeeType("VAT");

		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(countrycode, "test country", parentDataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(countrycode, Universal.Constants.TariffTypes.Import);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
		Factory.Save();

		var tariff = helper.CreateTariff(countrycode, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");

		var rateTypeDuty = helper.CreateCusRateType(countrycode, "DTY");
		var rateCodeDuty = helper.CreateCusRateCode(Factory, "A00", rateTypeDuty.PK);
		var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", countrycode, "EUN");

		var rateDuty = helper.CreateRefCusRate(tariff.PK, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);

		var tradeGroup = helper.CreateTradeGroup(countrycode, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroup, "EU");
		helper.AddCountry(tradeGroup, countrycode);
		helper.CreateCusApplicability(rateDuty.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var rateTypeCountervailing = helper.CreateCusRateType(countrycode, Customs.Universal.Constants.RateTypes.Countervailing, ensureDataGroupingExists: false);
		var rateCodeCountervailing = helper.CreateCusRateCode(Factory, "RC1", rateTypeCountervailing.PK);
		var testRateCountervailing = helper.CreateRate(tariff, rateCodeCountervailing.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");

		var rateTypeAntiDumping = helper.CreateCusRateType(countrycode, Customs.Universal.Constants.RateTypes.AntiDumping, ensureDataGroupingExists: false);
		var rateCodeAntiDumping = helper.CreateCusRateCode(Factory, "RC1", rateTypeAntiDumping.PK);
		var testRateAntiDumping = helper.CreateRate(tariff, rateCodeAntiDumping.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

		helper.CreateCusApplicability(testRateCountervailing, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
		helper.CreateCusApplicability(testRateAntiDumping, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

		var lastMonth = ZDateTime.Today.AddMonths(-1);
		var nextMonth = ZDateTime.Today.AddMonths(1);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
			.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC02", "EU AC02", lastMonth, nextMonth)
			.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);

		Factory.Save();
	}

	void SetDepartureMovementTransportInfo(
		NctsDepartureMovementHeader departureMovement,
		ZString transportMode,
		ZString transportType,
		ZString transportId,
		ZString transportCountry) =>
		SetDepartureMovementTransportInfo(
			departureMovement, transportMode, transportType, transportId, transportCountry,
			ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

	void SetDepartureMovementTransportInfo(
		NctsDepartureMovementHeader departureMovement,
		ZString transportMode,
		ZString transportType,
		ZString transportId,
		ZString transportCountry,
		ZString trailer1RegNo,
		ZString trailer1Nationality,
		ZString trailer2RegNo,
		ZString trailer2Nationality)
	{
		departureMovement.BM_InlandTransportMode = transportMode;
		departureMovement.BM_TransportAtDepartureType = transportType;
		departureMovement.BM_TransportAtDeparture = transportId;
		departureMovement.BM_RN_NKTransportAtDepartureCountry = transportCountry;
		departureMovement.BM_TransportAtDepartureTrailer1RegNo = trailer1RegNo;
		departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality = trailer1Nationality;
		departureMovement.BM_TransportAtDepartureTrailer2RegNo = trailer2RegNo;
		departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality = trailer2Nationality;
	}

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetArrivalBillTransportInfos(NctsBill bill) =>
		GetTransportMeansCollection(bill.ArrivalTransportInfos);

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetArrivalHeaderTransportInfos(NctsHeader header) =>
		GetTransportMeansCollection(header.ArrivalMovementHeader.ArrivalTransportInfos);

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetTransportMeansCollection(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> transportMeans) =>
		transportMeans.Select(x =>
			(x.TPM_SequenceNumber,
			x.TPM_TransportState,
			x.TPM_TypeOfIdentification,
			x.TPM_IdentificationNumber,
			x.TPM_RN_NKTransportNationality));
}
