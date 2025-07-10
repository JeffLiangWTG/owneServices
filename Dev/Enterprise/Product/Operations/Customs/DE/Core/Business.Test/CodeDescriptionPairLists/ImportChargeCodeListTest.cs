using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ImportChargeCodeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsSpecificRate()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecificRate(ImportChargeCodeList.Codes.SRC), Is.EqualTo(true), ImportChargeCodeList.Codes.SRC);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecificRate(ImportChargeCodeList.Codes.SRN), Is.EqualTo(true), ImportChargeCodeList.Codes.SRN);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecificRate(ImportChargeCodeList.Codes.SRS), Is.EqualTo(true), ImportChargeCodeList.Codes.SRS);
			});
		}

		[ExpectNoExceptions]
		public void TestIsSpecialRate()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecialRate(ImportChargeCodeList.Codes.SRC), Is.EqualTo(true), ImportChargeCodeList.Codes.SRC);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecialRate(ImportChargeCodeList.Codes.SRN), Is.EqualTo(true), ImportChargeCodeList.Codes.SRN);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecialRate(ImportChargeCodeList.Codes.SRS), Is.EqualTo(true), ImportChargeCodeList.Codes.SRS);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecialRate(ImportChargeCodeList.Codes.OPF), Is.EqualTo(true), ImportChargeCodeList.Codes.OPF);
				NUnit.Framework.Assert.That(ImportChargeCodeList.IsSpecialRate(ImportChargeCodeList.Codes.TCE), Is.EqualTo(true), ImportChargeCodeList.Codes.TCE);
			});
		}
	}
}
