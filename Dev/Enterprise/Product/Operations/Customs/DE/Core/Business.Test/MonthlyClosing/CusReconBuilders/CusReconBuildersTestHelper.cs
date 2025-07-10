using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	internal static class CusReconBuildersTestHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		internal static string TestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Operations\Customs\DE\Core\Business.Test\MonthlyClosing\CusReconBuilders\TestFiles");

		internal static Mock<ICFCRECLine> CFCRECLine()
		{
			var mock = new Mock<ICFCRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns("TestDescription");
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.CessionManagementFlag).Returns("TestCessionManagementFlag");
			mock.Setup(x => x.TobaccoRevenueStampNumber).Returns("TestTobaccoRevenueStampNumber");
			mock.Setup(x => x.PreferentialTreatment).Returns(GetLinePreferentialTreatment(GetAmount("X", 11, "NAR")));
			mock.Setup(x => x.NetMassMeasure).Returns(1.0M);
			mock.Setup(x => x.OriginCountry).Returns("TestOriginCountry");
			mock.Setup(x => x.SupplementaryInformation).Returns("TestSupplementaryInformation");
			mock.Setup(x => x.CommodityCode).Returns("TestCommodityCode");
			mock.Setup(x => x.AdditionalProcedure).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.SupplementaryCodes).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(109513M);
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(1.0M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(1.0M);
			mock.Setup(x => x.AssessmentAmount).Returns(new[] { GetAmount("B", 9.999M, "TNE") });
			mock.Setup(x => x.AssessmentSpecificRate).Returns(new[] { GetImportSpecificRate("D", 11.22M) });
			mock.Setup(x => x.AssessmentContentInformation).Returns(new[] { GetContentInformation("F", 11.44M) });
			mock.Setup(x => x.ExciseDuty).Returns(new[] { GetExciseDuty("A123", 0.02M, 1626.28M) });
			mock.Setup(x => x.Documents).Returns(new[] { GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2020, 08, 12), "J") });
			mock.Setup(x => x.PreferentialOriginCountry).Returns("AU");
			mock.Setup(x => x.RequestedPreviousProcedure).Returns("4000");
			mock.Setup(m => m.Package).Returns(IImportPackageEqualityComparerTest.ImportPackageMock.Object);

			return mock;
		}

		internal static ICFCRECLine EmptyCFCRECLine()
		{
			var mock = new Mock<ICFCRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns(string.Empty);
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.NetMassMeasure).Returns(0m);
			mock.Setup(x => x.OriginCountry).Returns(string.Empty);
			mock.Setup(x => x.SupplementaryInformation).Returns(string.Empty);
			mock.Setup(x => x.CommodityCode).Returns(string.Empty);
			mock.Setup(x => x.AdditionalProcedure).Returns(Array.Empty<string>());
			mock.Setup(x => x.SupplementaryCodes).Returns(Array.Empty<string>());
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(1m);
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(0.1M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(0m);
			mock.Setup(x => x.AssessmentAmount).Returns(Array.Empty<IAmount>());
			mock.Setup(x => x.AssessmentSpecificRate).Returns(Array.Empty<IImportSpecificRate>());
			mock.Setup(x => x.AssessmentContentInformation).Returns(Array.Empty<IContentInformation>());
			mock.Setup(x => x.ExciseDuty).Returns(Array.Empty<IExciseDuty>());
			mock.Setup(x => x.Documents).Returns(Array.Empty<IImportLineDocument>());
			mock.Setup(x => x.RequestedPreviousProcedure).Returns(string.Empty);
			mock.Setup(x => x.Package).Returns((IImportPackage)null);
			mock.Setup(x => x.CessionManagementFlag).Returns((string)null);
			mock.Setup(x => x.TobaccoRevenueStampNumber).Returns(string.Empty);
			var subMock = new Mock<ILinePreferentialTreatment>();
			subMock.Setup(l => l.RequestedPreferentialTreatment).Returns(string.Empty);
			subMock.Setup(l => l.ContingentNumber).Returns(new[] { string.Empty, string.Empty });
			subMock.Setup(l => l.Quantity).Returns((IAmount)null);
			mock.Setup(x => x.PreferentialOriginCountry).Returns((string)null);
			mock.Setup(x => x.PreferentialTreatment).Returns(subMock.Object);

			return mock.Object;
		}

		internal static ICFCRECHeader CFCRECHeader()
		{
			var mock = new Mock<ICFCRECHeader>();
			mock.Setup(x => x.BorderTransportMeansInformation).Returns("TestBorderTransportMeansInformation");
			mock.Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("TestForeignTradeStatisticsInlandTransportMode");
			mock.Setup(x => x.BorderTransportMeansMode).Returns("TestBorderTransportMeansMode");
			mock.Setup(x => x.BorderTransportMeansType).Returns("TestBorderTransportMeansType");
			mock.Setup(x => x.BorderTransportMeansNationality).Returns("TestBorderTransportMeansNationality");
			mock.Setup(x => x.DepartureCountry).Returns("BE");
			return mock.Object;
		}

		internal static Mock<ICFCPEDLine> CFCPEDLineMock
		{
			get
			{
				var result = GetMonthlyClosingDecLineMock<ICFCPEDLine>();
				result.Setup(x => x.CessionManagementFlag).Returns("A");
				result.Setup(x => x.PreferentialOriginCountry).Returns("CN");
				result.Setup(x => x.TobaccoRevenueStampNumber).Returns("1234");
				result.Setup(x => x.AssessmentOutwardProcessingFee).Returns(1.23m);
				result.Setup(x => x.AssessmentTaxCosts).Returns(4.56m);
				result.Setup(x => x.PreferentialTreatment).Returns(ILinePreferentialTreatmentEqualityComparerTest.PreferentialTreatmentMock.Object);
				result.Setup(x => x.SpecialCase).Returns(new[] { ImportSpecialCaseMock.Object, ImportSpecialCaseMock.Object });
				return result;
			}
		}

		internal static Mock<ISCIPEDLine> SCIPEDLineMock
		{
			get
			{
				var result = GetMonthlyClosingDecLineMock<ISCIPEDLine>();
				result.Setup(x => x.RequestedPreferentialTreatment).Returns("A");
				result.Setup(x => x.InwardMovementAmount).Returns(IAmountEqualityComparerTest.AmountMock.Object);
				return result;
			}
		}

		internal static Mock<ISCIRECLine> SCIRECLine()
		{
			var mock = new Mock<ISCIRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns("TestDescription");
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.RequestedPreferentialTreatment).Returns("TestPreferentialTreatment");
			mock.Setup(x => x.InwardMovementAmount).Returns(GetAmount("test", 1M, "test"));
			mock.Setup(x => x.NetMassMeasure).Returns(1.0M);
			mock.Setup(x => x.OriginCountry).Returns("TestOriginCountry");
			mock.Setup(x => x.SupplementaryInformation).Returns("TestSupplementaryInformation");
			mock.Setup(x => x.CommodityCode).Returns("TestCommodityCode");
			mock.Setup(x => x.AdditionalProcedure).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.SupplementaryCodes).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(1.0M);
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(109513M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(1.0M);
			mock.Setup(x => x.AssessmentAmount).Returns(new[] { GetAmount("B", 9.999M, "TNE") });
			mock.Setup(x => x.AssessmentSpecificRate).Returns(new[] { GetImportSpecificRate("D", 11.22M) });
			mock.Setup(x => x.AssessmentContentInformation).Returns(new[] { GetContentInformation("F", 11.44M) });
			mock.Setup(x => x.ExciseDuty).Returns(new[] { GetExciseDuty("A123", 0.02M, 1626.28M) });
			mock.Setup(x => x.Documents).Returns(new[] { GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2020, 08, 12), "J") });
			mock.Setup(x => x.RequestedPreviousProcedure).Returns("4000");
			mock.Setup(m => m.Package).Returns(IImportPackageEqualityComparerTest.ImportPackageMock.Object);

			return mock;
		}

		internal static ISCIRECLine EmptySCIRECLine()
		{
			var mock = new Mock<ISCIRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns(string.Empty);
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.RequestedPreferentialTreatment).Returns(string.Empty);

			var subMock = new Mock<IAmount>();
			subMock.Setup(x => x.MeasurementUnit).Returns(string.Empty);
			subMock.Setup(x => x.Qualifier).Returns(string.Empty);
			subMock.Setup(x => x.Quantity).Returns(0M);
			mock.Setup(x => x.InwardMovementAmount).Returns(subMock.Object);
			mock.Setup(x => x.NetMassMeasure).Returns(0M);
			mock.Setup(x => x.OriginCountry).Returns(string.Empty);
			mock.Setup(x => x.SupplementaryInformation).Returns(string.Empty);
			mock.Setup(x => x.CommodityCode).Returns(string.Empty);
			mock.Setup(x => x.AdditionalProcedure).Returns(Array.Empty<string>());
			mock.Setup(x => x.SupplementaryCodes).Returns(Array.Empty<string>());
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(1M);
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(0.1M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(0M);
			mock.Setup(x => x.AssessmentAmount).Returns(Array.Empty<IAmount>());
			mock.Setup(x => x.AssessmentSpecificRate).Returns(Array.Empty<IImportSpecificRate>());
			mock.Setup(x => x.AssessmentContentInformation).Returns(Array.Empty<IContentInformation>());
			mock.Setup(x => x.ExciseDuty).Returns(Array.Empty<IExciseDuty>());
			mock.Setup(x => x.Documents).Returns(Array.Empty<IImportLineDocument>());
			mock.Setup(x => x.RequestedPreviousProcedure).Returns(string.Empty);
			mock.Setup(x => x.Package).Returns((IImportPackage)null);
			return mock.Object;
		}

		internal static ISCIRECHeader SCIRECHeader()
		{
			var mock = new Mock<ISCIRECHeader>();
			mock.Setup(x => x.BorderTransportMeansInformation).Returns("TestBorderTransportMeansInformation");
			mock.Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("TestForeignTradeStatisticsInlandTransportMode");
			mock.Setup(x => x.BorderTransportMeansMode).Returns("TestBorderTransportMeansMode");
			mock.Setup(x => x.BorderTransportMeansType).Returns("TestBorderTransportMeansType");
			mock.Setup(x => x.BorderTransportMeansNationality).Returns("TestBorderTransportMeansNationality");
			mock.Setup(x => x.DepartureCountry).Returns("BE");

			return mock.Object;
		}

		internal static Mock<ISCWPEDLine> SCWPEDLineMock
		{
			get
			{
				var result = GetMonthlyClosingDecLineMock<ISCWPEDLine>();
				result.Setup(x => x.RequestedPreferentialTreatment).Returns("200");
				result.Setup(x => x.InwardMovementAmount).Returns(GetAmount("X", 18219, "NAR"));
				result.Setup(x => x.ForeignTradeImportEarlyClearanceFlag).Returns("Y");
				return result;
			}
		}

		internal static Mock<ISCWRECLine> SCWRECLine()
		{
			var mock = new Mock<ISCWRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns("TestDescription");
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.RequestedPreferentialTreatment).Returns("TestPreferentialTreatment");
			mock.Setup(x => x.InwardMovementAmount).Returns(GetAmount("test", 1M, "test"));
			mock.Setup(x => x.NetMassMeasure).Returns(1.0M);
			mock.Setup(x => x.OriginCountry).Returns("TestOriginCountry");
			mock.Setup(x => x.SupplementaryInformation).Returns("TestSupplementaryInformation");
			mock.Setup(x => x.CommodityCode).Returns("TestCommodityCode");
			mock.Setup(x => x.AdditionalProcedure).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.SupplementaryCodes).Returns(new[] { "str1", "str2" });
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(109513M);
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(1.0M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(1.0M);
			mock.Setup(x => x.AssessmentAmount).Returns(new[] { GetAmount("B", 9.999M, "TNE") });
			mock.Setup(x => x.AssessmentSpecificRate).Returns(new[] { GetImportSpecificRate("D", 11.22M) });
			mock.Setup(x => x.AssessmentContentInformation).Returns(new[] { GetContentInformation("F", 11.44M) });
			mock.Setup(x => x.ExciseDuty).Returns(new[] { GetExciseDuty("A123", 0.02M, 1626.28M) });
			mock.Setup(x => x.Documents).Returns(new[] { GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2020, 08, 12), "J") });
			mock.Setup(x => x.RequestedPreviousProcedure).Returns("4000");
			mock.Setup(m => m.Package).Returns(IImportPackageEqualityComparerTest.ImportPackageMock.Object);

			return mock;
		}

		internal static ISCWRECLine EmptySCWRECLine()
		{
			var mock = new Mock<ISCWRECLine>();
			mock.Setup(x => x.GoodsDescription).Returns(string.Empty);
			mock.Setup(x => x.SequenceNumber).Returns(2);
			mock.Setup(x => x.RequestedPreferentialTreatment).Returns((string)null);

			var subMock = new Mock<IAmount>();
			subMock.Setup(x => x.MeasurementUnit).Returns(string.Empty);
			subMock.Setup(x => x.Qualifier).Returns(string.Empty);
			subMock.Setup(x => x.Quantity).Returns(0M);
			mock.Setup(x => x.InwardMovementAmount).Returns(subMock.Object);
			mock.Setup(x => x.NetMassMeasure).Returns(0M);
			mock.Setup(x => x.OriginCountry).Returns(string.Empty);
			mock.Setup(x => x.SupplementaryInformation).Returns(string.Empty);
			mock.Setup(x => x.CommodityCode).Returns(string.Empty);
			mock.Setup(x => x.AdditionalProcedure).Returns(Array.Empty<string>());
			mock.Setup(x => x.SupplementaryCodes).Returns(Array.Empty<string>());
			mock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(1M);
			mock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(0.1M);
			mock.Setup(x => x.AssessmentCustomsValue).Returns(0M);
			mock.Setup(x => x.AssessmentAmount).Returns(Array.Empty<IAmount>());
			mock.Setup(x => x.AssessmentSpecificRate).Returns(Array.Empty<IImportSpecificRate>());
			mock.Setup(x => x.AssessmentContentInformation).Returns(Array.Empty<IContentInformation>());
			mock.Setup(x => x.ExciseDuty).Returns(Array.Empty<IExciseDuty>());
			mock.Setup(x => x.Documents).Returns(Array.Empty<IImportLineDocument>());
			mock.Setup(x => x.RequestedPreviousProcedure).Returns(string.Empty);
			mock.Setup(x => x.Package).Returns((IImportPackage)null);
			return mock.Object;
		}

		internal static ISCWRECHeader SCWRECHeader()
		{
			var mock = new Mock<ISCWRECHeader>();
			mock.Setup(x => x.BorderTransportMeansInformation).Returns("TestBorderTransportMeansInformation");
			mock.Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("TestForeignTradeStatisticsInlandTransportMode");
			mock.Setup(x => x.BorderTransportMeansMode).Returns("TestBorderTransportMeansMode");
			mock.Setup(x => x.BorderTransportMeansType).Returns("TestBorderTransportMeansType");
			mock.Setup(x => x.BorderTransportMeansNationality).Returns("TestBorderTransportMeansNationality");
			mock.Setup(x => x.DepartureCountry).Returns("BE");
			mock.Setup(x => x.ForeignTradeImportEarlyClearanceFlag).Returns("J");

			return mock.Object;
		}

		internal static ILinePreferentialTreatment GetLinePreferentialTreatment(IAmount quantity)
		{
			var mock = new Mock<ILinePreferentialTreatment>();
			mock.Setup(l => l.RequestedPreferentialTreatment).Returns("200");
			mock.Setup(l => l.ContingentNumber).Returns(new[] { "1XYZ" });
			mock.Setup(l => l.Quantity).Returns(quantity);

			return mock.Object;
		}

		internal static IImportSpecificRate GetImportSpecificRate(string type, decimal value)
		{
			var mock = new Mock<IImportSpecificRate>();
			mock.Setup(r => r.Type).Returns(type);
			mock.Setup(r => r.Value).Returns(value);

			return mock.Object;
		}

		internal static IContentInformation GetContentInformation(string contentType, decimal degreePercentage)
		{
			var mock = new Mock<IContentInformation>();
			mock.Setup(r => r.ContentType).Returns(contentType);
			mock.Setup(r => r.DegreePercentage).Returns(degreePercentage);

			return mock.Object;
		}

		internal static IExciseDuty GetExciseDuty(string code, decimal degreePercentage, decimal value)
		{
			var mock = new Mock<IExciseDuty>();
			mock.Setup(d => d.Code).Returns(code);
			mock.Setup(d => d.DegreePercentage).Returns(degreePercentage);
			mock.Setup(d => d.Value).Returns(value);
			mock.Setup(d => d.Amount).Returns(GetAmount("X", 11.111M, "NAR"));

			return mock.Object;
		}

		internal static IImportLineDocument GetImportLineDocument(string division, string documentType, string referenceNumber, DateTime? issuingDate, string atHandFlag)
		{
			var mock = new Mock<IImportLineDocument>();
			mock.Setup(d => d.Division).Returns(division);
			mock.Setup(d => d.DocumentType).Returns(documentType);
			mock.Setup(d => d.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(d => d.IssuingDate).Returns(issuingDate);
			mock.Setup(d => d.AtHandFlag).Returns(atHandFlag);
			mock.Setup(d => d.WriteOff).Returns(GetAmount("Z", 11.888M, "NAR"));

			return mock.Object;
		}

		internal static IAmount GetAmount(string qualifier, decimal quantity, string measurementUnit)
		{
			var mock = new Mock<IAmount>();
			mock.Setup(x => x.Qualifier).Returns(qualifier);
			mock.Setup(x => x.Quantity).Returns(quantity);
			mock.Setup(x => x.MeasurementUnit).Returns(measurementUnit);
			return mock.Object;
		}

		internal static IImportDocument GetImportDocument(string type, string referenceNumber, DateTime? issuingDate)
		{
			var mock = new Mock<IImportDocument>();
			mock.Setup(d => d.Type).Returns(type);
			mock.Setup(d => d.ReferenceNumber).Returns(referenceNumber);
			mock.Setup(d => d.IssuingDate).Returns(issuingDate);

			return mock.Object;
		}

		internal static ILinePreferentialTreatment GetPreferentialTreatment(IAmount quantity, string requestedPreferentialTreatment = "200", string[] contingentNumber = null)
		{
			var convertCcontingentNumber = new string[contingentNumber?.Length ?? 0];
			var index = 0;
			contingentNumber.ForEach(x => convertCcontingentNumber[index++] = x);

			var mock = new Mock<ILinePreferentialTreatment>();
			mock.Setup(l => l.RequestedPreferentialTreatment).Returns(requestedPreferentialTreatment);
			mock.Setup(l => l.ContingentNumber).Returns(contingentNumber == null ? new[] { "1XYZ" } : convertCcontingentNumber);
			mock.Setup(l => l.Quantity).Returns(quantity);

			return mock.Object;
		}

		internal static Mock<IAdditionDeduction> AdditionDeductionMock
		{
			get
			{
				var result = new Mock<IAdditionDeduction>();
				result.Setup(x => x.Type).Returns("T");
				result.Setup(x => x.CurrencyRateIATA).Returns(true);
				result.Setup(x => x.CurrencyRateDate).Returns(new DateTime(1988, 2, 11));
				result.Setup(x => x.Percentage).Returns(11.2m);
				return result;
			}
		}

		internal static Mock<IAirFreightCosts> AirFreightCostsMock
		{
			get
			{
				var result = new Mock<IAirFreightCosts>();
				result.Setup(x => x.CurrencyRateIATA).Returns(true);
				result.Setup(x => x.CurrencyRateDate).Returns(new DateTime(1988, 2, 11));
				return result;
			}
		}

		internal static Mock<IImportCosts> ImportCostsMock
		{
			get
			{
				var result = new Mock<IImportCosts>();
				result.Setup(x => x.CurrencyRateAgreedFlag).Returns(true);
				result.Setup(x => x.CurrencyRate).Returns(9.999m);
				return result;
			}
		}

		internal static Mock<IImportLineCustomsValue> ImportLineCustomsValueMock
		{
			get
			{
				var result = new Mock<IImportLineCustomsValue>();
				result.Setup(x => x.CustomsValueDepartureAirport).Returns("FRA");
				result.Setup(x => x.CustomsValueDestinationPlace).Returns("Berlin");
				result.Setup(x => x.CustomsValueAdditionDeductionDescription).Returns("Description");
				result.Setup(x => x.CustomsValueNetPrice).Returns(ImportCostsMock.Object);
				result.Setup(x => x.CustomsValueIndirectPayment).Returns(ImportCostsMock.Object);
				result.Setup(x => x.CustomsValueAirFreightCosts).Returns(AirFreightCostsMock.Object);
				result.Setup(x => x.CustomsValueAdditionDeduction).Returns(new[] { AdditionDeductionMock.Object, AdditionDeductionMock.Object });
				return result;
			}
		}

		internal static Mock<IImportSpecialCase> ImportSpecialCaseMock
		{
			get
			{
				var result = new Mock<IImportSpecialCase>();
				result.Setup(x => x.Group).Returns("A");
				result.Setup(x => x.ApplicationType).Returns("T");
				result.Setup(x => x.RateOrAmountOrFactor).Returns(1.23m);
				return result;
			}
		}

		internal static Mock<T> GetMonthlyClosingDecLineMock<T>()
			where T : class, IMonthlyClosingDecLine
		{
			{
				var amount = GetAmount("X", 18219, "NAR");
				var importSpecificRate = GetImportSpecificRate("S", 10.02m);
				var contentInformation = GetContentInformation("L", 0.01m);
				var exciseDuty = GetExciseDuty("A123", 0.02m, 1626.28m);
				var importLineDocument = GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2021, 8, 12), "J");
				var result = new Mock<T>();
				result.Setup(x => x.SequenceNumber).Returns(1);
				result.Setup(x => x.ReferencedSequenceNumber).Returns(2);
				result.Setup(x => x.MatterCode).Returns("MC");
				result.Setup(x => x.ArticleNumber).Returns("ARTICLE123");
				result.Setup(x => x.InvoiceAmount).Returns(1.23m);
				result.Setup(x => x.DepartureCountry).Returns("CN");
				result.Setup(x => x.CompleteDeclarationFlag).Returns(true);
				result.Setup(x => x.ForeignTradeStatisticsGoodsStatus).Returns("GS");
				result.Setup(x => x.ForeignTradeStatisticsTransactionType).Returns("TT");
				result.Setup(x => x.ForeignTradeStatisticsDestinationCountry).Returns("DE");
				result.Setup(x => x.ForeignTradeStatisticsDestinationFederalState).Returns("RLP");
				result.Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("AIR");
				result.Setup(x => x.ForeignTradeStatisticsAmount).Returns(amount);
				result.Setup(x => x.CustomsValue).Returns(ImportLineCustomsValueMock.Object);
				result.Setup(x => x.BorderTransportMeansMode).Returns("RAI");
				result.Setup(x => x.BorderTransportMeansType).Returns("MT");
				result.Setup(x => x.BorderTransportMeansInformation).Returns("FrenchTruck");
				result.Setup(x => x.BorderTransportMeansNationality).Returns("FR");
				result.Setup(x => x.NetMassMeasure).Returns(11866.4m);
				result.Setup(x => x.OriginCountry).Returns("CN");
				result.Setup(x => x.SupplementaryInformation).Returns("Positionszusatz");
				result.Setup(x => x.CommodityCode).Returns("62034311000");
				result.Setup(x => x.AdditionalProcedure).Returns(new[] { "C34", "D35" });
				result.Setup(x => x.SupplementaryCodes).Returns(new[] { "A12", "B13" });
				result.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(109513m);
				result.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.45m);
				result.Setup(x => x.AssessmentCustomsValue).Returns(100628.6m);
				result.Setup(x => x.AssessmentAmount).Returns(new[] { amount, amount });
				result.Setup(x => x.AssessmentSpecificRate).Returns(new[] { importSpecificRate, importSpecificRate });
				result.Setup(x => x.AssessmentContentInformation).Returns(new[] { contentInformation, contentInformation });
				result.Setup(x => x.ExciseDuty).Returns(new[] { exciseDuty, exciseDuty });
				result.Setup(x => x.Documents).Returns(new[] { importLineDocument, importLineDocument });
				return result;
			}
		}

		internal static Amount CreateAmountForBizo(string qualifier, decimal quantity, string measurementUnit)
		{
			return new Amount() { Qualifier = qualifier, Quantity = quantity, MeasurementUnit = measurementUnit };
		}

		internal static DEMonthlyClosingEntryLineSnapshotExciseDuty CreateDEMonthlyClosingEntryLineSnapshotExciseDutyForBizo(string code, decimal degreePercentage, decimal value, Amount amount)
		{
			return new DEMonthlyClosingEntryLineSnapshotExciseDuty()
			{
				Code = code,
				DegreePercentage = degreePercentage,
				DegreePercentageSpecified = !degreePercentage.IsZero(),
				Value = value,
				ValueSpecified = !value.IsZero(),
				Amount = amount
			};
		}

		internal static DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation CreateDEMonthlyClosingEntryLineSnapshotAssessmentContentInformationForBizo(string type, decimal degreePercentage)
		{
			return new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation() { Type = type, DegreePercentage = degreePercentage };
		}

		internal static DEMonthlyClosingEntryLineSnapshotDocument CreateDEMonthlyClosingEntryLineSnapshotDocumentForBizo(string division, string type, string referenceNumber, ZDate dateTime, string atHandFlag, Amount writeOff)
		{
			return new DEMonthlyClosingEntryLineSnapshotDocument()
			{
				Division = division.MapCodeToEnumWithItemPrefix<DEMonthlyClosingEntryLineSnapshotDocumentDivision>().EnumValue,
				Type = type,
				ReferenceNumber = referenceNumber,
				IssuingDate = dateTime.SafeDate(),
				IssuingDateSpecified = !dateTime.IsEmpty,
				AtHandFlag = atHandFlag,
				WriteOff = writeOff
			};
		}

		internal static DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate CreateDEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateForBizo(string type, decimal value)
		{
			return new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate() { Type = type, Value = value };
		}
	}
}
