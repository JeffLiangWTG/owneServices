using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementHeaderDocumentSupporter))]
	public class CusStatementHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.Statement, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CustomsDeclarationCustomiseDocument, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetDocumentWrappersInternal()
		{
			var wrappers = (header as IDocumentSupportable).DocumentSupporter.GetDocumentWrappers(DataContext.Statement, null);
			AssertEquals("Enterprise.Customs.FR.DocumentWrappers.Statement.DocStatement", wrappers.Single().GetType().FullName);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusStatementHeader>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusStatementHeader>();
			supporter = new CusStatementHeaderDocumentSupporter(header);
		}
		CusStatementHeader header;
		CusStatementHeaderDocumentSupporter supporter;
	}
}
