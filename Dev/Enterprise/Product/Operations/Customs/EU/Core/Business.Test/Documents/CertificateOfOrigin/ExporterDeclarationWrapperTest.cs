using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class ExporterDeclarationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPlace()
		{
			var wrapper = (IExporterDeclaration)new ExporterDeclarationWrapper();
			NUnit.Framework.Assert.That(wrapper.Place, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Place");
		}

		[ExpectNoExceptions]
		public void TestReferenceDate()
		{
			var wrapper = (IExporterDeclaration)new ExporterDeclarationWrapper();
			NUnit.Framework.Assert.That(wrapper.ReferenceDate, NUnit.Framework.Is.EqualTo(ZDate.Empty), "ReferenceDate");
		}

		[ExpectNoExceptions]
		public void TestExporterDetails()
		{
			var wrapper = (IExporterDeclaration)new ExporterDeclarationWrapper();
			NUnit.Framework.Assert.That(wrapper.ExporterDetails, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ExporterDetails");
		}
	}
}
