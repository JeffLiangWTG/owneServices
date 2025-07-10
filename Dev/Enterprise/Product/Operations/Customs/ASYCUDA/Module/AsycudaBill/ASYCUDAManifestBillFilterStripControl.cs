using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public partial class ASYCUDAManifestBillFilterStripControl : ZFilterStripControl
	{
		protected ASYCUDAManifestBillFilterStripControl() { }

		public ASYCUDAManifestBillFilterStripControl(IBusinessObjectCollection gridCollection, ASYCUDAManifestBillFilterStrip billFilterStripBusinessObject)
			: base(gridCollection, billFilterStripBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode);
			}
		}

		protected override int MaximumAllowableQueriesPerSqlStatement => ManifestCustomsDataRegistry.Instance.MaximumSearchableManifestBills.Value;

		protected override ZFilterStrip NewZFilterStrip() => new AsycudaModuleStrip();
	}
}
