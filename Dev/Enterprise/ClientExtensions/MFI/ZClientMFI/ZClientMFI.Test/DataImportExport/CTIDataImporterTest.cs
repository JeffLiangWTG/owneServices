using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.Data.Testing
{
	public class CTIDataImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, "PONL YARRA VALLEY"));
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = "PONL YARRA VALLEY";
				Factory.Save();
			}

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "5401";
			Factory.Save();
			CTIDataImporter importer = new CTIDataImporter();
			NotificationBuffer notify = new NotificationBuffer();
			AssertEquals("PreCondition: should be no matching consols in Database", 0, Factory.GetDatabaseCount(typeof(CommonConsol), Filter));

			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("Import.CaroTransTestFile.dat");
				using (StreamReader reader = new StreamReader(testFilePath))
				{
					importer.ImportData(reader, "CaroTransTestFile.Dat", notify, SourceInfo.EmptySourceInfo);
				}
				Assert("Must not have errors", !notify.HasErrors);
				AssertEquals("Should be 1 matching consol in Database", 1, Factory.GetDatabaseCount(typeof(CommonConsol), Filter));
			}
		}

		ZDBOnlyQuery Filter
		{
			get
			{
				if (fFilter == null)
				{
					ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), null);
					voyageQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, "PONL YARRA VALLEY");
					voyageQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, "5401");
					ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					PortMatcher matcher = new PortMatcher(Factory, "CSC", new NotificationBuffer(), GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					originQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, matcher.Result);
					originQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, new ZDateTime(2005, 1, 18));
					originQuery.AddSubQuery(JobVoyOriginSchema.JA_JV, JobVoyageSchema.PK, voyageQuery, JoinCondition.And);
					ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					matcher = new PortMatcher(Factory, "MEL", new NotificationBuffer(), GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, matcher.Result);
					destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, new ZDateTime(2005, 2, 16));
					destinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, JobVoyageSchema.PK, voyageQuery, JoinCondition.And);
					ZDBOnlySubQuery sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
					sailingQuery.AddSubQuery(originQuery, JoinCondition.And);
					sailingQuery.AddSubQuery(destinationQuery, JoinCondition.And);
					ZDBOnlySubQuery transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
					transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);
					fFilter = new ZDBOnlyQuery(typeof(CommonConsol));
					fFilter.AddToFilter(JobConsolSchema.JK_AgentsReference, "92637");
					fFilter.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);
				}

				return fFilter;
			}
		}

		ZDBOnlyQuery fFilter;
	}
}
