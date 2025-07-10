using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXIT1MessageType))]
	sealed class ReplacementTypeForEXIT1Test : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions()]
		public void TestContruct()
		{
			new EXIT1MessageType();
		}

		public void TestDefaultValues()
		{
			EXIT1MessageType testBO = new EXIT1MessageType();
			AssertEquals("Set", true, testBO.ZX_IsConfirmed);
			AssertEquals("Not set", false, testBO.ZX_IsConfirming);
		}
	}
}
