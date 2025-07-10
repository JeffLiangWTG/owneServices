using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class SupportIncidentFilterControlTest : TestCaseWithFactory
	{
		public void TestDatabaseProperties()
		{
			var buildDate = DateTime.Now.AddDays(-1);
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_ExeVersionDate = buildDate;
			build.HL_ReleaseStatus = "PRD";
			build.HL_Release = 1234;
			build.HL_Patch = 270;
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "EN2";
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ServerCode = "SC1";
			database.LD_HostedLocation = "SYD";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_HostServerName = "Host Server Name 1";
			database.LD_ReleaseRing = "RR1";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_LD = database.PK;
			Factory.Save();
			AssertEquals("SC1", incident.DatabaseServerCode);
			AssertEquals("Sydney", incident.LicenceDatabaseHostedLocation);
			AssertEquals("0.0.1234.270", incident.DatabaseCurrentVersion);
			AssertEquals("EN2", incident.Database.EnterpriseCode);
			AssertEquals("Host Server Name 1", incident.Database.LD_HostServerName);
			AssertEquals("RR1", incident.Database.LD_ReleaseRing);
			AssertEquals(buildDate.Date, incident.Database.CurrentVersionExeDate.Date);
			AssertEquals("Production 2003 May 19 patch 270", incident.Database.CurrentVersionRelease);
		}

		public void TestColumns()
		{
			using (var filterControl = new SupportIncidentFilterControl(new SupportIncidentCollection(Factory), new SupportIncidentFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "EstimateForBinding+CIE_EstimateSentDateLocal");
				AssertHasColumn(columns, "EstimateForBinding+CIE_EstimateExpiryDateLocal");
				AssertHasColumn(columns, "EstimateForBinding+CIE_QuoteRequestedLocal");
				AssertHasColumn(columns, "QuoteForBinding+CIQ_QuoteSentDateLocal");
				AssertHasColumn(columns, "QuoteForBinding+CIQ_QuoteExpiryDateLocal");
				AssertHasColumn(columns, "QuoteForBinding+CIQ_QuoteAcceptedDateLocal");
			}
		}

		public void TestTriageColumns()
		{
			using (var filterControl = new SupportIncidentFilterControl(new SupportIncidentCollection(Factory), new SupportIncidentFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "TriageNumber");
				AssertHasColumn(columns, "TriageSupportDescription");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
