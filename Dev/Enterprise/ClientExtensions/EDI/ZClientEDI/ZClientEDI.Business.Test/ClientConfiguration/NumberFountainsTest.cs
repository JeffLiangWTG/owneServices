using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Modules.Testing
{
	[UseSnapshotProtection(skipTransaction: true)]
	public class NumberFountainsTest : TransactionedTestCase
	{
		public void TestFountain()
		{
			AssertNotNull(Fountains.NewWorkItemNo);
			AssertNotNull(Fountains.ProfessionalServicesQuoteNo);
			AssertNotNull(Fountains.IncidentManagementGroupNumber);
			AssertNotNull(Fountains.IncidentTriageNumber);
			AssertNotNull(Fountains.IncidentTriageChecklistItemNumber);
			AssertNotNull(Fountains.IssueNo);
			AssertNotNull(Fountains.TrainingCourseBookingNo);
			AssertNotNull(Fountains.LicenceDatabaseNumber);
			AssertNotNull(Fountains.LicenceCompanyNumber);
			AssertNotNull(Fountains.EnterpriseID);
			AssertNotNull(Fountains.TrustedSystemNumber);

			Fountains.LicenceDatabaseNumber.SetNext(Db.Connection, 100);
			Fountains.LicenceCompanyNumber.SetNext(Db.Connection, 1000);
			AssertEquals("LicenceDatabaseNumber.GetNextInt()", 100L, Fountains.LicenceDatabaseNumber.GetNext(Db.Connection));
			AssertEquals("LicenceCompanyNumber.GetNextInt()", 1000L, Fountains.LicenceCompanyNumber.GetNext(Db.Connection));

			Fountains.EnterpriseID.SetNext(Db.Connection, 1);
			AssertEquals("E000001", Fountains.EnterpriseID.GetNextFormatted(Db.Connection));
			Fountains.EnterpriseID.SetNext(Db.Connection, 999999999);
			AssertEquals("E999999999", Fountains.EnterpriseID.GetNextFormatted(Db.Connection));
			Fountains.EnterpriseID.SetNext(Db.Connection, 1);

			Fountains.TrustedSystemNumber.SetNext(Db.Connection, 1);
			AssertEquals("ETS000001", Fountains.TrustedSystemNumber.GetNextFormatted(Db.Connection));
			Fountains.TrustedSystemNumber.SetNext(Db.Connection, 999999999);
			AssertEquals("ETS999999999", Fountains.TrustedSystemNumber.GetNextFormatted(Db.Connection));
			Fountains.TrustedSystemNumber.SetNext(Db.Connection, 1);
		}

		public void TestFountainNumbersStartsWithTheirPrefix()
		{
			Fountains.ProfessionalServicesQuoteNo.SetNext(Db.Connection, 1);
			AssertEquals("ProfessionalServicesQuoteNo.GetNext()", "PSQ00000001", Fountains.ProfessionalServicesQuoteNo.GetNextFormatted(Db.Connection));

			Fountains.IncidentManagementGroupNumber.SetNext(Db.Connection, 1);
			AssertEquals("IncidentManagementGroupNumber.GetNext()", "ING00000001", Fountains.IncidentManagementGroupNumber.GetNextFormatted(Db.Connection));

			Fountains.IncidentTriageNumber.SetNext(Db.Connection, 1);
			AssertEquals("IncidentTriageNumber.GetNext()", "TRI00000001", Fountains.IncidentTriageNumber.GetNextFormatted(Db.Connection));

			Fountains.IncidentTriageChecklistItemNumber.SetNext(Db.Connection, 1);
			AssertEquals("IncidentTriageChecklistItemNumber.GetNext()", "TRC00000001", Fountains.IncidentTriageChecklistItemNumber.GetNextFormatted(Db.Connection));

			Fountains.InvestigationItemNumber.SetNext(Db.Connection, 1);
			AssertEquals("InvestigationItemNumber.GetNext()", "INV00000001", Fountains.InvestigationItemNumber.GetNextFormatted(Db.Connection));

			Fountains.NewWorkItemNo.SetNext(Db.Connection, 1);
			AssertEquals("NewWorkItemNo.GetNext()", "WI00000001", Fountains.NewWorkItemNo.GetNextFormatted(Db.Connection));

			Env.NumberFountains.CustomerServiceIncidentNo.SetNext(Db.Connection, 1);
			AssertEquals("CustomerServiceIncidentNo.GetNext()", "CS00000001", Env.NumberFountains.CustomerServiceIncidentNo.GetNextFormatted(Db.Connection));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CargoWise.Data.Db.Connection.BeginTransaction();
			Fountains = ClientNumberFountainRegistration.GetInstance();
		}

		protected override void TearDown()
		{
			CargoWise.Data.Db.Connection.RollbackTransaction();
			base.TearDown();
		}

		ClientNumberFountainRegistration Fountains;

		#endregion
	}
}
