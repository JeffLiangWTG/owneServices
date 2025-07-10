using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Service
{
	public interface IDocumentListService
	{
		DocumentListItem[] GetDocumentList(string businessContext, OrgContact contact, ZGuid entityPK, string entityTableCode);
	}
}
