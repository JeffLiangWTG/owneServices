using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class CompanyLevelXMLTasksCreator
	{
		public CompanyLevelXMLTasksCreator(INotifications notify, BusinessObjectFactory factory)
		{
			Notify = notify;
			Factory = factory;
		}

		public IEnumerable<XMLTask> XMLTasks
		{
			get
			{
				if (exportTasks == null)
				{
					List<XMLTask> taskList = new List<XMLTask>();
					foreach (GlbCompany currentCompany in ActiveCompanies)
					{
						taskList.Add(CreateImportOrExportTask(currentCompany));
					}
					exportTasks = taskList.ToArray();
				}
				return exportTasks;
			}
		}
		IEnumerable<XMLTask> exportTasks;

		#region Implementation

		GlbCompanyCollection ActiveCompanies
		{
			get
			{
				if (companies == null)
				{
					ZQuery query = new ZQuery(GlbCompanySchema.GC_IsActive, true);
					companies = new GlbCompanyCollection(Factory, query);
				}
				return companies;
			}
		}
		GlbCompanyCollection companies;

		protected BusinessObjectFactory Factory;
		protected INotifications Notify;

		protected abstract IRegistryItem RegistryForImportOrExportTask { get; }
		protected abstract XMLTask CreateImportOrExportTask(GlbCompany company);

		#endregion
	}
}
