using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(CommonPreviousDocumentValidation))]
	abstract class PreviousDocumentValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			previousDocument = SetupPreviousDocument();
			validation = previousDocument.Validation;
		}
		protected JobDeclaration jobDeclaration;
		protected PreviousDocument previousDocument;
		protected CommonPreviousDocumentValidation validation;

		protected abstract string MessageType { get; }
		protected abstract PreviousDocument SetupPreviousDocument();
	}
}
