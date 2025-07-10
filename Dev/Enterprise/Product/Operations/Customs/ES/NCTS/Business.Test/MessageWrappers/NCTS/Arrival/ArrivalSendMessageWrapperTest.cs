using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalSendMessageWrapperTest : WrapperHelperTest<ArrivalSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Throw exception if arrivalType is null or empty", () => new ArrivalSendMessageWrapper(nctsHeader, Certificate, null));
				AssertExceptionThrown<ArgumentException>("Throw exception if arrivalType is null or empty", () => new ArrivalSendMessageWrapper(nctsHeader, Certificate, ZString.Empty));
			});
		}

		public void TestIsOnlyArrivalNotification()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected false IsOnlyArrivalNotification (declaration type is OBS)", false, wrapper.IsOnlyArrivalNotification);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected true IsOnlyArrivalNotification (declaration type is AVI)", true, wrapper.IsOnlyArrivalNotification);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected false IsOnlyArrivalNotification (declaration type is AVO)", false, wrapper.IsOnlyArrivalNotification);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected false IsOnlyArrivalNotification (declaration type is TNA)", false, wrapper.IsOnlyArrivalNotification);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected false IsOnlyArrivalNotification (declaration type is TAO)", false, wrapper.IsOnlyArrivalNotification);
			});
		}

		public void TestIsOnlyUnloadingRemarks()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected true IsOnlyUnloadingRemarks (declaration type is OBS)", true, wrapper.IsOnlyUnloadingRemarks);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected false IsOnlyUnloadingRemarks (declaration type is AVI)", false, wrapper.IsOnlyUnloadingRemarks);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected false IsOnlyUnloadingRemarks (declaration type is AVO)", false, wrapper.IsOnlyUnloadingRemarks);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected false IsOnlyUnloadingRemarks (declaration type is TNA)", false, wrapper.IsOnlyUnloadingRemarks);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected false IsOnlyUnloadingRemarks (declaration type is TAO)", false, wrapper.IsOnlyUnloadingRemarks);
			});
		}

		public void TestIsArrivalWithAVI()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected false IsArrivalWithAVI (declaration type is OBS)", false, wrapper.IsArrivalWithAVI);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected true IsArrivalWithAVI (declaration type is AVI)", true, wrapper.IsArrivalWithAVI);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected true IsArrivalWithAVI (declaration type is AVO)", true, wrapper.IsArrivalWithAVI);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected true IsArrivalWithAVI (declaration type is TNA)", true, wrapper.IsArrivalWithAVI);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected true IsArrivalWithAVI (declaration type is TAO)", true, wrapper.IsArrivalWithAVI);
			});
		}

		public void TestIsArrivalWithOBS()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected true IsArrivalWithOBS (declaration type is OBS)", true, wrapper.IsArrivalWithOBS);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected false IsArrivalWithOBS (declaration type is AVI)", false, wrapper.IsArrivalWithOBS);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected true IsArrivalWithOBS (declaration type is AVO)", true, wrapper.IsArrivalWithOBS);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected false IsArrivalWithOBS (declaration type is TNA)", false, wrapper.IsArrivalWithOBS);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected true IsArrivalWithOBS (declaration type is TAO)", true, wrapper.IsArrivalWithOBS);
			});
		}

		public void TestIsArrivalWithTNN()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected false IsArrivalWithTNN (declaration type is OBS)", false, wrapper.IsArrivalWithTNN);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected false IsArrivalWithTNN (declaration type is AVI)", false, wrapper.IsArrivalWithTNN);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected false IsArrivalWithTNN (declaration type is AVO)", false, wrapper.IsArrivalWithTNN);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected true IsArrivalWithTNN (declaration type is TNA)", true, wrapper.IsArrivalWithTNN);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected true IsArrivalWithTNN (declaration type is TAO)", true, wrapper.IsArrivalWithTNN);
			});
		}

		public void TestDocumentMessageName()
		{
			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected filled DocumentMessageName with declaration type ArrivalNotification", "AVI", wrapper.DocumentMessageName);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected filled DocumentMessageName with declaration type UloadingRemarks", "OBS", wrapper.DocumentMessageName);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected filled DocumentMessageName with declaration type ArrivalNotificationWithDepartureTnnPlusAvi", "TNA", wrapper.DocumentMessageName);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected filled DocumentMessageName with declaration type ArrivalNotificationWithUnloadingRemarksAviPlusObs", "AVO", wrapper.DocumentMessageName);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected filled DocumentMessageName with declaration type ArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb", "TAO", wrapper.DocumentMessageName);
			});
		}

		public void TestCustomsProcedureCategory1()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				nctsHeader.MovementHeader.BM_InBondEntryType = HeaderDataNCTS.DeclarationType;
				AssertEquals("Expected filled CustomsProcedureCategory1", HeaderDataNCTS.DeclarationType, wrapper.CustomsProcedureCategory1);
			});
		}

		public void TestCustomsProcedureCategory2()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				nctsHeader.MovementHeader.BM_InBondEntryType = HeaderDataNCTS.DeclarationType;
				AssertEquals("Expected empty CustomsProcedureCategory2", ZString.Empty, wrapper.CustomsProcedureCategory2);

				var customOffice = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
				customOffice.CY_Data = "AH";

				AssertEquals("Expected filled CustomsProcedureCategory2", "AH", wrapper.CustomsProcedureCategory2);
			});
		}

		public void TestCustomsTransitDestinationOffice()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty CustomsTransitDestinationOffice", ZString.Empty, wrapper.CustomsTransitDestinationOffice);

				var office = nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
				office.CY_Data = "7654321";
				AssertEquals("Expected filled CustomsTransitDestinationOffice with last 6 characters", "654321", wrapper.CustomsTransitDestinationOffice);

				office.CY_Data = "21";
				AssertEquals("Expected filled CustomsTransitDestinationOffice with 2 characters", "21", wrapper.CustomsTransitDestinationOffice);
			});
		}

		public void TestCustomsOfficesOfDestination()
		{
			nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNotNull("Expected not null CustomsOfficesOfDestination", wrapper.CustomsOfficesOfDestination);
		}

		public void TestDateOfArrival()
		{
			nctsHeader.ArrivalMovementHeader.BM_EntryDate = ZDateTime.BrettsBirthday;
			AssertEquals("Expected filled DateOfArrival", ZDateTime.BrettsBirthday, wrapper.DateOfArrival);
		}

		public void TestDateOfUnloading()
		{
			nctsHeader.UnloadingRemark.G9_UnloadingDate = ZDateTime.BrettsBirthday;
			AssertEquals("Expected filled DateOfUnloading", ZDateTime.BrettsBirthday, wrapper.DateOfUnloading);
		}

		public void TestGoodsInContainerIndicator()
		{
			CombineAssertions(() =>
			{
				nctsHeader.DepartureHeaderContainers.RemoveAndDeleteAll();
				AssertEquals("Expected filled GoodsInContainerIndicator without containers", "0", wrapper.GoodsInContainerIndicator);

				nctsHeader.DepartureHeaderContainers.AddNew();
				AssertEquals("Expected filled GoodsInContainerIndicator with containers", "1", wrapper.GoodsInContainerIndicator);
			});
		}

		public void TestUnloadingComplianceIndicator()
		{
			CombineAssertions(() =>
			{
				nctsHeader.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
				AssertEquals("Expected filled UnloadingComplianceIndicator", "1", wrapper.UnloadingComplianceIndicator);

				nctsHeader.UnloadingRemark.G9_Conform = YesNoList.Codes.No;
				AssertEquals("Expected filled UnloadingComplianceIndicator", "0", wrapper.UnloadingComplianceIndicator);
			});
		}

		public void TestSealsInGoodState()
		{
			CombineAssertions(() =>
			{
				nctsHeader.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
				AssertEquals("Expected filled SealsInGoodState", "B", wrapper.SealsInGoodState);

				nctsHeader.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.No;
				AssertEquals("Expected filled SealsInGoodState", "M", wrapper.SealsInGoodState);
			});
		}

		public void TestAttachedDocumentTypeA()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_InBondEntryType = "DAT";
				AssertEquals("Expected filled AttachedDocumentTypeA with EntryType equals to DAT", "A", wrapper.AttachedDocumentTypeA);

				nctsHeader.ArrivalMovementHeader.BM_InBondEntryType = "DUA";
				AssertEquals("Expected filled AttachedDocumentTypeA with EntryType equals to DUA", "4", wrapper.AttachedDocumentTypeA);

				nctsHeader.ArrivalMovementHeader.BM_InBondEntryType = "XX";
				AssertEquals("Expected empty AttachedDocumentTypeA with EntryType equals to invalid code", ZString.Empty, wrapper.AttachedDocumentTypeA);
			});
		}

		public void TestReceiverComplianceForAutoDischarge()
		{
			nctsHeader.ESNctsHeader.CEN_AutomaticCompletion = true;
			AssertEquals("Expected filled ReceiverComplianceForAutoDischarge", true, wrapper.ReceiverComplianceForAutoDischarge);
		}

		public void TestDirectlyLoadedOnCompletion()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_AutomaticTranshipment = true;
				AssertEquals("Expected filled DirectlyLoadedOnCompletion with AutomaticTranshipment equals to true", "1", wrapper.DirectlyLoadedOnCompletion);

				nctsHeader.ESNctsHeader.CEN_AutomaticTranshipment = false;
				AssertEquals("Expected filled DirectlyLoadedOnCompletion with AutomaticTranshipment equals to false", "0", wrapper.DirectlyLoadedOnCompletion);
			});
		}

		public void TestSealsStateDiscrepancies()
		{
			var seal = nctsHeader.Seals.AddNew();
			seal.IsBroken = false;

			seal = nctsHeader.Seals.AddNew();
			seal.IsBroken = true;

			var sealsStateDiscrepancies = wrapper.SealsStateDiscrepancies;

			AssertContainsExactElementsInAnyOrder("Expected filled SealsStateDiscrepancies", new string[] { "1", "2" }, sealsStateDiscrepancies);
			AssertSame("Cached SealsStateDiscrepancies", wrapper.SealsStateDiscrepancies, sealsStateDiscrepancies);
		}

		public void TestTIRCompletionNumber()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_TIRArrival = true;
				nctsHeader.ESNctsHeader.CEN_TIRCarnetPage = 22;
				AssertEquals("Expected filled TIRCompletionNumber with TIRArrival equals to true", "22", wrapper.TIRCompletionNumber);

				nctsHeader.ESNctsHeader.CEN_TIRArrival = false;
				AssertEquals("Expected empty TIRCompletionNumber with TIRArrival equals to false", string.Empty, wrapper.TIRCompletionNumber);
			});
		}

		public void TestTIRIsCompleteUnloading()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ESNctsHeader.CEN_TIRArrival = true;
				nctsHeader.ESNctsHeader.CEN_TIRPartialUnloading = true;
				AssertEquals("Expected filled TIRIsCompleteUnloading with TIRArrival and TIRPartialUnloading equals to true", "P", wrapper.TIRIsCompleteUnloading);

				nctsHeader.ESNctsHeader.CEN_TIRPartialUnloading = false;
				AssertEquals("Expected filled TIRIsCompleteUnloading with TIRArrival equals to true and TIRPartialUnloading equals to false", "T", wrapper.TIRIsCompleteUnloading);

				nctsHeader.ESNctsHeader.CEN_TIRArrival = false;
				AssertEquals("Expected empty TIRIsCompleteUnloading with TIRArrival and TIRPartialUnloading equals to false", string.Empty, wrapper.TIRIsCompleteUnloading);
			});
		}

		public void TestSealCodes()
		{
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Seal1 = "1234";
			container.BC_Seal2 = "5678";

			container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Seal1 = "1234";
			container.BC_Seal2 = "4321";

			var sealCodes = wrapper.SealCodes;

			AssertEquals("Expected filled SealCodes", new List<ZString> { "12345678", "12344321" }.ToString(), sealCodes.ToList().ToString());
			AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
		}

		public void TestSealCodesWithDiscrepancies()
		{
			var seal = nctsHeader.Seals.AddNew();
			seal.CY_Data = "123";

			seal = nctsHeader.Seals.AddNew();
			seal.CY_Data = "234";

			var sealCodesWithDiscrepancies = wrapper.SealCodesWithDiscrepancies;

			AssertEquals("Expected filled SealCodesWithDiscrepancies", new List<ZString> { "123", "234" }.ToString(), sealCodesWithDiscrepancies.ToList().ToString());
			AssertSame("Cached SealCodesWithDiscrepancies", wrapper.SealCodesWithDiscrepancies, sealCodesWithDiscrepancies);
		}

		public void TestReferenceGroup()
		{
			AssertNotNull("Expected not null ReferenceGroup", wrapper.ReferenceGroup);
		}

		public void TestTransitTransportMedium()
		{
			nctsHeader.UnloadedMeansOfTransportAtDepartureIdentity = "AH";
			AssertEquals("Expected filled TransitTransportMedium", "AH", wrapper.TransitTransportMedium);
		}

		public void TestTransportId()
		{
			nctsHeader.UnloadedMeansOfTransportAtDepartureIdentity = "AH";
			AssertEquals("Expected filled TransportId", "AH", wrapper.TransportId);
		}

		public void TestTransportNationality()
		{
			nctsHeader.UnloadedMeansOfTransportAtDepartureNationality = "AH";
			AssertEquals("Expected filled TransportNationality", "AH", wrapper.TransportNationality);
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				nctsHeader.DeclarantOrgPK = ZGuid.Empty;
				AssertNull("Expected null Declarant", wrapper.Declarant);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.DeclarantOrgPK = orgHeader.PK;
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				var declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestUnloadingObservations()
		{
			nctsHeader.HeaderUnloadingNotes = "AH";
			AssertEquals("Expected filled UnloadingObservations", "AH", wrapper.UnloadingObservations);
		}

		public void TestIsDeclarationInEuros()
		{
			AssertEquals("Expected filled IsDeclarationInEuros", true, wrapper.IsDeclarationInEuros);
		}

		public void TestLines()
		{
			CombineAssertions("For TNA declaration (arrival with departure)", () =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsHeader.MovementHeader.GoodsItems.AddNew();

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});

			CombineAssertions("For OBS declaration (unloading remarks)", () =>
			{
				var nctsHeaderOBS = Factory.New<NctsHeader>();
				nctsHeaderOBS.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderOBS.SetMovementType(NctsMovementType.Codes.Arrival);
				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, hasDifferences: true);
				var wrapperOBS = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);

				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapperOBS.Lines.Count);

				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, isMissing: true);
				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS);
				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, isNew: false);

				wrapperOBS = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);

				var linesOBS = wrapperOBS.Lines;

				AssertEquals("Expected 3 Lines. Lines that are not different, missing or new are not included in OBS Lines", 3, linesOBS.Count);
				AssertSame("Cached Lines", wrapperOBS.Lines, linesOBS);
			});

			CombineAssertions("For TAO declaration (arrival with departure and unloading remarks)", () =>
			{
				var nctsHeaderTAO = Factory.New<NctsHeader>();
				nctsHeaderTAO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderTAO.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				nctsHeaderTAO.UnloadingMovementHeader.GoodsItems.AddNew();
				var wrapperTAO = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);

				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapperTAO.Lines.Count);

				nctsHeaderTAO.UnloadingMovementHeader.GoodsItems.AddNew();
				nctsHeaderTAO.MovementHeader.GoodsItems.AddNew();
				nctsHeaderTAO.MovementHeader.GoodsItems.AddNew();

				wrapperTAO = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);

				var linesTAO = wrapperTAO.Lines;

				AssertEquals("Expected 4 Lines", 4, linesTAO.Count);
				AssertSame("Cached Lines", wrapperTAO.Lines, linesTAO);
			});

			CombineAssertions("For AVI declaration (only arrival)", () =>
			{
				var nctsHeaderAVI = Factory.New<NctsHeader>();
				nctsHeaderAVI.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderAVI.SetMovementType(NctsMovementType.Codes.Arrival);
				var wrapperAVI = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);

				AssertEquals("Expected 0 Lines since for arrival we don't have goods items", 0, wrapperAVI.Lines.Count);

				nctsHeaderAVI.UnloadingMovementHeader.GoodsItems.AddNew();

				wrapperAVI = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);

				var linesAVI = wrapperAVI.Lines;

				AssertEquals("Expected 0 Lines (AVI should always have 0 lines)", 0, linesAVI.Count);
				AssertSame("Cached Lines", wrapperAVI.Lines, linesAVI);
			});
		}

		public void TestTotalNumberOfGoods()
		{
			CombineAssertions("For TNA declaration (arrival with departure)", () =>
			{
				nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsHeader.MovementHeader.GoodsItems.AddNew();
				AssertEquals("Expected 3 TotalNumberOfGoods", 3, wrapper.TotalNumberOfGoods);
			});

			CombineAssertions("For OBS declaration (unloading remarks)", () =>
			{
				var nctsHeaderOBS = Factory.New<NctsHeader>();
				nctsHeaderOBS.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				nctsHeaderOBS.SetMovementType(NctsMovementType.Codes.Arrival);

				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, hasDifferences: true);
				var wrapperOBS = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);

				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, isMissing: true);
				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS);
				AddUnloadingGoodsItemToNctsHeader(nctsHeaderOBS, isNew: false);
				AssertEquals("Expected 3 TotalNumberOfGoods. Lines that are not different, missing or new are not included in OBS Lines", 3, wrapperOBS.TotalNumberOfGoods);
			});

			CombineAssertions("For TAO declaration (arrival with departure and unloading remarks)", () =>
			{
				var nctsHeaderTAO = Factory.New<NctsHeader>();
				nctsHeaderTAO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				nctsHeaderTAO.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				nctsHeaderTAO.UnloadingMovementHeader.GoodsItems.AddNew();
				var wrapperTAO = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);

				nctsHeaderTAO.UnloadingMovementHeader.GoodsItems.AddNew();
				nctsHeaderTAO.MovementHeader.GoodsItems.AddNew();
				nctsHeaderTAO.MovementHeader.GoodsItems.AddNew();

				AssertEquals("Expected 4 TotalNumberOfGoods", 4, wrapperTAO.TotalNumberOfGoods);
			});

			CombineAssertions("For AVI declaration (only arrival)", () =>
			{
				var nctsHeaderAVI = Factory.New<NctsHeader>();
				nctsHeaderAVI.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				nctsHeaderAVI.SetMovementType(NctsMovementType.Codes.Arrival);
				var wrapperAVI = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);

				AssertEquals("Expected 0 TotalNumberOfGoods since for arrival we don't have goods items", 0, wrapperAVI.TotalNumberOfGoods);

				nctsHeaderAVI.UnloadingMovementHeader.GoodsItems.AddNew();
				AssertEquals("Expected 0 TotalNumberOfGoods (AVI should always have 0 lines)", 0, wrapperAVI.TotalNumberOfGoods);
			});
		}

		public void TestTotalNumberOfPackageElements()
		{
			CombineAssertions("For TNA declaration (arrival with departure)", () =>
			{
				AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalNumberOfPackageElements);

				goodsItem.IsVehicles = true;
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();
				var wrapper1 = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected filled TotalNumberOfPackageElements with vehicles", 2, wrapper1.TotalNumberOfPackageElements);

				goodsItem.IsVehicles = false;
				var pack1 = goodsItem.Packages.AddNew();
				var pack2 = goodsItem.Packages.AddNew();
				var wrapper2 = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected filled TotalNumberOfPackageElements with empty packages", 2, wrapper2.TotalNumberOfPackageElements);

				pack1.B5_UnitCount = 2;
				pack2.B5_UnitCount = 1;
				var wrapper3 = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 3, wrapper3.TotalNumberOfPackageElements);
			});

			CombineAssertions("For OBS declaration (unloading remarks)", () =>
			{
				var nctsHeaderOBS = Factory.New<NctsHeader>();
				nctsHeaderOBS.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderOBS.SetMovementType(NctsMovementType.Codes.Arrival);
				var goodsItemOBS = nctsHeaderOBS.UnloadingMovementHeader.GoodsItems.AddNew();
				var wrapperOBS = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);

				AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapperOBS.TotalNumberOfPackageElements);

				var packOBS1 = goodsItemOBS.Packages.AddNew();
				var packOBS2 = goodsItemOBS.Packages.AddNew();
				var wrapperOBS2 = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected filled TotalNumberOfPackageElements with empty packages", 2, wrapperOBS2.TotalNumberOfPackageElements);

				packOBS1.B5_UnitCount = 2;
				packOBS2.B5_UnitCount = 1;
				var wrapperOBS3 = new ArrivalSendMessageWrapper(nctsHeaderOBS, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 3, wrapperOBS3.TotalNumberOfPackageElements);
			});

			CombineAssertions("For TAO declaration (arrival with departure and unloading remarks)", () =>
			{
				var nctsHeaderTAO = Factory.New<NctsHeader>();
				nctsHeaderTAO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderTAO.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				var goodsItemTAO = nctsHeaderTAO.UnloadingMovementHeader.GoodsItems.AddNew();
				var goodsItemTAO2 = nctsHeaderTAO.MovementHeader.GoodsItems.AddNew();
				var wrapperTAO = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);

				AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapperTAO.TotalNumberOfPackageElements);

				goodsItemTAO2.IsVehicles = true;
				goodsItemTAO2.Packages.AddNew();
				goodsItemTAO2.Packages.AddNew();
				var wrapperTAO1 = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected filled TotalNumberOfPackageElements with vehicles", 2, wrapperTAO1.TotalNumberOfPackageElements);

				var packTAO1 = goodsItemTAO.Packages.AddNew();
				var packTAO2 = goodsItemTAO.Packages.AddNew();

				goodsItemTAO2.IsVehicles = false;
				var packTAO3 = goodsItemTAO2.Packages.AddNew();
				var packTAO4 = goodsItemTAO2.Packages.AddNew();
				var wrapperTAO2 = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected filled TotalNumberOfPackageElements with empty packages", 4, wrapperTAO2.TotalNumberOfPackageElements);

				packTAO1.B5_UnitCount = 2;
				packTAO2.B5_UnitCount = 1;

				packTAO3.B5_UnitCount = 2;
				packTAO4.B5_UnitCount = 1;
				var wrapperTAO3 = new ArrivalSendMessageWrapper(nctsHeaderTAO, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 6, wrapperTAO3.TotalNumberOfPackageElements);
			});

			CombineAssertions("For AVI declaration (only arrival)", () =>
			{
				var nctsHeaderAVI = Factory.New<NctsHeader>();
				nctsHeaderAVI.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeaderAVI.SetMovementType(NctsMovementType.Codes.Arrival);
				var goodsItemAVI = nctsHeaderAVI.UnloadingMovementHeader.GoodsItems.AddNew();
				var wrapperAVI = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);

				AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapperAVI.TotalNumberOfPackageElements);

				var packAVI1 = goodsItemAVI.Packages.AddNew();
				var packAVI2 = goodsItemAVI.Packages.AddNew();
				var wrapperAVI2 = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected empty TotalNumberOfPackageElements with empty packages since AVI should always have 0 lines", 0, wrapperAVI2.TotalNumberOfPackageElements);

				packAVI1.B5_UnitCount = 2;
				packAVI2.B5_UnitCount = 1;
				var wrapperAVI3 = new ArrivalSendMessageWrapper(nctsHeaderAVI, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected empty TotalNumberOfPackageElements with full packages since AVI should always have 0 lines", 0, wrapperAVI3.TotalNumberOfPackageElements);
			});
		}

		public void TestLocalReferenceNumber()
		{
			nctsHeader.LocalReferenceNumber = "NCTS00000001";

			CombineAssertions(() =>
			{
				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				AssertEquals("Expected filled LocalReferenceNumber with declaration type ArrivalNotification", "AVINCTS00000001", wrapper.LocalReferenceNumber);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
				AssertEquals("Expected filled LocalReferenceNumber with declaration type UloadingRemarks", "OBSNCTS00000001", wrapper.LocalReferenceNumber);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
				AssertEquals("Expected filled LocalReferenceNumber with declaration type ArrivalNotificationWithDepartureTnnPlusAvi", "TNANCTS00000001", wrapper.LocalReferenceNumber);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);
				AssertEquals("Expected filled LocalReferenceNumber with declaration type ArrivalNotificationWithUnloadingRemarksAviPlusObs", "AVONCTS00000001", wrapper.LocalReferenceNumber);

				wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);
				AssertEquals("Expected filled LocalReferenceNumber with declaration type ArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb", "TAONCTS00000001", wrapper.LocalReferenceNumber);
			});
		}

		void AddUnloadingGoodsItemToNctsHeader(NctsHeader nctsHeader, bool hasDifferences = false, bool isMissing = false, bool isNew = true)
		{
			var nctsUnloadingLine = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			if (hasDifferences || isMissing || !isNew)
			{
				nctsUnloadingLine.IsNew = false;
				nctsUnloadingLine.HasDifferences = hasDifferences;
				nctsUnloadingLine.IsMissing = isMissing;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		ArrivalSendMessageWrapper wrapper;

		protected override ArrivalSendMessageWrapper GetProvider() => wrapper;
	}
}
