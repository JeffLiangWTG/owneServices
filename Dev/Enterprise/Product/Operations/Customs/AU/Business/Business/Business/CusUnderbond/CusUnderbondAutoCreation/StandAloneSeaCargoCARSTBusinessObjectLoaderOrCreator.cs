//using System;
//
//using Enterprise.ZArchitecture.Environment;
//using Enterprise.ZArchitecture;
//using Enterprise.ZArchitecture.Business;
//using Enterprise.ZArchitecture.Schema;
//using Enterprise.MasterFiles.Business;
//using ICusUnderbondDependentCollectionParent = Enterprise.ICusUnderbondDependentCollectionParent;
//using Enterprise.Customs.AU.SeaCargo.Business;
//
//#if DEBUG
//using Enterprise.ZArchitecture.Business.Testing;
//using Enterprise.Customs.Business.Testing;
//#endif
//
//namespace Enterprise.Customs.AU.Declaration.Business
//{
//	public class StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator : CARSTBusinessObjectLoaderOrCreator
//	{
//		protected override bool IsInterestedInCARST(CMRCARSTMessage Message)
//		{
//			return Message.IsSea && (StatusAdviceRelatedToDepot(Message) || SeaCargoDepotRecordExists(Message));
//		}
//
//		protected bool SeaCargoDepotRecordExists(CMRCARSTMessage Message)
//		{
//			bool Result = false;
//			if (!Message.ContainerNumber.IsEmpty)
//			{
//				ZQuery Filter = new ZQuery(CusSCAContainerSchema.CN_ContainerNumber, Message.ContainerNumber);
//				Result = Message.Factory.LoadTop1<CusSCAContainer>(Filter) != null;
//			}
//			if (!Result && !Message.OceanBillNumber.IsEmpty)
//			{
//				ZQuery Filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, Message.OceanBillNumber);
//				Result = Message.Factory.LoadTop1<CusSCAOceanBill>(Filter) != null;
//			}
//			return Result;
//		}
//
//		protected override BusinessObject LoadOrCreateRecordForMessageCore(CMRCARSTMessage Message)
//		{
//			CusSCARecordLoaderAndCreator Creator = new CusSCARecordLoaderAndCreator(Message.Factory);
//			BusinessObject Result = Creator.LoadOrCreateSCARecord(
//				Message.LloydsNumber,
//				Message.VoyageNumber,
//				Message.OceanBillNumber,
//				Message.HouseBillNumber,
//				Message.ContainerNumber,
//				Message.ContainerMode,
//				Message.NumberOfPackages,
//				Message.PackageType) as BusinessObject;
//			if (Result is CusSCAPivot) Result = (Result as CusSCAPivot).HouseBill;
//			if (Result is CusSCAContainer) Result = (Result as CusSCAContainer).OceanBill;
//			return Result;
//		}
//
//		#region TestCase
//
//#if DEBUG
//
//		class StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreatorTest : TestCaseWithFactory
//		{
//			public void TestIsInterestedInCARST()
//			{
//				GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
//				GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
//				
//				AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(HouseBillCMRCARSTMessage));
//				AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(HAWBCMRCARSTMessage));
//			}
//
//			public void TestLoadOrCreateRecordForMessageCore()
//			{
//				AssertEquals("Created Object Type", typeof(CMRCusSCAHouse), Creator.LoadOrCreateRecordForMessageCore(HouseBillCMRCARSTMessage).GetType());
//			}
//
//			public void TestReturnOceanBillForOceanBillStatus()
//			{
//				BusinessObject Result = Creator.LoadOrCreateRecordForMessageCore(OceanBillCMRCARSTMessage);
//				AssertEquals("Result.GetType()", typeof(CMRCusSCAOceanBill), Result.GetType());
//			}
//
//			#region Implementation
//
//			StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator fCreator;
//			StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator Creator
//			{
//				get
//				{
//					if (fCreator == null)
//					{
//						fCreator = new StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator();
//					}
//					return fCreator;
//				}
//			}
//
//
//			CMRCARSTMessage fHouseBillCMRCARSTMessage;
//			CMRCARSTMessage HouseBillCMRCARSTMessage 
//			{
//				get
//				{
//					if (fHouseBillCMRCARSTMessage == null)
//					{
//						fHouseBillCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
//						fHouseBillCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
//BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
//DTM+9:20050726162613590090:ZZZ'
//FTX+AHN+++CONSOLIDATED STATUS:HELD'
//FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
//FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
//FTX+AHN+++IAR ACS CLEARED:YES'
//FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
//FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
//FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
//FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
//FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
//FTX+AHN+++IAR AQIS CLEARED:YES'
//FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
//FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
//FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
//FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
//FTX+AHN+++ACS EVALUATION COMPLETE:YES'
//FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
//FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
//FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
//FTX+AHN+++IMPORT DECLARATION PAID:N/A'
//FTX+AHN+++CARGO REPORT SAC:NO'
//TDT+20+4365++11++++8811924::11'
//LOC+12+AUSYD::6'
//LOC+4+9914N::95'
//NAD+MR+AAA374M::95'
//NAD+UD+41065894724::95'
//RFF+MB:OB0987123'
//RFF+BH:HB4000'
//RFF+AAQ:C001'
//DOC+1'
//PAC+++LCL:67:95'
//UNT+34+000001'".Replace("\r\n", "");
//					}
//					return fHouseBillCMRCARSTMessage;
//				}
//			}
//
//			CMRCARSTMessage fOceanBillCMRCARSTMessage;
//			CMRCARSTMessage OceanBillCMRCARSTMessage 
//			{
//				get
//				{
//					if (fOceanBillCMRCARSTMessage == null)
//					{
//						fOceanBillCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
//						fOceanBillCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
//BGM+34:::CARST+1AIG 0299 58B5:1+8'
//DTM+9:20050831082857529209:ZZZ'
//FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
//FTX+AHN+++CARGO REPORT SAC:NO'
//TDT+20+936++11++++8811924::11'
//LOC+12+AUSYD::6'
//NAD+MR+AAA374M::95'
//NAD+UD+41065894724::95'
//RFF+ABO:H00000023/SYD1::1'
//RFF+MB:OBL250805006'
//RFF+AAQ:TXCU9304939'
//DOC+1'
//PAC+++FCL:67:95'
//UNT+15+000001'".Replace("\r\n", "");
//					}
//					return fOceanBillCMRCARSTMessage;
//				}
//			}
//
//			CMRCARSTMessage fHAWBCMRCARSTMessage;
//			CMRCARSTMessage HAWBCMRCARSTMessage 
//			{
//				get
//				{
//					if (fHAWBCMRCARSTMessage == null)
//					{
//						fHAWBCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
//						fHAWBCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
//BGM+34:::CARST+5AFE B52J 755:1+8'
//DTM+9:20050725203721536492:ZZZ'
//DTM+132:20050404:102'
//FTX+AHN+++CONSOLIDATED STATUS:HELD'
//FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
//FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
//FTX+AHN+++IAR ACS CLEARED:YES'
//FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
//FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
//FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
//FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
//FTX+AHN+++IAR AQIS CLEARED:YES'
//FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
//FTX+AHN+++IMPORT DECLARATIONS MATCHED:NO'
//FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:NO'
//FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
//FTX+AHN+++ACS EVALUATION COMPLETE:YES'
//FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
//FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:NO'
//FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
//FTX+AHN+++IMPORT DECLARATION PAID:N/A'
//TDT+20+300++6+QF::3'
//LOC+12+AUSYD::6'
//NAD+MR+AAA374M::95'
//RFF+MWB:08130038433'
//UNT+27+000001'".Replace("\r\n", "");
//					}
//					return fHAWBCMRCARSTMessage;
//				}
//			}
//
//			#endregion
//		}
//
//		public class TestStandAloneSeaCargoDepotCARSTMessageInterest : Testing.MessageFactoryInterestTest
//		{
//			public override void TestIsInterested()
//			{
//				GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
//				GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
//				StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator Creator = new StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator();
//				AssertEquals("Depot HAWB Interest", false, Creator.IsInterestedInCARST(HAWBCargoStatusMessage));
//				AssertEquals("Depot SEA Interest", true, Creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_TRCU3382910));
//			}
//
//			public void TestIsInterestedNotDepot()
//			{
//				GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
//				GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
//				StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator Creator = new StandAloneSeaCargoCARSTBusinessObjectLoaderOrCreator();
//				AssertEquals("CTO HAWB Interest", false, Creator.IsInterestedInCARST(HAWBCargoStatusMessage));
//				AssertEquals("CTO SEA Interest", false, Creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_TRCU3382910));
//			}
//		}
//		
//
//#endif
//
//		#endregion
//	}
//}
