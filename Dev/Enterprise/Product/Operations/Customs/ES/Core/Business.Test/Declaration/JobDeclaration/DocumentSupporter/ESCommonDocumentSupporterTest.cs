using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ESCommonDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestAddESSupportedBODataSources()
		{
			var dataContextValueList = new List<DataContextValue>();
			CombineAssertions(() =>
			{
				AssertEquals("List is empty before calling method", 0, dataContextValueList.Count);

				eSCommonDocSupporter.AddESSupportedBODataSources(dataContextValueList);

				AssertEquals("List has one element after calling method", 1, dataContextValueList.Count);
				AssertEquals("List contains the C10 DatContextValue", true, dataContextValueList.Contains(C10DataContextValue));
			});
		}

		public void TestGetESBODocDataProviders()
		{
			CombineAssertions(() =>
			{
				AssertNull("Method returns null when given DataContextValue is not expectd (C10)", eSCommonDocSupporter.GetESBODocDataProviders(new DataContextValue(".DummyBusinessObject")));

				AssertEquals("Method returns an empty IBODocDataProvider aray when given DataContextValue is C10 and BO is declaration but has no entryHeaders, count 0", 0, eSCommonDocSupporter.GetESBODocDataProviders(C10DataContextValue).Length);

				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				var esCommonDocumentSupporter = new ESCommonDocumentSupporter(entryHeader);
				AssertEquals("Method returns a IBODocDataProvider array with the entryHeader provided as BO when given DataContextValue is C10, count 1", 1, esCommonDocumentSupporter.GetESBODocDataProviders(C10DataContextValue).Length);

				declaration.ActiveEntryHeaders.AddNew();
				esCommonDocumentSupporter = new ESCommonDocumentSupporter(declaration);
				AssertEquals("Method returns a IBODocDataProvider array with all the active entryHeaders in the declaration provided as BO when given DataContextValue is C10, count 2", 2, esCommonDocumentSupporter.GetESBODocDataProviders(C10DataContextValue).Length);
			});
		}

		public void TestGetESSupportedDataContexts()
		{
			var dataContextArray = new DataContext[] { DataContext.SADH };
			var expectedDataContextArray = new DataContext[] { DataContext.ESSADH, DataContext.SADH };
			CombineAssertions(() =>
			{
				AssertEquals("Existing array has 1 element", 1, dataContextArray.Length);

				var returnedArray = eSCommonDocSupporter.GetESSupportedDataContexts(dataContextArray);

				AssertEquals("Method returns array with 2 elements (given and new ES one)", 2, returnedArray.Length);
				AssertContainsExactElementsInAnyOrder("Array contains the correct values", expectedDataContextArray, returnedArray);
			});
		}

		public void TestGetESDocumentWrappers()
		{
			CombineAssertions(() =>
			{
				var emptyEntryHeaderList = new List<CusEntryHeader>();
				AssertNull("Method returns null when given DataCOntext is not expected (ESSADH or SADH)", eSCommonDocSupporter.GetESDocumentWrappers(DataContext.AccountingJournal, emptyEntryHeaderList));

				AssertEquals("Method returns an empty DocumentWrapper array when given DataContext is expected (ESSADH or SADH) but entryHeaders list is empty, count 0", 0, eSCommonDocSupporter.GetESDocumentWrappers(DataContext.SADH, emptyEntryHeaderList).Length);

				var entryHeaderList = new List<CusEntryHeader>() { (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew() };
				AssertEquals("Method returns a DocumentWrapper array when given DataContext is expected (ESSADH or SADH) and entryHeaders list has data (one entry), count 1", 1, eSCommonDocSupporter.GetESDocumentWrappers(DataContext.ESSADH, entryHeaderList).Length);
			});
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "C10";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docC10Header = ESDocC10Header.New(entryHeader, declaration);

			CombineAssertions(() =>
			{
				var result = eSCommonDocSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentDocumentName is the expected one", "C10 - Reference", result.Title);

				result = eSCommonDocSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not ESDocC10Header", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = eSCommonDocSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentBusinessObject is ESDocC10Header and it has mrn", "C10 - MRNCode", result.Title);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			eSCommonDocSupporter = new ESCommonDocumentSupporter(Factory.New<JobDeclaration>());
		}
		ESCommonDocumentSupporter eSCommonDocSupporter;

		DataContextValue C10DataContextValue => new DataContextValue(".CusEntryHeaderC10");
	}
}
