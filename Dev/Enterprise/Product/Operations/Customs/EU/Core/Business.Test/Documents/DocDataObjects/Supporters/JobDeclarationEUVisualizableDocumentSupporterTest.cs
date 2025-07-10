using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	sealed class JobDeclarationEUVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCustomizeFormCheckpoint()
		{
			NUnit.Framework.Assert.That(supporter.CustomizeFormCheckpoint, NUnit.Framework.Is.EqualTo(Env.Security.MaintainJobDeclarationCustomiseForms).Using(CustomComparers.TypeComparison), nameof(supporter.CustomizeFormCheckpoint));
		}

		[ExpectNoExceptions]
		public void TestGetDocDataObject()
		{
			var docDataObject = supporter.GetDocDataObject(declaration, DataContext.CMRWayBill, null);
			NUnit.Framework.Assert.That(docDataObject.Right, NUnit.Framework.Is.Not.EqualTo(default(object)), "nameof(docDataObject.Right) - should not be [null]");
			NUnit.Framework.Assert.That(docDataObject.Right, NUnit.Framework.Is.TypeOf<CMRConsignmentNoteDocDataObject>(), $"{nameof(docDataObject.Right)} type");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			supporter = new JobDeclarationEUVisualizableDocumentSupporter(declaration);
		}

		JobDeclaration declaration;
		JobDeclarationEUVisualizableDocumentSupporter supporter;
	}
}
