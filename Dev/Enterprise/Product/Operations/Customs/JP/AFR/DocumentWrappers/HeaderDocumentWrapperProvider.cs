using CargoWise.Integration;
using Enterprise.Customs.JP.AFR.Business;
using static Enterprise.Integration.Customs.JP.AFR;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers;

public class HeaderDocumentWrapperProvider : IHeaderDocumentWrapperProvider
{
	IDocumentWrapper IHeaderDocumentWrapperProvider.GetDocumentWrapper(IJPAFRHeader header) => header is JPAFRHeader parent ? DocJPAFRHeader.New(parent, parent.Factory) : null;
}
