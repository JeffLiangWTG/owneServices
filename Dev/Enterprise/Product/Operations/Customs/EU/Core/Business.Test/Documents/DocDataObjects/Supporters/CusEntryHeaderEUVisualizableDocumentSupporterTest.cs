using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	sealed class CusEntryHeaderEUVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCustomizeFormCheckpoint()
		{
			NUnit.Framework.Assert.That(supporter.CustomizeFormCheckpoint, NUnit.Framework.Is.EqualTo(Env.Security.MaintainJobDeclarationCustomiseForms).Using(CustomComparers.TypeComparison), nameof(supporter.CustomizeFormCheckpoint));
		}

		[ExpectNoExceptions]
		public void TestGetDocDataObject()
		{
			var docDataObject = supporter.GetDocDataObject(entryHeader, DataContext.JobDeclaration, null);
			NUnit.Framework.Assert.That(docDataObject.Right, NUnit.Framework.Is.Not.EqualTo(default(object)), "nameof(docDataObject.Right) - should not be [null]");
			NUnit.Framework.Assert.That(docDataObject.Right, NUnit.Framework.Is.TypeOf<JobDeclarationDocDataObject>(), $"{nameof(docDataObject.Right)} type");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			supporter = new CusEntryHeaderEUVisualizableDocumentSupporter(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryHeaderEUVisualizableDocumentSupporter supporter;
	}
}
