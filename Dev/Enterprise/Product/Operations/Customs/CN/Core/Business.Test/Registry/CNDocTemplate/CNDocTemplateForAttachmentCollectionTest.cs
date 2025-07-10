using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachmentCollection))]
	class CNDocTemplateForAttachmentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CNDocTemplateForAttachmentCollection>
	{
		public void TestGetDefault()
		{
			var defaultCollection = CNDocTemplateForAttachmentCollection.GetDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, defaultCollection.Count);
				AssertCNDocTemplateForAttachment("#1", defaultCollection[0], ZGuid.Empty, ".CustomsDeclarationDocument", "CN Customs Invoice(System)", "MSC", "Miscellaneous Document", "00000001");
				AssertCNDocTemplateForAttachment("#2", defaultCollection[1], ZGuid.Empty, ".CustomsDeclarationDocument", "CN Purchase Order(System)", "MSC", "Miscellaneous Document", "00000004");
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override CNDocTemplateForAttachmentCollection GetCollectionToTest() => new CNDocTemplateForAttachmentCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CNDocTemplateForAttachment();

		static void AssertCNDocTemplateForAttachment(string message, CNDocTemplateForAttachment defaultDocTemplate, ZGuid organizationPK, ZString dataContext, ZString documentTemplate, ZString documentType, ZString documentDescription, ZString attachmentType)
		{
			AssertEquals(message + "->OrganizationPK", organizationPK, defaultDocTemplate.OrganizationPK);
			AssertEquals(message + "->DataContext", dataContext, defaultDocTemplate.DataContext);
			AssertEquals(message + "->DocumentTemplate", documentTemplate, defaultDocTemplate.DocumentTemplate);
			AssertEquals(message + "->DocumentType", documentType, defaultDocTemplate.DocumentType);
			AssertEquals(message + "->DocumentDescription", documentDescription, defaultDocTemplate.DocumentDescription);
			AssertEquals(message + "->AttachmentType", attachmentType, defaultDocTemplate.AttachmentType);
		}
	}
}
