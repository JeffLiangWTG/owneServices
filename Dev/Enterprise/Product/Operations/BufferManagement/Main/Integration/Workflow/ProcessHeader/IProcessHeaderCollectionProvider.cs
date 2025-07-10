using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderCollectionProvider
	{
		IProcessHeaderCollection GetForTemplate(IProcessTaskTemplate template);
		IProcessHeaderLinkCollection GetLinkCollection(IProcessTaskTemplate factory);
		IProcessHeaderCollection GetForTask(IProcessTask processTask);
		IBusinessObjectCollection GetCollectionWithAdhocRelationship(BusinessObjectFactory factory);
		IBusinessObjectCollection GetCollection(BusinessObjectFactory factory);

		void EnsureJobHeaderPresentWhenRequired(IProcessTaskTemplate template);
	}
}
