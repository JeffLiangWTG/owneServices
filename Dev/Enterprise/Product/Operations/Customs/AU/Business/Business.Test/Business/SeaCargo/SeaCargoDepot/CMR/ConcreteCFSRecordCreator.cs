using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ConcreteCFSRecordCreator : CFSRecordCreator
	{
		public ConcreteCFSRecordCreator(DepotCusOutturn outturn, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
		}

		sealed class ConcreteCFSRecordCreatorTest : CFSRecordCreatorTest
		{
			public void TestFindExistingConsolFromMasterBill()
			{
				CusOutturnHeader header = CreateOutturnHeader();
				CommonConsol consol = CreateNonCFSConsol(header, TestOceanBill1);
				CusUnderbond underbond = Factory.New<CusUnderbond>();
				underbond.C4_C6 = header.PK;
				DepotCusOutturn outturn = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

				ConcreteCFSRecordCreator creator = new ConcreteCFSRecordCreator(outturn, Factory);
				AssertNull("Existing Consol is not CFS Registered", creator.FindExistingConsol(outturn));

				CreateCFSConsol(header, TestOceanBill1);
				AssertNotNull("Existing Consol is CFS Registered", creator.FindExistingConsol(outturn));
			}

			public void TestFindExistingConsolFromContainer()
			{
				CusOutturnHeader header = CreateOutturnHeader();
				CommonConsol consol = CreateNonCFSConsol(header, TestOceanBill1);
				CommonContainer container = consol.Containers.AddNew();
				container.JC_ContainerNum = TestContainerNumber1;
				CusUnderbond underbond = Factory.New<CusUnderbond>();
				underbond.C4_C6 = header.PK;
				DepotCusOutturn outturn = AddContainerLine(header, underbond, TestContainerNumber1);

				ConcreteCFSRecordCreator creator = new ConcreteCFSRecordCreator(outturn, Factory);
				AssertNull("Existing Consol is not CFS Registered", creator.FindExistingConsol(outturn));

				container.JC_IsCFSRegistered = true;
				consol.JK_IsCFS = true;

				AssertNotNull("Existing Cntainer is CFS Registered", creator.FindExistingConsol(outturn));
			}

			CommonConsol CreateNonCFSConsol(CusOutturnHeader header, ZString oceanBillNumber)
			{
				return CreateForwardingConsol(header, oceanBillNumber, false);
			}

			CommonConsol CreateCFSConsol(CusOutturnHeader header, ZString oceanBillNumber)
			{
				return CreateForwardingConsol(header, oceanBillNumber, true);
			}

			CommonConsol CreateForwardingConsol(CusOutturnHeader header, ZString oceanBillNumber, ZBool isCFS)
			{
				CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

				Transport transport = consol.Transports[0];
				transport.JW_Vessel = VesselFromLloyds(header.C6_LloydsIMO);
				transport.JW_VoyageFlight = header.C6_VoyageNum;
				consol.JK_MasterBillNum = oceanBillNumber;

				JobVoyage voyage = Factory.New<JobVoyage>();
				VoyageOrigin origin = voyage.Origins.AddNew();

				VoyageDestination destination = voyage.Destinations.AddNew();
				if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.OrgProxy.OH_RL_NKClosestPort;
				}
				voyage.JV_RV_NKVessel = VesselFromLloyds(header.C6_LloydsIMO);
				voyage.JV_VoyageFlight = header.C6_VoyageNum;
				JobSailing sailing = voyage.Sailings.AddNew();
				sailing.JX_JB = destination.PK;
				sailing.JX_JA = origin.PK;

				transport.JW_JX = sailing.PK;
				consol.JK_IsCFS = isCFS;

				return consol;
			}
		}
	}
}
