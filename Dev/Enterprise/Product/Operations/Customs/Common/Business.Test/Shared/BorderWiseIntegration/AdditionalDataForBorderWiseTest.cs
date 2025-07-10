using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class AdditionalDataForBorderWiseTest : TestCaseWithFactory
	{
		#region TestDataSourceHasIHaveAdditionalDataForBorderWiseImplementedBlowsWhenWrong
		[TestDate(2005, 12, 4)]
		[ExpectNoExceptions]
		public void TestDataSourceHasIHaveAdditionalDataForBorderWiseImplementedBlowsWhenWrong()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Bool = true;
			dummyBO.Z0_Date = new ZDateTime(2004, 3, 23);
			try
			{
				NUnit.Framework.Assert.That(AdditionalDataForBorderWise.DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(dummyBO), Is.EqualTo(false), "AdditionalDataForBorderWise.DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(DummyBO)");
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise), Is.EqualTo(true), "ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise)");
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestDataSourceHasIHaveAdditionalDataForBorderWiseImplementedWorksWhenRight
		[TestDate(2005, 12, 4)]
		[ExpectNoExceptions]
		public void TestDataSourceHasIHaveAdditionalDataForBorderWiseImplementedWorksWhenRight()
		{
			DummyWithInterface dummyBO = Factory.New<DummyWithInterface>();
			dummyBO.Z0_Bool = true;
			dummyBO.Z0_Date = new ZDateTime(2004, 3, 23);
			NUnit.Framework.Assert.That(AdditionalDataForBorderWise.DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(dummyBO), Is.EqualTo(true), "AdditionalDataForBorderWise.DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(DummyBO)");
		}
		#endregion

		#region TestGetAdditionalDataFromWorksForBOWithIHaveAdditionalDataForBorderWise
		[ExpectNoExceptions]
		public void TestGetAdditionalDataFromWorksForBOWithIHaveAdditionalDataForBorderWise()
		{
			DummyWithInterface dummyBO = Factory.New<DummyWithInterface>();
			dummyBO.Z0_Bool = true;
			dummyBO.Z0_Date = new ZDateTime(2004, 3, 23);
			AdditionalDataForBorderWise additionalData = AdditionalDataForBorderWise.GetAdditionalDataFrom(dummyBO);
			NUnit.Framework.Assert.That(additionalData.ParameterForBorderWise, Is.EqualTo("I"), "AdditionalData.Parameterthingy");
			NUnit.Framework.Assert.That(additionalData.DateForDutyRate, Is.EqualTo(new ZDateTime(2004, 3, 23)), "AdditionalData.DateForDutyRate");
		}
		#endregion

		#region TestGetAdditionalDataFromWorksForBOWithoutIHaveAdditionalDataForBorderWise
		[TestDate(2005, 12, 4)]
		[ExpectNoExceptions]
		public void TestGetAdditionalDataFromWorksForBOWithoutIHaveAdditionalDataForBorderWise()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Bool = true;
			dummyBO.Z0_Date = new ZDateTime(2004, 3, 23);
			try
			{
				AdditionalDataForBorderWise additionalData = AdditionalDataForBorderWise.GetAdditionalDataFrom(dummyBO);
				NUnit.Framework.Assert.That(additionalData.ParameterForBorderWise, Is.EqualTo("E"), "AdditionalData.parameterForBorderWise");
				NUnit.Framework.Assert.That(additionalData.DateForDutyRate, Is.EqualTo(new ZDateTime(2005, 12, 4)), "AdditionalData.DateForDutyRate");
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise), Is.EqualTo(true), "ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise)");
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestGetAdditionalDataFromWorksForNull
		[TestDate(2005, 12, 4)]
		[ExpectNoExceptions]
		public void TestGetAdditionalDataFromWorksForNull()
		{
			try
			{
				AdditionalDataForBorderWise additionalData = AdditionalDataForBorderWise.GetAdditionalDataFrom(default(BusinessObject));
				NUnit.Framework.Assert.That(additionalData.ParameterForBorderWise, Is.EqualTo("E"), "AdditionalData.parameterForBorderWise");
				NUnit.Framework.Assert.That(additionalData.DateForDutyRate, Is.EqualTo(new ZDateTime(2005, 12, 4)), "AdditionalData.DateForDutyRate");
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectIsNull), Is.EqualTo(true), "ErrorReporter.LastMessageReported.Contains(AdditionalDataForBorderWise.DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise)");
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestConstructor
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			AdditionalDataForBorderWise additionalDataForFiltering = new AdditionalDataForBorderWise("I", new ZDateTime(2005, 12, 22), "FR");
			NUnit.Framework.Assert.That(additionalDataForFiltering.ParameterForBorderWise, Is.EqualTo("I"), "AdditionalDataForFiltering.parameterForBorderWise");
			NUnit.Framework.Assert.That(additionalDataForFiltering.DateForDutyRate, Is.EqualTo(new ZDateTime(2005, 12, 22)), "AdditionalDataForFiltering.DateForDutyRate");
			NUnit.Framework.Assert.That(additionalDataForFiltering.CountryCodeOverride, Is.EqualTo("FR"), "AdditionalDataForFiltering.CountryCode");
		}
		#endregion

		[ExpectNoExceptions]
		public void TestGetAdditionalDataFromWithParameters()
		{
			NUnit.Framework.Assert.That(AdditionalDataForBorderWise.GetAdditionalDataFrom("", DateTime.Now, "AU").ParameterForBorderWise, Is.EqualTo("E"), "AdditionalData.ParameterForBorderWise");

			NUnit.Framework.Assert.That(AdditionalDataForBorderWise.GetAdditionalDataFrom("I", DateTime.Now, "AU").ParameterForBorderWise, Is.EqualTo("I"), "AdditionalData.ParameterForBorderWise");

			var dateForDutyRate = DateTime.Now;
			var additionalData = AdditionalDataForBorderWise.GetAdditionalDataFrom("E", dateForDutyRate, "AU");
			NUnit.Framework.Assert.That(additionalData.ParameterForBorderWise, Is.EqualTo("E"), "AdditionalData.ParameterForBorderWise");
			NUnit.Framework.Assert.That(additionalData.DateForDutyRate, Is.EqualTo(dateForDutyRate).Using(CustomComparers.TypeComparison), "AdditionalData.DateForDutyRate");
			NUnit.Framework.Assert.That(additionalData.CountryCodeOverride, Is.EqualTo("AU"), "AdditionalData.CountryCodeOverride");
		}

		#region class DummyWithInterface
		class DummyWithInterface : DummyBusinessObject, IHaveAdditionalDataForBorderWise
		{
			public DummyWithInterface(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
			{
				return new AdditionalDataForBorderWise(Z0_Bool ? "I" : "E", Z0_Date);
			}

			public Type ExpectedBusinessObjectTypeForList
			{
				get { return null; }
			}
		}
		#endregion
	}
}
