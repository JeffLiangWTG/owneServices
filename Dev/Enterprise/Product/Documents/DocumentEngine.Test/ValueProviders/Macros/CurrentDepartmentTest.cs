using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentDepartment))]
	sealed class CurrentDepartmentTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CurrentDepartment >", ValueProviderToTest.IsResponsibleForReplacing("< CurrentDepartment >", Passes.FirstPass));
			Assert("should match < Current Department>", ValueProviderToTest.IsResponsibleForReplacing("< Current Department>", Passes.FirstPass));
			Assert("should match < Current Department >", ValueProviderToTest.IsResponsibleForReplacing("< Current Department >", Passes.FirstPass));
			Assert("should match < CurrentDepartment>", ValueProviderToTest.IsResponsibleForReplacing("< CurrentDepartment>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbDepartment.CurrentDepartment.PK, ValueProviderToTest.GetReplacement("<CurrentDepartment>", Report));
		}

		public void TestGetReplacement_WhenDepartmentIsNull_ShouldNotThrow()
		{
			using (Environment.Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertEquals(Guid.Empty, ValueProviderToTest.GetReplacement("<CurrentDepartment>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrentDepartment();
		}
	}
}
