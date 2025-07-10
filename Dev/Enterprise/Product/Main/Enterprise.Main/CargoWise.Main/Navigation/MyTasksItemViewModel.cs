using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Navigation;

public class MyTasksItemViewModel : ViewModelBase, IMyTasksItemViewModel
{
	static string TaskNotFoundErrorMessage => ResString.GetMultilingualString("28947EE0-9F17-4E4F-D49F-6372E7DAE126", "Task not found, may be deleted. Please refresh my tasks list.");
	static string TaskNotFoundCaption => ResString.GetMultilingualString("3595328E-9E04-BAAF-7831-5C144F430C3F", "Task not found");

#if DEBUG
	public bool IsTaskNotFound;
#endif
	public MyTasksItemViewModel(ProcessTask taskItem)
	{
		this.ParentId = taskItem.ParentCode;
		this.ParentSummary = taskItem.ParentDescription;
		this.Status = taskItem.P9_Status;
		this.Description = taskItem.LongDescription;
		this.LinkAction = new ClickCommand(() =>
		{
			var task = GetProcessTask(taskItem.PK);

			if (task == null)
			{
				using var notification = new ZMessageBox(TaskNotFoundErrorMessage, TaskNotFoundCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				ZFormModaliser.ShowDialogAndDispose(notification);

#if DEBUG
				IsTaskNotFound = true;
#endif 
				return;
			}

			using var filterModule = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks, null);
			filterModule.ModuleDecisionProvider.HandleDefaultAction(new BusinessObject[] { task });
		});
	}

	public string ParentId { get; }
	public string ParentSummary { get; }
	public string Status { get; }
	public string Description { get; }

	public string StatusText => Status switch
	{
		"WRK" => (string)ResString.GetMultilingualString("Main.MyTasks.WRK", "Working"),
		"SUS" => (string)ResString.GetMultilingualString("Main.MyTasks.SUS", "Suspended"),
		_ => Status,
	};

	public ClickCommand LinkAction { get; set; }

	BusinessObjectFactory factory;
	BusinessObjectFactory Factory
	{
		get
		{
			return factory ??= new BusinessObjectFactory { NameForDebugging = nameof(MyTasksItemViewModel) };
		}
	}
	protected virtual ProcessTask GetProcessTask(ZGuid pk)
	{
		var query = new ZDBOnlyQuery(typeof(ProcessTask));
		query.AddToFilter(ProcessTasksSchema.PK, pk);
		var result = Factory.LoadTop1<ProcessTask>(query);
		return result;
	}

	public bool AreBothParentIDandSummarySet => !ParentId.IsNullOrEmpty() && !ParentSummary.IsNullOrEmpty();
}
