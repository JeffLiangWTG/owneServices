using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARComplianceDocumentLineCollection))]
	sealed class DocARComplianceDocumentLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocARComplianceDocumentLineCollection>
	{
		protected override DocARComplianceDocumentLineCollection GetCollectionToTest()
		{
			return new DocARComplianceDocumentLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocARComplianceDocumentLine.New(DocumentLine, Factory);
		}

		AccComplianceDocumentLine DocumentLine;
		protected override void SetUp()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var complianceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			DocumentLine = testObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test");
			base.SetUp();
		}
	}
}
