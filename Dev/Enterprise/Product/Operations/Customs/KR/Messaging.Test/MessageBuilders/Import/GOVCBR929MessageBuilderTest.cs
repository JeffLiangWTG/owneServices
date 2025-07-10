using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR929MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportEntryHeader> importHeaderMock;
		Mock<IImportEntryLine> entryLine1Mock;

		protected override void SetUp()
		{
			base.SetUp();

			#region IImportEntryHeader
			importHeaderMock = new Mock<IImportEntryHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("4062001070010U");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("20");
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationIndicator).Returns(true);
			importHeaderMock.Setup(m => m.CargoManagementNo).Returns("01KE0766SS20010003");
			importHeaderMock.Setup(m => m.ArrivalDateAtDischargePort).Returns(new ZDate("2012-01-01"));
			importHeaderMock.Setup(m => m.PaymentType).Returns("13");
			importHeaderMock.Setup(m => m.ImportTypeCode).Returns("A");
			importHeaderMock.Setup(m => m.TradeType).Returns("11");
			importHeaderMock.Setup(m => m.DeclarationProcedureType).Returns("L");
			importHeaderMock.Setup(m => m.TotalGrossWeightInKG).Returns(95.2m);
			importHeaderMock.Setup(m => m.PackType).Returns("CT");
			importHeaderMock.Setup(m => m.TransportMode).Returns("4");
			importHeaderMock.Setup(m => m.ContainerPackMode).Returns("ETC");
			importHeaderMock.Setup(m => m.DepartureCountryCode).Returns("PR");
			importHeaderMock.Setup(m => m.BondedAreaCode).Returns("13011013");
			importHeaderMock.Setup(m => m.TotalInvoiceAmount).Returns(1000000m);
			importHeaderMock.Setup(m => m.InvoiceAmountCurrency).Returns("USD");
			importHeaderMock.Setup(m => m.InvoicePaymentTerm).Returns("DA");
			importHeaderMock.Setup(m => m.ExchangeRate).Returns(1210.12m);
			importHeaderMock.Setup(m => m.TotalDutyAmount).Returns(999999999999m);
			importHeaderMock.Setup(m => m.ApplicationForAgreedRate).Returns(new ZBool("Y"));
			#endregion

			#region IImportEntryLine
			entryLine1Mock = new Mock<IImportEntryLine>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(001);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("TACKS");
			entryLine1Mock.Setup(m => m.ModelName).Returns("RABBIT MEAT");
			entryLine1Mock.Setup(m => m.BrandCode).Returns("ZZZZ");
			entryLine1Mock.Setup(m => m.BrandName).Returns("상표명");
			entryLine1Mock.Setup(m => m.HSCode).Returns("0208100000");
			entryLine1Mock.Setup(m => m.CountryOfOrigin).Returns("CN");
			entryLine1Mock.Setup(m => m.CountryOfOriginDeterminationRule).Returns("12");
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelLocation).Returns("Y");
			entryLine1Mock.Setup(m => m.QuantityUQToClaimRefund).Returns("PC");
			entryLine1Mock.Setup(m => m.DutyRateCode).Returns("x");
			entryLine1Mock.Setup(m => m.DutyRateTypeCode).Returns("1");
			entryLine1Mock.Setup(m => m.DutyAmount).Returns(999999999999m);
			#endregion

		}
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2013, 01, 01)]
		public void TestGenerateDeclaration()
		{
			#region Setup(Conditional)
			#region IImportEntryHeader
			importHeaderMock.Setup(m => m.HouseBillNumber).Returns("DBSC96100123AB01");
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonCode).Returns("A");
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonDescription).Returns("분할기타사유");
			importHeaderMock.Setup(m => m.UnderbondMovementArrivalDate).Returns(new ZDate("2012-01-01"));
			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns("세관무역㈜");
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns("ABCD");
			importHeaderMock.Setup(m => m.DeclarationPlanCode).Returns("D");
			importHeaderMock.Setup(m => m.CertificateOfOriginIssued).Returns("Y");
			importHeaderMock.Setup(m => m.ValueDeclarationAttached).Returns("Y");
			importHeaderMock.Setup(m => m.TotalPackQty).Returns(2);
			importHeaderMock.Setup(m => m.ArrivalPort).Returns("KRPUS");
			importHeaderMock.Setup(m => m.VesselOrFlightNo).Returns("KE1098");
			importHeaderMock.Setup(m => m.VesselCountryCode).Returns("KR");
			importHeaderMock.Setup(m => m.MasterBillNumber).Returns("HJSC98100123AB01");
			importHeaderMock.Setup(m => m.CarrierID).Returns("HJSC");
			importHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns("가1A123456");
			importHeaderMock.Setup(m => m.Incoterm).Returns("CFR");
			importHeaderMock.Setup(m => m.TotalCustomsValueUSD).Returns(9999999999m);
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(999999999999m);
			importHeaderMock.Setup(m => m.Freight).Returns(1000m);
			importHeaderMock.Setup(m => m.Insurance).Returns(1000m);
			importHeaderMock.Setup(m => m.AdditionalAmount).Returns(999999999999m);
			importHeaderMock.Setup(m => m.DeductedAmount).Returns(999999999999m);
			importHeaderMock.Setup(m => m.CourierCompanyID).Returns("HJSC");
			importHeaderMock.Setup(m => m.AuthorizedImporterRegNo).Returns("1234567890");
			importHeaderMock.Setup(m => m.OwnerReferenceNumber).Returns("123456789");
			importHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns("Y");
			importHeaderMock.Setup(m => m.GoldTradeTransactionYN).Returns("Y");
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns("B");
			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(new ZDateTime("2012-02-10 10:12:00"));
			importHeaderMock.Setup(m => m.TotalSpecialConsumptionTax).Returns(100m);
			importHeaderMock.Setup(m => m.TotalTransportationTax).Returns(100m);
			importHeaderMock.Setup(m => m.TotalLiquorTax).Returns(100m);
			importHeaderMock.Setup(m => m.TotalEducationTax).Returns(100m);
			importHeaderMock.Setup(m => m.TotalAgricultureTax).Returns(100m);
			importHeaderMock.Setup(m => m.TotalVAT).Returns(999999999999m);
			importHeaderMock.Setup(m => m.TotalValueForVAT).Returns(999999999999m);
			importHeaderMock.Setup(m => m.TotalVATExemptionValue).Returns(999999999999m);
			importHeaderMock.Setup(m => m.PenaltyForLateDeclaration).Returns(999999999999m);
			importHeaderMock.Setup(m => m.PenaltyForMissedDeclaration).Returns(999999999999m);
			importHeaderMock.Setup(m => m.TotalPayableAmount).Returns(1234m);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode1).Returns("A");
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode2).Returns("A");
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode3).Returns("A");
			importHeaderMock.Setup(m => m.CustomsBrokerComment1).Returns("기재사항1");
			importHeaderMock.Setup(m => m.CustomsBrokerComment2).Returns("기재사항2");
			importHeaderMock.Setup(m => m.BlanketValuationDeclarationNumber).Returns("AAAAAAAAAAAA");
			#endregion

			#region IImportEntryLine
			entryLine1Mock.Setup(m => m.AdditionalTariffCode).Returns("0208100000-01-101");
			entryLine1Mock.Setup(m => m.SpecificUseCodeDutyRatePermitNo).Returns("123456789");
			entryLine1Mock.Setup(m => m.CertificateOfOriginNo).Returns("123456789");
			entryLine1Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns("01");
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2013-01-01"));
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssuingCountry).Returns("KR");
			entryLine1Mock.Setup(m => m.CertificateOfOriginAgencyName).Returns("발행기관명");
			entryLine1Mock.Setup(m => m.CertificateOfOriginAreaName).Returns("발급지역명");
			entryLine1Mock.Setup(m => m.CertificateOfOriginPersonName).Returns("발급담당자명");
			entryLine1Mock.Setup(m => m.CertificateOfOriginSplitIndicator).Returns("Y");
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelType).Returns("03");
			entryLine1Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns("14");
			entryLine1Mock.Setup(m => m.ProductOrMaterialCode).Returns("7");
			entryLine1Mock.Setup(m => m.MaterialLineNo).Returns(123);
			entryLine1Mock.Setup(m => m.QuantityToClaimRefund).Returns(123m);
			entryLine1Mock.Setup(m => m.MightRequireInspectionIndicator).Returns("Y");
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency1).Returns("023");
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns("019");
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns("020");
			entryLine1Mock.Setup(m => m.NetWeightInKG).Returns(99999.9m);
			entryLine1Mock.Setup(m => m.Quantity).Returns(30m);
			entryLine1Mock.Setup(m => m.QuantityUnit).Returns("DZ");
			entryLine1Mock.Setup(m => m.CustomsValueKRW).Returns(1000m);
			entryLine1Mock.Setup(m => m.CustomsValueUSD).Returns(100);
			entryLine1Mock.Setup(m => m.CourierCargoSelectivityIndicator).Returns("Y");
			entryLine1Mock.Setup(m => m.AdValoremDutyRate).Returns(99.99m);
			entryLine1Mock.Setup(m => m.AdditionalDutyRate).Returns(99.99m);
			entryLine1Mock.Setup(m => m.AdditionalDutyCode).Returns("I");
			entryLine1Mock.Setup(m => m.DutyReductionClassification).Returns("AA");
			entryLine1Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns("A028040100");
			entryLine1Mock.Setup(m => m.DutyReductionRate).Returns(99.99m);
			entryLine1Mock.Setup(m => m.DutyReductionAmount).Returns(2001020m);
			entryLine1Mock.Setup(m => m.DomesticTaxClassification).Returns("x");
			entryLine1Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns("0000001");
			entryLine1Mock.Setup(m => m.DomesticTaxCode).Returns("AA");
			entryLine1Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns("AA");
			entryLine1Mock.Setup(m => m.DomesticTaxRate).Returns(99.99m);
			entryLine1Mock.Setup(m => m.SpecialConsumptionTax).Returns(999999999999m);
			entryLine1Mock.Setup(m => m.VATRateCode).Returns("x");
			entryLine1Mock.Setup(m => m.VATReductionCode).Returns("11");
			entryLine1Mock.Setup(m => m.ValueForVAT).Returns(11m);
			entryLine1Mock.Setup(m => m.ValueExemptForVAT).Returns(11m);
			entryLine1Mock.Setup(m => m.VATAmount).Returns(999999999999m);
			entryLine1Mock.Setup(m => m.EducationTaxExemptIndicator).Returns("x");
			entryLine1Mock.Setup(m => m.EducationTaxAmount).Returns(999999999999m);
			entryLine1Mock.Setup(m => m.AgricultureTaxClassification).Returns("x");
			entryLine1Mock.Setup(m => m.AgricultureTax).Returns(999999999999m);
			entryLine1Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(999999999999m);

			var entryLine2Mock = new Mock<IImportEntryLine>();
			entryLine2Mock.Setup(m => m.EntryLineNo).Returns(002);
			entryLine2Mock.Setup(m => m.HSDescription).Returns("TACKS2");
			entryLine2Mock.Setup(m => m.ModelName).Returns("RABBIT MEAT2");
			entryLine2Mock.Setup(m => m.BrandCode).Returns("ZZZZ2");
			entryLine2Mock.Setup(m => m.BrandName).Returns("상표명2");
			entryLine2Mock.Setup(m => m.HSCode).Returns("0208122222");
			entryLine2Mock.Setup(m => m.CountryOfOrigin).Returns("CN");
			entryLine2Mock.Setup(m => m.CountryOfOriginDeterminationRule).Returns("11");
			entryLine2Mock.Setup(m => m.CountryOfOriginLabelLocation).Returns("Y");
			entryLine2Mock.Setup(m => m.DutyRateCode).Returns("x");
			entryLine2Mock.Setup(m => m.DutyRateTypeCode).Returns("1");
			entryLine2Mock.Setup(m => m.DutyAmount).Returns(999999999999m);
			entryLine2Mock.Setup(m => m.NonGADetails).Returns(System.Array.Empty<IImportNonGADetail>());
			entryLine2Mock.Setup(m => m.PreviousExpDecLines).Returns(System.Array.Empty<IImportPreviousExpDecLine>());
			entryLine2Mock.Setup(m => m.InvoiceLines).Returns(System.Array.Empty<IImportInvoiceLine>());

			entryLine2Mock.Setup(m => m.AdditionalTariffCode).Returns("0208100000-01-202");
			entryLine2Mock.Setup(m => m.SpecificUseCodeDutyRatePermitNo).Returns("987654321");
			entryLine2Mock.Setup(m => m.CertificateOfOriginNo).Returns("987654321");
			entryLine2Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns("02");
			entryLine2Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2013-02-02"));
			entryLine2Mock.Setup(m => m.CertificateOfOriginIssuingCountry).Returns("KR");
			entryLine2Mock.Setup(m => m.CertificateOfOriginAgencyName).Returns("발행기관명2");
			entryLine2Mock.Setup(m => m.CertificateOfOriginAreaName).Returns("발급지역명2");
			entryLine2Mock.Setup(m => m.CertificateOfOriginPersonName).Returns("발급담당자명2");
			entryLine2Mock.Setup(m => m.CertificateOfOriginSplitIndicator).Returns("Y");
			entryLine2Mock.Setup(m => m.CountryOfOriginLabelType).Returns("03");
			entryLine2Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns("12");
			entryLine2Mock.Setup(m => m.ProductOrMaterialCode).Returns("8");
			entryLine2Mock.Setup(m => m.MaterialLineNo).Returns(123);
			entryLine2Mock.Setup(m => m.QuantityUQToClaimRefund).Returns("PC");
			entryLine2Mock.Setup(m => m.QuantityToClaimRefund).Returns(123m);
			entryLine2Mock.Setup(m => m.MightRequireInspectionIndicator).Returns("Y");
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency1).Returns("024");
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns("020");
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns("021");
			entryLine2Mock.Setup(m => m.NetWeightInKG).Returns(99999.9m);
			entryLine2Mock.Setup(m => m.QuantityUnit).Returns("DZ");
			entryLine2Mock.Setup(m => m.Quantity).Returns(30m);
			entryLine2Mock.Setup(m => m.CustomsValueKRW).Returns(1000m);
			entryLine2Mock.Setup(m => m.CustomsValueUSD).Returns(100);
			entryLine2Mock.Setup(m => m.CourierCargoSelectivityIndicator).Returns("Y");
			entryLine2Mock.Setup(m => m.AdValoremDutyRate).Returns(99.99m);
			entryLine2Mock.Setup(m => m.AdditionalDutyRate).Returns(99.99m);
			entryLine2Mock.Setup(m => m.AdditionalDutyCode).Returns("I");
			entryLine2Mock.Setup(m => m.DutyReductionClassification).Returns("AA");
			entryLine2Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns("A028040122");
			entryLine2Mock.Setup(m => m.DutyReductionRate).Returns(99.99m);
			entryLine2Mock.Setup(m => m.DutyReductionAmount).Returns(2001020m);
			entryLine2Mock.Setup(m => m.DomesticTaxClassification).Returns("x");
			entryLine2Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns("0000002");
			entryLine2Mock.Setup(m => m.DomesticTaxCode).Returns("AA");
			entryLine2Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns("AA");
			entryLine2Mock.Setup(m => m.DomesticTaxRate).Returns(99.99m);
			entryLine2Mock.Setup(m => m.SpecialConsumptionTax).Returns(999999999999m);
			entryLine2Mock.Setup(m => m.VATRateCode).Returns("x");
			entryLine2Mock.Setup(m => m.VATReductionCode).Returns("11");
			entryLine2Mock.Setup(m => m.ValueForVAT).Returns(11m);
			entryLine2Mock.Setup(m => m.ValueExemptForVAT).Returns(11m);
			entryLine2Mock.Setup(m => m.VATAmount).Returns(999999999999m);
			entryLine2Mock.Setup(m => m.EducationTaxExemptIndicator).Returns("x");
			entryLine2Mock.Setup(m => m.EducationTaxAmount).Returns(999999999999m);
			entryLine2Mock.Setup(m => m.AgricultureTaxClassification).Returns("x");
			entryLine2Mock.Setup(m => m.AgricultureTax).Returns(999999999999m);
			entryLine2Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(999999999999m);

			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object, entryLine2Mock.Object });
			#endregion

			#region IImportNonGADetail
			var nonDADetails1Mock = new Mock<IImportNonGADetail>();
			nonDADetails1Mock.Setup(m => m.ReasonType).Returns("A");
			nonDADetails1Mock.Setup(m => m.RegulationCategoryCode).Returns("05");
			nonDADetails1Mock.Setup(m => m.NonGAReasonType).Returns("05A13");
			nonDADetails1Mock.Setup(m => m.Reason).Returns("기타사유1");

			var nonDADetails2Mock = new Mock<IImportNonGADetail>();
			nonDADetails2Mock.Setup(m => m.ReasonType).Returns("B");
			nonDADetails2Mock.Setup(m => m.RegulationCategoryCode).Returns("06");
			nonDADetails2Mock.Setup(m => m.NonGAReasonType).Returns("06B14");
			nonDADetails2Mock.Setup(m => m.Reason).Returns("기타사유2");

			entryLine1Mock.Setup(m => m.NonGADetails).Returns(new IImportNonGADetail[] { nonDADetails1Mock.Object, nonDADetails2Mock.Object });
			#endregion

			#region IImportPreviousExpDecLine
			var previousExpDecLine1Mock = new Mock<IImportPreviousExpDecLine>();
			previousExpDecLine1Mock.Setup(m => m.DeclarationNumber).Returns("1234567");
			previousExpDecLine1Mock.Setup(m => m.EntryLineNo).Returns(123);
			previousExpDecLine1Mock.Setup(m => m.InvoiceLineNo).Returns(12);
			previousExpDecLine1Mock.Setup(m => m.UQ).Returns("KG");
			previousExpDecLine1Mock.Setup(m => m.UsedQty).Returns(123m);
			previousExpDecLine1Mock.Setup(m => m.SequenceNumber).Returns(1);

			var previousExpDecLine2Mock = new Mock<IImportPreviousExpDecLine>();
			previousExpDecLine2Mock.Setup(m => m.DeclarationNumber).Returns("7654321");
			previousExpDecLine2Mock.Setup(m => m.EntryLineNo).Returns(321);
			previousExpDecLine2Mock.Setup(m => m.InvoiceLineNo).Returns(21);
			previousExpDecLine2Mock.Setup(m => m.UQ).Returns("KG");
			previousExpDecLine2Mock.Setup(m => m.UsedQty).Returns(321m);
			previousExpDecLine2Mock.Setup(m => m.SequenceNumber).Returns(2);

			entryLine1Mock.Setup(m => m.PreviousExpDecLines).Returns(new IImportPreviousExpDecLine[] { previousExpDecLine1Mock.Object, previousExpDecLine2Mock.Object });
			#endregion

			#region IImportInvoiceLine
			var invoiceLines1Mock = new Mock<IImportInvoiceLine>();
			invoiceLines1Mock.Setup(m => m.InvoiceLineNo).Returns(01);
			invoiceLines1Mock.Setup(m => m.ItemDescription).Returns("모델 및 규격1");
			invoiceLines1Mock.Setup(m => m.Ingredient).Returns("성분1");
			invoiceLines1Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns("PC");
			invoiceLines1Mock.Setup(m => m.InvoiceQuantity).Returns(999999999m);
			invoiceLines1Mock.Setup(m => m.UnitPrice).Returns(30.402575m);
			invoiceLines1Mock.Setup(m => m.Amount).Returns(30.4627m);
			invoiceLines1Mock.Setup(m => m.PartNumber).Returns("K12123");

			var invoiceLines2Mock = new Mock<IImportInvoiceLine>();
			invoiceLines2Mock.Setup(m => m.InvoiceLineNo).Returns(02);
			invoiceLines2Mock.Setup(m => m.ItemDescription).Returns("모델 및 규격2");
			invoiceLines2Mock.Setup(m => m.Ingredient).Returns("성분2");
			invoiceLines2Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns("PC");
			invoiceLines2Mock.Setup(m => m.InvoiceQuantity).Returns(999999999m);
			invoiceLines2Mock.Setup(m => m.UnitPrice).Returns(30.222222m);
			invoiceLines2Mock.Setup(m => m.Amount).Returns(30.2222m);
			invoiceLines2Mock.Setup(m => m.PartNumber).Returns("K22222");

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLines1Mock.Object, invoiceLines2Mock.Object });
			#endregion

			#region IImportGAApprovalDocument1
			var gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns("1");
			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns("01");
			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns("123456789");
			gAApprovalDocuments1Mock.Setup(m => m.DocumentName).Returns("서류명1");
			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(new ZDate("2012-01-01"));
			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns("11");
			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns("12455632");

			var gAApprovalDocuments2Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments2Mock.Setup(m => m.RequirementDocumentType).Returns("2");
			gAApprovalDocuments2Mock.Setup(m => m.RequirementApprovalNumber).Returns("02");
			gAApprovalDocuments2Mock.Setup(m => m.RegulationCategoryCode).Returns("987654321");
			gAApprovalDocuments2Mock.Setup(m => m.DocumentName).Returns("서류명2");
			gAApprovalDocuments2Mock.Setup(m => m.ApprovalDate).Returns(new ZDate("2012-01-02"));
			gAApprovalDocuments2Mock.Setup(m => m.UseCode).Returns("12");
			gAApprovalDocuments2Mock.Setup(m => m.UniqueItemID).Returns("23655421");

			invoiceLines1Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object, gAApprovalDocuments2Mock.Object });
			#endregion
			#region IImportGAApprovalDocument2
			gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns("3");
			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns("03");
			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns("741852963");
			gAApprovalDocuments1Mock.Setup(m => m.DocumentName).Returns("서류명3");
			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(new ZDate("2020-12-10"));
			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns("13");
			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns("45563212");

			gAApprovalDocuments2Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments2Mock.Setup(m => m.RequirementDocumentType).Returns("4");
			gAApprovalDocuments2Mock.Setup(m => m.RequirementApprovalNumber).Returns("04");
			gAApprovalDocuments2Mock.Setup(m => m.RegulationCategoryCode).Returns("963852741");
			gAApprovalDocuments2Mock.Setup(m => m.DocumentName).Returns("서류명4");
			gAApprovalDocuments2Mock.Setup(m => m.ApprovalDate).Returns(new ZDate("2020-12-01"));
			gAApprovalDocuments2Mock.Setup(m => m.UseCode).Returns("14");
			gAApprovalDocuments2Mock.Setup(m => m.UniqueItemID).Returns("65542123");

			invoiceLines2Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object, gAApprovalDocuments2Mock.Object });
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신고인 상호");
			declarantMock.Setup(m => m.MobileNumber).Returns("000-000-0000");
			declarantMock.Setup(m => m.ExtensionNumber).Returns("0000");
			declarantMock.Setup(m => m.Email).Returns("id@nnnn");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			#endregion
			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("관세상사1234561");
			importerMock.Setup(m => m.CompanyName).Returns("조인성");
			importHeaderMock.Setup(m => m.ImporterType).Returns("A");

			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion
			#region Payer
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.KoreanRegNoForForeigner).Returns("6510071645915");
			payerMock.Setup(m => m.UnipassIDForOrganization).Returns("모나리자1771025");
			payerMock.Setup(m => m.OfficeID).Returns("0001");
			payerMock.Setup(m => m.IsIndividual).Returns(true);
			payerMock.Setup(m => m.CompanyName).Returns("모나리자㈜");
			payerMock.Setup(m => m.RoadNameCode).Returns("012345678912");
			payerMock.Setup(m => m.AddressLine1).Returns("서울시 강남구 논현동 235");
			payerMock.Setup(m => m.Postcode).Returns("11087");
			payerMock.Setup(m => m.BuildingNumber).Returns("1234567891234567891234567");
			payerMock.Setup(m => m.AddressLine2).Returns("7층 101호");
			payerMock.Setup(m => m.RepresentativeName).Returns("홍나리");
			payerMock.Setup(m => m.MobileNumber).Returns("000-000-0000");
			payerMock.Setup(m => m.Email).Returns("id@domain.com");

			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			#endregion
			#region Seller
			var sellerMock = new Mock<IOrganization>();
			sellerMock.Setup(m => m.ForeignCompanyID).Returns("CNTOSHIN12347");
			sellerMock.Setup(m => m.CompanyName).Returns("OMR ENGR");
			sellerMock.Setup(m => m.CountryCode).Returns("JP");

			importHeaderMock.Setup(m => m.Supplier).Returns(sellerMock.Object);
			#endregion
			#endregion

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR929_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region IImportEntryHeader
			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.InvoiceAmount.CurrencyId);
			AssertEquals(1000000m, result.InvoiceAmount.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals(95.2m, result.TotalGrossMassMeasure.Value);
			AssertEquals("KG", result.TotalGrossMassMeasure.KcsUnitCode);
			AssertEquals(2m, result.TotalPackageQuantity.Value);
			AssertEquals("CT", result.TotalPackageQuantity.KcsUnitCode);
			AssertEquals("GOVCBR929", result.TypeCode.Value);
			AssertEquals("11", result.TransactionNatureCode.Value);
			AssertEquals("AB", result.ResponseTypeCode.Value);
			AssertEquals("13", result.PaymentTypeCode.Value);
			AssertEquals("분할기타사유", result.Reason.Value);
			AssertEquals("Y", result.AdditionalCode.CertificateOfOriginIndicatorCode.Value);
			AssertEquals("Y", result.AdditionalCode.ValueDeclarationFormIndicatorCode.Value);
			AssertEquals("A", result.AdditionalCode.BillOfLadingSplitCode.Value);
			AssertEquals("Y", result.AdditionalCode.GoldTradeAcountIndicatorCode.Value);
			AssertEquals("기재사항1", result.AdditionalInformation.Content.Value);
			AssertEquals("257", result.AdditionalInformation.StatementCode[0].Name);
			AssertEquals("A", result.AdditionalInformation.StatementCode[0].Value);
			AssertEquals("258", result.AdditionalInformation.StatementCode[1].Name);
			AssertEquals("A", result.AdditionalInformation.StatementCode[1].Value);
			AssertEquals("259", result.AdditionalInformation.StatementCode[2].Name);
			AssertEquals("A", result.AdditionalInformation.StatementCode[2].Value);
			AssertEquals("기재사항2", result.AdditionalInformation.StatementDescription.Value);
			AssertEquals("B", result.AdditionalInformation.UsageDclarationCode.Value);
			AssertEquals("20120210101200", result.AdditionalInformation.UsageDateTime);
			AssertEquals("1234567890", result.Agent.Id.Value);
			AssertEquals("20120101", result.BorderTransportMeans.ArrivalDateTime);
			AssertEquals("KE1098", result.BorderTransportMeans.Name.Value);
			AssertEquals("KR", result.BorderTransportMeans.RegistrationNationalityId.Value);
			AssertEquals("4", result.BorderTransportMeans.TypeCode.Value);
			AssertEquals(1210.12m, result.CurrencyExchange.RateNumeric);
			AssertEquals("A", result.CustomsProcedure.ProcessTypeCode.Value);
			AssertEquals("L", result.CustomsProcedure.TypeCode.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.DutyTaxFee[0].AdValoremTaxBaseAmount[0].CurrencyId);
			AssertEquals(999999999999m, result.DutyTaxFee[0].AdValoremTaxBaseAmount[0].Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.DutyTaxFee[0].AdValoremTaxBaseAmount[1].CurrencyId);
			AssertEquals(9999999999m, result.DutyTaxFee[0].AdValoremTaxBaseAmount[1].Value);
			AssertEquals("CUD", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals("IND", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(100m, result.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals("5AA", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(100m, result.DutyTaxFee[2].Payment.TaxAssessedAmount.Value);
			AssertEquals("ACT", result.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(100m, result.DutyTaxFee[3].Payment.TaxAssessedAmount.Value);
			AssertEquals("5AB", result.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(100m, result.DutyTaxFee[4].Payment.TaxAssessedAmount.Value);
			AssertEquals("CAP", result.DutyTaxFee[5].TypeCode.Value);
			AssertEquals(100m, result.DutyTaxFee[5].Payment.TaxAssessedAmount.Value);
			AssertEquals("VAT", result.DutyTaxFee[6].TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[6].Payment.TaxAssessedAmount.Value);
			AssertEquals("5AC", result.DutyTaxFee[7].TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[7].Payment.TaxAssessedAmount.Value);
			AssertEquals("5AY", result.DutyTaxFee[8].TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[8].Payment.TaxAssessedAmount.Value);
			AssertEquals("5CZ", result.DutyTaxFee[9].TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[9].TotalVatBaseAmount.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[9].TotalVatExemptionBaseAmount.Value);
			AssertEquals(1234m, result.DutyTaxFee[9].Payment.TaxAssessedAmount.Value);
			AssertEquals("PR", result.GoodsShipment.ExportationCountryCode.Value);
			AssertEquals("ABCD", result.GoodsShipment.Agent.Id.Value);
			AssertEquals("세관무역㈜", result.GoodsShipment.Agent.Name.Value);
			AssertEquals("HJSC", result.GoodsShipment.Consignment.Carrier.Id.Value);
			AssertEquals("HJSC", result.GoodsShipment.Consignment.Express.Id.Value);
			AssertEquals("DBSC96100123AB01", result.GoodsShipment.Consignment.TransportContractDocument[0].Id.Value);
			AssertEquals("714", result.GoodsShipment.Consignment.TransportContractDocument[0].TypeCode.Value);
			AssertEquals("HJSC98100123AB01", result.GoodsShipment.Consignment.TransportContractDocument[1].Id.Value);
			AssertEquals("704", result.GoodsShipment.Consignment.TransportContractDocument[1].TypeCode.Value);
			AssertEquals("ETC", result.GoodsShipment.Consignment.TransportEquipment.CharacteristicCode.Value);
			AssertEquals("KRPUS", result.GoodsShipment.Consignment.UnloadingLocation.Id.Value);
			AssertEquals(1000m, result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount.CurrencyId);
			AssertEquals(1000m, result.GoodsShipment.CustomsValuation.FreightChargeAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.GoodsShipment.CustomsValuation.FreightChargeAmount.CurrencyId);
			AssertEquals("01", result.GoodsShipment.CustomsValuation.ChargeDeduction[0].ChargesTypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.CustomsValuation.ChargeDeduction[0].OtherChargeDeductionAmount.Value);
			AssertEquals("02", result.GoodsShipment.CustomsValuation.ChargeDeduction[1].ChargesTypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.CustomsValuation.ChargeDeduction[1].OtherChargeDeductionAmount.Value);
			AssertEquals("CFR", result.GoodsShipment.TradeTerms.ConditionCode.Value);
			AssertEquals("DA", result.GoodsShipment.TradeTerms.SettlementConditionCode.Value);
			AssertEquals("01KE0766SS20010003", result.GoodsShipment.Ucr.CustomsAssignedReferenceId.Value);
			AssertEquals("123456789", result.GoodsShipment.Ucr.TraderAssignedReferenceId.Value);
			AssertEquals("20120101", result.GoodsShipment.Warehouse.ArrivalDateTime);
			AssertEquals("13011013", result.GoodsShipment.Warehouse.Id.Value);
			AssertEquals("가1A123456", result.GoodsShipment.Warehouse.Address.Line.Value);
			AssertEquals("D", result.GovernmentProcedure.CurrentCode.Value);
			AssertEquals("Y", result.SouthNorthTrade.TradeIndicatorCode.Value);
			AssertEquals(true, result.TransportContractDocument.SplitDeclarationIndicator);
			#endregion

			#region IImportEntryLine
			AssertEquals(001m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalCode.ExaminationIndicatorCode.Value);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.SequenceNumeric);
			AssertEquals("RABBIT MEAT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription.Value);
			AssertEquals("0208100000-01-101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CharacteristicCode.Value);
			AssertEquals("DZ", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals(30m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.Value);
			AssertEquals("TACKS", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertEquals("7", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.IntendedUseCode.Value);
			AssertEquals("상표명", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name.Value);
			AssertEquals("ZZZZ", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.NameCode.Value);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCountQuantity.KcsUnitCode);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCountQuantity.Value);
			AssertEquals("B", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.CriteriaCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.ExaminationIndicatorCode.Value);
			AssertEquals("123456789", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("01", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.CriteriaCode.Value);
			AssertEquals("20130101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssueDateTime);
			AssertEquals("123456789", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssueId.Value);
			AssertEquals("발행기관명", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssueAgencyName.Value);
			AssertEquals("KR", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssueLocationCode.Value);
			AssertEquals("발급지역명", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssueLocationName.Value);
			AssertEquals("발급담당자명", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.IssuerName.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin.SplitIndicatorCode.Value);
			AssertEquals("0208100000", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[0].CurrencyId);
			AssertEquals(1000m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[0].Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[1].CurrencyId);
			AssertEquals(100m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[1].Value);
			AssertEquals(2001020m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DeductAmount.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DutyTaxDecuctTypeCode.Value);
			AssertEquals("A028040100", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DutyTaxDeductionInstallmentId.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].TaxRateNumeric);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DutyRegimeCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DeductionRateNumeric);
			AssertEquals("CUD", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].DutyRegimeCode.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].InternalTaxTypeCode.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].SpecialConsumptionTaxDeductionCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].TaxRateNumeric);
			AssertEquals("LCN", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].TypeCode.Value);
			AssertEquals("0000001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].LiquorTaxDeduction.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].Payment.SpecialTaxDescriptionAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[2].DutyRegimeCode.Value);
			AssertEquals("5AB", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[2].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[3].DutyRegimeCode.Value);
			AssertEquals("CAP", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[3].Payment.TaxAssessedAmount.Value);
			AssertEquals("11", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].DeductionId.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].DutyRegimeCode.Value);
			AssertEquals("VAT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(11m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].VatBaseAmount.Value);
			AssertEquals(11m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].VatExemptionBaseAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[4].Payment.TaxAssessedAmount.Value);
			AssertEquals("I", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[5].DutyRegimeCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[5].TaxRateNumeric);
			AssertEquals("FCU", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[5].TypeCode.Value);
			AssertEquals("023", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency[0].Value);
			AssertEquals("019", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency[1].Value);
			AssertEquals("020", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency[2].Value);
			AssertEquals(99999.9m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals("CN", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.CountryCode.Value);
			AssertEquals("12", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.RuleCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals("03", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription.MarksNumbersId.Value);
			AssertEquals("14", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription.NonDescriptionReason.Value);

			AssertEquals(002m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].SequenceNumeric);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[1].AdditionalCode.ExaminationIndicatorCode.Value);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.SequenceNumeric);
			AssertEquals("RABBIT MEAT2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CargoDescription.Value);
			AssertEquals("0208100000-01-202", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CharacteristicCode.Value);
			AssertEquals("DZ", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals(30m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CountQuantity.Value);
			AssertEquals("TACKS2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Description.Value);
			AssertEquals("8", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.IntendedUseCode.Value);
			AssertEquals("상표명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Name.Value);
			AssertEquals("ZZZZ2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.NameCode.Value);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DetailedCountQuantity.KcsUnitCode);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DetailedCountQuantity.Value);
			AssertEquals("B", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalCode.CriteriaCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalCode.ExaminationIndicatorCode.Value);
			AssertEquals("987654321", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("02", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.CriteriaCode.Value);
			AssertEquals("20130202", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssueDateTime);
			AssertEquals("발행기관명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssueAgencyName.Value);
			AssertEquals("KR", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssueLocationCode.Value);
			AssertEquals("발급지역명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssueLocationName.Value);
			AssertEquals("발급담당자명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssuerName.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.SplitIndicatorCode.Value);
			AssertEquals("987654321", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CertificateOfOrigin.IssueId.Value);
			AssertEquals("0208122222", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Classification.Id.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[0].CurrencyId);
			AssertEquals(1000m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[0].Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[1].CurrencyId);
			AssertEquals(100m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].AdValoremTaxBaseAmount[1].Value);
			AssertEquals(2001020m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DeductAmount.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DutyTaxDecuctTypeCode.Value);
			AssertEquals("A028040122", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DutyTaxDeductionInstallmentId.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DeductionRateNumeric);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DutyRegimeCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].TaxRateNumeric);
			AssertEquals("CUD", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].DutyRegimeCode.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].InternalTaxTypeCode.Value);
			AssertEquals("AA", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].SpecialConsumptionTaxDeductionCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].TaxRateNumeric);
			AssertEquals("LCN", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].TypeCode.Value);
			AssertEquals("0000002", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].LiquorTaxDeduction.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].Payment.SpecialTaxDescriptionAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[2].DutyRegimeCode.Value);
			AssertEquals("5AB", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[2].Payment.TaxAssessedAmount.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[3].DutyRegimeCode.Value);
			AssertEquals("CAP", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[3].Payment.TaxAssessedAmount.Value);
			AssertEquals("11", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].DeductionId.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].DutyRegimeCode.Value);
			AssertEquals("VAT", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(11m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].VatBaseAmount.Value);
			AssertEquals(11m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].VatExemptionBaseAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[4].Payment.TaxAssessedAmount.Value);
			AssertEquals("I", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[5].DutyRegimeCode.Value);
			AssertEquals(99.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[5].TaxRateNumeric);
			AssertEquals("FCU", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[5].TypeCode.Value);
			AssertEquals("024", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.ResponsibleGovernmentAgency[0].Value);
			AssertEquals("020", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.ResponsibleGovernmentAgency[1].Value);
			AssertEquals("021", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.ResponsibleGovernmentAgency[2].Value);
			AssertEquals(99999.9m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals("CN", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.CountryCode.Value);
			AssertEquals("11", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.RuleCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals("03", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.OriginDescription.MarksNumbersId.Value);
			AssertEquals("12", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.OriginDescription.NonDescriptionReason.Value);
			#endregion

			#region IImportNonGADetail
			AssertEquals("기타사유1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].Content.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].ReconciliationReasonCode.Value);
			AssertEquals("05", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].CriteriaConformanceCode.Value);
			AssertEquals("05A13", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].StatementCode.Value);

			AssertEquals("기타사유2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[1].Content.Value);
			AssertEquals("B", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[1].ReconciliationReasonCode.Value);
			AssertEquals("06", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[1].CriteriaConformanceCode.Value);
			AssertEquals("06B14", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[1].StatementCode.Value);
			#endregion

			#region IImportPreviousExpDecLine
			AssertEquals("1234567", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].Id.Value);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].LineNumeric);
			AssertEquals(12m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].SequenceNumeric);
			AssertEquals("KG", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].GoodsMeasure.NetNetWeightMeasure.KcsUnitCode);
			AssertEquals(123m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].GoodsMeasure.NetNetWeightMeasure.Value);

			AssertEquals("7654321", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[1].Id.Value);
			AssertEquals(321m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[1].LineNumeric);
			AssertEquals(21m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[1].SequenceNumeric);
			AssertEquals("KG", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[1].GoodsMeasure.NetNetWeightMeasure.KcsUnitCode);
			AssertEquals(321m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[1].GoodsMeasure.NetNetWeightMeasure.Value);
			#endregion

			#region IImportInvoiceLine
			AssertEquals(01m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].SequenceNumeric);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);
			AssertEquals(999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
			AssertEquals("모델 및 규격1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Description.Value);
			AssertEquals("K12123", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].LotNumberId.Value);
			AssertEquals(30.402575m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].UnitPriceAmount.Value);
			AssertEquals(30.4627m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].ValueAmount.Value);
			AssertEquals("성분1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Constituent.ElementName.Value);

			AssertEquals(02m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].SequenceNumeric);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].CountQuantity.KcsUnitCode);
			AssertEquals(999999999m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].CountQuantity.Value);
			AssertEquals("모델 및 규격2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].Description.Value);
			AssertEquals("K22222", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].LotNumberId.Value);
			AssertEquals(30.222222m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].UnitPriceAmount.Value);
			AssertEquals(30.2222m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].ValueAmount.Value);
			AssertEquals("성분2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].Constituent.ElementName.Value);
			#endregion

			#region IImportGAApprovalDocument1
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].TypeCode.Value);
			AssertEquals("01", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].Id.Value);
			AssertEquals("123456789", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].CriteriaConformanceCode.Value);
			AssertEquals("서류명1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].Name.Value);
			AssertEquals("20120101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].IssueDateTime);
			AssertEquals("11", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].IntendedUseCode.Value);
			AssertEquals("12455632", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].CommercialCategorizationId.Value);

			AssertEquals("2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].TypeCode.Value);
			AssertEquals("02", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].Id.Value);
			AssertEquals("987654321", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].CriteriaConformanceCode.Value);
			AssertEquals("서류명2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].Name.Value);
			AssertEquals("20120102", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].IssueDateTime);
			AssertEquals("12", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].IntendedUseCode.Value);
			AssertEquals("23655421", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].CommercialCategorizationId.Value);
			#endregion
			#region IImportGAApprovalDocument2
			AssertEquals("741852963", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].CriteriaConformanceCode.Value);
			AssertEquals("03", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].Id.Value);
			AssertEquals("20201210", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].IssueDateTime);
			AssertEquals("3", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].TypeCode.Value);
			AssertEquals("서류명3", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].Name.Value);
			AssertEquals("13", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].IntendedUseCode.Value);
			AssertEquals("45563212", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[0].CommercialCategorizationId.Value);

			AssertEquals("963852741", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].CriteriaConformanceCode.Value);
			AssertEquals("04", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].Id.Value);
			AssertEquals("20201201", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].IssueDateTime);
			AssertEquals("4", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].TypeCode.Value);
			AssertEquals("서류명4", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].Name.Value);
			AssertEquals("14", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].IntendedUseCode.Value);
			AssertEquals("65542123", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[1].AdditionalDocument[1].CommercialCategorizationId.Value);
			#endregion

			#region Declarant
			AssertEquals("신고인 상호", result.Submitter.Name.Value);
			AssertEquals("TE", result.Submitter.Communication[0].TypeId.Value);
			AssertEquals("000-000-0000", result.Submitter.Communication[0].Id.Value);
			AssertEquals("EX", result.Submitter.Communication[1].TypeId.Value);
			AssertEquals("0000", result.Submitter.Communication[1].Id.Value);
			AssertEquals("EM", result.Submitter.Communication[2].TypeId.Value);
			AssertEquals("id@nnnn", result.Submitter.Communication[2].Id.Value);
			#endregion
			#region Importer
			AssertEquals("관세상사1234561", result.Importer.Id.Value);
			AssertEquals("조인성", result.Importer.Name.Value);
			AssertEquals("A", result.Importer.RoleCode.Value);
			#endregion
			#region Payer
			AssertEquals(AgencyIdentificationCodeContentType.Zzz, result.Payer.Id[0].SchemeAgencyId);
			AssertEquals("6510071645915", result.Payer.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Payer.Id[1].SchemeAgencyId);
			AssertEquals("모나리자1771025", result.Payer.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Kts, result.Payer.Id[2].SchemeAgencyId);
			AssertEquals("0001", result.Payer.Id[2].Value);
			AssertEquals("모나리자㈜", result.Payer.Name.Value);
			AssertEquals("03", result.Payer.RoleCode.Value);
			AssertEquals("012345678912", result.Payer.Address.CountrySubDivisionId.Value);
			AssertEquals("7층 101호", result.Payer.Address.Line.Value);
			AssertEquals("11087", result.Payer.Address.PostcodeId.Value);
			AssertEquals("1234567891234567891234567", result.Payer.Address.BuildingNumber.Value);
			AssertEquals("서울시 강남구 논현동 235", result.Payer.Address.Description.Value);
			AssertEquals("홍나리", result.Payer.Contact.Name.Value);
			AssertEquals("TE", result.Payer.Communication[0].TypeId.Value);
			AssertEquals("000-000-0000", result.Payer.Communication[0].Id.Value);
			AssertEquals("EM", result.Payer.Communication[1].TypeId.Value);
			AssertEquals("id@domain.com", result.Payer.Communication[1].Id.Value);
			#endregion
			#region Seller
			AssertEquals("CNTOSHIN12347", result.GoodsShipment.Seller.Id.Value);
			AssertEquals("OMR ENGR", result.GoodsShipment.Seller.Name.Value);
			AssertEquals("JP", result.GoodsShipment.Seller.Address.CountryCode.Value);
			#endregion
			importHeaderMock.VerifyAll();
			entryLine1Mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2013, 01, 01)]
		public void TestEmptyData()
		{
			#region Setup(Conditional - ZString.Empty)
			#region IImportEntryHeader
			importHeaderMock.Setup(m => m.HouseBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonDescription).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.UnderbondMovementArrivalDate).Returns(new ZDate(ZString.Empty));
			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.DeclarationPlanCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CertificateOfOriginIssued).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ValueDeclarationAttached).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.TotalPackQty).Returns(ZInt.Zero);
			importHeaderMock.Setup(m => m.ArrivalPort).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselOrFlightNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselCountryCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.MasterBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CarrierID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.Incoterm).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.TotalCustomsValueUSD).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.Freight).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.Insurance).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.AdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.DeductedAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.CourierCompanyID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.AuthorizedImporterRegNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.OwnerReferenceNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.GoldTradeTransactionYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(new ZDateTime(ZString.Empty));
			importHeaderMock.Setup(m => m.TotalSpecialConsumptionTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalTransportationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalLiquorTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalEducationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalAgricultureTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalValueForVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVATExemptionValue).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForLateDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForMissedDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalPayableAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode3).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BlanketValuationDeclarationNumber).Returns(ZString.Empty);
			#endregion

			#region IImportEntryLine
			entryLine1Mock.Setup(m => m.AdditionalTariffCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.SpecificUseCodeDutyRatePermitNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate(ZString.Empty));
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssuingCountry).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginAgencyName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginAreaName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginPersonName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginSplitIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelType).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ProductOrMaterialCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.MaterialLineNo).Returns(ZInt.Zero);
			entryLine1Mock.Setup(m => m.QuantityToClaimRefund).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.MightRequireInspectionIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency1).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.NetWeightInKG).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.Quantity).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CustomsValueKRW).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CustomsValueUSD).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CourierCargoSelectivityIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.AdValoremDutyRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AdditionalDutyRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AdditionalDutyCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DutyReductionAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DomesticTaxClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.SpecialConsumptionTax).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.VATRateCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.VATReductionCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ValueForVAT).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.ValueExemptForVAT).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.VATAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.EducationTaxExemptIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.EducationTaxAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AgricultureTaxClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.AgricultureTax).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(ZDecimal.Zero);

			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			#endregion

			#region IImportNonGADetail
			var nonDADetails1Mock = new Mock<IImportNonGADetail>();
			nonDADetails1Mock.Setup(m => m.ReasonType).Returns(ZString.Empty);
			nonDADetails1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			nonDADetails1Mock.Setup(m => m.Reason).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.NonGADetails).Returns(new IImportNonGADetail[] { nonDADetails1Mock.Object });
			#endregion

			#region IImportPreviousExpDecLine
			var previousExpDecLine1Mock = new Mock<IImportPreviousExpDecLine>();
			previousExpDecLine1Mock.Setup(m => m.DeclarationNumber).Returns(ZString.Empty);
			previousExpDecLine1Mock.Setup(m => m.EntryLineNo).Returns(ZInt.Zero);
			previousExpDecLine1Mock.Setup(m => m.InvoiceLineNo).Returns(ZInt.Zero);
			previousExpDecLine1Mock.Setup(m => m.UQ).Returns(ZString.Empty);
			previousExpDecLine1Mock.Setup(m => m.UsedQty).Returns(ZDecimal.Zero);

			entryLine1Mock.Setup(m => m.PreviousExpDecLines).Returns(new IImportPreviousExpDecLine[] { previousExpDecLine1Mock.Object });
			#endregion

			#region IImportInvoiceLine
			var invoiceLines1Mock = new Mock<IImportInvoiceLine>();
			invoiceLines1Mock.Setup(m => m.InvoiceLineNo).Returns(ZInt.Zero);
			invoiceLines1Mock.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			invoiceLines1Mock.Setup(m => m.Ingredient).Returns(ZString.Empty);
			invoiceLines1Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns("PC");
			invoiceLines1Mock.Setup(m => m.InvoiceQuantity).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.UnitPrice).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.Amount).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.PartNumber).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLines1Mock.Object });
			#endregion

			#region IImportGAApprovalDocument1
			var gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.DocumentName).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(new ZDate(ZString.Empty));
			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns(ZString.Empty);

			invoiceLines1Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object });
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신고인 상호");
			declarantMock.Setup(m => m.MobileNumber).Returns("000-000-0000");
			declarantMock.Setup(m => m.ExtensionNumber).Returns(ZString.Empty);
			declarantMock.Setup(m => m.Email).Returns("id@nnnn");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			#endregion
			#region Payer
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.BusinessRegNo).Returns("6510071645915");
			payerMock.Setup(m => m.IsIndividual).Returns(false);
			payerMock.Setup(m => m.CompanyName).Returns("모나리자㈜");
			payerMock.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			payerMock.Setup(m => m.AddressLine1).Returns("서울시 강남구 논현동 235");
			payerMock.Setup(m => m.Postcode).Returns(ZString.Empty);
			payerMock.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			payerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			payerMock.Setup(m => m.RepresentativeName).Returns("홍나리");
			payerMock.Setup(m => m.MobileNumber).Returns("000-000-0000");
			payerMock.Setup(m => m.Email).Returns(ZString.Empty);

			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			#endregion
			#endregion

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR929_1.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region IImportEntryHeader
			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.InvoiceAmount.CurrencyId);
			AssertEquals(1000000m, result.InvoiceAmount.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals(95.2m, result.TotalGrossMassMeasure.Value);
			AssertEquals("KG", result.TotalGrossMassMeasure.KcsUnitCode);
			AssertEquals("CT", result.TotalPackageQuantity.KcsUnitCode);
			AssertEquals("11", result.TransactionNatureCode.Value);
			AssertEquals("13", result.PaymentTypeCode.Value);
			AssertEquals("20120101", result.BorderTransportMeans.ArrivalDateTime);
			AssertEquals("4", result.BorderTransportMeans.TypeCode.Value);
			AssertEquals(1210.12m, result.CurrencyExchange.RateNumeric);
			AssertEquals("A", result.CustomsProcedure.ProcessTypeCode.Value);
			AssertEquals("L", result.CustomsProcedure.TypeCode.Value);
			AssertEquals(999999999999m, result.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals("PR", result.GoodsShipment.ExportationCountryCode.Value);
			AssertEquals("ETC", result.GoodsShipment.Consignment.TransportEquipment.CharacteristicCode.Value);
			AssertEquals("DA", result.GoodsShipment.TradeTerms.SettlementConditionCode.Value);
			AssertEquals("01KE0766SS20010003", result.GoodsShipment.Ucr.CustomsAssignedReferenceId.Value);
			AssertEquals("13011013", result.GoodsShipment.Warehouse.Id.Value);
			AssertEquals(true, result.TransportContractDocument.SplitDeclarationIndicator);
			#endregion

			#region IImportEntryLine
			AssertEquals(001m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertEquals("RABBIT MEAT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription.Value);
			AssertEquals("TACKS", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertEquals("상표명", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name.Value);
			AssertEquals("ZZZZ", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.NameCode.Value);
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.CriteriaCode.Value);
			AssertEquals("0208100000", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id.Value);
			AssertEquals("x", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DutyRegimeCode.Value);
			AssertEquals("CUD", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].TypeCode.Value);
			AssertEquals("CN", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.CountryCode.Value);
			AssertEquals("12", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.RuleCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals("B", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCountQuantity.KcsUnitCode);
			#endregion

			#region IImportInvoiceLine
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);
			#endregion

			#region Declarant
			AssertEquals("신고인 상호", result.Submitter.Name.Value);
			AssertEquals("TE", result.Submitter.Communication[0].TypeId.Value);
			AssertEquals("000-000-0000", result.Submitter.Communication[0].Id.Value);
			AssertEquals("id@nnnn", result.Submitter.Communication[1].Id.Value);
			#endregion
			#region Importer
			AssertNull(result.Importer);
			#endregion
			#region Payer
			AssertEquals(AgencyIdentificationCodeContentType.Zzz, result.Payer.Id[0].SchemeAgencyId);
			AssertEquals("6510071645915", result.Payer.Id[0].Value);
			AssertEquals("모나리자㈜", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);
			AssertEquals("서울시 강남구 논현동 235", result.Payer.Address.Description.Value);
			AssertEquals("홍나리", result.Payer.Contact.Name.Value);
			AssertEquals("TE", result.Payer.Communication[0].TypeId.Value);
			AssertEquals("000-000-0000", result.Payer.Communication[0].Id.Value);
			#endregion
			importHeaderMock.VerifyAll();
			entryLine1Mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 03)]
		public void TestEmptyData2()
		{
			#region Setup(Conditional - ZString.Empty)
			#region IImportEntryHeader
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("6N00221000004M");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("040");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("11");
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationIndicator).Returns(false);
			importHeaderMock.Setup(m => m.CargoManagementNo).Returns("20KE0E087II00330003");
			importHeaderMock.Setup(m => m.ArrivalDateAtDischargePort).Returns(new ZDate("2020-08-07"));
			importHeaderMock.Setup(m => m.PaymentType).Returns("43");
			importHeaderMock.Setup(m => m.ImportTypeCode).Returns("A");
			importHeaderMock.Setup(m => m.TradeType).Returns("11");
			importHeaderMock.Setup(m => m.DeclarationProcedureType).Returns("21");
			importHeaderMock.Setup(m => m.TotalGrossWeightInKG).Returns(3.1m);
			importHeaderMock.Setup(m => m.PackType).Returns("CT");
			importHeaderMock.Setup(m => m.TransportMode).Returns("40");
			importHeaderMock.Setup(m => m.ContainerPackMode).Returns("ETC");
			importHeaderMock.Setup(m => m.DepartureCountryCode).Returns("FR");
			importHeaderMock.Setup(m => m.BondedAreaCode).Returns("04077004");
			importHeaderMock.Setup(m => m.TotalInvoiceAmount).Returns(7991692m);
			importHeaderMock.Setup(m => m.InvoiceAmountCurrency).Returns("KRW");
			importHeaderMock.Setup(m => m.InvoicePaymentTerm).Returns("TT");
			importHeaderMock.Setup(m => m.ExchangeRate).Returns(1m);
			importHeaderMock.Setup(m => m.TotalDutyAmount).Returns(0m);
			importHeaderMock.Setup(m => m.ApplicationForAgreedRate).Returns(new ZBool("N"));

			importHeaderMock.Setup(m => m.HouseBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonDescription).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.UnderbondMovementArrivalDate).Returns(new ZDate(ZString.Empty));
			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.DeclarationPlanCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CertificateOfOriginIssued).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ValueDeclarationAttached).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.TotalPackQty).Returns(ZInt.Zero);
			importHeaderMock.Setup(m => m.ArrivalPort).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselOrFlightNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselCountryCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.MasterBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CarrierID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.Incoterm).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.TotalCustomsValueUSD).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.Freight).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.Insurance).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.AdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.DeductedAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.CourierCompanyID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.AuthorizedImporterRegNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.OwnerReferenceNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.GoldTradeTransactionYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(new ZDateTime(ZString.Empty));
			importHeaderMock.Setup(m => m.TotalSpecialConsumptionTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalTransportationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalLiquorTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalEducationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalAgricultureTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalValueForVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVATExemptionValue).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForLateDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForMissedDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalPayableAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode3).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BlanketValuationDeclarationNumber).Returns(ZString.Empty);
			#endregion

			#region IImportEntryLine
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(001);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("WALLEET, CARD HOLDERT");
			entryLine1Mock.Setup(m => m.ModelName).Returns("WALLEET, CARD HOLDERT");
			entryLine1Mock.Setup(m => m.BrandCode).Returns("0312");
			entryLine1Mock.Setup(m => m.BrandName).Returns("CHRISTIAN DIOR");
			entryLine1Mock.Setup(m => m.HSCode).Returns("4202311010");
			entryLine1Mock.Setup(m => m.CountryOfOrigin).Returns("IT");
			entryLine1Mock.Setup(m => m.CountryOfOriginDeterminationRule).Returns("4");
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelLocation).Returns("G");
			entryLine1Mock.Setup(m => m.QuantityUQToClaimRefund).Returns("PC");
			entryLine1Mock.Setup(m => m.DutyRateCode).Returns("FEU1");
			entryLine1Mock.Setup(m => m.DutyRateTypeCode).Returns("1");
			entryLine1Mock.Setup(m => m.DutyAmount).Returns(0m);

			entryLine1Mock.Setup(m => m.AdditionalTariffCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.SpecificUseCodeDutyRatePermitNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate(ZString.Empty));
			entryLine1Mock.Setup(m => m.CertificateOfOriginIssuingCountry).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginAgencyName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginAreaName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginPersonName).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginSplitIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelType).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ProductOrMaterialCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.MaterialLineNo).Returns(ZInt.Zero);
			entryLine1Mock.Setup(m => m.QuantityToClaimRefund).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.MightRequireInspectionIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency1).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.NetWeightInKG).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.Quantity).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CustomsValueKRW).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CustomsValueUSD).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.CourierCargoSelectivityIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.AdValoremDutyRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AdditionalDutyRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AdditionalDutyCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DutyReductionAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DomesticTaxClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxRate).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.SpecialConsumptionTax).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.VATRateCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.VATReductionCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ValueForVAT).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.ValueExemptForVAT).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.VATAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.EducationTaxExemptIndicator).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.EducationTaxAmount).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.AgricultureTaxClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.AgricultureTax).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(ZDecimal.Zero);

			var entryLine2Mock = new Mock<IImportEntryLine>();
			entryLine2Mock.Setup(m => m.EntryLineNo).Returns(002);
			entryLine2Mock.Setup(m => m.HSDescription).Returns("OF CROCODILE");
			entryLine2Mock.Setup(m => m.ModelName).Returns("HANDBAG");
			entryLine2Mock.Setup(m => m.BrandCode).Returns("0312");
			entryLine2Mock.Setup(m => m.BrandName).Returns("CHRISTIAN DIOR");
			entryLine2Mock.Setup(m => m.HSCode).Returns("4202211030");
			entryLine2Mock.Setup(m => m.CountryOfOrigin).Returns("IT");
			entryLine2Mock.Setup(m => m.CountryOfOriginDeterminationRule).Returns("6");
			entryLine2Mock.Setup(m => m.CountryOfOriginLabelLocation).Returns("G");
			entryLine2Mock.Setup(m => m.QuantityUQToClaimRefund).Returns("PC");
			entryLine2Mock.Setup(m => m.QuantityUnit).Returns("DZ");
			entryLine2Mock.Setup(m => m.DutyRateCode).Returns("FEU1");
			entryLine2Mock.Setup(m => m.DutyRateTypeCode).Returns("1");
			entryLine2Mock.Setup(m => m.DutyAmount).Returns(0m);

			entryLine2Mock.Setup(m => m.AdditionalTariffCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.SpecificUseCodeDutyRatePermitNo).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginNo).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate(ZString.Empty));
			entryLine2Mock.Setup(m => m.CertificateOfOriginIssuingCountry).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginAgencyName).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginAreaName).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginPersonName).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginSplitIndicator).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CountryOfOriginLabelType).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.ProductOrMaterialCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.MaterialLineNo).Returns(ZInt.Zero);
			entryLine2Mock.Setup(m => m.QuantityToClaimRefund).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.MightRequireInspectionIndicator).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency1).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.NetWeightInKG).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.Quantity).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.CustomsValueKRW).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.CustomsValueUSD).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.CourierCargoSelectivityIndicator).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.AdValoremDutyRate).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.AdditionalDutyRate).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.AdditionalDutyCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DutyReductionClassification).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DutyReductionRate).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.DutyReductionAmount).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.DomesticTaxClassification).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DomesticTaxCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DomesticTaxRate).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.SpecialConsumptionTax).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.VATRateCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.VATReductionCode).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.ValueForVAT).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.ValueExemptForVAT).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.VATAmount).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.EducationTaxExemptIndicator).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.EducationTaxAmount).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.AgricultureTaxClassification).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.AgricultureTax).Returns(ZDecimal.Zero);
			entryLine2Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(ZDecimal.Zero);

			entryLine2Mock.Setup(m => m.NonGADetails).Returns(System.Array.Empty<IImportNonGADetail>());
			entryLine2Mock.Setup(m => m.PreviousExpDecLines).Returns(System.Array.Empty<IImportPreviousExpDecLine>());
			entryLine2Mock.Setup(m => m.InvoiceLines).Returns(System.Array.Empty<IImportInvoiceLine>());

			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object, entryLine2Mock.Object });
			#endregion

			#region IImportNonGADetail
			var nonDADetails1Mock = new Mock<IImportNonGADetail>();
			nonDADetails1Mock.Setup(m => m.ReasonType).Returns(ZString.Empty);
			nonDADetails1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			nonDADetails1Mock.Setup(m => m.Reason).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.NonGADetails).Returns(new IImportNonGADetail[] { nonDADetails1Mock.Object });
			#endregion

			#region IImportPreviousExpDecLine
			var previousExpDecLine1Mock = new Mock<IImportPreviousExpDecLine>();
			previousExpDecLine1Mock.Setup(m => m.DeclarationNumber).Returns(ZString.Empty);
			previousExpDecLine1Mock.Setup(m => m.EntryLineNo).Returns(ZInt.Zero);
			previousExpDecLine1Mock.Setup(m => m.InvoiceLineNo).Returns(ZInt.Zero);
			previousExpDecLine1Mock.Setup(m => m.UQ).Returns("KG");
			previousExpDecLine1Mock.Setup(m => m.UsedQty).Returns(ZDecimal.Zero);

			entryLine1Mock.Setup(m => m.PreviousExpDecLines).Returns(new IImportPreviousExpDecLine[] { previousExpDecLine1Mock.Object });
			#endregion

			#region IImportInvoiceLine
			var invoiceLines1Mock = new Mock<IImportInvoiceLine>();
			invoiceLines1Mock.Setup(m => m.InvoiceLineNo).Returns(ZInt.Zero);
			invoiceLines1Mock.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			invoiceLines1Mock.Setup(m => m.Ingredient).Returns(ZString.Empty);
			invoiceLines1Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns("PC");
			invoiceLines1Mock.Setup(m => m.InvoiceQuantity).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.UnitPrice).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.Amount).Returns(ZDecimal.Zero);
			invoiceLines1Mock.Setup(m => m.PartNumber).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLines1Mock.Object });
			#endregion

			#region IImportGAApprovalDocument1
			var gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.DocumentName).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(new ZDate(ZString.Empty));
			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns(ZString.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns(ZString.Empty);

			invoiceLines1Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object });
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("레디코리아/김환태");
			declarantMock.Setup(m => m.MobileNumber).Returns("02-548-7636");
			declarantMock.Setup(m => m.ExtensionNumber).Returns(ZString.Empty);
			declarantMock.Setup(m => m.Email).Returns("snoopy2075@hanmail.net");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			#endregion
			#region Payer
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.BusinessRegNo).Returns("1208174197");
			payerMock.Setup(m => m.IsIndividual).Returns(false);
			payerMock.Setup(m => m.CompanyName).Returns("크리스챤디올꾸뛰르코리아(주)");
			payerMock.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			payerMock.Setup(m => m.AddressLine1).Returns("서울시 강남구 도산대로 458 (청담동,");
			payerMock.Setup(m => m.Postcode).Returns(ZString.Empty);
			payerMock.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			payerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			payerMock.Setup(m => m.RepresentativeName).Returns("트렁히엔트란");
			payerMock.Setup(m => m.MobileNumber).Returns("02-513-3220");
			payerMock.Setup(m => m.Email).Returns(ZString.Empty);

			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			#endregion
			#endregion

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR929_2.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region IImportEntryHeader
			AssertEquals("6N00221000004M", result.Id.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals("04011", result.DeclarationOfficeId.Value);
			AssertEquals(false, result.TransportContractDocument.SplitDeclarationIndicator);
			AssertEquals("20KE0E087II00330003", result.GoodsShipment.Ucr.CustomsAssignedReferenceId.Value);
			AssertEquals("20200807", result.BorderTransportMeans.ArrivalDateTime);
			AssertEquals("43", result.PaymentTypeCode.Value);
			AssertEquals("A", result.CustomsProcedure.ProcessTypeCode.Value);
			AssertEquals("11", result.TransactionNatureCode.Value);
			AssertEquals("21", result.CustomsProcedure.TypeCode.Value);
			AssertEquals(3.1m, result.TotalGrossMassMeasure.Value);
			AssertEquals("KG", result.TotalGrossMassMeasure.KcsUnitCode);
			AssertEquals("CT", result.TotalPackageQuantity.KcsUnitCode);
			AssertEquals("40", result.BorderTransportMeans.TypeCode.Value);
			AssertEquals("ETC", result.GoodsShipment.Consignment.TransportEquipment.CharacteristicCode.Value);
			AssertEquals("FR", result.GoodsShipment.ExportationCountryCode.Value);
			AssertEquals("04077004", result.GoodsShipment.Warehouse.Id.Value);
			AssertEquals(7991692m, result.InvoiceAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Krw, result.InvoiceAmount.CurrencyId);
			AssertEquals("TT", result.GoodsShipment.TradeTerms.SettlementConditionCode.Value);
			AssertEquals(1m, result.CurrencyExchange.RateNumeric);
			AssertEquals(0m, result.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			#endregion

			#region IImportEntryLine
			AssertEquals(001m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertEquals("WALLEET, CARD HOLDERT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertEquals("WALLEET, CARD HOLDERT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription.Value);
			AssertEquals("0312", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.NameCode.Value);
			AssertEquals("CHRISTIAN DIOR", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name.Value);
			AssertEquals("4202311010", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id.Value);
			AssertEquals("IT", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.CountryCode.Value);
			AssertEquals("4", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.RuleCode.Value);
			AssertEquals("G", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals("N", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCountQuantity.KcsUnitCode);
			AssertEquals("FEU1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee[0].DutyRegimeCode.Value);
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalCode.CriteriaCode.Value);

			AssertEquals(002m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].SequenceNumeric);
			AssertEquals("OF CROCODILE", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Description.Value);
			AssertEquals("HANDBAG", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CargoDescription.Value);
			AssertEquals("0312", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.NameCode.Value);
			AssertEquals("CHRISTIAN DIOR", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Name.Value);
			AssertEquals("4202211030", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Classification.Id.Value);
			AssertEquals("IT", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.CountryCode.Value);
			AssertEquals("6", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.RuleCode.Value);
			AssertEquals("G", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals("N", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity);
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCountQuantity.KcsUnitCode);
			AssertEquals("FEU1", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.DutyTaxFee[0].DutyRegimeCode.Value);
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalCode.CriteriaCode.Value);
			#endregion

			#region IImportInvoiceLine
			AssertEquals("PC", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);
			#endregion

			#region Declarant
			AssertEquals("레디코리아/김환태", result.Submitter.Name.Value);
			AssertEquals("TE", result.Submitter.Communication[0].TypeId.Value);
			AssertEquals("02-548-7636", result.Submitter.Communication[0].Id.Value);
			AssertEquals("snoopy2075@hanmail.net", result.Submitter.Communication[1].Id.Value);
			#endregion
			#region Importer
			AssertNull(result.Importer);
			#endregion
			#region Payer
			AssertEquals(AgencyIdentificationCodeContentType.Zzz, result.Payer.Id[0].SchemeAgencyId);
			AssertEquals("1208174197", result.Payer.Id[0].Value);
			AssertEquals("크리스챤디올꾸뛰르코리아(주)", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);
			AssertEquals("서울시 강남구 도산대로 458 (청담동,", result.Payer.Address.Description.Value);
			AssertEquals("트렁히엔트란", result.Payer.Contact.Name.Value);
			AssertEquals("TE", result.Payer.Communication[0].TypeId.Value);
			AssertEquals("02-513-3220", result.Payer.Communication[0].Id.Value);
			#endregion
			importHeaderMock.VerifyAll();
			entryLine1Mock.VerifyAll();
		}

		public void TestEmptyEntryHeaderData()
		{
			#region Setup(Conditional - ZString.Empty)
			#region IImportEntryHeader
			importHeaderMock.Setup(m => m.HouseBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationReasonDescription).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.UnderbondMovementArrivalDate).Returns(new ZDate(ZString.Empty));
			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.DeclarationPlanCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CertificateOfOriginIssued).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ValueDeclarationAttached).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PackType).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.TotalPackQty).Returns(ZInt.Zero);
			importHeaderMock.Setup(m => m.ArrivalPort).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselOrFlightNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.VesselCountryCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.MasterBillNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CarrierID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BlanketValuationDeclarationNumber).Returns(ZString.Empty);
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.ExtensionNumber).Returns(ZString.Empty);

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			#endregion

			#region Payer
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			payerMock.Setup(m => m.Postcode).Returns(ZString.Empty);
			payerMock.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			payerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			payerMock.Setup(m => m.Email).Returns(ZString.Empty);

			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			#endregion
			#endregion

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.Consignment.TransportContractDocument);
			AssertNull(result.Reason);
			AssertNull(result.GoodsShipment.Warehouse.ArrivalDateTime);
			AssertEquals(2, result.Submitter.Communication.Count);
			AssertNotNull(result.Payer);
			AssertNull(result.Payer.Address.PostcodeId);
			AssertNull(result.Payer.Address.CountrySubDivisionId);
			AssertNull(result.Payer.Address.BuildingNumber);
			AssertNull(result.Payer.Address.Line);
			AssertEquals(false, result.Payer.Communication.Any(c => c.TypeId.Value == Communication.Email));
			AssertEquals(1, result.Payer.Id.Count);
			AssertNull(result.GoodsShipment.Agent);
			AssertNull(result.GovernmentProcedure);
			AssertNull(result.TotalPackageQuantity);
			AssertNull(result.GoodsShipment.Consignment.UnloadingLocation);
			AssertNull(result.BorderTransportMeans.Name);
			AssertNull(result.BorderTransportMeans.RegistrationNationalityId);
			AssertNull(result.GoodsShipment.Consignment.Carrier);
			AssertNull(result.GoodsShipment.Warehouse.Address);
			AssertNull(result.AggregationPriceDeclarationId);

			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns("세관무역㈜");
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns(ZString.Empty);

			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.Agent);
			AssertEquals("세관무역㈜", result.GoodsShipment.Agent.Name.Value);
			AssertNull(ZString.Empty, result.GoodsShipment.Agent.Id);

			importHeaderMock.Setup(m => m.FreightForwarderCompanyName).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.FreightForwarderID).Returns("ABCD");

			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.Agent);
			AssertNull(ZString.Empty, result.GoodsShipment.Agent.Name);
			AssertEquals("ABCD", result.GoodsShipment.Agent.Id.Value);
		}

		public void TestEmptyImporter()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.ImporterType).Returns("B");
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("관세상사1234561");
			importerMock.Setup(m => m.CompanyName).Returns("조인성");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("B", result.Importer.RoleCode.Value);

			importerMock.Setup(m => m.UnipassIDForOrganization).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ImporterType).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.Importer.RoleCode);
			AssertNull(result.Importer.Id);

			importerMock.Setup(m => m.CompanyName).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.Importer);
		}

		public void TestEmptySeller()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns(ZString.Empty);
			supplierMock.Setup(m => m.RepresentativeName).Returns("OMR ENGR");
			supplierMock.Setup(m => m.CountryCode).Returns("JP");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Seller.Id);

			supplierMock.Setup(m => m.RepresentativeName).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Seller.Name);

			supplierMock.Setup(m => m.CountryCode).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Seller);
		}

		public void TestEmptyInvoiceLine()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var invoiceLines1Mock = new Mock<IImportInvoiceLine>();
			invoiceLines1Mock.Setup(m => m.InvoiceLineNo).Returns(ZInt.Zero);
			invoiceLines1Mock.Setup(m => m.ItemDescription).Returns("description");
			invoiceLines1Mock.Setup(m => m.Ingredient).Returns("ingredient");
			invoiceLines1Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns("PC");
			invoiceLines1Mock.Setup(m => m.InvoiceQuantity).Returns(1);
			invoiceLines1Mock.Setup(m => m.UnitPrice).Returns(1);
			invoiceLines1Mock.Setup(m => m.Amount).Returns(1);
			invoiceLines1Mock.Setup(m => m.PartNumber).Returns("1");

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLines1Mock.Object });
			var gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			invoiceLines1Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object });

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].SequenceNumeric);

			invoiceLines1Mock.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Description);

			invoiceLines1Mock.Setup(m => m.Ingredient).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Constituent);

			invoiceLines1Mock.Setup(m => m.InvoiceUnitOfQuantiy).Returns(ZString.Empty);
			invoiceLines1Mock.Setup(m => m.InvoiceQuantity).Returns(0);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity);

			invoiceLines1Mock.Setup(m => m.UnitPrice).Returns(0);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].UnitPriceAmount);

			invoiceLines1Mock.Setup(m => m.Amount).Returns(0);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].ValueAmount);

			invoiceLines1Mock.Setup(m => m.PartNumber).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);
		}

		public void TestEmptyEntryHeaderDataSimple()
		{
			importHeaderMock.Setup(m => m.Incoterm).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CourierCompanyID).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.AuthorizedImporterRegNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.OwnerReferenceNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.GoldTradeTransactionYN).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(new ZDateTime(ZString.Empty));
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.TradeTerms.ConditionCode);
			AssertNull(result.GoodsShipment.Consignment.Express);
			AssertNull(result.Agent);
			AssertNull(result.GoodsShipment.Ucr.TraderAssignedReferenceId);
			AssertNull(result.SouthNorthTrade);
		}

		public void TestEmptyGoodsShipmentCustomsValuation()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.Freight).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.Insurance).Returns(1);
			importHeaderMock.Setup(m => m.AdditionalAmount).Returns(1);
			importHeaderMock.Setup(m => m.DeductedAmount).Returns(1);
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.CustomsValuation.FreightChargeAmount);

			importHeaderMock.Setup(m => m.Insurance).Returns(ZDecimal.Zero);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount);

			importHeaderMock.Setup(m => m.AdditionalAmount).Returns(ZDecimal.Zero);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.GoodsShipment.CustomsValuation.ChargeDeduction.Any(c => c.ChargesTypeCode.Value == "01"));

			importHeaderMock.Setup(m => m.DeductedAmount).Returns(ZDecimal.Zero);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.CustomsValuation);
		}

		public void TestEmptyAdditionalInformation()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(ZDateTime.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerComment2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode1).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode2).Returns("test CustomsBrokerCommentCode2");
			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode3).Returns("test CustomsBrokerCommentCode3");
			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns("B");
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.AdditionalInformation.StatementCode.Any(s => s.Name == Constants.AdditionalInformationStatementCodes_929._257));
			AssertNull(result.AdditionalInformation.Content);
			AssertNull(result.AdditionalInformation.StatementDescription);

			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode2).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.AdditionalInformation.StatementCode.Any(s => s.Name == Constants.AdditionalInformationStatementCodes_929._258));

			importHeaderMock.Setup(m => m.CustomsBrokerCommentCode3).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.StatementCode);

			AssertEquals("B", result.AdditionalInformation.UsageDclarationCode.Value);

			importHeaderMock.Setup(m => m.BondedFactoryUseCode).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation);
		}

		public void TestEmptyDutyTaxFee()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.TotalSpecialConsumptionTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalTransportationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalLiquorTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalEducationTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalAgricultureTax).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForLateDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PenaltyForMissedDeclaration).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalValueForVAT).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalVATExemptionValue).Returns(1);
			importHeaderMock.Setup(m => m.TotalPayableAmount).Returns(1);

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes.IND));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5AA));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes.ACT));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5AB));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes.VAT));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5AC));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5AY));
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes.CAP));
			var dutyTaxFee = result.DutyTaxFee.First(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5CZ);
			AssertNull(dutyTaxFee.TotalVatBaseAmount);

			importHeaderMock.Setup(m => m.TotalVATExemptionValue).Returns(ZDecimal.Zero);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			dutyTaxFee = result.DutyTaxFee.First(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5CZ);
			AssertNull(dutyTaxFee.TotalVatExemptionBaseAmount);

			importHeaderMock.Setup(m => m.TotalPayableAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.TotalEducationTax).Returns(ZDecimal.Zero);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.DutyTaxFee.Any(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5CZ));
		}

		public void TestEmptyGAApprovalDocument()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var invoiceLines1Mock = new Mock<IImportInvoiceLine>();
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLines1Mock.Object });
			var gAApprovalDocuments1Mock = new Mock<IImportGAApprovalDocument>();
			invoiceLines1Mock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gAApprovalDocuments1Mock.Object });
			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns("11");
			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns("id");
			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns("category");
			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns("approval number");
			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(ZDate.Today);
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns("document");

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].Name);

			gAApprovalDocuments1Mock.Setup(m => m.UseCode).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].IntendedUseCode);

			gAApprovalDocuments1Mock.Setup(m => m.UniqueItemID).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].CommercialCategorizationId);

			gAApprovalDocuments1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].CriteriaConformanceCode);

			gAApprovalDocuments1Mock.Setup(m => m.RequirementApprovalNumber).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].Id);

			gAApprovalDocuments1Mock.Setup(m => m.ApprovalDate).Returns(ZDate.Empty);
			gAApprovalDocuments1Mock.Setup(m => m.RequirementDocumentType).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);
		}

		public void TestEmptyNonGADetail()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var nonDADetails1Mock = new Mock<IImportNonGADetail>();
			nonDADetails1Mock.Setup(m => m.SequenceNo).Returns(1);
			nonDADetails1Mock.Setup(m => m.ReasonType).Returns("reason type");
			nonDADetails1Mock.Setup(m => m.RegulationCategoryCode).Returns("category code");

			entryLine1Mock.Setup(m => m.NonGADetails).Returns(new IImportNonGADetail[] { nonDADetails1Mock.Object });
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].Content);

			nonDADetails1Mock.Setup(m => m.ReasonType).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].ReconciliationReasonCode);

			nonDADetails1Mock.Setup(m => m.RegulationCategoryCode).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation[0].CriteriaConformanceCode);

			nonDADetails1Mock.Setup(m => m.SequenceNo).Returns(0);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation);
		}

		public void TestEmptyPreviousExpDecLine()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var previousExpDecLine1Mock = new Mock<IImportPreviousExpDecLine>();
			previousExpDecLine1Mock.Setup(m => m.DeclarationNumber).Returns("1");
			previousExpDecLine1Mock.Setup(m => m.EntryLineNo).Returns(1);
			previousExpDecLine1Mock.Setup(m => m.InvoiceLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.PreviousExpDecLines).Returns(new IImportPreviousExpDecLine[] { previousExpDecLine1Mock.Object });
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].GoodsMeasure);

			previousExpDecLine1Mock.Setup(m => m.InvoiceLineNo).Returns(0);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].SequenceNumeric);

			previousExpDecLine1Mock.Setup(m => m.DeclarationNumber).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument[0].Id);

			previousExpDecLine1Mock.Setup(m => m.EntryLineNo).Returns(0);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
		}

		public void TestEmptyOtherGroups()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var invoiceLinesMock = new Mock<IImportInvoiceLine>();
			var gaApprovalMock = new Mock<IImportGAApprovalDocument>();
			invoiceLinesMock.Setup(m => m.GAApprovalDocuments).Returns(new IImportGAApprovalDocument[] { gaApprovalMock.Object });
			var nonGADetailsMock = new Mock<IImportNonGADetail>();
			var previousExpDecLinesMock = new Mock<IImportPreviousExpDecLine>();
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImportInvoiceLine[] { invoiceLinesMock.Object });
			entryLine1Mock.Setup(m => m.NonGADetails).Returns(new IImportNonGADetail[] { nonGADetailsMock.Object });
			entryLine1Mock.Setup(m => m.PreviousExpDecLines).Returns(new IImportPreviousExpDecLine[] { previousExpDecLinesMock.Object });
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
		}

		public void TestEntryLineEmptyTaxFee()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			entryLine1Mock.Setup(m => m.DutyReductionAmount).Returns(0);
			entryLine1Mock.Setup(m => m.DutyReductionClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionOrInstallmentCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DutyReductionRate).Returns(0);
			entryLine1Mock.Setup(m => m.AdValoremDutyRate).Returns(0);

			entryLine1Mock.Setup(m => m.DomesticTaxClassification).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfSpecialConsumptionTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxRate).Returns(0);
			entryLine1Mock.Setup(m => m.ExemptionCodeOfLiquorTax).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.DomesticTaxBaseQtyOrPrice).Returns(0);

			entryLine1Mock.Setup(m => m.EducationTaxExemptIndicator).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.AgricultureTaxClassification).Returns(ZString.Empty);

			entryLine1Mock.Setup(m => m.VATReductionCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.VATRateCode).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.ValueExemptForVAT).Returns(0);

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(1, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.Count);
			var cudDutyTaxFee = result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.First(d => d.TypeCode.Value == EntryTaxTypeList.Codes.CUD);
			AssertNull(cudDutyTaxFee.DeductAmount);
			AssertNull(cudDutyTaxFee.DutyTaxDecuctTypeCode);
			AssertNull(cudDutyTaxFee.DutyTaxDeductionInstallmentId);
			AssertNull(cudDutyTaxFee.DeductionRateNumeric);
			AssertEquals(0m, cudDutyTaxFee.TaxRateNumeric);
			AssertEquals(2, cudDutyTaxFee.AdValoremTaxBaseAmount.Count);
			AssertEquals(true, cudDutyTaxFee.AdValoremTaxBaseAmount.Any(a => a.CurrencyId == Iso3AlphaCurrencyCodeContentType.Krw));
			AssertEquals(true, cudDutyTaxFee.AdValoremTaxBaseAmount.Any(a => a.CurrencyId == Iso3AlphaCurrencyCodeContentType.Usd));

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.FirstOrDefault(d => d.TypeCode.Value == EntryTaxTypeList.Codes.LCN));
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.FirstOrDefault(d => d.TypeCode.Value == EntryTaxTypeList.Codes._5AB));
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.FirstOrDefault(d => d.TypeCode.Value == EntryTaxTypeList.Codes.CAP));
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.FirstOrDefault(d => d.TypeCode.Value == EntryTaxTypeList.Codes.VAT));
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DutyTaxFee.FirstOrDefault(d => d.TypeCode.Value == EntryTaxTypeList.Codes.FCU));
		}

		public void TestEntryLineEmptyOthers()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns("2");
			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns("3");
			entryLine1Mock.Setup(m => m.CountryOfOriginLabelType).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.CertificateOfOriginExemptionReason).Returns(ZString.Empty);
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(2, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency.Count);
			AssertEquals(true, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency.Any(r => r.Value == "2"));
			AssertEquals(true, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency.Any(r => r.Value == "3"));

			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency2).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(1, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency.Count);
			AssertEquals(true, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency.Any(r => r.Value == "3"));

			entryLine1Mock.Setup(m => m.PostClearanceProcedureAgency3).Returns(ZString.Empty);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ResponsibleGovernmentAgency);
		}

		public void TestNotCreateElementWhenNoMandatory_ImportEntryLine1()
		{
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { new Mock<IImportEntryLine>().Object });

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("레디코리아/김환태");
			declarantMock.Setup(m => m.MobileNumber).Returns("02-548-7636");
			declarantMock.Setup(m => m.ExtensionNumber).Returns(ZString.Empty);
			declarantMock.Setup(m => m.Email).Returns("snoopy2075@hanmail.net");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			#endregion

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			var governmentAgencyGoodsItem = result.GoodsShipment.GovernmentAgencyGoodsItem[0];

			AssertEquals(ZDecimal.Zero, governmentAgencyGoodsItem.SequenceNumeric);
			AssertNull(governmentAgencyGoodsItem.AdditionalCode);
			AssertNull(governmentAgencyGoodsItem.Commodity.SequenceNumeric);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertNull(governmentAgencyGoodsItem.Commodity.CharacteristicCode);
			AssertNull(governmentAgencyGoodsItem.Commodity.CountQuantity);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.Description.Value);
			AssertNull(governmentAgencyGoodsItem.Commodity.IntendedUseCode);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.Name.Value);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.NameCode.Value);
			AssertNull(governmentAgencyGoodsItem.Commodity.DetailedCountQuantity);
			AssertEquals(Constants.SupportingDocAttachedCode.AgreementTaxRateApplicationAllInputs, governmentAgencyGoodsItem.Commodity.AdditionalCode.AttachmentIndicatorCode.Value);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.AdditionalCode.CriteriaCode.Value);
			AssertNull(governmentAgencyGoodsItem.Commodity.AdditionalCode.ExaminationIndicatorCode);
			AssertNull(governmentAgencyGoodsItem.Commodity.AdditionalDocument);
			AssertNull(governmentAgencyGoodsItem.Commodity.CertificateOfOrigin);
			AssertEquals(ZString.Empty, governmentAgencyGoodsItem.Commodity.Classification.Id.Value);

			var importEntryLine = new Mock<IImportEntryLine>();
			importEntryLine.Setup(m => m.CertificateOfOriginCriteriaCode).Returns("01");
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { importEntryLine.Object });
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			var certificateOfOrigin = result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin;

			AssertNotNull(certificateOfOrigin);
			AssertEquals("01", certificateOfOrigin.CriteriaCode.Value);
			AssertNull(certificateOfOrigin.IssueDateTime);
			AssertNull(certificateOfOrigin.IssueId);
			AssertNull(certificateOfOrigin.IssueAgencyName);
			AssertNull(certificateOfOrigin.IssueLocationCode);
			AssertNull(certificateOfOrigin.IssueLocationName);
			AssertNull(certificateOfOrigin.IssuerName);
			AssertNull(certificateOfOrigin.SplitIndicatorCode);
			AssertNull(certificateOfOrigin.TotalGrossMassMeasure);
			AssertNull(certificateOfOrigin.TotalQuantityQuantity);
			AssertNull(certificateOfOrigin.UsageGrossMassMeasure);
			AssertNull(certificateOfOrigin.UsageQuantityQuantity);
		}

		public void TestEmptyArrivalDateTime()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			importHeaderMock.Setup(m => m.ArrivalDateAtDischargePort).Returns(ZDate.Invalid);
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.BorderTransportMeans.ArrivalDateTime);

			importHeaderMock.Setup(m => m.ArrivalDateAtDischargePort).Returns(new ZDate("2012-01-01"));
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("20120101", result.BorderTransportMeans.ArrivalDateTime);
		}

		public void TestEmptyCertificateOfOriginIssueDateTime()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			entryLine1Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(ZDate.Invalid);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			var governmentAgencyGoodsItem = result.GoodsShipment.GovernmentAgencyGoodsItem[0];
			AssertNull(governmentAgencyGoodsItem.Commodity.CertificateOfOrigin);

			entryLine1Mock.Setup(m => m.CertificateOfOriginCriteriaCode).Returns("01");
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			var certificateOfOrigin = result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin;
			AssertNull(certificateOfOrigin.IssueDateTime);

			entryLine1Mock.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2013-01-01"));
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			certificateOfOrigin = result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CertificateOfOrigin;
			AssertEquals("20130101", certificateOfOrigin.IssueDateTime);
		}

		public void TestEmptyUsageDateTime()
		{
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation);

			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(ZDateTime.Invalid);
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation);

			importHeaderMock.Setup(m => m.BondedFactoryUseDate).Returns(new ZDateTime("2012-02-10 10:12:00"));
			result = new GOVCBR929MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("20120210101200", result.AdditionalInformation.UsageDateTime);
		}
	}
}
