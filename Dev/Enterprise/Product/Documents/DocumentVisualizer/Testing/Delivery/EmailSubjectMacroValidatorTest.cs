using CargoWise.Application;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EmailSubjectMacroValidatorTest : TestCase
	{
		public void TestValidateMacro_ValidSyntax()
		{
			const string emailSubjectMacro = "My email subject: <JobNumber>";

			var validator = new EmailSubjectMacroValidator();
			AssertEquals("no validation errors", string.Empty, validator.Validate(emailSubjectMacro));
		}

		public void TestValidateMacro_InvalidSyntax()
		{
			const string emailSubjectMacro = "My email subject: <JobNumber";

			var validator = new EmailSubjectMacroValidator();
			AssertNotEquals("validation errors", string.Empty, validator.Validate(emailSubjectMacro));
		}

		public void TestObjectFactoryRegistration()
		{
			var validator = ObjectFactory.Get<IEmailSubjectMacroValidator>();
			AssertNotNull("email subject validator has been registered", validator);
		}
	}
}
