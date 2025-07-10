using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashDocumentText))]
	sealed class DashDocumentTextTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var eDoc1 = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc1.PK, "CIV");
			var dashDocumentText = helpers.CreateDashDocumentText(dashDocument.PK, "This is the document text");
			return dashDocumentText;
		}
	}
}
