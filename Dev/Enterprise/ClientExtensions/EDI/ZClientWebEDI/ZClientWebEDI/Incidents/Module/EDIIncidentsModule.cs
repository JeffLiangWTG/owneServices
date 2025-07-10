using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZClientWebCargoWiseEDI.Module
{
	public class EDIIncidentsModule : ZArchitecture.Web.Modules.ZFilterGridModule
	{
		public EDIIncidentsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.CargoWiseEDIIncidents; }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebSupportIncidentFilterBusinessObject); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(IncidentSearchUserControl), "IncidentSearchUserControl.ascx", Page,
				"Enterprise.ZClientWebCargoWiseEDI.Incidents");
		}

		public override Type FilterControlType
		{
			get { return typeof(IncidentSearchUserControl); }
		}

		#endregion

		#region Columns

		internal DataGridColumn[] InternalGetNewGridColumnFields() => GetNewGridColumnFields();
		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			List<DataGridColumn> columns = new List<DataGridColumn>();

			ZHyperLinkColumn incidentNumberColumn = new ZHyperLinkColumn("Number", SupportIncident.Schema.IM_IncidentNumber);
			incidentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(Global.IncidentDetailsPagePath) + "?Ref={0}";
			incidentNumberColumn.DataNavigateUrlFields = new string[1] { "PK" };
			columns.Add(incidentNumberColumn);

			ZBindToChecker.CheckBindTo(((SupportIncident)null).Lookups.ModuleListAllModules);
			ZDropEditColumn moduleColumn = new ZDropEditColumn("Module", SupportIncident.Schema.IM_Module);
			moduleColumn.BindToList = "Lookups.ModuleListAllModules";
			moduleColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			columns.Add(moduleColumn);

			ZTextEditColumn criticalityColumn = new ZTextEditColumn("Crit.", SupportIncident.Schema.IM_Priority);
			columns.Add(criticalityColumn);

			ZDateTimeColumn addedColumn = new ZDateTimeColumn("Added", SupportIncident.Schema.IM_SystemCreateTimeUtc);
			addedColumn.ItemStyle.Width = Unit.Pixel(70);
			columns.Add(addedColumn);

			ZDateTimeColumn closedColumn = new ZDateTimeColumn("Closed", SupportIncident.Schema.IM_CloseTimeUtc);
			columns.Add(closedColumn);

			ZBindToChecker.CheckBindTo(((SupportIncident)null).IM_Status);
			ZBindToChecker.CheckBindTo(((SupportIncident)null).Lookups.StatusList);
			ZDropEditColumn statusColumn = new ZDropEditColumn("Status", SupportIncident.Schema.IM_Status);
			statusColumn.BindToList = "Lookups.StatusList";
			statusColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			columns.Add(statusColumn);

			ZBindToChecker.CheckBindTo(((SupportIncident)null).Stage);
			ZTextEditColumn stageColumn = new ZTextEditColumn("Stage", "Stage");
			columns.Add(stageColumn);

			ZNewRowColumn descriptionColumn = new ZNewRowColumn(IncidentMainSchema.Constants.IM_Description);
			descriptionColumn.ItemTemplate = new ZTextEditColumnItemTemplate(descriptionColumn);
			descriptionColumn.ItemStyle.CssClass = "DescriptionRow";
			descriptionColumn.HeaderText = "Description";
			columns.Add(descriptionColumn);

			return columns.ToArray();
		}

		#endregion

		#region Sorting

		public override IComparer GetNewCollectionSorterCore(string sortProperty, ListSortDirection direction)
		{
			return new EDIIncidentWebCollectionSorter(sortProperty, direction);
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(IncidentMainSchema.IM_SystemCreateTimeUtc.Name, DefaultSortOrder) };
		}

		public override ListSortDirection DefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion

		#region Collection Type

		public override Type GridCollectionType
		{
			get { return typeof(SupportIncidentCollection); }
		}

#endregion
	}
}
