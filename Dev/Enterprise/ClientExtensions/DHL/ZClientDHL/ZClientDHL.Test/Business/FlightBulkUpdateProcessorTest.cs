using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using CusBiz = Enterprise.Customs.Business;

namespace Enterprise.Client.DHL.Business.Testing
{
	public class FlightBulkUpdateProcessorTest : TestCaseWithFactory
	{
		#region TestBulkUpdate
		public void TestBulkUpdate_SingleDeclaration()
		{
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("No updates", 0, processor.BulkUpdate());
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			AssertEquals("AssertStandaloneDeclarations not created yet.", false, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			AddVariousCustomDeclarations();
			AssertEquals("Standalone Declarations Created", true, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			BulkUpdateBizO.MAWB = "081-13245675";
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 6, processor.BulkUpdate());
			AssertEquals("Original Standalone Declarations Changed", false, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		public void TestBulkUpdate_MultipleDeclarations()
		{
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("No updates", 0, processor.BulkUpdate());
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			AssertEquals("AssertStandaloneDeclarations not created yet.", false, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			AddValidCustomDeclarationsInBulk(20);
			AssertEquals("Standalone Declarations Created", true, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			BulkUpdateBizO.MAWB = "081-13245675";
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 20, processor.BulkUpdate());
			AssertEquals("Original Standalone Declarations Changed", false, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		public void TestBulkUpdate_PartialUpdateWithConcurrencyException()
		{
			var processor = new FlightBulkUpdateProcessorForTest(BulkUpdateBizO);
			AssertEquals("No updates", 0, processor.BulkUpdate());
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			AssertEquals("AssertStandaloneDeclarations not created yet.", false, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			AddValidCustomDeclarationsInBulk(82);
			AssertEquals("Standalone Declarations Created", true, ExistingJobDecs(BulkUpdateBizO.ExistingMAWB));
			BulkUpdateBizO.MAWB = "081-13245675";
			BulkUpdateBizO.EDITransmitDate = new ZDateTime(2014, 8, 29);
			processor = new FlightBulkUpdateProcessorForTest(BulkUpdateBizO);
			int committedCount = processor.BulkUpdate();
			AssertEquals(50, committedCount);
			AssertEquals("1st committed batch with new MAWB", 50, Factory.Load<JobDeclaration>(BulkUpdateBizO.StandaloneJobDecFilter(BulkUpdateBizO.MAWB)).Length);
			AssertEquals("2nd rolled back Counter with old MAWB", 32, Factory.Load<JobDeclaration>(BulkUpdateBizO.StandaloneJobDecFilter(BulkUpdateBizO.ExistingMAWB)).Length);
			AssertContains(@"While you were editing your data, another user", processor.ConcurrencyErrorMessage);
			AssertContains(@"modified it.
Your changes cannot be saved because they may conflict with the other user's changes.
Please close and open this form to try again.

50 out of 82 records have been updated.", processor.ConcurrencyErrorMessage);
		}

		class FlightBulkUpdateProcessorForTest : FlightBulkUpdateProcessor
		{
			public FlightBulkUpdateProcessorForTest(FlightBulkUpdateBusinessObject flightBulkUpdateBizO) : base(flightBulkUpdateBizO)
			{
			}

			int batchCount;
			protected override void SaveCurrentAndCreateNew(JobDeclarationBusinessObjectFactoryProvider factoryProvider)
			{
				batchCount++;
				if (batchCount == 2)
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)factoryProvider.Current.New<DummyBusinessObject>()).Row, null), factoryProvider.Current);
				}

				base.SaveCurrentAndCreateNew(factoryProvider);
			}
		}

		#endregion
		#region TestOnlyUpdatedChangedProperties
		public void TestOnlyMAWBUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.MAWB = "081-13245675";
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertOnlyMAWBUpdated(jobDecs(BulkUpdateBizO.MAWB));
		}

		void AssertOnlyMAWBUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08113245675");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "QF253");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate == new ZDateTime(2005, 12, 12));
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival == new ZDateTime(2005, 12, 13));
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.Date);
		}

		public void TestOnlyFlightNumberUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.FlightNo = "FLIGHTNO";
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertOnlyFlightNumberUpdated(jobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		void AssertOnlyFlightNumberUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08111111111");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "FLIGHTNO");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate == new ZDateTime(2005, 12, 12));
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival == new ZDateTime(2005, 12, 13));
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.Date);
		}

		public void TestOnlyATDUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.DepartureDate = ZDateTime.Now;
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertOnlyATDUpdated(jobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		void AssertOnlyATDUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08111111111");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "QF253");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate.Date == ZDateTime.Now.Date);
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival == new ZDateTime(2005, 12, 13));
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.Date);
		}

		public void TestOnlyATAUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.ArrivalDate = ZDateTime.Now.AddDays(1);
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertOnlyATAUpdated(jobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		void AssertOnlyATAUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08111111111");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "QF253");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate == new ZDateTime(2005, 12, 12));
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival.Date == ZDateTime.Now.AddDays(1).Date);
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.Date);
		}

		public void TestOnlyTransmitDateUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.EDITransmitDate = ZDateTime.Now.AddDays(2);
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertOnlyTransmitDateUpdated(jobDecs(BulkUpdateBizO.ExistingMAWB));
		}

		void AssertOnlyTransmitDateUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08111111111");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "QF253");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate == new ZDateTime(2005, 12, 12));
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival == new ZDateTime(2005, 12, 13));
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.AddDays(2).Date);
		}

		public void TestAllUpdated()
		{
			AddOneValidCustomDeclaration();
			Factory.Save();
			BulkUpdateBizO.ExistingMAWB = "081-11111111";
			BulkUpdateBizO.MAWB = "081-13245675";
			BulkUpdateBizO.FlightNo = "FLIGHTNO";
			BulkUpdateBizO.DepartureDate = ZDateTime.Now;
			BulkUpdateBizO.ArrivalDate = ZDateTime.Now.AddDays(1);
			BulkUpdateBizO.EDITransmitDate = ZDateTime.Now.AddDays(2);
			processor = new FlightBulkUpdateProcessor(BulkUpdateBizO);
			AssertEquals("Counter", 1, processor.BulkUpdate());
			AssertAllUpdated(jobDecs(BulkUpdateBizO.MAWB));
		}

		void AssertAllUpdated(JobDeclarationCollection jobDecs)
		{
			Assert("JE_MasterBill", jobDecs[0].JE_MasterBill == "08113245675");
			Assert("JE_VoyageFlightNo", jobDecs[0].JE_VoyageFlightNo == "FLIGHTNO");
			Assert("JE_ExportDate", jobDecs[0].JE_ExportDate.Date == ZDateTime.Now.Date);
			Assert("JE_DateOfArrival", jobDecs[0].JE_DateOfArrival.Date == ZDateTime.Now.AddDays(1).Date);
			Assert("JE_EDITransmitDate", jobDecs[0].JE_EDITransmitDate.Date == ZDateTime.Now.AddDays(2).Date);
		}

		#endregion
		bool ExistingJobDecs(ZString mawb)
		{
			JobDeclaration jobDec = Factory.LoadTop1<JobDeclaration>(BulkUpdateBizO.StandaloneJobDecFilter(mawb));
			return jobDec != null;
		}

		void AddOneValidCustomDeclaration()
		{
			AddCustomDeclaration(Constants.TransportModes.Air, LowValueConsignmentStatusList.Codes.NotSentToCustoms, JobMessageTypeList.Codes.Import, true, GlbBranch.CurrentBranch.PK);
		}

		void AddVariousCustomDeclarations()
		{
			foreach (string transportMode in new string[] { Constants.TransportModes.Air, Constants.TransportModes.Sea })
			{
				foreach (string eciConsignmentStatus in new string[] { LowValueConsignmentStatusList.Codes.NotSentToCustoms, LowValueConsignmentStatusList.Codes.SentToCustoms, LowValueConsignmentStatusList.Codes.ReadyForManifesting, LowValueConsignmentStatusList.Codes.ManifestedReadyToSend })
				{
					foreach (string jobMessageType in new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export })
					{
						AddCustomDeclaration(transportMode, eciConsignmentStatus, jobMessageType, true, GlbBranch.CurrentBranch.PK);
						AddCustomDeclaration(transportMode, eciConsignmentStatus, jobMessageType, true, ZGuid.Empty);
						AddCustomDeclaration(transportMode, eciConsignmentStatus, jobMessageType, false, ZGuid.Empty);
					}
				}
			}

			Factory.Save();
		}

		void AddValidCustomDeclarationsInBulk(int max)
		{
			for (int idx = 0; idx < max; idx++)
			{
				AddCustomDeclaration(Constants.TransportModes.Air, LowValueConsignmentStatusList.Codes.NotSentToCustoms, JobMessageTypeList.Codes.Import, true, GlbBranch.CurrentBranch.PK);
			}

			Factory.Save();
		}

		void AddCustomDeclaration(string transportMode, string entryStatus, string messageType, bool standalone, ZGuid branchPK)
		{
			JobDeclaration jobDec = JobDec;
			var voyageFlightNo = jobDec.JE_VoyageFlightNo;
			jobDec.JE_TransportMode = transportMode;
			jobDec.JE_VoyageFlightNo = voyageFlightNo;
			jobDec.JE_EntryStatus = entryStatus;
			jobDec.JE_MessageType = messageType;
			if (branchPK.IsEmpty)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				company.Branches.Add(branch);
				jobDec.JE_GB = branch.PK;
			}
			else
			{
				jobDec.JE_GB = branchPK;
			}

			string x = jobDec.JE_MasterBill;
			if (!standalone)
			{
				jobDec.JE_JS = TestShipment.PK;
			}
		}

		JobDeclarationCollection jobDecs(ZString mawb)
		{
			var jobDecs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			jobDecs.Load(BulkUpdateBizO.StandaloneJobDecFilter(mawb));
			return jobDecs;
		}

		JobDeclaration JobDec
		{
			get
			{
				JobDeclaration jobDec = Factory.NewWithValidTestData<JobDeclaration>();
				jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				jobDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				jobDec.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
				jobDec.JE_MasterBill = "08111111111";
				jobDec.JE_TransportMode = Constants.TransportModes.Air;
				jobDec.JE_VoyageFlightNo = "QF253";
				jobDec.JE_RL_NKPortOfLoading = "USLAX";
				jobDec.JE_RL_NKPortOfArrival = "NZAKL";
				jobDec.JE_ExportDate = new ZDateTime(2005, 12, 12);
				jobDec.JE_DateOfArrival = new ZDateTime(2005, 12, 13);
				jobDec.JE_EDITransmitDate = ZDateTime.Now;
				CusBiz.BaseCusContainer container = jobDec.CusContainers.AddNew();
				var bill = jobDec.Bills.AddNew();
				CusBiz.BasePackingGroup packingGroup = bill.PackingGroups.AddNew();
				packingGroup.CR_CO_Container = container.PK;
				CusBiz.BasePackage package = packingGroup.Packages.AddNew();
				return jobDec;
			}
		}

		ForwardingShipment TestShipment
		{
			get
			{
				return testShipment ?? (testShipment = Factory.NewWithValidTestData<ForwardingShipment>());
			}
		}

		ForwardingShipment testShipment;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DHL");
		}

		FlightBulkUpdateBusinessObject BulkUpdateBizO
		{
			get
			{
				return bulkUpdateBizO ?? (bulkUpdateBizO = new FlightBulkUpdateBusinessObject());
			}
		}

		FlightBulkUpdateBusinessObject bulkUpdateBizO;
		FlightBulkUpdateProcessor processor;
	}
}
