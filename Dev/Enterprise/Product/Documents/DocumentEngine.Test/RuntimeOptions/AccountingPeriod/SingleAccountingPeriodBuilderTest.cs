using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SingleAccountingPeriodBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new SingleAccountingPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			var filterBuilder = new SingleAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(SingleAccountingPeriodField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild1()
		{
			var filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
			AssertEquals(true, filterBuilder.CanBuild("Single Period"));
		}

		public void TestCanBuild2()
		{
			var filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
			AssertEquals(true, filterBuilder.CanBuild("Single Accounting Period"));
		}

		public void TestCanBuild3()
		{
			var filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
			AssertEquals(false, filterBuilder.CanBuild("Accounting Period"));
		}

		public void TestCanBuild4()
		{
			var filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
			AssertEquals(false, filterBuilder.CanBuild("Single Accounting"));
		}

		[TestDate(2012, 3, 8)]
		public void TestICustomBuilder()
		{
			var filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
			var customBuilder = filterBuilder as ICustomBuilder;
			AssertNotNull("Must implement ICustomBuilder", customBuilder);
			 
			var filterTree = new StringTreeNode();
			filterTree.Children.Add(new StringTreeNode() { Value = FilterBuilderPropertyCodeDescriptionList.Codes.OnlyCurrentPeriodIfPayByWebService });
			var filterField = (SingleAccountingPeriodField)filterBuilder.Build(filterTree, new StringCollection());
			Assert("Should be empty by default", filterField.SinglePeriod.IsEmpty);

			new AccountingPeriodTestHelper().SetupSinglePeriod(201203, new ZDateTime(2012, 3, 1), new ZDateTime(2012, 3, 31));

			// Mock Payment Web Service enabled
			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute(mockSupporter.Object))
			{
				filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
				customBuilder = filterBuilder;

				filterField = (SingleAccountingPeriodField)filterBuilder.Build(filterTree, new StringCollection());
				Assert("Should be Empty because it is not required", filterField.SinglePeriod.IsEmpty);

				filterTree.Children.Add(new StringTreeNode() { Value = FilterBuilderPropertyCodeDescriptionList.Codes.Required });
				filterBuilder = (SingleAccountingPeriodBuilder)GetFilterBuilderToTest();
				customBuilder = filterBuilder;

				filterField = (SingleAccountingPeriodField)filterBuilder.Build(filterTree, new StringCollection());
				AssertEquals("Should be current period", 201203, filterField.SinglePeriod);
			}
		}

		public class SingleAccountingPeriodBuilderForTesting : SingleAccountingPeriodBuilder
		{
			public SingleAccountingPeriodBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
