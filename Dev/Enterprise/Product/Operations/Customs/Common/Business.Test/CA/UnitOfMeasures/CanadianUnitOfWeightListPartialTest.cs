using NUnit.Framework;

namespace Enterprise.Customs.Common.CA.Testing
{
	class CanadianUnitOfWeightListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetUnitAndDescription()
		{
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.MetricCarat), Is.EqualTo("CTM Metric Carat"), "Metric Carat");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Deciton), Is.EqualTo("DTN Deciton"), "Deciton");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Gram), Is.EqualTo("GRM Gram"), "Gram");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Hectogram), Is.EqualTo("HGM Hectogram"), "Hectogram");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Kilogram), Is.EqualTo("KGM Kilogram"), "Kilogram");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.KilogramOfNamedSubstance), Is.EqualTo("KNS Kilogram of Named Substance"), "Kilogram of Named Substance");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Kilogram90PercentAirDry), Is.EqualTo("KSD Kilogram 90% Air Dry"), "Kilogram 90% Air Dry");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Kiloton), Is.EqualTo("KTN Kiloton"), "Kiloton");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.Milligram), Is.EqualTo("MGM Milligram"), "Milligram");
			NUnit.Framework.Assert.That(CanadianUnitOfWeightList.GetUnitAndDescription(UnitOfWeightList.Codes.MetricTon), Is.EqualTo("TNE Metric Ton"), "Metric Ton");
		}
	}
}
