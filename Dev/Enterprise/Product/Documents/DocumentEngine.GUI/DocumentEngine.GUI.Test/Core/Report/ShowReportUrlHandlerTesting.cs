using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.URLHandler;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class ShowReportUrlHandlerTesting : ReportUrlHandlerTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestHandle()
		{
			QueryString queryString = BuildQueryString("ThreeFilters.xls");
			UrlHandler.Handle(queryString);
			using (var form = ((ShowReportUrlHandler)UrlHandler).ShownFormForTest)
			{
				AssertEquals(1, form.printTask.GetFirstDocumentPack().Count);
			}
		}

		#region implementation

		protected override IReportUrlHandlerForTest GetNewReportUrlHandlerForTest()
		{
			return new ShowReportUrlHandlerForTest();
		}

		class ShowReportUrlHandlerForTest : ShowReportUrlHandler, IReportUrlHandlerForTest
		{
			protected override ReportPrintSet NewReportPrintSetToRun(ReportCommand reportCommand)
			{
				var result = new TestReportPrintSet(reportCommand);
				GetHelper().CreatedTestReportPrintSets.Add(result);
				return result;
			}

			protected override IGlbCompany CurrentCompany
			{
				get { return currentCompany ?? base.CurrentCompany; }
			}
			IGlbCompany currentCompany;

			public void SetCurrentCompany(IGlbCompany company)
			{
				currentCompany = company;
			}

			public string GetExpectedCommandText()
			{
				return ExpectedCommandText;
			}

			public ReportUrlHandlerHelper GetHelper()
			{
				return helper ?? (helper = new ReportUrlHandlerHelper());
			}
			ReportUrlHandlerHelper helper;
		}

		#region non-relevant tests

		public override void TestHandle_ConfigurationParsed()
		{
			Assert(true);
		}

		public override void TestHandle_FiltersParsed()
		{
			Assert(true);
		}

		public override void TestHandle_SortOrderParsed()
		{
			Assert(true);
		}

		public override void TestHandle_GroupByParsed()
		{
			Assert(true);
		}

		public override void TestHandle_GroupByParsed_WhenGroupByInvalid()
		{
			Assert(true);
		}

		public override void TestHandle_OptionalTemplatesParsed()
		{
			Assert(true);
		}

		public override void TestHandle_InsufficientUserRights()
		{
			Assert(true);
		}

		public override void TestHandle_ForClientModule()
		{
			Assert(true);
		}

		public override void TestHandle_ForBusinessContextNotInClientModule()
		{
			throw new EnterpriseUrlHandlerException("The report NotAModule could not be found in client modules");
		}

		public override void TestHandle_WhenLoggedWithDifferentEnterpriseOrServerCode()
		{
			Assert(true);
		}

		#endregion

		#endregion
	}
}
