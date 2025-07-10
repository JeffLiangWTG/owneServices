using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class DeclarationTypeAndEntryStyleValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DeclarationTypeAndEntryStyleValidator(entryInstruction: null));
	}
}
