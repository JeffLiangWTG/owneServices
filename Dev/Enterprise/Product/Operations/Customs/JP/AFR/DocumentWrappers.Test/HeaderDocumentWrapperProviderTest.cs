using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs.JP.AFR;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing;

sealed class HeaderDocumentWrapperProviderTest : TestCase
{
	public void TestGetDocumentWrapper()
	{
		var header = new BusinessObjectFactory().New<JPAFRHeader>();
		var provider = ObjectFactory.Get<IHeaderDocumentWrapperProvider>();
		var wrapper = provider.GetDocumentWrapper(header);

		AssertType<DocJPAFRHeader>(wrapper);

		wrapper = provider.GetDocumentWrapper(null);
		AssertNull(wrapper);
	}
}
