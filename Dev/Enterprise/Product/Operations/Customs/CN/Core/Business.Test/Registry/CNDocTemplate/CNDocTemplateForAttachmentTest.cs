using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachment))]
	class CNDocTemplateForAttachmentTest : RegistryBusinessObjectTemplateTestCase<CNDocTemplateForAttachment>
	{
		public void TestDefaultDataContext()
		{
			var docTemplate = (CNDocTemplateForAttachment)GetNewBusinessObject();
			AssertEquals(".CustomsDeclarationDocument", docTemplate.DataContext);
		}

		public void TestDefaultDocumentDescription()
		{
			CombineAssertions(() =>
			{
				var docTemplate = (CNDocTemplateForAttachment)GetNewBusinessObject();
				docTemplate.DocumentType = "INV";
				AssertEquals("Default DocumentDescription when it is empty", "Invoice", docTemplate.DocumentDescription);

				docTemplate.DocumentType = "MSC";
				AssertEquals("Not default DocumentDescription when it has value", "Invoice", docTemplate.DocumentDescription);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new CNDocTemplateForAttachmentCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override CNDocTemplateForAttachment GetBusinessObjectToClone() => (CNDocTemplateForAttachment)GetNewBusinessObject();

		protected override CNDocTemplateForAttachment GetBusinessObjectToSerialise() => (CNDocTemplateForAttachment)GetNewBusinessObject();
	}
}
