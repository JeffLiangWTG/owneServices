using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEProcessQueueLogCollection))]
	internal class UPEProcessQueueLogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewLogEventTimeBasedonProcessQueueParentBranchTimeZone()
		{
			GlbBranch melBranch = Factory.New<GlbBranch>();
			melBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melBranch.GB_Code = "TML";
			melBranch.GB_RL_NKHomePort = "AUMEL";

			GlbBranch perBranch = Factory.New<GlbBranch>();
			perBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perBranch.GB_Code = "TPR";
			perBranch.GB_RL_NKHomePort = "AUPER";

			Factory.Save();

			ZDateTime melbourneTime;
			ProcessQueueLog newlyAddedLog;

			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_GB = perBranch.PK;
			var decProcessQueue = Factory.New<UPEDeclarationQueue>();
			decProcessQueue.Parent = jobDec;
			var collection = new UPEProcessQueueLogCollection(decProcessQueue, ProcessQueueType.Enum.Customs);

			using (DisposableEnvironment.ForBranch(melBranch.GB_Code))
			{
				melbourneTime = ZDateTime.Now;
				newlyAddedLog = collection.AddNew("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, "", "", "");
			}

			AssertEquals("Collection contains the newlyAddLog", 1, collection.Count);
			AssertNotNull(newlyAddedLog);
			AssertGreaterThan("Log 's event time should be based on declaration's branch time zone", melbourneTime, newlyAddedLog.SL_EventTime);

			var cusHawb = Factory.New<CusHAWB>();
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_GB = perBranch.PK;
			cusHawb.CS_CM = cusMawb.PK;

			var cargoProcessQueue = Factory.New<UPECargoReportQueue>();
			cargoProcessQueue.Parent = cusHawb;
			collection = new UPEProcessQueueLogCollection(cargoProcessQueue, ProcessQueueType.Enum.Commercial);

			using (DisposableEnvironment.ForBranch(melBranch.GB_Code))
			{
				melbourneTime = ZDateTime.Now;
				newlyAddedLog = collection.AddNew("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, "", "", "");
			}

			AssertEquals("Collection contains the newlyAddLog", 1, collection.Count);
			AssertNotNull(newlyAddedLog);
			AssertGreaterThan("Log 's event time should be based on cusmawb's branch time zone", melbourneTime, newlyAddedLog.SL_EventTime);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UPEProcessQueueLogCollection(UPEProcessQueue, ProcessQueueType.Enum.Customs);
		}

		UPEProcessQueue UPEProcessQueue
		{
			get
			{
				return upeProcessQueue ?? (upeProcessQueue = Factory.NewWithValidTestData<UPEDeclarationQueue>());
			}
		}
		UPEProcessQueue upeProcessQueue;
	}
}
