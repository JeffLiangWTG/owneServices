using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Testing
{
	[TestedType(typeof(DocDeclarationDocument))]
	sealed class DocDeclarationDocumentTestClass : NonPersistentBusinessObjectTestCase
	{
		public void TestName()
		{
			DocDeclarationDocument decDoc = new DocDeclarationDocument("Name", ZBool.False, ZBool.False);
			AssertEquals("Name", "Name", decDoc.Name);
		}

		public void TestIsRequired()
		{
			DocDeclarationDocument decDoc = new DocDeclarationDocument("Name", ZBool.False, ZBool.False);
			Assert("!IsRequired", !decDoc.IsRequired);

			decDoc = new DocDeclarationDocument("Name", ZBool.True, ZBool.False);
			Assert("IsRequired", decDoc.IsRequired);
		}

		public void TestIsReceived()
		{
			DocDeclarationDocument decDoc = new DocDeclarationDocument("Name", ZBool.False, ZBool.False);
			Assert("!IsReceived", !decDoc.IsReceived);

			decDoc = new DocDeclarationDocument("Name", ZBool.False, ZBool.True);
			Assert("IsReceived", decDoc.IsReceived);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDeclarationDocument("Name", ZBool.False, ZBool.False);
		}
	}
}
