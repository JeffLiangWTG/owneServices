using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class IBODocDataProviderCollectionFunctionalityTest : TestCaseWithFactory
	{
		public void TestFind()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var element1 = collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "VarCharMax 1";

			var element2 = collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "VarCharMax 2";

			var element3 = collection.AddNew();
			element3.Z0_Number = 3;
			element3.Z0_VarCharMax = "ON BEHALF OF: \"Kelvin";

			var helper = new BODocDataProviderCollectionHelper(collection);

			AssertEquals("Find", element1, helper.Find("{Z0_Number} == 1"));
			AssertEquals("Find", element2, helper.Find("{Z0_Number} == 2"));
			AssertEquals("Find", element3, helper.Find("{Z0_Number} == 3"));
			AssertEquals("Find", null, helper.Find("{Z0_Number} == 4"));

			AssertEquals("Find", element1, helper.Find("\"{Z0_VarCharMax}\" == \"VarCharMax 1\""));
			AssertEquals("Find", element2, helper.Find("\"{Z0_VarCharMax}\" == \"VarCharMax 2\""));
			AssertEquals("Find", element3, helper.Find("\"{Z0_VarCharMax}\" ==  \"ON BEHALF OF: \"Kelvin\""));
			AssertEquals("Find", null, helper.Find("\"{Z0_VarCharMax}\"  == \"/\""));
			AssertEquals("Find", null, helper.Find("\"{Z0_VarCharMax}\" == \"VarCharMax 5\""));
		}

		public void TestFindMarco()
		{
			var templateWithTranslateLegacyDocument = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[TranslateLegacyDocument]
{A}-[#SectionBody]
{B}-[<HideRowIfCellIsEmpty><Collection.Find(""{DeliveryType}"" == ""Delivery Type"").ServiceType> <Collection.Find(""{DeliveryType}"" == ""Delivery Type"").code>]
{A}-[#EndOfReport]");

			var templateWithoutTranslateLegacyDocument = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]s
{B}-[<HideRowIfCellIsEmpty><Collection.Find(""{DeliveryType}"" == ""Delivery Type"").ServiceType> <Collection.Find(""{DeliveryType}"" == ""Delivery Type"").code>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			var childDummyBusinessObject = dummy.Collection.AddNew("ABC");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document1 = documentCommand.Documents.AddNew();
			document1.SI_SU = documentCommand.PK;
			document1.SI_SO = templateWithTranslateLegacyDocument.PK;

			var document2 = documentCommand.Documents.AddNew();
			document2.SI_SU = documentCommand.PK;
			document2.SI_SO = templateWithoutTranslateLegacyDocument.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var reports = documentPack.OfType<Report>().ToList();
				var reportWithTranslateLegacyDocument = reports.FirstOrDefault();
				documentPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				using (var stream = new MemoryStream())
				{
					using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
					{
						mockChs.Put("2375508B-40B0-467F-9DE0-EF1EE149D562", new ResourceStringData("2375508B-40B0-467F-9DE0-EF1EE149D562", "货运类型"));
						mockChs.Put("F057BB85-A8D4-4288-AAEF-232D6FF31D61", new ResourceStringData("F057BB85-A8D4-4288-AAEF-232D6FF31D61", "服务类型"));

						using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
						{
							reportWithTranslateLegacyDocument.Save(stream);
							using (var excelInterface = new ExcelInterface())
							{
								excelInterface.LoadExcelFile(stream);
								var workSheet = excelInterface.WorkSheets.First();
								AssertEquals("货运类型", childDummyBusinessObject.DeliveryType);
								AssertEquals("服务类型", childDummyBusinessObject.ServiceType);
								AssertMultilineASCIIEquals(
									"",
									"{B}-[服务类型 ABC]",
									workSheet.ToString());
							}
						}

						documentPack.Language = Core.SharedConstants.Languages.EnglishAmerican;
						using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishAmerican))
						{
							var reportWithoutTranslateLegacyDocument = reports[1];
							reportWithoutTranslateLegacyDocument.Save(stream);
							using (var excelInterface = new ExcelInterface())
							{
								excelInterface.LoadExcelFile(stream);
								var workSheet = excelInterface.WorkSheets.First();
								AssertEquals("Delivery Type", childDummyBusinessObject.DeliveryType);
								AssertEquals("Service Type", childDummyBusinessObject.ServiceType);
								AssertMultilineASCIIEquals(
									"",
									"{B}-[Service Type ABC]",
									workSheet.ToString());
							}
						}
					}
				}
			}
		}

		public void TestFindException()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var element1 = collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "VarCharMax 1";

			var helper = new BODocDataProviderCollectionHelper(collection);
			AssertExceptionThrown(typeof(BODocDataProviderCollectionFindException), "Could not evaluate the following match: \"{Z0_VarCharMax}\" == \"I will break", () => helper.Find("\"{Z0_VarCharMax}\" == \"I will break"));
		}

		public void TestStringBasedIndexerOnDocumentWrapper()
		{
			var collection = new TestClasses.TestBOCollection(Factory);

			var boCollection = new TestClasses.DocTestBOWrapperCollection(collection);
			var bo1 = TestClasses.DocTestBOWrapper.New(new TestClasses.TestBO(), Factory);
			var bo2 = TestClasses.DocTestBOWrapper.New(new TestClasses.TestBO(), Factory);
			var bo3 = TestClasses.DocTestBOWrapper.New(new TestClasses.TestBO(), Factory);
			boCollection.Add(bo1);
			boCollection.Add(bo2);
			boCollection.Add(bo3);

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);

			CombineAssertions(delegate
			{
				// No values come back cause there's no override, but no stack overflow either... :-)
				AssertEquals("docCollection['Zero']", null, BODocDataProvider.GetBusinessObject(docCollection["Zero"]));
				AssertEquals("docCollection['One']", null, BODocDataProvider.GetBusinessObject(docCollection["One"]));
				AssertEquals("docCollection['Two']", null, BODocDataProvider.GetBusinessObject(docCollection["Two"]));
				AssertEquals("docCollection['Three']", null, BODocDataProvider.GetBusinessObject(docCollection["Three"]));
				AssertEquals("docCollection['Four']", null, BODocDataProvider.GetBusinessObject(docCollection["Four"]));

				// Test falls back to base indexer
				AssertEquals("docCollection['0']", null, BODocDataProvider.GetBusinessObject(docCollection["0"]));
				AssertEquals("docCollection['1']", bo1, BODocDataProvider.GetBusinessObject(docCollection["1"]));
				AssertEquals("docCollection['2']", bo2, BODocDataProvider.GetBusinessObject(docCollection["2"]));
				AssertEquals("docCollection['3']", bo3, BODocDataProvider.GetBusinessObject(docCollection["3"]));
				AssertEquals("docCollection['4']", null, BODocDataProvider.GetBusinessObject(docCollection["4"]));
			});
		}

		public void TestCustomStringBasedIndexer()
		{
			var boCollection = new TestBOCollectionWithStringBasedIndexer(Factory);
			TestBO bo1 = boCollection.AddNew();
			TestBO bo2 = boCollection.AddNew();
			TestBO bo3 = boCollection.AddNew();

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);

			CombineAssertions(delegate
			{
				// Test values from overridden indexer
				AssertEquals("docCollection['Zero']", null, BODocDataProvider.GetBusinessObject(docCollection["Zero"]));
				AssertEquals("docCollection['One']", bo1, BODocDataProvider.GetBusinessObject(docCollection["One"]));
				AssertEquals("docCollection['Two']", bo2, BODocDataProvider.GetBusinessObject(docCollection["Two"]));
				AssertEquals("docCollection['Three']", bo3, BODocDataProvider.GetBusinessObject(docCollection["Three"]));
				AssertEquals("docCollection['Four']", null, BODocDataProvider.GetBusinessObject(docCollection["Four"]));

				// Test falls back to base indexer
				AssertEquals("docCollection['0']", null, BODocDataProvider.GetBusinessObject(docCollection["0"]));
				AssertEquals("docCollection['1']", bo1, BODocDataProvider.GetBusinessObject(docCollection["1"]));
				AssertEquals("docCollection['2']", bo2, BODocDataProvider.GetBusinessObject(docCollection["2"]));
				AssertEquals("docCollection['3']", bo3, BODocDataProvider.GetBusinessObject(docCollection["3"]));
				AssertEquals("docCollection['4']", null, BODocDataProvider.GetBusinessObject(docCollection["4"]));
			});
		}

		public void TestIntBasedIndexer()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bo1 = boCollection.AddNew();
			TestBO bo2 = boCollection.AddNew();
			TestBO bo3 = boCollection.AddNew();

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertNull(docCollection[-1]);
			AssertEquals(bo1, BODocDataProvider.GetBusinessObject(docCollection[0]));
			AssertEquals(bo2, BODocDataProvider.GetBusinessObject(docCollection[1]));
			AssertEquals(bo3, BODocDataProvider.GetBusinessObject(docCollection[2]));
			AssertNull(docCollection[3]);
		}

		public void TestStringBasedIndexer()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bo1 = boCollection.AddNew();
			TestBO bo2 = boCollection.AddNew();
			TestBO bo3 = boCollection.AddNew();

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(null, docCollection["0"]);
			AssertEquals(bo1, BODocDataProvider.GetBusinessObject(docCollection["1"]));
			AssertEquals(bo2, BODocDataProvider.GetBusinessObject(docCollection["2"]));
			AssertEquals(bo3, BODocDataProvider.GetBusinessObject(docCollection["3"]));
			AssertEquals(null, BODocDataProvider.GetBusinessObject(docCollection["4"]));

			AssertEquals("docCollection[1]", bo1, BODocDataProvider.GetBusinessObject(docCollection["1"]));
			AssertEquals("docCollection[2]", bo2, BODocDataProvider.GetBusinessObject(docCollection["2"]));
			AssertEquals("docCollection[3]", bo3, BODocDataProvider.GetBusinessObject(docCollection["3"]));
			AssertEquals("docCollection[4]", null, BODocDataProvider.GetBusinessObject(docCollection["4"]));

			AssertEquals("docCollection[count]", bo3, BODocDataProvider.GetBusinessObject(docCollection["count"]));
			AssertEquals("docCollection[count-1]", bo2, BODocDataProvider.GetBusinessObject(docCollection["count-1"]));
			AssertEquals("docCollection[count-2]", bo1, BODocDataProvider.GetBusinessObject(docCollection["count-2"]));
			AssertEquals("docCollection[count-3]", null, BODocDataProvider.GetBusinessObject(docCollection["count-3"]));

			AssertEquals("docCollection[count - 1]  (spaces should be ignored) ", bo2, BODocDataProvider.GetBusinessObject(docCollection["count - 1"]));
			AssertEquals("docCollection[COUNT]      (case should be ignored)   ", bo3, BODocDataProvider.GetBusinessObject(docCollection["COUNT"]));

			AssertEquals("docCollection[first]", bo1, BODocDataProvider.GetBusinessObject(docCollection["first"]));
			AssertEquals("docCollection[last]", bo3, BODocDataProvider.GetBusinessObject(docCollection["last"]));

			AssertEquals("docCollection[AnyThingIDon'tRecognise]", null, docCollection["AnyWordIDon'tRecognise"]);
		}

		public void TestGetFilteredBusinessObjects()
		{
			var boCollection = new TestBOCollection(Factory);
			var bo1 = boCollection.AddNew();
			bo1.TextField = "Not In Filter";

			var bo2 = boCollection.AddNew();
			bo2.TextField = "In Filter";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(1, docCollection.GetFilteredBusinessObjects(@"""{TextField}"" == ""In Filter""").Length);
			AssertEquals(1, docCollection.GetFilteredBusinessObjects(@"""{TextField}"" == ""Not In Filter""").Length);
			AssertEquals(0, docCollection.GetFilteredBusinessObjects(@"""{TextField}"" == ""Invalid Filter""").Length);
		}

		public void TestGetPropertyInfo()
		{
			var boCollection = new TestBOCollection(Factory);
			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertNull("Should be null if no records", docCollection.GetPropertyInfo("IntField"));

			var bo = boCollection.AddNew();
			bo.IntField = 1;
			AssertEquals("IntField", docCollection.GetPropertyInfo("IntField").Name);
		}

		public void TestTotalForZInt()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().IntField = 1;
			boCollection.AddNew().IntField = 2;
			boCollection.AddNew().IntField = 3;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(6, docCollection.Total("IntField", null, null));
			AssertEquals(6, docCollection.Total("IntField", "", null));
			AssertEquals(6, docCollection.Total("IntField", "1", null));
			AssertEquals(6, docCollection.Total("IntField", "A", null));
		}

		public void TestTotalForZIntWithFilter()
		{
			var boCollection = new TestBOCollection(Factory);
			var bo1 = boCollection.AddNew();
			bo1.IntField = 1;
			bo1.TextField = "Not In Filter";

			var bo2 = boCollection.AddNew();
			bo2.IntField = 2;
			bo2.TextField = "In Filter";

			var bo3 = boCollection.AddNew();
			bo3.IntField = 3;
			bo3.TextField = "In Filter";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(5, docCollection.Total("IntField", null, @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5, docCollection.Total("IntField", "", @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5, docCollection.Total("IntField", "1", @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5, docCollection.Total("IntField", "A", @"""{TextField}"" == ""In Filter"""));
		}

		public void TestTotalForZLong()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().LongField = 1;
			boCollection.AddNew().LongField = 2;
			boCollection.AddNew().LongField = 3;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			CombineAssertions(() =>
			{
				AssertEquals(6L, docCollection.Total("LongField", null, null));
				AssertEquals(6L, docCollection.Total("LongField", "", null));
				AssertEquals(6L, docCollection.Total("LongField", "1", null));
				AssertEquals(6L, docCollection.Total("LongField", "A", null));
			});
		}

		public void TestTotalForZLongWithFilter()
		{
			var boCollection = new TestBOCollection(Factory);
			var bo1 = boCollection.AddNew();
			bo1.LongField = 1;
			bo1.TextField = "Not In Filter";

			var bo2 = boCollection.AddNew();
			bo2.LongField = 2;
			bo2.TextField = "In Filter";

			var bo3 = boCollection.AddNew();
			bo3.LongField = 3;
			bo3.TextField = "In Filter";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			CombineAssertions(() =>
			{
				AssertEquals(5L, docCollection.Total("LongField", null, @"""{TextField}"" == ""In Filter"""));
				AssertEquals(5L, docCollection.Total("LongField", "", @"""{TextField}"" == ""In Filter"""));
				AssertEquals(5L, docCollection.Total("LongField", "1", @"""{TextField}"" == ""In Filter"""));
				AssertEquals(5L, docCollection.Total("LongField", "A", @"""{TextField}"" == ""In Filter"""));
			});
		}

		public void TestTotalForZDecimal()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().DecimalField = 1.11m;
			boCollection.AddNew().DecimalField = 2.02m;
			boCollection.AddNew().DecimalField = 3.01m;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(6.14m, docCollection.Total("DecimalField", null, null));
			AssertEquals(6.14m, docCollection.Total("DecimalField", "", null));
			AssertEquals(6m, docCollection.Total("DecimalField", "0", null));
			AssertEquals(6.1m, docCollection.Total("DecimalField", "1", null));
			AssertEquals(6.14m, docCollection.Total("DecimalField", "2", null));
			AssertEquals("6.140", docCollection.Total("DecimalField", "3", null).ToString());
			AssertEquals(6.14m, docCollection.Total("DecimalField", "A", null));
		}

		public void TestTotalWithSamePropertyInDifferentType_Decimal()
		{
			var collection = new TestBOBaseCollection();
			var bo1 = new BO1 { DecimalField = 1.1m };
			var bo2 = new BO2 { DecimalField = 2.2m };
			collection.Add(bo1);
			collection.Add(bo2);

			var helper = new BODocDataProviderCollectionHelper(collection);

			AssertEquals(3.3m, helper.Total("DecimalField", null, null));
		}

		public void TestTotalWithSamePropertyInDifferentType_ZInt()
		{
			var collection = new TestBOBaseCollection();
			var bo1 = new BO1 { ZIntNumber = 11 };
			var bo2 = new BO2 { ZIntNumber = 22 };
			collection.Add(bo1);
			collection.Add(bo2);

			var helper = new BODocDataProviderCollectionHelper(collection);

			AssertEquals(33, helper.Total("ZIntNumber", null, null));
		}

		public void TestTotalForZDecimalWithFilter()
		{
			var boCollection = new TestBOCollection(Factory);
			var bo1 = boCollection.AddNew();
			bo1.DecimalField = 1.11m;
			bo1.TextField = "Not In Filter";
			var bo2 = boCollection.AddNew();
			bo2.DecimalField = 2.02m;
			bo2.TextField = "In Filter";
			var bo3 = boCollection.AddNew();
			bo3.DecimalField = 3.01m;
			bo3.TextField = "In Filter";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals(5.03m, docCollection.Total("DecimalField", null, @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5.03m, docCollection.Total("DecimalField", "", @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5m, docCollection.Total("DecimalField", "0", @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5.0m, docCollection.Total("DecimalField", "1", @"""{TextField}"" == ""In Filter"""));
			AssertEquals(5.03m, docCollection.Total("DecimalField", "2", @"""{TextField}"" == ""In Filter"""));
			AssertEquals("5.030", docCollection.Total("DecimalField", "3", @"""{TextField}"" == ""In Filter""").ToString());
			AssertEquals(5.03m, docCollection.Total("DecimalField", "A", @"""{TextField}"" == ""In Filter"""));
		}

		public void TestTotalForITotalValueAndUnits()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bO1 = boCollection.AddNew();
			bO1.Weight.Value = 1.11m;
			bO1.Weight.Unit = "KGS";
			TestBO bO2 = boCollection.AddNew();
			bO2.Weight.Value = 2.02m;
			bO2.Weight.Unit = "KGS";
			TestBO bO3 = boCollection.AddNew();
			bO3.Weight.Value = 3.01m;
			bO3.Weight.Unit = "KGS";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("6.14 KGS", docCollection.Total("Weight", "2", null));
		}

		public void TestTotalForITotalValueAndUnitsWithFilter()
		{
			var boCollection = new TestBOCollection(Factory);
			var bo1 = boCollection.AddNew();
			bo1.Weight.Value = 1.11m;
			bo1.Weight.Unit = "KGS";
			bo1.TextField = "Not In Filter";
			var bo2 = boCollection.AddNew();
			bo2.Weight.Value = 2.02m;
			bo2.Weight.Unit = "KGS";
			bo2.TextField = "In Filter";
			var bo3 = boCollection.AddNew();
			bo3.Weight.Value = 3.01m;
			bo3.Weight.Unit = "KGS";
			bo3.TextField = "In Filter";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("5.03 KGS", docCollection.Total("Weight", "2", @"""{TextField}"" == ""In Filter"""));
		}

		public void TestTotalWithUnrecognisedType()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("", docCollection.Total("TextField", null, null));
			AssertEquals("", docCollection.Total("TextField", "", null));
			AssertEquals("", docCollection.Total("TextField", "1", null));
			AssertEquals("", docCollection.Total("TextField", "A", null));
		}

		public void TestFormatMethodWithTwoParameters()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("ONE : TWO", docCollection.Format("{TextField}", "Colon", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE - TWO", docCollection.Format("{TextField}", "Dash", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE\r\nTWO", docCollection.Format("{TextField}", "NewLine", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE TWO", docCollection.Format("{TextField}", "Space", ZString.Empty, ZString.Empty, 0));
		}

		public void TestFormatMethodWithFilterString()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";
			boCollection.AddNew().TextField = "THREE";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("ONE, TWO", docCollection.Format("{TextField}", "Comma", "\"{TextField}\" != \"THREE\"", ZString.Empty, 0));
			AssertEquals("THREE", docCollection.Format("{TextField}", "Comma", "\"{TextField}\" == \"THREE\"", ZString.Empty, 0));
		}

		public void TestFormatWithInvalidFilter()
		{
			//Non-Boolean Filter
			var boCollection = new TestBOCollection(Factory);
			string booleanFilter = "ThisIsNotABooleanStatementAsItIsARandomString";

			boCollection.AddNew().TextField = "Whatever";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertExceptionThrown("Expected a format exception to be thrown!", typeof(BODocDataProviderCollectionFormatException),
				() => docCollection.Format("{TextField}", "Comma", booleanFilter, ZString.Empty, 0));

			//Expression with Unmatched Quotes
			boCollection.AddNew().TextField = "ExpectFailures";

			AssertExceptionThrown("Expected a format exception to be thrown!", typeof(BODocDataProviderCollectionFormatException),
				() => docCollection.Format("{TextField}", "Comma", @"""CNY""!=""USD"" && ""CNY!=""EUR""", ZString.Empty, 0));
		}

		public void TestFormatMethodWithGroupBy()
		{
			var boCollection = new TestBOCollection(Factory);

			var bo1 = boCollection.AddNew();
			var bo2 = boCollection.AddNew();
			var bo3 = boCollection.AddNew();
			var bo4 = boCollection.AddNew();

			bo1.TextField = "";
			bo2.TextField = "ONE";
			bo3.TextField = "ONE";
			bo4.TextField = "TWO";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count()} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 0));

			bo1.IntField = 0;
			bo2.IntField = 3;
			bo3.IntField = 5;
			bo4.IntField = 7;

			AssertEquals("0 x , 8 x ONE, 7 x TWO", docCollection.Format("{Sum(IntField)} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1  (0), 2 ONE (8), 1 TWO (7)", docCollection.Format("{Count} {TextField} ({Sum(IntField)})", "Comma", ZString.Empty, "\"{TextField}\"", 0));

			bo1.Weight.Value = 0;
			bo2.Weight.Value = 3;
			bo2.Weight.Unit = "KG";
			bo3.Weight.Value = 5;
			bo3.Weight.Unit = "KG";
			bo4.Weight.Value = 7;
			bo4.Weight.Unit = "LB";

			AssertEquals("1 x PKG 0, 2 x PKG 8KG, 1 x PKG 7LB", docCollection.Format("{Count} x PKG {Sum(Weight.Value)}{Weight.Unit}", "Comma", ZString.Empty, "\"{Weight.Unit}\"", 0));
		}

		public void TestFormatMethodWithFilterStringAndGroupBy()
		{
			var boCollection = new TestBOCollection(Factory);

			var bo1 = boCollection.AddNew();
			bo1.TextField = "";
			bo1.IntField = 1;

			var bo2 = boCollection.AddNew();
			bo2.TextField = "ONE";
			bo2.IntField = 2;

			var bo3 = boCollection.AddNew();
			bo3.TextField = "ONE";
			bo3.IntField = 3;

			var bo4 = boCollection.AddNew();
			bo4.TextField = "TWO";
			bo4.IntField = 4;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", "{IntField} > 1", "\"{TextField}\"", 0));
			AssertEquals("1 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", "{IntField} > 2", "\"{TextField}\"", 0));
			AssertEquals("2 x ONE", docCollection.Format("{Count} x {TextField}", "Comma", "\"{TextField}\" == \"ONE\"", "\"{TextField}\"", 0));
		}

		public void TestFormatMethodWithMaximunItems()
		{
			var boCollection = new TestBOCollection(Factory);

			var bo1 = boCollection.AddNew();
			bo1.TextField = "";
			bo1.IntField = 1;

			var bo2 = boCollection.AddNew();
			bo2.TextField = "ONE";
			bo2.IntField = 2;

			var bo3 = boCollection.AddNew();
			bo3.TextField = "ONE";
			bo3.IntField = 3;

			var bo4 = boCollection.AddNew();
			bo4.TextField = "TWO";
			bo4.IntField = 4;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", "", "", -1));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", "", "", 0));
			AssertEquals("ONE", docCollection.Format("{TextField}", "Comma", "", "", 1));
			AssertEquals("ONE, ONE", docCollection.Format("{TextField}", "Comma", "", "", 2));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", "", "", 3));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", "", "", 10));
		}

		public void TestFormatMethodWithGroupByAndMaximunItems()
		{
			var boCollection = new TestBOCollection(Factory);

			var bo1 = boCollection.AddNew();
			bo1.TextField = "";
			bo1.IntField = 1;

			var bo2 = boCollection.AddNew();
			bo2.TextField = "ONE";
			bo2.IntField = 2;

			var bo3 = boCollection.AddNew();
			bo3.TextField = "ONE";
			bo3.IntField = 3;

			var bo4 = boCollection.AddNew();
			bo4.TextField = "ONE";
			bo4.IntField = 4;

			var bo5 = boCollection.AddNew();
			bo5.TextField = "TWO";
			bo5.IntField = 5;

			var bo6 = boCollection.AddNew();
			bo6.TextField = "TWO";
			bo6.IntField = 6;

			var bo7 = boCollection.AddNew();
			bo7.TextField = "THREE";
			bo7.IntField = 7;

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("1 x , 3 x ONE, 2 x TWO, 1 x THREE", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", -1));
			AssertEquals("1 x , 3 x ONE, 2 x TWO, 1 x THREE", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 0));
			AssertEquals("1 x ", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 1));
			AssertEquals("1 x , 3 x ONE", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 2));
			AssertEquals("1 x , 3 x ONE, 2 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 3));
			AssertEquals("1 x , 3 x ONE, 2 x TWO, 1 x THREE", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 4));
			AssertEquals("1 x , 3 x ONE, 2 x TWO, 1 x THREE", docCollection.Format("{Count} x {TextField}", "Comma", "", "\"{TextField}\"", 10));
		}

		public void TestFormatHtmlNewLine()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";
			boCollection.AddNew().TextField = "THREE";
			boCollection.AddNew().TextField = "FOUR";
			boCollection.AddNew().TextField = "FIVE";

			var docCollection = new BODocDataProviderCollectionHelper(boCollection);
			AssertEquals("ONE<br />TWO<br />THREE<br />FOUR<br />FIVE", docCollection.Format("{TextField}", "HtmlNewLine", ZString.Empty, ZString.Empty, 0));
		}

		public void TestEnumerationModificationMessage()
		{
			AssertExceptionThrown<InvalidOperationException>("Modifying while enumerating should throw an exception", "Collection was modified; enumeration operation may not execute.", () =>
			{
				var boCollection = new TestBOCollection(Factory);
				boCollection.AddNew();
				foreach (BusinessObject bizO in boCollection)
				{
					boCollection.AddNew();
				}
			});

			AssertStartsWith("The only reported key should be:", "EnumerationCockUp_TestBOCollection", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}
	}
}
