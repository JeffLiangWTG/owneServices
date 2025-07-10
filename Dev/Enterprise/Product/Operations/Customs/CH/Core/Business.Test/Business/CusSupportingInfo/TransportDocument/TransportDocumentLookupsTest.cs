using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business.Testing;

public class TransportDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		RefCusCodeTestHelper.CreateTransportDocumentCodes(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var transportDocument = invoiceHeader.TransportDocuments.AddNew();

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		var validExportCodes = new[] { RefCusCodeTestHelper.TransportDocumentCodeBorderau, RefCusCodeTestHelper.TransportDocumentCodeCarnetTIR, RefCusCodeTestHelper.TransportDocumentCodeCarnetATA };

		var collection = (ZZRefCusCodeListCombinedCollection)transportDocument.Lookups.CodeList;
		collection.Load();
		AssertContainsExactElementsInAnyOrder("Code List Elements", validExportCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(c => c.ZZD_Code));
	}
}
