using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValidation))]
	abstract class JobDeclarationValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			validation = declaration.Validation;
		}
		protected JobDeclaration declaration;
		protected JobDeclarationValidation validation;

		protected abstract string MessageType { get; }

		protected abstract JobDeclarationValidation GetValidation();
	}
}
