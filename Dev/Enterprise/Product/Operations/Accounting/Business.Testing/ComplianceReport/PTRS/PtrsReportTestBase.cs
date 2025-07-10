using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using static Enterprise.Accounting.Business.ComplianceReport.AccTaxReturn;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	public abstract class PtrsReportTestBase : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var ptrsReport = (PtrsReportBase)Factory.New(GetExpectedBusinessObjectType());
			AssertNotNull("New PTRS Tax Return", ptrsReport);

			AssertEquals(nameof(ptrsReport.ATR_ReturnType), ReturnType.PTRS, ptrsReport.ATR_ReturnType);
			AssertEquals(nameof(ptrsReport.ATR_Status), ZString.Empty, ptrsReport.ATR_Status);
			AssertEquals(nameof(ptrsReport.ATR_Version), 1, ptrsReport.ATR_Version);
		}

		public void TestPopulatingReportCompanyData()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var orgProxy = Factory.Load<OrgHeader>(company.GC_OH_OrgProxy);
			var orgABN = orgProxy.CustomsCodes.AddNew();
			orgABN.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgABN.OK_CustomsRegNo = "22-255-588-899-972";
			Factory.Save();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();

			var ptrsReport = (PtrsReportBase)GetNewBusinessObject();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("ATR_CompanyName", "EDI CUSTOMS BROKERS", ptrsReport.ATR_CompanyName);
			AssertEquals("ATR_VATRegNo", "22255588899972", ptrsReport.ATR_VATRegNo);
			AssertEquals("ATR_Address1", "184 Bourke Road", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "", ptrsReport.ATR_Address2);
			AssertEquals("ATR_City", "Alexandria", ptrsReport.ATR_City);
			AssertEquals("ATR_State", "NSW", ptrsReport.ATR_State);
			AssertEquals("ATR_PostCode", "2015", ptrsReport.ATR_PostCode);
			AssertEquals("ATR_RN_NKCountryCode", "AU", ptrsReport.ATR_RN_NKCountryCode);

			orgABN.Delete();
			company.GC_Address1 = "74 O'Riordan Street";
			company.GC_Address2 = "Main Reception";
			Factory.Save();
			AssertEquals("Saved", AccTaxReturn.Status.Saved, ptrsReport.ATR_Status);

			var newFactory = new BusinessObjectFactory();
			ptrsReport = (PtrsReportBase)newFactory.Load(GetExpectedBusinessObjectType(), ptrsReport.PK);
			AssertEquals("ATR_VATRegNo", "", ptrsReport.ATR_VATRegNo);
			AssertEquals("ATR_Address1", "74 O'Riordan Street", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "Main Reception", ptrsReport.ATR_Address2);

			// Make ptrsReport submitted 
			newFactory.Save();
			ptrsReport.SubmitReport();
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			newFactory.Save();

			// Change Company details
			orgABN = orgProxy.CustomsCodes.AddNew();
			orgABN.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			orgABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgABN.OK_CustomsRegNo = "11-234-567-899-972";

			company.GC_Address1 = "25 Bourke Road";
			company.GC_Address2 = "Reception at Level 2";
			Factory.Save();

			// No changes to company details on ptrsRreport after submitting
			newFactory = new BusinessObjectFactory();
			ptrsReport = (PtrsReportBase)newFactory.Load(GetExpectedBusinessObjectType(), ptrsReport.PK);
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			AssertEquals("ATR_Address1", "74 O'Riordan Street", ptrsReport.ATR_Address1);
			AssertEquals("ATR_Address2", "Main Reception", ptrsReport.ATR_Address2);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var complianceReport = factory.NewWithValidTestData<AccComplianceReport>();

			var ptrsReport = (PtrsReportBase)base.GetNewBusinessObjectForDeleteTest(factory);
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			return ptrsReport;
		}

		#region Implementation

		protected PtrsAllPaymentsReport2024Data DefaultAllPaymentsData = new PtrsAllPaymentsReport2024Data()
		{
			SmallBusinessPartialPaymentAmount = 100m,
			SmallBusinessFullPaymentAmount = 200m,
			OthersPartialPaymentAmount = 300m,
			OthersFullPaymentAmount = 400m,
		};

		protected PtrsReport2024Data DefaultPtrsData = new PtrsReport2024Data()
		{
			MostCommonPaymentTerm = 15,
			PaymentTermMin = 10,
			PaymentTermMax = 30,

			AveragePaymentTime = 11.53m,
			MedianPaymentTime = 10.55m,

			PaymentTimeOf80thPercentile = 12,
			PaymentTimeOf95thPercentile = 15,

			PercentagePaidWithinTerm = 95.97m,
			PercentagePaidWithin30days = 89.53m,
			PercentagePaidBetween31And60Days = 8.83m,
			PercentagePaidAfter60Days = 1.74m,

			SmallBusinessPaymentPercentage = 11.73m,
		};

		protected PtrsAllPaymentsReport2024Data AlternativeAllPaymentsData = new PtrsAllPaymentsReport2024Data()
		{
			SmallBusinessPartialPaymentAmount = 101.11m,
			SmallBusinessFullPaymentAmount = 202.22m,
			OthersPartialPaymentAmount = 303.03m,
			OthersFullPaymentAmount = 404.04m,
		};

		protected PtrsReport2024Data AlternativePtrsData = new PtrsReport2024Data()
		{
			MostCommonPaymentTerm = 14,
			PaymentTermMin = 7,
			PaymentTermMax = 60,

			AveragePaymentTime = 8.55m,
			MedianPaymentTime = 7.57m,

			PaymentTimeOf80thPercentile = 10,
			PaymentTimeOf95thPercentile = 12,

			PercentagePaidWithinTerm = 95.88m,
			PercentagePaidWithin30days = 90.57m,
			PercentagePaidBetween31And60Days = 7.71m,
			PercentagePaidAfter60Days = 1.82m,

			SmallBusinessPaymentPercentage = 10.65m,
		};

		protected Mock<IPtrsReport2024Helper> SetupReport2024HelperMock(BusinessObjectFactory factory, PtrsAllPaymentsReport2024Data allPaymentsData, PtrsReport2024Data smallBusinessData)
		{
			var mockHelper = new Mock<IPtrsReport2024Helper>();

			mockHelper.Setup(x =>
				x.CalculatePtrsAllPaymentsReport2024Data(It.IsAny<AccComplianceReport>()))
				.Returns<AccComplianceReport>((_) => allPaymentsData);

			mockHelper.Setup(x =>
				x.CalculatePtrsReport2024Data(It.IsAny<AccComplianceReport>(), It.IsAny<PtrsAllPaymentsReport2024Data>()))
				.Returns<AccComplianceReport, PtrsAllPaymentsReport2024Data>((_, _) => smallBusinessData);

			if (factory.ServiceContainer.GetService<IPtrsReport2024Helper>() != null)
			{
				factory.ServiceContainer.RemoveService<IPtrsReport2024Helper>();
			}

			factory.ServiceContainer.AddService<IPtrsReport2024Helper>(mockHelper.Object);
			return mockHelper;
		}

		protected void AssertPtrsAllPaymentsReport2024Data(PtrsAllPaymentsReport2024 report, PtrsAllPaymentsReport2024Data data)
		{
			AssertNotNull("All Payments Report", report);

			AssertEquals(nameof(report.SmallBusinessPartialPaymentAmount), data.SmallBusinessPartialPaymentAmount, report.SmallBusinessPartialPaymentAmount);
			AssertEquals(nameof(report.SmallBusinessFullPaymentAmount), data.SmallBusinessFullPaymentAmount, report.SmallBusinessFullPaymentAmount);
			AssertEquals(nameof(report.OthersPartialPaymentAmount), data.OthersPartialPaymentAmount, report.OthersPartialPaymentAmount);
			AssertEquals(nameof(report.OthersFullPaymentAmount), data.OthersFullPaymentAmount, report.OthersFullPaymentAmount);
		}

		protected void AssertPtrsReport2024Data(PtrsReport2024 report, PtrsReport2024Data data)
		{
			AssertNotNull("Small Business Full Payments Report", report);

			AssertEquals(nameof(report.MostCommonPaymentTerm), data.MostCommonPaymentTerm, report.MostCommonPaymentTerm);
			AssertEquals(nameof(report.PaymentTermMin), data.PaymentTermMin, report.PaymentTermMin);
			AssertEquals(nameof(report.PaymentTermMax), data.PaymentTermMax, report.PaymentTermMax);

			AssertEquals(nameof(report.AveragePaymentTime), data.AveragePaymentTime, report.AveragePaymentTime);
			AssertEquals(nameof(report.MedianPaymentTime), data.MedianPaymentTime, report.MedianPaymentTime);

			AssertEquals(nameof(report.PaymentTimeOf80thPercentile), data.PaymentTimeOf80thPercentile, report.PaymentTimeOf80thPercentile);
			AssertEquals(nameof(report.PaymentTimeOf95thPercentile), data.PaymentTimeOf95thPercentile, report.PaymentTimeOf95thPercentile);

			AssertEquals(nameof(report.PercentagePaidWithinTerm), data.PercentagePaidWithinTerm, report.PercentagePaidWithinTerm);
			AssertEquals(nameof(report.PercentagePaidWithin30days), data.PercentagePaidWithin30days, report.PercentagePaidWithin30days);
			AssertEquals(nameof(report.PercentagePaidBetween31And60Days), data.PercentagePaidBetween31And60Days, report.PercentagePaidBetween31And60Days);
			AssertEquals(nameof(report.PercentagePaidAfter60Days), data.PercentagePaidAfter60Days, report.PercentagePaidAfter60Days);

			AssertEquals(nameof(report.SmallBusinessPaymentPercentage), data.SmallBusinessPaymentPercentage, report.SmallBusinessPaymentPercentage);
		}

		#endregion
	}
}
