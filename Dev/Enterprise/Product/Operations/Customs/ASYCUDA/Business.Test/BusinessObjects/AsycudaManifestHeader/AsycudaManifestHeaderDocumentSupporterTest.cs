using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderDocumentSupporter))]
	sealed class AsycudaManifestHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(BusinessContext.AsycudaManifest, header.DocumentSupporter.BusinessContext);
		}

		public void TestDataContexts()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(true, header.DocumentSupporter.ListOfSupportedDataContexts.ContainsCode(DataContext.AsycudaManifestHeader));
		}

		public void TestGetDocumentWrappersInternal()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var wrappers = header.DocumentSupporter.GetDocumentWrappers(DataContext.AsycudaManifestHeader, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals("Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderDocWrapper", wrappers[0].GetType().FullName);
			AssertEquals(header, wrappers[0].WrappedObject);
		}

		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var supporter = new AsycudaManifestHeaderDocumentSupporter(header);
			var menuItem = Factory.New<IStmMenuItem>();
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Core.Constants.DataContext.AsycudaManifestHeader), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Core.Constants.DataContext.AsycudaManifestHeader), menuItem));
			menuItem.SU_MenuName = AsycudaManifestHeaderDocWrapper.ZAManifestWithBarcode;
			AssertEquals("Cannot produce this Document because there is no South African Manifest data.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Core.Constants.DataContext.AsycudaManifestHeader), menuItem));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();
	}
}
