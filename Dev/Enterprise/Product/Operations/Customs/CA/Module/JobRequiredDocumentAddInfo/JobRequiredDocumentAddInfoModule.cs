using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	public class JobRequiredDocumentAddInfoModule : MasterFiles.Module.JobRequiredDocumentAddInfoModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobRequiredDocumentAddInfoFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new JobRequiredDocumentAddInfoFilterStripControl(GridCollection, FilterBusinessObject);
		}
	}
}
