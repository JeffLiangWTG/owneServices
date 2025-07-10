using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation;

[CodeAlive("Used in CargoWise.Main.Navigation.Winzor project")]
public class MyTasksViewModel : ViewModelBase, IMyTasksViewModel
{
	static string NoTasksText => ResString.GetMultilingualString("9e821a4a-bc7a-4d31-b254-ef4906f2ee01", "No tasks.");
	static string IdText => ResString.GetMultilingualString("91bdaa60-9038-43ef-8270-a1b13ea458e1", "ID #");
	static string NameText => ResString.GetMultilingualString("62e2dd35-f708-4277-9ad6-a3e41fcb47ad", "Name");
	static string TaskDescriptionText => ResString.GetMultilingualString("da0fbea2-ddad-4d5f-aa1c-79b3099262a6", "Task Description");
	static string StatusText => ResString.GetMultilingualString("0d52c931-2217-43bf-8066-52dee1e41f1f", "Status");
	static string SectionDisplayName => Res.GetString("A696C0FC-46F5-404A-B148-7099177C5ECB", "Current Tasks");
	public string NoMyTasksText
	{
		get
		{
			return NoTasksText;
		}
	}
	public string IdHeaderText
	{
		get
		{
			return IdText;
		}
	}
	public string NameHeaderText
	{
		get
		{
			return NameText;
		}
	}
	public string TaskDescriptionHeaderText
	{
		get
		{
			return TaskDescriptionText;
		}
	}
	public string StatusHeaderText
	{
		get
		{
			return StatusText;
		}
	}
	public string DisplayName
	{
		get
		{
			return SectionDisplayName;
		}
	}

	ObservableCollection<IMyTasksItemViewModel> myTasks;
	public ObservableCollection<IMyTasksItemViewModel> MyTasks
	{
		get => myTasks;
		set
		{
			myTasks = value;
			OnPropertyChanged();
		}
	}
	internal Action OpenTaskModuleForm { get; set; }
	public MyTasksViewModel()
	{
		OpenTaskFormCommand = new ClickCommand(() =>
		{
			OpenTaskModuleForm?.Invoke();
		});
	}

	public ICommand OpenTaskFormCommand { get; }

	public void Refresh()
	{
		LoadMyTasks();
	}

	BusinessObjectFactory factory;
	internal void LoadMyTasks()
	{
		factory = new BusinessObjectFactory { NameForDebugging = nameof(MyTasksViewModel) };
		var resultantMyTasks = new List<IMyTasksItemViewModel>();
		var myTasksItems = GetMyTasks(factory);
		foreach (var myTaskItem in myTasksItems)
		{
			resultantMyTasks.Add(new MyTasksItemViewModel(myTaskItem));
		}
		MyTasks = new ObservableCollection<IMyTasksItemViewModel>(resultantMyTasks);
	}
	ProcessTask[] GetMyTasks(BusinessObjectFactory factory, int tasksQueryLimit = 25)
	{
		var query = new ZDBOnlyQuery(typeof(ProcessTask));

		var staffFilterQuery = new ZQuery();
		staffFilterQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, GlbStaff.CurrentUser.GS_Code);
		query.AddToFilter(staffFilterQuery);

#pragma warning disable CW1161 // Res.GetString Analyzer
		string startableTaskFilterQuery = @" P9_PK IN (
				SELECT tii.P9_PK 
					FROM GetCurrentTasksInWorkflowsIgnoringIterations() tii 
				WHERE
					tii.P9_PK = P9_PK 
					AND tii.P9_GS_NKAssignedStaffMember = @AssignedStaffMember
					AND NOT EXISTS (
						SELECT TOP 1 ph.FH_PK
							FROM ProcessHeader ph
						WHERE
							ph.FH_PK = tii.P9_FH_ProcessHeader
							AND ph.FH_DoNotStartBeforeDate > @DoNotStartBeforeDate)
				UNION
				SELECT tii.P9_PK
					FROM GetCurrentTasksNotInWorkflows() tii
				WHERE
					tii.P9_PK = P9_PK
					AND tii.P9_GS_NKAssignedStaffMember = @AssignedStaffMember
					AND NOT EXISTS (
						SELECT TOP 1 ph.FH_PK
							FROM ProcessHeader ph, ProcessTasks pt
						WHERE
							ph.FH_ParentId = pt.p9_ParentId
							AND tii.P9_PK = pt.P9_PK
							AND fh_category = 'JOB'
							AND ph.FH_DoNotStartBeforeDate > @DoNotStartBeforeDate)
			)";
#pragma warning restore CW1161 // Res.GetString Analyzer
		var doNotStartBeforeDate = ZSqlParameter.New("@DoNotStartBeforeDate", ZDateTime.Now, ProcessHeaderSchema.FH_DoNotStartBeforeDate);
		var assignedStaffMember = ZSqlParameter.New("@AssignedStaffMember", GlbStaff.CurrentUser.GS_Code, ProcessTasksSchema.P9_GS_NKAssignedStaffMember);
		var parameterCollection = new ZSqlParameterCollection(doNotStartBeforeDate, assignedStaffMember);
		query.AddFilterAndZSQLParameterCollection(startableTaskFilterQuery, parameterCollection);

		var statusFilterQuery = new ZQuery();
		statusFilterQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_Status, new[] { "WRK", "SUS", "ASN" });
		query.AddToFilter(statusFilterQuery);

		query.OrderBy = $"{ProcessTasksSchema.P9_SystemLastEditTimeUtc.Name} desc";
		query.MaximumRows = tasksQueryLimit;
		var tasks1 = factory.Load<ProcessTask>(query);
		return tasks1;
	}
}
