using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public sealed class SecurityFilterField : FilterField, IJsonSerializable
	{
		SecurityFilterContainer filterContainer;
		ZBool hideDeniedRights;
		ZBool groupByStaff;
		ZBool groupBySummary;
		readonly string securityRight;
		readonly string humanReadableName;

		public SecurityFilterField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal SecurityFilterField(SecurityFilterFieldJsonData data)
			: base(data)
		{
			securityRight = data.SecurityRight;
			var itemGuid = data.ItemGuid;
			FilterContainer.LookupKey = new CheckpointLookupKey(securityRight, itemGuid);
			humanReadableName = Env.Security.FindCheckPoint(FilterContainer.LookupKey)?.HumanReadableName.GetUnresolvedString() ?? securityRight;

			GroupByStaff = data.GroupByStaff;
			HideDeniedRights = data.HideDeniedRights;
			GroupBySummary = !GroupByStaff;
		}

		#endregion

		public override bool IsEmpty
		{
			get { return FilterContainer.LookupKey.IsEmpty && GroupBySummary && !HideDeniedRights; }
		}

		public SecurityFilterContainer FilterContainer
		{
			get
			{
				if (filterContainer == null)
				{
					filterContainer = new SecurityFilterContainer(DisplayName);
					RegisterEditableChildObject(filterContainer);
				}
				return filterContainer;
			}
		}

		public ZBool GroupByStaff
		{
			get { return groupByStaff; }
			set
			{
				UpdateGroupBy(value, !value);
			}
		}

		public ZPropertyInfo GroupByStaffInfo
		{
			get { return GetZPropertyInfo(nameof(GroupByStaff)); }
		}

		public ZBool GroupBySummary
		{
			get { return groupBySummary; }
			set
			{
				UpdateGroupBy(!value, value);
			}
		}

		public ZPropertyInfo GroupBySummaryInfo
		{
			get { return GetZPropertyInfo(nameof(GroupBySummary)); }
		}

		public ZBool HideDeniedRights
		{
			get { return hideDeniedRights; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(HideDeniedRightsInfo, ref hideDeniedRights, value);
				if (!IsValidationSuspended)
				{
					FilterContainer.ValidateLookupKeyValidationProxy();
				}
			}
		}

		public ZPropertyInfo HideDeniedRightsInfo
		{
			get { return GetZPropertyInfo(nameof(HideDeniedRights)); }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.SecurityFilterControl; }
		}

		public override object ValueAsObject => humanReadableName;

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			throw new NotImplementedException();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GroupBySummary = true;
		}

		void UpdateGroupBy(bool staff, bool summary)
		{
			SetNonPersistentPropertyValue<ZBool>(GroupByStaffInfo, ref groupByStaff, staff);
			SetNonPersistentPropertyValue<ZBool>(GroupBySummaryInfo, ref groupBySummary, summary);
			if (!IsValidationSuspended)
			{
				FilterContainer.ValidateLookupKeyValidationProxy();
			}
		}

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is SecurityFilterField securityFilter)
			{
				FilterContainer.LookupKey = new CheckpointLookupKey(securityFilter.FilterContainer.LookupKey.Code, securityFilter.FilterContainer.LookupKey.ItemGuid);
				GroupByStaff = securityFilter.GroupByStaff;
			}
		}

		public override void ClearValues()
		{
			FilterContainer.LookupKey = new CheckpointLookupKey();
			GroupByStaff = false;
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValid)
			{
				FilterContainer.LookupKeyValidationProxyInfo.ClearAllNotifications();
				FilterContainer.LookupKeyValidationProxyInfo.AddError(ValidationError);
				FilterContainer.SuspendValidation();
			}
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new SecurityFilter();
			SetBaseFilterData(filterData);

			if (!FilterContainer.LookupKey.IsEmpty)
			{
				filterData.SelectedValue = FilterContainer.LookupKey.Code.ToUpperInvariant();
			}

			reportFilterData.SecurityFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.SecurityFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				if (!string.IsNullOrEmpty(selectedValue.SelectedValue))
				{
					FilterContainer.LookupKey = new CheckpointLookupKey(selectedValue.SelectedValue);
				}
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new SecurityFilterFieldJsonData()
			{
				SecurityRight = FilterContainer.LookupKey.Code,
				ItemGuid = FilterContainer.LookupKey.ItemGuid,
				HideDeniedRights = HideDeniedRights,
				GroupByStaff = GroupByStaff
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
