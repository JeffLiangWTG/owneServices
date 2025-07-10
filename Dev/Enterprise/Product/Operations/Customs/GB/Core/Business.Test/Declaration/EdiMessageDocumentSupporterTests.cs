using Enterprise.Core;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(EdiMessageDocumentSupporter))]
	class EdiMessageDocumentSupporterTests : DocumentSupporterTest
	{
		public void TestSupportedDataContexts()
		{
			var message = Factory.New<GbEDIMessage>();
			var supporter = new EdiMessageDocumentSupporter(message);
			AssertEquals("GbCcsuk", true, supporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GbCcsuk)));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<GbEDIMessage>();
	}
}
