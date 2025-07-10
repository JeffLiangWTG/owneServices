using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	partial class NctsHeaderBaseOnlyTest
	{
		public void TestIsArrivalTabReadOnly()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				arrivalHeader.BH_HeaderType = "D";
				AssertEquals("When is Departure, the result shoul be false", false, arrivalHeader.IsArrivalTabReadOnly);
				arrivalHeader.BH_HeaderType = "A";

				arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'AUP', the result shoul be true", true, arrivalHeader.IsArrivalTabReadOnly);
				arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'ART', the result shoul be true", true, arrivalHeader.IsArrivalTabReadOnly);

				arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
				arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
				AssertEquals("When is Arrival and EffectiveMessageStatus is 'MAS', the result shoul be true", true, arrivalHeader.IsArrivalTabReadOnly);
				arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
				AssertEquals("When is Arrival and EffectiveMessageStatus is 'MUS', the result shoul be true", true, arrivalHeader.IsArrivalTabReadOnly);
				arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksRejected;
				AssertEquals("When is Arrival and EffectiveMessageStatus is 'MUR', the result shoul be true", true, arrivalHeader.IsArrivalTabReadOnly);

				arrivalHeader.EffectiveMessageStatus = ZString.Empty;
				AssertEquals("When is Arrival and EffectiveMessageStatus is empty and EffectiveMessageStatus is empty, the result shoul be false", false, arrivalHeader.IsArrivalTabReadOnly);
			});
		}

		public void TestBH_RL_NKImportLoadPortReadOnly_Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals(false, arrivalHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);

			var goodItem = arrivalHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(false, arrivalHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);

			goodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			AssertEquals(false, arrivalHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);
		}

		public void TestDestinationTrader_IsPersistentWhenIsPluggedIn()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.BH_ParentID = shipment.PK;
			arrivalHeader.BH_ParentTableCode = shipment.TablePrefix;
			var destinationTrader = arrivalHeader.DestinationTrader;
			AssertEquals("DestinationTrader JobDocAddress Persistent", true, destinationTrader.IsPersistent);
		}

		public void TestArrivalMovementHeader_Delete()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;
				AssertType<NctsArrivalMovementHeader>("Create", arrivalMovementHeader);
				arrivalMovementHeader.Delete();
				AssertEquals("Create new one", false, Equals(arrivalMovementHeader, arrivalHeader.ArrivalMovementHeader));
			});
		}

		public void TestArrivalMovementHeader_NotArrivalHeaderType()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertNull(arrivalHeader.ArrivalMovementHeader);
		}

		public void TestArrivalMovementHeaderChildEditable()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals(true, arrivalHeader.IsRegisteredEditableChildObject(arrivalHeader.ArrivalMovementHeader));
		}

		public override void TestCloneAuditProperties()
		{
			Assert("Clone only works for a DepartureMovement", true);
		}

		public void TestDeclarantId_Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			const string eoriCode1 = "123456789000";
			const string eoriCode2 = "987654321000";
			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			arrivalHeader.Declarant.E2_OA_Address = org1.MainAddress.PK;
			arrivalHeader.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			AssertContains("Arrival Header", eoriCode2, arrivalHeader.DeclarantId);
		}

		public void TestPlaceOfUnloadingCode_Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.PlaceOfUnloadingCode = "GBLON";
			AssertEquals("GBLON", arrivalHeader.ArrivalMovementHeader.BM_PlaceOfUnloading);
		}

		public void TestLocalReferenceNumberReadOnly_Arrival()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				AssertEquals("Arrival Header", false, arrivalHeader.LocalReferenceNumberReadOnly);
				arrivalHeader.Messages.AddNew();
				AssertEquals("Has message", true, arrivalHeader.LocalReferenceNumberReadOnly);
			});
		}

		public void TestLocalReferenceNumber_Phase5Arrival()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			arrivalHeader.BH_JobReference = "NCT000001";

			CombineAssertions(() =>
			{
				AssertEquals("No LRN filled, LocalReferenceNumber should be filled with JobReference", "NCT000001", arrivalHeader.LocalReferenceNumber);
				arrivalHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "LRN123";
				AssertEquals("LRN filled, LocalReferenceNumber should be retrieved from BM_PaperlessInbondNum", "LRN123", arrivalHeader.LocalReferenceNumber);
				arrivalHeader.LocalReferenceNumber = "LRN321";
				AssertEquals("LocalReferenceNumber should be saved to BM_PaperlessInbondNum", "LRN321", arrivalHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			});
		}

		public void TestLocalReferenceNumber_Phase4Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.BH_JobReference = "NCT000001";

			CombineAssertions(() =>
			{
				AssertEquals("No LRN filled, LocalReferenceNumber should be filled with JobReference", "NCT000001", arrivalHeader.LocalReferenceNumber);
				arrivalHeader.SetSystemDefinedValue(nameof(NctsHeader.Schema.LocalReferenceNumber), (ZString)"LRN123");
				AssertEquals("LRN filled, LocalReferenceNumber should be retrieved from BM_PaperlessInbondNum", "LRN123", arrivalHeader.LocalReferenceNumber);
				arrivalHeader.LocalReferenceNumber = "LRN321";
				AssertEquals("LocalReferenceNumber should be saved to BM_PaperlessInbondNum", "LRN321", arrivalHeader.GetSystemDefinedValue<ZString>(nameof(NctsHeader.Schema.LocalReferenceNumber)));
			});
		}

		public void TestDefaultMessageStatus_Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals(NctsMessageStatusList.Codes.ArrivalNotificationNotSent, arrivalHeader.EffectiveMessageStatus);
		}

		public void TestDestinationCustomsOfficeCode_Arrival()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				AssertNullOrEmpty(arrivalHeader.DestinationCustomsOfficeCodeForArrival);
				NCTSTestHelper.CreateCustomsOfficeForTest(arrivalHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "ZZ123456", ZDateTime.Empty);
				AssertEquals("ZZ123456", arrivalHeader.DestinationCustomsOfficeCodeForArrival);
			});
		}

		public void TestDestinationCustomsOfficeCodeReadOnly_Arrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals("Arrival Header", true, arrivalHeader.DestinationCustomsOfficeCodeInfo.ReadOnly);
			AssertEquals("Arrival Header", false, arrivalHeader.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
		}

		public void TestArrivalMrnFromUserReadOnly_Phase4()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				AssertEquals("No GoodsItems in ArrivalMovementHeader", false, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);

				arrivalHeader.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("Has GoodsItems in ArrivalMovementHeader", true, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);
			});
		}

		public void TestArrivalMrnFromUserReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase5ArrivalHeader();
				AssertEquals("No GoodsItems", false, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);

				arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
				AssertEquals("Has GoodsItems", true, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);
			});
		}

		public void TestEnRouteSeals()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				var enRouteSeals = arrivalHeader.EnRouteSeals;
				AssertEquals("Registered Editable", true, arrivalHeader.IsRegisteredEditableChildObject(enRouteSeals));
				AssertEquals("Not ReadOnly", false, enRouteSeals.ReadOnly);
				AssertSame("Cached", enRouteSeals, arrivalHeader.EnRouteSeals);
			});
		}

		public void TestEnRouteTranshipments()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				var transhipments = arrivalHeader.EnRouteTransshipments;
				AssertSame("Cached", transhipments, arrivalHeader.EnRouteTransshipments);
				AssertEquals("Not ReadOnly", false, transhipments.ReadOnly);
				AssertEquals("Registered editable", true, arrivalHeader.IsRegisteredEditableChildObject(transhipments));
			});
		}

		public void TestEnRouteIncidents()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				var incidents = arrivalHeader.EnRouteIncidents;
				AssertSame("Cached", incidents, arrivalHeader.EnRouteIncidents);
				AssertEquals("Not ReadOnly", false, incidents.ReadOnly);
				AssertEquals("Registered editable", true, arrivalHeader.IsRegisteredEditableChildObject(incidents));
			});
		}

		public void TestUnloadingRemark()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertSame("Cached", arrivalHeader.UnloadingRemark, arrivalHeader.UnloadingRemark);
		}

		public void TestResultsOfControlCollection()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var resultsOfControl = arrivalHeader.ResultsOfControlCollection;
			CombineAssertions(() =>
			{
				AssertSame("Cached", resultsOfControl, arrivalHeader.ResultsOfControlCollection);
				AssertEquals("Registered editable", true, arrivalHeader.IsRegisteredEditableChildObject(resultsOfControl));
			});
		}

		public void TestResetResultsOfControlCollection()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.ResultsOfControlCollection.AddNew();
			arrivalHeader.ResultsOfControlCollection.AddNew();
			arrivalHeader.ResetUnloadedValues();
			AssertEquals("1 ROC remains after ResetUnloadedValues because in pressing this button we recreate the unloaded nationality field", 1, arrivalHeader.ResultsOfControlCollection.Count);
		}

		public void TestResetG9_StateOfSealsOk()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZString.Empty, unloadingRemark.G9_StateOfSealsOk);
		}

		public void TestResetG9_UnloadingRemark()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_UnloadingRemark = YesNoList.Codes.Yes;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZString.Empty, unloadingRemark.G9_UnloadingRemark);
		}

		public void TestResetG9_Conform()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZString.Empty, unloadingRemark.G9_Conform);
		}

		public void TestResetG9_UnloadingCompletion()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_UnloadingCompletion = YesNoList.Codes.Yes;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZString.Empty, unloadingRemark.G9_UnloadingCompletion);
		}

		public void TestResetG9_UnloadingDate()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_UnloadingDate = ZDate.BrettsBirthday;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZDate.Today, unloadingRemark.G9_UnloadingDate);
		}

		public void TestResetG9_NoOfSeals()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingRemark = arrivalHeader.UnloadingRemark;
			unloadingRemark.G9_NoOfSeals = 99;
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(ZInt.Zero, unloadingRemark.G9_NoOfSeals);

			arrivalHeader.ArrivalMovementHeader.Seals.AddNew();
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(1, unloadingRemark.G9_NoOfSeals);

			arrivalHeader.ArrivalMovementHeader.Seals.AddNew();
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(2, unloadingRemark.G9_NoOfSeals);

			arrivalHeader.ArrivalMovementHeader.Seals.RemoveAndDeleteAll();
			arrivalHeader.ResetUnloadedValues();
			AssertEquals(0, unloadingRemark.G9_NoOfSeals);
		}

		public void TestDefaultUnloadedMeansOfTransportAtDepartureNationality()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality = ZString.Empty;
			arrivalHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "DC";
			AssertEquals("Actual unloading nationality comes from BM_RN_NKTransportAtDepartureCountry now that it is not blank", "DC", arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality);
		}

		public void TestResetDefaultsUnloadedMeansOfTransportAtDepartureNationality()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				arrivalHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "DC";
				arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality = "JL";
				AssertEquals("Confirm Set", "JL", arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality);
				arrivalHeader.ResetUnloadedValues();
				AssertEquals("Actual unloading nationality comes from expected once Reset is executed", "DC", arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality);
				var resultsOfControlData = arrivalHeader.ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == NctsHeader.MeansOfTransportAtDepartureNationalityPointer).Data;
				AssertEquals("G9_CorrectedValue", "DC", resultsOfControlData.G9_CorrectedValue);
				AssertEquals("The indicator for result-of-control is blank when actual value is the default value", "", resultsOfControlData.G9_ControlIndicator);
			});
		}

		public void TestResetUnloadedTotalGrossMassInKilograms()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				arrivalHeader.ArrivalMovementHeader.BM_GrossWeight = 9999m;
				AssertEquals("pre-req", 0m, arrivalHeader.UnloadingMovementHeader.TotalGrossMassInKilograms);
				arrivalHeader.ResetUnloadedValues();
				AssertEquals("UnloadedTotalGrossMassInKilograms", 9999m, arrivalHeader.UnloadingMovementHeader.TotalGrossMassInKilograms);
				arrivalHeader.UnloadingMovementHeader.TotalGrossMassInKilograms = 1111m;
				arrivalHeader.ResetUnloadedValues();
				AssertEquals("UnloadedTotalGrossMassInKilograms", 9999m, arrivalHeader.UnloadingMovementHeader.TotalGrossMassInKilograms);
			});
		}

		public void TestUnloadedMeansOfTransportAtDepartureNationality_ControlIndicator()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				arrivalHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "DC";
				arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality = "LM";
				var resultsOfControlData = arrivalHeader.ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == NctsHeader.MeansOfTransportAtDepartureNationalityPointer).Data;
				AssertEquals("Corrected Value", "LM", resultsOfControlData.G9_CorrectedValue);
				AssertEquals("Different control indicator", ResultOfControlCodes.Codes.Different, resultsOfControlData.G9_ControlIndicator);
			});
		}

		public void TestResetUnloadedValues_HasChanges()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.UnloadedMeansOfTransportAtDepartureNationality = "JL";
			arrivalHeader.ResetUnloadedValues();
			Factory.Save();
			_ = arrivalHeader.UnloadedMeansOfTransportAtDepartureNationalityInfo;
			AssertEquals(false, arrivalHeader.HasChanges);
		}

		public void TestTurnArrivalIntoDepartureAndArrival()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
			arrivalHeader.TurnArrivalIntoDepartureAndArrival();
			CombineAssertions(() =>
			{
				AssertEquals("BH_HeaderType", NctsMovementType.Codes.DepartureAndArrival, arrivalHeader.BH_HeaderType);
				AssertEquals("MovementHeader.BM_CustomsStatus", NctsTransitStatusList.Codes.Unknown, arrivalHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("EffectiveMessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationNotSent, arrivalHeader.EffectiveMessageStatus);
			});
		}

		public void TestUnloadedGoodsItems()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var unloadingMovement = arrivalHeader.UnloadingMovementHeader;
			unloadingMovement.GoodsItems.AddNew();

			var arrivalMovement = arrivalHeader.ArrivalMovementHeader;
			var goodsItem1 = arrivalMovement.GoodsItems.AddNew();
			var sd = goodsItem1.SupportingDocuments.AddNew();
			sd.CSI_Code = "316";
			sd.CSI_ReferenceNumber = "SD001";
			sd.CSI_Description = "Pre Entry Lin=9999";
			var container = goodsItem1.Containers.AddNew();
			container.BC_ContainerNum = "CONTAINER1";
			container.BC_Seal1 = "SEAL1";
			container.BC_Seal2 = "SEAL2";
			var package = goodsItem1.Packages.AddNew();
			package.B5_MarksAndNumbers = "M123";
			var sgiCode = goodsItem1.AdditionalInfos.AddNew();
			sgiCode.CSI_Code = "SGIXX";
			arrivalMovement.GoodsItems.AddNew();
			arrivalMovement.GoodsItems.AddNew();

			arrivalHeader.ResetUnloadedValues();

			CombineAssertions(() =>
			{
				AssertEquals("ArrivalMovementHeader.GoodsItems.Count", 3, arrivalMovement.GoodsItems.Count);
				AssertEquals("UnloadingMovementHeader.GoodsItems.Count", 3, unloadingMovement.GoodsItems.Count);

				var unloadedGoodsItem1 = unloadingMovement.GoodsItems[0];
				var unloadedGoodsItem1SupportingDoc = unloadedGoodsItem1.SupportingDocuments[0];
				AssertEquals("supportingDoc.CSI_Code", "316", unloadedGoodsItem1SupportingDoc.CSI_Code);
				AssertEquals("supportingDoc.CSI_ReferenceNumber", "SD001", unloadedGoodsItem1SupportingDoc.CSI_ReferenceNumber);
				AssertEquals("supportingDoc.CSI_Description", "Pre Entry Lin=9999", unloadedGoodsItem1SupportingDoc.CSI_Description);

				var unloadedGoodsItem1Container = unloadedGoodsItem1.Containers[0];
				AssertEquals("container.ContainerNumber", "CONTAINER1", unloadedGoodsItem1Container.ContainerNumber);
				AssertEquals("container.BC_Seal1", "SEAL1", unloadedGoodsItem1Container.BC_Seal1);
				AssertEquals("container.BC_Seal2", "SEAL2", unloadedGoodsItem1Container.BC_Seal2);

				AssertEquals("package.B5_MarksAndNumbers", "M123", unloadedGoodsItem1.Packages[0].B5_MarksAndNumbers);
				AssertEquals("sgiCode.CSI_Code", "XX", unloadedGoodsItem1.SgiCodes.First().Code);

				AssertEquals("item1.BY_LineNo", (short)1, unloadedGoodsItem1.BY_LineNo);
				AssertEquals("item2.BY_LineNo", (short)2, unloadingMovement.GoodsItems[1].BY_LineNo);
				AssertEquals("item3.BY_LineNo", (short)3, unloadingMovement.GoodsItems[2].BY_LineNo);
			});
		}

		public void TestClone_Phase4Arrival()
		{
			var arrival = CreateTestArrival(CusInBondApplicationCodeList.Codes.NCTS4);
			var clone = (NctsHeader)arrival.TemplateCopy();
			AssertCloneResult_Common(clone, NctsMovementType.Codes.Arrival);
			AssertCloneResult_Arrival_NonPhase5(clone);
		}

		public void TestCloneForPhase4ArrivalHeaderWithUnloadingHeader()
		{
			var arrival = CreateTestArrival(CusInBondApplicationCodeList.Codes.NCTS4);
			// Adding Unloading movement to test it is not copied
			arrival.UnloadingMovementHeader.BM_AdditionalText = "ABC";
			arrival.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			arrival.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;

			AssertEquals("Pre-assertion - Should be two movement headers (1 unloading, 1 arrival)", 2, arrival.MovementHeaders.Count);

			var clone = (NctsHeader)arrival.TemplateCopy();
			AssertCloneResultForUnloadingAndArrival(clone, NctsMovementType.Codes.Arrival);
		}

		public void TestTotalNumberOfItems()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals("TotalNumberOfItems for Arrivals", 0, arrivalHeader.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals("TotalNumberOfPackages for Arrivals", 0, arrivalHeader.TotalNumberOfPackages);
		}

		public void TestTotalGrossMassInKilograms()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			AssertEquals("TotalGrossMassInKilograms for Arrivals", 0m, arrivalHeader.TotalGrossMassInKilograms);
		}

		public void TestIsPhase5Arrival()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			AssertEquals("Phase 5 Arrivals => true", true, arrivalHeader.IsPhase5Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Phase 4 Arrivals => false", false, arrivalHeader.IsPhase5Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertEquals("Phase 5 Departure => false", false, arrivalHeader.IsPhase5Arrival);
		}

		public void TestArrivalDetailsReadOnly()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			arrivalHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Unknown;
			var incident = arrivalHeader.EnRouteIncidents.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Message Status different from 'ACK' or 'SNT', MRN should be enabled", false, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Incidents should be enabled", false, arrivalHeader.EnRouteIncidents.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Incident flag should be enabled", false, arrivalHeader.BH_ExportFlagInfo.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Local Reference Number should be enabled", false, arrivalHeader.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Language should be enabled", false, arrivalHeader.BH_CommunicationLanguageInfo.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', DestinationTrader should be enabled", false, arrivalHeader.DestinationTrader.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Destination Customs Office Code for Arrival should be enabled", false, arrivalHeader.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Authorizations should be enabled", false, arrivalHeader.CusAuthorizationUsages.ReadOnly);
				AssertEquals("Message Status different from 'ACK' or 'SNT', Goods Location in incidents should be enabled", false, incident.GoodsLocation.ReadOnly);

				arrivalHeader = CreatePhase5ArrivalHeader();
				arrivalHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
				incident = arrivalHeader.EnRouteIncidents.AddNew();
				AssertEquals("Message Status 'ACK', MRN should be readonly", true, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Message Status 'ACK', Incidents should be readonly", true, arrivalHeader.EnRouteIncidents.ReadOnly);
				AssertEquals("Message Status 'ACK', Incident flag should be readonly", true, arrivalHeader.BH_ExportFlagInfo.ReadOnly);
				AssertEquals("Message Status 'ACK', Local Reference Number should be readonly", true, arrivalHeader.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Message Status 'ACK', Language should be readonly", true, arrivalHeader.BH_CommunicationLanguageInfo.ReadOnly);
				AssertEquals("Message Status 'ACK', DestinationTrader should be readonly", true, arrivalHeader.DestinationTrader.ReadOnly);
				AssertEquals("Message Status 'ACK', Destination Customs Office Code for Arrival should be readonly", true, arrivalHeader.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Message Status 'ACK', Goods Location in incidents should be readonly", true, incident.GoodsLocation.ReadOnly);

				arrivalHeader = CreatePhase5ArrivalHeader();
				arrivalHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
				incident = arrivalHeader.EnRouteIncidents.AddNew();
				AssertEquals("Message Status 'SNT', MRN should be readonly", true, arrivalHeader.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Message Status 'SNT', Incidents should be readonly", true, arrivalHeader.EnRouteIncidents.ReadOnly);
				AssertEquals("Message Status 'SNT', Incident flag should be readonly", true, arrivalHeader.BH_ExportFlagInfo.ReadOnly);
				AssertEquals("Message Status 'SNT', Local Reference Number should be readonly", true, arrivalHeader.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Message Status 'SNT', Language should be readonly", true, arrivalHeader.BH_CommunicationLanguageInfo.ReadOnly);
				AssertEquals("Message Status 'SNT', DestinationTrader should be readonly", true, arrivalHeader.DestinationTrader.ReadOnly);
				AssertEquals("Message Status 'SNT', Destination Customs Office Code for Arrival should be readonly", true, arrivalHeader.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Customs Status 'SNT', Goods Location in incidents should be readonly", true, incident.GoodsLocation.ReadOnly);
			});
		}

		public void TestDeleteBill()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			bill.Delete();
			AssertNoExceptionThrown(() => header.RunPreSaveValidation());
		}

		public void TestIsArrivalDetailsReadOnly()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = CreatePhase4ArrivalHeader();
				arrivalHeader.BH_HeaderType = "D";
				AssertEquals("When is Departure, the result should be false", false, arrivalHeader.IsArrivalDetailsReadOnly);
				arrivalHeader.BH_HeaderType = "A";

				arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
				AssertEquals("When is Arrival and Message Status is 'ACK', the result should be true", true, arrivalHeader.IsArrivalDetailsReadOnly);
				arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When is Arrival and Message Status is 'SNT', the result should be true", true, arrivalHeader.IsArrivalDetailsReadOnly);

				arrivalHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.Unknown;
				AssertEquals("When is Arrival and Message Status is empty (not ACK or SNT), the result should be false.", false, arrivalHeader.IsArrivalDetailsReadOnly);
			});
		}

		void AssertCloneResultForUnloadingAndArrival(NctsHeader clone, ZString headerType)
		{
			AssertEquals("Testing headerType", headerType, clone.BH_HeaderType);
			AssertEquals("Should only be one movement header (Arrival)", 1, clone.MovementHeaders.Count);
			AssertEquals("EffectiveMessageStatus should be 'MAN'", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, clone.EffectiveMessageStatus);
			AssertEquals("BM_CustomsStatus should be ''", ZString.Empty, clone.ArrivalMovementHeader.BM_CustomsStatus);
		}

		public void TestNctsHeaderContainers()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			var container = arrivalHeader.ArrivalHeaderContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("ArrivalHeaderContainers should contain 1 record", 1, arrivalHeader.ArrivalHeaderContainers.Count);
				AssertEquals("HeaderContainers should not contain any records for arrival", 0, arrivalHeader.DepartureHeaderContainers.Count);
			});
		}

		public void TestRemoveIncidentsWhenIncidentFlagChanged()
		{
			var arrivalHeader = CreatePhase4ArrivalHeader();
			arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;
			arrivalHeader.EnRouteIncidents.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				arrivalHeader.RemoveIncidentsWhenIncidentFlagChanged += (sender, e) => e.Cancel = true;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertEquals("Incidents to not be removed, BH_ExportFlag is set to Y", EventFlagList.Codes.Yes, arrivalHeader.BH_ExportFlag);
				AssertEquals("Incidents unchanged", 1, arrivalHeader.EnRouteIncidents.Count);

				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;
				AssertEquals("Incidents not to be removed, BH_ExportFlag remains unchanged", EventFlagList.Codes.Yes, arrivalHeader.BH_ExportFlag);
				AssertEquals("Incidents remain unchanged", 1, arrivalHeader.EnRouteIncidents.Count);

				arrivalHeader.RemoveIncidentsWhenIncidentFlagChanged += (sender, e) => e.Cancel = false;
				arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;
				Factory.Save();
				AssertEquals("Incidents to be removed, BH_ExportFlag is set to N", EventFlagList.Codes.No, arrivalHeader.BH_ExportFlag);
				AssertEquals("All Incidents removed", 0, arrivalHeader.EnRouteIncidents.Count);

				arrivalHeader.EnRouteIncidents.AddNew();
				arrivalHeader.BH_ExportFlag = arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertEquals("Incidents to be removed, BH_ExportFlag is set to Y", EventFlagList.Codes.Yes, arrivalHeader.BH_ExportFlag);
				AssertEquals("No incidents removed", 1, arrivalHeader.EnRouteIncidents.Count);
			});
		}

		public void TestBusinessObjectsWithRelatedEvents_Arrival()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();

			AssertCollectionContains(arrivalHeader.BusinessObjectsWithRelatedEvents, bo => bo == arrivalHeader.ArrivalMovementHeader);
		}

		public void TestApportionedAmountToGuaranteesLiabilityAmount_Phase5Arrival()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;

			var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = "Ref";
			guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransaction.CPL_TranValue = 150m;
			guaranteeLineTransaction.CPL_Reference = "XJ5 - 00003877";

			NCTSTestHelper.SetUpTariff(Factory);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
			var bill = header.Bills.AddNew();
			var goodsItem = Factory.New<NCTSTestHelper.NctsArrivalCargoDescForTest>();
			goodsItem.BY_ParentTableCode = bill.TablePrefix;
			goodsItem.BY_ParentID = bill.PK;
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

			var guarantee = header.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 10m;
			guarantee.PW_BondNumber = "19860101";

			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] LiabilityAmount before setting needed data to calculate it", 0m, goodsItem.LiabilityAmount);

				Factory.Save();
				AssertEquals("When Override is true, PW_BondAmount has the value set by user", 10m, guarantee.PW_BondAmount);

				guarantee.PW_Override = false;
				AssertEquals("When Override is false, PW_BondAmount is set to empty if there is no LiabilityAmount, before saving", 0m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to empty if there is no LiabilityAmount, after saving", 0m, guarantee.PW_BondAmount);

				goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem.BY_MonetaryValue = 1_000m;
				AssertEquals("[PRE-CONDITION] LiabilityAmount after setting needed data to calculate it", 120m, goodsItem.LiabilityAmount);

				AssertEquals("When Override is false, PW_BondAmount is not changed to LiabilityAmount (when changed) before saving", 0m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to LiabilityAmount (when changed) after saving", 120m, guarantee.PW_BondAmount);

				goodsItem.BY_MonetaryValue = 1_200m;
				AssertEquals("[PRE-CONDITION] LiabilityAmount after changing monetary value", 144m, goodsItem.LiabilityAmount);

				AssertEquals("When Override is false, PW_BondAmount is not changed to LiabilityAmount (when changed a second time) before saving", 120m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to LiabilityAmount (when changed a second time) after saving", 144m, guarantee.PW_BondAmount);
			});
		}

		public void TestIsDepartureAmendmentAllowed_DoesNotThrowForArrival()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			AssertNoExceptionThrown(() => _ = arrivalHeader.IsDepartureAmendmentAllowed);
		}
	}
}
