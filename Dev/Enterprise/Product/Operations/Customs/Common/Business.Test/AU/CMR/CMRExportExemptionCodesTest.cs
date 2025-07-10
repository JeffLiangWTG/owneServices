using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRExportExemptionCodesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFromExit2Exemption()
		{
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EX1), Is.EqualTo("EXPE").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EX2), Is.EqualTo("EXLV").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EX3), Is.EqualTo("EXTI").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EX5), Is.EqualTo("EXTI").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EXA), Is.EqualTo("EXML").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EXB), Is.EqualTo("EXDC").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.GetFromExit2Exemption(CusEntryNumberTypes.Australia.EXC), Is.EqualTo("EXSP").Using(CustomComparers.TypeComparison), "Code");
		}

		[ExpectNoExceptions]
		public void TestGet3CharCode()
		{
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EX1"), Is.EqualTo("EX1").Using(CustomComparers.TypeComparison), "Code");

			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXDC"), Is.EqualTo("XDC").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXDD"), Is.EqualTo("XDD").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXLV"), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXML"), Is.EqualTo("XML").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXPE"), Is.EqualTo("XPE").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXSP"), Is.EqualTo("XSP").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get3CharCode("EXTI"), Is.EqualTo("XTI").Using(CustomComparers.TypeComparison), "Code");
		}

		[ExpectNoExceptions]
		public void TestGet4CharCode()
		{
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("EX1"), Is.EqualTo("EX1").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XDC"), Is.EqualTo("EXDC").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XDD"), Is.EqualTo("EXDD").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XLV"), Is.EqualTo("EXLV").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XML"), Is.EqualTo("EXML").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XPE"), Is.EqualTo("EXPE").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XSP"), Is.EqualTo("EXSP").Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(CMRExportExemptionCodes.Get4CharCode("XTI"), Is.EqualTo("EXTI").Using(CustomComparers.TypeComparison), "Code");
		}
	}
}
