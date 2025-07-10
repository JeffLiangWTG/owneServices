using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class CreateWorkItemResult
	{
		public string StaffEmailAddress;
		public IRotationWorkItem CreatedWorkItem;
		public CreateWorkItemStatusCode StatusCode;
		public WorkItemDTO WorkItemDTO;
	}
	public interface IRotationWorkItem
	{
		WorkItem WorkItem { get; }
		string HumanReadableName { get; }
	}
	public class RotationWorkItem : IRotationWorkItem
	{
		readonly WorkItem _workItem;
		public RotationWorkItem(WorkItem workItem)
		{
			_workItem = workItem;
		}
		public WorkItem WorkItem => _workItem;
		public string HumanReadableName => WorkItem.HumanReadableName.ToString();
	}

	public enum CreateWorkItemStatusCode
	{
		Success,
		StaffNotFound,
		RotationManagerNotFound,
		InvalidStartDate,
		InvalidRotationManager,
		InvalidWeeksPerRotation,
		InvalidWeeksInCoreSkillsTraining,
		InvalidLocation,
		WorkItemPreviouslyCreated
	}

	public class WorkItemDTO : INotifyPropertyChanged
	{
		public WorkItemDTO(string summary, string type, string area, string activityType, string activitySubType, string priority, string staffCode = "", string locationCode = "")
		{
			StaffCode = staffCode;
			LocationCode = locationCode;
			Summary = summary;
			Type = type;
			Area = area;
			ActivityType = activityType;
			ActivitySubType = activitySubType;
			Priority = priority;
			StartDate = new DateOnly(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		const int maxCodeLength = 3;

		string staffCode;
		public string StaffCode
		{
			get
			{
				return staffCode;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				staffCode = value;
				OnPropertyChanged(nameof(StaffCode));
			}
		}

		string locationCode;
		public string LocationCode
		{
			get => locationCode;
			set
			{
				locationCode = value;
				OnPropertyChanged(nameof(LocationCode));
			}
		}

		string summary;
		public string Summary
		{
			get => summary;
			set
			{
				summary = value;
				OnPropertyChanged(nameof(Summary));
			}
		}

		string type;
		public string Type
		{
			get
			{
				return type;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				type = value;
				OnPropertyChanged(nameof(Type));
			}
		}
		string area;
		public string Area
		{
			get
			{
				return area;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				area = value;
				OnPropertyChanged(nameof(Area));
			}
		}
		string activityType;
		public string ActivityType
		{
			get
			{
				return activityType;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				activityType = value;
				OnPropertyChanged(nameof(ActivityType));
			}
		}
		string activitySubType;
		public string ActivitySubType
		{
			get
			{
				return activitySubType;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				activitySubType = value;
				OnPropertyChanged(nameof(ActivitySubType));
			}
		}
		string priority;
		public string Priority
		{
			get
			{
				return priority;
			}
			set
			{
				if (value.Length > maxCodeLength)
				{
					return;
				}
				priority = value;
				OnPropertyChanged(nameof(Priority));
			}
		}

		bool workItemCreated;

		/// <summary>
		/// Once set to true, cannot be reset.
		/// </summary>
		public bool WorkItemCreated
		{
			get
			{
				return workItemCreated;
			}
			set
			{
				if (value)
				{
					workItemCreated = true;
				}
			}
		}
		public DateOnly startDate;
		public DateOnly StartDate
		{
			get
			{
				return startDate;
			}
			set
			{
				startDate = value;
				OnPropertyChanged(nameof(StartDate));
			}
		}

		// rotation item custom fields
		public string Rotation1Team { get; set; } = string.Empty;
		public string RotationManager { get; set; } = string.Empty;
		public int WeeksPerRotation { get; set; }
		public int WeeksInCoreSkillsTraining { get; set; }
		public bool IsCoreSkillsTraining { get; set; } = true;
		public bool IsDeveloper { get; set; } = true;
	}
}
