using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(LocalReferenceNumberGenerator))]
sealed class LocalReferenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new LocalReferenceNumberGenerator(dbConnected: null));
	}

	[TestDate(2025, 01, 01)]
	public void TestGenerate()
	{
		ILocalReferenceNumberGenerator localReferenceNumberGenerator = new LocalReferenceNumberGenerator(Factory);
		AssertEquals("First LRN", "2025EDIDAT000000000001", localReferenceNumberGenerator.Generate());
		AssertEquals("Second LRN", "2025EDIDAT000000000002", localReferenceNumberGenerator.Generate());
	}
}
