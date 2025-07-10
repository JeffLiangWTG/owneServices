using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public void TestGetDocumentWrappersInternal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menu = GetMenu();
			CombineAssertions(() =>
			{
				AssertEquals("No entry", 0, GetDocumentWrappers().Length);
				declaration.CustomsEntryHeaders.AddNew();
				AssertEquals("One entry", 1, GetDocumentWrappers().Length);
				declaration.CustomsEntryHeaders.AddNew();
				AssertEquals("two entry", 2, GetDocumentWrappers().Length);
			});
			DocumentWrapper[] GetDocumentWrappers() => declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, menu);
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Enterprise.Customs.AsycudaCustoms.Business.DocCusEntryHeader", result[0].GetType().ToString());
		}

		public void TestGetBODocDataProviders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(SummaryCusDecDocument), null);
			AssertEquals(2, providers.Length);
		}

		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menu = GetMenu();
			var testingContext = new DataContextValue(SummaryCusDecDocument);
			CombineAssertions(() =>
			{
				AssertEquals("No Entry Header has been found for  document.", GetNotFoundMessageIfMenuIsNull());
				AssertEquals("No Entry Header has been found for Summary Note document.", GetNotFoundMessage());
				testingContext = new DataContextValue("CusEntryHeader");
				AssertEquals("Entry Header cannot be found.", GetNotFoundMessageIfMenuIsNull());
				AssertEquals("Entry Header cannot be found.", GetNotFoundMessage());
			});
			ZString GetNotFoundMessageIfMenuIsNull() => declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null);
			ZString GetNotFoundMessage() => declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, menu);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				yield return (JobDeclaration)GetDocumentSupportableBusinessObject();
			}
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = base.GetJobDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}

		IStmMenuItem GetMenu()
		{
			var menu = Factory.New<IStmMenuItem>();
			menu.SU_MenuName = "Summary Note";
			return menu;
		}

		internal const string SummaryCusDecDocument = ".SummaryCusDecDocument";
	}
}
