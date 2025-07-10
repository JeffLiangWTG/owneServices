using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderFilterStrip : ZFilterStrip
	{
		#region ZFilterStrip Overrides

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var taskStatusFilter = currentModuleFilter as TaskStatusFilter;
			if (taskStatusFilter != null)
			{
				result = GetTaskStatusFilterControls(taskStatusFilter).ToArray();
				SetPreferredHeight(result);
			}
			else if (currentModuleFilter is TaskAssignedFilter)
			{
				result = GetTaskAssignedFilterControls().ToArray();
			}
			else if (currentModuleFilter is ModuleDurationFilter)
			{
				result = GetModuleDurationFilterControls().ToArray();
			}
			else if (currentModuleFilter is OpenTaskEstimateRangeFilter)
			{
				result = GetOpenTaskEstimateRangeControls().ToArray();
			}
			else if (currentModuleFilter is JobTextPropertyFilter)
			{
				result = GetJobTextPropertyFilterControls().ToArray();
			}
			else if (currentModuleFilter is JobDetailFilter)
			{
				result = GetJobDetailFilterControls().ToArray();
			}
			else if (currentModuleFilter is WorkflowCategoryFilter)
			{
				result = GetWorkflowCategoryFilterControls().ToArray();
			}
			else if (currentModuleFilter is LeadTimeFilter)
			{
				result = GetLeadTimeFilterControls().ToArray();
				SetPreferredHeight(result);
			}
			else if (currentModuleFilter is DeadlineTypeFilter)
			{
				result = GetDeadlineTypeFilterControls().ToArray();
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		protected override bool IsZPropertyInfoType(PropertyInfo info)
		{
			return info.PropertyType == typeof(ZPropertyInfo) || info.PropertyType == typeof(ZWrappedPropertyInfo);
		}

		#endregion

		#region Job Property Controls

		IEnumerable<Control> GetJobTextPropertyFilterControls()
		{
			const int PropertyNameMaxLength = 16;

			var comparisonOperatorDropList = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
			comparisonOperatorDropList.TabIndex = 0;
			yield return comparisonOperatorDropList;

			var workflowTypeDropEdit = new ZDropEdit();
			workflowTypeDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref workflowTypeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref workflowTypeDropEdit, FilterControlsBox1Start, true);
			workflowTypeDropEdit.ShowDescriptionBox = false;
			workflowTypeDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			workflowTypeDropEdit.PreBoundMaxLength = ProcessTaskTemplateSchema.P0_ProcessType.MaxLength;
			workflowTypeDropEdit.TabIndex = 1;
			workflowTypeDropEdit.BindTo = JobTextPropertyFilter.Schema.WorkflowTypeCode;
			workflowTypeDropEdit.BindToList = "List";
			FilterControlBindingSource.SetBindingMember(workflowTypeDropEdit, workflowTypeDropEdit.BindTo);

			yield return workflowTypeDropEdit;

			var nameDropEdit = new ZDropEdit();
			nameDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref nameDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref nameDropEdit, workflowTypeDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			nameDropEdit.ShowDescriptionBox = false;
			nameDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			nameDropEdit.PreBoundMaxLength = PropertyNameMaxLength;
			nameDropEdit.TabIndex = 2;
			nameDropEdit.BindTo = JobTextPropertyFilter.Schema.JobPropertyName;
			nameDropEdit.BindToList = JobTextPropertyFilter.Schema.JobPropertyNameList;
			FilterControlBindingSource.SetBindingMember(nameDropEdit, nameDropEdit.BindTo);

			yield return nameDropEdit;

			var textBox = new ZTextBox();
			ControlDpiScalingHelper.SetTop(ref textBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref textBox, nameDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(ref textBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start + FilterControlBoxWidth) - textBox.Left, false);
			textBox.TabIndex = 3;
			textBox.BindTo = "Property";
			FilterControlBindingSource.SetBindingMember(textBox, "Property");

			yield return textBox;
		}

		#endregion

		#region Job Description

		IEnumerable<Control> GetJobDetailFilterControls()
		{
			var comparisonOperatorDropList = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
			comparisonOperatorDropList.TabIndex = 0;
			yield return comparisonOperatorDropList;

			var workflowTypeDropEdit = new ZDropEdit();
			workflowTypeDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref workflowTypeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref workflowTypeDropEdit, FilterControlsBox1Start, true);
			workflowTypeDropEdit.ShowDescriptionBox = false;
			workflowTypeDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			workflowTypeDropEdit.PreBoundMaxLength = ProcessTaskTemplateSchema.P0_ProcessType.MaxLength;
			workflowTypeDropEdit.TabIndex = 1;
			workflowTypeDropEdit.BindTo = JobDescriptionFilter.Schema.WorkflowTypeCode;
			workflowTypeDropEdit.BindToList = "List";
			FilterControlBindingSource.SetBindingMember(workflowTypeDropEdit, workflowTypeDropEdit.BindTo);

			yield return workflowTypeDropEdit;

			var textBox = new ZTextBox();
			ControlDpiScalingHelper.SetTop(ref textBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref textBox, workflowTypeDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(ref textBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start + FilterControlBoxWidth) - textBox.Left, false);
			textBox.TabIndex = 3;
			textBox.BindTo = "Property";
			FilterControlBindingSource.SetBindingMember(textBox, "Property");

			yield return textBox;
		}

		#endregion

		#region Workflow Category Controls

		IEnumerable<Control> GetWorkflowCategoryFilterControls()
		{
			var workflowTypeDropEdit = new ZDropEdit();
			workflowTypeDropEdit.CaptionResourceString = Res.GetData("a1747cdc-6be7-4e79-b446-15332f60538d", "Workflow Type");
			workflowTypeDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref workflowTypeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref workflowTypeDropEdit, FilterControlsBox1Start, true);
			workflowTypeDropEdit.ShowDescriptionBox = false;
			workflowTypeDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			workflowTypeDropEdit.PreBoundMaxLength = ProcessTaskTemplateSchema.P0_ProcessType.MaxLength;
			workflowTypeDropEdit.TabIndex = 0;
			workflowTypeDropEdit.BindTo = WorkflowCategoryFilter.Schema.WorkflowType;
			workflowTypeDropEdit.BindToList = "List";
			FilterControlBindingSource.SetBindingMember(workflowTypeDropEdit, workflowTypeDropEdit.BindTo);

			yield return workflowTypeDropEdit;

			var categoryDropEdit = new ZDropEdit();
			categoryDropEdit.CaptionResourceString = Res.GetData("52108f7d-7827-4f8b-b438-90279999e54d", "Category");
			categoryDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref categoryDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref categoryDropEdit, workflowTypeDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls + 60), false);
			categoryDropEdit.ShowDescriptionBox = false;
			categoryDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			categoryDropEdit.PreBoundMaxLength = ProcessTaskTemplateSchema.P0_ProcessType.MaxLength;
			categoryDropEdit.TabIndex = 1;
			categoryDropEdit.BindTo = WorkflowCategoryFilter.Schema.WorkflowCategory;
			categoryDropEdit.BindToList = WorkflowCategoryFilter.Schema.WorkflowCategoryList;
			FilterControlBindingSource.SetBindingMember(categoryDropEdit, categoryDropEdit.BindTo);

			yield return categoryDropEdit;
		}

		#endregion

		#region Task Status Controls

		IEnumerable<Control> GetTaskStatusFilterControls(TaskStatusFilter moduleFilter)
		{
			const int PropertyNameMaxLength = 4;

			var statusAggregateDropEdit = new ZDropEdit();
			statusAggregateDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref statusAggregateDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref statusAggregateDropEdit, this.FilterDescriptionDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(ref statusAggregateDropEdit, 50, true);
			statusAggregateDropEdit.ShowDescriptionBox = false;
			statusAggregateDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			statusAggregateDropEdit.PreBoundMaxLength = PropertyNameMaxLength;
			statusAggregateDropEdit.TabIndex = 1;
			statusAggregateDropEdit.BindTo = TaskStatusFilter.Schema.TaskAggregator;
			statusAggregateDropEdit.BindToList = TaskStatusFilter.Schema.TaskAggregatorList;
			FilterControlBindingSource.SetBindingMember(statusAggregateDropEdit, statusAggregateDropEdit.BindTo);

			yield return statusAggregateDropEdit;

			var taskStatusCheckBox = new ZCheckedListBox();
			taskStatusCheckBox.BindingItems = null;
			taskStatusCheckBox.BindTo = TaskStatusFilter.Schema.TaskStatusCheckList;
			FilterControlBindingSource.SetBindingMember(taskStatusCheckBox, taskStatusCheckBox.BindTo);
			taskStatusCheckBox.FormattingEnabled = true;
			taskStatusCheckBox.Dock = DockStyle.Fill;

			var statusGroupBox = new ZGroupBox();
			statusGroupBox.Text = Res.GetString("c0669615-8b74-42a7-bf41-ce29933c2a22", "Status");
			ControlDpiScalingHelper.SetTop(ref statusGroupBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref statusGroupBox, statusAggregateDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(ref statusGroupBox, 80, true);
			ControlDpiScalingHelper.SetHeight(ref statusGroupBox, 100, true);
			statusGroupBox.TabIndex = 2;
			statusGroupBox.Controls.Add(taskStatusCheckBox);

			yield return statusGroupBox;

			var taskTypeCheckBox = new ZCheckedListBox();
			taskTypeCheckBox.BindingItems = null;
			taskTypeCheckBox.BindTo = TaskStatusFilter.Schema.TaskTypeCheckList;
			FilterControlBindingSource.SetBindingMember(taskTypeCheckBox, taskTypeCheckBox.BindTo);
			taskTypeCheckBox.Dock = DockStyle.Fill;
			taskTypeCheckBox.FormattingEnabled = true;
			taskTypeCheckBox.TabIndex = 2;

			var typeGroupBox = new ZGroupBox();
			typeGroupBox.Text = Res.GetString("53ebb6ed-71a4-4a7f-98b9-bb0ec116a215", "Type");
			ControlDpiScalingHelper.SetWidth(ref typeGroupBox, 220, true);
			ControlDpiScalingHelper.SetTop(ref typeGroupBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref typeGroupBox, statusGroupBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			typeGroupBox.TabIndex = 3;
			typeGroupBox.Controls.Add(taskTypeCheckBox);

			moduleFilter.TaskStatusCheckList.OnPairChanged += OnPairChanged;
			taskStatusCheckBox.Disposed += (s, e) => { moduleFilter.TaskStatusCheckList.OnPairChanged -= OnPairChanged; };

			moduleFilter.TaskTypeCheckList.OnPairChanged += OnPairChanged;
			taskTypeCheckBox.Disposed += (s, e) => { moduleFilter.TaskTypeCheckList.OnPairChanged -= OnPairChanged; };

			yield return typeGroupBox;
		}

		void OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			OnFilterEdited();
		}

		#endregion

		#region Task Assigned Controls

		IEnumerable<Control> GetTaskAssignedFilterControls()
		{
			const int PropertyNameMaxLength = 4;

			var assignmentAggregateDropEdit = new ZDropEdit();
			assignmentAggregateDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(assignmentAggregateDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(assignmentAggregateDropEdit, this.FilterDescriptionDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(assignmentAggregateDropEdit, 50, true);
			assignmentAggregateDropEdit.ShowDescriptionBox = false;
			assignmentAggregateDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			assignmentAggregateDropEdit.PreBoundMaxLength = PropertyNameMaxLength;
			assignmentAggregateDropEdit.TabIndex = 1;
			assignmentAggregateDropEdit.BindTo = TaskAssignedFilter.Schema.TaskOrdinality;
			assignmentAggregateDropEdit.BindToList = TaskAssignedFilter.Schema.TaskOrdinalityList;
			FilterControlBindingSource.SetBindingMember(assignmentAggregateDropEdit, assignmentAggregateDropEdit.BindTo);

			yield return assignmentAggregateDropEdit;

			var statusAggregateDropEdit = new ZDropEdit();
			statusAggregateDropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(statusAggregateDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(statusAggregateDropEdit, assignmentAggregateDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls), false);
			ControlDpiScalingHelper.SetWidth(statusAggregateDropEdit, 50, true);
			statusAggregateDropEdit.ShowDescriptionBox = false;
			statusAggregateDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			statusAggregateDropEdit.PreBoundMaxLength = PropertyNameMaxLength;
			statusAggregateDropEdit.TabIndex = 2;
			statusAggregateDropEdit.BindTo = TaskAssignedFilter.Schema.TaskAssignmentType;
			statusAggregateDropEdit.BindToList = TaskAssignedFilter.Schema.TaskAssignmentTypeList;
			FilterControlBindingSource.SetBindingMember(statusAggregateDropEdit, statusAggregateDropEdit.BindTo);

			yield return statusAggregateDropEdit;
		}

		#endregion

		#region Open Task Estimate Range Controls

		IEnumerable<Control> GetOpenTaskEstimateRangeControls()
		{
			var control = new OpenTaskEstimateRangeFilterControl();
			ControlDpiScalingHelper.SetTop(ref control, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(control, FilterDescriptionDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
			FilterControlBindingSource.SetBindingMember(control, ".");

			yield return control;
		}

		#endregion

		#region Lead Time Controls

		IEnumerable<Control> GetLeadTimeFilterControls()
		{
			var leadTimeDropEdit = new ZDropEdit
			{
				CharacterCasing = CharacterCasing.Normal,
				ShowDescriptionBox = false,
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				PreBoundMaxLength = 3,
				TabIndex = 1,
				BindTo = LeadTimeFilter.Schema.LeadTimeRangeCode,
				BindToList = LeadTimeFilter.Schema.LeadTimeRangeCodeList,
				CaptionResourceString = Res.GetData("LeadTimeFilter.LeadTimeRangeCode", "Range", "Whether to find results which are within or outside the time leading up to the Agreed Delivery Date, considering completion within Zone 2 of the specified buffer."),
			};

			ControlDpiScalingHelper.SetTop(leadTimeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(leadTimeDropEdit, this.FilterDescriptionDropEdit.Right + GetScaledLeftPosition(leadTimeDropEdit, null), false);
			ControlDpiScalingHelper.SetWidth(leadTimeDropEdit, 50, true);

			FilterControlBindingSource.SetBindingMember(leadTimeDropEdit, leadTimeDropEdit.BindTo);

			yield return leadTimeDropEdit;

			var bufferFindBox = new ZGuidFindBox
			{
				ShowDescriptionBox = false,
				TabIndex = 2,
				BindTo = LeadTimeFilter.Schema.BufferPK,
				BindToList = LeadTimeFilter.Schema.BuffersList,
				CaptionResourceString = Res.GetData("LeadTimeFilter.BufferPK", "Buffer", "The buffer to consider when calculating the profitable lead time."),
			};

			ControlDpiScalingHelper.SetTop(bufferFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(bufferFindBox, GetScaledLeftPosition(bufferFindBox, leadTimeDropEdit), false);
			ControlDpiScalingHelper.SetWidth(bufferFindBox, 80, true);

			FilterControlBindingSource.SetBindingMember(bufferFindBox, bufferFindBox.BindTo);

			yield return bufferFindBox;

			var estimateFactorCalcEdit = new ZCalcEdit
			{
				Decimals = 2,
				TabIndex = 3,
				BindTo = LeadTimeFilter.Schema.LeadTimeEstimateFactor,
				CaptionResourceString = Res.GetData("LeadTimeFilter.LeadTimeEstimateFactor", "Estimate Multiple", "Planned Duration is multiplied by this number when added to profitable lead time."),
			};

			ControlDpiScalingHelper.SetTop(estimateFactorCalcEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(estimateFactorCalcEdit, GetScaledLeftPosition(estimateFactorCalcEdit, bufferFindBox), false);
			ControlDpiScalingHelper.SetWidth(estimateFactorCalcEdit, 40, true);

			FilterControlBindingSource.SetBindingMember(estimateFactorCalcEdit, estimateFactorCalcEdit.BindTo);

			yield return estimateFactorCalcEdit;
		}

		#endregion

		#region DeadlineType Controls

		IEnumerable<Control> GetDeadlineTypeFilterControls()
		{
			var deadlineTypeDropEdit = new ZDropEdit
			{
				CharacterCasing = CharacterCasing.Normal,
				ShowDescriptionBox = false,
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				PreBoundMaxLength = 3,
				TabIndex = 0,
				BindTo = DeadlineTypeFilter.Schema.DeadlineType,
			};
			ControlDpiScalingHelper.SetTop(ref deadlineTypeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref deadlineTypeDropEdit, FilterControlsBox1Start, true);
			deadlineTypeDropEdit.BindToList = "List";
			FilterControlBindingSource.SetBindingMember(deadlineTypeDropEdit, deadlineTypeDropEdit.BindTo);

			yield return deadlineTypeDropEdit;

			var isEffectiveCheckBox = new ZCheckBox
			{
				Text = Res.GetString("27b11068-09d0-4eea-b1ec-83cba76651c3", "Effective"),
				TabIndex = 1,
				BindTo = DeadlineTypeFilter.Schema.IsEffective,
				Checked = true,
			};
			ControlDpiScalingHelper.SetWidth(ref isEffectiveCheckBox, isEffectiveCheckBox.PreferredSize.Width, false);
			ControlDpiScalingHelper.SetHeight(ref isEffectiveCheckBox, isEffectiveCheckBox.PreferredSize.Height, false);
			ControlDpiScalingHelper.SetTop(ref isEffectiveCheckBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref isEffectiveCheckBox, deadlineTypeDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls + 15), false);
			FilterControlBindingSource.SetBindingMember(isEffectiveCheckBox, isEffectiveCheckBox.BindTo);

			yield return isEffectiveCheckBox;

			var isImmediateCheckBox = new ZCheckBox
			{
				Text = Res.GetString("b56dc9c7-8a0b-4dd8-8453-99391e0588be", "Immediate"),
				TabIndex = 2,
				BindTo = DeadlineTypeFilter.Schema.IsImmediate,
				Checked = false,
			};
			ControlDpiScalingHelper.SetWidth(ref isImmediateCheckBox, isImmediateCheckBox.PreferredSize.Width, false);
			ControlDpiScalingHelper.SetHeight(ref isImmediateCheckBox, isImmediateCheckBox.PreferredSize.Height, false);
			ControlDpiScalingHelper.SetTop(ref isImmediateCheckBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref isImmediateCheckBox, isEffectiveCheckBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls + 5), false);
			FilterControlBindingSource.SetBindingMember(isImmediateCheckBox, isImmediateCheckBox.BindTo);

			yield return isImmediateCheckBox;
		}

		#endregion

		#region Module Duration Controls
		IEnumerable<Control> GetModuleDurationFilterControls()
		{
			var control = new ModuleDurationFilterControl();
			ControlDpiScalingHelper.SetTop(ref control, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(control, FilterDescriptionDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
			FilterControlBindingSource.SetBindingMember(control, ".");

			yield return control;
		}
		#endregion

		#region Implementation

		void SetPreferredHeight(IEnumerable<Control> controls)
		{
			PreferredHeight = controls.MaxBySafe(c => c.Height).Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
		}

		static int GetScaledLeftPosition<T>(T newControl, Control previousControl)
			where T : Control, IResCaptionedControl
		{
			var result = ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenControls + TextRenderer.MeasureText(newControl.CaptionResourceString.Caption + ": ", newControl.Font).Width);

			if (previousControl != null)
			{
				result += previousControl.Right;
			}

			return result;
		}

		const int SpaceBetweenControls = 3;

		#endregion
	}
}
