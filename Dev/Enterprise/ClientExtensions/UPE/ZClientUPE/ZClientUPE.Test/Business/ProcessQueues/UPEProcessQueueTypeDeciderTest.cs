using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPEProcessQueueTypeDeciderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals(typeof(ProcessQueue), Factory.New(typeof(ProcessQueue)).GetType());
		}

		public void TestLoad()
		{
			ProcessQueue queue = Factory.New<ProcessQueue>();
			queue.P4_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals(typeof(UPECargoReportQueue), new BusinessObjectFactory().Load(typeof(ProcessQueue), queue.PK).GetType());
			queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			AssertEquals(typeof(UPECalloutQueue), new BusinessObjectFactory().Load(typeof(ProcessQueue), queue.PK).GetType());
			queue.P4_Status = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			Factory.Save();
			AssertEquals(typeof(UPECargoReportQueue), new BusinessObjectFactory().Load(typeof(ProcessQueue), queue.PK).GetType());
			queue.P4_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals(typeof(UPEDeclarationQueue), new BusinessObjectFactory().Load(typeof(ProcessQueue), queue.PK).GetType());
			queue.P4_ParentTableCode = "$%";
			Factory.Save();
			AssertEquals(typeof(ProcessQueue), new BusinessObjectFactory().Load(typeof(ProcessQueue), queue.PK).GetType());
		}
	}
}
