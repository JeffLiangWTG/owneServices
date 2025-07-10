using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderDocumentSupporter))]
	sealed class TemporaryStorageHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", BusinessContext.EuPnts, DocumentSupporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("CustomisationSecurityCheckpoint", Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestDefaultDataContext()
		{
			AssertEquals("DefaultDataContext", DataContext.EuPnts, DocumentSupporter.DefaultDataContext);
		}

		public void TestGetDocumentWrappers()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.EuPnts, null);
			AssertEquals("Wrappers count", 1, wrappers.Length);
			AssertEquals("Wrapper type", "Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper", wrappers[0].GetType().FullName);
		}

		public void TestGetFilterValue()
		{
			AssertEquals("Filter CTYEG=EUN", "EUN", DocumentSupporter.GetFilterValue(DocumentFilters.CTYEG));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Header;

		TemporaryStorageHeader Header => header ??= Factory.New<TemporaryStorageHeader>();
		TemporaryStorageHeader header;

		DocumentSupporter DocumentSupporter => documentSupporter ??= ((IDocumentSupportable)Header).DocumentSupporter;
		DocumentSupporter documentSupporter;
	}
}
