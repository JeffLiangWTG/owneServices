using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class ValidationResultTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestValid()
		{
			CombineAssertions(() =>
			{
				var valid = ValidationResult.Valid;
				NUnit.Framework.Assert.That(valid.IsValid, NUnit.Framework.Is.EqualTo(true), "IsValid");
				NUnit.Framework.Assert.That(valid.Message, NUnit.Framework.Is.EqualTo(string.Empty), "Message");
			});
		}

		[ExpectNoExceptions]
		public void TestInvalid()
		{
			CombineAssertions(() =>
			{
				var valid = ValidationResult.Invalid("Invalid");
				NUnit.Framework.Assert.That(valid.IsValid, NUnit.Framework.Is.EqualTo(false), "IsValid");
				NUnit.Framework.Assert.That(valid.Message, NUnit.Framework.Is.EqualTo("Invalid"), "Message");
			});
		}
	}
}
