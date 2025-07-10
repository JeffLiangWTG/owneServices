using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public partial class NctsMovementFilterControl : ZFilterStripControl
	{
		public NctsMovementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode);
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor);
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor);
			}
		}
	}
}
