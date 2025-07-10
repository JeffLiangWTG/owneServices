using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditReportItemCollection))]
	sealed class CreditReportsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CreditReportItemCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestAllowSort()
		{
			var collection = new CreditReportItemCollectionForTest();
			AssertEquals(false, collection.AllowSortForTest);
		}

		public void TestAllowNewCore()
		{
			var collection = new CreditReportItemCollectionForTest();
			AssertEquals(false, collection.AllowNewCoreForTest);
		}

		public void TestAllowRemoveCore()
		{
			var collection = new CreditReportItemCollectionForTest();
			AssertEquals(false, collection.AllowRemoveCoreForTest);
		}

		public new void TestClone()
		{
			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new CreditReportItemCollection();
			var originalReport = collection.AddNew();
			originalReport.CountryCode = "Dummy Code1";
			originalReport.Country = "Dummy Country1";
			originalReport.CountryEnabledForCompany = true;
			originalReport.CountryEnabledForOrganisation = true;
			originalReport.CommercialBureauEnquiryEnabled = true;
			originalReport.ComprehensiveReportEnabled = true;
			originalReport.FailureRiskEnabled = false;
			originalReport.LatePaymentRiskEnabled = false;

			var report2 = collection.AddNew();
			report2.CountryCode = "Dummy Code2";
			report2.Country = "Dummy Country2";
			report2.CountryEnabledForCompany = false;
			report2.CountryEnabledForOrganisation = false;

			var clone = collection.Clone(currentFallbackLevel, Factory);
			AssertNotEquals("Clone should be a different instance.", collection, clone);
			var cloneElement1 = clone[0] as CreditReportItem;
			var cloneElement2 = clone[1] as CreditReportItem;

			CombineAssertions("clone element1 and original report should have identical properties", () =>
			{
				AssertEquals(cloneElement1.CountryCode, originalReport.CountryCode);
				AssertEquals(cloneElement1.Country, originalReport.Country);
				AssertEquals(cloneElement1.CountryEnabledForCompany, originalReport.CountryEnabledForCompany);
				AssertEquals(cloneElement1.CountryEnabledForOrganisation, originalReport.CountryEnabledForOrganisation);
				AssertEquals(cloneElement1.FailureRiskEnabled, originalReport.FailureRiskEnabled);
				AssertEquals(cloneElement1.CommercialBureauEnquiryEnabled, originalReport.CommercialBureauEnquiryEnabled);
				AssertEquals(cloneElement1.LatePaymentRiskEnabled, originalReport.LatePaymentRiskEnabled);
				AssertEquals(cloneElement1.ComprehensiveReportEnabled, originalReport.ComprehensiveReportEnabled);
			});

			CombineAssertions("clone element2 and report2 should have identical properties", () =>
			{
				AssertEquals(cloneElement2.CountryCode, report2.CountryCode);
				AssertEquals(cloneElement2.Country, report2.Country);
				AssertEquals(cloneElement2.CountryEnabledForCompany, report2.CountryEnabledForCompany);
				AssertEquals(cloneElement2.CountryEnabledForOrganisation, report2.CountryEnabledForOrganisation);
				AssertEquals(cloneElement2.FailureRiskEnabled, report2.FailureRiskEnabled);
				AssertEquals(cloneElement2.CommercialBureauEnquiryEnabled, report2.CommercialBureauEnquiryEnabled);
				AssertEquals(cloneElement2.LatePaymentRiskEnabled, report2.LatePaymentRiskEnabled);
				AssertEquals(cloneElement2.ComprehensiveReportEnabled, report2.ComprehensiveReportEnabled);
			});
		}

		protected override CreditReportItemCollection GetCollectionToTest()
		{
			return new CreditReportItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CreditReportItem();
		}
	}
}
