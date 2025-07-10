using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondDocumentSupporter))]
	public class CusUnderbondDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var bo = Factory.New<CusUnderbond>();
			AssertEquals("This shipment does not have any underbond.", bo.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			var bo = Factory.New<CusUnderbond>();
			AssertEquals(true, bo.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.GenericFreightJob, null));
			AssertEquals(false, bo.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusUnderbond>();
		}
	}
}
