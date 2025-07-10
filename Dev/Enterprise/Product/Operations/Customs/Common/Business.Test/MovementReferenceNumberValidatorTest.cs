using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class MovementReferenceNumberValidatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMrnTypes()
		{
			NUnit.Framework.Assert.That(((MrnTypes)0).Equals(MrnTypes.Unknown), Is.EqualTo(true));
			NUnit.Framework.Assert.That(((MrnTypes)1).Equals(MrnTypes.Transit), Is.EqualTo(true));
			NUnit.Framework.Assert.That(((MrnTypes)2).Equals(MrnTypes.Import), Is.EqualTo(true));
			NUnit.Framework.Assert.That(((MrnTypes)3).Equals(MrnTypes.Export), Is.EqualTo(true));
		}
	}
}
