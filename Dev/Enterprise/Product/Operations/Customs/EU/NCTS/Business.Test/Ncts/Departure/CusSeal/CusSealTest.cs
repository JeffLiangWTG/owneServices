using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusSeal))]
	sealed class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusSealTypeDecider>(CusSeal.TypeDecider);
		}

		public void TestBK_UnloadingState_ReadOnly_NCTS5_Arrival_ART_FRC()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;
			var seal = header.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();

			CombineAssertions(() =>
			{
				seal.BK_UnloadingState = "NEW";
				AssertEquals("Status not MIS, DIF, DEC", false, seal.BK_UnloadingStateInfo.ReadOnly);
				seal.BK_UnloadingState = "MIS";
				AssertEquals("Status MIS", true, seal.BK_UnloadingStateInfo.ReadOnly);
				AssertEquals("Readonly decision made for appropriate parent", typeof(NctsDepartureHeaderContainer), seal.ParentType);
			});
		}

		public void TestBK_SealNumber_ReadOnly_NCTS5_Arrival_ART_FRC()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var moveHeader = header.ArrivalMovementHeader;
			var seal = header.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("not NCTS5_Arrival_ART_FRC", false, seal.BK_SealNumberInfo.ReadOnly);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;
				AssertEquals("NCTS5_Arrival_ART_FRC", true, seal.BK_SealNumberInfo.ReadOnly);
				AssertEquals("Readonly decision made for appropriate parent", typeof(NctsDepartureHeaderContainer), seal.ParentType);
			});
		}

		public void TestHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var seal = container1.AdditionalSeals.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Departure", nctsHeader, seal.Header);
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var container = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
				seal = container.Seals.AddNew();
				AssertEquals("For Incident", nctsHeader, seal.Header);

				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
				seal = headerContainer.Seals.AddNew();
				AssertEquals("For NctsArrivalHeaderContainer", nctsHeader, seal.Header);
			});
		}

		public void TestBK_UnloadingState_ReadOnly_SentToCustoms()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var seal = header.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				header.ArrivalMovementHeader.BM_MessageStatus = "ACC";
				AssertEquals("Not sent to customs", false, seal.BK_UnloadingStateInfo.ReadOnly);
				header.ArrivalMovementHeader.BM_MessageStatus = "SNT";
				AssertEquals("Sent to customs", true, seal.BK_UnloadingStateInfo.ReadOnly);
			});
		}

		public void TestBK_SealNumber_ReadOnly_SentToCustoms()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var seal = header.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();

			CombineAssertions(() =>
			{
				header.ArrivalMovementHeader.BM_MessageStatus = "ACC";
				AssertEquals("Not sent to customs", false, seal.BK_SealNumberInfo.ReadOnly);
				header.ArrivalMovementHeader.BM_MessageStatus = "SNT";
				AssertEquals("Sent to customs", true, seal.BK_SealNumberInfo.ReadOnly);
			});
		}

		public void TestBK_SealNumber_NotReadOnly_Incident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var moveHeader = header.ArrivalMovementHeader;
			var seal = header.EnRouteIncidents.AddNew().IncidentContainers.AddNew().Seals.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("not NCTS5_Arrival_ART_FRC", false, seal.BK_SealNumberInfo.ReadOnly);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;
				AssertEquals("NCTS5_Arrival_ART_FRC", false, seal.BK_SealNumberInfo.ReadOnly);
				AssertEquals("Readonly decision made for appropriate parent", typeof(NctsContainer), seal.ParentType);
			});
		}

		public void TestBK_SealNumber_Caption()
		{
			var seal = Factory.New<CusSeal>();
			NCTSTestHelper.AssertCaptions(seal.BK_SealNumberInfo, "Seal Number", "Seal Number", "Seal No.");
		}

		public void TestBK_SealNumberReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			var seal = header.ArrivalHeaderContainers.AddNew().Seals.AddNew();
			seal.BK_SealNumber = "1111";

			CombineAssertions(() =>
			{
				AssertEquals("Unloaded state = NEW. SealNumber should be enabled.", false, seal.BK_SealNumberInfo.ReadOnly);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Unloaded state = DEC. Seal Number should be disabled.", true, seal.BK_SealNumberInfo.ReadOnly);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("Unloaded state = MIS. Seal Number should be disabled.", true, seal.BK_SealNumberInfo.ReadOnly);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
				AssertEquals("Unloaded state = DAM. Seal Number should be disabled.", true, seal.BK_SealNumberInfo.ReadOnly);
				seal.BK_UnloadingState = ZString.Empty;
				AssertEquals("Unloaded state is empty. Seal Number should be disabled.", true, seal.BK_SealNumberInfo.ReadOnly);
			});

			var cusSeal = header.ArrivalHeaderContainers.AddNew().Seals.AddNew();
			cusSeal.BK_SealNumber = "1112";
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(header.ArrivalMovementHeader, x => ((CusSeal)x).BK_SealNumberInfo.ReadOnly, cusSeal);

			var headerForArrivalNotificationTest = Factory.New<NctsHeader>();
			headerForArrivalNotificationTest.SetMovementType(NctsMovementType.Codes.Arrival);
			headerForArrivalNotificationTest.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerForArrivalNotificationTest.BH_ExportFlag = EventFlagList.Codes.Yes;
			var cusSeal2 = headerForArrivalNotificationTest.EnRouteIncidents.AddNew().IncidentContainers.AddNew().Seals.AddNew();
			cusSeal2.BK_SealNumber = "2222";
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(cusSeal2.BK_SealNumberInfo, headerForArrivalNotificationTest);
		}

		public void TestBK_SequenceNumber_Caption()
		{
			var seal = Factory.New<CusSeal>();
			NCTSTestHelper.AssertCaptions(seal.BK_SequenceNumberInfo, "Sequence Number", "Sequence No.", "Seq.No.");
		}

		public void TestBK_SequenceNumber_ReadOnly()
		{
			var seal = Factory.New<CusSeal>();
			AssertEquals("Sequence Number should always be read only", true, seal.BK_SequenceNumberInfo.ReadOnly);
		}

		public void TestBK_UnloadingState_Captions()
		{
			var seal = Factory.New<CusSeal>();
			NCTSTestHelper.AssertCaptions(seal.BK_UnloadingStateInfo, "Unloaded State", "Unloaded State", "State");
		}

		public void TestBK_UnloadingState_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var seal = Factory.New<CusSeal>();
				AssertEquals("Unloaded State should be read only when Unloaded State = NEW", true, seal.BK_UnloadingStateInfo.ReadOnly);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Unloaded State should be enabled when Unloaded State = DEC", false, seal.BK_UnloadingStateInfo.ReadOnly);
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("Unloaded State should be enabled when Unloaded State = MIS", false, seal.BK_UnloadingStateInfo.ReadOnly);
				seal.BK_UnloadingState = ZString.Empty;
				AssertEquals("Unloaded State should be enabled when Unloaded State = empty", false, seal.BK_UnloadingStateInfo.ReadOnly);
			});
		}

		public void TestLookups()
		{
			var seal = Factory.New<CusSeal>();
			AssertType<CusSealLookups>(seal.Lookups);
		}

		public void TestValidation()
		{
			var headerPhase4 = Factory.New<NctsHeader>();
			headerPhase4.BH_ApplicationCode = ZString.Empty;

			var headerPhase5 = Factory.New<NctsHeader>();
			headerPhase5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			CombineAssertions(() =>
			{
				var seal = headerPhase4.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();
				AssertType<CusSealValidation>("If header IS NOT Phase5 then validation type is CusSealValidation", seal.Validation);
				var sealValidationAsPhase5 = seal.Validation as CusSealPhase5Validation;
				AssertNull("If header IS NOT Phase5 then validation type is not CusSealPhase5Validation", sealValidationAsPhase5);
				var seal2 = headerPhase5.DepartureHeaderContainers.AddNew().AdditionalSeals.AddNew();
				AssertType<CusSealPhase5Validation>("If header IS Phase5 then validation type is CusSealPhase5Validation", seal2.Validation);
			});
		}

		public void TestDefaultValuesNewSeal()
		{
			var seal = Factory.New<CusSeal>();
			AssertEquals("Unloading state should be 'NEW'", NctsUnloadedStateList.Codes.NEW, seal.BK_UnloadingState);
		}

		public void TestFKToHeader()
		{
			var container = Factory.New<NctsArrivalHeaderContainer>();
			var seal = container.Seals.AddNew() as ISequenceNumberLine;
			AssertEquals("FK should point to header-container", container.PK, seal.FKToHeader);
		}

		public void TestParent()
		{
			var container = Factory.New<NctsDepartureHeaderContainer>();
			var seal = container.AdditionalSeals.AddNew();
			CombineAssertions(() =>
			{
				AssertType<NctsDepartureHeaderContainer>("Seal Parent is NctsHeaderContainer", seal.Parent);

				var nctsHeader = Factory.New<NctsHeader>();
				seal = nctsHeader.CusSeals.AddNew();
				AssertType<NctsHeader>("Seal Parent is NctsHeader", seal.Parent);
			});
		}

		public void TestCanDelete()
		{
			var seal = getPhase5ArrivalSeal();

			CombineAssertions(() =>
			{
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Status is NEW", true, seal.CanDelete);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Status is DEC", false, seal.CanDelete);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("Status is MIS", false, seal.CanDelete);

				seal.BK_UnloadingState = ZString.Empty;
				AssertEquals("Status is empty", false, seal.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			var seal = getPhase5ArrivalSeal();
			AssertEquals("Cannot delete seals which were entered by customs", seal.ReasonForNotAbleToDelete);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var cusSeal = header.CusSeals.AddNew();
			cusSeal.BK_SealNumber = "SEAL1";
			return cusSeal;
		}

		CusSeal getPhase5ArrivalSeal()
		{
			var header = Factory.New<NctsHeader>();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container = header.ArrivalHeaderContainers.AddNew();
			var cusSeal = container.Seals.AddNew();
			cusSeal.BK_SealNumber = "SEAL1";
			return cusSeal;
		}
	}
}
