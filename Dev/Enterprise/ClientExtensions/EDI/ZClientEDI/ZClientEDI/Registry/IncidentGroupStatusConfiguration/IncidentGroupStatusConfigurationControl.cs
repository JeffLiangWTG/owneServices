using System;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class IncidentGroupStatusConfigurationControl : RegistryZUserControl
	{
		public IncidentGroupStatusConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var incidentGroupType = this.CurrentDataItem as IncidentGroupType;
			if (incidentGroupType != null)
			{
				SetParentForChildren(incidentGroupType);
				incidentGroupType.ShowChildrenNotification();
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			IncidentGroupTypeZGrid.ReadOnly = readOnly;
			IncidentGroupStatusConfigurationsZGrid.ReadOnly = readOnly;
		}

		void IncidentGroupTypeZGrid_SelectIndexChanged(object sender, EventArgs e)
		{
			var incidentGroupType = this.CurrentDataItem as IncidentGroupType;
			if (incidentGroupType != null)
			{
				SetParentForChildren(incidentGroupType);
				incidentGroupType.ShowChildrenNotification();
			}
		}

		public bool IsReadOnly => IncidentGroupTypeZGrid.ReadOnly && IncidentGroupStatusConfigurationsZGrid.ReadOnly;

		void SetParentForChildren(IncidentGroupType incidentGroupType)
		{
			var groupTypeCode = incidentGroupType.GroupType;
			var incidentGroupStatusConfigurations = incidentGroupType.IncidentGroupStatusConfigurations;
			incidentGroupStatusConfigurations.IncidentGroupTypeCode = groupTypeCode;
			foreach (var item in incidentGroupStatusConfigurations)
			{
				item.ParentCode = groupTypeCode;
			}
		}
	}
}
