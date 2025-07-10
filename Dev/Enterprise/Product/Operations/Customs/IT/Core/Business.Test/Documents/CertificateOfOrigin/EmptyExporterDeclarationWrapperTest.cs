using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EmptyExporterDeclarationWrapperTest : TestCase
{
	public void TestPlace()
	{
		var emptyExporterDeclarationWrapper = (IExporterDeclaration)new EmptyExporterDeclarationWrapper();
		AssertEquals("Place", "", emptyExporterDeclarationWrapper.Place);
	}

	public void TestReferenceDate()
	{
		var emptyExporterDeclarationWrapper = (IExporterDeclaration)new EmptyExporterDeclarationWrapper();
		AssertEquals("RefereneDate", ZDate.Empty, emptyExporterDeclarationWrapper.ReferenceDate);
	}

	public void TestExporterDetails()
	{
		var emptyExporterDeclarationWrapper = (IExporterDeclaration)new EmptyExporterDeclarationWrapper();
		AssertEquals("ExporterDetails", "", emptyExporterDeclarationWrapper.ExporterDetails);
	}
}
