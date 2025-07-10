using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACClassHeader))]
	sealed class CACClassHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCollections()
		{
			var classHeader = Factory.New<CACClassHeader>();
			AssertEquals("RefNumbers", typeof(CACTaxRefNumHeaderCollection), classHeader.RefNumbers.GetType());
			var childObjects
				= new List<BusinessObject>
					{
						CreateNewRate(CACRateHeader.RateType.ClassificationRate, classHeader),
						CreateNewRate(CACRateHeader.RateType.ClassificationRate, classHeader),
						CreateNewRate(CACRateHeader.RateType.ClassificationRate, classHeader),
						CreateNewRate(CACRateHeader.RateType.ExciseDutyRate, classHeader),
						CreateNewRate(CACRateHeader.RateType.ExciseDutyRate, classHeader),
						classHeader.RefNumbers.AddNew()
					};

			AssertEquals("ClassRates", 3, classHeader.ClassRates.Count);
			AssertEquals("ExciseDutyRates", 2, classHeader.ExciseDutyRates.Count);
			AssertEquals("RefNumbers", 1, classHeader.RefNumbers.Count);

			//Delete
			classHeader.Delete();
			foreach (var childObject in childObjects)
			{
				AssertEquals("CACClassHeader ChildObject deleted", true, childObject.IsDeleted);
			}
		}

		public void TestLoad()
		{
			CreateNewClassHeader(ZDateTime.Today.AddDays(+1), ZDateTime.Today.AddDays(+1), false);
			CreateNewClassHeader(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(+1), false);
			var classHeader = CreateNewClassHeader(ZDateTime.Today, ZDateTime.Today, false);
			AssertEquals("CACClassHeader", classHeader, CACClassHeader.Load(Factory, ZDateTime.Today, "12345"));
		}

		[ExpectNoExceptions]
		public void TestLoadWithInvalidEffectiveDate()
		{
			CreateNewClassHeader(new ZDateTime(" "), ZDateTime.Today, false);
			CACClassHeader.Load(Factory, new ZDateTime(" "), "12345");
			string expectedErrorMessage = "You should input a valid effectiveDate";
			AssertContains("An error should be thrown", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region Implementation

		CACRateHeader CreateNewRate(string type, CACClassHeader classHeader)
		{
			var rate = Factory.New<CACRateHeader>();
			rate.ZB_RateType = type;
			rate.ZB_ZA_ClassHeader = classHeader.PK;
			return rate;
		}

		CACClassHeader CreateNewClassHeader(ZDateTime effectiveDate, ZDateTime expDate, bool inactive)
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "12345";
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expDate;
			classHeader.ZA_InactiveInd = inactive;
			return classHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_AreaCode = "AAA";
			return classHeader;
		}

		#endregion
	}
}
