using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	internal abstract class CompanyLevelXMLTasksCreatorTest : TestCaseWithFactory
	{
		public void TestXMLTasks()
		{
			int numOfTasks = ((IList<XMLTask>)XMLTasksCreator.XMLTasks).Count;
			AssertEquals("no of xml tasks created should equal to no. of active companies in the system", numOfTasks, Companies.Count);
		}

		protected abstract CompanyLevelXMLTasksCreator XMLTasksCreator { get; }

		GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.GC_IsActive, true))); }
		}
		GlbCompanyCollection companies;

		protected NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer()); }
		}
		NotificationBuffer buffer;
	}
}
