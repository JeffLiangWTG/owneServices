using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(CommonSupportingDocumentValidation))]
	abstract class SupportingDocumentValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			supportingDocument = SetupSupportingDocument();
			validation = supportingDocument.Validation;
		}
		protected JobDeclaration declaration;
		protected SupportingDocument supportingDocument;
		protected CommonSupportingDocumentValidation validation;

		protected abstract string MessageType { get; }
		protected abstract SupportingDocument SetupSupportingDocument();
	}
}
