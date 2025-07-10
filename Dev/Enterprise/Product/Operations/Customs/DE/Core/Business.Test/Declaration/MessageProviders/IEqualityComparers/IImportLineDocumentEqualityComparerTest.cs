using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IImportLineDocumentEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, importLineDocumentEqualityComparer.GetHashCode(originalImportLineDocument));
		}

		public void TestEquals()
		{
			AssertEquals(true, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, ImportLineDocumentMock.Object));
		}

		public void TestEquals_Division()
		{
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.Division).Returns("2");
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		public void TestEquals_DocumentType()
		{
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.DocumentType).Returns("6FFG");
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		public void TestEquals_ReferenceNumber()
		{
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.ReferenceNumber).Returns("POCL7341657901");
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		public void TestEquals_IssuingDate()
		{
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.IssuingDate).Returns(DateTime.Today);
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		public void TestEquals_AtHandFlag()
		{
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.AtHandFlag).Returns("N");
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		public void TestEquals_WriteOff()
		{
			var amountMock = IAmountEqualityComparerTest.AmountMock;
			amountMock.Setup(x => x.Qualifier).Returns("M");
			var importLineDocumentMock = ImportLineDocumentMock;
			importLineDocumentMock.Setup(x => x.WriteOff).Returns(amountMock.Object);
			AssertEquals(false, importLineDocumentEqualityComparer.Equals(originalImportLineDocument, importLineDocumentMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalImportLineDocument = ImportLineDocumentMock.Object;
			importLineDocumentEqualityComparer = new IImportLineDocumentEqualityComparer();
		}
		IImportLineDocument originalImportLineDocument;
		IImportLineDocumentEqualityComparer importLineDocumentEqualityComparer;

		Mock<IImportLineDocument> ImportLineDocumentMock
		{
			get
			{
				var importLineDocumentMock = new Mock<IImportLineDocument>();
				importLineDocumentMock.Setup(x => x.Division).Returns("4");
				importLineDocumentMock.Setup(x => x.DocumentType).Returns("7HHF");
				importLineDocumentMock.Setup(x => x.ReferenceNumber).Returns("COSU6271657530");
				importLineDocumentMock.Setup(x => x.IssuingDate).Returns(new DateTime(2022, 2, 17));
				importLineDocumentMock.Setup(x => x.AtHandFlag).Returns("J");
				importLineDocumentMock.Setup(x => x.WriteOff).Returns(IAmountEqualityComparerTest.AmountMock.Object);
				return importLineDocumentMock;
			}
		}
	}
}
