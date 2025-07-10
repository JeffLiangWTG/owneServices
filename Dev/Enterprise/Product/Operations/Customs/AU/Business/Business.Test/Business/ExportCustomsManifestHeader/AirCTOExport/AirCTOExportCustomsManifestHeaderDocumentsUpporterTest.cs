using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCTOExportCustomsManifestHeaderDocumentSupporter))]
	sealed class AirCTOExportCustomsManifestHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.AirCTOExport, Header.DocumentSupporter.BusinessContext);
		}

		public void TestDataContexts()
		{
			AssertEquals(true, Header.DocumentSupporter.ListOfSupportedDataContexts.ContainsCode(Core.Constants.DataContext.AirCTOExport));
		}

		public void GetDocumentWrappersInternal_AirCTOExport()
		{
			var wrappers = Header.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AirCTOExport, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals("Enterprise.DocumentWrappers.Customs.AU.DocAirCTOExport", wrappers[0].GetType().FullName);
			AssertEquals(Header, wrappers[0].WrappedObject);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Header;

		AirCTOExportCustomsManifestHeader header;
		AirCTOExportCustomsManifestHeader Header => header ?? (header = Factory.New<AirCTOExportCustomsManifestHeader>());
	}
}
