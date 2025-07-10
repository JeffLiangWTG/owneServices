using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class SupportIncidentFilterControl : ZFilterStripControl
	{
		public SupportIncidentFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, IncidentConstants.IncidentType.SupportIncident);
			FilteredGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(FilteredGrid_ColourDeciding);
		}

		#region Colour Deciding

		void FilteredGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
		}
		#endregion

	}
}
