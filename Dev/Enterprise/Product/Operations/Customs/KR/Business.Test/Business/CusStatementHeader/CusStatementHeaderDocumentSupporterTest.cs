using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderDocumentSupporter))]
	sealed class CusStatementHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowShowReasonForNotPrinting()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(Env.Security.CustomsDeclarationCustomiseDocument, docSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Statement)));
		}

		public void TestBusinessContext()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(BusinessContext.Statement, docSupporter.BusinessContext);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusStatementHeader>();
		}
	}
}
