using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DeclarationExporter))]
	public class DeclarationExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when dataProvider parameter is null", () => new DeclarationExporter(exporterDeclaration: null, factory: null, maxLengthInfo: null));
		}

		[ExpectNoExceptions]
		public void TestAllFieldsAreAlterable()
		{
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(declarationExporter, ZString.Empty, nameof(declarationExporter.Place));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(declarationExporter, ZDate.Empty, nameof(declarationExporter.ReferenceDate));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(declarationExporter, ZString.Empty, nameof(declarationExporter.SupplierDetails));
		}

		[ExpectNoExceptions]
		public void TestPlace()
		{
			dataProviderMock.Setup(x => x.Place).Returns("PLACE");
			NUnit.Framework.Assert.That(declarationExporter.Place, NUnit.Framework.Is.EqualTo("PLACE").Using(CustomComparers.TypeComparison), nameof(declarationExporter.Place));
		}

		[ExpectNoExceptions]
		public void TestReferenceDate()
		{
			dataProviderMock.Setup(x => x.ReferenceDate).Returns(new ZDate(2022, 01, 01));
			NUnit.Framework.Assert.That(declarationExporter.ReferenceDate, NUnit.Framework.Is.EqualTo(new ZDate(2022, 01, 01)), nameof(declarationExporter.ReferenceDate));
		}

		[ExpectNoExceptions]
		public void TestSupplierDetails()
		{
			dataProviderMock.Setup(x => x.ExporterDetails).Returns("SUPDET");
			NUnit.Framework.Assert.That(declarationExporter.SupplierDetails, NUnit.Framework.Is.EqualTo("SUPDET").Using(CustomComparers.TypeComparison), nameof(declarationExporter.SupplierDetails));
		}

		protected override BusinessObject GetNewBusinessObject() => new DeclarationExporter(new Mock<IExporterDeclaration>().Object, factory: null, maxLengthInfo: null);

		protected override void SetUp()
		{
			base.SetUp();
			dataProviderMock = new Mock<IExporterDeclaration>();
			declarationExporter = new DeclarationExporter(dataProviderMock.Object, factory: null, maxLengthInfo: null);
		}

		Mock<IExporterDeclaration> dataProviderMock;
		DeclarationExporter declarationExporter;
	}
}
