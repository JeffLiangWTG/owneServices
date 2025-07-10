using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class CustomsNumberTypeCodeDescriptionTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionImplementation()
		{
			CodeDescriptionPair codeDescriptionPair = new CustomsNumberTypeCodeDescription("XXX", (NoResString)"Description");
			NUnit.Framework.Assert.That(codeDescriptionPair.Code, Is.EqualTo("XXX"));
			NUnit.Framework.Assert.That(codeDescriptionPair.Description, Is.EqualTo("Description"));
		}

		[ExpectNoExceptions]
		public void TestIsUnique()
		{
			NUnit.Framework.Assert.That(new CustomsNumberTypeCodeDescription("XXX", (NoResString)"Description").IsUnique, Is.EqualTo(true), "Unique by default");
			NUnit.Framework.Assert.That(new CustomsNumberTypeCodeDescription("XXX", (NoResString)"Description", true).IsUnique, Is.EqualTo(true));
			NUnit.Framework.Assert.That(new CustomsNumberTypeCodeDescription("XXX", (NoResString)"Description", false).IsUnique, Is.EqualTo(false));
		}
	}
}
