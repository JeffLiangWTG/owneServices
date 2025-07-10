using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.Registry
{
	public interface ICodeDescriptionBoolTreeNodeCollectionExtension
	{
		CodeDescriptionBoolTreeView CreateView(ZGuid parentID);
	}
}
