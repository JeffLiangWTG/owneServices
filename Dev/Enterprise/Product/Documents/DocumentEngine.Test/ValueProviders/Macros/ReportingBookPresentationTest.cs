using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ReportingBookPresentation))]
	sealed class ReportingBookPresentationTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<ReportingBookPresentation>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< ReportingBookPresentation (    Tst ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<ReportingBookPresentation(C1BC517C-5DF8-4556-A4CA-02472BAB1360)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<ReportingBookPresentation(C1BC517C-5DF8-4556-A4CA-02472BAB136066666)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<ReportingBookPresentation( C1BC517C-5DF8-4556-A4CA-02472BAB136066 )>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var mock = new Mock<IAccounting>();
			var list = new CodeDescriptionPairList();
			list.AddPair("OUC", "Group 1");
			list.AddPair("IOS", "Category 2");
			mock.Setup(m => m.GLPresentationJournalCategoriesList(GlbCompany.CurrentCompany.PK.ToGuid()))
				.Returns(list);
			mock.Setup(m => m.GetCategorisWithChildren("OUC")).Returns("OUC,SU1,SU2");
			using (ObjectFactory.Substitute(mock.Object))
			{
				var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
				reportingBook.ARB_IncludeChildPresentation = ZBool.True;
				reportingBook.ARB_IncludePresentationJournals = "OUC";
				AssertEquals("Has Child", "OUC,SU1,SU2", (string)ValueProviderToTest.GetReplacement($"<ReportingBookPresentation({reportingBook.PK})>", Report));

				reportingBook.ARB_IncludeChildPresentation = ZBool.False;
				AssertEquals("No Child", "OUC", (string)ValueProviderToTest.GetReplacement($"<ReportingBookPresentation({reportingBook.PK})>", Report));

				reportingBook.ARB_IncludePresentationJournals = "";
				AssertEquals("Empty", "", (string)ValueProviderToTest.GetReplacement($"<ReportingBookPresentation({reportingBook.PK})>", Report));
				AssertEquals("Empty", "", (string)ValueProviderToTest.GetReplacement($"<ReportingBookPresentation({Guid.NewGuid()})>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ReportingBookPresentation();
		}
	}
}
