using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetail))]
	sealed class CusInBondMoveDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Customs In Bond Move Details", cusInBondMoveDetail.HumanReadableName);
		}

		public void TestTypeDecider()
		{
			AssertType<CusInBondMoveDetailTypeDecider>(CusInBondMoveDetail.TypeDecider);
		}

		public void TestArrivalMoveHeader()
		{
			AssertType<NctsArrivalMovementHeader>(cusInBondMoveDetail.ArrivalMoveHeader);
		}

		public void TestDepartureMoveHeader()
		{
			var cusInBondMoveDetailDeparture = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS5, declarationType: NctsMovementType.Codes.Departure);
			AssertType<NctsDepartureMovementHeader>(cusInBondMoveDetailDeparture.DepartureMoveHeader);
		}

		public void TestCusInBondContainerType()
		{
			var suporter = cusInBondMoveDetail as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(CusInBondContainer), suporter.ContainerType);
			AssertEquals(typeof(CusInBondContainer), cusInBondMoveDetail.ContainerType);
		}

		public void TestAddressRequirementType() => CombineAssertions(() =>
		{
			AssertType<JobDocAddressRequirement>(cusInBondMoveDetail.ConsignorDocAddressRequirement);
			AssertType<JobDocAddressRequirement>(cusInBondMoveDetail.ConsigneeDocAddressRequirement);
		});

		public void TestDifferenceWeight()
		{
			var bill = cusInBondMoveDetail.Bill;
			bill.B0_WeightUQ = "KG";
			CombineAssertions(() =>
			{
				AssertEquals("Invisible on UI", ZDecimal.Zero, cusInBondMoveDetail.DifferenceWeight);

				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
				goodsItem1.BY_GrossWeight = 10;
				goodsItem1.BY_GrossWeightUnit = "KG";
				var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
				goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				goodsItem2.BY_GrossWeight = 2;
				goodsItem2.BY_GrossWeightUnit = "KG";
				var unloadedGoodsItem = goodsItem2.UnloadedGoodsItem;
				unloadedGoodsItem.BY_GrossWeight = 10;
				unloadedGoodsItem.BY_GrossWeightUnit = "KG";

				var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
				goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				goodsItem3.BY_GrossWeight = 60;
				goodsItem3.BY_GrossWeightUnit = "KG";

				var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
				goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				goodsItem4.BY_GrossWeight = 6000;
				goodsItem4.BY_GrossWeightUnit = "G";
				AssertEquals("B0_WeightUQ is 'KG'", new ZDecimal(26), cusInBondMoveDetail.DifferenceMoveDetail.DifferenceWeight);

				bill.B0_WeightUQ = "G";
				AssertEquals("B0_WeightUQ is 'G'", new ZDecimal(26000), cusInBondMoveDetail.DifferenceMoveDetail.DifferenceWeight);
			});
		}

		public void TestDifferenceWeightUnit()
		{
			cusInBondMoveDetail.Bill.B0_WeightUQ = "KG";
			AssertEquals("KG", cusInBondMoveDetail.DifferenceWeightUnit);
		}

		public void TestB9_SeqNo_ReadOnly()
		{
			AssertEquals(true, cusInBondMoveDetail.B9_SeqNoInfo.ReadOnly);
		}

		public void TestB9_SeqNo_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_SeqNoInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence No.", resourceStringData.Caption);
				AssertEquals("FullDescription", "Sequence Number of House Bill", resourceStringData.FullDescription);
				AssertEquals("ShortCaption", "Seq. No.", resourceStringData.ShortCaption);
			});
		}

		public void TestConsignorName_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.ConsignorNameInfo);
			AssertEquals("Caption", "Consignor", resourceStringData.Caption);
		}

		public void TestConsigneeName_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.ConsigneeNameInfo);
			AssertEquals("Caption", "Consignee", resourceStringData.Caption);
		}

		public void TestB9_UnloadedState_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_UnloadedStateInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Unloaded State", resourceStringData.Caption);
				AssertEquals("ShortCaption", "State", resourceStringData.ShortCaption);
			});
		}

		public void TestBill()
		{
			var bill1 = nctsHeader.Bills.AddNew();
			cusInBondMoveDetail.B9_B0 = bill1.PK;
			AssertEquals("Bill type", typeof(NctsBill), cusInBondMoveDetail.Bill.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("Lookups type", typeof(CusInBondMoveDetailLookups), cusInBondMoveDetail.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals("Validation type", typeof(CusInBondMoveDetailValidation), cusInBondMoveDetail.Validation.GetType());
		}

		public void TestConsigneeDocAddress()
		{
			var address = cusInBondMoveDetail.ConsigneeDocAddress;
			CombineAssertions(() =>
			{
				AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ConsigneeAddress)", address);
				AssertEquals("DocAddressType", DocAddressType.ConsigneeAddress, address.DocAddressType);
				AssertEquals("Consignee Read-Only", true, address.ReadOnly);
			});
		}

		public void TestConsignorDocAddress()
		{
			var address = cusInBondMoveDetail.ConsignorDocAddress;
			CombineAssertions(() =>
			{
				AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ConsignorAddress)", address);
				AssertEquals("DocAddressType", DocAddressType.ConsignorDocumentaryAddress, address.DocAddressType);
				AssertEquals("Consignor Read-Only", true, address.ReadOnly);
			});
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Arrival_Phase5()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).AreUnloadingRemarksFullyAccepted, cusInBondMoveDetail);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.IsUnloadingRemarksReadOnly);
		}

		public void TestB9_TransportAtDepartureID_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_TransportAtDepartureIDInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_TransportAtDepartureID_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
		}

		public void TestB9_RN_NKTransportAtDepartureIDNationality_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_RN_NKTransportAtDepartureIDNationality_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
		}

		public void TestB9_TransportAtDepartureTrailer1RegNo_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_TransportAtDepartureTrailer1RegNo_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
		}

		public void TestB9_TransportAtDepartureID_Caption()
		{
			AssertEquals("Transport ID", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_TransportAtDepartureIDInfo).Caption);
		}

		public void TestB9_RN_NKTransportAtDepartureIDNationality_Caption()
		{
			AssertEquals("Nationality", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo).Caption);
		}

		public void TestB9_TransportAtDepartureTrailer1RegNo_Caption()
		{
			AssertEquals("Trailer ID 1", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo).Caption);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer1Nationality_Caption()
		{
			AssertEquals("Nationality", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo).Caption);
		}

		public void TestB9_TransportAtDepartureTrailer2RegNo_Caption()
		{
			AssertEquals("Trailer ID 2", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo).Caption);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer2Nationality_Caption()
		{
			AssertEquals("Nationality", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo).Caption);
		}

		public void TestB9_AircraftIDAtDeparture_Caption()
		{
			AssertEquals("Aircraft ID", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo).Caption);
		}

		public void TestVesselNameAtDeparture_Caption()
		{
			AssertEquals("Vessel", DataBoundResourceStrings.GetDataForProperty(cusInBondMoveDetail.VesselNameAtDepartureInfo).Caption);
		}

		public void TestUnloadedStateAfterWeightEntered()
		{
			CombineAssertions(() =>
			{
				cusInBondMoveDetail.Bill.B0_Weight = 1;
				cusInBondMoveDetail.Bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Unloaded State should be NEW after weight & unit has been filled in.", NctsUnloadedStateList.Codes.NEW, cusInBondMoveDetail.B9_UnloadedState);
				cusInBondMoveDetail.Bill.B0_Weight = 2;
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertNotEquals("Unloaded State should not change again, if weight & unit has already been filled in.", NctsUnloadedStateList.Codes.NEW, cusInBondMoveDetail.B9_UnloadedState);
			});
		}

		public void TestRecordCreatedOnUnloadedStateDIF()
		{
			cusInBondMoveDetail.B9_SeqNo = "4";
			cusInBondMoveDetail.Bill.B0_SecurityIndicatorFromExport = true;
			cusInBondMoveDetail.Bill.B0_ReferenceID = "refid";
			cusInBondMoveDetail.Bill.B0_Weight = 1;
			cusInBondMoveDetail.Bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DIF;
			Factory.Save();

			var moveDetailChild = Factory.LoadTop1<CusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, cusInBondMoveDetail.PK));

			CombineAssertions(() =>
			{
				AssertEquals("B9_B0", cusInBondMoveDetail.Bill.PK, moveDetailChild.Bill.PK);
				AssertEquals("B9_B9_inBondMoveDetail", cusInBondMoveDetail.PK, moveDetailChild.B9_B9_InBondMoveDetail);
				AssertEquals("B9_SeqNo", cusInBondMoveDetail.B9_SeqNo, moveDetailChild.B9_SeqNo);
				AssertEquals("B9_UnloadedState", NctsUnloadedStateListForHouseConsignment.Codes.NEW, moveDetailChild.B9_UnloadedState);

				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DIF;
				Factory.Save();
				AssertEquals("No extra child should be created when status was already DIF", 1, Factory.Load<CusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, cusInBondMoveDetail.PK)).Length);
			});
		}

		public void TestRecordCreatedOnUnloadStateDIFOfSameType()
		{
			var moveDetail = Factory.New<CusInBondMoveDetailForTesting>();
			moveDetail.B9_B0 = cusInBondMoveDetail.B9_B0;
			moveDetail.B9_BM = cusInBondMoveDetail.B9_BM;
			moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertType<CusInBondMoveDetailForTesting>("Differences MoveDetail is of same type as parent MoveDetail", moveDetail.DifferenceMoveDetailCollection.Last());
		}

		public void TestRecordNotCreatedOnUnloadedStateDIFWhenB9_B0IsEmpty()
		{
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.MovementDetails.DeleteAll();
			var movementDetail = movementHeader.MovementDetails.AddNew();
			movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("No additional MovementDetails are created", movementDetail.PK, movementHeader.MovementDetails.Single().PK);
		}

		public void TestRecordDeletedOnUnloadedStateDIFToDECOrMIS()
		{
			bool MoveDetailHasChild() => Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(CusInBondMoveDetail)), new ZQuery(CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, cusInBondMoveDetail.PK));

			void AddChild()
			{
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DIF;
				Factory.Save();
				AssertEquals("MoveDetail child exists after UnloadedState is changed to DIF", true, MoveDetailHasChild());
			}

			CombineAssertions(() =>
			{
				AddChild();
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
				Factory.Save();
				AssertEquals("MoveDetail child no longer exists after UnloadedState is changed to DEC", false, MoveDetailHasChild());

				AddChild();
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;
				Factory.Save();
				AssertEquals("MoveDetail child no longer exists after UnloadedState is changed to MIS", false, MoveDetailHasChild());
			});
		}

		public void TestB9_UnloadedState_Captions_Phase5Arrival()
		{
			NCTSTestHelper.AssertCaptions(cusInBondMoveDetail.B9_UnloadedStateInfo, "Unloaded State", string.Empty, "State");
		}

		public void TestB9_UnloadedStateReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 4", false, CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4).B9_UnloadedStateInfo.ReadOnly);
				AssertEquals("Phase 5", true, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestB9_TransportAtDepartureTrailer2RegNo_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_TransportAtDepartureTrailer2RegNo_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
			});
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
		}

		public void TestB9_AircraftIDAtDeparture_ReadOnly_Phase5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", true, cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", false, cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_AircraftIDAtDepartureInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_AircraftIDAtDeparture_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);

			AssertEquals(false, movementDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
		}

		public void TestB9_UnloadedState_ReadOnly_Phase5()
		{
			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
				AssertEquals("Status empty", false, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status DEC", false, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
				cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status NEW", true, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
			});
			cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(cusInBondMoveDetail.ArrivalMoveHeader, x => ((CusInBondMoveDetail)x).B9_UnloadedStateInfo.ReadOnly, cusInBondMoveDetail);
		}

		public void TestB9_UnloadedState_ReadOnly_Phase4()
		{
			var movementDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS4);
			movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			AssertEquals(false, movementDetail.B9_UnloadedStateInfo.ReadOnly);
		}

		public void TestShouldValidateOnSave_ArrivalGoodsItem()
		{
			AssertShouldValidateOnSave(cusInBondMoveDetail.Bill.ArrivalGoodsItems, "BY_UnloadedState");
		}

		public void TestShouldValidateOnSave_TransportMean()
		{
			AssertShouldValidateOnSave(cusInBondMoveDetail.Bill.ArrivalTransportInfos, "TPM_TransportState");
		}

		public void TestShouldValidateOnSave_SupportingDocument()
		{
			AssertShouldValidateOnSave(cusInBondMoveDetail.Bill.SupportingDocuments);
		}

		public void TestShouldValidateOnSave_AdditionalDocument()
		{
			AssertShouldValidateOnSave(cusInBondMoveDetail.Bill.AdditionalDocuments);
		}

		public void TestShouldValidateOnSave_PreviousDocument()
		{
			AssertShouldValidateOnSave(cusInBondMoveDetail.Bill.PreviousDocuments);
		}

		void AssertShouldValidateOnSave(IBusinessObjectCollection collection, string propertyInfo = "CSI_Status")
		{
			CombineAssertions(collection.GetType().Name, () =>
			{
				AssertEquals("Before PreSaveValidation", true, cusInBondMoveDetail.ShouldValidateOnSave);

				cusInBondMoveDetail.RunPreSaveValidation();
				AssertEquals("After PreSaveValidation", false, cusInBondMoveDetail.ShouldValidateOnSave);

				var item = collection.AddNew();
				AssertEquals("After AddNew", true, cusInBondMoveDetail.ShouldValidateOnSave);

				cusInBondMoveDetail.RunPreSaveValidation();
				AssertEquals("After AddNew & PreSaveValidation", false, cusInBondMoveDetail.ShouldValidateOnSave);

				item.FindPropertyInfo(propertyInfo).RefreshBindingForParentRelationFK();
				AssertEquals("After RefreshBindingForParentRelationFK", true, cusInBondMoveDetail.ShouldValidateOnSave);
			});
		}

		public void TestSentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.IsUnloadingRemarksReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestB9_TransportAtDepartureID_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_TransportAtDepartureIDInfo.ReadOnly);
			});
		}

		public void TestB9_RN_NKTransportAtDepartureIDNationality_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
			});
		}

		public void TestB9_TransportAtDepartureTrailer1RegNo_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
			});
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
			});
		}

		public void TestB9_TransportAtDepartureTrailer2RegNo_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
			});
		}

		public void TestB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
			});
		}

		public void TestB9_AircraftIDAtDeparture_ReadOnly_SentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_AircraftIDAtDepartureInfo.ReadOnly);
			});
		}

		public void TestB9_UnloadedState_ReadOnly_SentToCustoms()
		{
			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, cusInBondMoveDetail.B9_UnloadedStateInfo.ReadOnly);
			});
		}

		public void TestB9_UnloadedState_ShouldSetGoodsItemsUnloadedStateToMIS_WhenSetMIS()
		{
			var bill = cusInBondMoveDetail.Bill;
			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;

			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DIF;

			var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.NEW;

			var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;

			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;

			CombineAssertions(() =>
			{
				AssertEquals("GoodsItem1 state should change to 'MIS'", NctsUnloadedStateListForHouseConsignment.Codes.MIS, goodsItem1.BY_UnloadedState);
				AssertEquals("GoodsItem2 state should change to 'MIS'", NctsUnloadedStateListForHouseConsignment.Codes.MIS, goodsItem2.BY_UnloadedState);
				AssertEquals("GoodsItem3 state should remain 'NEW'", NctsUnloadedStateListForHouseConsignment.Codes.NEW, goodsItem3.BY_UnloadedState);
				AssertEquals("GoodsItem4 state should remain 'MIS'", NctsUnloadedStateListForHouseConsignment.Codes.MIS, goodsItem4.BY_UnloadedState);
			});
		}

		public void TestB9_UnloadedState_ShouldRestoreToStateDEC_WhenChangedFromMIS()
		{
			var bill = cusInBondMoveDetail.Bill;

			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.NEW;

			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.MIS;

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(NctsUnloadedStateListForHouseConsignment.Codes.MIS, goodsItem1.BY_UnloadedState);
				AssertEquals(NctsUnloadedStateListForHouseConsignment.Codes.NEW, goodsItem2.BY_UnloadedState);
			});

			cusInBondMoveDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DIF;

			CombineAssertions("Unloaded states restored", () =>
			{
				AssertEquals("GoodsItem1 state should be restored to 'DEC'", NctsUnloadedStateListForHouseConsignment.Codes.DEC, goodsItem1.BY_UnloadedState);
				AssertEquals("GoodsItem2 state should remain 'NEW'", NctsUnloadedStateListForHouseConsignment.Codes.NEW, goodsItem2.BY_UnloadedState);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateMoveDetail(factory, CusInBondApplicationCodeList.Codes.NCTS5);

		protected override BusinessObject GetNewBusinessObject() => CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS5);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS5);

		protected override void SetUp()
		{
			cusInBondMoveDetail = CreateMoveDetail(Factory, CusInBondApplicationCodeList.Codes.NCTS5);
			nctsHeader = cusInBondMoveDetail.ArrivalMoveHeader.Header;
		}

		NctsHeader nctsHeader;
		CusInBondMoveDetail cusInBondMoveDetail;

		static CusInBondMoveDetail CreateMoveDetail(BusinessObjectFactory factory, string applicationCode, string declarationType = NctsMovementType.Codes.Arrival)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = applicationCode;
			header.SetMovementType(declarationType);
			return applicationCode == CusInBondApplicationCodeList.Codes.NCTS5
				? header.Bills.AddNew().MovementDetail
				: header.ArrivalMovementHeader.MovementDetails.AddNew();
		}

		class CusInBondMoveDetailForTesting : CusInBondMoveDetail
		{
			public CusInBondMoveDetailForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
