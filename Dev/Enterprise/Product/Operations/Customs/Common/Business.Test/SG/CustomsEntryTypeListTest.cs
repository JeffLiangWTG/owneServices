using NUnit.Framework;

namespace Enterprise.Customs.Common.SG.Testing
{
	class CustomsEntryTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsTDBExemption()
		{
			NUnit.Framework.Assert.That(CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption("DP"), Is.EqualTo(true), "Exemption");
			NUnit.Framework.Assert.That(CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption("PMT"), Is.EqualTo(false), "Non Exemption code");
			NUnit.Framework.Assert.That(CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption("CER"), Is.EqualTo(false), "Non Exemption code");
			NUnit.Framework.Assert.That(CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption("CA"), Is.EqualTo(true), "Exemption");
		}
	}
}
