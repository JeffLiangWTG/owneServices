using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclarationDocumentSupporter))]
	class EMCSJobDeclarationDocumentSupporterTest : BaseJobDeclarationDocumentSupportTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.NewWithValidTestData<EMCSJobDeclaration>();

		protected override IDocumentSupportable GetDocumentSupportableBusinessObjectForRunningDocuments()
		{
			return declaration = (EMCSJobDeclaration)GetDocumentSupportableBusinessObject();
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var declaration = (EMCSJobDeclaration)GetDocumentSupportableBusinessObject();
				yield return declaration;
			}
		}

		public override void TestShowReasonForNotPrinting()
		{
			AssertEquals(expected: false, docSupporter.ShowReasonForNotPrinting(DataContext.Dummy, null));
		}

		public void TestDataContextSupported()
		{
			AssertEquals(expected: true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.EMCSDeclaration)));
		}

		public void TestGetBODocDataProviders()
		{
			CombineAssertions(() =>
			{
				var stmMenuItem = Factory.New<StmMenuItem>();
				stmMenuItem.SU_MenuName = "Internal Accompanying Document (IAD/W8)";
				AssertBODocDataProviders(stmMenuItem);
				stmMenuItem.SU_MenuName = "Electronic Administrative Document (E-AD)";
				AssertBODocDataProviders(stmMenuItem);
			});
		}

		void AssertBODocDataProviders(StmMenuItem menuItem)
		{
			var dataProvider = docSupporter.GetBODocDataProviders(new DataContextValue(".EMCSJobDeclaration"), menuItem);
			AssertEquals(1, dataProvider.Length);
			AssertEquals(ExpectedWrapperClassFullName, dataProvider[0].GetType().FullName);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand) =>
			documentsToExcludeTesting.Any(menu => documentCommand.SU_MenuName.Contains(menu, StringComparison.InvariantCultureIgnoreCase));

		readonly string[] documentsToExcludeTesting = new[]
		{
			"Authorization for Service",
			"Cartage Advice",
			"Commercial Invoice",
			"Invoice Batch Report",
			"Pro Forma Invoice",
			"Request for Service",
			"Export Accompanying Doc (EAD) and ELOI",
		};

		public override void TestGetDocBusinessObjects()
		{
			var declaration = (EMCSJobDeclaration)GetDocumentSupportableBusinessObject();
			var result = declaration.DocumentSupporter.GetDocumentWrappers(DataContext.EMCSDeclaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", ExpectedWrapperClassFullName, result[0].GetType().ToString());
		}

		public void TestGetFilterValueHIDEForEMCS()
		{
			AssertEquals("For filter 'HIDE=Y' result is Y", "Y", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.HIDE));
		}

		public void TestGetFilterValueHIDEForNonEMCS()
		{
			var gbDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("For filter 'HIDE=Y' result is Y", "Y", gbDeclaration.DocumentSupporter.GetFilterValue(DocumentFilters.HIDE));
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = new Dictionary<string, int>();
				maxHits["JobComInvoiceHeader"] = 2;
				return maxHits;
			}
		}

		EMCSJobDeclaration declaration;
		EMCSJobDeclarationDocumentSupporter docSupporter;

		protected override void SetUp()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			docSupporter = (EMCSJobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			base.SetUp();
		}

		const string ExpectedWrapperClassFullName = "Enterprise.DocumentWrappers.Customs.EU.EMCS.EMCSDeclarationWrapper";
	}
}
