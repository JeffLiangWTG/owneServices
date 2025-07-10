using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.GSSi.Testing
{
	class GSSiManagerTest : TestCaseWithFactory
	{
		public void TestSendMessageCreatesMessage_CusHAWB()
		{
			UPEBranchIDsRegistryObjectCollection branchIDColl = new UPEBranchIDsRegistryObjectCollection();
			UPEBranchIDsRegistryObject branchID = branchIDColl.AddNew();
			branchID.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			branchID.BuildingID = "AUAUSYD";
			UPEDataRegistry.Instance.BranchCodeIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branchIDColl);
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			cusHAWB.CS_RL_NKDestination = "AUSYD";
			TestSendMessageCreatesMessage(cusHAWB, cusHAWB.Destination);
		}

		public void TestSendMessageCreatesMessage_Declaration()
		{
			UPEBranchIDsRegistryObjectCollection branchIDColl = new UPEBranchIDsRegistryObjectCollection();
			UPEBranchIDsRegistryObject branchID = branchIDColl.AddNew();
			branchID.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			branchID.BuildingID = "AUAUSYD";
			UPEDataRegistry.Instance.BranchCodeIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branchIDColl);
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_RL_NKDestination = "AUSYD";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			cusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			TestSendMessageCreatesMessage(declaration, cusHAWB.Destination);
		}

		void TestSendMessageCreatesMessage(EnterpriseBusinessObject parent, RefUNLOCO relatedPort)
		{
			IProcessQueueParent queueParent = ((IProcessQueueParent)parent);
			UPEProcessQueueLog log = Factory.New<UPEProcessQueueLog>();
			log.Master = queueParent.CurrentQueue;
			log.SetQueueDetails("CUS", "X2", "", "", "");
			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));
			GSSiManager.Instance.CreateHoldMessage("1111", parent, log, relatedPort);
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull(ediMessage);
			var exportLogQuery = CreateExportLogFilter(parent.PK, "03", "X2", "1111");
			AssertNotNull(Factory.LoadTop1<StmALog>(exportLogQuery));
			ediMessage.DeleteFromTest();
			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));
			log = Factory.New<UPEProcessQueueLog>();
			log.Master = queueParent.CurrentQueue;
			log.SetQueueDetails("BLA", "SR", "", "", "");
			GSSiManager.Instance.CreateHoldMessage("1111", parent, log, relatedPort);
			ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull(ediMessage);
			exportLogQuery = CreateExportLogFilter(parent.PK, "03", "SR", "1111");
			AssertNotNull(Factory.LoadTop1<StmALog>(exportLogQuery));
			ediMessage.DeleteFromTest();
			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));
			log = Factory.New<UPEProcessQueueLog>();
			log.Master = queueParent.CurrentQueue;
			log.SetQueueDetails("BLA", "DA", "", "", "");
			Factory.Save();
			GSSiManager.Instance.CreateResolutionMessage(parent, Factory, "1111", log, relatedPort);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("3 EdiMessages are created", 3, ediMessages.Length);
			exportLogQuery = CreateExportLogFilter(parent.PK, "04", "X2", "1111");
			AssertNotNull(Factory.LoadTop1<StmALog>(exportLogQuery));
			exportLogQuery = CreateExportLogFilter(parent.PK, "04", "SR", "1111");
			AssertNotNull(Factory.LoadTop1<StmALog>(exportLogQuery));
			exportLogQuery = CreateExportLogFilter(parent.PK, "03", "SR", "1111");
			AssertNotNull(Factory.LoadTop1<StmALog>(exportLogQuery));
		}

		ZQuery CreateExportLogFilter(ZGuid parentPK, ZString shipmentStatus, ZString reasonCode, ZString trackingID)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, trackingID + "|" + shipmentStatus + "|" + reasonCode);
			return filter;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
