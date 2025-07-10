using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Data
{
	#region DataUpgrader Factories

	public class DataUpgraderFactory
	{
		public BaseUpgrader New(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade)
		{
			return new DataUpgrader(manager, upgConnection, versionBeforeUpgrade);
		}
	}

	#endregion

	public class DataUpgrader : BaseUpgrader
	{
		public DataUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			fUpgradeTaskList = new List<IUpgradeTask>();
			InitialiseUpgradeTasks();
		}

		/// <summary>
		/// Number of Tasks + Start + DataVersion Update + Completion
		/// </summary>
		public override int EstimatedNumberOfTasks
		{
			get { return 6; }
		}

		protected override void DoUpgrade()
		{
			RunDataUpgradeTasks();
			UpdateDataVersionOnDatabase();
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return System.Array.Empty<string>(); }
		}

		public override string Name
		{
			get { return "System Data Update"; } // Database upgrade message
		}

		/// <summary>
		/// Expose fUpgradeTaskList as the Generator can have a list of task to have the XML file updated.
		/// </summary>
		public IUpgradeTask[] UpgradeTaskList
		{
			get { return fUpgradeTaskList.ToArray(); }
		}

		protected override VersionLabel LatestVersion
		{
			get { return DataVersion.Application; }
		}

		#region Implementation

		void RunDataUpgradeTasks()
		{
			for (int i = 0; i < fUpgradeTaskList.Count; i++)
			{
				// This will allow the task to be garbage collected when Task falls out of scope
				var task = fUpgradeTaskList[i];
				fUpgradeTaskList[i] = null;

				if (TaskIsRequired(task))
				{
					StartNonEstimatedTask(task.TaskNameWhenUpgrading);
					task.Run();
				}
			}
		}

		protected readonly List<IUpgradeTask> fUpgradeTaskList;

		protected virtual void InitialiseUpgradeTasks()
		{
			fUpgradeTaskList.Add(new GlbDepartmentUpgradeTask());
			fUpgradeTaskList.Add(new LocalCartageUpgradeTask());
			fUpgradeTaskList.Add(new DocumentsUpgradeTask(new DocumentsCompleteDataFile()));
			fUpgradeTaskList.Add(new RefCountryUpgradeTask());
			fUpgradeTaskList.Add(new RefCityPCodeUpgradeTask());
			fUpgradeTaskList.Add(new RefLatLongPostcodeUpgradeTask());
			fUpgradeTaskList.Add(new RefAirlineUpgradeTask());
			fUpgradeTaskList.Add(new RefPackTypeUpgradeTask());
			fUpgradeTaskList.Add(new RefPacksUpgradeTask());
			fUpgradeTaskList.Add(new StmEventUpgradeTask());
			fUpgradeTaskList.Add(new StmSystemDefinedFieldUpgradeTask());
			fUpgradeTaskList.Add(new ProcessTaskTemplateUpgradeTask());
			fUpgradeTaskList.Add(new StmModuleFilterUpgradeTask());
			fUpgradeTaskList.Add(new TagRuleUpgradeTask());
			fUpgradeTaskList.Add(new TransportBookingTemplateUpgradeTask());
			fUpgradeTaskList.Add(new BarcodeRuleUpgradeTask());
			fUpgradeTaskList.Add(new InventoryHeldCodeUpgradeTask());
			fUpgradeTaskList.Add(new OrgSalesProductUpgradeTask());
			fUpgradeTaskList.Add(new WhsSystemLocationTypeUpgradeTask());
			fUpgradeTaskList.Add(new RefWRSZoneUpgradeTask());
			fUpgradeTaskList.Add(new RefLocalLanguageUpgradeTask());
			fUpgradeTaskList.Add(new ProductionRuleSetUpgradeTask());
			fUpgradeTaskList.Add(new ExceptionTypesUpgradeTask());
			fUpgradeTaskList.Add(new ExternalRequestTypeUpgradeTask());
			fUpgradeTaskList.Add(new AccAssetDepreciationBookUpgradeTask());

			// Base Data Tasks
			fUpgradeTaskList.Add(new BaseData.RefTables.RefTablesUpgradeTask());
			fUpgradeTaskList.Add(new BaseData.System.StmNumberSequenceUpgradeTask());
			fUpgradeTaskList.AddRange(GetSystemInstallTasksIfNewInstallation());
		}

		IEnumerable<IUpgradeTask> GetSystemInstallTasksIfNewInstallation()
		{
			if (versionBeforeUpgrade.CompareTo(new VersionLabel(0, 0)) == 0)
			{
				yield return new BaseData.Organisation.OrganisationUpgradeTask();
				yield return new BaseData.Company.CompanyUpgradeTask();
				yield return new BaseData.RefZone.RefZoneUpgradeTask();
				yield return new BaseData.Accounting.AccountingUpgradeTask();
				yield return new BaseData.System.SystemUpgradeTask();
				yield return new BaseData.StmData.StmDataUpgradeTask();
			}
			else
			{
				yield break;
			}
		}

		protected virtual bool TaskIsRequired(IUpgradeTask task)
		{
			return task.IsRequired;
		}

		/// <summary>
		/// Updates the Data Version on the Database
		/// </summary>
		protected virtual void UpdateDataVersionOnDatabase()
		{
			StartTask("Updating system data version");  // Task name, not client visible
			DbRegistry.DatabaseSystemDataVersionMajor.SaveValue(DataVersion.Application.Major, upgConnection);
			DbRegistry.DatabaseSystemDataVersionMinor.SaveValue(DataVersion.Application.Minor, upgConnection);
		}

		#endregion
	}
}
