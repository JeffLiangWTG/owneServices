using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class WarehouseAdjustmentJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_OA_Representative_Mandatory()
		{
			var message = "Please enter a Representative for Rep. Type 'DIR'.";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				jobDeclaration.JE_OA_Representative = ZGuid.Empty;
				AssertHasMessageError("JE_DeclarantType is 'DIR'", jobDeclaration.JE_OA_RepresentativeInfo, message);

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertNoMessageError("JE_DeclarantType isn't 'DIR'", jobDeclaration.JE_OA_RepresentativeInfo, message);
			});
		}

		public void TestCheckJE_EntryStyle()
		{
			jobDeclaration.Validation.ValidateJE_EntryStyle();
			AssertNoNotifications(jobDeclaration.JE_EntryStyleInfo);
		}

		public void TestCheckJE_TransportMode()
		{
			jobDeclaration.Validation.ValidateJE_TransportMode();
			AssertNoNotifications(jobDeclaration.JE_TransportModeInfo);
		}

		public void TestCheckJE_ContainerMode()
		{
			jobDeclaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications(jobDeclaration.JE_ContainerModeInfo);
		}

		public void TestCheckJE_MasterBill()
		{
			jobDeclaration.Validation.ValidateJE_MasterBill();
			AssertNoNotifications(jobDeclaration.JE_MasterBillInfo);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoNotifications(jobDeclaration.JE_VoyageFlightNoInfo);
		}

		public void TestCheckJE_VesselName()
		{
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoNotifications(jobDeclaration.JE_VesselNameInfo);
		}

		public void TestCheckJE_RN_NKTransportNationality()
		{
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoNotifications(jobDeclaration.JE_RN_NKTransportNationalityInfo);
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoNotifications(jobDeclaration.JE_RL_NKPortOfLoadingInfo);
		}

		public void TestCheckJE_ExportDate()
		{
			jobDeclaration.Validation.ValidateJE_ExportDate();
			AssertNoNotifications(jobDeclaration.JE_ExportDateInfo);
		}

		public void TestCheckJE_RL_NKPortOfFirstArrival()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKPortOfFirstArrival();
			AssertNoNotifications(jobDeclaration.JE_RL_NKPortOfFirstArrivalInfo);
		}

		public void TestCheckJE_DateOfFirstArrival()
		{
			jobDeclaration.Validation.ValidateJE_DateOfFirstArrival();
			AssertNoNotifications(jobDeclaration.JE_DateOfFirstArrivalInfo);
		}

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoNotifications(jobDeclaration.JE_RL_NKPortOfArrivalInfo);
		}

		public void TestCheckJE_DateOfArrival()
		{
			jobDeclaration.Validation.ValidateJE_DateOfArrival();
			AssertNoNotifications(jobDeclaration.JE_DateOfArrivalInfo);
		}

		public void TestCheckJE_HouseBill()
		{
			jobDeclaration.Validation.ValidateJE_HouseBill();
			AssertNoNotifications(jobDeclaration.JE_HouseBillInfo);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoNotifications(jobDeclaration.JE_RL_NKOriginInfo);
		}

		public void TestCheckJE_GoodsOrigin()
		{
			jobDeclaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoNotifications(jobDeclaration.JE_GoodsOriginInfo);
		}

		public void TestCheckJE_DateAtOrigin()
		{
			jobDeclaration.Validation.ValidateJE_DateAtOrigin();
			AssertNoNotifications(jobDeclaration.JE_DateAtOriginInfo);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoNotifications(jobDeclaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestCheckJE_GoodsDestination()
		{
			jobDeclaration.Validation.ValidateJE_GoodsDestination();
			AssertNoNotifications(jobDeclaration.JE_GoodsDestinationInfo);
		}

		public void TestCheckJE_DateAtFinalDestination()
		{
			jobDeclaration.Validation.ValidateJE_DateAtFinalDestination();
			AssertNoNotifications(jobDeclaration.JE_DateAtFinalDestinationInfo);
		}

		public void TestCheckJE_GoodsDescription()
		{
			jobDeclaration.Validation.ValidateJE_GoodsDescription();
			AssertNoNotifications(jobDeclaration.JE_GoodsDescriptionInfo);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			jobDeclaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoNotifications(jobDeclaration.JE_LocationOfGoodsInfo);
		}

		public void TestCheckJE_TotalNoOfPacks()
		{
			jobDeclaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoNotifications(jobDeclaration.JE_TotalNoOfPacksInfo);
		}

		public void TestCheckJE_TotalNoOfPacksPackType()
		{
			jobDeclaration.Validation.ValidateJE_TotalNoOfPacksPackType();
			AssertNoNotifications(jobDeclaration.JE_TotalNoOfPacksPackTypeInfo);
		}

		public void TestCheckJE_TotalWeight()
		{
			jobDeclaration.Validation.ValidateJE_TotalWeight();
			AssertNoNotifications(jobDeclaration.JE_TotalWeightInfo);
		}

		public void TestCheckJE_TotalWeightUnit()
		{
			jobDeclaration.Validation.ValidateJE_TotalWeightUnit();
			AssertNoNotifications(jobDeclaration.JE_TotalWeightUnitInfo);
		}

		public void TestJE_ShipmentIncoTerm()
		{
			jobDeclaration.Validation.ValidateJE_ShipmentIncoTerm();
			AssertNoNotifications(jobDeclaration.JE_ShipmentIncoTermInfo);
		}

		public void TestCheckJE_TotalVolume()
		{
			jobDeclaration.Validation.ValidateJE_TotalVolume();
			AssertNoNotifications(jobDeclaration.JE_TotalVolumeInfo);
		}

		public void TestCheckJE_TotalVolumeUnit()
		{
			jobDeclaration.Validation.ValidateJE_TotalVolumeUnit();
			AssertNoNotifications(jobDeclaration.JE_TotalVolumeUnitInfo);
		}

		public void TestCheckJE_ShipmentIncoTermPlace()
		{
			jobDeclaration.Validation.ValidateJE_ShipmentIncoTermPlace();
			AssertNoNotifications(jobDeclaration.JE_ShipmentIncoTermPlaceInfo);
		}

		public void TestCheckJE_AgentsReference()
		{
			jobDeclaration.Validation.ValidateJE_AgentsReference();
			AssertNoNotifications(jobDeclaration.JE_AgentsReferenceInfo);
		}

		public void TestCheckJE_UCR()
		{
			jobDeclaration.Validation.ValidateJE_UCR();
			AssertNoNotifications(jobDeclaration.JE_UCRInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
		}
		JobDeclaration jobDeclaration;
	}
}
