using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACRateHeader))]
	sealed class CACRateHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var rate = classHeader.ClassRates.AddNew();
			AssertEquals("ClassHeader", classHeader, rate.ClassHeader);
			AssertEquals("Rates", typeof(CACRateCollection), rate.Rates.GetType());

			//Delete
			var child = rate.Rates.AddNew();
			rate.Delete();
			AssertEquals("CACRateHeader ChildObject deleted", true, child.IsDeleted);
		}

		public void TestLoader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var rate1 = classHeader.ClassRates.AddNew();
			rate1.ZB_EffectiveDate = ZDateTime.Today.AddDays(-2);
			rate1.ZB_ExpiryDate = ZDateTime.Today.AddDays(+1);

			var rate = classHeader.ClassRates.AddNew();
			rate.ZB_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate.ZB_ExpiryDate = ZDateTime.Today.AddDays(+1);

			AssertEquals("CACRateHeader", rate, CACRateHeader.Load(classHeader, ZDateTime.Today, CACRateHeader.RateType.ClassificationRate));

			var invalidDate = new ZDateTime(" ");
			AssertEquals("CACRateHeader", rate, CACRateHeader.Load(classHeader, invalidDate, CACRateHeader.RateType.ClassificationRate));
			string expectedErrorMessage = "You should input a valid effectiveDate";
			AssertContains("An error should be thrown", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestLoadEffectiveRateHeaders()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var rate1 = classHeader.ClassRates.AddNew();
			rate1.ZB_EffectiveDate = ZDateTime.Today.AddDays(-2);
			rate1.ZB_ExpiryDate = ZDateTime.Today.AddDays(+1);

			var rate = classHeader.ClassRates.AddNew();
			rate.ZB_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate.ZB_ExpiryDate = ZDateTime.Today.AddDays(+1);

			AssertContainsExactElementsInExactOrder("CACRateHeaders order by ZB_EffectiveDate", new[] { rate, rate1 }, CACRateHeader.LoadEffectiveRateHeaders(classHeader, ZDateTime.Today, CACRateHeader.RateType.ClassificationRate));

			AssertEquals("Nothing loaded", 0, CACRateHeader.LoadEffectiveRateHeaders(classHeader, ZDateTime.Today.AddMonths(1), CACRateHeader.RateType.ClassificationRate).Length);
		}

		public void TestUniqueIndexForClassificationRate()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_AreaCode = "AAA";

			var rate = Factory.New<CACRateHeader>();
			rate.ZB_RateType = CACRateHeader.RateType.ClassificationRate;
			rate.ZB_ZA_ClassHeader = classHeader.PK;
			rate.ZB_EffectiveDate = ZDateTime.Today;
			rate = Factory.New<CACRateHeader>();
			rate.ZB_RateType = CACRateHeader.RateType.ClassificationRate;
			rate.ZB_ZA_ClassHeader = classHeader.PK;
			rate.ZB_EffectiveDate = new ZDateTime(2000, 10, 10);
			Factory.Save();

			rate.ZB_EffectiveDate = ZDateTime.Today;
			AssertExceptionThrownContains("IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate", typeof(ZSaveException), () => Factory.Save());
		}

		public void TestUniqueIndexForExciseDutyRate()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_AreaCode = "AAA";

			var rate = Factory.New<CACRateHeader>();
			rate.ZB_RateType = CACRateHeader.RateType.ExciseDutyRate;
			rate.ZB_ZA_ClassHeader = classHeader.PK;
			rate.ZB_UnitOfMeasure = UnitOfWeightList.Codes.Kilogram;
			rate.ZB_EffectiveDate = ZDateTime.Today;

			rate = Factory.New<CACRateHeader>();
			rate.ZB_RateType = CACRateHeader.RateType.ExciseDutyRate;
			rate.ZB_ZA_ClassHeader = classHeader.PK;
			rate.ZB_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			rate.ZB_UnitOfMeasure = UnitOfWeightList.Codes.Kilogram;
			AssertExceptionThrownContains("IX_CACRateHeader_ZB_ZA_ClassHeader_ZB_EffectiveDate_ZB_UnitOfMeasure", typeof(ZSaveException), () => Factory.Save());
		}

		void AssertExceptionThrownContains(string expectedMessage, Type expectedType, AnonymousMethod method)
		{
			try
			{
				method();
			}
			catch (Exception ex)
			{
				AssertEquals("Exception Type", expectedType, ex.GetType());
				AssertContains("Exception Message", expectedMessage, ex.Message);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var classHeader = factory.New<CACClassHeader>();
			classHeader.ZA_AreaCode = "A A";
			return classHeader.ClassRates.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_AreaCode = "A A";
			return classHeader.ClassRates.AddNew();
		}
	}
}
