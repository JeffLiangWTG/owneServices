using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IImportDecLineEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, importDecLineEqualityComparer.GetHashCode(originalImportDecLine));
		}

		public void TestEquals()
		{
			AssertEquals(true, importDecLineEqualityComparer.Equals(originalImportDecLine, GetImportDecLine<IImportDecLine>().Object));
		}

		public void TestEquals_SequenceNumber()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.SequenceNumber).Returns(2);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_RequestedPreviousProcedure()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.RequestedPreviousProcedure).Returns("3000");
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_GoodsDescription()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.GoodsDescription).Returns("Latzhosen und kurze Hosen");
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_NetMassMeasure()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.NetMassMeasure).Returns(9877.48m);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_OriginCountry()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.OriginCountry).Returns("NZ");
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_SupplementaryInformation()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.SupplementaryInformation).Returns("NJ000001");
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_CommodityCode()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.CommodityCode).Returns("38023399836");
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_ForeignTradeStatisticsQuantity()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(19732m);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_ForeignTradeStatisticsGrossMassMeasure()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(10234.47m);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_AssessmentCustomsValue()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.AssessmentCustomsValue).Returns(102784.32m);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_SupplementaryCodes()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.SupplementaryCodes).Returns(new[] { "A12", "A13" });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_AdditionalProcedure()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.AdditionalProcedure).Returns(new[] { "C35", "D35" });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_Package()
		{
			var importPackageMock = IImportPackageEqualityComparerTest.ImportPackageMock;
			importPackageMock.Setup(p => p.MarksNumbers).Returns("Import Dec changed package");
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.Package).Returns(importPackageMock.Object);
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_AssessmentAmount()
		{
			var amountMock = GetAmountMock(18000, "NAR", "X");
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.AssessmentAmount).Returns(new IAmount[] { amountMock.Object, IAmountEqualityComparerTest.AmountMock.Object });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_AssessmentSpecificRate()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { GetRateMock("R", 9.34m).Object, GetRateMock("S", 10.02m).Object });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_AssessmentContentInformation()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.AssessmentContentInformation).Returns(new IContentInformation[] { GetContentInformationMock("L", 1.02m).Object, GetContentInformationMock("M", 1.02m).Object });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_ExciseDuty()
		{
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.ExciseDuty).Returns(new IExciseDuty[] { GetExciseDutyMock("A123", 0.02m, 1627.28m).Object, GetExciseDutyMock("B456", 0.05m, 974.32m).Object });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		public void TestEquals_Documents()
		{
			var importLineDocument1 = GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2022, 2, 18), "J").Object;
			var importLineDocument2 = GetImportLineDocument("3", "8JRN", "PULO6923659237", new DateTime(2021, 9, 18), "N").Object;
			var importDecLine = GetImportDecLine<IImportDecLine>();
			importDecLine.Setup(x => x.Documents).Returns(new IImportLineDocument[] { importLineDocument1, importLineDocument2 });
			AssertEquals(false, importDecLineEqualityComparer.Equals(originalImportDecLine, importDecLine.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalImportDecLine = GetImportDecLine<IImportDecLine>().Object;
			importDecLineEqualityComparer = new IImportDecLineEqualityComparer();
		}
		IImportDecLine originalImportDecLine;
		IImportDecLineEqualityComparer importDecLineEqualityComparer;

		internal static Mock<T> GetImportDecLine<T>()
			where T : class, IImportDecLine
		{
			var importDecLineMock = new Mock<T>();
			importDecLineMock.Setup(x => x.SequenceNumber).Returns(1);
			importDecLineMock.Setup(x => x.RequestedPreviousProcedure).Returns("4000");
			importDecLineMock.Setup(x => x.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken");
			importDecLineMock.Setup(x => x.NetMassMeasure).Returns(11866.4m);
			importDecLineMock.Setup(x => x.OriginCountry).Returns("CN");
			importDecLineMock.Setup(x => x.SupplementaryInformation).Returns("Positionszusatz");
			importDecLineMock.Setup(x => x.CommodityCode).Returns("62034311000");
			importDecLineMock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(109513m);
			importDecLineMock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.45m);
			importDecLineMock.Setup(x => x.AssessmentCustomsValue).Returns(100628.6m);
			importDecLineMock.Setup(x => x.SupplementaryCodes).Returns(new[] { "A12", "B13" });
			importDecLineMock.Setup(x => x.AdditionalProcedure).Returns(new[] { "C34", "D35" });
			importDecLineMock.Setup(x => x.Package).Returns(IImportPackageEqualityComparerTest.ImportPackageMock.Object);
			importDecLineMock.Setup(x => x.AssessmentAmount).Returns(new IAmount[] { GetAmountMock(18219, "NAR", "X").Object, IAmountEqualityComparerTest.AmountMock.Object });
			importDecLineMock.Setup(x => x.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { GetRateMock("S", 10.02m).Object, GetRateMock("T", 9.34m).Object });
			importDecLineMock.Setup(x => x.AssessmentContentInformation).Returns(new IContentInformation[] { GetContentInformationMock("L", 0.01m).Object, GetContentInformationMock("M", 1.02m).Object });
			importDecLineMock.Setup(x => x.ExciseDuty).Returns(new IExciseDuty[] { GetExciseDutyMock("A123", 0.02m, 1626.28m).Object, GetExciseDutyMock("B456", 0.05m, 974.32m).Object });
			var importLineDocument1 = GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2021, 8, 12), "J").Object;
			var importLineDocument2 = GetImportLineDocument("3", "8JRN", "PULO6923659237", new DateTime(2021, 9, 18), "N").Object;
			importDecLineMock.Setup(x => x.Documents).Returns(new IImportLineDocument[] { importLineDocument1, importLineDocument2 });
			return importDecLineMock;
		}

		internal static Mock<IAmount> GetAmountMock(decimal quantity, string measurementUnit, string qualifier)
		{
			var amountMock = new Mock<IAmount>();
			amountMock.Setup(a => a.Quantity).Returns(quantity);
			amountMock.Setup(a => a.MeasurementUnit).Returns(measurementUnit);
			amountMock.Setup(a => a.Qualifier).Returns(qualifier);
			return amountMock;
		}

		static Mock<IImportSpecificRate> GetRateMock(string type, decimal value)
		{
			var rateMock = new Mock<IImportSpecificRate>();
			rateMock.Setup(a => a.Type).Returns(type);
			rateMock.Setup(a => a.Value).Returns(value);
			return rateMock;
		}

		static Mock<IContentInformation> GetContentInformationMock(string contentType, decimal degreePercentage)
		{
			var contentInformationMock = new Mock<IContentInformation>();
			contentInformationMock.Setup(c => c.ContentType).Returns(contentType);
			contentInformationMock.Setup(c => c.DegreePercentage).Returns(degreePercentage);
			return contentInformationMock;
		}

		static Mock<IExciseDuty> GetExciseDutyMock(string code, decimal degreePercentage, decimal value)
		{
			var exciseDutyMock = new Mock<IExciseDuty>();
			exciseDutyMock.Setup(x => x.Code).Returns(code);
			exciseDutyMock.Setup(x => x.DegreePercentage).Returns(degreePercentage);
			exciseDutyMock.Setup(x => x.Value).Returns(value);
			exciseDutyMock.Setup(x => x.Amount).Returns(IAmountEqualityComparerTest.AmountMock.Object);
			return exciseDutyMock;
		}

		static Mock<IImportLineDocument> GetImportLineDocument(string division, string documentType, string referenceNumber, DateTime issuingDate, string atHandFlag)
		{
			var importLineDocumentMock = new Mock<IImportLineDocument>();
			importLineDocumentMock.Setup(d => d.Division).Returns(division);
			importLineDocumentMock.Setup(d => d.DocumentType).Returns(documentType);
			importLineDocumentMock.Setup(d => d.ReferenceNumber).Returns(referenceNumber);
			importLineDocumentMock.Setup(d => d.IssuingDate).Returns(issuingDate);
			importLineDocumentMock.Setup(d => d.AtHandFlag).Returns(atHandFlag);
			importLineDocumentMock.Setup(d => d.WriteOff).Returns(IAmountEqualityComparerTest.AmountMock.Object);
			return importLineDocumentMock;
		}
	}
}
